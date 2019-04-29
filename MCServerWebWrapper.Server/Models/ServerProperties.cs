using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Models
{
	public class ServerProperties
	{
		public bool AllowFlight { get; set; }
		public bool AllowNether { get; set; }
		public int Difficulty { get; set; }
		public bool EnableQuery { get; set; }
		public bool EnableRcon { get; set; }
		public bool EnableCommandBlock { get; set; }
		public bool ForceGamemode { get; set; }
		public int Gamemode { get; set; }
		public bool GenerateStructures { get; set; }
		public string GeneratorSettings { get; set; }
		public bool Hardcore { get; set; }
		public string LevelName { get; set; }
		public string LevelSeed { get; set; }
		public string LevelType { get; set; }
		public int MaxBuildHeight { get; set; }
		public int MaxPlayers { get; set; }
		public int MaxTickTime { get; set; }
		public int MaxWorldSize { get; set; }
		public string MOTD { get; set; }
		public int NetworkCompressionThreshold { get; set; }
		public bool OnlineMode { get; set; }
		public int OpPermissionLevel { get; set; }
		public int PlayerIdleTimeout { get; set; }
		public bool PreventProxyConnections { get; set; }
		public bool Pvp { get; set; }
		public int QueryPort { get; set; }
		public string RconPassword { get; set; }
		public int RconPort { get; set; }
		public string ResourcePack { get; set; }
		public string ResourcePackSha1 { get; set; }
		public string ServerIP { get; set; }
		public int ServerPort { get; set; }
		public bool SnooperEnabled { get; set; }
		public bool SpawnAnimals { get; set; }
		public bool SpawnMonsters { get; set; }
		public bool SpawnNpcs { get; set; }
		public int SpawnProtection { get; set; }
		public bool UseNativeTransport { get; set; }
		public int ViewDistance { get; set; }
		public bool WhiteList { get; set; }
		public bool EnforceWhiteList { get; set; }
		
		public ServerProperties()
		{
			AllowFlight = false;
			AllowNether = true;
			Difficulty = 1;
			EnableQuery = false;
			EnableRcon = false;
			EnableCommandBlock = false;
			ForceGamemode = false;
			Gamemode = 0;
			GenerateStructures = true;
			GeneratorSettings = "";
			Hardcore = false;
			LevelName = "world";
			LevelSeed = "";
			LevelType = "DEFAULT";
			MaxBuildHeight = 256;
			MaxPlayers = 20;
			MaxTickTime = 60000;
			MaxWorldSize = 29999984;
			MOTD = "A Minecraft Server";
			NetworkCompressionThreshold = 256;
			OnlineMode = true;
			OpPermissionLevel = 4;
			PlayerIdleTimeout = 0;
			PreventProxyConnections = false;
			Pvp = true;
			QueryPort = 25565;
			RconPassword = "";
			RconPort = 25575;
			ResourcePack = "";
			ResourcePackSha1 = "";
			ServerIP = "";
			ServerPort = 25565;
			SnooperEnabled = true;
			SpawnAnimals = true;
			SpawnMonsters = true;
			SpawnNpcs = true;
			SpawnProtection = 16;
			UseNativeTransport = true;
			ViewDistance = 10;
			WhiteList = false;
			EnforceWhiteList = false;
		}

		public async Task Save(string serverPropertiesPath)
		{
			if (File.Exists(serverPropertiesPath))
			{
				File.Delete(serverPropertiesPath);
			}

			using (StreamWriter sw = File.CreateText(serverPropertiesPath))
			{
				await sw.WriteLineAsync("#Minecraft server properties");
				await sw.WriteLineAsync($"#{DateTime.UtcNow.ToString("ddd MMM dd HH’:’mm’:’ss ‘GMT’ yyyy")}");
				await sw.WriteLineAsync($"generator-settings={GeneratorSettings}");
				await sw.WriteLineAsync($"op-permission-level={OpPermissionLevel}");
				await sw.WriteLineAsync($"allow-nether={AllowNether}");
				await sw.WriteLineAsync($"enforce-whitelist={EnforceWhiteList}");
				await sw.WriteLineAsync($"level-name={LevelName}");
				await sw.WriteLineAsync($"enable-query={EnableQuery}");
				await sw.WriteLineAsync($"allow-flight={AllowFlight}");
				await sw.WriteLineAsync($"prevent-proxy-connections={PreventProxyConnections}");
				await sw.WriteLineAsync($"server-port={ServerPort}");
				await sw.WriteLineAsync($"max-world-size={MaxWorldSize}");
				await sw.WriteLineAsync($"level-type={LevelType}");
				await sw.WriteLineAsync($"enable-rcon={EnableRcon}");
				await sw.WriteLineAsync($"level-seed={LevelSeed}");
				await sw.WriteLineAsync($"force-gamemode={ForceGamemode}");
				await sw.WriteLineAsync($"server-ip={ServerIP}");
				await sw.WriteLineAsync($"network-compression-threshold={NetworkCompressionThreshold}");
				await sw.WriteLineAsync($"max-build-height={MaxBuildHeight}");
				await sw.WriteLineAsync($"spawn-npcs={SpawnNpcs}");
				await sw.WriteLineAsync($"white-list={WhiteList}");
				await sw.WriteLineAsync($"spawn-animals={SpawnAnimals}");
				await sw.WriteLineAsync($"hardcore={Hardcore}");
				await sw.WriteLineAsync($"snooper-enabled={SnooperEnabled}");
				await sw.WriteLineAsync($"resource-pack-sha1={ResourcePackSha1}");
				await sw.WriteLineAsync($"online-mode={OnlineMode}");
				await sw.WriteLineAsync($"resource-pack={ResourcePack}");
				await sw.WriteLineAsync($"pvp={Pvp}");
				await sw.WriteLineAsync($"difficulty={Difficulty}");
				await sw.WriteLineAsync($"enable-command-block={EnableCommandBlock}");
				await sw.WriteLineAsync($"gamemode={Gamemode}");
				await sw.WriteLineAsync($"player-idle-timeout={PlayerIdleTimeout}");
				await sw.WriteLineAsync($"max-players={MaxPlayers}");
				await sw.WriteLineAsync($"max-tick-time={MaxTickTime}");
				await sw.WriteLineAsync($"spawn-monsters={SpawnMonsters}");
				await sw.WriteLineAsync($"view-distance={ViewDistance}");
				await sw.WriteLineAsync($"generate-structures={GenerateStructures}");
				await sw.WriteLineAsync($"motd={MOTD}");
				await sw.WriteLineAsync($"use-native-transport={UseNativeTransport}");
				await sw.WriteLineAsync($"spawn-protection={SpawnProtection}");
				if (EnableRcon)
				{
					await sw.WriteLineAsync($"rcon.password={RconPassword}");
					await sw.WriteLineAsync($"rcon.port={RconPort}");
				}
				if (EnableQuery)
				{
					await sw.WriteLineAsync($"query.port={QueryPort}");
				}
			}
			return;
		}
	}
}
