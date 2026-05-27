using AutoMapper;
using JobPortal_API.DTOs;
using JobPortal_API.Models;

namespace JobPortal_API.Utilities
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Candidato, CandidatoDTO>();
            CreateMap<CandidatoDTO, Candidato>();

            CreateMap<AplicacaoTrabalho, AplicacaoTrabalhoDTO>();
            CreateMap<AplicacaoTrabalhoDTO, AplicacaoTrabalho>();

            CreateMap<Empresa, EmpresaDTO>();
            CreateMap<EmpresaDTO, Empresa>();

            CreateMap<OfertaEmprego, OfertaEmpregoDTO>();
            CreateMap<OfertaEmpregoDTO, OfertaEmprego>();

            CreateMap<Foto, FotoDTO>();
            CreateMap<FotoDTO, Foto>();

            CreateMap<LogoEmpresa, LogoEmpresaDTO>();
            CreateMap<LogoEmpresaDTO, LogoEmpresa>();

            CreateMap<FileCV, FileCVDTO>();
            CreateMap<FileCVDTO, FileCV>();

            CreateMap<ReviewDTO, Review>();
            CreateMap<Review, ReviewDTO>();

            CreateMap<NotificationsDTO, Notifications>();
            CreateMap<Notifications, NotificationsDTO>();

            CreateMap<CV, CVDTO>().ReverseMap(); // ALTERAÇÃO 26/05
        }
    }
}
