using MCServerWebWrapper.Shared;
using MCServerWebWrapper.Shared.SignalR;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Hubs
{
	public class BlazorHub : Hub
	{
		public Task Ping(string replyMessage)
		{
			var response = new PingResponse()
			{
				PingMessage = replyMessage,
				ReceivedAt = DateTime.UtcNow
			};

			return Clients.Caller.SendAsync(SignalrMethodNames.Ping, response);
		}
	}
}
