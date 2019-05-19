export interface MinecraftServer{
  id: string;
  name: string;
  maxRamMB: number;
  minRamMB: number;
  isRunning: boolean;
  latestLogs: string[];
}
