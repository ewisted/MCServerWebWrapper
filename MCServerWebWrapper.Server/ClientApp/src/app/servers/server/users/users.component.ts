import { Component, OnInit, Inject, Input } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { MatSnackBar } from '@angular/material';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@aspnet/signalr';
import { User } from '../../../models/user';
import { trigger, state, style, transition, animate } from '@angular/animations';

@Component({
  selector: 'server-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.css'],
  animations: [
    trigger('detailExpand', [
      state('collapsed', style({ height: '0px', minHeight: '0', display: 'none' })),
      state('expanded', style({ height: '*' })),
      transition('expanded <=> collapsed', animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)')),
    ]),
  ],
})
export class UsersComponent implements OnInit {
  @Input() id: string;
  private _hubConnection: HubConnection | HubConnectionBuilder;
  public joinedUsers: string[] = [];
  public users: User[];
  expandedUser: User | null;
  columnsToDisplay = ['username'];


  constructor(private _http: HttpClient, @Inject('BASE_URL') private _baseUrl: string, private snackBar: MatSnackBar) { }

  ngOnInit() {
    this._http.get<User[]>(this._baseUrl + `api/MCServer/GetUsersByServerId?id=${this.id}`).subscribe(result => {
      this.users = result;
      this.users.forEach(u => {
        if (u.connectedServerId == this.id) {
          this.joinedUsers.push(u.username);
        }
      });
    });

    this._hubConnection = new HubConnectionBuilder()
      .withUrl(this._baseUrl + 'angular-hub')
      .configureLogging(LogLevel.Information)
      .build();

    this._hubConnection.start().catch(err => console.error(err.toString()));

    this._hubConnection.on("userjoined", (id: string, username: string) => {
      if (id == this.id) {
        this.joinedUsers.push(username);
        var user = new User();
        user.connectedServerId = id;
        user.username = username;
        var index = this.users.findIndex(u => u.username == username);
        if (index == null) {
          this.users.push(user);
        }
        else {
          this.users[index] = user;
        }
      }
    });

    this._hubConnection.on("userleft", (id: string, username: string) => {
      if (id == this.id) {
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
  }

  kick(user: User) {
    var userList: string[] = [user.username];
    var httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' })
    };
    this._http.post<string[]>(this._baseUrl + `api/MCServer/KickUsers?serverId=${this.id}`, userList, httpOptions).subscribe(() => {
      this.snackBar.open("Selected users have been kicked.", "Close", {
        duration: 5000,
      });
    }, error => console.log(error));
  }

  ban(user: User) {
    var userList: string[] = [user.username];
    var httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' })
    };
    this._http.post<string[]>(this._baseUrl + `api/MCServer/BanUsers?serverId=${this.id}`, userList, httpOptions).subscribe(() => {
      this.snackBar.open("Selected users have been banned.", "Close", {
        duration: 5000,
      });
    }, error => console.log(error));
  }

  kickSet(users: any[]) {
    var userList: string[] = [];
    for (var i = 0; i < users.length; i++) {
      userList.push(users[i].value);
    }
    var httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' })
    };
    this._http.post<string[]>(this._baseUrl + `api/MCServer/KickUsers?serverId=${this.id}`, userList, httpOptions).subscribe(() => {
      this.snackBar.open("Selected users have been kicked.", "Close", {
        duration: 5000,
      });
    }, error => console.log(error));
  }

  banSet(users: any) {
    var userList: string[] = [];
    for (var i = 0; i < users.length; i++) {
      userList.push(users[i].value);
    }
    var httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' })
    };
    this._http.post<string[]>(this._baseUrl + `api/MCServer/BanUsers?serverId=${this.id}`, userList, httpOptions).subscribe(() => {
      this.snackBar.open("Selected users have been banned.", "Close", {
        duration: 5000,
      });
    }, error => console.log(error));
  }
}
