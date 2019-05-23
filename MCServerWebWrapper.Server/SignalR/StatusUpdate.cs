using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.SignalR
{
	public class StatusUpdate
	{
		public int CpuUsagePercent { get; set; }
		public string CpuUsageString { get; set; }
		public int RamUsageMB { get; set; }
		public string RamUsageString { get; set; }
	}
}
