using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Data.Models
{
	public class PlayerCountChange
	{
		public DateTime Timestamp { get; set; }
		public int PlayersCurrentlyConnected { get; set; }
		public string TriggeredByUsername { get; set; }
		public bool IsJoin { get; set; }
	}
}
