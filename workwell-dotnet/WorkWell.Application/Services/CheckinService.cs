using AutoMapper;
using WorkWell.Application.DTOs;
using WorkWell.Domain.Entities;
using WorkWell.Domain.Interfaces;

namespace WorkWell.Application.Services;

public interface ICheckinService
{
    Task<CheckinDiarioDto> CreateCheckinAsync(int usuarioId, CreateCheckinRequest request);
    Task<CheckinDiarioDto?> GetCheckinByIdAsync(int id);
    Task<IEnumerable<CheckinDiarioDto>> GetUserCheckinsAsync(int usuarioId, DateTime? dataInicio = null, DateTime? dataFim = null);
    Task<CheckinStatisticsDto> GetUserStatisticsAsync(int usuarioId, DateTime? dataInicio = null, DateTime? dataFim = null);
}

public class CheckinService : ICheckinService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CheckinService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CheckinDiarioDto> CreateCheckinAsync(int usuarioId, CreateCheckinRequest request)
    {
        // Verificar se usuário existe
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(usuarioId);
        if (usuario == null)
        {
            throw new InvalidOperationException("Usuário não encontrado");
        }

        // Verificar se já existe checkin para o dia
        var dataCheckin = request.DataCheckin ?? DateTime.UtcNow;
        var existingCheckin = await _unitOfWork.Checkins.GetByUsuarioAndDataAsync(usuarioId, dataCheckin);
        if (existingCheckin != null)
        {
            throw new InvalidOperationException("Já existe um check-in para esta data");
        }

        var checkin = _mapper.Map<CheckinDiario>(request);
        checkin.UsuarioId = usuarioId;
        checkin.DataCheckin = dataCheckin;
        
        // Calcular score de bem-estar
        checkin.ScoreBemEstar = CalculateWellbeingScore(checkin);

        await _unitOfWork.Checkins.AddAsync(checkin);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<CheckinDiarioDto>(checkin);
        dto.UsuarioNome = usuario.Nome;

        return dto;
    }

    public async Task<CheckinDiarioDto?> GetCheckinByIdAsync(int id)
    {
        var checkin = await _unitOfWork.Checkins.GetByIdAsync(id);
        return checkin == null ? null : _mapper.Map<CheckinDiarioDto>(checkin);
    }

    public async Task<IEnumerable<CheckinDiarioDto>> GetUserCheckinsAsync(int usuarioId, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        var checkins = await _unitOfWork.Checkins.GetByUsuarioAsync(usuarioId, dataInicio, dataFim);
        return _mapper.Map<IEnumerable<CheckinDiarioDto>>(checkins);
    }

    public async Task<CheckinStatisticsDto> GetUserStatisticsAsync(int usuarioId, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        var checkins = (await _unitOfWork.Checkins.GetByUsuarioAsync(usuarioId, dataInicio, dataFim)).ToList();

        if (!checkins.Any())
        {
            return new CheckinStatisticsDto();
        }

        var stats = new CheckinStatisticsDto
        {
            TotalCheckins = checkins.Count,
            MediaStress = (decimal)checkins.Average(c => c.NivelStress),
            MediaHorasTrabalhadas = checkins.Average(c => c.HorasTrabalhadas),
            MediaHorasSono = checkins.Where(c => c.HorasSono.HasValue).Select(c => c.HorasSono!.Value).DefaultIfEmpty(0).Average(),
            MediaScoreBemEstar = checkins.Where(c => c.ScoreBemEstar.HasValue).Select(c => c.ScoreBemEstar!.Value).DefaultIfEmpty(0).Average(),
            SentimentosDistribuicao = checkins
                .Where(c => !string.IsNullOrEmpty(c.Sentimento))
                .GroupBy(c => c.Sentimento!)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return stats;
    }

    private decimal CalculateWellbeingScore(CheckinDiario checkin)
    {
        decimal score = 100;

        // Penalizar por alto stress (0-40 pontos de desconto)
        score -= (checkin.NivelStress - 1) * 4.44m; // Cada nível acima de 1 desconta ~4.44 pontos

        // Penalizar por horas excessivas de trabalho
        if (checkin.HorasTrabalhadas > 8)
        {
            score -= (checkin.HorasTrabalhadas - 8) * 2; // 2 pontos por hora extra
        }

        // Penalizar por sono insuficiente
        if (checkin.HorasSono.HasValue)
        {
            if (checkin.HorasSono < 6)
            {
                score -= (6 - checkin.HorasSono.Value) * 5; // 5 pontos por hora de sono faltante
            }
            else if (checkin.HorasSono > 10)
            {
                score -= (checkin.HorasSono.Value - 10) * 2; // Muito sono também pode ser problemático
            }
        }

        // Garantir que o score fique entre 0 e 100
        return Math.Max(0, Math.Min(100, score));
    }
}

