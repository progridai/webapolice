using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Clientes.Application.Ports;
using WebApolice.Modulos.Clientes.Application.UseCases.ConsultarCliente;
using WebApolice.Modulos.Clientes.Application.UseCases.ListarClientes;
using WebApolice.Modulos.Clientes.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Clientes.Infrastructure.Persistence.Queries;

internal sealed class ClientesQueries : IClientesQueries
{
    private readonly ClientesDbContext _dbContext;

    public ClientesQueries(ClientesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(ClienteListagemItemResult[] Itens, int TotalItens, int TotalPaginas)> ListarPaginadoAsync(
        int pagina, 
        int tamanhoPagina, 
        string? nome, 
        string? documento, 
        int? statusId, 
        string? ordenarPor, 
        string? direcao, 
        CancellationToken cancellationToken)
    {
        var query = from c in _dbContext.Clientes.AsNoTracking()
                    join p in _dbContext.Pessoas.AsNoTracking() on c.PessoaId equals p.Id
                    join s in _dbContext.Status.AsNoTracking() on c.StatusId equals s.Id
                    where c.DeletedAt == null
                    select new { c, p, s };

        if (!string.IsNullOrWhiteSpace(nome))
        {
            var nomeNormalizado = nome.Trim().ToUpperInvariant();
            query = query.Where(x => x.p.NomeNormalizado != null && x.p.NomeNormalizado.Contains(nomeNormalizado));
        }

        if (!string.IsNullOrWhiteSpace(documento))
        {
            var docLimpo = new string(documento.Where(char.IsDigit).ToArray());
            query = query.Where(x => x.p.DocumentoPrincipalLimpo == docLimpo);
        }

        if (statusId.HasValue)
        {
            query = query.Where(x => x.c.StatusId == statusId.Value);
        }

        // Ordenação
        var desc = direcao?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;
        
        query = ordenarPor?.ToLowerInvariant() switch
        {
            "nome" => desc ? query.OrderByDescending(x => x.p.Nome).ThenBy(x => x.c.Id) : query.OrderBy(x => x.p.Nome).ThenBy(x => x.c.Id),
            "documento" => desc ? query.OrderByDescending(x => x.p.DocumentoPrincipal).ThenBy(x => x.c.Id) : query.OrderBy(x => x.p.DocumentoPrincipal).ThenBy(x => x.c.Id),
            "datacadastro" => desc ? query.OrderByDescending(x => x.c.CreatedAt).ThenBy(x => x.c.Id) : query.OrderBy(x => x.c.CreatedAt).ThenBy(x => x.c.Id),
            _ => desc ? query.OrderByDescending(x => x.c.CreatedAt).ThenBy(x => x.c.Id) : query.OrderBy(x => x.c.CreatedAt).ThenBy(x => x.c.Id)
        };

        var totalItens = await query.CountAsync(cancellationToken);
        var totalPaginas = (int)Math.Ceiling(totalItens / (double)tamanhoPagina);

        var skip = (pagina - 1) * tamanhoPagina;
        var resultadosBrutos = await query
            .Skip(skip)
            .Take(tamanhoPagina)
            .Select(x => new
            {
                Id = x.c.PublicId,
                Nome = x.p.Nome,
                Documento = x.p.DocumentoPrincipal,
                TipoPessoa = x.p.TipoPessoa,
                Status = x.s.Nome,
                CreatedAt = x.c.CreatedAt
            })
            .ToArrayAsync(cancellationToken);

        // Mascaramento em memória
        var itens = resultadosBrutos.Select(r => new ClienteListagemItemResult(
            r.Id,
            r.Nome,
            MascararDocumento(r.Documento, r.TipoPessoa),
            r.Status,
            r.CreatedAt
        )).ToArray();

        return (itens, totalItens, totalPaginas);
    }

    private static string MascararDocumento(string? documento, short tipoPessoa)
    {
        if (string.IsNullOrWhiteSpace(documento)) return "Não informado";
        
        var limpo = new string(documento.Where(char.IsDigit).ToArray());
        
        // 1 = Pessoa Física (CPF), 2 = Pessoa Jurídica (CNPJ)
        if (tipoPessoa == 1 && limpo.Length == 11)
        {
            return $"***.***.{limpo.Substring(6, 3)}-{limpo.Substring(9, 2)}";
        }
        if (tipoPessoa == 2 && limpo.Length == 14)
        {
            return $"***.***.{limpo.Substring(5, 3)}/{limpo.Substring(8, 4)}-{limpo.Substring(12, 2)}";
        }

        return "Documento Inválido";
    }

    public async Task<ConsultarClienteResult?> ObterDetalheAsync(Guid id, CancellationToken cancellationToken)
    {
        // 1. Consulta Base (Cliente, Pessoa, Status)
        var baseInfo = await (
            from c in _dbContext.Clientes.AsNoTracking()
            join p in _dbContext.Pessoas.AsNoTracking() on c.PessoaId equals p.Id
            join s in _dbContext.Status.AsNoTracking() on c.StatusId equals s.Id
            where c.PublicId == id && c.DeletedAt == null && p.DeletedAt == null
            select new { c, p, s }
        ).FirstOrDefaultAsync(cancellationToken);

        if (baseInfo == null) return null;

        var pessoaId = baseInfo.p.Id;
        var clienteId = baseInfo.c.Id;

        // 2. Contatos
        var contatosRaw = await _dbContext.Contatos.AsNoTracking()
            .Where(x => x.PessoaId == pessoaId && x.Ativo)
            .OrderByDescending(x => x.Principal)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var contatos = contatosRaw.Select(c => new ClienteContatoResponse(
            c.TipoContato, c.Valor, c.Principal, c.Ativo
        )).ToList();

        // 3. Endereços
        var enderecosRaw = await _dbContext.Enderecos.AsNoTracking()
            .Where(x => x.PessoaId == pessoaId && x.Ativo)
            .OrderByDescending(x => x.Principal)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
            
        var enderecos = enderecosRaw.Select(e => new ClienteEnderecoResponse(
            e.TipoEndereco, e.Cep ?? "", e.Logradouro ?? "", e.Numero ?? "", e.Complemento ?? "", e.Bairro ?? "", "", e.Uf ?? "", e.Principal, e.Ativo
        )).ToList();

        // 4. Vínculos Resolvendo Nomes
        var vinculosRaw = await (
            from v in _dbContext.Vinculos.AsNoTracking()
            where v.ClienteId == clienteId && v.Ativo
            select new { v } // Simplify, we will join in memory or do left joins if needed. 
            // Wait, EF Core handles left joins well with multiple joins, but let's do it cleanly:
        ).ToListAsync(cancellationToken);
        
        var estipulanteIds = vinculosRaw.Where(v => v.v.EstipulanteId.HasValue).Select(v => v.v.EstipulanteId!.Value).Distinct().ToList();
        var estipulantes = await _dbContext.Estipulantes.AsNoTracking().Where(e => estipulanteIds.Contains(e.Id)).ToDictionaryAsync(e => e.Id, e => e.Nome, cancellationToken);
        
        var grupoIds = vinculosRaw.Where(v => v.v.GrupoId.HasValue).Select(v => v.v.GrupoId!.Value).Distinct().ToList();
        var grupos = await _dbContext.Grupos.AsNoTracking().Where(g => grupoIds.Contains(g.Id)).ToDictionaryAsync(g => g.Id, g => g.Nome, cancellationToken);

        var vinculos = vinculosRaw.Select(v => new ClienteVinculoResponse(
            v.v.Matricula ?? "",
            v.v.Ativo,
            v.v.EstipulanteId.HasValue && estipulantes.ContainsKey(v.v.EstipulanteId.Value) ? estipulantes[v.v.EstipulanteId.Value] : "",
            "", // Subestipulante (left for later)
            v.v.GrupoId.HasValue && grupos.ContainsKey(v.v.GrupoId.Value) ? grupos[v.v.GrupoId.Value] : "",
            "",
            ""
        )).ToList();

        // 5. Dependentes
        var dependentesRaw = await _dbContext.Dependentes.AsNoTracking()
            .Where(d => d.ClienteId == clienteId)
            .OrderBy(d => d.Id)
            .ToListAsync(cancellationToken);

        var dependentes = dependentesRaw.Select(d => new ClienteDependenteResponse(
            d.Nome,
            d.TipoRelacao,
            MascararDocumento(d.Cpf, 1), // Assuming CPF for dependentes
            d.DataNascimento
        )).ToList();

        return new ConsultarClienteResult(
            baseInfo.c.PublicId,
            baseInfo.p.Nome,
            MascararDocumento(baseInfo.p.DocumentoPrincipal, baseInfo.p.TipoPessoa),
            new ClienteStatusResponse(baseInfo.s.Codigo, baseInfo.s.Nome),
            baseInfo.p.DataNascimento,
            baseInfo.c.Falecido,
            baseInfo.c.DataObito,
            contatos,
            enderecos,
            vinculos,
            dependentes
        );
    }
}
