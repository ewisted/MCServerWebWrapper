using MCServerWebWrapper.Server.Data.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Data
{
	public class UserMongoRepo : IUserRepo
	{
		private readonly IMongoCollection<User> _users;
		private static readonly SemaphoreSlim UpdateSemaphore = new SemaphoreSlim(1, 1);
		private static readonly ConcurrentQueue<User> UpdateUserQueue = new ConcurrentQueue<User>();
		public UserMongoRepo(IConfiguration config)
		{
			var client = new MongoClient(config.GetConnectionString("MCServerDb"));
			var database = client.GetDatabase(config.GetValue<string>("DbName"));
			_users = database.GetCollection<User>("Users");
		}

		public async Task<IEnumerable<User>> GetConnectedUsersByServerId(string id)
		{
			var users = await _users.FindAsync(u => u.ConnectedServerId == id);
			return await users.ToListAsync();
		}

		public async Task<IEnumerable<User>> GetUsersByServerId(string id)
		{
			var users = await _users.FindAsync(u => u.JoinedServerIds.Contains(id));
			return await users.ToListAsync();
		}

		public async Task<User> GetUserByUUID(string uuid)
		{
			var users = await _users.FindAsync(u => u.UUID == uuid);
			return await users.FirstOrDefaultAsync();
		}

		public async Task<User> GetUserByUsername(string username)
		{
			var users = await _users.FindAsync(u => u.Username == username);
			return await users.FirstOrDefaultAsync();
		}

		public async Task UpsertUser(User user)
		{
			UpdateUserQueue.Enqueue(user);

			await UpdateSemaphore.WaitAsync();
			UpdateUserQueue.TryDequeue(out var requestedUser);
			var dbUser = await GetUserByUsername(requestedUser.Username);
			if (dbUser == null)
			{
				requestedUser.JoinedServerIds = new List<string>();
				if (!string.IsNullOrWhiteSpace(requestedUser.ConnectedServerId))
				{
					requestedUser.JoinedServerIds.Add(requestedUser.ConnectedServerId);
				}
				await _users.InsertOneAsync(requestedUser);
				UpdateSemaphore.Release();
				return;
			}
			if (!string.IsNullOrWhiteSpace(requestedUser.ConnectedServerId))
			{
				if (!dbUser.JoinedServerIds.Contains(requestedUser.ConnectedServerId))
				{
					dbUser.JoinedServerIds.Add(requestedUser.ConnectedServerId);
				}
			}
			var properties = requestedUser.GetType().GetProperties();
			foreach (var property in properties)
			{
				var newValue = property.GetValue(requestedUser);
				if (newValue != null && newValue.GetType() != typeof(List<string>))
				{
					property.SetValue(dbUser, newValue);
				}
			}
			
			await _users.ReplaceOneAsync(u => u.Username == dbUser.Username, dbUser);
			UpdateSemaphore.Release();

			return;
		}
	}
}
