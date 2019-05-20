using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Data.Models
{
	public class User
	{
		[BsonId]
		public string Username { get; set; }
		public string UUID { get; set; }
		public string IP { get; set; }
		public string ConnectedServerId { get; set; }
		public List<string> JoinedServerIds { get; set; }
	}
}
