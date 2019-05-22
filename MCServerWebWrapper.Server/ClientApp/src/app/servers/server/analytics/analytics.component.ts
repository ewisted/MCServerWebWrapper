import { Component, OnInit, Input, Inject } from '@angular/core';
import { MinecraftServer } from '../../../models/minecraft-server';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@aspnet/signalr';
import { StatusUpdate } from '../../../models/status-update';
import { CpuData } from '../../../models/cpu-data';

@Component({
  selector: 'server-analytics',
  templateUrl: './analytics.component.html',
  styleUrls: ['./analytics.component.css']
})
export class AnalyticsComponent implements OnInit {
  @Input() server: MinecraftServer;
  private _hubConnection: HubConnection | HubConnectionBuilder;
  percentUpTime: number = 0;
  cpuData: CpuData = new CpuData();
  cpuPointsString: string = "";
  ramPoints: string[] = [];
  ramPointsString: string = "";
  xValuesIncrementor = 0;

  constructor(@Inject('BASE_URL') private _baseUrl: string) { }

  ngOnInit() {
    this.percentUpTime = this.server.percentUpTime;
    var dataPoints = [];
    this.server.playerCountChanges.forEach(change => {
      dataPoints.push({ x: change.timestamp, y: change.playersCurrentlyConnected });
    });

    this._hubConnection = new HubConnectionBuilder()
      .withUrl(this._baseUrl + 'angular-hub')
      .configureLogging(LogLevel.Information)
      .build();

    this._hubConnection.start().catch(err => console.error(err.toString()));

    this._hubConnection.on("statusupdate", (id: string, update: StatusUpdate) => {
      if (id == this.server.id) {
        this.cpuPointsString = this.cpuData.addData(update.cpuUsuagePercent);

        if (this.ramPoints.length >= 60) {
          this.ramPoints.shift();
        }
        this.ramPoints.push(`${this.ramPoints.length},${update.ramUsageMB}`);
        this.ramPointsString = this.ramPoints.join(" ");
      }
    });
  }

  formatX(input: number) {
    return (60 - input) * 7;
  }

  formatY(input: number) {
    return (100 - input) * 2;
  }
}
