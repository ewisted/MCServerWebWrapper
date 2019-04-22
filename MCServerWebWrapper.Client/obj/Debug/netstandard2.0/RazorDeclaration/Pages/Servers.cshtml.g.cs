#pragma checksum "C:\Users\RGBeast\source\repos\MCServerWebWrapper\MCServerWebWrapper.Client\Pages\Servers.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "8eb9c677cd97b63f5e03a9350de0d6dd0f946de7"
// <auto-generated/>
#pragma warning disable 1591
#pragma warning disable 0414
#pragma warning disable 0649
#pragma warning disable 0169

namespace MCServerWebWrapper.Client.Pages
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
    using System.Net.Http;
    using Microsoft.AspNetCore.Components.Forms;
    using Microsoft.AspNetCore.Components.Layouts;
    using Microsoft.AspNetCore.Components.Routing;
    using Microsoft.JSInterop;
    using MCServerWebWrapper.Client;
    using MCServerWebWrapper.Client.Shared;
    using Blazor.Extensions;
    using MCServerWebWrapper.Shared.SignalR;
    using MCServerWebWrapper.Shared.DTOs;
    [Microsoft.AspNetCore.Components.Layouts.LayoutAttribute(typeof(MainLayout))]

    [Microsoft.AspNetCore.Components.RouteAttribute("/servers")]
    public class Servers : Microsoft.AspNetCore.Components.ComponentBase
    {
        #pragma warning disable 1998
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.RenderTree.RenderTreeBuilder builder)
        {
        }
        #pragma warning restore 1998
#line 52 "C:\Users\RGBeast\source\repos\MCServerWebWrapper\MCServerWebWrapper.Client\Pages\Servers.cshtml"
            
	string serverId;
	string consoleInput;
	string serverName;
	int maxRam = 2048;
	int minRam = 2048;
	MinecraftServerDTO currentServer;
	List<string> outputLines = new List<string>();

	protected override async Task OnInitAsync()
	{
		var connection = new HubConnectionBuilder()
			.WithUrl("/signalr",
			opt =>
			{
				opt.LogLevel = SignalRLogLevel.Trace;
				opt.Transport = HttpTransportType.WebSockets;
			})
			.Build(JSRuntime);

		connection.On<string, string>(SignalrMethodNames.ServerOutput, this.HandleOutput);

		await connection.StartAsync();

	}

	private async void NewServer()
	{
		if (!String.IsNullOrEmpty(serverName) && maxRam >= minRam)
		{
			currentServer = await Http.GetJsonAsync<MinecraftServerDTO>($"api/MCServer/NewServer?name={serverName}&maxRamMB={maxRam}&minRamMB={minRam}");
			serverId = currentServer.Id;
			Console.WriteLine(serverId);
			Console.WriteLine(currentServer);
		}
	}

	private void SendConsoleInput()
	{

	}

	private async void StartServer()
	{
		if (!String.IsNullOrEmpty(serverId))
		{
			outputLines.Add("Starting Server...");
			this.StateHasChanged();
			await Http.GetAsync($"api/MCServer/StartServer?id={serverId}");
		}
		else
		{
			outputLines.Add("No server selected");
			this.StateHasChanged();
		}
	}

	private async void StopServer()
	{
		await Http.GetAsync($"api/MCServer/StopServer?id={serverId}");
	}

	private async Task HandleOutput(string id, string msg)
	{
		if (id != serverId) return;
		else
		{
			outputLines.Add(msg);
			this.StateHasChanged();
			await JSRuntime.InvokeAsync<bool>("ScrollToBottom");
			return;
		}
	}

#line default
#line hidden
        [global::Microsoft.AspNetCore.Components.InjectAttribute] private HttpClient Http { get; set; }
        [global::Microsoft.AspNetCore.Components.InjectAttribute] private IJSRuntime JSRuntime { get; set; }
    }
}
#pragma warning restore 1591
