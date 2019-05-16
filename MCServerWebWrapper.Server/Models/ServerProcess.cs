using MCServerWebWrapper.Server.Hubs;
using MCServerWebWrapper.Shared.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Models
{
	public class ServerProcess
	{
		public Process Server { get; private set; }
		private ProcessStartInfo StartInfo { get; set; }
		public ServerProperties Properties { get; set; }
		public string ServerId { get; private set; }
		public int MaxRamMb { get; private set; }
		public int MinRamMb { get; private set; }

		public ServerProcess(string serverId, int maxRam, int minRam)
		{
			ServerId = serverId;
			MaxRamMb = maxRam;
			MinRamMb = minRam;

			var buildPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var serverDirectory = Directory.CreateDirectory(Path.Combine(buildPath, serverId));

			StartInfo = new ProcessStartInfo("java", $"-Xmx{MaxRamMb}M -Xms{MinRamMb}M -jar server.jar nogui");
			StartInfo.WorkingDirectory = serverDirectory.FullName;
			StartInfo.RedirectStandardOutput = true;
			StartInfo.RedirectStandardInput = true;
			StartInfo.RedirectStandardError = true;
			StartInfo.CreateNoWindow = true;
			StartInfo.UseShellExecute = false;

			Server = new Process();
			Server.StartInfo = StartInfo;
			Server.EnableRaisingEvents = true;
		}

		public int StartServer(ILogger logger, IHubContext<AngularHub> angularHub)
		{
			Server.OutputDataReceived += async (sender, args) =>
			{
				logger.Log(LogLevel.Information, args.Data);
				await angularHub.Clients.All.SendAsync(SignalrMethodNames.ServerOutput, ServerId, args.Data);
			};
			Server.Start();
			Server.BeginOutputReadLine();
			return Server.Id;
		}

		public async Task StopServer()
		{
			Server.Exited += (sender, eArgs) =>
			{
				Server.Dispose();
			};
			await Server.StandardInput.WriteLineAsync("stop");
		}
	}
}
