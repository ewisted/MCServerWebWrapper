export class CpuData {
  cpuLogs: number[];
  xValues: number[];

  constructor() {
    this.cpuLogs = [];
    this.xValues = [];
    for (var i = 1; i <= 60; i++) {
      this.xValues.push(i * 7);
    }
  }

  public addData(cpuUsage: number) {
    if (this.cpuLogs.length >= 60) {
      this.cpuLogs.pop();
    }
    this.cpuLogs.unshift((100 - cpuUsage) * 2);
    return this.getString();
  }

  getString() {
    var arr = [];
    for (var i = 0; i < this.cpuLogs.length; i++) {
      arr.push(`${this.xValues[i]},${this.cpuLogs[i]}`);
    }
    return arr.join(" ");
  }
}
