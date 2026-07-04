using System;
using WebApolice.Modulos.Clientes.Domain.Exceptions;

namespace WebApolice.Modulos.Clientes.Domain;

public sealed class Cliente
{
    public long Id { get; private set; }
    public string Nome { get; private set; }
    public string Cpf { get; private set; }
    public DateOnly? DataNascimento { get; private set; }
    public string? Email { get; private set; }
    public string? Telefone { get; private set; }
    public StatusCliente Status { get; private set; }
    public DateTime DataCadastroUtc { get; private set; }
    public DateTime DataAtualizacaoUtc { get; private set; }
    public long? CodigoLegado { get; private set; }

    // Construtor para o EF Core
    private Cliente()
    {
        Nome = null!;
        Cpf = null!;
    }

    public Cliente(
        string nome,
        string cpf,
        DateOnly? dataNascimento,
        string? email,
        string? telefone,
        long? codigoLegado)
    {
        ValidarNome(nome);
        ValidarCpf(cpf);
        
        if (dataNascimento.HasValue && dataNascimento.Value > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ClienteInvalidoException("Data de nascimento não pode ser no futuro.");

        Nome = NormalizarNome(nome);
        Cpf = NormalizarCpfEValidar(cpf);
        DataNascimento = dataNascimento;
        Email = NormalizarEmail(email);
        Telefone = NormalizarTelefone(telefone);
        CodigoLegado = codigoLegado;
        
        Status = StatusCliente.Ativo;
        DataCadastroUtc = DateTime.UtcNow;
        DataAtualizacaoUtc = DataCadastroUtc;
    }

    public void Alterar(
        string nome,
        DateOnly? dataNascimento,
        string? email,
        string? telefone)
    {
        ValidarNome(nome);
        if (dataNascimento.HasValue && dataNascimento.Value > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ClienteInvalidoException("Data de nascimento não pode ser no futuro.");

        Nome = NormalizarNome(nome);
        DataNascimento = dataNascimento;
        Email = NormalizarEmail(email);
        Telefone = NormalizarTelefone(telefone);

        DataAtualizacaoUtc = DateTime.UtcNow;
    }

    public void Ativar()
    {
        if (Status == StatusCliente.Ativo) return;

        Status = StatusCliente.Ativo;
        DataAtualizacaoUtc = DateTime.UtcNow;
    }

    public void Inativar()
    {
        if (Status == StatusCliente.Inativo) return;

        Status = StatusCliente.Inativo;
        DataAtualizacaoUtc = DateTime.UtcNow;
    }

    private void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ClienteInvalidoException("Nome do cliente é obrigatório.");
            
        if (nome.Trim().Length < 2)
            throw new ClienteInvalidoException("Nome do cliente deve ter no mínimo 2 caracteres.");
            
        if (nome.Length > 150)
            throw new ClienteInvalidoException("Nome do cliente não pode exceder 150 caracteres.");
    }

    private void ValidarCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            throw new ClienteInvalidoException("CPF é obrigatório.");
    }

    private string NormalizarNome(string nome)
    {
        var nomeAparado = nome.Trim();
        // Reduz espaços repetidos internamente
        return string.Join(" ", nomeAparado.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
    }

    private string NormalizarEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return null!;
        var emailAparado = email.Trim();
        if (emailAparado.Length > 254)
            throw new ClienteInvalidoException("Email não pode exceder 254 caracteres.");

        if (!emailAparado.Contains("@") || emailAparado.StartsWith("@") || emailAparado.EndsWith("@") || emailAparado.Contains(" "))
            throw new ClienteInvalidoException("Email inválido.");

        return emailAparado;
    }

    private string NormalizarTelefone(string? telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone)) return null!;
        var fone = telefone.Trim();
        if (fone.Length > 20)
            throw new ClienteInvalidoException("Telefone não pode exceder 20 caracteres.");

        var apenasDigitos = new string(fone.Where(char.IsDigit).ToArray());
        if (apenasDigitos.Length < 8)
            throw new ClienteInvalidoException("Telefone inválido. Deve ter no mínimo 8 dígitos.");

        return fone;
    }

    private string NormalizarCpfEValidar(string cpf)
    {
        // Remove caracteres não numéricos
        var apenasDigitos = new string(cpf.Where(char.IsDigit).ToArray());
        
        if (apenasDigitos.Length != 11)
            throw new ClienteInvalidoException("O CPF deve conter 11 dígitos.");

        // Verifica sequências inválidas conhecidas
        if (apenasDigitos.Distinct().Count() == 1)
            throw new ClienteInvalidoException("CPF inválido.");

        // Validar dígitos verificadores
        var soma = 0;
        var resto = 0;
        for (var i = 1; i <= 9; i++)
            soma += int.Parse(apenasDigitos[i - 1].ToString()) * (11 - i);
        resto = (soma * 10) % 11;
        if (resto == 10 || resto == 11) resto = 0;
        if (resto != int.Parse(apenasDigitos[9].ToString()))
            throw new ClienteInvalidoException("CPF inválido.");

        soma = 0;
        for (var i = 1; i <= 10; i++)
            soma += int.Parse(apenasDigitos[i - 1].ToString()) * (12 - i);
        resto = (soma * 10) % 11;
        if (resto == 10 || resto == 11) resto = 0;
        if (resto != int.Parse(apenasDigitos[10].ToString()))
            throw new ClienteInvalidoException("CPF inválido.");

        return apenasDigitos;
    }
}
