using Microsoft.EntityFrameworkCore;
using WorkWell.Domain.Entities;
using WorkWell.Domain.Enums;
using WorkWell.Domain.Interfaces;
using WorkWell.Infrastructure.Data;

namespace WorkWell.Infrastructure.Repositories;

public class AlertaBurnoutRepository : Repository<AlertaBurnout>, IAlertaBurnoutRepository
{
    public AlertaBurnoutRepository(WorkWellDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AlertaBurnout>> GetByUsuarioAsync(int usuarioId, bool? lido = null)
    {
        var query = _dbSet.Where(a => a.UsuarioId == usuarioId);

        if (lido.HasValue)
        {
            query = query.Where(a => a.Lido == lido.Value);
        }

        return await query
            .OrderByDescending(a => a.DataAlerta)
            .ToListAsync();
    }

    public async Task<IEnumerable<AlertaBurnout>> GetByNivelRiscoAsync(NivelRisco nivelRisco)
    {
        return await _dbSet
            .Include(a => a.Usuario)
            .Where(a => a.NivelRisco == nivelRisco)
            .OrderByDescending(a => a.DataAlerta)
            .ToListAsync();
    }

    public async Task<IEnumerable<AlertaBurnout>> GetNaoLidosAsync()
    {
        return await _dbSet
            .Include(a => a.Usuario)
            .Where(a => !a.Lido)
            .OrderByDescending(a => a.ScoreRisco)
            .ThenByDescending(a => a.DataAlerta)
            .ToListAsync();
    }
}

