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
            CreateMap<CreateAbonoDto, Abono>();
            CreateMap<Abono, AbonoDto>();
            CreateMap<AbonoDto, Abono>();
            CreateMap<ConsultaMedica, ConsultaMedicaDto>()
                .ForMember(dest => dest.NombreCompleto, opt =>
                    opt.MapFrom(src => src.Usuario.FirstName + " " + src.Usuario.LastName))
                .ForMember(dest => dest.Email, opt =>
                    opt.MapFrom(src => src.Usuario.Email))
                .ForMember(dest => dest.DNI, opt =>
                    opt.MapFrom(src => src.Usuario.DNI));
            // De DTO de creación a entidad (solo mapea campos del usuario)
            CreateMap<CreateConsultaMedicaDto, ConsultaMedica>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "Pendiente"))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Usuario, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.Fecha, opt => opt.Ignore()) // Fecha la asigna el agente luego
                .ForMember(dest => dest.Medico, opt => opt.Ignore())
                .ForMember(dest => dest.Especialidad, opt => opt.Ignore());

            // De DTO de actualización a entidad
            CreateMap<UpdateConsultaMedicaDto, ConsultaMedica>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}

