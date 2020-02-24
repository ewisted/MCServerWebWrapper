using Docker.DotNet.Models;
using MCServerWebWrapper.Server.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Extensions
{
    public static class MinecraftServerExtensions
    {
        public static Task<CreateContainerParameters> GetContainerParametersAsync(this MinecraftServer server)
        {
            var parameters = new CreateContainerParameters
            {
                HostConfig = new HostConfig
                {
                    RestartPolicy = new RestartPolicy
                    {
                        MaximumRetryCount = 3,
                        Name = RestartPolicyKind.UnlessStopped
                    },
                    PortBindings = GetPortBindings(server.Properties.ServerPort.ToString()),
                    Binds = GetBinds(server.ServerPath)
                },
                Image = "itzg/minecraft-server",
                Env = GetServerEnvVariables(server),
                Name = server.Name,
                ExposedPorts = GetExposedPorts(),
                OpenStdin = true,
                AttachStderr = true,
                AttachStdin = true,
                AttachStdout = true,
                Tty = true
            };

            return Task.FromResult(parameters);
        }

        private static IDictionary<string, IList<PortBinding>> GetPortBindings(string port)
        {
            var ports = new Dictionary<string, IList<PortBinding>>();
            var binding = new PortBinding()
            {
                HostPort = port
            };
            ports.Add("25565/tcp", new List<PortBinding> { binding });
            return ports;
        }

        private static IDictionary<string, EmptyStruct> GetExposedPorts()
        {
            var exposedPorts = new Dictionary<string, EmptyStruct>();
            exposedPorts.Add("25565/tcp", new EmptyStruct());
            return exposedPorts;
        }

        private static IList<string> GetBinds(string path)
        {
            var volBinds = new List<string>();
            volBinds.Add($"{path}:/data");
            return volBinds;
        }

        private static IList<string> GetServerEnvVariables(MinecraftServer server)
        {
            var env = new List<string>();
            env.Add($"TYPE={server.Type}");
            foreach (var envVar in server.TypeSpecificEnvVariables)
            {
                env.Add($"{envVar.Key}={envVar.Value}");
            }
            env.Add("EULA='TRUE'");
            env.Add($"SERVER_NAME={server.Name}");
            env.Add("SERVER_PORT=25565");
            env.Add($"INIT_MEMORY={server.InitRamMB}M");
            env.Add($"MAX_MEMORY={server.MaxRamMB}M");
            env.Add($"DIFFICULTY={GetNameForDifficulty(server.Properties.Difficulty)}");
            if (server.Properties.WhiteList && server.WhitelistedPlayers.Count > 0) env.Add("WHITELIST=" + string.Join(',', server.WhitelistedPlayers));
            if (server.Operators.Count > 0) env.Add("OPS=" + string.Join(',', server.Operators));
            if (server.ServerIconUri != null) env.Add($"ICON={server.ServerIconUri.AbsoluteUri}");
            if (server.Properties.EnableRcon)
            {
                env.Add("ENABLE_RCON='TRUE'");
                env.Add($"RCON_PASSWORD={server.Properties.RconPassword}");
            }
            env.Add($"ENABLE_QUERY='{server.Properties.EnableQuery}'");
            env.Add($"MAX_PLAYERS={server.Properties.MaxPlayers}");
            env.Add($"MAX_WORLD_SIZE={server.Properties.MaxWorldSize}");
            env.Add($"ALLOW_NETHER='{server.Properties.AllowNether}'");
            env.Add($"ANNOUNCE_PLAYER_ACHIEVEMENTS='{server.Properties.AnnouncePlayerAchievements}'");
            env.Add($"ENABLE_COMMAND_BLOCK='{server.Properties.EnableCommandBlock}'");
            env.Add($"FORCE_GAMEMODE='{server.Properties.ForceGamemode}'");
            env.Add($"GENERATE_STRUCTURES='{server.Properties.GenerateStructures}'");
            env.Add($"HARDCORE='{server.Properties.Hardcore}'");
            env.Add($"SNOOPER_ENABLED='{server.Properties.SnooperEnabled}'");
            env.Add($"MAX_BUILD_HEIGHT={server.Properties.MaxBuildHeight}");
            env.Add($"MAX_TICK_TIME={server.Properties.MaxTickTime}");
            env.Add($"SPAWN_ANIMALS='{server.Properties.SpawnAnimals}'");
            env.Add($"SPAWN_MONSTERS='{server.Properties.SpawnMonsters}'");
            env.Add($"SPAWN_NPCS='{server.Properties.SpawnNpcs}'");
            env.Add($"SPAWN_PROTECTION={server.Properties.SpawnProtection}");
            env.Add($"VIEW_DISTANCE={server.Properties.ViewDistance}");
            if (!string.IsNullOrWhiteSpace(server.Properties.LevelSeed)) env.Add($"SEED={server.Properties.LevelSeed}");
            env.Add($"MODE={server.Properties.Gamemode}");
            env.Add($"MOTD={server.Properties.MOTD}");
            env.Add($"PVP='{server.Properties.Pvp}'");
            env.Add($"LEVEL_TYPE={server.Properties.LevelType}");
            if (!string.IsNullOrWhiteSpace(server.Properties.GeneratorSettings)) env.Add($"GENERATOR_SETTINGS={server.Properties.GeneratorSettings}");
            if (!string.IsNullOrWhiteSpace(server.Properties.ResourcePack) && !string.IsNullOrWhiteSpace(server.Properties.ResourcePackSha1))
            {
                env.Add($"RESOURCE_PACK='{server.Properties.ResourcePack}'");
                env.Add($"RESOURCE_PACK_SHA1='{server.Properties.ResourcePackSha1}'");
            }
            env.Add($"LEVEL={server.Properties.LevelName}");
            env.Add($"ONLINE_MODE='{server.Properties.OnlineMode}'");
            env.Add($"ALLOW_FLIGHT='{server.Properties.AllowFlight}'");
            env.Add("CONSOLE='FALSE'");
            env.Add("GUI='FALSE'");

            return env;
        }

        private static string GetNameForDifficulty(int difficulty)
        {
            switch (difficulty)
            {
                case 0:
                    return "peaceful";
                case 1:
                    return "easy";
                case 2:
                    return "normal";
                case 3:
                    return "hard";
                default:
                    throw new ArgumentOutOfRangeException("Difficulty was not an expected value");
            }
        }
    }
}
