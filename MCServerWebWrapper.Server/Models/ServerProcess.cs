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
		public bool HasAcceptedEula { get; set; }
		public string ServerId { get; private set; }
		public int ProcessId { get; set; }
		public int MaxRamMb { get; private set; }
		public int MinRamMb { get; private set; }

		public ServerProcess(string serverId, int maxRam, int minRam)
		{
			ServerId = serverId;
			MaxRamMb = maxRam;
			MinRamMb = minRam;

			var buildPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var serverDirectory = Directory.CreateDirectory(Path.Combine(buildPath, serverId));
			var jarPath = Path.Combine(buildPath, "LargeFiles", "server.jar");
			File.Copy(jarPath, Path.Combine(serverDirectory.FullName, "server.jar"), true);
			
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

		public void StartServer(ILogger logger, IHubContext<BlazorHub> blazorHub)
		{
			if (!HasAcceptedEula)
			{
				EventHandler handler = null;
				handler = (obj, eventArgs) =>
				{
					Server.CancelOutputRead();
					var buildPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
					var eulaPath = Path.Combine(buildPath, ServerId, "eula.txt");
					var newEulaPath = Path.Combine(buildPath, "LargeFiles", "eula.txt");
					File.Copy(newEulaPath, eulaPath, true);
					Server.Exited -= handler;
					Server.Start();
					Server.BeginOutputReadLine();
					ProcessId = Server.Id;
				};
				Server.Exited += handler;
			}
			Server.OutputDataReceived += async (sender, args) =>
			{
				logger.Log(LogLevel.Information, args.Data);
				await blazorHub.Clients.All.SendAsync(SignalrMethodNames.ServerOutput, ServerId, args.Data);
			};
			Server.Start();
			Server.BeginOutputReadLine();
			ProcessId = Server.Id;
		}

		public async Task StopServer()
		{
			await Server.StandardInput.WriteLineAsync("stop");
			// TODO: We need to dispose of the process here, but we need to make sure the server has stopped first
		}
	}
}
