using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Documentos.src.WebApolice.Modulos.Documentos.Infrastructure.Persistence.Models;

public partial class Arquivo
{
    public long Id { get; set; }

    public Guid PublicId { get; set; }

    public short? StorageProviderId { get; set; }

    public string? Bucket { get; set; }

    public string? StorageKey { get; set; }

    public string? StoragePath { get; set; }

    public string? NomeOriginal { get; set; }

    public string? NomeArmazenado { get; set; }

    public string? Titulo { get; set; }

    public string? Descricao { get; set; }

    public string? Extensao { get; set; }

    public string? MimeType { get; set; }

    public long? TamanhoBytes { get; set; }

    public string? HashSha256 { get; set; }

    public DateOnly? DataDocumento { get; set; }

    public DateTime? DataUpload { get; set; }

    public string? HoraOriginal { get; set; }

    public string Origem { get; set; } = null!;

    public string? CaminhoLegado { get; set; }

    public string? ArquivoLegado { get; set; }

    public string Status { get; set; } = null!;

    public long? CriadoPorUsuarioId { get; set; }

    public int? CriadoPorUsuarioLegadoId { get; set; }

    public int? LegadoId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? ExtensaoOriginal { get; set; }

    public string? ExtensaoNormalizada { get; set; }

    public bool ExtensaoConfiavel { get; set; }

    public string MigracaoStatus { get; set; } = null!;

    public string? MigracaoErro { get; set; }

    public virtual ICollection<ArquivoAcessoLog> ArquivoAcessoLogs { get; set; } = new List<ArquivoAcessoLog>();

    public virtual ICollection<ArquivoVersao> ArquivoVersaos { get; set; } = new List<ArquivoVersao>();

    public virtual ICollection<ArquivoVinculo> ArquivoVinculos { get; set; } = new List<ArquivoVinculo>();

    public virtual StorageProvider? StorageProvider { get; set; }
}
