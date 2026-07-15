# 13 — Componentes do Design System Reutilizável

**Status:** ✅ Implementado e Homologado  
**Versão:** 1.0.0  
**Data:** Julho 2026

---

## Objetivo

Este documento cataloga todos os componentes reutilizáveis do Design System da aplicação WebApólice. Os componentes estão localizados em `apps/web/src/components/ui/` e devem ser **obrigatoriamente** usados em todas as páginas e features futuras. É proibido criar componentes paralelos com estilos próprios, cores fixas ou padrões duplicados.

---

## Estrutura de Pastas

```text
apps/web/src/
├── components/
│   └── ui/
│       ├── Alert/          (Alert.tsx, Alert.css, index.ts)
│       ├── Badge/          (Badge.tsx, Badge.css, index.ts)
│       ├── Button/         (Button.tsx, Button.css, index.ts)
│       ├── Card/           (Card.tsx, Card.css, index.ts)
│       ├── Checkbox/       (Checkbox.tsx, Checkbox.css, index.ts)
│       ├── ConfirmDialog/  (ConfirmDialog.tsx, ConfirmDialog.css, index.ts)
│       ├── EmptyState/     (EmptyState.tsx, EmptyState.css, index.ts)
│       ├── FormField/      (FormField.tsx, FormField.css, index.ts)
│       ├── Input/          (Input.tsx, Input.css, index.ts)
│       ├── Modal/          (Modal.tsx, Modal.css, index.ts)
│       ├── Pagination/     (Pagination.tsx, Pagination.css, index.ts)
│       ├── Select/         (Select.tsx, Select.css, index.ts)
│       ├── Skeleton/       (Skeleton.tsx, Skeleton.css, index.ts)
│       ├── Spinner/        (Spinner.tsx, Spinner.css, index.ts)
│       ├── Table/          (Table.tsx, Table.css, index.ts)
│       ├── Textarea/       (Textarea.tsx, Textarea.css, index.ts)
│       ├── Icons.tsx       (Biblioteca centralizada de SVGs)
│       └── index.ts        (Exportações centralizadas)
├── pages/
│   └── DesignSystem/       (Catálogo visual interativo `/design-system`)
└── scripts/
    └── lint-design-system.js (Verificação automatizada de conformidade)
```

---

## Importação

Todos os componentes são exportados via barril centralizado:

```typescript
import { Button, FormField, Input, Modal, Table, Pagination } from '@/components/ui';
// ou
import { Button } from '../../components/ui';
```

---

## Catálogo de Componentes

### Button

Botão interativo com suporte a variantes, tamanhos e estados.

| Prop | Tipo | Padrão | Descrição |
|------|------|--------|-----------|
| `variant` | `'primary' \| 'secondary' \| 'text' \| 'danger'` | `'primary'` | Estilo visual |
| `size` | `'small' \| 'medium' \| 'large'` | `'medium'` | Tamanho do botão |
| `loading` | `boolean` | `false` | Estado de carregamento com spinner |
| `disabled` | `boolean` | `false` | Estado desabilitado |

```tsx
<Button variant="primary" size="medium" loading={isSubmitting}>
  Salvar
</Button>
<Button variant="danger" onClick={handleDelete}>
  Excluir
</Button>
```

---

### FormField

Contêiner semântico que associa `<label>`, elemento filho, mensagem de erro e dica de texto de forma acessível.

| Prop | Tipo | Descrição |
|------|------|-----------|
| `label` | `ReactNode` | Texto do rótulo |
| `required` | `boolean` | Exibe asterisco `*` e `required` class |
| `error` | `string` | Mensagem de erro (aciona `aria-invalid`) |
| `hint` | `string` | Texto de ajuda abaixo do campo |
| `children` | `ReactElement` | Campo de formulário filho |

```tsx
<FormField label="Nome Completo" required error={errors.nome} hint="Sem abreviações.">
  <Input value={nome} onChange={(e) => setNome(e.target.value)} />
</FormField>
```

> ⚠️ O `FormField` injeta automaticamente `id`, `aria-describedby` e `aria-invalid` no filho. Não é necessário setá-los manualmente.

---

### Input / Textarea / Select

Campos nativos encapsulados com estilos padronizados e suporte a estados de erro, desabilitado e somente leitura.

```tsx
<Input placeholder="Digite aqui..." disabled={isLoading} />
<Textarea rows={4} placeholder="Observações..." />
<Select value={status} onChange={(e) => setStatus(e.target.value)}>
  <option value="ativo">Ativo</option>
  <option value="inativo">Inativo</option>
</Select>
```

