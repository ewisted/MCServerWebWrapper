using MCServerWebWrapper.Server.Data.Models;
using MCServerWebWrapper.Server.Extensions;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Services
{
    public class DockerComposeService
    {
        public DockerComposeService()
        {

        }

        public async Task CreateOrUpdateComposeFile(string path, params MinecraftServer[] servers)
        {
            var compose = new StringBuilder();
            compose.AppendLine("version: '3'");
            compose.AppendLine();
            compose.AppendLine("services:");

            foreach (var server in servers)
            {
                compose.AppendLine($"\t{server.Name}:");
                compose.AppendServerConfig(server);
            }

            await File.WriteAllTextAsync(path, compose.ToString());
            return;
        }
    }
}
