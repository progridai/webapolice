using System;

namespace WebApolice.Modulos.Estipulantes.Api.Controllers.Requests;

public class AtualizarEstipulanteRequest
{
    public string RazaoSocial { get; set; } = string.Empty;
    public string NomeFantasia { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public long? GrupoId { get; set; }
    public Guid? SeguradoraPublicId { get; set; }
    public string? Observacao { get; set; }
    public AtualizarEstipulanteEnderecoRequest? Endereco { get; set; }
    public System.Collections.Generic.List<AtualizarEstipulanteContatoRequest>? Contatos { get; set; }
    public System.Collections.Generic.List<AtualizarEstipulanteContatoInstitucionalRequest>? ContatosInstitucionais { get; set; }
    public AtualizarEstipulanteConfiguracaoRequest Configuracao { get; set; } = new();
}

public class AtualizarEstipulanteConfiguracaoRequest
{
    public DateOnly DataInicioVigencia { get; set; }
    public DateOnly? DataFimVigencia { get; set; }
    public int? Carencia { get; set; }
    public string? AdesaoPor { get; set; }
    public string? Custeio { get; set; }
    public string? Adesao { get; set; }
}

public class AtualizarEstipulanteEnderecoRequest
{
    public string Cep { get; set; } = string.Empty;
    public string Logradouro { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public long? CidadeId { get; set; }
    public string Uf { get; set; } = string.Empty;
}

public class AtualizarEstipulanteContatoRequest
{
    public string TipoContato { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public bool Principal { get; set; }
}

public class AtualizarEstipulanteContatoInstitucionalRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Departamento { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Ramal { get; set; }
}
