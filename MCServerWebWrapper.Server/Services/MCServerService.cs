using MCServerWebWrapper.Server.Data.Models;
using MCServerWebWrapper.Server.Hubs;
using MCServerWebWrapper.Server.Models;
using MCServerWebWrapper.Server.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MCServerWebWrapper.Shared.SignalR;
using System.IO;
using System.Reflection;
using AutoMapper;
using System.Text.RegularExpressions;

namespace MCServerWebWrapper.Server.Services
{
	public class MCServerService
	{
		private readonly IHubContext<AngularHub> _angularHub;
		private readonly ILogger<MCServerService> _logger;
		//private static ConcurrentQueue<OutputData> _outputBuffer = 
		//	new ConcurrentQueue<OutputData>();
		private static readonly ConcurrentDictionary<string, ServerProcess> _runningServers = 
			new ConcurrentDictionary<string, ServerProcess>();
		private readonly IServerRepo _repo;
		private readonly IUserRepo _userRepo;

		public MCServerService(IHubContext<AngularHub> angularHub, ILogger<MCServerService> logger, IServerRepo repo, IUserRepo userRepo)
		{
			_angularHub = angularHub;
			_logger = logger;
			_repo = repo;
			_userRepo = userRepo;
		}

		public async Task<IEnumerable<Output>> GetCurrentOutput(string serverId)
		{
			var logs = await _repo.GetLogData(serverId, DateTime.UtcNow - TimeSpan.FromHours(2), DateTime.UtcNow);
			return logs;
		}

		public async Task<MinecraftServer> NewServer(string name)
		{
			var server = await _repo.GetServerByName(name);
			if (server != null)
			{
				// TODO: Add better error handling here
				throw new Exception("Server already exists");
			}
			server = new MinecraftServer()
			{
				Id = ObjectId.GenerateNewId().ToString(),
				DateCreated = DateTime.UtcNow,
				IsRunning = false,
				Name = name,
				MaxRamMB = 2048,
				MinRamMB = 2048,
				TimesRan = 0,
				Logs = new List<Output>(),
				TotalUpTime = TimeSpan.Zero,
				PlayerCountChanges = new List<PlayerCountChange>(),
			};

			// Build the server path
			var buildPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var serverDirectory = Directory.CreateDirectory(Path.Combine(buildPath, server.Id));
			server.ServerPath = serverDirectory.FullName;

			// Copy in server jar
			var serverJarPath = Path.Combine(serverDirectory.FullName, "server.jar");
			var jarPath = Path.Combine(buildPath, "LargeFiles", "server.jar");
			File.Copy(jarPath, serverJarPath, true);

			// Copy in eula
			var eulaPath = Path.Combine(buildPath, server.Id, "eula.txt");
			var newEulaPath = Path.Combine(buildPath, "LargeFiles", "eula.txt");
			File.Copy(newEulaPath, eulaPath, true);

			// Set server properties of db object
			var properties = new ServerProperties();
			server.Properties = properties as Properties;

			// Add server to database
			await _repo.AddServer(server);

			return server;
		}

		public async Task RemoveServer(string id)
		{
			_runningServers.TryRemove(id, out var server);
			if (server != null)
			{
				await server.StopServer();
			}
			var buildPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var serverPath = Path.Combine(buildPath, id);
			Directory.Delete(serverPath, true);
			await _repo.RemoveServer(id);
		}

		public async Task StartServerById(string id, int maxRamMB, int minRamMB)
		{
			var server = await _repo.GetServerById(id);
			if (server == null)
			{
				// TODO: Add error handling here
				return;
			}
			if (_runningServers.ContainsKey(id))
			{
				// TODO: Add error handling here
				return;
			}

			var serverProcess = new ServerProcess(server.Id, maxRamMB, minRamMB);
			serverProcess.OutputReceived += s_OutputReceived;
			serverProcess.StatusUpdated += s_StatusUpdated;

			var pId = serverProcess.StartServer();
			_runningServers.TryAdd(server.Id, serverProcess);

			server.MaxRamMB = maxRamMB;
			server.MinRamMB = minRamMB;
			server.ProcessId = pId;
			server.IsRunning = true;
			server.TimesRan++;
			server.DateLastStarted = DateTime.UtcNow;
			await _repo.UpsertServer(server);
			await _angularHub.Clients.All.SendAsync(SignalrMethodNames.ServerStarted, id);

			return;
		}

		public async Task StopServerById(string id)
		{
			_runningServers.TryGetValue(id, out var server);
			if (server == null)
			{
				// TODO: Add error handling here
				return;
			}

			await server.StopServer();
			server.OutputReceived -= s_OutputReceived;
			server.StatusUpdated -= s_StatusUpdated;
			_runningServers.TryRemove(id, out server);

			var dbServer = await _repo.GetServerById(id);
			dbServer.IsRunning = false;
			dbServer.ProcessId = null;
			dbServer.DateLastStopped = DateTime.UtcNow;
			dbServer.TotalUpTime = dbServer.TotalUpTime + (DateTime.UtcNow - dbServer.DateLastStarted);
			await _repo.UpsertServer(dbServer);
			await _angularHub.Clients.All.SendAsync(SignalrMethodNames.ServerStopped, id);

			return;
		}

