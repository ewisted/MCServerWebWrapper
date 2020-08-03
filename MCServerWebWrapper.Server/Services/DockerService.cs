using Docker.DotNet;
using Docker.DotNet.Models;
using MCServerWebWrapper.Server.Data;
using MCServerWebWrapper.Server.Data.Models;
using MCServerWebWrapper.Server.Extensions;
using MCServerWebWrapper.Server.Hubs;
using MCServerWebWrapper.Server.Models;
using MCServerWebWrapper.Shared.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Services
{
    public class DockerService : IServerManagementService
    {
        private readonly IHubContext<AngularHub> _angularHub;
        private readonly ILogger<DockerService> _logger;
        private readonly IServerRepo _repo;
        private static readonly ConcurrentDictionary<string, StreamManager> _runningServers =
            new ConcurrentDictionary<string, StreamManager>();
        private readonly IUserRepo _userRepo;
        private readonly DockerClient _docker;
        private readonly IConfigurationSection _dockerHubConfig;

        public DockerService(IHubContext<AngularHub> angularHub, ILogger<DockerService> logger, IServerRepo repo, IUserRepo userRepo, IConfiguration config)
        {
            _angularHub = angularHub;
            _logger = logger;
            _repo = repo;
            _userRepo = userRepo;
            _dockerHubConfig = config.GetSection("DockerHub");
            string dockerEndpoint = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "npipe://./pipe/docker_engine"
                : "unix:///var/run/docker.sock";
            _docker = new DockerClientConfiguration(new Uri(dockerEndpoint)).CreateClient();
            Task.Run(async () => await EnsureMCImageExists());
        }

        public async Task EnsureMCImageExists()
        {
            await _docker.Images.CreateImageAsync(
                new ImagesCreateParameters
                {
                    Repo = "itzg/minecraft-server"
                },
                new AuthConfig
                {
                    Username = _dockerHubConfig["Username"],
                    Password = _dockerHubConfig["AccessToken"]
                },
                new Progress<JSONMessage>()
            );
        }

        public async Task<IEnumerable<Output>> GetCurrentOutput(string serverId, TimeSpan timeFrame)
        {
            var logs = await _repo.GetLogData(serverId, DateTime.UtcNow - timeFrame, DateTime.UtcNow);
            return logs;
        }

        public async Task<JavaServer> NewServer(string name)
        {
            var server = await _repo.GetServerByName(name);
            if (server != null)
            {
                // TODO: Add better error handling here
                throw new Exception("Server already exists");
            }
            server = new JavaServer()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                DateCreated = DateTime.UtcNow,
                IsRunning = false,
                Name = name,
                MaxRamMB = 2048,
                InitRamMB = 2048,
                TimesRan = 0,
                Logs = new List<Output>(),
                TotalUpTime = TimeSpan.Zero,
                PlayerCountChanges = new List<PlayerCountChange>(),
                Operators = new List<string>()
            };

            // Build the server path
            var buildPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var serverDirectory = Directory.CreateDirectory(Path.Combine(buildPath, server.Id));
            server.ServerPath = serverDirectory.FullName;

            // Set server properties of db object
            var properties = new JavaServerProperties();
            server.Properties = properties as JavaProperties;

            // Add server to database
            await _repo.AddServer(server);

            return server;
        }

        public async Task RemoveServer(string id)
        {
            var server = await _repo.GetServerById(id);
            if (server != null)
            {
                if (_runningServers.ContainsKey(server.Id))
                {
                    await _docker.Containers.StopContainerAsync(server.ContainerId, new ContainerStopParameters { WaitBeforeKillSeconds = 5 });
                    await _docker.Containers.RemoveContainerAsync(server.ContainerId, new ContainerRemoveParameters
                    {
                        Force = true,
                        RemoveLinks = true,
                        RemoveVolumes = true
                    });
                }
                var buildPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var serverPath = Path.Combine(buildPath, id);
                Directory.Delete(serverPath, true);
                await _repo.RemoveServer(id);
            }
            return;
        }

        public async Task SaveServerProperties(string id, JavaServerProperties properties)
        {
            var server = await _repo.GetServerById(id);
            if (server == null)
            {
                throw new Exception("Server not found.");
            }

            server.Properties = properties as JavaProperties;
            await _repo.UpsertServer(server);

            var propertiesPath = Path.Combine(server.ServerPath, "server.properties");
            if (File.Exists(propertiesPath))
            {
                await properties.Save(propertiesPath);
            }
            return;
        }

        public async Task SendConsoleInput(string serverId, string msg)
        {
            var result = _runningServers.TryGetValue(serverId, out var streamManager);
            if (!result)
            {
                throw new Exception("Server is not currently running");
            }
            await streamManager.SendInput(msg);
            return;
        }

        public async Task<bool> StartServerById(string id, int maxRamMB, int minRamMB)
        {
            var server = await _repo.GetServerById(id);
            if (server == null)
            {
                // TODO: Add error handling here
                return false;
            }
            if (_runningServers.ContainsKey(server.Id))
            {
                // TODO: Add error handling here
                return false;
            }
            if (!string.IsNullOrWhiteSpace(server.ContainerId))
            {
                try
                {
                    await _docker.Containers.RemoveContainerAsync(server.ContainerId, new ContainerRemoveParameters
                    {
                        Force = true,
                        RemoveLinks = true,
                        RemoveVolumes = true
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            var createParams = await server.GetContainerParametersAsync();
            var resp = await _docker.Containers.CreateContainerAsync(createParams);
            server.ContainerId = resp.ID;

            var running = await _docker.Containers.StartContainerAsync(server.ContainerId, new ContainerStartParameters());
            if (running)
            {
                //var containers = await _docker.Containers.ListContainersAsync(new ContainersListParameters { All = true });

                var stream = await _docker.Containers.AttachContainerAsync(server.ContainerId, true, new ContainerAttachParameters
                {
                    DetachKeys = "ctrl-@",
                    Stderr = true,
                    Stdin = true,
                    Stdout = true,
                    Stream = true
                });

                var tokenSource = new CancellationTokenSource();
                var manager = new StreamManager(server.Id, server.ContainerId, stream, tokenSource);
                manager.OutputReceived += s_OutputReceived;
                manager.StatUpdateReceived += s_StatusUpdated;

                Task.Run(async () => await _docker.Containers.GetContainerStatsAsync(server.ContainerId, new ContainerStatsParameters { Stream = true }, manager.StatProgress, tokenSource.Token));

                _runningServers.AddOrUpdate(server.Id, manager, (key, oldValue) =>
                {
                    oldValue.Dispose();
                    return manager;
                });

                server.MaxRamMB = maxRamMB;
                server.InitRamMB = minRamMB;
                server.IsRunning = true;
                server.TimesRan++;
                server.DateLastStarted = DateTime.UtcNow;
                await _repo.UpsertServer(server);
                await _angularHub.Clients.All.SendAsync(SignalrMethodNames.ServerStarted, server.Id);
            }

            return running;
        }

        public async Task<bool> StopServerById(string id)
        {
            var server = await _repo.GetServerById(id);
            if (server == null || string.IsNullOrWhiteSpace(server.ContainerId))
            {
                // TODO: Add error handling here
                return false;
            }

            try
            {
                await _docker.Containers.StopContainerAsync(server.ContainerId, new ContainerStopParameters { WaitBeforeKillSeconds = 5 });
                await _docker.Containers.RemoveContainerAsync(server.ContainerId, new ContainerRemoveParameters { Force = true });
                server.ContainerId = null;
                await _repo.UpsertServer(server);
                _runningServers.TryRemove(id, out var manager);
                manager.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }

            server.IsRunning = false;
            server.DateLastStopped = DateTime.UtcNow;
            server.TotalUpTime = server.TotalUpTime + (DateTime.UtcNow - server.DateLastStarted);
            await _repo.UpsertServer(server);
            await _angularHub.Clients.All.SendAsync(SignalrMethodNames.ServerStopped, server.Id);
            return true;
        }

        private async Task HandleOutput(string serverId, string line)
        {
            var output = new Output();
            output.TimeStamp = DateTime.UtcNow;
            output.Line = line;

            var result = _runningServers.TryGetValue(serverId, out var server);
            if (!result) return;

            //server.LastOutput = output;
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

            var userMessageMatch = Regex.Match(output.Line, @"\<(.*?)\>");
            if (!string.IsNullOrWhiteSpace(userMessageMatch.Value))
            {
                output.User = userMessageMatch.Value.Trim('<', '>');
                await _repo.AddLogDataByServerId(serverId, output);
                return;
            }

            var userConnectedMatch = Regex.Match(output.Line, @"(?=.*?)([A-Za-z0-9]+)\[\/(.*?)\]");
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
            if (sender.GuardValidObjectId(out var serverId))
            {
                _logger.Log(LogLevel.Information, args.Data);
                await _angularHub.Clients.All.SendAsync(SignalrMethodNames.ServerOutput, serverId, args.Data);
                await HandleOutput(serverId, args.Data);
            }
        }

        private async void s_StatusUpdated(object sender, StatusUpdatedEventArgs args)
        {
            if (sender.GuardValidObjectId(out var serverId))
            {
                await _angularHub.Clients.All.SendAsync(SignalrMethodNames.StatusUpdate, serverId, args.StatusUpdate);
            }
        }
    }
}
