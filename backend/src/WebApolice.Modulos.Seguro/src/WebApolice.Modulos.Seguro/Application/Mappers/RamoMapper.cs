using WebApolice.Modulos.Seguro.Application.DTOs;
using WebApolice.Modulos.Seguro.Domain.Modelos;

namespace WebApolice.Modulos.Seguro.Application.Mappers;

public static class RamoMapper
{
    public static RamoDto ToDto(this RamoModel entity)
    {
        return new RamoDto
        {
            PublicId = entity.PublicId,
            Codigo = entity.Codigo,
            Nome = entity.Nome,
            Descricao = entity.Descricao,
            Ativo = entity.Ativo,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
