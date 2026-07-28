namespace WebApolice.Modulos.Seguranca.Application.Authorization;

public static class PermissoesSeguranca
{
    public const string PrefixoPolicy = "Permissao:";

    public static class Clientes
    {
        public const string Visualizar = "clientes.visualizar";
        public const string Inserir = "clientes.inserir";
        public const string Alterar = "clientes.alterar";
        public const string Inativar = "clientes.inativar";
        public const string Reativar = "clientes.reativar";
    }
}
