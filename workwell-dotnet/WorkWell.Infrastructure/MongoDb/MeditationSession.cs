using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WorkWell.Infrastructure.MongoDb;

public class MeditationSession
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("usuario_id")]
    public int UsuarioId { get; set; }

    [BsonElement("tipo")]
    public string Tipo { get; set; } = string.Empty; // "Mindfulness", "Respiração", etc.

    [BsonElement("duracao_minutos")]
    public int DuracaoMinutos { get; set; }

    [BsonElement("data_sessao")]
    public DateTime DataSessao { get; set; } = DateTime.UtcNow;

    [BsonElement("concluida")]
    public bool Concluida { get; set; }

    [BsonElement("feedback")]
    public string? Feedback { get; set; }

    [BsonElement("nivel_relaxamento_antes")]
    public int? NivelRelaxamentoAntes { get; set; }

    [BsonElement("nivel_relaxamento_depois")]
    public int? NivelRelaxamentoDepois { get; set; }
}

