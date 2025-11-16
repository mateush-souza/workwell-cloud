using WorkWell.Domain.Enums;

namespace WorkWell.Application.DTOs;

public class AlertaBurnoutDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string? UsuarioNome { get; set; }
    public DateTime DataAlerta { get; set; }
    public NivelRisco NivelRisco { get; set; }
    public decimal ScoreRisco { get; set; }
    public string? Descricao { get; set; }
    public string? Recomendacoes { get; set; }
    public bool Lido { get; set; }
    public DateTime? DataLeitura { get; set; }
}

public class BurnoutPredictionResult
{
    public int UsuarioId { get; set; }
    public NivelRisco NivelRisco { get; set; }
    public decimal ScoreRisco { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public List<string> Recomendacoes { get; set; } = new();
    public Dictionary<string, decimal> FatoresRisco { get; set; } = new();
}

