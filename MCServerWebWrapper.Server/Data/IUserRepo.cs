using MCServerWebWrapper.Server.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Data
{
	public interface IUserRepo
	{
		Task<IEnumerable<User>> GetConnectedUsersByServerId(string id);
		Task<IEnumerable<User>> GetUsersByServerId(string id);
		Task<User> GetUserByUUID(string uuid);
		Task<User> GetUserByUsername(string username);
		Task UpsertUser(User user);
	}
}
