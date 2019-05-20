using MCServerWebWrapper.Server.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Data
{
	public interface IServerRepo
	{
		IQueryable<MinecraftServer> GetServers();
		IQueryable<MinecraftServer> GetServers(int offset, int take);
		Task<IEnumerable<Output>> GetLogData(string id);
		Task<IEnumerable<Output>> GetLogData(string id, int offset, int take);
		Task<IEnumerable<Output>> GetLogData(string id, DateTime from, DateTime to);
		Task<MinecraftServer> GetServerById(string id);
		Task<MinecraftServer> GetServerByName(string name);
		Task<bool> AddServer(MinecraftServer server);
		Task<bool> RemoveServer(string id);
		Task<bool> UpsertServer(MinecraftServer server);
	}
}
