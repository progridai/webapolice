using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Estipulantes.Application.Ports;
using WebApolice.Modulos.Estipulantes.Application.UseCases.ConsultarEstipulante;
using WebApolice.Modulos.Estipulantes.Application.UseCases.ConsultarEstipulanteConfiguracao;

namespace WebApolice.Modulos.Estipulantes.Infrastructure.Persistence.Queries;

public class EstipulantesQueries : IEstipulantesQueries
{
    private readonly EstipulantesDbContext _dbContext;

    public EstipulantesQueries(EstipulantesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(IEnumerable<EstipulanteDetalheResult> itens, int totalItens, int totalPaginas)> ListarPaginadoAsync(
        int pagina, 
        int tamanhoPagina, 
        string? nome, 
        string? cnpj, 
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Estipulantes.AsNoTracking().Where(e => e.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(nome))
            query = query.Where(e => e.Nome.Contains(nome) || (e.NomeFormatado != null && e.NomeFormatado.Contains(nome)));

        if (!string.IsNullOrWhiteSpace(cnpj))
            query = query.Where(e => e.Cnpj == cnpj || e.CnpjLimpo == cnpj);

        var totalItens = await query.CountAsync(cancellationToken);
        var totalPaginas = (int)Math.Ceiling(totalItens / (double)tamanhoPagina);

        var itens = await query
            .OrderBy(e => e.Nome)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(e => new EstipulanteDetalheResult
            {
                PublicId = e.PublicId,
                RazaoSocial = e.Nome,
                NomeFantasia = e.NomeFormatado,
                Codigo = e.Codigo,
                Cnpj = e.Cnpj,
                CnpjLimpo = e.CnpjLimpo,
                Ativo = e.Ativo,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return (itens, totalItens, totalPaginas);
    }

    public async Task<EstipulanteDetalheResult?> ObterPorPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var estipulante = await _dbContext.Estipulantes
            .AsNoTracking()
            .Where(e => e.PublicId == publicId && e.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);

        if (estipulante == null) return null;

        var result = new EstipulanteDetalheResult
        {
            PublicId = estipulante.PublicId,
            RazaoSocial = estipulante.Nome,
            NomeFantasia = estipulante.NomeFormatado,
            Codigo = estipulante.Codigo,
            Cnpj = estipulante.Cnpj,
            CnpjLimpo = estipulante.CnpjLimpo,
            Ativo = estipulante.Ativo,
            CreatedAt = estipulante.CreatedAt,
            UpdatedAt = estipulante.UpdatedAt,
            GrupoId = estipulante.GrupoId,
            Observacao = estipulante.Observacao
        };

        if (estipulante.SeguradoraId.HasValue)
        {
            var seguradoraId = await _dbContext.Seguradoras
                .AsNoTracking()
                .Where(s => s.Id == estipulante.SeguradoraId.Value)
                .Select(s => s.PublicId)
                .FirstOrDefaultAsync(cancellationToken);
            
            if (seguradoraId != Guid.Empty)
                result.SeguradoraPublicId = seguradoraId;
        }

        if (estipulante.PessoaId.HasValue)
        {
            var endereco = await _dbContext.Enderecos
                .AsNoTracking()
                .Where(e => e.PessoaId == estipulante.PessoaId.Value && e.Ativo && e.Principal)
                .FirstOrDefaultAsync(cancellationToken);

            if (endereco != null)
            {
                result.Endereco = new EstipulanteDetalheResult.EnderecoDetalheResult
                {
                    Cep = endereco.Cep,
                    Logradouro = endereco.Logradouro,
                    Numero = endereco.Numero,
                    Complemento = endereco.Complemento,
                    Bairro = endereco.Bairro,
                    CidadeId = endereco.CidadeId,
                    Uf = endereco.Uf
                };
            }

            var contatosDb = await _dbContext.Contatos
                .AsNoTracking()
                .Where(c => c.PessoaId == estipulante.PessoaId.Value && c.Ativo)
                .ToListAsync(cancellationToken);

            if (contatosDb.Any())
            {
                result.Contatos = contatosDb.Select(c => new EstipulanteDetalheResult.ContatoDetalheResult
                {
                    TipoContato = c.TipoContato,
                    Valor = c.Valor,
                    Principal = c.Principal
                }).ToList();
            }

            var contatosInstDb = await _dbContext.ContatosInstitucionais
                .AsNoTracking()
                .Where(c => c.PessoaId == estipulante.PessoaId.Value && c.Ativo)
                .ToListAsync(cancellationToken);

            if (contatosInstDb.Any())
            {
                result.ContatosInstitucionais = contatosInstDb.Select(c => new EstipulanteDetalheResult.ContatoInstitucionalDetalheResult
                {
                    Nome = c.Nome,
                    Departamento = c.Departamento,
                    Email = c.Email,
                    Telefone = c.Telefone,
                    Ramal = c.Ramal
                }).ToList();
            }
        }

        return result;
    }

    public async Task<object?> ObterConfiguracaoPorPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
    {
        return await _dbContext.Configuracoes
            .AsNoTracking()
            .Where(c => c.Estipulante.PublicId == publicId && c.Estipulante.DeletedAt == null)
            .Select(c => new EstipulanteConfiguracaoResult
            {
                PermitePropostas = c.PermitePropostas,
                ControlaComissao = c.ControlaComissao,
                DataInicioVigencia = c.DataInicioVigencia,
                DataFimVigencia = c.DataFimVigencia,
                DataAniversario = c.DataAniversario,
                DataUltimoReajuste = c.DataUltimoReajuste,
                DataBaseReajuste = c.DataBaseReajuste,
                DataLimiteReajuste = c.DataLimiteReajuste,
                DiasAvisoReajuste = c.DiasAvisoReajuste,
                Carencia = c.Carencia,
                AdesaoPor = c.AdesaoPor,
                Custeio = c.Custeio,
                Adesao = c.Adesao,
                FaixaEtariaInicio = c.FaixaEtariaInicio,
                FaixaEtariaFim = c.FaixaEtariaFim
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
