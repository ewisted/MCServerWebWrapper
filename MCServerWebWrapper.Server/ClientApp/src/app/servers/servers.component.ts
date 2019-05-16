import { Component, OnInit, Inject } from '@angular/core';
import { MinecraftServer } from '../models/minecraft-server';
import { HttpClient } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-servers',
  templateUrl: './servers.component.html',
  styleUrls: ['./servers.component.css']
})
export class ServersComponent implements OnInit {
  public servers: MinecraftServer[];
  http: HttpClient;
  baseUrl: string;
  router: Router;
  route: ActivatedRoute;


  constructor(_http: HttpClient, @Inject('BASE_URL') _baseUrl: string, _router: Router, _route: ActivatedRoute) {
    this.http = _http;
    this.baseUrl = _baseUrl;
    this.router = _router;
    this.route = _route;
  }

  ngOnInit() {
    this.http.get<MinecraftServer[]>(this.baseUrl + 'api/MCServer/GetAllServers').subscribe(result => {
      this.servers = result;
    }, error => console.error(error));
  }

  newServer(newServerName: string) {
    this.http.get<MinecraftServer>(this.baseUrl + `api/MCServer/NewServer?name=${newServerName}`).subscribe(result => {
      this.servers.push(result);
    });
  }

  goToServer(serverId: string) {
    this.router.navigate(['server/', { id: serverId }]);
  }
}
