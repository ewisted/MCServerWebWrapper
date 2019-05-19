using AutoMapper;
using MCServerWebWrapper.Server.Data.Models;
using MCServerWebWrapper.Models.DTOs;

namespace MCServerWebWrapper.Server.MapperProfiles
{
	public class MinecraftServerDTOMapProfile : Profile
	{
		public MinecraftServerDTOMapProfile()
		{
			CreateMap<MinecraftServer, MinecraftServerDTO>().ForMember(x => x.LatestLogs, opt => opt.Ignore());
		}
	}
}