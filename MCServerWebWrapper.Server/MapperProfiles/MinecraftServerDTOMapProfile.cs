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
			CreateMap<MinecraftServer, MinecraftServerDTO>()
				.ForMember(x => 
					x.LatestLogs,
					opt => opt.MapFrom(m => m.Logs.TakeLast(200).ToList()))
				.ForMember(x => 
					x.TotalUpTimeSeconds, 
					opt => opt.MapFrom(m => m.TotalUpTime.TotalSeconds));
		}
	}
}
