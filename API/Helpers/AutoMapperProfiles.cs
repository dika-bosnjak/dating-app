using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers
{
    //AutoMapperProfiles class is used to map entities to dtos and dtos to entities
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            //map app user to member dto using main photo as photourl and calculate age
            CreateMap<AppUser, MemberDTO>()
            .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.Photos.FirstOrDefault(x => x.IsMain).Url))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));

            //map photo to photo dto
            CreateMap<Photo, PhotoDTO>();

            //map member update dto (updated data on UI) to app user (which is saved in the db)
            CreateMap<MemberUpdateDTO, AppUser>();

            //map register dto (inserted data during the registration process) to app user (which is saved in the db)
            CreateMap<RegisterDTO, AppUser>();
        }
    }
}