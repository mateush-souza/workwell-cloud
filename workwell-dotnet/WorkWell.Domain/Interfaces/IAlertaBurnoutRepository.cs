using WorkWell.Domain.Entities;
using WorkWell.Domain.Enums;

namespace WorkWell.Domain.Interfaces;

public interface IAlertaBurnoutRepository : IRepository<AlertaBurnout>
{
    Task<IEnumerable<AlertaBurnout>> GetByUsuarioAsync(int usuarioId, bool? lido = null);
    Task<IEnumerable<AlertaBurnout>> GetByNivelRiscoAsync(NivelRisco nivelRisco);
    Task<IEnumerable<AlertaBurnout>> GetNaoLidosAsync();
}

