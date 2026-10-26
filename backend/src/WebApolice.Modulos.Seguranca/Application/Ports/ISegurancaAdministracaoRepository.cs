using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguranca.Domain;
using WebApolice.Modulos.Seguranca.Domain.Auditoria;
using WebApolice.SharedKernel.Application.Persistence;

namespace WebApolice.Modulos.Seguranca.Application.Ports;

public interface ISegurancaAdministracaoRepository
{
    Task<List<Perfil>> ObterPerfisAsync(List<Guid> publicIds, bool somenteAtivos, CancellationToken ct);
    Task<List<Permissao>> ObterPermissoesAsync(List<Guid> publicIds, bool somenteAtivas, CancellationToken ct);
    Task<Usuario?> ObterUsuarioPorSubAsync(string? sub, CancellationToken ct);
    Task<Usuario?> ObterUsuarioComPerfisAsync(Guid publicId, CancellationToken ct);
    Task<Perfil?> ObterPerfilComPermissoesAsync(Guid publicId, CancellationToken ct);
    Task<Modulo?> ObterModuloAsync(Guid publicId, CancellationToken ct);
    Task<Recurso?> ObterRecursoAsync(Guid publicId, CancellationToken ct);
    Task<bool> UsuarioDuplicadoAsync(string username, string email, CancellationToken ct);
    Task<bool> CodigoPerfilExisteAsync(string codigo, CancellationToken ct);
    void AdicionarUsuario(Usuario usuario);
    void AdicionarPerfil(Perfil perfil);
    void AdicionarAuditoria(AuditoriaPermissao evento);
    Task SalvarAlteracoesAsync(CancellationToken ct);
    Task<ITransacaoPersistencia> BeginTransactionAsync(CancellationToken ct);
}
