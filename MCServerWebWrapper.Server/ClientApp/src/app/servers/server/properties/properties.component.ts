import { Component, OnInit, Input, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ServerProperties } from '../../../models/server-properties';

@Component({
  selector: 'server-properties',
  templateUrl: './properties.component.html',
  styleUrls: ['./properties.component.css']
})
export class PropertiesComponent implements OnInit {
  @Input() id: string;
  public properties: ServerProperties;
  http: HttpClient;
  baseUrl: string;

  constructor(_http: HttpClient, @Inject('BASE_URL') _baseUrl: string) {
    this.http = _http;
    this.baseUrl = _baseUrl;
  }

  ngOnInit() {
    this.http.get<ServerProperties>(this.baseUrl + `api/MCServer/GetServerPropertiesById?id=${this.id}`).subscribe(result => {
      this.properties = result;
    });
  }

  saveProperties() {
    console.log(this.properties);
  }
}
