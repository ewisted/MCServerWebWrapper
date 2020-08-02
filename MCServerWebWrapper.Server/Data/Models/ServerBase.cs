using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Data.Models
{
    public class ServerBase
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        public string ServerPath { get; set; }
        public bool IsRunning { get; set; }
        public int TimesRan { get; set; }
        public int PlayersCurrentlyConnected { get; set; }
        public IList<PlayerCountChange> PlayerCountChanges { get; set; }
        public int ProcessId { get; set; }
        public string ContainerId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateLastStarted { get; set; }
        public DateTime DateLastStopped { get; set; }
        public TimeSpan TotalUpTime { get; set; }
    }
}
