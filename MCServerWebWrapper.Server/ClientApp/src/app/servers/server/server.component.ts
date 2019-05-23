import { Component, OnInit, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@aspnet/signalr';
import { MinecraftServer } from '../../models/minecraft-server';
import { forEach } from '@angular/router/src/utils/collection';
import { StatusUpdate } from '../../models/status-update';

@Component({
  selector: 'app-server',
  templateUrl: './server.component.html',
  styleUrls: ['./server.component.css']
})
export class ServerComponent implements OnInit {
  private _hubConnection: HubConnection | HubConnectionBuilder;
  serverId: string;
  public currentServer: MinecraftServer;
  public maxRam: number;
  public minRam: number;
  public outputLines: string[] = [];
  public isRunning: boolean;
  cpuPointsString: string = "";
  ramPointsString: string = "";

  constructor(private _http: HttpClient, @Inject('BASE_URL') private _baseUrl: string, private _router: Router, private _route: ActivatedRoute) {
    this._route.params.subscribe(params => {
      this.serverId = params['id'];
    });
  }

  ngOnInit() {
    this._http.get<MinecraftServer>(this._baseUrl + `api/MCServer/GetServerById?id=${this.serverId}`).subscribe(result => {
      this.currentServer = result;
      this.isRunning = this.currentServer.isRunning;
      this.maxRam = this.currentServer.maxRamMB;
      this.minRam = this.currentServer.minRamMB;
      this.currentServer.latestLogs.forEach(output => {
        this.outputLines.push(output.line);
      });
      this.scrollOutputToBottom();
    }, error => console.error(error));

    this._hubConnection = new HubConnectionBuilder()
      .withUrl(this._baseUrl + 'angular-hub')
      .configureLogging(LogLevel.Information)
      .build();

    this._hubConnection.start().catch(err => console.error(err.toString()));

    this._hubConnection.on("outputreceived", (id: string, msg: string) => {
      if (id == this.serverId) {
        this.outputLines.push(msg);
        this.scrollOutputToBottom();
      }
    });

    this._hubConnection.on("statusupdate", (id: string, update: StatusUpdate) => {
      if (id == this.serverId) {
        this.cpuPointsString = update.cpuUsageString;
        this.ramPointsString = update.ramUsageString;
        console.log(this.ramPointsString);
      }
    });

    this._hubConnection.on("serverstarted", (id: string) => {
      if (this.serverId == id) {
        this.isRunning = true;
      }
    });

    this._hubConnection.on("serverstopped", (id: string) => {
      if (this.serverId == id) {
        this.isRunning = false;
      }
    });
  }

  scrollOutputToBottom() {
    var outputEle = document.getElementById("output-list");
    outputEle.scrollTop = outputEle.scrollHeight - outputEle.clientHeight;
    return;
  }

  sendConsoleInput() {
    var inputEle = <HTMLInputElement>document.getElementById("console-input");
    if (inputEle.value != null) {
      this._http.get(this._baseUrl + `api/MCServer/SendConsoleInput?serverId=${this.serverId}&msg=${inputEle.value}`).subscribe(() => {
        if (inputEle.value.toLowerCase() == "stop") this.isRunning = false;
        inputEle.value = "";
      });
    }
  }

  startServer() {
    if (this.minRam > this.maxRam) {
      this.outputLines.push("Min ram cannot exceed max ram.");
      this.scrollOutputToBottom();
    }
    else {
      this.outputLines.push("Starting Server...");
      this.scrollOutputToBottom();
      this._http.get(this._baseUrl + `api/MCServer/StartServer?id=${this.serverId}&maxRamMB=${this.maxRam}&minRamMB=${this.minRam}`).subscribe(error => console.error(error));
    }
  }

  stopServer() {
    this._http.get(this._baseUrl + `api/MCServer/StopServer?id=${this.serverId}`).subscribe(error => console.error(error));
  }

  removeServer() {
    this._http.get(this._baseUrl + `api/MCServer/RemoveServer?id=${this.serverId}`).subscribe(() => {
      this._router.navigate(['servers/']);
    }, error => console.error(error));
  }
}
