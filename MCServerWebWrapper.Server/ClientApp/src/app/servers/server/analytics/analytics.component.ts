import { Component, OnInit, Input } from '@angular/core';
import { MinecraftServer } from '../../../models/minecraft-server';

@Component({
  selector: 'server-analytics',
  templateUrl: './analytics.component.html',
  styleUrls: ['./analytics.component.css']
})
export class AnalyticsComponent implements OnInit {
  @Input() server: MinecraftServer;
  percentUpTime: number = 0;

  constructor() { }

  ngOnInit() {
    this.percentUpTime = this.server.percentUpTime;
    var dataPoints = [];
    this.server.playerCountChanges.forEach(change => {
      dataPoints.push({ x: change.timestamp, y: change.playersCurrentlyConnected });
    });
  }
}
