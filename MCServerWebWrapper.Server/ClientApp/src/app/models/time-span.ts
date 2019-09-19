export class TimeSpan {

  public static getTimeString(ms: number): string {
    var totalSeconds = Math.floor(ms / 1000);
    var totalMinutes = Math.floor(totalSeconds / 60);
    var totalHours = Math.floor(totalMinutes / 60);

    var days = Math.floor(totalHours / 24);
    var hours = totalHours % 24;
    var minutes = totalMinutes % 60;
    var seconds = totalSeconds % 60;;

    return `${days}d:${hours}h:${minutes}m:${seconds}s`;
  }
}
