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

namespace MCServerWebWrapper.Server.Services
{
	public class MCServerService
	{
		private readonly IHubContext<BlazorHub> _blazorHub;
		private readonly ILogger<MCServerService> _logger;
		//private static ConcurrentQueue<OutputData> _outputBuffer = 
		//	new ConcurrentQueue<OutputData>();
		private static readonly ConcurrentDictionary<string, ServerProcess> _runningServers = 
			new ConcurrentDictionary<string, ServerProcess>();
		private readonly IServerRepo _repo;

		public MCServerService(IHubContext<BlazorHub> blazorHub, ILogger<MCServerService> logger, IServerRepo repo)
		{
			_blazorHub = blazorHub;
			_logger = logger;
			_repo = repo;
		}

		public async Task<MinecraftServer> NewServer(string name, int maxRamMB, int minRamMB)
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
				MaxRamMB = maxRamMB,
				MinRamMB = minRamMB,
				HasAcceptedEula = false
			};
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

		public async Task StartServerById(string id)
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

			var serverProcess = new ServerProcess(server.Id, server.MaxRamMB, server.MinRamMB);

			_runningServers.TryAdd(server.Id, serverProcess);
			serverProcess.StartServer(_logger, _blazorHub);



			return;
		}

		public async Task StopServerById(string id)
		{
			_runningServers.TryRemove(id, out var server);
			if (server == null)
			{
				// TODO: Add error handling here
				return;
			}

			await server.StopServer();

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
	}
}
