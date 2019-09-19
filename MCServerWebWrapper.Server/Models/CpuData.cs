using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Models
{
	public class CpuData
	{
		public List<int> CpuLogs { get; set; }
		private List<int> xValues { get; set; }

		public CpuData()
		{
			CpuLogs = new List<int>();
			xValues = new List<int>();
			for (var i = 0; i < 60; i++)
			{
				xValues.Add(i * 7);
			}
		}

		public string AddDataAndGetString(int cpuUsage)
		{
			if (CpuLogs.Count >= 60)
			{
				CpuLogs.RemoveAt(CpuLogs.Count - 1);
			}
			CpuLogs.Insert(0, (100 - cpuUsage) * 2);
			return GetString();
		}

		public string GetString()
		{
			var coordsList = new List<string>();
			coordsList.Insert(0, "0,200");
			for (var i = 0; i < CpuLogs.Count; i++)
			{
				coordsList.Add($"{xValues[i]},{CpuLogs[i]}");
				if (i == CpuLogs.Count - 1)
				{
					coordsList.Add($"{xValues[i]},200");
				}
			}
			return String.Join(" ", coordsList);
		}
	}
}
