export interface MinecraftServer {
  id: string;
  name: string;
  maxRamMB: number;
  minRamMB: number;
  isRunning: boolean;
  timesRan: number;
  dateCreated: Date;
  dateLastStarted: Date;
  dateLastStopped: Date;
  totalUpTimeSeconds: number;
  latestLogs: Output[];
}

interface Output {
  timeStamp: Date;
  line: string;
}
