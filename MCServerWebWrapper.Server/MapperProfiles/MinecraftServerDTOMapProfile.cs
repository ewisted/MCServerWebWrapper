using AutoMapper;
using MCServerWebWrapper.Server.Models.DTOs;
using MCServerWebWrapper.Server.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCServerWebWrapper.Server.MapperProfiles
{
	public class MinecraftServerDTOMapProfile : Profile
	{
		public MinecraftServerDTOMapProfile()
		{
			CreateMap<JavaServer, MinecraftServerDTO>()
				.ForMember(x =>
					x.LatestLogs,
					opt => opt.MapFrom(m => m.Logs.TakeLast(200).ToList()))
				.ForMember(x =>
					x.PercentUpTime,
					//opt => opt.MapFrom(m => Convert.ToInt32(((m.IsRunning ? m.TotalUpTime + (DateTime.UtcNow - m.DateLastStarted) : m.TotalUpTime) / (DateTime.UtcNow - m.DateCreated)) * 100)))
					opt => opt.Ignore())
				.ForMember(x =>
					x.PlayerCountChanges,
					opt => opt.MapFrom(m => m.PlayerCountChanges.TakeLast(200).ToList()))
				.ForMember(x => 
					x.TotalUpTimeMs,
					opt => opt.MapFrom(m => m.TotalUpTime.TotalMilliseconds));
		}
	}
}
