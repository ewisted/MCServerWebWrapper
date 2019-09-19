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
		public bool IsRunning { get; private set; }
		public CpuData CpuData { get; private set; }
		public RamData RamData { get; private set; }

		public Output LastOutput { get; set; }
		public event EventHandler<OutputReceivedEventArgs> OutputReceived;
		public event EventHandler<StatusUpdatedEventArgs> StatusUpdated;
		private System.Timers.Timer _timer;
		private DateTime LastUpdateTimestamp;
		private TimeSpan LastCpuUsage;

		private static readonly SemaphoreSlim ServerIOSemaphore = new SemaphoreSlim(1, 1);
		public event EventHandler ServerStarted;
		public event EventHandler ServerStopped;
		

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

		public bool StartServer()
		{
			try
			{
				CpuData = new CpuData();
				RamData = new RamData(MaxRamMb);
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
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}
		private void OnOutputReceived(object sender, DataReceivedEventArgs args)
		{
			if (args.Data != null)
			{
				var eArgs = new OutputReceivedEventArgs()
				{
					Data = args.Data,
				};
				OutputReceived.Invoke(ServerId, eArgs);

				var result = Regex.IsMatch(args.Data, @".*\[[:0-9]{8}\] \[Server thread\/INFO\]: Done \([s.0-9]{6,8}\)! For help, type ""help"".*");
				if (result)
				{
					IsRunning = true;
					ServerStarted.Invoke(this, new EventArgs());
				}
			}
		}

		private void OnStatusUpdated(object sender, System.Timers.ElapsedEventArgs e)
		{
			try
			{
				Server.Refresh();
				var updateTimeStamp = DateTime.UtcNow;
				var cpuUsageTime = Server.TotalProcessorTime;
				var cpuUsageTotal = (cpuUsageTime - LastCpuUsage).TotalMilliseconds / (Environment.ProcessorCount * (updateTimeStamp - LastUpdateTimestamp).TotalMilliseconds);

				var cpuUsage = Convert.ToInt32(cpuUsageTotal * 100);
				var cpuUsageString = CpuData.AddDataAndGetString(cpuUsage);
				var ramUsage = Convert.ToInt32(Server.WorkingSet64 / 1000000);
				var ramUsageString = RamData.AddDataAndGetString(ramUsage);

				var update = new StatusUpdate()
				{
					CpuUsagePercent = cpuUsage,
					CpuUsageString = cpuUsageString,
					RamUsageMB = ramUsage,
					RamUsageString = ramUsageString,
				};

				StatusUpdated.Invoke(ServerId, new StatusUpdatedEventArgs()
				{
					StatusUpdate = update,
				});

				LastUpdateTimestamp = updateTimeStamp;
				LastCpuUsage = cpuUsageTime;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
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
			IsRunning = false;
			ServerStopped.Invoke(this, new EventArgs());
			Server.OutputDataReceived -= OnOutputReceived;
			_timer.Elapsed -= OnStatusUpdated;
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
	}

	public class StatusUpdatedEventArgs : EventArgs
	{
		public StatusUpdate StatusUpdate { get; set; } 
	}
}
