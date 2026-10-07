namespace WebApolice.Modulos.Seguro.Application.Ports;
public interface ISubgruposCadastro
{
    Task<Guid> SalvarAsync(Guid apolicePublicId,Guid? subgrupoPublicId,string nome,string? observacao,Guid? convenioPublicId,CancellationToken ct);
    Task InativarAsync(Guid apolicePublicId,Guid subgrupoPublicId,CancellationToken ct);
}
