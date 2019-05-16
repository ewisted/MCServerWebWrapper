using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Models
{
	public class OutputData
	{
		public string ServerId { get; private set; }
		public string OutputLine { get; private set; }

		public OutputData(string serverId, string outputLine)
		{
			ServerId = serverId;
			OutputLine = outputLine;
		}
	}
}
