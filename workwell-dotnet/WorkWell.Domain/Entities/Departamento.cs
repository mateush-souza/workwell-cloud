using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkWell.Domain.Entities;

[Table("DEPARTAMENTOS")]
public class Departamento : BaseEntity
{
    [Column("NOME")]
    [Required]
    [MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Column("DESCRICAO")]
    [MaxLength(500)]
    public string? Descricao { get; set; }

    [Column("EMPRESA_ID")]
    [Required]
    public int EmpresaId { get; set; }

    // Navigation Properties
    [ForeignKey("EmpresaId")]
    public virtual Empresa Empresa { get; set; } = null!;
    
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}

