using AutoMapper;
using ProgramDesigner.Application.DTOs;
using ProgramDesigner.Domain.Entities;
using System;

namespace ProgramDesigner.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Entity to DTO
            CreateMap<ProgramEntity, ProgramDto>();
            
            CreateMap<ProgramNode, ProgramNodeDto>()
                .ForMember(dest => dest.IsGroup, opt => opt.MapFrom(src => src is Group))
                .Include<Group, ProgramNodeDto>()
                .Include<Step, ProgramNodeDto>();
                
            CreateMap<Group, ProgramNodeDto>()
                .ForMember(dest => dest.Rule, opt => opt.MapFrom(src => src.Rule))
                .ForMember(dest => dest.PickCount, opt => opt.MapFrom(src => src.PickCount))
                .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.Children));
                
            CreateMap<Step, ProgramNodeDto>();

            // DTO to Entity
            CreateMap<CreateProgramDto, ProgramEntity>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()));
                
            CreateMap<ProgramNodeDto, ProgramNode>()
                .ConstructUsing(dto => dto.IsGroup ? (ProgramNode)new Group() : new Step())
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id != Guid.Empty ? src.Id : Guid.NewGuid()))
                .Include<ProgramNodeDto, Group>()
                .Include<ProgramNodeDto, Step>();
                
            CreateMap<ProgramNodeDto, Group>()
                .ForMember(dest => dest.Rule, opt => opt.MapFrom(src => src.Rule.GetValueOrDefault()))
                .ForMember(dest => dest.PickCount, opt => opt.MapFrom(src => src.PickCount))
                .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.Children));
                
            CreateMap<ProgramNodeDto, Step>();
        }
    }
}
