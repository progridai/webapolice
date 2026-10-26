# Identidade Visual e Design System — WebApólice

Este documento define as especificações formais de marca, design tokens, acessibilidade de contraste, diretrizes de interface de usuário (UI) e a infraestrutura técnica de temas para a aplicação **WebApólice**.

---

## Fonte oficial de verdade

> [!IMPORTANT]
> Este documento representa a **fonte oficial de verdade** para toda a identidade visual e design system do projeto WebApólice.
> - Qualquer nova tela, componente ou fluxo funcional implementado no frontend deve, obrigatoriamente, seguir estas definições e consumir os tokens semânticos documentados.
> - É terminantemente proibido sobrescrever a identidade visual localmente em componentes ou telas com cores arbitrárias ou estilos paralelos.
> - Qualquer alteração nas especificações de marca exige a revisão dos tokens do código, testes automatizados, e atualização formal sincronizada deste documento e das regras do projeto.
> - Qualquer divergência identificada na aplicação em relação a esta guia de estilo deve ser tratada como um bug e corrigida imediatamente.

---

## 1. Paleta de Cores Oficial

A paleta oficial da marca é composta por cores sóbrias, garantindo um visual corporativo elegante e profissional:

| Cor | Hexadecimal | Uso Principal |
| :--- | :---: | :--- |
| **Dourado** | `#D4AF37` | Destaques, botões primários, ícones de marca e elementos de destaque |
| **Branco** | `#FFFFFF` | Cards, superfícies elevadas e áreas de respiro (Tema Claro) |
| **Preto Suave** | `#1A1A1A` | Textos principais, títulos e ícones escuros (Tema Claro) e fundo (Tema Escuro) |
| **Cinza Claro** | `#F6F6F6` | Fundo geral da aplicação e áreas neutras (Tema Claro) |

### Cores Funcionais (Uso exclusivo para comunicação de estados)

As cores funcionais **não fazem parte** da paleta de marketing da marca e são utilizadas exclusivamente para feedbacks e status do sistema, sempre acompanhadas por textos ou ícones:

- **Sucesso (Verde)**: `--cor-sucesso` (Texto/Ícone) | `--cor-sucesso-fundo` | `--cor-sucesso-borda`
- **Erro (Vermelho)**: `--cor-erro` (Texto/Ícone) | `--cor-erro-fundo` | `--cor-erro-borda`
- **Alerta (Amarelo/Laranja)**: `--cor-alerta` (Texto/Ícone) | `--cor-alerta-fundo` | `--cor-alerta-borda`
- **Informação (Azul)**: `--cor-informacao` (Texto/Ícone) | `--cor-informacao-fundo` | `--cor-informacao-borda`

---

## 2. Acessibilidade e Contraste (WCAG)

Para garantir conformidade com as diretrizes WCAG (mínimo de **4.5:1** para texto normal e **3:1** para textos grandes):

### Contraste da Marca Principal
- **Texto Branco (`#FFFFFF`) sobre Dourado Padrão**: Contraste de **2.09:1** (REPROVADO). **Nunca utilize.**
- **Texto Preto Suave (`#1A1A1A`) sobre Dourado Padrão**: Contraste de **8.30:1** (APROVADO em conformidade máxima). **Combinação oficial obrigatória para botões primários.**

