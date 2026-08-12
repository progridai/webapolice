using System;

namespace WebApolice.Modulos.Estipulantes.Infrastructure.Persistence.Models;

public class PessoaEnderecoModel
{
    public long Id { get; set; }
    public long PessoaId { get; set; }
    public long? CidadeId { get; set; }
    public string TipoEndereco { get; set; } = "PRINCIPAL";
    public string? Cep { get; set; }
    public string? Logradouro { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Uf { get; set; }
    public bool Principal { get; set; } = false;
    public bool Ativo { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class PessoaContatoModel
{
    public long Id { get; set; }
    public long PessoaId { get; set; }
    public string TipoContato { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public string? ValorNormalizado { get; set; }
    public bool Principal { get; set; } = false;
    public bool Ativo { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class GrupoModel
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}

public class SeguradoraModel
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public string Nome { get; set; } = string.Empty;
}

public class CidadeModel
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}

// Modelos auxiliares para verificação de Pessoa Compartilhada
public class ClienteModel
{
    public long Id { get; set; }
    public long PessoaId { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}

public class SubestipulanteModel
{
    public long Id { get; set; }
    public long PessoaId { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}

public class CorretoraModel
{
    public long Id { get; set; }
    public long PessoaId { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}

public class AgenciadorModel
{
    public long Id { get; set; }
    public long PessoaId { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
