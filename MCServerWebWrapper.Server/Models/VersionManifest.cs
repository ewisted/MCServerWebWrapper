using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Models
{
	public class VersionManifest
	{
		public LastestInfo Latest { get; set; }
		public IEnumerable<VanillaVersion> Versions { get; set; }
	}

	public class LastestInfo
	{
		public string Release { get; set; }
		public string Snapshot { get; set; }
	}

	public class VanillaVersion
	{
		public string Id { get; set; }
		public string Type { get; set; }
		public string Url { get; set; }
		public DateTime Time { get; set; }
		public DateTime ReleaseTime { get; set; }
	}
}
