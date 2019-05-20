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
		public string ServerPath { get; set; }
		public Properties Properties { get; set; }
		public bool IsRunning { get; set; }
		public int TimesRan { get; set; }
		public int? ProcessId { get; set; }
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
		public List<Output> Logs { get; set; }
	}
}
