using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class DocumentoAnexoMigrationMap
{
    public long Id { get; set; }

    public int LegadoDocumentoId { get; set; }

    public long ArquivoId { get; set; }

    public string? TituloOriginal { get; set; }

    public string? TipoAnexoOriginal { get; set; }

    public string? ExtensaoOriginal { get; set; }

    public string? ArquivoOriginal { get; set; }

    public int? PkCliente { get; set; }

    public long? ClienteId { get; set; }

    public int? PkProposta { get; set; }

    public long? PropostaId { get; set; }

    public int? PkSinistro { get; set; }

    public long? SinistroId { get; set; }

    public int? PkEstipulante { get; set; }

    public long? EstipulanteId { get; set; }

    public int? PkProtocolo { get; set; }

    public string? CriterioMigracao { get; set; }

    public string? Observacao { get; set; }

    public DateTime CreatedAt { get; set; }
}
