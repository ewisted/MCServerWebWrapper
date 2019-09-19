using MCServerWebWrapper.Server.Hubs;
using MCServerWebWrapper.Server.Models;
using MCServerWebWrapper.Shared.SignalR;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.Services
{
	public class ServerJarService
	{
		private readonly IHttpClientFactory _clientFactory;
		private readonly IHubContext<AngularHub> _angularHub;

		public ServerJarService(IHttpClientFactory clientFactory, IHubContext<AngularHub> angularHub)
		{
			_clientFactory = clientFactory;
			_angularHub = angularHub;
		}

		public async Task<object> GetJarVersions()
		{
			var client = _clientFactory.CreateClient();

			var response = await client.GetAsync("https://launchermeta.mojang.com/mc/game/version_manifest.json");

			if (response.IsSuccessStatusCode)
			{
				var manifest = await response.Content.ReadAsAsync<VersionManifest>();

				var filteredVersions = manifest.Versions.Where(e => e.Type == "release");
				return filteredVersions;
			}
			else
			{
				throw new Exception("Could not load version manifest from specified url.");
			}
		}

		public async Task<Uri> GetJarUriFromVersion(VanillaVersion version)
		{
			var client = _clientFactory.CreateClient();

			var response = await client.GetAsync(version.Url);
			var json = await response.Content.ReadAsStringAsync();
			return OfficialVersion.FromJson(json).Downloads.Server.Url;
		}

		public Task DownloadJar(string serverId, Uri jarUrl, string absServerPath)
		{
			try
			{
				var jarPath = Path.Combine(absServerPath, "server.jar");
				if (File.Exists(jarPath))
				{
					File.Delete(jarPath);
				}
				using (var client = new WebClient())
				{
					client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(async (sender, e) =>
					{
						await _angularHub.Clients.All.SendAsync(SignalrMethodNames.JarDownloadProgressChanged, serverId, e.ProgressPercentage);
					});
					client.DownloadDataCompleted += new DownloadDataCompletedEventHandler(async (sender, e) =>
					{
						await _angularHub.Clients.All.SendAsync(SignalrMethodNames.JarDownloadCompleted, serverId);
					});
					client.DownloadFileAsync(jarUrl, jarPath);
				}
			}
			catch (Exception ex)
			{
				return Task.FromException(ex);
			}

			return Task.CompletedTask;
		}
	}
}
