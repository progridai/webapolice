# Guia: Como criar migrations para tabelas já existentes (Legado/Dump)

No projeto WebApólice, o esquema de banco de dados (`cadastro`, `core`, etc.) foi inicialmente carregado através de scripts SQL (`schema_clientes.sql`) e não foi gerado desde o início pelas migrations do Entity Framework Core.

Por conta disso, muitas das configurações de entidades nesses módulos utilizam a instrução `.ExcludeFromMigrations()`. Essa instrução foi essencial para evitar que o EF tentasse criar uma tabela que já existia no banco no momento de inicializar os testes ou rodar o sistema pela primeira vez.

No entanto, quando precisamos **adicionar uma nova coluna** a uma dessas tabelas legadas (ex: `cadastro.cliente`), o fluxo normal do EF Core não funciona diretamente se a tabela continuar ignorada. Se não fizermos o processo corretamente, o EF tentará rodar um `CREATE TABLE` destruindo ou ignorando a tabela que já existe.

Siga os passos abaixo **exatamente nesta ordem** para evoluir a estrutura de tabelas que já existem fisicamente no banco.

---

## 1. Validar e Assumir a Propriedade da Tabela

Para adicionar um novo campo a uma tabela, primeiro precisamos dizer ao Entity Framework que um `DbContext` específico será o dono oficial daquela estrutura.

1. Identifique qual é o `DbContext` principal responsável pela tabela (ex: `ClientesDbContext`).
2. Abra o script SQL original do banco e compare **todas as colunas e tipos de dados** com a sua classe de mapeamento (`Configuration.cs` e a entidade `.cs`). **Eles devem bater 1:1.** 
3. *Atenção:* Se o mapeamento C# estiver diferente do SQL original, corrija o C# primeiro.
4. Se o mapeamento estiver perfeito, vá no arquivo de configuração do EF Core e remova a instrução `.ExcludeFromMigrations()`.

**Antes:**
```csharp
builder.ToTable("cliente", "cadastro", t => t.ExcludeFromMigrations());
```

**Depois:**
```csharp
builder.ToTable("cliente", "cadastro");
```

> Mantenha o `.ExcludeFromMigrations()` nos demais DbContexts paralelos que possam estar referenciando a tabela, para não causar conflitos de propriedade.

---

## 2. Gerar a Migration de BASELINE (Estado Atual)

O EF Core ainda não sabe que aquela tabela já existe. Se criarmos o campo novo direto, ele pode gerar um `CreateTable` junto. Vamos isolar esse "conhecimento prévio" numa **Migration de Baseline**.

1. **Ainda sem adicionar as propriedades novas** no C#, gere uma migration.
   - Pelo CLI: `dotnet ef migrations add BaselineNOME_DA_TABELA --context NOME_DO_DBCONTEXT --project CAMINHO_DO_PROJETO`
   - Exemplo: `dotnet ef migrations add BaselineCliente --context ClientesDbContext --project backend/src/WebApolice.Modulos.Clientes`

2. O EF Core irá gerar um arquivo `.cs` de migration onde o método `Up()` conterá o comando `migrationBuilder.CreateTable(...)`. 
3. **PASSO CRÍTICO:** Abra a classe dessa migration recém gerada e **apague todo o código de dentro do método `Up()` e `Down()`**. Deixe-os vazios.

**Como deve ficar a Migration Baseline:**
```csharp
public partial class BaselineCliente : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // VAZIO. A tabela já existe fisicamente no banco através do dump inicial.
        // O snapshot do EF apenas registrará que chegamos até aqui.
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // VAZIO.
    }
}
```

Isso garante que o Snapshot do EF fique atualizado e saiba o formato da tabela sem executar nenhum DDL perigoso no banco real.

---

## 3. Gerar a Migration da Nova Coluna

Agora que o EF Core conhece e controla a tabela, adicionar campos segue o fluxo padrão.

1. Adicione a nova propriedade no seu arquivo de `Domain` (`Cliente.cs`).
2. Adicione o mapeamento `builder.Property(...)` no seu arquivo `Configuration.cs`.
3. Gere uma nova migration:
   - Exemplo: `dotnet ef migrations add AdicionarCampoReEmCliente --context ClientesDbContext --project backend/src/WebApolice.Modulos.Clientes`

4. Verifique o código gerado pelo EF para essa segunda migration. Ele deverá conter apenas a instrução `AddColumn(...)`.

**Exemplo esperado:**
```csharp
public partial class AdicionarCampoReEmCliente : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "re",
            schema: "cadastro",
            table: "cliente",
            type: "varchar(32)",
            maxLength: 32,
            nullable: true);
    }
}
```

---

## 4. Aplicação e Validação

Nunca crie migrations diferentes para testes e produção. A exata mesma dupla de migrations geradas deve ser propagada:

1. Aplique a atualização contra a **base de testes** para garantir o funcionamento estrutural e rode os testes automatizados (ou o `run-tests-local.bat`). 
   *(Neste caso, a baseline rodará sem efeito e a migration real adicionará a coluna).*
2. Somente após a aprovação e verificação visual no banco local, você poderá executar o update da migration na base de **Produção/Desenvolvimento Principal**.
