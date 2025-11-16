using Microsoft.ML;
using Microsoft.ML.Data;
using WorkWell.Application.DTOs;
using WorkWell.Domain.Entities;
using WorkWell.Domain.Enums;
using WorkWell.Domain.Interfaces;

namespace WorkWell.Application.Services;

public interface IBurnoutPredictionService
{
    Task<BurnoutPredictionResult> PredictBurnoutRiskAsync(int usuarioId);
    Task TrainModelAsync();
}

public class BurnoutPredictionService : IBurnoutPredictionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly MLContext _mlContext;
    private ITransformer? _trainedModel;

    public BurnoutPredictionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _mlContext = new MLContext(seed: 42);
    }

    public async Task<BurnoutPredictionResult> PredictBurnoutRiskAsync(int usuarioId)
    {
        // Buscar dados históricos do usuário (últimos 30 dias)
        var dataInicio = DateTime.UtcNow.AddDays(-30);
        var checkins = (await _unitOfWork.Checkins.GetByUsuarioAsync(usuarioId, dataInicio)).ToList();

        if (!checkins.Any())
        {
            return new BurnoutPredictionResult
            {
                UsuarioId = usuarioId,
                NivelRisco = NivelRisco.Baixo,
                ScoreRisco = 0,
                Descricao = "Dados insuficientes para análise",
                Recomendacoes = new List<string> { "Continue registrando seus check-ins diários para análise mais precisa" }
            };
        }

        // Calcular features do usuário
        var features = CalculateUserFeatures(checkins);

        // Fazer predição (usando regras simples como fallback se modelo não estiver treinado)
        var scoreRisco = CalculateRiskScore(features);
        var nivelRisco = DetermineRiskLevel(scoreRisco);
        var recomendacoes = GenerateRecommendations(features, nivelRisco);

        return new BurnoutPredictionResult
        {
            UsuarioId = usuarioId,
            NivelRisco = nivelRisco,
            ScoreRisco = scoreRisco,
            Descricao = GetRiskDescription(nivelRisco, scoreRisco),
            Recomendacoes = recomendacoes,
            FatoresRisco = new Dictionary<string, decimal>
            {
                { "Stress Médio", (decimal)features.AvgStress },
                { "Horas Trabalhadas Médias", (decimal)features.AvgHorasTrabalhadas },
                { "Qualidade do Sono", (decimal)features.AvgHorasSono },
                { "Score de Bem-Estar", (decimal)features.AvgWellbeingScore },
                { "Tendência de Piora", features.TrendWorsening ? 100 : 0 }
            }
        };
    }

    public async Task TrainModelAsync()
    {
        // Implementação simplificada de treinamento
        // Em produção, você carregaria dados históricos rotulados
        await Task.CompletedTask;
    }

    private UserFeatures CalculateUserFeatures(List<CheckinDiario> checkins)
    {
        var orderedCheckins = checkins.OrderBy(c => c.DataCheckin).ToList();

        return new UserFeatures
        {
            AvgStress = (float)checkins.Average(c => c.NivelStress),
            AvgHorasTrabalhadas = (float)checkins.Average(c => c.HorasTrabalhadas),
            AvgHorasSono = (float)checkins.Where(c => c.HorasSono.HasValue).Select(c => c.HorasSono!.Value).DefaultIfEmpty(7).Average(),
            AvgWellbeingScore = (float)checkins.Where(c => c.ScoreBemEstar.HasValue).Select(c => c.ScoreBemEstar!.Value).DefaultIfEmpty(50).Average(),
            TotalCheckins = checkins.Count,
            TrendWorsening = IsWorsening(orderedCheckins)
        };
    }

    private bool IsWorsening(List<CheckinDiario> orderedCheckins)
    {
        if (orderedCheckins.Count < 7) return false;

        var recentWeek = orderedCheckins.TakeLast(7).ToList();
        var previousWeek = orderedCheckins.SkipLast(7).TakeLast(7).ToList();

        if (!previousWeek.Any()) return false;

        var recentAvgStress = recentWeek.Average(c => c.NivelStress);
        var previousAvgStress = previousWeek.Average(c => c.NivelStress);

        return recentAvgStress > previousAvgStress + 1.5; // Aumento significativo no stress
    }

    private decimal CalculateRiskScore(UserFeatures features)
    {
        decimal score = 0;

        // Stress alto (0-35 pontos)
        if (features.AvgStress >= 8) score += 35;
        else if (features.AvgStress >= 7) score += 25;
        else if (features.AvgStress >= 6) score += 15;
        else if (features.AvgStress >= 5) score += 5;

        // Horas de trabalho excessivas (0-25 pontos)
        if (features.AvgHorasTrabalhadas >= 12) score += 25;
        else if (features.AvgHorasTrabalhadas >= 10) score += 18;
        else if (features.AvgHorasTrabalhadas >= 9) score += 10;

        // Sono insuficiente (0-20 pontos)
        if (features.AvgHorasSono < 5) score += 20;
        else if (features.AvgHorasSono < 6) score += 15;
        else if (features.AvgHorasSono < 7) score += 8;

        // Score de bem-estar baixo (0-15 pontos)
        if (features.AvgWellbeingScore < 40) score += 15;
        else if (features.AvgWellbeingScore < 60) score += 10;
        else if (features.AvgWellbeingScore < 70) score += 5;

        // Tendência de piora (0-5 pontos)
        if (features.TrendWorsening) score += 5;

        return Math.Min(100, score);
    }

    private NivelRisco DetermineRiskLevel(decimal score)
    {
        return score switch
        {
            >= 75 => NivelRisco.Critico,
            >= 50 => NivelRisco.Alto,
            >= 25 => NivelRisco.Moderado,
            _ => NivelRisco.Baixo
        };
    }

    private string GetRiskDescription(NivelRisco nivel, decimal score)
    {
        return nivel switch
        {
            NivelRisco.Critico => $"Risco crítico de burnout detectado (Score: {score:F1}/100). Ação imediata recomendada.",
            NivelRisco.Alto => $"Alto risco de burnout (Score: {score:F1}/100). Monitoramento próximo necessário.",
            NivelRisco.Moderado => $"Risco moderado de burnout (Score: {score:F1}/100). Atenção aos sinais de alerta.",
            _ => $"Risco baixo de burnout (Score: {score:F1}/100). Continue mantendo hábitos saudáveis."
        };
    }

    private List<string> GenerateRecommendations(UserFeatures features, NivelRisco nivel)
    {
        var recomendacoes = new List<string>();

        if (features.AvgStress >= 7)
        {
            recomendacoes.Add("Pratique técnicas de gerenciamento de stress como meditação e respiração profunda");
            recomendacoes.Add("Considere conversar com um profissional de saúde mental");
        }

        if (features.AvgHorasTrabalhadas >= 10)
        {
            recomendacoes.Add("Reduza suas horas de trabalho e estabeleça limites claros");
            recomendacoes.Add("Delegue tarefas quando possível e priorize o essencial");
        }

        if (features.AvgHorasSono < 7)
        {
            recomendacoes.Add("Melhore sua higiene do sono: estabeleça horários regulares e evite telas antes de dormir");
            recomendacoes.Add("Busque dormir pelo menos 7-8 horas por noite");
        }

        if (features.TrendWorsening)
        {
            recomendacoes.Add("Seus indicadores estão piorando. Tire alguns dias de folga se possível");
        }

        if (nivel == NivelRisco.Critico || nivel == NivelRisco.Alto)
        {
            recomendacoes.Add("Agende uma consulta com RH para discutir ajustes na carga de trabalho");
            recomendacoes.Add("Considere tirar férias ou licença médica se necessário");
        }

        if (!recomendacoes.Any())
        {
            recomendacoes.Add("Continue mantendo seus hábitos saudáveis atuais");
            recomendacoes.Add("Mantenha o equilíbrio entre trabalho e vida pessoal");
        }

        return recomendacoes;
    }

    private class UserFeatures
    {
        public float AvgStress { get; set; }
        public float AvgHorasTrabalhadas { get; set; }
        public float AvgHorasSono { get; set; }
        public float AvgWellbeingScore { get; set; }
        public int TotalCheckins { get; set; }
        public bool TrendWorsening { get; set; }
    }
}

