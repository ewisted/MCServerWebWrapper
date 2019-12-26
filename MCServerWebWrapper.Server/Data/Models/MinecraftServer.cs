using MCServerWebWrapper.Server.Models;
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
        public bool IsRunning { get; set; }
        public int TimesRan { get; set; }
        public int PlayersCurrentlyConnected { get; set; }
        public IList<PlayerCountChange> PlayerCountChanges { get; set; }
        public int ContainerId { get; set; }
        public int MaxRamMB { get; set; }
        public int InitRamMB { get; set; }
        public string ServerPath { get; set; }
        public IList<string> Operators { get; set; }
        public IList<string> WhitelistedPlayers { get; set; }
        public string Type { get; set; }
        public IDictionary<string, string> TypeSpecificEnvVariables { get; set; }
        public string Version { get; set; }
        public Uri ServerIconUri { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateLastStarted { get; set; }
        public DateTime DateLastStopped { get; set; }
        public TimeSpan TotalUpTime { get; set; }
        public Properties Properties { get; set; }
        public IList<Output> Logs { get; set; }
	}
}
