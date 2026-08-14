using System;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarEstipulante;

public class EstipulanteDetalheResult
{
    public Guid PublicId { get; set; }
    public string RazaoSocial { get; set; } = string.Empty;
    public string? NomeFantasia { get; set; }
    public string? Codigo { get; set; }
    public string? Cnpj { get; set; }
    public string? CnpjLimpo { get; set; }
    public bool Ativo { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    
    public long? GrupoId { get; set; }
    public Guid? SeguradoraPublicId { get; set; }
    public string? Observacao { get; set; }
    public EnderecoDetalheResult? Endereco { get; set; }
    public IReadOnlyList<ContatoDetalheResult>? Contatos { get; set; }
    public IReadOnlyList<ContatoInstitucionalDetalheResult>? ContatosInstitucionais { get; set; }

    public class EnderecoDetalheResult
    {
        public string? Cep { get; set; }
        public string? Logradouro { get; set; }
        public string? Numero { get; set; }
        public string? Complemento { get; set; }
        public string? Bairro { get; set; }
        public long? CidadeId { get; set; }
        public string? Uf { get; set; }
    }

    public class ContatoDetalheResult
    {
        public string TipoContato { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
        public bool Principal { get; set; }
    }

    public class ContatoInstitucionalDetalheResult
    {
        public string Nome { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? Ramal { get; set; }
    }
}
