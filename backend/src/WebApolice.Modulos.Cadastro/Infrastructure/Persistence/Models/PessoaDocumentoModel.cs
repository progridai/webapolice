using System;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;

public class PessoaDocumentoModel
{
    public long Id { get; private set; }
    public long PessoaId { get; private set; }
    public string TipoDocumento { get; private set; }
    public string Numero { get; private set; }
    public string NumeroLimpo { get; private set; }
    public string? OrgaoEmissor { get; private set; }
    public DateTime? DataEmissao { get; private set; }
    public bool Principal { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // Construtor EF
    protected PessoaDocumentoModel() { }

    public PessoaDocumentoModel(long pessoaId, string tipoDocumento, string numero, string numeroLimpo, string? orgaoEmissor, DateTime? dataEmissao, bool principal)
    {
        PessoaId = pessoaId;
        TipoDocumento = tipoDocumento;
        Numero = numero;
        NumeroLimpo = numeroLimpo;
        OrgaoEmissor = orgaoEmissor;
        DataEmissao = dataEmissao;
        Principal = principal;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void AtualizarDocumento(string numero, string numeroLimpo, string? orgaoEmissor, DateTime? dataEmissao)
    {
        Numero = numero;
        NumeroLimpo = numeroLimpo;
        OrgaoEmissor = orgaoEmissor;
        DataEmissao = dataEmissao;
    }
}
