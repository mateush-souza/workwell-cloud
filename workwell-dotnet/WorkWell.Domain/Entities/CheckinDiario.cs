using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkWell.Domain.Entities;

[Table("CHECKINS_DIARIOS")]
public class CheckinDiario : BaseEntity
{
    [Column("USUARIO_ID")]
    [Required]
    public int UsuarioId { get; set; }

    [Column("DATA_CHECKIN")]
    [Required]
    public DateTime DataCheckin { get; set; } = DateTime.UtcNow;

    [Column("NIVEL_STRESS")]
    [Range(1, 10)]
    public int NivelStress { get; set; }

    [Column("HORAS_TRABALHADAS")]
    [Range(0, 24)]
    public decimal HorasTrabalhadas { get; set; }

    [Column("HORAS_SONO")]
    [Range(0, 24)]
    public decimal? HorasSono { get; set; }

    [Column("SENTIMENTO")]
    [MaxLength(50)]
    public string? Sentimento { get; set; }

    [Column("OBSERVACOES")]
    [MaxLength(1000)]
    public string? Observacoes { get; set; }

    [Column("SCORE_BEMESTAR")]
    public decimal? ScoreBemEstar { get; set; }

    // Navigation Properties
    [ForeignKey("UsuarioId")]
    public virtual Usuario Usuario { get; set; } = null!;
}

