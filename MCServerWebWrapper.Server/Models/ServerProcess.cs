using MCServerWebWrapper.Server.Data.Models;
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
using System.Threading;
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
		public List<Output> Output { get; set; }

		public ServerProcess(string serverId, int maxRam, int minRam)
		{
			ServerId = serverId;
			MaxRamMb = maxRam;
			MinRamMb = minRam;

			var buildPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var ServerPath = Directory.CreateDirectory(Path.Combine(buildPath, serverId)).FullName;

			StartInfo = new ProcessStartInfo("java", $"-Xmx{MaxRamMb}M -Xms{MinRamMb}M -jar server.jar nogui");
			StartInfo.WorkingDirectory = ServerPath;
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
			Output = new List<Output>();
			Server.OutputDataReceived += async (sender, args) =>
			{
				var output = new Output();
				output.TimeStamp = DateTime.UtcNow;
				output.Line = args.Data;
				Output.Add(output);
				logger.Log(LogLevel.Information, args.Data);
				await angularHub.Clients.All.SendAsync(SignalrMethodNames.ServerOutput, ServerId, args.Data);
			};
			Server.Start();
			Server.BeginOutputReadLine();
			return Server.Id;
		}

		public async Task StopServer()
		{
			var lastOutputBeforeShutdown = Output.Last();
			await Server.StandardInput.WriteLineAsync("stop");

			// Really shitty, but can't find a better way to wait for the server to stop completely
			var timeSinceLastOutput = TimeSinceLastOutput();
			while (timeSinceLastOutput <= TimeSpan.FromSeconds(1) || Output.Last().Line == lastOutputBeforeShutdown.Line)
			{
				Thread.Sleep(100);
				timeSinceLastOutput = TimeSinceLastOutput();
			}
			Server.Dispose();
			return;
		}

		private TimeSpan TimeSinceLastOutput()
		{
			var time = DateTime.UtcNow - Output.Last().TimeStamp;
			return time;
		}
	}
}
