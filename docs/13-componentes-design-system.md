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

Todos os componentes e páginas devem usar exclusivamente **variáveis CSS semânticas**. O script abaixo detecta automaticamente violações:

```bash
npm run lint:design-system
```

**Permitido:**
```css
color: var(--cor-texto-principal);
background-color: var(--cor-marca-principal);
```

**Proibido:**
```css
color: #D4AF37;           /* ❌ Hex fixo */
background-color: #1A1A1A; /* ❌ Hex fixo */
color: rgba(0, 0, 0, 0.5); /* ❌ RGBA fixo */
```

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
