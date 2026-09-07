using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace WebApolice.Modulos.Cadastro.Domain.Validators;

public static class DocumentoValidator
{
    public static bool IsValidCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return false;
        
        cpf = Regex.Replace(cpf, "[^0-9]", "");
        if (cpf.Length != 11) return false;
        if (cpf.All(c => c == cpf[0])) return false;
        
        int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        
        string tempCpf = cpf.Substring(0, 9);
        int soma = 0;
        
        for (int i = 0; i < 9; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
            
        int resto = soma % 11;
        if (resto < 2) resto = 0;
        else resto = 11 - resto;
        
        string digito = resto.ToString();
        tempCpf = tempCpf + digito;
        soma = 0;
        
        for (int i = 0; i < 10; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
            
        resto = soma % 11;
        if (resto < 2) resto = 0;
        else resto = 11 - resto;
        
        digito = digito + resto.ToString();
        
        return cpf.EndsWith(digito);
    }
}
