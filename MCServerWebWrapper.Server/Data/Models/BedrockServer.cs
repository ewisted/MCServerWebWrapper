using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Data.Models
{
    public class BedrockServer : ServerBase
    {
        public IList<string> Operators { get; set; }
        public IList<string> WhitelistedPlayers { get; set; }
        public BedrockProperties Properties { get; set; }
    }
}
