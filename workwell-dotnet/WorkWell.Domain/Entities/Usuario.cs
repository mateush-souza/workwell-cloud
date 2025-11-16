using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WorkWell.Domain.Enums;

namespace WorkWell.Domain.Entities;

[Table("USUARIOS")]
public class Usuario : BaseEntity
{
    [Column("NOME")]
    [Required]
    [MaxLength(200)]
    public string Nome { get; set; } = string.Empty;

    [Column("EMAIL")]
    [Required]
    [MaxLength(200)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Column("SENHA_HASH")]
    [Required]
    public string SenhaHash { get; set; } = string.Empty;

    [Column("EMPRESA_ID")]
    [Required]
    public int EmpresaId { get; set; }

    [Column("DEPARTAMENTO_ID")]
    public int? DepartamentoId { get; set; }

    [Column("CARGO")]
    [MaxLength(100)]
    public string? Cargo { get; set; }

    [Column("ROLE")]
    [Required]
    public UserRole Role { get; set; } = UserRole.USER;

    [Column("ATIVO")]
    public bool Ativo { get; set; } = true;

    [Column("DATA_ULTIMO_ACESSO")]
    public DateTime? DataUltimoAcesso { get; set; }

    // Navigation Properties
    [ForeignKey("EmpresaId")]
    public virtual Empresa Empresa { get; set; } = null!;

    [ForeignKey("DepartamentoId")]
    public virtual Departamento? Departamento { get; set; }

    public virtual ICollection<CheckinDiario> Checkins { get; set; } = new List<CheckinDiario>();
    public virtual ICollection<MetricaSaude> MetricasSaude { get; set; } = new List<MetricaSaude>();
    public virtual ICollection<AlertaBurnout> Alertas { get; set; } = new List<AlertaBurnout>();
}

