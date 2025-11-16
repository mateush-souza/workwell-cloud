using WorkWell.Domain.Enums;

namespace WorkWell.Application.DTOs;

public class UsuarioDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int EmpresaId { get; set; }
    public string? EmpresaNome { get; set; }
    public int? DepartamentoId { get; set; }
    public string? DepartamentoNome { get; set; }
    public string? Cargo { get; set; }
    public UserRole Role { get; set; }
    public bool Ativo { get; set; }
    public DateTime? DataUltimoAcesso { get; set; }
    public DateTime DataCriacao { get; set; }
}

public class CreateUsuarioRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public int EmpresaId { get; set; }
    public int? DepartamentoId { get; set; }
    public string? Cargo { get; set; }
    public UserRole Role { get; set; }
}

public class UpdateUsuarioRequest
{
    public string? Nome { get; set; }
    public int? DepartamentoId { get; set; }
    public string? Cargo { get; set; }
    public bool? Ativo { get; set; }
}

