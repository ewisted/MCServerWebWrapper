using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Data.Models
{
	public class JavaProperties
	{
		public int SpawnProtection { get; set; }
		public int MaxTickTime { get; set; }
		public string GeneratorSettings { get; set; }
		public bool ForceGamemode { get; set; }
		public bool AllowNether { get; set; }
		public bool EnforceWhitelist { get; set; }
		public int Gamemode { get; set; }
		public bool BroadcastConsoleToOps { get; set; }
		public bool EnableQuery { get; set; }
		public int PlayerIdleTimeout { get; set; }
		public int Difficulty { get; set; }
		public bool SpawnMonsters { get; set; }
		public int OpPermissionLevel { get; set; }
		public bool Pvp { get; set; }
		public bool SnooperEnabled { get; set; }
		public string LevelType { get; set; }
		public bool Hardcore { get; set; }
		public bool EnableCommandBlock { get; set; }
		public int MaxPlayers { get; set; }
		public int NetworkCompressionThreshold { get; set; }
		public string ResourcePackSha1 { get; set; }
		public int MaxWorldSize { get; set; }
		public int ServerPort { get; set; }
		public string ServerIP { get; set; }
		public bool SpawnNpcs { get; set; }
		public bool AllowFlight { get; set; }
		public string LevelName { get; set; }
		public int ViewDistance { get; set; }
		public string ResourcePack { get; set; }
		public bool SpawnAnimals { get; set; }
		public bool WhiteList { get; set; }
		public bool GenerateStructures { get; set; }
		public bool OnlineMode { get; set; }
		public int MaxBuildHeight { get; set; }
		public string LevelSeed { get; set; }
		public bool UseNativeTransport { get; set; }
		public bool PreventProxyConnections { get; set; }
		public string MOTD { get; set; }
		public bool EnableRcon { get; set; }

		public int QueryPort { get; set; }
		public string RconPassword { get; set; }
		public int RconPort { get; set; }
        public bool AnnouncePlayerAchievements { get; set; }
	}
}
