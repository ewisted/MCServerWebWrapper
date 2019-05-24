import { Component, OnInit, Input, Inject } from '@angular/core';
import { MinecraftServer } from '../../../models/minecraft-server';
import { StatusUpdate } from '../../../models/status-update';

@Component({
  selector: 'server-analytics',
  templateUrl: './analytics.component.html',
  styleUrls: ['./analytics.component.css']
})
export class AnalyticsComponent implements OnInit {
  @Input() server: MinecraftServer;
  @Input() cpuPointsString: string;
  @Input() ramPointsString: string;
  @Input() upTimeThisSession: string;
  @Input() totalUpTime: string;
  percentUpTime: number = 0;

  constructor(@Inject('BASE_URL') private _baseUrl: string) { }

  ngOnInit() {
    this.percentUpTime = this.server.percentUpTime;
    var dataPoints = [];
    this.server.playerCountChanges.forEach(change => {
      dataPoints.push({ x: change.timestamp, y: change.playersCurrentlyConnected });
    });
  }
}
