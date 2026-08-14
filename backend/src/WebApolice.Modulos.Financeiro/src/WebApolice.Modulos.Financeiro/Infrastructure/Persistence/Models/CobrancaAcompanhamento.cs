using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Financeiro.src.WebApolice.Modulos.Financeiro.Infrastructure.Persistence.Models;

public partial class CobrancaAcompanhamento
{
    public long Id { get; set; }

    public long? PessoaId { get; set; }

    public long? ClienteId { get; set; }

    public DateOnly? DataAcompanhamento { get; set; }

    public string? HoraOriginal { get; set; }

    public string? Contato { get; set; }

    public string? Descricao { get; set; }

    public int? UsuarioLegadoId { get; set; }

    public int LegadoId { get; set; }

    public int? LegadoClienteId { get; set; }

    public DateTime CreatedAt { get; set; }
}