		public async Task SaveServerProperties(string id, ServerProperties properties)
		{
			var server = await _repo.GetServerById(id);
			if (server == null)
			{
				throw new Exception("Server not found.");
			}
			else if (server.TimesRan == 0)
			{
				throw new Exception("You must first run the server to generate neccessary files before saving.");
			}

			try
			{
				server.Properties = properties as Properties;
				await _repo.UpsertServer(server);

				await properties.Save(Path.Combine(server.ServerPath, "server.properties"));
			}
			catch (Exception ex)
			{
				throw ex;
			}

			return;
		}

		public async Task SendConsoleInput(string serverId, string msg)
		{
			_runningServers.TryGetValue(serverId, out var server);
			if (server == null)
			{
				throw new Exception("Server is not currently running");
			}
			await server.Server.StandardInput.WriteLineAsync(msg);
		}

		private async Task HandleOutput(string serverId, string line)
		{
			var output = new Output();
			output.TimeStamp = DateTime.UtcNow;
			output.Line = line;

			_runningServers.TryGetValue(serverId, out var server);
			server.LastOutput = output;
			_runningServers.AddOrUpdate(serverId, server, (key, value) => value = server);

			if (line.Contains("UUID of player"))
			{
				var userStr = line.Split("UUID of player").Last().Split("is");
				var user = new User();
				user.ConnectedServerId = serverId;
				user.UUID = userStr[1].Trim();
				user.Username = userStr[0].Trim();
				await _userRepo.UpsertUser(user);
				await _repo.AddLogDataByServerId(serverId, output);
				return;
			}

			if (line.Contains("left the game"))
			{
				var username = line.Split(':').Last().Replace("left the game", "");
				var user = new User();
				user.Username = username.Trim();
				user.ConnectedServerId = "";
				await _angularHub.Clients.All.SendAsync(SignalrMethodNames.UserLeft, serverId, user.Username);
				await _userRepo.UpsertUser(user);
				var dbServer = await _repo.GetServerById(serverId);
				var change = new PlayerCountChange()
				{
					Timestamp = DateTime.UtcNow,
					PlayersCurrentlyConnected = dbServer.PlayersCurrentlyConnected - 1,
					TriggeredByUsername = user.Username,
					IsJoin = false,
				};
				await _repo.AddPlayerCountDataByServerId(serverId, change);
				await _repo.AddLogDataByServerId(serverId, output);
				return;
			}

			var userMessageTest = new Regex(@"\<(.*?)\>");
			var userMessageMatch = userMessageTest.Match(output.Line);
			if (!string.IsNullOrWhiteSpace(userMessageMatch.Value))
			{
				output.User = userMessageMatch.Value.Trim('<', '>');
				await _repo.AddLogDataByServerId(serverId, output);
				return;
			}

			var userConnectedTest = new Regex(@"(?=.*?)([A-Za-z0-9]+)\[\/(.*?)\]");
			var userConnectedMatch = userConnectedTest.Match(output.Line);
			if (!string.IsNullOrWhiteSpace(userConnectedMatch.Value))
			{
				await _angularHub.Clients.All.SendAsync(SignalrMethodNames.UserJoined, serverId, userConnectedMatch.Groups[1].Value);
				output.User = userConnectedMatch.Groups[1].Value;
				var user = new User();
				user.Username = output.User;
				user.IP = userConnectedMatch.Groups[2].Value.Split(':').FirstOrDefault();
				await _userRepo.UpsertUser(user);

				var dbServer = await _repo.GetServerById(serverId);
				var change = new PlayerCountChange()
				{
					Timestamp = DateTime.UtcNow,
					PlayersCurrentlyConnected = dbServer.PlayersCurrentlyConnected + 1,
					TriggeredByUsername = user.Username,
					IsJoin = true,
				};
				await _repo.AddPlayerCountDataByServerId(serverId, change);
			}

			await _repo.AddLogDataByServerId(serverId, output);
			return;
		}

		private async void s_OutputReceived(object sender, OutputReceivedEventArgs args)
		{
			_logger.Log(LogLevel.Information, args.Data);
			await _angularHub.Clients.All.SendAsync(SignalrMethodNames.ServerOutput, args.ServerId, args.Data);
			await HandleOutput(args.ServerId, args.Data);
		}

		private async void s_StatusUpdated(object sender, StatusUpdatedEventArgs args)
		{
			await _angularHub.Clients.All.SendAsync(SignalrMethodNames.StatusUpdate, args.ServerId, args.StatusUpdate);
		}
	}
}
