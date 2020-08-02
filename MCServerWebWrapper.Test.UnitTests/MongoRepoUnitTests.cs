using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using MCServerWebWrapper.Server.Data;
using MCServerWebWrapper.Server.Data.Models;

namespace MCServerWebWrapper.Test.UnitTests
{
	public class MongoRepoUnitTests : IDisposable
	{
		private readonly IServerRepo _repo;
		public MongoRepoUnitTests()
		{
			_repo = new ServerMongoRepo("mongodb://localhost:27017", "MCServerDb-test");
		}

		// Arrange
		// Setup a couple dummy servers to use for test data
		private static JavaServer fooServer1 = new JavaServer()
		{
			Id = ObjectId.GenerateNewId().ToString(),
			DateCreated = DateTime.UtcNow,
			IsRunning = false,
			Name = "FooServer1",
			MaxRamMB = 2048,
			InitRamMB = 2048
		};

		private static JavaServer fooServer2 = new JavaServer()
		{
			Id = ObjectId.GenerateNewId().ToString(),
			DateCreated = DateTime.UtcNow,
			IsRunning = false,
			Name = "FooServer2",
			MaxRamMB = 1024,
			InitRamMB = 1024
		};

		[Fact]
		public async Task GetServerByIdTest()
		{
			// Arrange
			await _repo.AddServer(fooServer1);

			// Act
			var server = await _repo.GetServerById(fooServer1.Id);
			await _repo.RemoveServer(fooServer1.Id);

			// Assert
			Assert.NotNull(server);
			Assert.Equal(fooServer1.Id, server.Id);
		}

		[Fact]
		public async Task GetServerByNameTest()
		{
			// Arrange
			await _repo.AddServer(fooServer1);

			// Act
			var server = await _repo.GetServerByName(fooServer1.Name);
			await _repo.RemoveServer(fooServer1.Id);

			// Assert
			Assert.NotNull(server);
			Assert.Equal(fooServer1.Name, server.Name);
		}

		[Fact]
		public async Task GetServersTest()
		{
			// Arrange
			await _repo.AddServer(fooServer1);
			await _repo.AddServer(fooServer2);

			// Act
			var servers = _repo.GetServers(0, 2).ToList();
			await _repo.RemoveServer(fooServer1.Id);
			await _repo.RemoveServer(fooServer2.Id);
			Console.WriteLine(servers);

			// Assert
			var expectedFooServer1 = servers.Where(s => s.Id == fooServer1.Id).FirstOrDefault();
			var expectedFooServer2 = servers.Where(s => s.Id == fooServer2.Id).FirstOrDefault();
			Assert.NotNull(servers);
			Assert.Equal(fooServer1.Name, expectedFooServer1.Name);
			Assert.Equal(fooServer2.Name, expectedFooServer2.Name);
			Assert.Equal(2, servers.Count());
		}

		[Fact]
		public async Task UpsertServerTest()
		{
			// Arrange
			await _repo.AddServer(fooServer1);
			fooServer1.Version = "1.13.2";

			// Act
			await _repo.UpsertServer(fooServer1);
			var server = await _repo.GetServerById(fooServer1.Id);
			await _repo.RemoveServer(fooServer1.Id);

			// Assert
			Assert.NotNull(server);
			Assert.Equal("1.13.2", server.Version);
		}

		public void Dispose()
		{
			var client = new MongoClient("mongodb://localhost:27017");
			client.DropDatabase("MCServerDb-test");
		}
	}
}
