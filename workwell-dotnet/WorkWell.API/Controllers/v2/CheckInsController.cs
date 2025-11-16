using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;
using System.Text.Json;
using WorkWell.API.Helpers;
using WorkWell.Application.DTOs;
using WorkWell.Application.Services;

namespace WorkWell.API.Controllers.v2;

/// <summary>
/// Versão 2 do CheckInsController com funcionalidades avançadas (cache, análise avançada)
/// </summary>
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces("application/json")]
public class CheckInsController : ControllerBase
{
    private readonly ICheckinService _checkinService;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CheckInsController> _logger;

    public CheckInsController(
        ICheckinService checkinService,
        IDistributedCache cache,
        ILogger<CheckInsController> logger)
    {
        _checkinService = checkinService;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// [V2] Lista check-ins do usuário autenticado com cache Redis
    /// </summary>
    [HttpGet("me", Name = "GetMyCheckinsV2")]
    [ProducesResponseType(typeof(PagedResponse<CheckinDiarioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyCheckins(
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool useCache = true)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var cacheKey = $"checkins_{userId}_{dataInicio}_{dataFim}_{pageNumber}_{pageSize}";

        PagedResponse<CheckinDiarioDto>? pagedResponse = null;

        // Try get from cache
        if (useCache)
        {
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                pagedResponse = JsonSerializer.Deserialize<PagedResponse<CheckinDiarioDto>>(cachedData);
                _logger.LogInformation("Check-ins retrieved from cache for user {UserId}", userId);
            }
        }

        if (pagedResponse == null)
        {
            var checkins = (await _checkinService.GetUserCheckinsAsync(userId, dataInicio, dataFim)).ToList();
            pagedResponse = PaginationHelper.CreatePagedResponse(checkins, pageNumber, pageSize, checkins.Count);
            pagedResponse.Links = HateoasHelper.GeneratePaginationLinks(
                Url,
                "GetMyCheckinsV2",
                pageNumber,
                pageSize,
                pagedResponse.TotalPages,
                new { dataInicio, dataFim });

            // Cache the result
            if (useCache)
            {
                var serialized = JsonSerializer.Serialize(pagedResponse);
                await _cache.SetStringAsync(
                    cacheKey,
                    serialized,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    });
                _logger.LogInformation("Check-ins cached for user {UserId}", userId);
            }
        }

        return Ok(pagedResponse);
    }

    /// <summary>
    /// [V2] Obtém análise avançada dos check-ins com insights adicionais
    /// </summary>
    [HttpGet("me/advanced-analytics", Name = "GetAdvancedAnalytics")]
    [ProducesResponseType(typeof(AdvancedAnalyticsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdvancedAnalytics(
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var statistics = await _checkinService.GetUserStatisticsAsync(userId, dataInicio, dataFim);

        var response = new AdvancedAnalyticsResponse
        {
            BasicStatistics = statistics,
            AnalysisPeriod = new
            {
                Start = dataInicio ?? DateTime.UtcNow.AddDays(-30),
                End = dataFim ?? DateTime.UtcNow
            },
            Insights = GenerateInsights(statistics),
            TrendAnalysis = AnalyzeTrends(statistics)
        };

        return Ok(response);
    }

    private List<string> GenerateInsights(CheckinStatisticsDto stats)
    {
        var insights = new List<string>();

        if (stats.MediaStress >= 7)
            insights.Add("⚠️ Seus níveis de stress estão elevados. Considere praticar técnicas de relaxamento.");

        if (stats.MediaHorasTrabalhadas >= 10)
            insights.Add("⏰ Você está trabalhando muitas horas. Tente estabelecer limites mais saudáveis.");

        if (stats.MediaHorasSono < 7)
            insights.Add("😴 Seu sono está abaixo do recomendado. Priorize descansar pelo menos 7-8 horas.");

        if (stats.MediaScoreBemEstar < 60)
            insights.Add("📉 Seu score de bem-estar está baixo. Considere buscar apoio profissional.");
        else if (stats.MediaScoreBemEstar >= 80)
            insights.Add("✨ Excelente! Seu bem-estar está ótimo. Continue assim!");

        return insights;
    }

    private object AnalyzeTrends(CheckinStatisticsDto stats)
    {
        return new
        {
            StressTrend = stats.MediaStress >= 7 ? "Crescente" : stats.MediaStress <= 4 ? "Decrescente" : "Estável",
            WorkloadTrend = stats.MediaHorasTrabalhadas >= 10 ? "Alto" : "Normal",
            SleepQuality = stats.MediaHorasSono >= 7 ? "Boa" : "Inadequada",
            OverallWellbeing = stats.MediaScoreBemEstar >= 70 ? "Positivo" : stats.MediaScoreBemEstar >= 50 ? "Moderado" : "Preocupante"
        };
    }
}

public class AdvancedAnalyticsResponse
{
    public CheckinStatisticsDto BasicStatistics { get; set; } = new();
    public object AnalysisPeriod { get; set; } = new();
    public List<string> Insights { get; set; } = new();
    public object TrendAnalysis { get; set; } = new();
}

