using System;

namespace WebApolice.Modulos.Seguranca.Domain.Auditoria;

public sealed class AuditoriaPermissao
{
    public long Id { get; private set; }
    public Guid PublicId { get; private set; }
    public long? UsuarioExecutorId { get; private set; }
    public string Acao { get; private set; } = null!;
    public string EntidadeTipo { get; private set; } = null!;
    public long EntidadeId { get; private set; }
    public long? UsuarioAfetadoId { get; private set; }
    public long? PerfilId { get; private set; }
    public long? PermissaoId { get; private set; }
    public string? DadosAnteriores { get; private set; }
    public string? DadosNovos { get; private set; }
    public string? Motivo { get; private set; }
    public string? IpOrigem { get; private set; }
    public string? UserAgent { get; private set; }
    public string? CorrelationId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Usuario? UsuarioExecutor { get; private set; }
    public Usuario? UsuarioAfetado { get; private set; }
    public Perfil? Perfil { get; private set; }
    public Permissao? Permissao { get; private set; }

    public AuditoriaPermissao(
        string acao,
        string entidadeTipo,
        long entidadeId,
        long? usuarioExecutorId = null,
        long? usuarioAfetadoId = null,
        long? perfilId = null,
        long? permissaoId = null,
        string? dadosAnteriores = null,
        string? dadosNovos = null)
    {
        Acao = acao;
        EntidadeTipo = entidadeTipo;
        EntidadeId = entidadeId;
        UsuarioExecutorId = usuarioExecutorId;
        UsuarioAfetadoId = usuarioAfetadoId;
        PerfilId = perfilId;
        PermissaoId = permissaoId;
        DadosAnteriores = dadosAnteriores;
        DadosNovos = dadosNovos;
        CreatedAt = DateTime.UtcNow;
    }

    private AuditoriaPermissao() { }
}
