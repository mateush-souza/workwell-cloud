using AutoMapper;
using WorkWell.Application.DTOs;
using WorkWell.Domain.Entities;

namespace WorkWell.Application.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Usuario mappings
        CreateMap<Usuario, UsuarioDto>()
            .ForMember(dest => dest.EmpresaNome, opt => opt.MapFrom(src => src.Empresa.Nome))
            .ForMember(dest => dest.DepartamentoNome, opt => opt.MapFrom(src => src.Departamento != null ? src.Departamento.Nome : null));

        CreateMap<CreateUsuarioRequest, Usuario>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SenhaHash, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataAtualizacao, opt => opt.Ignore());

        CreateMap<RegisterRequest, Usuario>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SenhaHash, opt => opt.Ignore())
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => Domain.Enums.UserRole.USER))
            .ForMember(dest => dest.Ativo, opt => opt.MapFrom(src => true));

        // Empresa mappings
        CreateMap<Empresa, EmpresaDto>()
            .ForMember(dest => dest.TotalUsuarios, opt => opt.MapFrom(src => src.Usuarios.Count))
            .ForMember(dest => dest.TotalDepartamentos, opt => opt.MapFrom(src => src.Departamentos.Count));

        CreateMap<CreateEmpresaRequest, Empresa>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DataCadastro, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Departamento mappings
        CreateMap<Departamento, DepartamentoDto>()
            .ForMember(dest => dest.EmpresaNome, opt => opt.MapFrom(src => src.Empresa.Nome))
            .ForMember(dest => dest.TotalUsuarios, opt => opt.MapFrom(src => src.Usuarios.Count));

        // CheckinDiario mappings
        CreateMap<CheckinDiario, CheckinDiarioDto>()
            .ForMember(dest => dest.UsuarioNome, opt => opt.MapFrom(src => src.Usuario.Nome));

        CreateMap<CreateCheckinRequest, CheckinDiario>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UsuarioId, opt => opt.Ignore())
            .ForMember(dest => dest.DataCheckin, opt => opt.MapFrom(src => src.DataCheckin ?? DateTime.UtcNow))
            .ForMember(dest => dest.ScoreBemEstar, opt => opt.Ignore());

        // MetricaSaude mappings
        CreateMap<MetricaSaude, MetricaSaudeDto>();
        CreateMap<CreateMetricaSaudeRequest, MetricaSaude>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UsuarioId, opt => opt.Ignore())
            .ForMember(dest => dest.DataRegistro, opt => opt.MapFrom(src => DateTime.UtcNow));

        // AlertaBurnout mappings
        CreateMap<AlertaBurnout, AlertaBurnoutDto>()
            .ForMember(dest => dest.UsuarioNome, opt => opt.MapFrom(src => src.Usuario.Nome));
    }
}

public class DepartamentoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int EmpresaId { get; set; }
    public string EmpresaNome { get; set; } = string.Empty;
    public int TotalUsuarios { get; set; }
}

public class MetricaSaudeDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public DateTime DataRegistro { get; set; }
    public int? QualidadeSono { get; set; }
    public int? MinutosAtividadeFisica { get; set; }
    public decimal? LitrosAgua { get; set; }
    public int? FrequenciaCardiaca { get; set; }
    public int? PassosDiarios { get; set; }
    public decimal? PesoKg { get; set; }
}

public class CreateMetricaSaudeRequest
{
    public int? QualidadeSono { get; set; }
    public int? MinutosAtividadeFisica { get; set; }
    public decimal? LitrosAgua { get; set; }
    public int? FrequenciaCardiaca { get; set; }
    public int? PassosDiarios { get; set; }
    public decimal? PesoKg { get; set; }
}

