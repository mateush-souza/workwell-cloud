using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkWell.Application.Services;

namespace WorkWell.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces("application/json")]
public class AIAssistantController : ControllerBase
{
    private readonly IGeminiAIService _geminiService;
    private readonly ILogger<AIAssistantController> _logger;

    public AIAssistantController(IGeminiAIService geminiService, ILogger<AIAssistantController> logger)
    {
        _geminiService = geminiService;
        _logger = logger;
    }

    /// <summary>
    /// Chat com assistente de IA para suporte emocional
    /// </summary>
    [HttpPost("chat")]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var response = await _geminiService.ChatWithUserAsync(request.Message, request.History);

            _logger.LogInformation("AI chat interaction for user {UserId}", userId);

            return Ok(new ChatResponse
            {
                Message = response,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AI chat");
            return BadRequest(new { message = "Erro ao processar mensagem" });
        }
    }

    /// <summary>
    /// Gera recomendações personalizadas de bem-estar
    /// </summary>
    [HttpPost("recommendations")]
    [ProducesResponseType(typeof(RecommendationsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecommendations([FromBody] RecommendationsRequest request)
    {
        try
        {
            var recommendations = await _geminiService.GenerateWellbeingRecommendationsAsync(request.Context);

            return Ok(new RecommendationsResponse
            {
                Recommendations = recommendations,
                GeneratedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recommendations");
            return BadRequest(new { message = "Erro ao gerar recomendações" });
        }
    }

    /// <summary>
    /// Analisa sentimento de um texto
    /// </summary>
    [HttpPost("analyze-sentiment")]
    [ProducesResponseType(typeof(SentimentResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> AnalyzeSentiment([FromBody] SentimentRequest request)
    {
        try
        {
            var sentiment = await _geminiService.AnalyzeSentimentAsync(request.Text);

            return Ok(new SentimentResponse
            {
                Sentiment = sentiment,
                Text = request.Text
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing sentiment");
            return BadRequest(new { message = "Erro ao analisar sentimento" });
        }
    }
}

// DTOs for AI Assistant
public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
    public List<ChatHistoryItem> History { get; set; } = new();
}

public class ChatResponse
{
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class RecommendationsRequest
{
    public string Context { get; set; } = string.Empty;
}

public class RecommendationsResponse
{
    public string Recommendations { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}

public class SentimentRequest
{
    public string Text { get; set; } = string.Empty;
}

public class SentimentResponse
{
    public string Sentiment { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

