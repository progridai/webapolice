using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Estipulantes.Infrastructure.Persistence.Models;

public class PessoaModel
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public short TipoPessoa { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? DocumentoPrincipal { get; set; }
    public string? DocumentoPrincipalLimpo { get; set; }
    public bool DocumentoValido { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    
    public ICollection<EstipulanteModel> Estipulantes { get; set; } = new List<EstipulanteModel>();
}
