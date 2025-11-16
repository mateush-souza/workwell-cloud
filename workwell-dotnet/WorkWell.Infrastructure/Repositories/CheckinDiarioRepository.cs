using Microsoft.EntityFrameworkCore;
using WorkWell.Domain.Entities;
using WorkWell.Domain.Interfaces;
using WorkWell.Infrastructure.Data;

namespace WorkWell.Infrastructure.Repositories;

public class CheckinDiarioRepository : Repository<CheckinDiario>, ICheckinDiarioRepository
{
    public CheckinDiarioRepository(WorkWellDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CheckinDiario>> GetByUsuarioAsync(int usuarioId, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        var query = _dbSet.Where(c => c.UsuarioId == usuarioId);

        if (dataInicio.HasValue)
        {
            query = query.Where(c => c.DataCheckin >= dataInicio.Value);
        }

        if (dataFim.HasValue)
        {
            query = query.Where(c => c.DataCheckin <= dataFim.Value);
        }

        return await query
            .OrderByDescending(c => c.DataCheckin)
            .ToListAsync();
    }

    public async Task<CheckinDiario?> GetByUsuarioAndDataAsync(int usuarioId, DateTime data)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && c.DataCheckin.Date == data.Date);
    }

    public async Task<IEnumerable<CheckinDiario>> GetByEmpresaAsync(int empresaId, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        var query = _dbSet
            .Include(c => c.Usuario)
            .Where(c => c.Usuario.EmpresaId == empresaId);

        if (dataInicio.HasValue)
        {
            query = query.Where(c => c.DataCheckin >= dataInicio.Value);
        }

        if (dataFim.HasValue)
        {
            query = query.Where(c => c.DataCheckin <= dataFim.Value);
        }

        return await query
            .OrderByDescending(c => c.DataCheckin)
            .ToListAsync();
    }
}

