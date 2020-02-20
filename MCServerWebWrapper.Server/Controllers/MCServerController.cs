using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MCServerWebWrapper.Server.Data;
using MCServerWebWrapper.Server.Models;
using MCServerWebWrapper.Server.Services;
using MCServerWebWrapper.Server.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace MCServerWebWrapper.Server.Controllers
{
	[Route("api/[controller]")]
	public class MCServerController : Controller
	{
		private readonly IServerManagementService _serverService;
		private readonly ServerJarService _jarService;
		private readonly IMapper _mapper;
		private readonly IServerRepo _repo;
		private readonly IUserRepo _userRepo;

		public MCServerController(IServerManagementService serverService, ServerJarService jarService, IMapper mapper, IServerRepo repo, IUserRepo userRepo)
		{
			_serverService = serverService;
			_jarService = jarService;
			_mapper = mapper;
			_repo = repo;
			_userRepo = userRepo;
		}

		[HttpGet("[action]")]
		public async Task<IActionResult> GetAvailableVersions()
		{
			try
			{
				var versions = await _jarService.GetJarVersions();
				return Ok(versions);
			}
			catch (Exception ex)
			{
				return StatusCode(500, ex.Message);
			}
		}

		[HttpPost("[action]")]
		public async Task<IActionResult> DownloadServerJar([Required] string id, [FromBody] VanillaVersion version)
		{
			try
			{
				var server = await _repo.GetServerById(id);
				var jarUri = await _jarService.GetJarUriFromVersion(version);
				await _jarService.DownloadJar(id, jarUri, server.ServerPath);
				server.Version = version.Id;
				await _repo.UpsertServer(server);
			}
			catch (Exception ex)
			{
				return StatusCode(500, ex.Message);
			}
			return Ok();
		}

		[HttpPost("[action]")]
		public async Task<IActionResult> SaveServerProperties([Required] string id, [FromBody] ServerProperties properties)
		{
			if (properties == null)
			{
				return StatusCode(400, "Server properties were invalid.");
			}
			try
			{
				await _serverService.SaveServerProperties(id, properties);
			}
			catch (Exception ex)
			{
				return StatusCode(500, ex.Message);
			}
			return Ok();
		}

		[HttpGet("[action]")]
		public async Task<IActionResult> NewServer([Required] string name)
		{
			MinecraftServerDTO serverDTO;
			try
			{
				var server = await _serverService.NewServer(name);
				serverDTO = _mapper.Map<MinecraftServerDTO>(server);
			}
			catch (Exception e)
			{
				return StatusCode(500, e.Data);
			}
			
			return Ok(serverDTO);
		}

		[HttpGet("[action]")]
		public async Task<IActionResult> StartServer([Required] string id, [Required] int maxRamMB, [Required] int minRamMB)
		{
			var result = await _serverService.StartServerById(id, maxRamMB, minRamMB);
			if (result)
			{
				return Ok();
			}
			else
			{
				return StatusCode(500);
			}
		}

		[HttpGet("[action]")]
		public async Task<IActionResult> StopServer([Required] string id)
		{
			await _serverService.StopServerById(id);
			return Ok();
		}

		[HttpGet("[action]")]
		public async Task<IActionResult> RemoveServer([Required] string id)
		{
			await _serverService.RemoveServer(id);
			return Ok();
		}

		[HttpGet("[action]")]
		public IActionResult GetAllServers()
		{
			var servers = _repo.GetServers();
			var serversDTO = new List<MinecraftServerDTO>();
			foreach (var server in servers)
			{
				var serverDTO = _mapper.Map<MinecraftServerDTO>(server);
				serversDTO.Add(serverDTO);
			}
			return Ok(serversDTO);
		}

		[HttpGet("[action]")]
		public async Task<IActionResult> GetUsersByServerId([Required] string id)
		{
			var dbUsers = (await _userRepo.GetUsersByServerId(id)).ToList(); ;
			var users = _mapper.Map<List<UserDTO>>(dbUsers);
			return Ok(users);
		}

		[HttpGet("[action]")]
		public async Task<IActionResult> GetServerById([Required] string id)
		{
			var server = await _repo.GetServerById(id);
			var serverDTO = _mapper.Map<MinecraftServerDTO>(server);
			var lifetime = DateTime.UtcNow - serverDTO.DateCreated;
			if (server.IsRunning)
			{
				server.TotalUpTime = server.TotalUpTime + (DateTime.UtcNow - server.DateLastStarted);
			}
			serverDTO.PercentUpTime = Convert.ToInt32((server.TotalUpTime / lifetime) * 100);
			return Ok(serverDTO);
		}

		[HttpGet("[action]")]
		public async Task<IActionResult> GetServerPropertiesById([Required] string id)
		{
			var serverProperties = (await _repo.GetServerById(id)).Properties;
			if (serverProperties == null)
			{
				return NotFound();
			}
			else
			{
				return Ok(serverProperties);
			}
		}

		[HttpPost("[action]")]
		public async Task<IActionResult> KickUsers([Required] string serverId, [FromBody] List<string> usernames)
		{
			foreach (var username in usernames)
			{
				await _serverService.SendConsoleInput(serverId, $"kick {username}");
			}
			return Ok();
		}

		[HttpPost("[action]")]
		public async Task<IActionResult> BanUsers([Required] string serverId, [FromBody] List<string> usernames)
		{
			foreach (var username in usernames)
			{
				await _serverService.SendConsoleInput(serverId, $"ban {username}");
			}
			return Ok();
		}

		[HttpGet("[action]")]
		public async Task<IActionResult> SendConsoleInput([Required] string serverId, [Required] string msg)
		{
			try
			{
				await _serverService.SendConsoleInput(serverId, msg);
			}
			catch (Exception ex)
			{
				return StatusCode(500, ex.Data);
			}
			return Ok();
		}
	}
}