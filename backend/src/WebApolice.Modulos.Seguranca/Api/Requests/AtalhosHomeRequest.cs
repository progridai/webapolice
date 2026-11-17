using System.ComponentModel.DataAnnotations;

namespace WebApolice.Modulos.Seguranca.Api.Requests;

public sealed record AtalhosHomeRequest([Required] string[] AtalhosHome);
