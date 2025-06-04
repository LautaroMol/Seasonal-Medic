using APISeasonalMedic.Models;
using AutoMapper;

namespace APISeasonalMedic.DTOs
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<CreditCard, CreditCardDto>().ReverseMap();
            CreateMap<Abono, AbonoDto>().ReverseMap();
            CreateMap<MovimientosAbono, MovAbonosDto>().ReverseMap();
            CreateMap<User, UserDto>();
            CreateMap<RegisterDto, User>();
        }
    }
}

