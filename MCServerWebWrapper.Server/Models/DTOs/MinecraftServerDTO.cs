using MCServerWebWrapper.Server.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCServerWebWrapper.Server.Models.DTOs
{
	public class MinecraftServerDTO
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public int MaxRamMB { get; set; }
		public int MinRamMB { get; set; }
		public bool IsRunning { get; set; }
		public int TimesRan { get; set; }
		public DateTime DateCreated { get; set; }
		public DateTime DateLastStarted { get; set; }
		public DateTime DateLastStopped { get; set; }
		public double TotalUpTimeSeconds { get; set; }
		public List<Output> LatestLogs { get; set; }
	}
}
