using MCServerWebWrapper.Server.Data.Models;
using MCServerWebWrapper.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Services
{
    public interface IServerManagementService
    {
        Task<IEnumerable<Output>> GetCurrentOutput(string serverId, TimeSpan timeFrame);
        Task<JavaServer> NewServer(string name);
        Task RemoveServer(string id);
        Task<bool> StartServerById(string id, int maxRamMB, int minRamMB);
        Task<bool> StopServerById(string id);
        Task SaveServerProperties(string id, JavaServerProperties properties);
        Task SendConsoleInput(string serverId, string msg);
    }
}
