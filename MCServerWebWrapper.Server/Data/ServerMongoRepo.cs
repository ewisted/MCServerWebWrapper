using MCServerWebWrapper.Server.Data.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Data
{
	public class ServerMongoRepo : IServerRepo
	{
		private readonly IMongoCollection<MinecraftServer> _servers;
		public ServerMongoRepo(IConfiguration config)
		{
			var client = new MongoClient(config.GetConnectionString("MCServerDb"));
			var database = client.GetDatabase(config.GetValue<string>("DbName"));
			_servers = database.GetCollection<MinecraftServer>("MinecraftServers");
		}
		
		// This constructor is used for unit testing only
		public ServerMongoRepo(string connectionString, string databaseString)
		{
			var client = new MongoClient(connectionString);
			var database = client.GetDatabase(databaseString);
			_servers = database.GetCollection<MinecraftServer>("MinecraftServers");
		}

		public IQueryable<MinecraftServer> GetServers()
		{
			return _servers.AsQueryable().OrderBy(s => s.Name);
		}

		public IQueryable<MinecraftServer> GetServers(int offset, int take)
		{
			return GetServers().Skip(offset).Take(take);
		}

		public async Task<IEnumerable<Output>> GetLogData(string id)
		{
			var server = await GetServerById(id);
			return server.Logs;
		}

		public async Task<IEnumerable<Output>> GetLogData(string id, int offset, int take)
		{
			var server = await GetServerById(id);
			var logs = server.Logs.GetRange(offset, take);
			return logs;
		}

		public async Task<IEnumerable<Output>> GetLogData(string id, DateTime from, DateTime to)
		{
			var server = await GetServerById(id);
			var logs = server.Logs.Where(e => e.TimeStamp >= from && e.TimeStamp <= to);
			return logs;
		}

		public Task<MinecraftServer> GetServerById(string id)
		{
			var server = _servers.AsQueryable().Where(s => s.Id == id).FirstOrDefault();
			return Task.FromResult(server);
		}

		public Task<MinecraftServer> GetServerByName(string name)
		{
			var server = _servers.AsQueryable().Where(s => s.Name == name).FirstOrDefault();
			return Task.FromResult(server);
		}

		public async Task<bool> AddServer(MinecraftServer server)
		{
			var tryGetServer = await GetServerByName(server.Name);
			if (tryGetServer != null)
			{
				return false;
			}

			await _servers.InsertOneAsync(server);
			return true;
		}

		public async Task<bool> RemoveServer(string id)
		{
			var result = await _servers.DeleteOneAsync(e => e.Id == id);
			if (result.DeletedCount > 0)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public async Task<bool> UpsertServer(MinecraftServer server)
		{
			var result = await _servers.ReplaceOneAsync(s => s.Id == server.Id, server);
			if (result.ModifiedCount > 0)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
