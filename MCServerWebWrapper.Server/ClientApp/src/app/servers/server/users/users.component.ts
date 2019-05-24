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
  @Input() joinedUsers: string[];
  @Input() users: User[];
  expandedUser: User | null;
  columnsToDisplay = ['username'];


  constructor(private _http: HttpClient, @Inject('BASE_URL') private _baseUrl: string, private snackBar: MatSnackBar) { }

  ngOnInit() {
    this._http.get<User[]>(this._baseUrl + `api/MCServer/GetUsersByServerId?id=${this.id}`).subscribe(result => {
      if (result != null) {
        this.users = result;
        this.users.forEach(u => {
          if (u.connectedServerId == this.id) {
            this.joinedUsers.push(u.username);
          }
        });
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
