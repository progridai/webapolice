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

    public static class Apolices
    {
        public const string Visualizar = "apolices.visualizar";
        public const string Inserir = "apolices.inserir";
        public const string Alterar = "apolices.alterar";
    }
    
    public static class ApolicesRamos
    {
        public const string Inserir = "apolices.ramos.inserir";
        public const string Alterar = "apolices.ramos.alterar";
        public const string Inativar = "apolices.ramos.inativar";
    }

    public static class ApolicesSubestipulantes
    {
        public const string Inserir = "apolices.subestipulantes.inserir";
        public const string Alterar = "apolices.subestipulantes.alterar";
        public const string Inativar = "apolices.subestipulantes.inativar";
    }

    /// <summary>
    /// Permissões para gestão de Subgrupos da Apólice.
    /// Subgrupo é uma divisão contextual da Apólice — não é cadastro global.
    /// Leitura de Subgrupos cobre-se por apolices.visualizar.
    /// </summary>
    public static class ApolicesSubgrupos
    {
        public const string Inserir = "apolices.subgrupos.inserir";
        public const string Alterar = "apolices.subgrupos.alterar";
        public const string Inativar = "apolices.subgrupos.inativar";
    }

    public static class ApolicesModulos
    {
        public const string Inserir = "apolices.modulos.inserir";
        public const string Alterar = "apolices.modulos.alterar";
        public const string Inativar = "apolices.modulos.inativar";
    }

    public static class ApolicesVidas
    {
        public const string Inserir = "apolices.vidas.inserir";
        public const string Alterar = "apolices.vidas.alterar";
        public const string Inativar = "apolices.vidas.inativar";
    }

    public static class Ramos
    {
        public const string Visualizar = "ramos.visualizar";
        public const string Inserir = "ramos.inserir";
        public const string Alterar = "ramos.alterar";
        public const string Inativar = "ramos.inativar";
        public const string Reativar = "ramos.reativar";
    }

    public static class Seguradoras
    {
        public const string Visualizar = "seguradoras.visualizar";
        public const string Inserir = "seguradoras.inserir";
        public const string Alterar = "seguradoras.alterar";
        public const string Inativar = "seguradoras.inativar";
        public const string Reativar = "seguradoras.reativar";
    }

    public static class Subestipulantes
    {
        public const string Visualizar = "subestipulantes.visualizar";
        public const string Inserir = "subestipulantes.inserir";
        public const string Alterar = "subestipulantes.alterar";
        public const string Inativar = "subestipulantes.inativar";
        public const string Reativar = "subestipulantes.reativar";
    }

    public static class Corretoras
    {
        public const string Visualizar = "corretoras.visualizar";
        public const string Inserir = "corretoras.inserir";
        public const string Alterar = "corretoras.alterar";
        public const string Inativar = "corretoras.inativar";
        public const string Reativar = "corretoras.reativar";
    }

    /// <summary>
    /// Permissões para o Cadastro Global de Módulos.
    /// Não confundir com <see cref="ApolicesModulos"/> (vínculo Apólice→Módulo).
    /// </summary>
    public static class Modulos
    {
        public const string Visualizar = "modulos.visualizar";
        public const string Inserir = "modulos.inserir";
        public const string Alterar = "modulos.alterar";
        public const string Inativar = "modulos.inativar";
    }
}
