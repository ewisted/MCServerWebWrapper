using System;
using System.Collections.Generic;
using System.Text;

namespace MCServerWebWrapper.Shared.SignalR
{
	public class PingResponse
	{
		public string PingMessage { get; set; }
		public DateTime ReceivedAt { get; set; }
	}
}
