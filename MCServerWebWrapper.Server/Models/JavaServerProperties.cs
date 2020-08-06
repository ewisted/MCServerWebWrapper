using MCServerWebWrapper.Server.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;

namespace MCServerWebWrapper.Server.Models
{
	public class JavaServerProperties : JavaProperties
	{
		public JavaServerProperties()
		{
			AllowFlight = false;
			AllowNether = true;
			BroadcastConsoleToOps = true;
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
			EnforceWhitelist = false;
		}

		public async Task<JavaServerProperties> FromFile(string serverPropertiesPath)
		{
			JavaServerProperties properties = null;
			using (var streamReader = File.OpenText(serverPropertiesPath))
			{
				using (var writeStream = new MemoryStream())
                {
					using (var writer = new Utf8JsonWriter(writeStream))
                    {
						writer.WriteStartObject();
						while (!streamReader.EndOfStream)
						{
							var line = streamReader.ReadLine();
							if (!line.StartsWith("#"))
							{
								var lineKeyAndValue = line.Split("=");
								var key = KeyToPascalCase(lineKeyAndValue[0]);
								var value = lineKeyAndValue[1];
								if (Int32.TryParse(value, out int intValue))
								{
									writer.WriteNumber(key, intValue);
								}
								else if (Boolean.TryParse(value, out var boolValue))
								{
									writer.WriteBoolean(key, boolValue);
								}
								else
								{
									writer.WriteString(key, value);
								}
							}
						}
						writer.WriteEndObject();
					}
					properties = await JsonSerializer.DeserializeAsync<JavaServerProperties>(writeStream);
				}
			}
			if (properties == null) throw new InvalidDataException($"Unable to read the Java MC server properties from {serverPropertiesPath}");
			return properties;
		}

		private string KeyToPascalCase(string key)
		{
			var keyBuilder = new StringBuilder();
			foreach (var word in key.Split("-"))
			{
				keyBuilder.Append(char.ToUpper(word[0]) + word.Substring(1));
			}
			return keyBuilder.ToString();
		}

		private string KeyToPropertiesFormat(string key)
		{
			var sb = new StringBuilder();

			for (var i = 0; i < key.Length; i++)
			{
				if (i > 0)
				{
					if (Char.IsUpper(key[i]) && Char.IsLower(key[i - 1]))
					{
						sb.Append("-");
					}
				}
				sb.Append(key[i].ToString().ToLower());
			}

			return sb.ToString();
		}

		public async Task Save(string serverPropertiesPath)
		{
			var sb = new List<string>();
			using (var reader = File.OpenText(serverPropertiesPath))
			{
				sb.Add(reader.ReadLine());
				sb.Add(reader.ReadLine());
				reader.Dispose();
			}
			var properties = this.GetType().GetProperties();
			foreach (var propertyInfo in properties)
			{
				var value = propertyInfo.GetValue(this, null);
				if (value != null)
				{
					if (value.GetType() == typeof(bool))
					{
						value = value.ToString().ToLower();
					}
					var line = KeyToPropertiesFormat(propertyInfo.Name) + "=" + value;
					sb.Add(line);
				}
			}
			await File.WriteAllLinesAsync(serverPropertiesPath, sb);
			return;
		}
	}
}
