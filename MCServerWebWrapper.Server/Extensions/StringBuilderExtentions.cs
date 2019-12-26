using MCServerWebWrapper.Server.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Extensions
{
    public static class StringBuilderExtentions
    {
        private const string T2 = "\t\t";
        private const string T3 = "\t\t\t";
        public static StringBuilder AppendServerConfig(this StringBuilder sb, MinecraftServer server)
        {
            
            sb.AppendLine(T2 + $"container_name: {server.Name}");
            sb.AppendLine(T2 + "image: itzg/minecraft-server");
            sb.AppendLine(T2 + $"environment:");
            sb.AppendServerEnvVariables(server);
            sb.AppendLine(T2 + "ports:");
            sb.AppendLine(T3 + $"- {server.Properties.ServerPort}:25565");
            sb.AppendLine(T2+ "volumes:");
            sb.AppendLine(T3 + $"- {server.ServerPath}:/data");
            sb.AppendLine(T2+ "tty: true");
            sb.AppendLine(T2 + "stdin_open: true");
            sb.AppendLine(T2 + $"restart: unless-stopped");
            return sb;
        }

        private static StringBuilder AppendServerEnvVariables(this StringBuilder sb, MinecraftServer server)
        {
            sb.AppendLine(T3 + $"TYPE: {server.Type}");
            foreach (var envVar in server.TypeSpecificEnvVariables)
            {
                sb.AppendLine(T3 + $"{envVar.Key}: {envVar.Value}");
            }
            sb.AppendLine(T3 + "EULA: 'TRUE'");
            sb.AppendLine(T3 + $"SERVER_NAME: {server.Name}");
            sb.AppendLine(T3 + "SERVER_PORT: 25565");
            sb.AppendLine(T3 + $"INIT_MEMORY: {server.InitRamMB}M");
            sb.AppendLine(T3 + $"MAX_MEMORY: {server.MaxRamMB}M");
            sb.AppendLine(T3 + $"DIFFICULTY: {GetNameForDifficulty(server.Properties.Difficulty)}");
            if (server.Properties.WhiteList && server.WhitelistedPlayers.Count > 0) sb.AppendLine(T3 + "WHITELIST:" + string.Join(',', server.WhitelistedPlayers));
            if (server.Operators.Count > 0) sb.AppendLine(T3 + "OPS:" + string.Join(',', server.Operators));
            if (server.ServerIconUri != null) sb.AppendLine(T3 + $"ICON: {server.ServerIconUri.AbsoluteUri}");
            if (server.Properties.EnableRcon)
            {
                sb.AppendLine(T3 + "ENABLE_RCON: 'TRUE'");
                sb.AppendLine(T3 + $"RCON_PASSWORD: {server.Properties.RconPassword}");
            }
            sb.AppendLine(T3 + $"ENABLE_QUERY: '{server.Properties.EnableQuery}'");
            sb.AppendLine(T3 + $"MAX_PLAYERS: {server.Properties.MaxPlayers}");
            sb.AppendLine(T3 + $"MAX_WORLD_SIZE: {server.Properties.MaxWorldSize}");
            sb.AppendLine(T3 + $"ALLOW_NETHER: '{server.Properties.AllowNether}'");
            sb.AppendLine(T3 + $"ANNOUNCE_PLAYER_ACHIEVEMENTS: '{server.Properties.AnnouncePlayerAchievements}'");
            sb.AppendLine(T3 + $"ENABLE_COMMAND_BLOCK: '{server.Properties.EnableCommandBlock}'");
            sb.AppendLine(T3 + $"FORCE_GAMEMODE: '{server.Properties.ForceGamemode}'");
            sb.AppendLine(T3 + $"GENERATE_STRUCTURES: '{server.Properties.GenerateStructures}'");
            sb.AppendLine(T3 + $"HARDCORE: '{server.Properties.Hardcore}'");
            sb.AppendLine(T3 + $"SNOOPER_ENABLED: '{server.Properties.SnooperEnabled}'");
            sb.AppendLine(T3 + $"MAX_BUILD_HEIGHT: {server.Properties.MaxBuildHeight}");
            sb.AppendLine(T3 + $"MAX_TICK_TIME: {server.Properties.MaxTickTime}");
            sb.AppendLine(T3 + $"SPAWN_ANIMALS: '{server.Properties.SpawnAnimals}'");
            sb.AppendLine(T3 + $"SPAWN_MONSTERS: '{server.Properties.SpawnMonsters}'");
            sb.AppendLine(T3 + $"SPAWN_NPCS: '{server.Properties.SpawnNpcs}'");
            sb.AppendLine(T3 + $"SPAWN_PROTECTION: {server.Properties.SpawnProtection}");
            sb.AppendLine(T3 + $"VIEW_DISTANCE: {server.Properties.ViewDistance}");
            if (!string.IsNullOrWhiteSpace(server.Properties.LevelSeed)) sb.AppendLine(T3 + $"SEED: {server.Properties.LevelSeed}");
            sb.AppendLine(T3 + $"MODE: {server.Properties.Gamemode}");
            sb.AppendLine(T3 + $"MOTD: {server.Properties.MOTD}");
            sb.AppendLine(T3 + $"PVP: '{server.Properties.Pvp}'");
            sb.AppendLine(T3 + $"LEVEL_TYPE: {server.Properties.LevelType}");
            if (!string.IsNullOrWhiteSpace(server.Properties.GeneratorSettings)) sb.AppendLine(T3 + $"GENERATOR_SETTINGS: {server.Properties.GeneratorSettings}");
            if (!string.IsNullOrWhiteSpace(server.Properties.ResourcePack) && !string.IsNullOrWhiteSpace(server.Properties.ResourcePackSha1))
            {
                sb.AppendLine(T3 + $"RESOURCE_PACK: '{server.Properties.ResourcePack}'");
                sb.AppendLine(T3 + $"RESOURCE_PACK_SHA1: '{server.Properties.ResourcePackSha1}'");
            }
            sb.AppendLine(T3 + $"LEVEL: {server.Properties.LevelName}");
            sb.AppendLine(T3 + $"ONLINE_MODE: '{server.Properties.OnlineMode}'");
            sb.AppendLine(T3 + $"ALLOW_FLIGHT: '{server.Properties.AllowFlight}'");
            sb.AppendLine(T3 + "CONSOLE: 'FALSE'");
            sb.AppendLine(T3 + "GUI: 'FALSE'");

            return sb;
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
