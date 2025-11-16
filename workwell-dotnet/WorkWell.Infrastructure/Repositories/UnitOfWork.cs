using WorkWell.Domain.Entities;
using WorkWell.Domain.Interfaces;
using WorkWell.Infrastructure.Data;

namespace WorkWell.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly WorkWellDbContext _context;
    private IUsuarioRepository? _usuarios;
    private IEmpresaRepository? _empresas;
    private IRepository<Departamento>? _departamentos;
    private ICheckinDiarioRepository? _checkins;
    private IRepository<MetricaSaude>? _metricasSaude;
    private IAlertaBurnoutRepository? _alertas;

    public UnitOfWork(WorkWellDbContext context)
    {
        _context = context;
    }

    public IUsuarioRepository Usuarios =>
        _usuarios ??= new UsuarioRepository(_context);

    public IEmpresaRepository Empresas =>
        _empresas ??= new EmpresaRepository(_context);

    public IRepository<Departamento> Departamentos =>
        _departamentos ??= new Repository<Departamento>(_context);

    public ICheckinDiarioRepository Checkins =>
        _checkins ??= new CheckinDiarioRepository(_context);

    public IRepository<MetricaSaude> MetricasSaude =>
        _metricasSaude ??= new Repository<MetricaSaude>(_context);

    public IAlertaBurnoutRepository Alertas =>
        _alertas ??= new AlertaBurnoutRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        await _context.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        await _context.CommitTransactionAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        await _context.RollbackTransactionAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

