# Guia Oficial para Utilização do Design System e Criação de Telas

**Status:** Em vigor  
**Versão:** 2.0  
**Data:** 14 de Agosto de 2026  
**Finalidade:** Padronizar a criação e adaptação de telas (Listagem, Detalhes e Formulários) no WebApólice, focando em sistemas administrativos de alta densidade (ERPs).

Este guia contém as decisões arquiteturais e regras de ouro sobre como utilizar nosso Design System de forma escalável. Sempre consulte estas diretrizes antes de iniciar ou refatorar uma tela.

---

## 1. Princípios Básicos de Layout (Tailwind como Motor Oficial)

### 1. Separação de Preocupações (Semântica vs. Utilitários)
O WebApólice utiliza uma arquitetura para estilização onde o **Tailwind CSS é o motor oficial de layout**:
- **Design Tokens e Componentes Base**: Os componentes visuais puramente reaproveitáveis do Design System (`Button`, `Input`, `Select`) consomem os tokens globais.
- **Layouts e Espaçamentos de Tela**: Construídos **exclusivamente** com **Tailwind CSS**. O Tailwind foi configurado (`tailwind.config.js`) para consumir nativamente os Design Tokens do projeto (ex: `gap-6`, `grid-cols-2`, `max-w-[1440px]`, `bg-fundo-aplicacao`, `text-erro`, `border-borda`).

**Regra de Ouro**: 
1. Ao construir **Páginas, Formulários e Telas**, utilize as classes utilitárias do Tailwind (`flex`, `grid`, `gap-6`, `col-span-2`, `pt-6`) para agilizar o posicionamento dos elementos.
2. **NUNCA** crie classes CSS semânticas estruturais para páginas (ex: `.estipulantes-grid`, `.usuarios-page`).
3. **NUNCA** utilize cores hardcoded nativas do Tailwind (ex: `bg-gray-50`, `text-red-500`, `dark:bg-gray-800`). Você DEVE utilizar os tokens do Design System mapeados (ex: `bg-fundo-aplicacao`, `text-erro`, `border-borda`). O modo escuro é resolvido automaticamente pelas variáveis CSS dos tokens.

---

## 2. Anatomia de Detail Views (Telas de Leitura)

Sistemas administrativos lidam com volume intenso de dados. Telas de detalhes não podem ter aspecto de "rede social" com imensos espaços em branco. A densidade informacional é prioridade.

### A. O Cabeçalho (PageHeader) é Autossuficiente
- **Não crie cartões gigantes (`EntitySummary`)** no topo de telas administrativas padrão apenas para repetir o nome e status. 
- O `PageHeader` foi evoluído para comportar essas informações:
  - Passe o nome limpo e direto da entidade na propriedade `title`. (Evite rótulos estáticos poluentes como `"Username: Rodrigo"`. Use apenas o dado puro: `"Rodrigo"`).
  - Ancore as badges de status na propriedade `titleExtras`. Elas aparecerão perfeitamente alinhadas ao lado do título.
  - Utilize a propriedade `description` para exibir um subtítulo discreto (ex: o identificador interno ou e-mail), se estritamente necessário.

### B. Densidade e Escalabilidade (DescriptionList)
Para renderizar pares de `Label` e `Valor`, sempre utilize o componente genérico `DescriptionList` e siga estas regras:
1. **Densidade:** Utilize o padrão `density="compact"` para todas as telas administrativas. Ele reduz o espaçamento vertical entre os campos (para `8px`), permitindo acomodar 20 campos na tela sem forçar rolagem excessiva.
2. **Escalonamento Horizontal (Colunas):** 
   - Para agrupamentos pequenos (3 ou 4 campos), utilize `columns={1}`.
   - Para formulários grandes (como dados completos de Seguradoras ou Estipulantes), utilize **obrigatoriamente** `columns={2}` no desktop (o componente lida com o mobile).

### C. Agrupamento Semântico
- Divida a página em blocos lógicos usando o componente `DetailsSection`.
- *Exemplos:* "Dados Gerais", "Endereço", "Contatos", "Perfis Atribuídos". 

### D. Redundância de Informação (Regra de Ouro)
> A interface deve informar, não repetir.
- Se o **Nome** e o **Código** (ou Username) de uma entidade forem textualmente idênticos na prática (ex: Perfil "Administrador" e Código "ADMINISTRADOR"), a interface deve programaticamente **ocultar o código** (comparando `toLowerCase()`). 
- **Exceção para Listagens (Grids)**: Nas listagens, exiba **somente o Nome completo**. Não exiba "Códigos" ou "Usernames" como subtítulos para manter a tabela limpa.

