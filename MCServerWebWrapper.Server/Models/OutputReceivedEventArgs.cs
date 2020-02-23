using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Models
{
	public class OutputReceivedEventArgs : EventArgs
	{
		public string Data { get; set; }
	}
}