---

### Checkbox

Checkbox customizado com suporte a estado indeterminado e label integrada.

```tsx
<Checkbox label="Aceitar os termos" checked={aceito} onChange={(e) => setAceito(e.target.checked)} />
<Checkbox indeterminate={parcial} checked={todos} onChange={toggleTodos} aria-label="Selecionar tudo" />
```

---

### Alert

Mensagem de feedback com variantes semânticas e ícones automáticos.

| Variante | Uso |
|----------|-----|
| `success` | Operação concluída com êxito |
| `error` | Falha crítica ou erro de validação |
| `warning` | Aviso sobre pendências ou riscos |
| `info` | Informação contextual neutra |

```tsx
<Alert variant="success" title="Cadastro Salvo">
  Cliente registrado com sucesso.
</Alert>
<Alert variant="error" onClose={() => setError(null)}>
  Falha ao conectar com o servidor.
</Alert>
```

---

### Card

Superfície estrutural com subcomponentes compostos.

```tsx
<Card>
  <CardHeader>
    <CardTitle>Título do Cartão</CardTitle>
    <CardDescription>Descrição breve.</CardDescription>
  </CardHeader>
  <CardContent>
    {/* Conteúdo principal */}
  </CardContent>
  <CardFooter>
    <Button variant="primary">Ação</Button>
  </CardFooter>
</Card>
```

---

### Badge

Indicador compacto de status.

```tsx
<Badge variant="success" dot>Ativo</Badge>
<Badge variant="error">Inativo</Badge>
<Badge variant="warning">Pendente</Badge>
<Badge variant="brand">VIP</Badge>
```

---

### Spinner & Skeleton

Componentes de carregamento.

```tsx
{/* Spinner inline */}
<Spinner size="medium" aria-label="Carregando dados..." />

{/* Skeleton para estruturas de espaço reservado */}
<Skeleton variant="text" width="70%" />
<Skeleton variant="avatar" />
<Skeleton variant="block" height={200} />
```

---

### Modal

Diálogo acessível com trap de foco, bloqueio de rolagem e fechamento por Escape.

| Prop | Tipo | Descrição |
|------|------|-----------|
| `aberto` | `boolean` | Controla a visibilidade |
| `onClose` | `() => void` | Callback ao fechar |
| `title` | `string` | Título do diálogo (aria-labelledby) |
| `size` | `'small' \| 'medium' \| 'large'` | Tamanho máximo |
| `footer` | `ReactNode` | Rodapé com ações |

```tsx
<Modal aberto={aberto} onClose={fechar} title="Editar Cliente" size="medium"
  footer={
    <>
      <Button variant="secondary" onClick={fechar}>Cancelar</Button>
      <Button variant="primary" onClick={salvar} loading={salvando}>Salvar</Button>
    </>
  }
>
  {/* Formulário de edição */}
</Modal>
```

---

### ConfirmDialog

Especialização do Modal para confirmações críticas.

```tsx
<ConfirmDialog
  aberto={aberto}
  onClose={cancelar}
  onConfirm={confirmarExclusao}
  title="Confirmar Inativação"
  description="Esta ação irá suspender o cadastro do cliente."
  variant="danger"
  confirmText="Sim, Inativar"
  cancelText="Não, Cancelar"
  loading={excluindo}
/>
```

---

### Table

Conjunto de subcomponentes para listagem de dados tabulares.

```tsx
<Table>
  <TableHeader>
    <TableRow>
      <TableCell header>Nome</TableCell>
      <TableCell header>Status</TableCell>
    </TableRow>
  </TableHeader>
  <TableBody>
    {clientes.map(cliente => (
      <TableRow key={cliente.id} selecionado={selecionados.includes(cliente.id)}>
        <TableCell>{cliente.nome}</TableCell>
        <TableCell>
          <Badge variant={cliente.ativo ? 'success' : 'neutral'} dot>
            {cliente.ativo ? 'Ativo' : 'Inativo'}
          </Badge>
        </TableCell>
      </TableRow>
    ))}
  </TableBody>
</Table>
```

---

### Breadcrumbs

Indicador de caminho de navegação.

```tsx
<Breadcrumbs items={[
  { label: 'Início', href: '/' },
  { label: 'Clientes', href: '/clientes' },
  { label: 'Editar Cliente' }
]} />
```

---

### PageHeader

Cabeçalho padrão para páginas da aplicação.

