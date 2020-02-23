﻿using Docker.DotNet;
using Docker.DotNet.Models;
using MCServerWebWrapper.Server.Data;
using MCServerWebWrapper.Server.Data.Models;
using MCServerWebWrapper.Server.Hubs;
using MCServerWebWrapper.Server.Models;
using MCServerWebWrapper.Shared.SignalR;
using Microsoft.AspNetCore.SignalR;
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

        public DockerService(IHubContext<AngularHub> angularHub, ILogger<DockerService> logger, IServerRepo repo, IUserRepo userRepo)
        {
            _angularHub = angularHub;
            _logger = logger;
            _repo = repo;
            _userRepo = userRepo;
            string dockerEndpoint = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "npipe://./pipe/docker_engine"
                : "unix:///var/run/docker.sock";
            _docker = new DockerClientConfiguration(new Uri(dockerEndpoint)).CreateClient();
        }

        public async Task<IEnumerable<Output>> GetCurrentOutput(string serverId, TimeSpan timeFrame)
        {
            var logs = await _repo.GetLogData(serverId, DateTime.UtcNow - timeFrame, DateTime.UtcNow);
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
                InitRamMB = 2048,
                TimesRan = 0,
                Logs = new List<Output>(),
                TotalUpTime = TimeSpan.Zero,
                PlayerCountChanges = new List<PlayerCountChange>(),
            };

            // Build the server path
            var buildPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var serverDirectory = Directory.CreateDirectory(Path.Combine(buildPath, server.Id));
            server.ServerPath = serverDirectory.FullName;

            // Set server properties of db object
            var properties = new ServerProperties();
            server.Properties = properties as Properties;

            var parameters = new CreateContainerParameters
            {
                HostConfig = new HostConfig
                {

                }
            };
            var resp = await _docker.Containers.CreateContainerAsync(parameters);
            server.ContainerId = resp.ID;

            // Add server to database
            await _repo.AddServer(server);

            return server;
        }

        public async Task RemoveServer(string id)
        {
            var server = await _repo.GetServerById(id);
            if (server != null)
            {
                if (await IsContainerRunning(server.ContainerId))
                {
                    await _docker.Containers.StopContainerAsync(server.ContainerId, new ContainerStopParameters { WaitBeforeKillSeconds = 5 });
                }
                await _docker.Containers.RemoveContainerAsync(server.ContainerId, new ContainerRemoveParameters
                {
                    Force = true,
                    RemoveLinks = true,
                    RemoveVolumes = true
                });
                var buildPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var serverPath = Path.Combine(buildPath, id);
                Directory.Delete(serverPath, true);
                await _repo.RemoveServer(id);
            }
        }

        public Task SaveServerProperties(string id, ServerProperties properties)
        {
            
            throw new NotImplementedException();
        }

        public Task SendConsoleInput(string serverId, string msg)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> StartServerById(string id, int maxRamMB, int minRamMB)
        {
            var server = await _repo.GetServerById(id);
            if (server == null)
            {
                // TODO: Add error handling here
                return false;
            }
            if (await IsContainerRunning(server.ContainerId))
            {
                // TODO: Add error handling here
                return false;
            }
            var parameters = new ContainerStartParameters
            {
                DetachKeys = "ctrl-@"
            };
            var running = await _docker.Containers.StartContainerAsync(server.ContainerId, parameters);
            if (running)
            {
                var stream = await _docker.Containers.AttachContainerAsync(server.ContainerId, true, new ContainerAttachParameters
                {
                    DetachKeys = "ctrl-@",
                    Stderr = true,
                    Stdin = true,
                    Stdout = true,
                    Stream = true
                });

                var manager = new StreamManager(server.Id, server.ContainerId, stream);
                manager.OutputReceived += s_OutputReceived;
                //manager.StatusUpdated += s_StatusUpdated;
                manager.ServerStarted += s_ServerStarted;
                manager.ServerStopped += s_ServerStopped;

                _runningServers.AddOrUpdate(server.Id, manager, (key, oldValue) =>
                {
                    oldValue.Dispose();
                    return manager;
                });
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

            return await _docker.Containers.StopContainerAsync(server.ContainerId, new ContainerStopParameters { WaitBeforeKillSeconds = 5 });
        }

        private async Task<bool> IsContainerRunning(string containerId)
        {
            var containerParams = new ContainersListParameters() { Filters = new Dictionary<string, IDictionary<string, bool>>() };

            var statusFilter = new Dictionary<string, bool>();
            statusFilter.Add("running", true);

            var idFilter = new Dictionary<string, bool>();
            idFilter.Add(containerId, true);

            containerParams.Filters.Add("status", statusFilter);
            containerParams.Filters.Add("id", idFilter);

            var containerSearchResult = (await _docker.Containers.ListContainersAsync(containerParams)).SingleOrDefault();

            return containerSearchResult != null ? true : false;
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

        private async void s_ServerStarted(object sender, EventArgs args)
        {
            if (sender.GetType() == typeof(ServerProcess))
            {
                var server = (ServerProcess)sender;

                var dbServer = await _repo.GetServerById(server.ServerId);
                dbServer.MaxRamMB = server.MaxRamMb;
                dbServer.InitRamMB = server.MinRamMb;
                dbServer.ProcessId = server.Server.Id;
                dbServer.IsRunning = true;
                dbServer.TimesRan++;
                dbServer.DateLastStarted = DateTime.UtcNow;
                await _repo.UpsertServer(dbServer);

                await _angularHub.Clients.All.SendAsync(SignalrMethodNames.ServerStarted, server.ServerId);
            }
        }

        private async void s_ServerStopped(object sender, EventArgs args)
        {
            if (sender.GetType() == typeof(StreamManager))
            {
                var server = (StreamManager)sender;
                server.OutputReceived -= s_OutputReceived;
                //server.StatusUpdated -= s_StatusUpdated;
                server.ServerStarted -= s_ServerStarted;
                server.ServerStopped -= s_ServerStopped;
                _runningServers.TryRemove(server.ServerId, out server);

                var dbServer = await _repo.GetServerById(server.ServerId);
                dbServer.IsRunning = false;
                dbServer.DateLastStopped = DateTime.UtcNow;
                dbServer.TotalUpTime = dbServer.TotalUpTime + (DateTime.UtcNow - dbServer.DateLastStarted);
                await _repo.UpsertServer(dbServer);
                await _angularHub.Clients.All.SendAsync(SignalrMethodNames.ServerStopped, server.ServerId);
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