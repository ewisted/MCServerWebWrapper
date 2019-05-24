export interface MinecraftServer {
  id: string;
  name: string;
  maxRamMB: number;
  minRamMB: number;
  isRunning: boolean;
  timesRan: number;
  playersCurrentlyConnected: number
  playerCountChanges: PlayerCountChange[];
  dateCreated: Date;
  dateLastStarted: Date;
  dateLastStopped: Date;
  totalUpTimeMs: number;
  percentUpTime: number;
  latestLogs: Output[];
}

interface Output {
  timeStamp: Date;
  line: string;
}

interface PlayerCountChange {
  timestamp: Date;
  playersCurrentlyConnected: number;
  triggeredByUsername: string;
  isJoin: boolean;
}
