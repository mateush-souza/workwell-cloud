namespace WorkWell.Application.DTOs;

public class EmpresaDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string? Setor { get; set; }
    public DateTime DataCadastro { get; set; }
    public int TotalUsuarios { get; set; }
    public int TotalDepartamentos { get; set; }
}

public class CreateEmpresaRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string? Setor { get; set; }
}

public class UpdateEmpresaRequest
{
    public string? Nome { get; set; }
    public string? Setor { get; set; }
}

