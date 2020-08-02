using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Data.Models
{
    public class BedrockProperties
    {
        /// <summary>
        /// Used as the server name
        /// Allowed values: Any string
        /// </summary>
        public string ServerName { get; set; }
        /// <summary>
        /// Sets the game mode for new players.
        /// Allowed values: "survival", "creative", or "adventure"
        /// </summary>
        public string Gamemode { get; set; }
        /// <summary>
        /// Sets the difficulty of the world.
        /// Allowed values: "peaceful", "easy", "normal", or "hard"
        /// </summary>
        public string Difficulty { get; set; }
        /// <summary>
        /// If true then cheats like commands can be used.
        /// Allowed values: "true" or "false"
        /// </summary>
        public bool AllowCheats { get; set; }
        /// <summary>
        /// The maximum number of players that can play on the server.
        /// Allowed values: Any positive integer
        /// </summary>
        public int MaxPlayers { get; set; }
        /// <summary>
        /// If true then all connected players must be authenticated to Xbox Live.
        /// Clients connecting to remote (non-LAN) servers will always require Xbox Live authentication regardless of this setting.
        /// If the server accepts connections from the Internet, then it's highly recommended to enable online-mode.
        /// Allowed values: "true" or "false"
        /// </summary>
        public bool OnlineMode { get; set; }
        /// <summary>
        /// If true then all connected players must be listed in the separate whitelist.json file.
        /// Allowed values: "true" or "false"
        /// </summary>
        public bool WhiteList { get; set; }
        /// <summary>
        /// Which IPv4 port the server should listen to.
        /// Allowed values: Integers in the range [1, 65535]
        /// </summary>
        public int ServerPort { get; set; }
        /// <summary>
        /// Which IPv6 port the server should listen to.
        /// Allowed values: Integers in the range [1, 65535]
        /// </summary>
        public int ServerPortv6 { get; set; }
        /// <summary>
        /// The maximum allowed view distance in number of chunks.
        /// Allowed values: Any positive integer.
        /// </summary>
        public int ViewDistance { get; set; }
        /// <summary>
        /// The world will be ticked this many chunks away from any player.
        /// Allowed values: Integers in the range [4, 12]
        /// </summary>
        public int TickDistance { get; set; }
        /// <summary>
        /// After a player has idled for this many minutes they will be kicked. If set to 0 then players can idle indefinitely.
        /// Allowed values: Any non-negative integer.
        /// </summary>
        public int PlayerIdleTimeout { get; set; }
        /// <summary>
        /// Maximum number of threads the server will try to use. If set to 0 or removed then it will use as many as possible.
        /// Allowed values: Any positive integer.
        /// </summary>
        public int MaxThreads { get; set; }
        /// <summary>
        /// Allowed values: Any string
        /// </summary>
        public string LevelName { get; set; }
        /// <summary>
        /// Use to randomize the world
        /// Allowed values: Any string
        /// </summary>
        public string LevelSeed { get; set; }
        /// <summary>
        /// Permission level for new players joining for the first time.
        /// Allowed values: "visitor", "member", "operator"
        /// </summary>
        public string DefaultPlayerPermissionLevel { get; set; }
        /// <summary>
        /// Force clients to use texture packs in the current world.
        /// Allowed values: "true" or "false"
        /// </summary>
        public bool TexturePackRequired { get; set; }
        /// <summary>
        /// Enables logging content errors to a file.
        /// Allowed values: "true" or "false"
        /// </summary>
        public bool ContentLogFileEnabled { get; set; }
        /// <summary>
        /// Determines the smallest size of raw network payload to compress.
        /// Allowed values: 0-65535
        /// </summary>
        public int CompressionThreshold { get; set; }
        /// <summary>
        /// Enables server authoritative movement. 
        /// If true, the server will replay local user input on the server and send down corrections when the client's position doesn't match the server's.
        /// Corrections will only happen if correct-player-movement is set to true.
        /// </summary>
        public bool ServerAuthoritativeMovement { get; set; }
        /// <summary>
        /// The number of incongruent time intervals needed before abnormal behavior is reported.
        /// Disabled by server-authoritative-movement.
        /// </summary>
        public int PlayerMovementScoreThreshold { get; set; }
        /// <summary>
        /// The difference between server and client positions that needs to be exceeded before abnormal behavior is detected.
        /// Disabled by server-authoritative-movement.
        /// </summary>
        public decimal PlayerMovementDistanceThreshold { get; set; }
        /// <summary>
        /// The duration of time the server and client positions can be out of sync (as defined by player-movement-distance-threshold) before the abnormal movement score is incremented.
        /// This value is defined in milliseconds. Disabled by server-authoritative-movement.
        /// </summary>
        public int PlayerMovementDurationThresholdInMs { get; set; }
        /// <summary>
        /// If true, the client position will get corrected to the server position if the movement score exceeds the threshold.
        /// </summary>
        public bool CorrectPlayerMovement { get; set; }
    }
}
