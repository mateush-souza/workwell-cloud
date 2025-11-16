using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WorkWell.Domain.Enums;

namespace WorkWell.Domain.Entities;

[Table("ALERTAS_BURNOUT")]
public class AlertaBurnout : BaseEntity
{
    [Column("USUARIO_ID")]
    [Required]
    public int UsuarioId { get; set; }

    [Column("DATA_ALERTA")]
    [Required]
    public DateTime DataAlerta { get; set; } = DateTime.UtcNow;

    [Column("NIVEL_RISCO")]
    [Required]
    public NivelRisco NivelRisco { get; set; }

    [Column("SCORE_RISCO")]
    [Range(0, 100)]
    public decimal ScoreRisco { get; set; }

    [Column("DESCRICAO")]
    [MaxLength(1000)]
    public string? Descricao { get; set; }

    [Column("RECOMENDACOES")]
    public string? Recomendacoes { get; set; }

    [Column("LIDO")]
    public bool Lido { get; set; } = false;

    [Column("DATA_LEITURA")]
    public DateTime? DataLeitura { get; set; }

    // Navigation Properties
    [ForeignKey("UsuarioId")]
    public virtual Usuario Usuario { get; set; } = null!;
}

