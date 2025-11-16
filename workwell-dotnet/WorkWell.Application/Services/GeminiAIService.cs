using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace WorkWell.Application.Services;

public interface IGeminiAIService
{
    Task<string> GenerateWellbeingRecommendationsAsync(string userContext);
    Task<string> ChatWithUserAsync(string userMessage, List<ChatHistoryItem> history);
    Task<string> AnalyzeSentimentAsync(string text);
}

public class GeminiAIService : IGeminiAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models";

    public GeminiAIService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API Key not configured");
    }

    public async Task<string> GenerateWellbeingRecommendationsAsync(string userContext)
    {
        var prompt = $@"Você é um especialista em saúde mental e bem-estar corporativo. 
Com base no seguinte contexto do usuário, forneça recomendações personalizadas e práticas para melhorar o bem-estar:

{userContext}

Forneça 3-5 recomendações específicas, práticas e acionáveis. Seja empático e encorajador.";

        return await GenerateContentAsync(prompt);
    }

    public async Task<string> ChatWithUserAsync(string userMessage, List<ChatHistoryItem> history)
    {
        var systemPrompt = @"Você é um assistente de bem-estar emocional e mental chamado WellBot. 
Seu objetivo é fornecer suporte emocional, ouvir ativamente e sugerir estratégias de coping saudáveis.
Seja empático, não-julgador e encorajador. 
Se o usuário demonstrar sinais de crise, sugira buscar ajuda profissional imediatamente.
Nunca dê diagnósticos médicos ou psicológicos.";

        var conversationContext = string.Join("\n", history.Select(h => $"{h.Role}: {h.Content}"));
        var fullPrompt = $"{systemPrompt}\n\nHistórico da conversa:\n{conversationContext}\n\nUsuário: {userMessage}\n\nAssistente:";

        return await GenerateContentAsync(fullPrompt);
    }

    public async Task<string> AnalyzeSentimentAsync(string text)
    {
        var prompt = $@"Analise o sentimento do seguinte texto e classifique-o em uma das categorias: 
Positivo, Neutro, Negativo, Ansioso, Estressado, Feliz, Triste, Cansado.

Texto: ""{text}""

Responda apenas com a categoria mais apropriada.";

        return await GenerateContentAsync(prompt);
    }

    private async Task<string> GenerateContentAsync(string prompt)
    {
        try
        {
            var request = new GeminiRequest
            {
                Contents = new[]
                {
                    new Content
                    {
                        Parts = new[] { new Part { Text = prompt } }
                    }
                }
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{BaseUrl}/gemini-pro:generateContent?key={_apiKey}",
                request
            );

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
            return result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text 
                   ?? "Desculpe, não consegui gerar uma resposta no momento.";
        }
        catch (Exception ex)
        {
            return $"Erro ao comunicar com o serviço de IA: {ex.Message}";
        }
    }

    // Models for Gemini API
    private class GeminiRequest
    {
        [JsonPropertyName("contents")]
        public Content[] Contents { get; set; } = Array.Empty<Content>();
    }

    private class Content
    {
        [JsonPropertyName("parts")]
        public Part[] Parts { get; set; } = Array.Empty<Part>();
    }

    private class Part
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    private class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public Candidate[]? Candidates { get; set; }
    }

    private class Candidate
    {
        [JsonPropertyName("content")]
        public Content? Content { get; set; }
    }
}

public class ChatHistoryItem
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

