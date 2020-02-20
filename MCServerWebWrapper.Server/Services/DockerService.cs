using Docker.DotNet;
using Docker.DotNet.Models;
using MCServerWebWrapper.Server.Data.Models;
using MCServerWebWrapper.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Services
{
    public class DockerService : IServerManagementService
    {
        private readonly DockerClient _docker;

        public DockerService()
        {
            string dockerEndpoint = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "npipe://./pipe/docker_engine"
                : "unix:///var/run/docker.sock";
            _docker = new DockerClientConfiguration(new Uri(dockerEndpoint)).CreateClient();
        }

        public Task<IEnumerable<Output>> GetCurrentOutput(string serverId, TimeSpan timeFrame)
        {
            throw new NotImplementedException();
        }

        public Task<MinecraftServer> NewServer(string name)
        {
            throw new NotImplementedException();
        }

        public Task RemoveServer(string id)
        {
            throw new NotImplementedException();
        }

        public Task SaveServerProperties(string id, ServerProperties properties)
        {
            throw new NotImplementedException();
        }

        public Task SendConsoleInput(string serverId, string msg)
        {
            throw new NotImplementedException();
        }

        public Task<bool> StartServerById(string id, int maxRamMB, int minRamMB)
        {
            throw new NotImplementedException();
        }

        public Task StopServerById(string id)
        {
            throw new NotImplementedException();
        }
    }
}
