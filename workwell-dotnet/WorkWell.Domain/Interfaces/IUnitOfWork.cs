namespace WorkWell.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUsuarioRepository Usuarios { get; }
    IEmpresaRepository Empresas { get; }
    IRepository<Entities.Departamento> Departamentos { get; }
    ICheckinDiarioRepository Checkins { get; }
    IRepository<Entities.MetricaSaude> MetricasSaude { get; }
    IAlertaBurnoutRepository Alertas { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

