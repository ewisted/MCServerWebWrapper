using MCServerWebWrapper.Server.Data.Models;
using MCServerWebWrapper.Server.Hubs;
using MCServerWebWrapper.Server.SignalR;
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
using System.Text.RegularExpressions;
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
		public Output LastOutput { get; set; }
		public event EventHandler<OutputReceivedEventArgs> OutputReceived;
		private System.Timers.Timer _timer;
		private DateTime LastUpdateTimestamp;
		private TimeSpan LastCpuUsage;
		public event EventHandler<StatusUpdatedEventArgs> StatusUpdated;

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

		public int StartServer()
		{
			Server.OutputDataReceived += OnOutputReceived;
			Server.Start();
			Server.BeginOutputReadLine();
			LastUpdateTimestamp = DateTime.UtcNow;
			LastCpuUsage = Server.TotalProcessorTime;
			Task.Run(async () =>
			{
				await Task.Delay(1000);
				_timer = new System.Timers.Timer(1000);
				_timer.Elapsed += OnStatusUpdated;
				_timer.Enabled = true;
			});
			
			return Server.Id;
		}
		private void OnOutputReceived(object sender, DataReceivedEventArgs args)
		{
			if (args.Data != null)
			{
				var eArgs = new OutputReceivedEventArgs();
				eArgs.Data = args.Data;
				eArgs.ServerId = ServerId;
				OutputReceived.Invoke(this, eArgs);
			}
		}

		private void OnStatusUpdated(object sender, System.Timers.ElapsedEventArgs e)
		{
			var updateTimeStamp = DateTime.UtcNow;
			var cpuUsage = Server.TotalProcessorTime;
			var cpuUsedMs = (cpuUsage - LastCpuUsage).TotalMilliseconds;
			var totalMsPassed = (updateTimeStamp - LastUpdateTimestamp).TotalMilliseconds;
			var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

			var update = new StatusUpdate()
			{
				CpuUsuagePercent = Convert.ToInt32(cpuUsageTotal * 100),
				RamUsageMB = Convert.ToInt32(Server.WorkingSet64 / 1000000),
			};
			
			StatusUpdated.Invoke(this, new StatusUpdatedEventArgs(ServerId, update));

			LastUpdateTimestamp = updateTimeStamp;
			LastCpuUsage = cpuUsage;
		}

		public async Task StopServer()
		{
			var lastOutputBeforeShutdown = LastOutput;
			await Server.StandardInput.WriteLineAsync("stop");

			// Really shitty, but can't find a better way to wait for the server to stop completely
			var timeSinceLastOutput = TimeSinceLastOutput();
			while (timeSinceLastOutput <= TimeSpan.FromSeconds(1) || LastOutput.Line == lastOutputBeforeShutdown.Line)
			{
				Thread.Sleep(100);
				timeSinceLastOutput = TimeSinceLastOutput();
			}
			Server.OutputDataReceived += OnOutputReceived;
			_timer.Elapsed += OnStatusUpdated;
			_timer.Enabled = false;
			_timer = null;
			Server.Dispose();
			return;
		}

		private TimeSpan TimeSinceLastOutput()
		{
			var time = DateTime.UtcNow - LastOutput.TimeStamp;
			return time;
		}
	}

	public class OutputReceivedEventArgs : EventArgs
	{
		public string Data { get; set; }
		public string ServerId { get; set; }
	}

	public class StatusUpdatedEventArgs : EventArgs
	{
		public StatusUpdatedEventArgs(string serverId, StatusUpdate update)
		{
			ServerId = serverId;
			StatusUpdate = update;
		}
		public string ServerId { get; set; }
		public StatusUpdate StatusUpdate { get; set; } 
	}
}
