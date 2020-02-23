using MCServerWebWrapper.Server.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Models
{
	public class StatusUpdatedEventArgs : EventArgs
	{
		public StatusUpdate StatusUpdate { get; set; }
	}
}
