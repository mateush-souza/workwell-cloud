using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkWell.Domain.Entities;

[Table("EMPRESAS")]
public class Empresa : BaseEntity
{
    [Column("NOME")]
    [Required]
    [MaxLength(200)]
    public string Nome { get; set; } = string.Empty;

    [Column("CNPJ")]
    [Required]
    [MaxLength(14)]
    public string Cnpj { get; set; } = string.Empty;

    [Column("SETOR")]
    [MaxLength(100)]
    public string? Setor { get; set; }

    [Column("DATA_CADASTRO")]
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual ICollection<Departamento> Departamentos { get; set; } = new List<Departamento>();
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}

