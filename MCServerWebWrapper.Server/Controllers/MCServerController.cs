using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MCServerWebWrapper.Server.Data;
using MCServerWebWrapper.Server.Services;
using MCServerWebWrapper.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace MCServerWebWrapper.Server.Controllers
{
	[Route("api/[controller]")]
	public class MCServerController : Controller
	{
		private readonly MCServerService _serverService;
		private readonly IMapper _mapper;
		private readonly IServerRepo _repo;

		public MCServerController(MCServerService serverService, IMapper mapper, IServerRepo repo)
		{
			_serverService = serverService;
			_mapper = mapper;
			_repo = repo;
		}

		[HttpGet("[action]")]
		public async Task<IActionResult> NewServer([Required] string name, [Required] int maxRamMB, [Required] int minRamMB)
		{
			MinecraftServerDTO serverDTO;
			try
			{
				var server = await _serverService.NewServer(name, maxRamMB, minRamMB);
				serverDTO = _mapper.Map<MinecraftServerDTO>(server);
			}
			catch (Exception e)
			{
				return StatusCode(500, e.Data);
			}
			
			return Ok(serverDTO);
		}

		[HttpGet("[action]")]
		public async Task<IActionResult> StartServer([Required] string id)
		{
			await _serverService.StartServerById(id);
			return Ok();
		}

		[HttpGet("[action]")]
		public async Task<IActionResult> StopServer([Required] string id)
		{
			await _serverService.StopServerById(id);
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
		public async Task<IActionResult> GetServerById([Required] string id)
		{
			var server = await _repo.GetServerById(id);
			var serverDTO = _mapper.Map<MinecraftServerDTO>(server);
			return Ok(serverDTO);
		}

		[HttpGet("[action]")]
		public async Task<IActionResult> SendConsoleInput([Required] string serverId, [Required] string msg)
		{
			try
			{
				await _serverService.SendConsoleInput(serverId, msg);
			}
			catch (Exception e)
			{
				return StatusCode(500, e.Data);
			}
			return Ok();
		}
	}
}