```tsx
<PageHeader 
  title="Editar Cliente"
  description="Atualize os dados do cliente."
  icon={<UsersIcon size={24} />}
  breadcrumbs={<Breadcrumbs items={items} />}
/>
```

---

### FormSection

Container padronizado para agrupamento lógico de campos em formulários.

```tsx
<FormSection title="Dados Pessoais" icon={<UserIcon size={20} />}>
  {/* campos */}
</FormSection>
```

---

### FormGrid

Grid system baseado em 12 colunas para layout consistente de formulários responsivos.

```tsx
<FormGrid>
  <div className="col-span-12 lg:col-span-6">
    <FormField label="Nome"><Input /></FormField>
  </div>
  <div className="col-span-12 lg:col-span-6">
    <FormField label="Sobrenome"><Input /></FormField>
  </div>
</FormGrid>
```

---

### FormActions

Barra de ações fixada no rodapé (sticky) para formulários longos.

```tsx
<FormActions>
  <Button variant="text">Cancelar</Button>
  <Button variant="primary">Salvar</Button>
</FormActions>
```

---

### ReadOnlyField

Campo para exibição de valores imutáveis de forma clara e acessível, como alternativas ao input desabilitado.

```tsx
<ReadOnlyField value="123.456.789-00" />
```

---

### Pagination

Controle de paginação desacoplado.

```tsx
<Pagination
  currentPage={pagina}
  totalPages={totalPaginas}
  onPageChange={(p) => setPagina(p)}
  totalItems={total}
  pageSize={10}
/>
```

---

### EmptyState

Tela de estado vazio para listas sem registros.

```tsx
<EmptyState
  title="Nenhum cliente encontrado"
  description="Tente ajustar os filtros de pesquisa ou adicione um novo cliente."
  icon={<span aria-hidden="true">👥</span>}
  action={<Button variant="primary">Novo Cliente</Button>}
/>
```

---

### EntitySummary

Componente de resumo inicial contendo avatar/ícone, nome, documento, status funcional (via `StatusBadge`) e informações complementares secundárias. Deve ser renderizado abaixo do `PageHeader`.

```tsx
<EntitySummary
  name={cliente.nome}
  documentInfo={cliente.documentoMascarado}
  badges={<StatusBadge status={cliente.status.codigo} />}
  secondaryInfo={<div>Nascimento: {cliente.dataNascimento}</div>}
/>
```

---

### DetailsSection

Componente reutilizável (`Card` especializado) para agrupar informações de leitura em blocos nomeados. Aceita estado vazio e propriedades integradas.

```tsx
<DetailsSection title="Contatos" isEmpty={!contatos.length} emptyState="Nenhum contato.">
  {/* Conteúdo iterativo */}
</DetailsSection>
```

---

### DescriptionList

Lista descritiva padronizada com colunas (1 a 3) usando `dl`, `dt` e `dd` nativos, focado na acessibilidade de chave-valor. Requer a importação conjunta do subcomponente `DescriptionItem`.

```tsx
<DescriptionList columns={2}>
  <DescriptionItem label="Nome" value={cliente.nome} />
  <DescriptionItem label="E-mail" value={cliente.email} />
</DescriptionList>
```

---

### StatusBadge

Um badge especializado para propósitos funcionais de entidades (baseado no `Badge` normal), garantindo semântica correta de cor e estilo: "ativo" (success), "inativo" (error), "pendente" (warning), etc. A cor da marca não deve ser usada para esse propósito.

```tsx
<StatusBadge status="ativo" />
<StatusBadge status="inativo" />
```

---

## Anatomia Padrão de uma Página de Detalhes

Toda nova página de detalhes (ex: Detalhe do Cliente, Vínculos, Propostas) deve seguir rigorosamente a mesma anatomia visual, utilizando os componentes descritos:

```text
Página de Detalhes
├── PageHeader (com Breadcrumbs, Título e Ações Principais)
├── EntitySummary (Resumo top-level do registro)
├── Grid de 1 ou 2 colunas
│   ├── DetailsSection (Dados principais contendo DescriptionList)
│   ├── DetailsSection (Listagens 1)
│   └── DetailsSection (Listagens 2)
```

---

## Ícones Disponíveis

Centralizados em `Icons.tsx`, todos baseados em `currentColor` para total compatibilidade com os temas:

| Exportação | Uso Indicado |
|------------|--------------|
| `ThemeIcon` | Seletor de tema |
| `SearchIcon` | Campos de pesquisa |
| `CheckIcon` | Confirmações e sucesso |
| `AlertIcon` | Avisos |
| `ErrorIcon` | Erros |
| `InfoIcon` | Informações |
| `SortIcon` | Ordenação de tabelas |
| `CalendarIcon` | Campos de data |

