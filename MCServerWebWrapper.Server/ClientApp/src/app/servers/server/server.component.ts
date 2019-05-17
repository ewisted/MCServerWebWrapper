import { Component, OnInit, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@aspnet/signalr';
import { MinecraftServer } from '../../models/minecraft-server';

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
    }, error => console.error(error));

    this._hubConnection = new HubConnectionBuilder()
      .withUrl(this._baseUrl + 'angular-hub')
      .configureLogging(LogLevel.Information)
      .build();

    this._hubConnection.start().catch(err => console.error(err.toString()));

    this._hubConnection.on("outputreceived", (id: string, msg: string) => {
      if (id == this.serverId) {
        this.outputLines.push(msg);
        var outputEle = document.getElementById("output-list");
        outputEle.scrollTop = outputEle.scrollHeight;
      }
    });
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
    }
    else {
      this.outputLines.push("Starting Server...");
      this._http.get(this._baseUrl + `api/MCServer/StartServer?id=${this.serverId}&maxRamMB=${this.maxRam}&minRamMB=${this.minRam}`).subscribe(result => {
        if (result == true) {
          this.isRunning = true;
        }
        else {
          console.error(result.toString())
        }
      }, error => console.error(error));
    }
  }

  stopServer() {
    this._http.get(this._baseUrl + `api/MCServer/StopServer?id=${this.serverId}`).subscribe(() => {
      this.isRunning = false;
    }, error => console.error(error));
  }

  removeServer() {
    this._http.get(this._baseUrl + `api/MCServer/RemoveServer?id=${this.serverId}`).subscribe(() => {
      this._router.navigate(['servers/']);
    }, error => console.error(error));
  }
}
