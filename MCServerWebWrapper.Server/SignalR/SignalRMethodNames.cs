using System;
using System.Collections.Generic;
using System.Text;

namespace MCServerWebWrapper.Shared.SignalR
{
	public static class SignalrMethodNames
	{
		public const string Ping = "ping";
		public const string ServerOutput = "outputreceived";
		public const string ServerStarted = "serverstarted";
		public const string ServerStopped = "serverstopped";
		public const string StatusUpdate = "statusupdate";
		public const string UserJoined = "userjoined";
		public const string UserLeft = "userleft";
		public const string JarDownloadProgressChanged = "jardownloadprogresschanged";
		public const string JarDownloadCompleted = "jardownloadcompleted";
	}
}
