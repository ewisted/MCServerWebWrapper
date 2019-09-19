using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server
{
	public static class Guards
	{
		public static bool GuardValidObjectId(this object obj, out string id)
		{
			id = obj.ToString();
			if (Regex.IsMatch(id, @"^[a-fA-F0-9]{24}$"))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
