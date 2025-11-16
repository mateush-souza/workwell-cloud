using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WorkWell.Domain.Entities;

namespace WorkWell.Infrastructure.Data;

public class WorkWellDbContext : DbContext
{
    private IDbContextTransaction? _currentTransaction;

    public WorkWellDbContext(DbContextOptions<WorkWellDbContext> options) : base(options)
    {
    }

    public DbSet<Empresa> Empresas { get; set; } = null!;
    public DbSet<Departamento> Departamentos { get; set; } = null!;
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<CheckinDiario> CheckinsDiarios { get; set; } = null!;
    public DbSet<MetricaSaude> MetricasSaude { get; set; } = null!;
    public DbSet<AlertaBurnout> AlertasBurnout { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar convenção de nomenclatura Oracle (maiúsculas)
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Tabelas
            entity.SetTableName(entity.GetTableName()?.ToUpperInvariant());

            // Colunas
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(property.GetColumnName()?.ToUpperInvariant());
            }

            // Chaves
            foreach (var key in entity.GetKeys())
            {
                key.SetName(key.GetName()?.ToUpperInvariant());
            }

            // Índices
            foreach (var index in entity.GetIndexes())
            {
                index.SetDatabaseName(index.GetDatabaseName()?.ToUpperInvariant());
            }

            // Foreign Keys
            foreach (var foreignKey in entity.GetForeignKeys())
            {
                foreignKey.SetConstraintName(foreignKey.GetConstraintName()?.ToUpperInvariant());
            }
        }

        // Configurações específicas de entidades
        ConfigureEmpresa(modelBuilder);
        ConfigureDepartamento(modelBuilder);
        ConfigureUsuario(modelBuilder);
        ConfigureCheckinDiario(modelBuilder);
        ConfigureMetricaSaude(modelBuilder);
        ConfigureAlertaBurnout(modelBuilder);
    }

    private void ConfigureEmpresa(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasIndex(e => e.Cnpj).IsUnique();

            entity.HasMany(e => e.Departamentos)
                .WithOne(d => d.Empresa)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Usuarios)
                .WithOne(u => u.Empresa)
                .HasForeignKey(u => u.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureDepartamento(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Departamento>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasMany(d => d.Usuarios)
                .WithOne(u => u.Departamento)
                .HasForeignKey(u => u.DepartamentoId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void ConfigureUsuario(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasIndex(e => e.Email).IsUnique();

            entity.Property(e => e.Role)
                .HasConversion<int>();

            entity.HasMany(u => u.Checkins)
                .WithOne(c => c.Usuario)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.MetricasSaude)
                .WithOne(m => m.Usuario)
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.Alertas)
                .WithOne(a => a.Usuario)
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureCheckinDiario(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CheckinDiario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.HorasTrabalhadas)
                .HasPrecision(5, 2);

            entity.Property(e => e.HorasSono)
                .HasPrecision(5, 2);

            entity.Property(e => e.ScoreBemEstar)
                .HasPrecision(5, 2);

            entity.HasIndex(e => new { e.UsuarioId, e.DataCheckin });
        });
    }

    private void ConfigureMetricaSaude(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MetricaSaude>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.LitrosAgua)
                .HasPrecision(4, 2);

            entity.Property(e => e.PesoKg)
                .HasPrecision(6, 2);

            entity.HasIndex(e => new { e.UsuarioId, e.DataRegistro });
        });
    }

    private void ConfigureAlertaBurnout(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AlertaBurnout>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.NivelRisco)
                .HasConversion<int>();

            entity.Property(e => e.ScoreRisco)
                .HasPrecision(5, 2);

            entity.HasIndex(e => e.UsuarioId);
            entity.HasIndex(e => e.NivelRisco);
            entity.HasIndex(e => e.Lido);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.DataCriacao = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entity.DataAtualizacao = DateTime.UtcNow;
            }
        }
    }

    public async Task BeginTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            return;
        }

        _currentTransaction = await Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await SaveChangesAsync();
            await _currentTransaction?.CommitAsync()!;
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        try
        {
            await _currentTransaction?.RollbackAsync()!;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }
}

