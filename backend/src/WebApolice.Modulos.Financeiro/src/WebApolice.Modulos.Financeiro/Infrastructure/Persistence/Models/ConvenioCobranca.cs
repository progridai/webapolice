using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Financeiro.src.WebApolice.Modulos.Financeiro.Infrastructure.Persistence.Models;

public partial class ConvenioCobranca
{
    public long Id { get; set; }

    public long? BancoId { get; set; }

    public string? Nome { get; set; }

    public string? Agencia { get; set; }

    public string? ContaCorrente { get; set; }

    public string? NomeEmpresa { get; set; }

    public string? CodigoEmpresa { get; set; }

    public int? NumeroArquivo { get; set; }

    public string? NomeInicialArquivo { get; set; }

    public string? ExtensaoArquivo { get; set; }

    public short? LayoutArquivo { get; set; }

    public string? LocalRemessaArquivo { get; set; }

    public string? LocalRetornoArquivo { get; set; }

    public bool? ComunicaVindi { get; set; }

    public string? Observacao { get; set; }

    public int? LegadoId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? InscricaoEstadual { get; set; }

    public string? EstEndereco { get; set; }

    public string? EstNumero { get; set; }

    public string? EstBairro { get; set; }

    public string? EstComplemento { get; set; }

    public string? EstCep { get; set; }

    public string? EstCidade { get; set; }

    public string? EstUf { get; set; }

    public string? EstNome { get; set; }

    public virtual ICollection<ContaCobranca> ContaCobrancas { get; set; } = new List<ContaCobranca>();

    public virtual ICollection<EstipulanteFaturamentoConfig> EstipulanteFaturamentoConfigs { get; set; } = new List<EstipulanteFaturamentoConfig>();

    public virtual ICollection<Titulo> Titulos { get; set; } = new List<Titulo>();
}
