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

    public static class Estipulantes
    {
        public const string Visualizar = "estipulantes.visualizar";
        public const string Inserir = "estipulantes.inserir";
        public const string Alterar = "estipulantes.alterar";
        public const string Excluir = "estipulantes.excluir";
        public const string Inativar = "estipulantes.inativar";
        public const string Reativar = "estipulantes.reativar";
    }

    public static class Cooperados
    {
        public const string Visualizar = "cooperados.visualizar";
        public const string Inserir = "cooperados.inserir";
        public const string Alterar = "cooperados.alterar";
        public const string Inativar = "cooperados.inativar";
        public const string Reativar = "cooperados.reativar";
    }
}
