using System;
using System.Collections.Generic;
using System.Text;

namespace MCServerWebWrapper.Shared.SignalR
{
	public static class SignalrMethodNames
	{
		public const string Ping = "ping";
		public const string ServerOutput = "outputreceived";
		public const string ServerError = "errorreceived";
	}
}
