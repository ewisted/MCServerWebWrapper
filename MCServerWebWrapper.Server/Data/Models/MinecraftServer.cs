using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Data.Models
{
	public class MinecraftServer
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }
		public string Name { get; set; }
		public bool HasAcceptedEula { get; set; }
		public bool IsRunning { get; set; }
		public int? ProcessId { get; set; }
		public int Port { get; set; }
		public string WorldName { get; set; }
		public int MaxRamMB { get; set; }
		public int MinRamMB { get; set; }
		public string MinecraftVersion { get; set; }
		public DateTime DateCreated { get; set; }
		public DateTime DateLastStarted { get; set; }
		public DateTime DateLastStopped { get; set; }
		public TimeSpan TotalUpTime { get; set; }
		public TimeSpan UpTimeSinceLastRestart { get; set; }
		public int PercentUpTime { get; set; }
		public int PlayersConnected { get; set; }

	}
}
