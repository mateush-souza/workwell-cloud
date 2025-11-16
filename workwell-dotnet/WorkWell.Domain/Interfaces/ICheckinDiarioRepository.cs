using WorkWell.Domain.Entities;

namespace WorkWell.Domain.Interfaces;

public interface ICheckinDiarioRepository : IRepository<CheckinDiario>
{
    Task<IEnumerable<CheckinDiario>> GetByUsuarioAsync(int usuarioId, DateTime? dataInicio = null, DateTime? dataFim = null);
    Task<CheckinDiario?> GetByUsuarioAndDataAsync(int usuarioId, DateTime data);
    Task<IEnumerable<CheckinDiario>> GetByEmpresaAsync(int empresaId, DateTime? dataInicio = null, DateTime? dataFim = null);
}

