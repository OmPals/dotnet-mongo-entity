using AutoMapper;
using Project1.Models;

namespace Project1.Helpers
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			CreateMap<RegisterModel, User>();
			CreateMap<UpdateModel, User>();
		}
	}
}