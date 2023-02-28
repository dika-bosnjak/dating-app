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

            //map the message to the messageDTO with sender and recipient photos
            CreateMap<Message, MessageDTO>()
            .ForMember(dest => dest.SenderPhotoUrl, opt => opt.MapFrom(src => src.Sender.Photos.FirstOrDefault(x => x.IsMain).Url))
            .ForMember(dest => dest.RecipientPhotoUrl, opt => opt.MapFrom(src => src.Recipient.Photos.FirstOrDefault(x => x.IsMain).Url));

            //map the date time to date time using utc datetime
            CreateMap<DateTime, DateTime>().ConvertUsing(d => DateTime.SpecifyKind(d, DateTimeKind.Utc));
            //map the optional date time to optional date time using utc datetime
            CreateMap<DateTime?, DateTime?>().ConvertUsing(d => d.HasValue ? DateTime.SpecifyKind(d.Value, DateTimeKind.Utc) : null);
        }
    }
}