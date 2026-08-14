using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Documentos.src.WebApolice.Modulos.Documentos.Infrastructure.Persistence.Models;

public partial class ArquivoVersao
{
    public long Id { get; set; }

    public long ArquivoId { get; set; }

    public int Versao { get; set; }

    public short? StorageProviderId { get; set; }

    public string? Bucket { get; set; }

    public string? StorageKey { get; set; }

    public string? StoragePath { get; set; }

    public string? NomeOriginal { get; set; }

    public string? Extensao { get; set; }

    public string? MimeType { get; set; }

    public long? TamanhoBytes { get; set; }

    public string? HashSha256 { get; set; }

    public string? Motivo { get; set; }

    public long? CriadoPorUsuarioId { get; set; }

    public int? CriadoPorUsuarioLegadoId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Arquivo Arquivo { get; set; } = null!;

    public virtual StorageProvider? StorageProvider { get; set; }
}
