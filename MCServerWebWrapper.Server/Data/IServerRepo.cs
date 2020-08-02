using MCServerWebWrapper.Server.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Data
{
	public interface IServerRepo
	{
		IQueryable<JavaServer> GetServers();
		IQueryable<JavaServer> GetServers(int offset, int take);
		Task<IEnumerable<Output>> GetLogData(string id);
		Task<IEnumerable<Output>> GetLogData(string id, int offset, int take);
		Task<IEnumerable<Output>> GetLogData(string id, DateTime from, DateTime to);
		Task<JavaServer> GetServerById(string id);
		Task<JavaServer> GetServerByName(string name);
		Task AddLogDataByServerId(string id, Output output);
		Task AddPlayerCountDataByServerId(string id, PlayerCountChange change);
		Task<bool> AddServer(JavaServer server);
		Task<bool> RemoveServer(string id);
		Task<bool> UpsertServer(JavaServer server);
	}
}
