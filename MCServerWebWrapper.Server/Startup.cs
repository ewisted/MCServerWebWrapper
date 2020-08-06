using AutoMapper;
using MCServerWebWrapper.Server.Data;
using MCServerWebWrapper.Server.Hubs;
using MCServerWebWrapper.Server.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SpaServices.Extensions;
using System;
using Microsoft.AspNetCore.SpaServices.AngularCli;

namespace MCServerWebWrapper
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }
		bool InDocker { get { return Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"; } }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
			services.AddHttpClient();
			services.AddSignalR();
			//services.AddSingleton<IServerManagementService>(sp =>
			//{
			//	switch (InDocker)
			//	{
			//		case true:
			//			return sp.GetService<DockerService>();
			//		case false:
			//			return sp.GetService<ProcessService>();
			//		default:
			//			throw new InvalidOperationException("Could not determine runtime enviornment.");
			//	}
			//});
			services.AddSingleton<IServerManagementService, DockerService>();
			services.AddSingleton<ServerJarService>();
			services.AddTransient<IServerRepo, ServerMongoRepo>();
			services.AddTransient<IUserRepo, UserMongoRepo>();
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest);

			// In production, the Angular files will be served from this directory
			services.AddSpaStaticFiles(configuration =>
			{
				configuration.RootPath = "ClientApp/dist";
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.EnvironmentName == "Development")
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseSpaStaticFiles();

			app.UseRouting();

			app.UseEndpoints(builder =>
			{
				builder.MapHub<AngularHub>("/angular-hub");
				builder.MapDefaultControllerRoute();
			});

			app.UseSpa(spa =>
			{
				// To learn more about options for serving an Angular SPA from ASP.NET Core,
				// see https://go.microsoft.com/fwlink/?linkid=864501

				spa.Options.SourcePath = "ClientApp";

				if (env.EnvironmentName == "Development")
				{
					spa.UseAngularCliServer(npmScript: "start");
				}
			});
		}
	}
}
