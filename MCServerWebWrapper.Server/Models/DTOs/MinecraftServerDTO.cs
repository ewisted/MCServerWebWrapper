using System;
using System.Collections.Generic;
using System.Text;

namespace MCServerWebWrapper.Models.DTOs
{
	public class MinecraftServerDTO
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public int MaxRamMB { get; set; }
		public int MinRamMB { get; set; }
		public bool IsRunning { get; set; }
		public IEnumerable<string> LatestLogs { get; set; }
	}
}
