using MCServerWebWrapper.Server.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Data.Models
{
	public class JavaServer : ServerBase
	{
        
        public int MaxRamMB { get; set; }
        public int InitRamMB { get; set; }
        public IList<string> Operators { get; set; }
        public IList<string> WhitelistedPlayers { get; set; }
        public string Type { get; set; }
        public IDictionary<string, string> TypeSpecificEnvVariables { get; set; }
        public string Version { get; set; }
        public Uri ServerIconUri { get; set; }
        public JavaProperties Properties { get; set; }
        public IList<Output> Logs { get; set; }
	}
}
