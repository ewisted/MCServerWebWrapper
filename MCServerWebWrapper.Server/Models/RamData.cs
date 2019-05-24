using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Models
{
	public class RamData
	{
		public List<int> RamLogs { get; set; }
		private List<int> xValues { get; set; }
		private int MaxRamMB { get; set; }

		public RamData(int maxRamMB)
		{
			MaxRamMB = maxRamMB;
			RamLogs = new List<int>();
			xValues = new List<int>();
			for (var i = 0; i < 60; i++)
			{
				xValues.Add(i * 7);
			}
		}

		public string AddDataAndGetString(int ramMB)
		{
			if (RamLogs.Count >= 60)
			{
				RamLogs.RemoveAt(RamLogs.Count - 1);
			}
			double ramUsed = (double)ramMB / (double)MaxRamMB;
			RamLogs.Insert(0, Convert.ToInt32((1 - ramUsed) * 200));
			return GetString();
		}

		public string GetString()
		{
			var coordsList = new List<string>();
			coordsList.Insert(0, "0,200");
			for (var i = 0; i < RamLogs.Count; i++)
			{
				coordsList.Add($"{xValues[i]},{RamLogs[i]}");
				if (i == RamLogs.Count - 1)
				{
					coordsList.Add($"{xValues[i]},200");
				}
			}
			return String.Join(" ", coordsList);
		}
	}
}
