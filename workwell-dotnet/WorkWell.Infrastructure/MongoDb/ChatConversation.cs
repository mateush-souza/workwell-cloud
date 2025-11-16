using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WorkWell.Infrastructure.MongoDb;

public class ChatConversation
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("usuario_id")]
    public int UsuarioId { get; set; }

    [BsonElement("mensagens")]
    public List<ChatMessage> Mensagens { get; set; } = new();

    [BsonElement("data_inicio")]
    public DateTime DataInicio { get; set; } = DateTime.UtcNow;

    [BsonElement("data_ultima_mensagem")]
    public DateTime DataUltimaMensagem { get; set; } = DateTime.UtcNow;

    [BsonElement("topico")]
    public string? Topico { get; set; }
}

public class ChatMessage
{
    [BsonElement("role")]
    public string Role { get; set; } = string.Empty; // "user" ou "assistant"

    [BsonElement("conteudo")]
    public string Conteudo { get; set; } = string.Empty;

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [BsonElement("sentimento")]
    public string? Sentimento { get; set; }
}

