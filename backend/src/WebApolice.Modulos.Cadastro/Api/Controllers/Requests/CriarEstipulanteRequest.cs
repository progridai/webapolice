using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Cadastro.Api.Controllers.Requests;

public class CriarEstipulanteRequest
{
    public string RazaoSocial { get; set; } = string.Empty;
    public string? NomeFantasia { get; set; }
    public string Cnpj { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    
    // Usando long pois o cadastro.grupo usa id bigint legado (sem public_id mapeado ainda)
    public long? GrupoId { get; set; }
    
    public Guid? SeguradoraPublicId { get; set; }
    public string? Observacao { get; set; }
    
    public CriarEstipulanteEnderecoRequest? Endereco { get; set; }
    public System.Collections.Generic.List<CriarEstipulanteContatoRequest>? Contatos { get; set; }
    public System.Collections.Generic.List<CriarEstipulanteContatoInstitucionalRequest>? ContatosInstitucionais { get; set; }
    public CriarEstipulanteConfiguracaoRequest Configuracao { get; set; } = new();
}

public class CriarEstipulanteEnderecoRequest
{
    public string? Cep { get; set; }
    public string? Logradouro { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    
    // Usando long porque cidade usa id bigint sem public_id
    public long? CidadeId { get; set; }
    public string? Uf { get; set; }
}

public class CriarEstipulanteContatoRequest
{
    public string TipoContato { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public bool Principal { get; set; }
}

public class CriarEstipulanteContatoInstitucionalRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Departamento { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Ramal { get; set; }
}

public class CriarEstipulanteConfiguracaoRequest
{
    public DateOnly DataInicioVigencia { get; set; }
    public DateOnly? DataFimVigencia { get; set; }
    public int? Carencia { get; set; }
    public string? AdesaoPor { get; set; }
    public string? Custeio { get; set; }
    public string? Adesao { get; set; }
}
