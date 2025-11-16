using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkWell.Domain.Entities;

[Table("METRICAS_SAUDE")]
public class MetricaSaude : BaseEntity
{
    [Column("USUARIO_ID")]
    [Required]
    public int UsuarioId { get; set; }

    [Column("DATA_REGISTRO")]
    [Required]
    public DateTime DataRegistro { get; set; } = DateTime.UtcNow;

    [Column("QUALIDADE_SONO")]
    [Range(1, 10)]
    public int? QualidadeSono { get; set; }

    [Column("MINUTOS_ATIVIDADE_FISICA")]
    public int? MinutosAtividadeFisica { get; set; }

    [Column("LITROS_AGUA")]
    [Range(0, 10)]
    public decimal? LitrosAgua { get; set; }

    [Column("FREQUENCIA_CARDIACA")]
    public int? FrequenciaCardiaca { get; set; }

    [Column("PASSOS_DIARIOS")]
    public int? PassosDiarios { get; set; }

    [Column("PESO_KG")]
    public decimal? PesoKg { get; set; }

    // Navigation Properties
    [ForeignKey("UsuarioId")]
    public virtual Usuario Usuario { get; set; } = null!;
}

