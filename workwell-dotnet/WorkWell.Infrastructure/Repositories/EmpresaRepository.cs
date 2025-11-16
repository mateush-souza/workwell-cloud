using Microsoft.EntityFrameworkCore;
using WorkWell.Domain.Entities;
using WorkWell.Domain.Interfaces;
using WorkWell.Infrastructure.Data;

namespace WorkWell.Infrastructure.Repositories;

public class EmpresaRepository : Repository<Empresa>, IEmpresaRepository
{
    public EmpresaRepository(WorkWellDbContext context) : base(context)
    {
    }

    public async Task<Empresa?> GetByCnpjAsync(string cnpj)
    {
        return await _dbSet
            .Include(e => e.Departamentos)
            .FirstOrDefaultAsync(e => e.Cnpj == cnpj);
    }

    public async Task<bool> CnpjExistsAsync(string cnpj)
    {
        return await _dbSet.AnyAsync(e => e.Cnpj == cnpj);
    }

    public override async Task<Empresa?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(e => e.Departamentos)
            .FirstOrDefaultAsync(e => e.Id == id);
    }
}