```tsx
import { SearchIcon, CheckIcon } from '../../components/ui';

<SearchIcon size={16} />
<CheckIcon size={20} className="icon-success" />
```

---

## Conformidade de Tokens

### Regra: Nenhuma Cor Fixa Permitida

Todos os componentes e páginas devem usar exclusivamente **variáveis CSS semânticas**. É estritamente proibido:
- Usar cores fixas em Hexadecimal, RGB, RGBA ou HSL dentro do CSS dos componentes.
- O uso direto do nome "dourado" ou propriedades atreladas a uma marca (ex: `--cor-sobre-dourado`).
- Receber cores arbitrárias via Props para customizar aparência individual de elementos (a identidade visual deve ser resolvida globalmente pela camada `IdentidadeVisualService`).

O script abaixo detecta automaticamente violações:

```bash
npm run lint:design-system
```

**Permitido (Do's):**
```css
color: var(--cor-texto-principal);
background-color: var(--cor-marca-principal);
color: var(--cor-sobre-marca-principal);
```

**Proibido (Don'ts):**
```css
color: #D4AF37;           /* ❌ Hex fixo */
background-color: #1A1A1A; /* ❌ Hex fixo */
color: rgba(0, 0, 0, 0.5); /* ❌ RGBA fixo */
color: var(--cor-sobre-dourado); /* ❌ Uso de nome acoplado à marca antiga */
```

> [!WARNING]
> O alias `--cor-sobre-dourado` foi mantido apenas para garantir retrocompatibilidade com componentes ainda não migrados. Novos componentes e manutenções **devem** substituir pelo token semântico `--cor-sobre-marca-principal`. A remoção definitiva do alias é uma dívida técnica controlada e ocorrerá futuramente.

### Comportamento com Identidades Diferentes e Estados

A aplicação suporta identidades organizacionais dinâmicas. Os componentes foram desenhados para não saberem qual cor está ativa:
- **Estados (Hover, Active, Focus, Disabled)**: Todos os estados derivados devem ser consumidos via tokens (ex: `--cor-marca-principal-hover`), pois a geração desses contrastes é responsabilidade do Serviço de Identidade Visual.
- **Botões e Elementos Ativos**: O texto que vai sobre a cor principal deve consumir SEMPRE `--cor-sobre-marca-principal`, pois a cor da organização pode ser clara (exigindo texto escuro) ou escura (exigindo texto claro).

---

## Showcase / Catálogo Visual

Para visualizar todos os componentes interativamente em ambiente de desenvolvimento, navegue para:

```
http://localhost:5173/#design-system
```

A rota `/design-system` (hash-based) renderiza a `DesignSystemPage` com estados interativos, demonstrando todos os componentes com dados mock.

---

## Regras Obrigatórias para Novas Features

1. **Não crie componentes UI duplicados.** Sempre verifique `src/components/ui/` antes.
2. **Não escreva estilos inline com cores.** Use tokens CSS semânticos.
3. **Execute `npm run lint:design-system` antes de qualquer PR** e garanta que passe sem erros.
4. **Não importe componentes diretamente de subpastas.** Use o barril `../../components/ui`.
5. **Novos ícones** devem ser adicionados ao `Icons.tsx` centralizado.
6. **Componentes de domínio** (ex: `ClienteCard`) ficam em `src/features/[modulo]/components/`, nunca em `src/components/ui/`.

---

## Anatomia Padrão de uma Página de Listagem

Toda nova página de listagem/consulta (ex: Clientes, Vínculos, Propostas) deve seguir a mesma anatomia:

```text
Página de Listagem
├── PageHeader (com Breadcrumbs, Título e Ação Principal)
├── FilterBar
│   ├── SearchField (busca com debounce)
│   ├── Select (filtro de status ou outros)
│   └── Button (limpar filtros)
├── ResultsSummary ("Exibindo X–Y de Z registros")
├── DataTable (desktop) ou Cards responsivos (mobile)
│   ├── Estado loading (Skeletons automáticos)
│   ├── Estado vazio sem filtros
│   └── Estado vazio com filtros (com ação "Limpar filtros")
├── Pagination
└── Estado de erro com retry
```

### FilterBar

Container flexível para agrupar filtros. Agnóstico ao domínio.

```tsx
<FilterBar>
  <SearchField id="busca" placeholder="Buscar..." value={q} onChange={setQ} />
  <Select options={statusOptions} value={status} onChange={setStatus} />
  <Button variant="secondary" disabled={!hasFilters} onClick={clearFilters}>Limpar</Button>
</FilterBar>
```

> **Responsividade**: empilhamento em coluna no mobile (`< 768px`), linha no desktop.

---

### SearchField

Campo de busca com debounce interno, ícone de lupa e botão de limpeza rápida.

| Prop | Tipo | Padrão | Descrição |
|------|------|--------|-----------|
| `id` | `string` | — | ID do input (obrigatório) |
| `value` | `string` | `''` | Valor externo (controlado) |
| `onChange` | `(value: string) => void` | — | Callback após debounce |
| `debounceMs` | `number` | `500` | Delay do debounce em ms |
| `disabled` | `boolean` | `false` | Estado desabilitado |

```tsx
<SearchField id="busca-cliente" placeholder="Nome ou CPF" value={q} onChange={setQ} />
```

> ⚠️ Sempre use `id` + `<label htmlFor>`. Não dependa apenas de `placeholder`.

---

### DataTable

Wrapper genérico para tabelas de dados com ordenação, loading (skeletons) e estado vazio internos.

```tsx
import type { Column } from '../../components/ui/DataTable/DataTable';

const columns: Column<Cliente>[] = [
  { key: 'nome', label: 'Nome', sortable: true },
  { key: 'status', label: 'Status', render: (item) => <StatusBadge status={item.status} /> },
  { key: 'acoes', label: 'Ações', align: 'right', render: (item) => <RowActions ... /> },
];

<DataTable
  data={clientes}
  columns={columns}
  keyExtractor={(item) => item.id}
  isLoading={isLoading}
  sortBy="nome"
  direction="asc"
  onSort={handleSort}
  emptyTitle="Nenhum cliente cadastrado"
  aria-label="Lista de clientes"
/>
```

> ⚠️ Importe `Column` como `type` import direto: `import type { Column } from '../../components/ui/DataTable/DataTable'`

---

### RowActions

Padroniza ações por linha: botão primário visível + menu dropdown para ações secundárias.

```tsx
<RowActions
  primaryAction={{ label: 'Detalhes', icon: <EyeIcon />, onClick: () => navigate(`/${id}`) }}
  actions={[
    { label: 'Editar', icon: <EditIcon />, onClick: handleEditar },
    { label: 'Inativar', icon: <XCircleIcon />, variant: 'danger', onClick: handleInativar },
  ]}
  ariaLabel={`Ações para ${nome}`}
/>
```

> ❌ **Antipadrão**: Não use múltiplos `<Button>` lado a lado — use `RowActions`.

---

### DropdownMenu / DropdownMenuItem

Popover posicionado para menus contextuais. Fecha ao clicar fora ou ao selecionar item.

```tsx
<DropdownMenu align="right" trigger={<Button icon={<MoreVerticalIcon />} aria-label="Mais ações" />}>
  <DropdownMenuItem icon={<EditIcon />} onClick={handleEditar}>Editar</DropdownMenuItem>
  <DropdownMenuItem icon={<XCircleIcon />} className="row-actions-item-danger" onClick={handleInativar}>
    Inativar
  </DropdownMenuItem>
</DropdownMenu>
```

---

### ResultsSummary

Exibe "Exibindo X–Y de Z registros". Posicionado **acima** da tabela. Retorna `null` quando `totalItems === 0`.

```tsx
<ResultsSummary
  currentPage={data.paginaAtual}
  pageSize={data.tamanhoPagina}
  totalItems={data.totalItens}
/>
```

---

## Ícones Disponíveis

Centralizados em `Icons.tsx`, todos em `currentColor`:

| Exportação | Uso |
|------------|-----|
| `ThemeIcon` | Seletor de tema |
| `SearchIcon` | Campos de pesquisa |
| `CheckIcon` | Confirmações |
| `AlertIcon` | Avisos |
| `ErrorIcon` | Erros |
| `InfoIcon` | Informações |
| `SortIcon` | Ordenação |
| `CalendarIcon` | Data |
| `EyeIcon` | "Ver detalhes" |
| `EditIcon` | "Editar" |
| `MoreVerticalIcon` | Trigger de menu |
| `CheckCircleIcon` | "Ativar" |
| `XCircleIcon` | "Inativar" |
| `UsersIcon` | Contexto de clientes |
| `HomeIcon` | Navegação inicial |
