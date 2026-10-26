using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Domain.Modelos;
using WebApolice.Modulos.Cadastro.Application.UseCases.Modulos;
using WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.ListarModulos;
using WebApolice.SharedKernel.Application.Models;

namespace WebApolice.Modulos.Cadastro.Application.Ports;

public interface IModulosCadastroRepository
{
    Task<ModuloModel?> ObterAsync(Guid publicId, CancellationToken cancellationToken);
    Task<bool> NomeExisteAsync(string nome, long? desconsiderarId, CancellationToken cancellationToken);
    Task<PagedResult<ModuloListDto>> ListarAsync(ListarModulosQuery request, CancellationToken cancellationToken);
    void Adicionar(ModuloModel modulo);
    void Atualizar(ModuloModel modulo);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
}
