export interface MinecraftServer {
  id: string;
  name: string;
  maxRamMB: number;
  minRamMB: number;
  isRunning: boolean;
  latestLogs: Output[];
}

interface Output {
  timeStamp: Date;
  line: string;
}