### E. Paridade de Campos entre View e Edit (Regra de Ouro)
> A tela de visualização (Detalhes) deve exibir **todos os campos** que o formulário de edição permite alterar.
- Todo campo presente no formulário de edição deve ter um equivalente visual na tela de detalhes, agrupado na mesma seção semântica (ex: o campo "Sexo" do formulário aparece em "Dados Pessoais" na view; a seção "Informações Adicionais" do formulário deve existir na view com os mesmos campos: `falecido`, `dataObito`, `observacao`).
- Campos condicionais (ex: `dataObito` só aparece se `falecido = true`) devem seguir a mesma lógica condicional tanto no formulário quanto na view.
- Ao adicionar um novo campo ao formulário de edição, adicione **obrigatoriamente** o campo correspondente à tela de detalhes na mesma PR/commit.

---

## 3. Composição de Formulários e Telas de Edição

### A. Hierarquia de Ações (Botões)
- **Ação Principal:** O botão afirmativo de maior importância recebe `variant="primary"`.
- **Ações Secundárias:** Botões como "Voltar" ou "Cancelar" devem receber `variant="secondary"` ou `variant="ghost"`.

### B. Uso do Componente Select (Combobox)
- O componente aceita duas formas de uso equivalentes:
  - **Via prop `options`** (preferida para listas simples e estáticas):
    ```tsx
    <Select
      options={[
        { label: 'Todos', value: '' },
        { label: 'Ativo', value: '1' },
        { label: 'Inativo', value: '2' },
      ]}
    />
    ```
  - **Via `children`** (use quando precisar de lógica condicional ou iterar dinamicamente):
    ```tsx
    <Select>
      <option value="1">Opção 1</option>
      <option value="2">Opção 2</option>
    </Select>
    ```

### C. Layout Estrutural de Formulários
- Utilize o componente `FormGrid` nativo para estruturar as colunas dos formulários (baseado em grid de 12 colunas).
- Envolva as páginas com tags principais estilizadas via Tailwind: `<main className="flex flex-col gap-6 w-full max-w-[1440px] mx-auto p-0 focus:outline-none">` para garantir um padrão único em toda a aplicação.

### D. Ações Destrutivas (Regra do Botão Excluir)
- Ações destrutivas (como "Excluir") devem ser renderizadas no **rodapé do formulário** (`FormActions`), na lateral **esquerda** (opostas ao botão primário "Salvar"). Isso minimiza toques acidentais e padroniza o fluxo visual da tela. 
- Para botões destrutivos textuais ou secundários, use as cores do design system (ex: `text-erro hover:text-erro/80`), não cores hardcoded (ex: `text-red-500`).

---

## 4. Como a IA deve atuar usando este guia

Sempre que solicitada a construir, refatorar ou analisar o design de uma página no WebApólice, a IA deve validar o código contra as regras acima:
1. **Abraçar o Tailwind:** Refatorar imediatamente qualquer arquivo `.css` criado para estruturar a página (ex: `.clientes-page { display: flex }`) usando as utilidades Tailwind (`flex flex-col gap-6`). O Tailwind é o padrão.
2. **Impedir o uso de cores fixas:** Verificar arduamente se componentes ou seções usam cores fixas como `bg-gray-50`, `dark:bg-slate-800`, `text-gray-500` ou `text-red-500`. Elas causam bugs no Dark Mode! Substitua por `bg-fundo-aplicacao`, `bg-fundo-superficie`, `text-texto-secundario`, `text-erro` e `border-borda`.
3. **Visão Global do Módulo**: Ao ajustar uma tela, adeque **todas as telas associadas** à funcionalidade (Listagem, Visualização/Detalhes, Edição e Cadastro) para manter a consistência do Design System em todo o ciclo de vida do CRUD.
4. **Simplificação de Interfaces:** Eliminar redundâncias de informações (`toLowerCase()`) e aplicar `DescriptionList` compacto para maximizar o uso da tela em sistemas ERP.
5. **Formatação de Dados:** Sempre utilize funções de formatação (ex: `formatarTelefone`, `formatarCep`, `formatarDataOuVazio` de `formatters.ts`) nas telas de detalhamento/listagem. Jamais exiba dados em estado bruto (como 51999999999) para o usuário final.
