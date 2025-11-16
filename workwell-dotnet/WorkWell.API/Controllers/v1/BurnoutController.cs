using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkWell.Application.DTOs;
using WorkWell.Application.Services;

namespace WorkWell.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces("application/json")]
public class BurnoutController : ControllerBase
{
    private readonly IBurnoutPredictionService _burnoutService;
    private readonly ILogger<BurnoutController> _logger;

    public BurnoutController(IBurnoutPredictionService burnoutService, ILogger<BurnoutController> logger)
    {
        _burnoutService = burnoutService;
        _logger = logger;
    }

    /// <summary>
    /// Analisa o risco de burnout do usuário autenticado usando ML.NET
    /// </summary>
    [HttpGet("predict/me", Name = "PredictMyBurnout")]
    [ProducesResponseType(typeof(BurnoutPredictionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PredictMyBurnout()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var prediction = await _burnoutService.PredictBurnoutRiskAsync(userId);

        _logger.LogInformation("Burnout prediction for user {UserId}: Risk level {RiskLevel}, Score {Score}",
            userId, prediction.NivelRisco, prediction.ScoreRisco);

        return Ok(prediction);
    }

    /// <summary>
    /// Analisa o risco de burnout de um usuário específico (Admin only)
    /// </summary>
    [HttpGet("predict/{usuarioId}", Name = "PredictUserBurnout")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(BurnoutPredictionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PredictUserBurnout(int usuarioId)
    {
        var prediction = await _burnoutService.PredictBurnoutRiskAsync(usuarioId);

        _logger.LogInformation("Burnout prediction for user {UserId} requested by admin: Risk level {RiskLevel}",
            usuarioId, prediction.NivelRisco);

        return Ok(prediction);
    }

    /// <summary>
    /// Retreina o modelo de predição de burnout (Admin only)
    /// </summary>
    [HttpPost("train-model")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> TrainModel()
    {
        await _burnoutService.TrainModelAsync();
        _logger.LogInformation("Burnout prediction model training initiated");

        return Ok(new { message = "Modelo retreinado com sucesso" });
    }
}

