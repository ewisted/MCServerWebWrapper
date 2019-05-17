import { Component, OnInit, Inject } from '@angular/core';
import { MinecraftServer } from '../models/minecraft-server';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-servers',
  templateUrl: './servers.component.html',
  styleUrls: ['./servers.component.css']
})
export class ServersComponent implements OnInit {
  public servers: MinecraftServer[];

  constructor(private _http: HttpClient, @Inject('BASE_URL') private _baseUrl: string, private _router: Router) { }

  ngOnInit() {
    this._http.get<MinecraftServer[]>(this._baseUrl + 'api/MCServer/GetAllServers').subscribe(result => {
      this.servers = result;
    }, error => console.error(error));
  }

  newServer(newServerName: string) {
    this._http.get<MinecraftServer>(this._baseUrl + `api/MCServer/NewServer?name=${newServerName}`).subscribe(result => {
      this.servers.push(result);
    });
  }

  goToServer(serverId: string) {
    this._router.navigate(['server/', { id: serverId }]);
  }
}
