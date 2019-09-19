import { Component, OnInit, Input, Inject } from '@angular/core';
import { MinecraftServer } from '../../../models/minecraft-server';
import { MatSnackBar } from '@angular/material';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { VanillaVersion } from '../../../models/vanilla-version';

@Component({
  selector: 'server-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.css']
})
export class ServerSettingsComponent implements OnInit {
	@Input() server: MinecraftServer;
	@Input() jarDownloadProgress: number;
	@Input() jarDownloadComplete: boolean;
	isDownloading: boolean = false;
	versions: VanillaVersion[];
	selectedVersion: VanillaVersion;

  constructor(private _http: HttpClient, @Inject('BASE_URL') private _baseUrl: string, private snackBar: MatSnackBar) { }

	ngOnInit() {
		this._http.get<VanillaVersion[]>(this._baseUrl + "api/MCServer/GetAvailableVersions").subscribe(result => {
			this.versions = result;
		});
  }

	downloadServerJar() {
		if (this.selectedVersion != null) {
			let versionId = this.selectedVersion.id;
			this.isDownloading = true;
			var httpOptions = {
				headers: new HttpHeaders({ 'Content-Type': 'application/json' })
			};
			this._http.post<VanillaVersion>(this._baseUrl + `api/MCServer/DownloadServerJar?id=${this.server.id}`, this.selectedVersion, httpOptions).subscribe(() => {
				this.isDownloading = false;
				this.server.serverVersion = versionId;
			}, error => console.log(error));
		}
	}
}
