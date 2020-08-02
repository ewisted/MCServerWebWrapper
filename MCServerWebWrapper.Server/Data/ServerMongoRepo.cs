using MCServerWebWrapper.Server.Data.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Data
{
	public class ServerMongoRepo : IServerRepo
	{
		private readonly IMongoCollection<JavaServer> _servers;
		public ServerMongoRepo(IConfiguration config)
		{
			var client = new MongoClient(config.GetConnectionString("MCServerDb"));
			var database = client.GetDatabase(config.GetValue<string>("DbName"));
			_servers = database.GetCollection<JavaServer>("MinecraftServers");
		}
		
		// This constructor is used for unit testing only
		public ServerMongoRepo(string connectionString, string databaseString)
		{
			var client = new MongoClient(connectionString);
			var database = client.GetDatabase(databaseString);
			_servers = database.GetCollection<JavaServer>("MinecraftServers");
		}

		public IQueryable<JavaServer> GetServers()
		{
			return _servers.AsQueryable().OrderBy(s => s.Name);
		}

		public IQueryable<JavaServer> GetServers(int offset, int take)
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
			var logs = server.Logs.ToList().GetRange(offset, take);
			return logs;
		}

		public async Task<IEnumerable<Output>> GetLogData(string id, DateTime from, DateTime to)
		{
			var server = await GetServerById(id);
			var logs = server.Logs.Where(e => e.TimeStamp >= from && e.TimeStamp <= to);
			return logs;
		}

		public Task<JavaServer> GetServerById(string id)
		{
			var server = _servers.AsQueryable().Where(s => s.Id == id).FirstOrDefault();
			return Task.FromResult(server);
		}

		public Task<JavaServer> GetServerByName(string name)
		{
			var server = _servers.AsQueryable().Where(s => s.Name == name).FirstOrDefault();
			return Task.FromResult(server);
		}

		public async Task AddLogDataByServerId(string id, Output output)
		{
			await _servers.FindOneAndUpdateAsync(
				s => s.Id == id, 
				Builders<JavaServer>.Update.Push(
					c => c.Logs,
					output));
			return;
		}

		public async Task AddPlayerCountDataByServerId(string id, PlayerCountChange change)
		{
			await _servers.FindOneAndUpdateAsync(
				s => s.Id == id,
				Builders<JavaServer>.Update.Push(
					c => c.PlayerCountChanges,
					change));
			return;
		}

		public async Task<bool> AddServer(JavaServer server)
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

		public async Task<bool> UpsertServer(JavaServer server)
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
