using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

public class ApoliceSubestipulanteModuloModel
{
    public long Id { get; set; }
    
    // A FK aponta para a tabela associativa de Subestipulante da Apólice
    public long ApoliceSubestipulanteId { get; set; }
    
    // Referência ao Catálogo Global (cadastro.modulo)
    public long ModuloId { get; set; }
    
    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public bool Ativo { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // Propriedades de Navegação (EF)
    public ApoliceSubestipulanteModel? ApoliceSubestipulante { get; set; }
    public ICollection<ApoliceVidaModel> Vidas { get; set; } = new List<ApoliceVidaModel>();
}
