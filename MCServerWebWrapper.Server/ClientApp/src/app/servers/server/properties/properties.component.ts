import { Component, OnInit, Input, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ServerProperties } from '../../../models/server-properties';
import { MatSnackBar } from '@angular/material';

@Component({
  selector: 'server-properties',
  templateUrl: './properties.component.html',
  styleUrls: ['./properties.component.css']
})
export class PropertiesComponent implements OnInit {
  @Input() id: string;
  public properties: ServerProperties;

  constructor(private _http: HttpClient, @Inject('BASE_URL') private _baseUrl: string, private snackBar: MatSnackBar) { }

  ngOnInit() {
    this._http.get<ServerProperties>(this._baseUrl + `api/MCServer/GetServerPropertiesById?id=${this.id}`).subscribe(result => {
      this.properties = result;
    });
  }

  saveProperties() {
    var httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' })
    };
    this._http.post<ServerProperties>(this._baseUrl + `api/MCServer/SaveServerProperties?id=${this.id}`, this.properties, httpOptions).subscribe(() => {
      this.snackBar.open("Server properties have been saved and will take effect next restart.", "Close", {
        duration: 5000,
      });
    }, error => console.log(error));
  }
}
