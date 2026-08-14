using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Documentos.src.WebApolice.Modulos.Documentos.Infrastructure.Persistence.Models;

public partial class StorageProvider
{
    public short Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nome { get; set; } = null!;

    public string? Descricao { get; set; }

    public bool Ativo { get; set; }

    public virtual ICollection<ArquivoVersao> ArquivoVersaos { get; set; } = new List<ArquivoVersao>();

    public virtual ICollection<Arquivo> Arquivos { get; set; } = new List<Arquivo>();
}
