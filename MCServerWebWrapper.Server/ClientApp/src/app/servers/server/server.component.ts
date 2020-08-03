import { Component, OnInit, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@aspnet/signalr';
import { MinecraftServer } from '../../models/minecraft-server';
import { forEach } from '@angular/router/src/utils/collection';
import { StatusUpdate } from '../../models/status-update';
import { TimeSpan } from '../../models/time-span';
import { User } from '../../models/user';

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

  joinedUsers: string[] = [];
  users: User[] = [];

  cpuPointsString: string;
  ramPointsString: string;
  upTimeThisSession: string;
	totalUpTime: string;

	jarDownloadProgress: number;
	jarDownloadComplete: boolean;

  constructor(private _http: HttpClient, @Inject('BASE_URL') private _baseUrl: string, private _router: Router, private _route: ActivatedRoute) {
    this._route.params.subscribe(params => {
      this.serverId = params['id'];
    });
  }

  ngOnInit() {
    this._http.get<MinecraftServer>(this._baseUrl + `api/MCServer/GetServerById?id=${this.serverId}`).subscribe(result => {
      this.currentServer = result;
      this.currentServer.dateCreated = new Date(Date.parse(result.dateCreated.toString()));
      this.currentServer.dateLastStarted = new Date(Date.parse(result.dateLastStarted.toString()));
      this.currentServer.dateLastStopped = new Date(Date.parse(result.dateLastStopped.toString()));
      this.isRunning = this.currentServer.isRunning;
      if (this.isRunning) {
        this.currentServer.totalUpTimeMs = result.totalUpTimeMs + (Date.now() - this.currentServer.dateLastStarted.getTime());
        this.upTimeThisSession = TimeSpan.getTimeString(Date.now() - this.currentServer.dateLastStarted.getTime());
      }
      this.totalUpTime = TimeSpan.getTimeString(this.currentServer.totalUpTimeMs);
      this.maxRam = this.currentServer.maxRamMB;
      this.minRam = this.currentServer.minRamMB;
      this.currentServer.latestLogs.forEach(output => {
        this.outputLines.push(output.line);
      });
      setTimeout(() => {
        this.scrollOutputToBottom();
      }, 100);
    }, error => console.error(error));

	  this.setupSignalR();
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
      setTimeout(() => {
        this.scrollOutputToBottom();
      }, 10);
      this._http.get(this._baseUrl + `api/MCServer/StartServer?id=${this.serverId}&maxRamMB=${this.maxRam}&minRamMB=${this.minRam}`).subscribe(() => {
        this.currentServer.dateLastStarted = new Date();
        this.isRunning = true;
      }, error => console.error(error));
    }
  }

  stopServer() {
    this._http.get(this._baseUrl + `api/MCServer/StopServer?id=${this.serverId}`).subscribe(() => {
      this.currentServer.dateLastStopped = new Date();
      this.isRunning = false;
      this.cpuPointsString = null;
      this.ramPointsString = null;
      this.upTimeThisSession = null;
    }, error => console.error(error));
  }

  removeServer() {
    this._http.get(this._baseUrl + `api/MCServer/RemoveServer?id=${this.serverId}`).subscribe(() => {
      this._router.navigate(['servers/']);
    }, error => console.error(error));
	}

	setupSignalR() {
		this._hubConnection = new HubConnectionBuilder()
			.withUrl(this._baseUrl + 'angular-hub')
			.configureLogging(LogLevel.Information)
			.build();

		this._hubConnection.start().catch(err => console.error(err.toString()));

		this._hubConnection.on("outputreceived", (id: string, msg: string) => {
			if (id == this.serverId) {
				this.outputLines.push(msg);
				setTimeout(() => {
					this.scrollOutputToBottom();
				}, 10);
			}
		});

		this._hubConnection.on("statusupdate", (id: string, update: StatusUpdate) => {
			if (id == this.serverId) {
				this.cpuPointsString = update.cpuUsageString;
				this.ramPointsString = update.ramUsageString;
				if (this.isRunning) {
					var upTimeThisSessionMs = Date.now() - this.currentServer.dateLastStarted.getTime()
					this.upTimeThisSession = TimeSpan.getTimeString(upTimeThisSessionMs);
					this.totalUpTime = TimeSpan.getTimeString(this.currentServer.totalUpTimeMs + upTimeThisSessionMs);
				}
			}
		});

		this._hubConnection.on("serverstarted", (id: string) => {
			if (this.serverId == id) {
				this.currentServer.dateLastStarted = new Date();
				this.isRunning = true;
			}
		});

		this._hubConnection.on("serverstopped", (id: string) => {
			if (this.serverId == id) {
				this.currentServer.dateLastStopped = new Date();
				this.isRunning = false;
				this.cpuPointsString = null;
				this.ramPointsString = null;
				this.upTimeThisSession = null;
			}
		});

		this._hubConnection.on("userjoined", (id: string, username: string) => {
			if (id == this.serverId) {
				this.joinedUsers.push(username);
				var user = new User();
				user.connectedServerId = id;
				user.username = username;
				var index = this.users.findIndex(u => u.username == username);
				if (index == -1) {
					this.users.push(user);
				}
				else {
					this.users[index] = user;
				}
			}
		});

		this._hubConnection.on("userleft", (id: string, username: string) => {
			if (id == this.serverId) {
				var index = this.joinedUsers.indexOf(username);
				var newUserList: string[] = [];
				for (var i = 0; i < this.joinedUsers.length; i++) {
					if (i != index) {
						newUserList.push(this.joinedUsers[i]);
					}
				}
				index = this.users.findIndex(u => u.username == username);
				if (index != null) {
					var user = new User();
					user.connectedServerId = "";
					user.username = username;
					this.users[index] = user;
				}
				this.joinedUsers = newUserList;
			}
		});

		this._hubConnection.on("jardownloadprogresschanged", (id: string, progress: number) => {
			if (id == this.serverId) {
				this.jarDownloadProgress = progress;
			}
		});

		this._hubConnection.on("jardownloadcompleted", (id: string) => {
			if (id == this.serverId) {
				this.jarDownloadComplete = true;
			}
		});
	}
}
