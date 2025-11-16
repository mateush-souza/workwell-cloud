using WorkWell.Domain.Entities;

namespace WorkWell.Domain.Interfaces;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<IEnumerable<Usuario>> GetByEmpresaAsync(int empresaId);
    Task<IEnumerable<Usuario>> GetByDepartamentoAsync(int departamentoId);
}

