namespace WorkWell.Application.DTOs;

public class CheckinDiarioDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string? UsuarioNome { get; set; }
    public DateTime DataCheckin { get; set; }
    public int NivelStress { get; set; }
    public decimal HorasTrabalhadas { get; set; }
    public decimal? HorasSono { get; set; }
    public string? Sentimento { get; set; }
    public string? Observacoes { get; set; }
    public decimal? ScoreBemEstar { get; set; }
}

public class CreateCheckinRequest
{
    public DateTime? DataCheckin { get; set; }
    public int NivelStress { get; set; }
    public decimal HorasTrabalhadas { get; set; }
    public decimal? HorasSono { get; set; }
    public string? Sentimento { get; set; }
    public string? Observacoes { get; set; }
}

public class CheckinStatisticsDto
{
    public decimal MediaStress { get; set; }
    public decimal MediaHorasTrabalhadas { get; set; }
    public decimal MediaHorasSono { get; set; }
    public decimal MediaScoreBemEstar { get; set; }
    public int TotalCheckins { get; set; }
    public Dictionary<string, int> SentimentosDistribuicao { get; set; } = new();
}

