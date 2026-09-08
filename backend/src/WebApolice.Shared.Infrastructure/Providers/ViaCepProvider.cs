using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebApolice.SharedKernel.Application.Models;
using WebApolice.SharedKernel.Application.Ports;

namespace WebApolice.Shared.Infrastructure.Providers;

public class ViaCepProvider : ICEPProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ViaCepProvider> _logger;

    public ViaCepProvider(HttpClient httpClient, ILogger<ViaCepProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<EnderecoProviderDto?> ConsultarCEPAsync(string cep, CancellationToken cancellationToken)
    {
        try
        {
            // URL já contempla que o CEP está limpo de máscaras e validado com 8 dígitos pelo caller
            var response = await _httpClient.GetAsync($"https://viacep.com.br/ws/{cep}/json/", cancellationToken);
            
            response.EnsureSuccessStatusCode();

            var viaCepResult = await response.Content.ReadFromJsonAsync<ViaCepResponse>(cancellationToken: cancellationToken);

            if (viaCepResult == null || viaCepResult.Erro)
            {
                return null;
            }

            return new EnderecoProviderDto(
                Cep: viaCepResult.Cep.Replace("-", ""),
                Logradouro: viaCepResult.Logradouro,
                Bairro: viaCepResult.Bairro,
                Cidade: viaCepResult.Localidade,
                Uf: viaCepResult.Uf
            );
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro de integração com o ViaCEP ao consultar o CEP {Cep}", cep);
            throw new Exception("Falha ao comunicar com o serviço de CEP externo.", ex);
        }
    }

    private class ViaCepResponse
    {
        [JsonPropertyName("cep")]
        public string Cep { get; set; } = string.Empty;
        
        [JsonPropertyName("logradouro")]
        public string? Logradouro { get; set; }
        
        [JsonPropertyName("bairro")]
        public string? Bairro { get; set; }
        
        [JsonPropertyName("localidade")]
        public string Localidade { get; set; } = string.Empty;
        
        [JsonPropertyName("uf")]
        public string Uf { get; set; } = string.Empty;
        
        [JsonPropertyName("erro")]
        public bool Erro { get; set; }
    }
}
