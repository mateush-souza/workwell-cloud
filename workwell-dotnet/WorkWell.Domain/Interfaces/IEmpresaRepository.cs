using WorkWell.Domain.Entities;

namespace WorkWell.Domain.Interfaces;

public interface IEmpresaRepository : IRepository<Empresa>
{
    Task<Empresa?> GetByCnpjAsync(string cnpj);
    Task<bool> CnpjExistsAsync(string cnpj);
}

