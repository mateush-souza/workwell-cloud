using Microsoft.EntityFrameworkCore;
using WorkWell.Domain.Entities;
using WorkWell.Domain.Interfaces;
using WorkWell.Infrastructure.Data;

namespace WorkWell.Infrastructure.Repositories;

public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(WorkWellDbContext context) : base(context)
    {
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .Include(u => u.Empresa)
            .Include(u => u.Departamento)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<Usuario>> GetByEmpresaAsync(int empresaId)
    {
        return await _dbSet
            .Include(u => u.Departamento)
            .Where(u => u.EmpresaId == empresaId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Usuario>> GetByDepartamentoAsync(int departamentoId)
    {
        return await _dbSet
            .Include(u => u.Departamento)
            .Where(u => u.DepartamentoId == departamentoId)
            .ToListAsync();
    }

    public override async Task<Usuario?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(u => u.Empresa)
            .Include(u => u.Departamento)
            .FirstOrDefaultAsync(u => u.Id == id);
    }
}