A camada de serviço (`IdentidadeVisualService`) assegura o contraste dinamicamente para identidades de organizações (White-label):
- Calcula a luminância relativa da `marcaPrincipal` e a razão de contraste das alternativas de texto, sem arredondar a decisão.
- Na escolha automática, prefere `#1A1A1A` quando a razão sobre a marca alcança `4.5:1`; caso contrário, usa `#FFFFFF` se essa alternativa alcançar o mínimo.
- Se ambas falharem em um tom intermediário, usa `#000000`. Por exemplo, vermelho `#FF0000` exige preto puro: preto suave e branco ficam abaixo de `4.5:1`.
- O mínimo obrigatório para texto normal é **WCAG AA (4.5:1)**, conforme [SC 1.4.3 do W3C](https://www.w3.org/WAI/WCAG22/Understanding/contrast-minimum.html). O algoritmo automático passou a conferir a razão efetiva; o corte anterior de luminância `0.179` não garantia esse mínimo com preto suave.
- Uma `corSobreMarcaPrincipal` explicitamente fornecida continua sendo respeitada pelo contrato existente; essa configuração precisa ser conferida na homologação. Esta correção não altera a política de valores explícitos nem constitui auditoria de todos os estados de todos os componentes.

### Contraste de Textos Secundários
- **Tema Claro**: O texto secundário mapeado para `#52525B` sobre fundo branco possui contraste de **5.73:1** (Aprovado).
- **Tema Escuro**: O texto secundário mapeado para `#A1A1AA` sobre fundo de superfície `#27272A` possui contraste de **5.38:1** (Aprovado).

### Diretrizes Adicionais de Acessibilidade
1. **Multimodalidade**: Nenhum estado ou erro deve ser comunicado unicamente pela cor. Sempre inclua texto explicativo, ícones ou rótulos acessíveis.
2. **Foco Teclado**: O estado `:focus-visible` global do navegador não deve ser removido. Os campos interativos usam uma borda dourada `--cor-foco` com outline sutil e offset.
3. **Labels de Formulário**: Placeholder não substitui o elemento `<label>`. O label deve sempre permanecer visível na árvore de acessibilidade e visualmente na tela.

---

## 3. Estrutura dos Tokens Semânticos

Os estilos e tokens estão organizados de forma modular sob `apps/web/src/styles/`:

- [cores.css](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/apps/web/src/styles/tokens/cores.css): Paleta crua e variáveis funcionais base.
- [temas.css](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/apps/web/src/styles/tokens/temas.css): Mapeamento semântico claro e escuro.
- [tipografia.css](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/apps/web/src/styles/tokens/tipografia.css): Escala e tamanhos tipográficos.
- [espacamentos.css](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/apps/web/src/styles/tokens/espacamentos.css): Escala baseada em múltiplos de 4px.
- [bordas.css](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/apps/web/src/styles/tokens/bordas.css): Raios e espessuras.
- [sombras.css](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/apps/web/src/styles/tokens/sombras.css): Sombras adaptativas para relevos.

### Lista de Principais Tokens de Cores Semânticas

```css
--cor-marca-principal:        Cor de Marca (Padrão: #D4AF37)
--cor-marca-principal-hover:  Hover da Marca (Padrão: #C59F2E)
--cor-marca-principal-ativa:  Active da Marca (Padrão: #B28E25)
--cor-fundo-aplicacao:        Fundo de tela neutro (Claro: #F6F6F6 | Escuro: #18181B)
--cor-fundo-superficie:       Cards e tabelas (Claro: #FFFFFF | Escuro: #222225)
--cor-texto-principal:        Títulos e textos comuns (Claro: #1A1A1A | Escuro: #F4F4F5)
--cor-texto-secundario:       Apoio e legendas (Claro: #52525B | Escuro: #A1A1AA)
--cor-borda:                  Bordas padrão sutis (Claro: #E4E4E7 | Escuro: #3F3F46)
--cor-sobre-marca-principal:  Texto legível garantido sobre a marca principal
```

---

### Seletor assíncrono e simulador de identidade

Revisão em 22/10/2026, registrada no [documento 53](53-estabilizacao-tokens-design-system.md):

- `AsyncSelect` usa os tokens `--cor-*` existentes para superfícies, texto, borda, foco e erro, além de `--sombra-card` para o menu. Não usa os aliases antigos inexistentes `--bg-*`, `--marca-primaria`, `--status-erro`, `--texto-*` e `--borda-*`.
- Opção selecionada e estado ativo usam o par `--cor-marca-principal` / `--cor-sobre-marca-principal`, acompanhando a identidade em vez de assumir texto branco sobre dourado.
- Os seis botões do simulador white-label usam a variante secundária padrão. Seus nomes continuam identificando as marcas de demonstração; as cores são dados técnicos em `shared/identidade/marcasDemonstracao.ts`, entregues ao serviço de identidade, e não estilos locais dos controles.
- Nenhum token ou paleta nova foi criado. Foram preservadas as seis configurações e a restauração do dourado. O scanner permanece com as mesmas regras e abrangência, sem novas exceções.

---

## 4. Tipografia e Espaçamento

### Escala de Espaçamento (Base 4px)
- `--espaco-1`: `0.25rem` (4px)
- `--espaco-2`: `0.5rem` (8px)
- `--espaco-3`: `0.75rem` (12px)
- `--espaco-4`: `1rem` (16px)
- `--espaco-5`: `1.25rem` (20px)
- `--espaco-6`: `1.5rem` (24px)
- `--espaco-8`: `2rem` (32px)
- `--espaco-10`: `2.5rem` (40px)

### Tipografia
- **Família de fontes**: `Inter`, com fallbacks do sistema operacional.
- **Tamanhos**:
  - `--fonte-tamanho-xs`: `0.75rem` (12px)
  - `--fonte-tamanho-sm`: `0.875rem` (14px)
  - `--fonte-tamanho-md`: `1rem` (16px)
  - `--fonte-tamanho-lg`: `1.125rem` (18px)
  - `--fonte-tamanho-xl`: `1.25rem` (20px)
  - `--fonte-tamanho-2xl`: `1.5rem` (24px)

---

## 5. Suporte a Temas e Persistência

A aplicação implementa suporte completo a três modos de tema:
1. **Claro (`claro`)**: Força o uso do tema claro (`data-theme="light"`).
2. **Escuro (`escuro`)**: Força o uso do tema escuro (`data-theme="dark"`).
3. **Sistema (`sistema`)**: Reage e acompanha automaticamente as preferências do sistema operacional (`prefers-color-scheme`).

### Detalhes Técnicos e Persistência
- A preferência do tema é armazenada localmente no navegador via `localStorage` na chave neutra `"webapolice-tema"`.
- Para evitar flashes de temas incorretos (flash de fundo branco ao iniciar no tema escuro), um script inline síncrono é injetado diretamente no `<head>` do [index.html](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/apps/web/index.html) para recuperar e aplicar a tag `data-theme` na raiz do documento antes da renderização do React começar.

---

## 6. Exemplos de Uso (Do's and Don'ts)

### Permitido (Do's) ✅
```css
/* Correto: Consumindo tokens semânticos */
.my-card {
  background-color: var(--cor-fundo-superficie);
  border: var(--borda-largura-padrao) solid var(--cor-borda);
  border-radius: var(--raio-medio);
}

.my-button {
  background-color: var(--cor-marca-principal);
  color: var(--cor-sobre-marca-principal);
}
```

```tsx
// Correto: Utilizando o hook de tema
const { temaResolvido } = useTema();
```

### Proibido (Don'ts) ❌
```css
/* Incorreto: Uso de cores fixas hardcoded */
.my-card {
  background-color: #ffffff; /* Quebra no tema escuro */
  border: 1px solid #e4e4e7;
}

/* Incorreto: Texto branco ou hardcoded sobre botão */
.my-button {
  background-color: #D4AF37;
  color: #FFFFFF; 
}
```

---

## 7. Identidade Visual por Organização (White-label)

A aplicação suporta identidades visuais dinâmicas (branding) definidas por organização.
O WebApólice possui o Dourado (`#D4AF37`) como identidade **padrão** (Fallback seguro).

### Conceito e Propriedades Configuráveis
A cada organização é permitido configurar:
- **`marcaPrincipal`**: Cor base da marca.
- **`logotipoUrl` e `nomeExibicao`**.
- As cores de interação (`hover`, `ativa`) e a `corSobreMarcaPrincipal` podem ser calculadas dinamicamente caso não fornecidas.

### Propriedades Não Configuráveis
Para manter consistência, acessibilidade e harmonia estrutural, as organizações **não podem** customizar livremente:
- Cores Funcionais de Status (Sucesso, Erro, Alerta, Informação).
- Fundo da Aplicação e de Superfície.
- Tipografia e Espaçamentos.
- Raios de borda e Sombras.

### Cache Versionado e Validade
A identidade é armazenada no `localStorage` sob o modelo `IdentidadeVisualCache`:
- O cache possui `versao` (atual: '1.0') e `atualizadoEm`.
- Possui uma expiração máxima de 7 dias, obrigando o frontend a atualizar eventuais mudanças da API após esse período.
- Em caso de logout explícito ou encerramento da sessão de um usuário, o método `IdentidadeVisualService.restaurarIdentidadePadrao()` deve ser chamado, apagando a chave `webapolice-identidade-atual`. Isso previne o vazamento visual de uma organização (Ex: Org A) na tela pública antes de um usuário da Org B fazer login.

### Resolução e Prevenção de FOUC
- Os dados da organização carregada ficam cacheados no `localStorage` (Chave: `webapolice-identidade-{organizacaoId}`).
- O script no `<head>` de `index.html` resolve o tema (Claro/Escuro/Sistema) e, síncronamente, verifica se existe uma identidade de organização no cache que seja compatível com a versão atual e dentro do prazo de validade. Se for validada, os tokens são injetados no `:root`.
- O FOUC de identidade visual é mitigado SEM realizar cálculos demorados (HSL, Luminância) dentro do html. Ele carrega cegamente um JSON validado, mas faz sanity-check do payload e tempo de vida.
- A mudança na paleta da organização não afeta a preferência do usuário (Claro/Escuro/Sistema). O Modo Dark mode funciona em harmonia com qualquer cor escolhida.
