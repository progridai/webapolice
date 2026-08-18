using System;

namespace WebApolice.Modulos.Cadastro.Api.Controllers.Requests;

public class CriarModuloRequest
{
    public string Nome { get; set; } = null!;
    public string? Descricao { get; set; }
}

public class AtualizarModuloRequest
{
    public string Nome { get; set; } = null!;
    public string? Descricao { get; set; }
    public bool Ativo { get; set; }
}
