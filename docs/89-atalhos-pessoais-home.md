# Atalhos Pessoais na Home

Data: 17/11/2026.

## Regras e Modelagem

Cada usuário ativo pode escolher quais telas aparecem em Meus Atalhos na Home.
A preferência pertence à conta autenticada e é compartilhada entre navegadores
e computadores. Não depende de uma permissão administrativa nem concede acesso
a telas. A Home e o menu compartilham o catálogo e as mesmas regras de
visibilidade por módulo, recurso, permissão, operador e flag de Design System.

A tabela existente `seguranca.usuario` recebe `atalhos_home`, array PostgreSQL
`text[]` anulável, mapeado como `Usuario.AtalhosHome`. Cada elemento é uma rota
técnica conhecida, sem parâmetros, query string ou URL externa. Não é texto
humano e não recebe Title Case. Não há nova coluna humana, alteração das listas
compartilhadas, nova tabela ou necessidade de reaplicar triggers de Title Case.

Valor null significa que a conta ainda não personalizou seus atalhos. Nesse caso,
a Home sugere as telas autorizadas de Operação, na ordem Clientes → Apólices →
Propostas. Array vazio significa a escolha explícita de não ter atalhos. A lista
tem no máximo 18 rotas distintas, limitadas aos destinos atuais do menu, exceto
Início. Telas sem acesso não aparecem nem na configuração nem nos cartões. A
preferência não substitui a autorização definitiva das rotas e APIs.

## Uso

O botão Personalizar atalhos abre uma janela com as telas disponíveis agrupadas
como no menu. O usuário marca e desmarca telas e confirma em Salvar atalhos.
Cancelar, fechar ou pressionar Escape descarta a edição. É permitido salvar
nenhuma tela. Os cartões seguem a ordem do menu principal. O salvamento impede
envios duplicados e fechamento durante o envio; uma falha mantém a edição para
nova tentativa. Carregamento e falha de consulta possuem estados próprios.

## API e Isolamento

- `GET /api/seguranca/me/atalhos-home` retorna `{ atalhosHome: string[] | null }`.
- `PUT /api/seguranca/me/atalhos-home` recebe `{ atalhosHome: string[] }` e retorna
  a seleção persistida.
- Ambos exigem autenticação e usuário local ativo. O usuário é obtido
  exclusivamente do sub autenticado. Não recebem ID de usuário de destino.
- Entradas desconhecidas, repetidas ou acima do limite são rejeitadas com 422;
  corpo ausente ou seleção null é rejeitado com 400 pelo binding.
- A preferência armazena apenas destinos conhecidos. Mesmo uma entrada gravada
  diretamente não habilita módulo, recurso, perfil ou permissão.
- Não há concessões, alterações de perfis ou operações de negócio/auditoria
  histórica. A atualização altera somente a preferência pessoal e seu timestamp.
- Esta personalização de interface não registra evento de segurança: não altera
  acessos nem dados de negócio. Registros existentes de auditoria são preservados.

## Instalação e Validação

A migration `20261117160039_AdicionarAtalhosHomeUsuario` de Segurança deve ser
aplicada antes de publicar a nova API. Não
reprocessa usuários nem altera seus dados de identidade, perfis ou concessões.
Não executar uma atualização geral de migrations em bases existentes para
instalar apenas este recurso; revisar e aplicar o script específico
`backend/scripts/migrations/home/atalhos-home.sql`. O script é transacional,
idempotente pelo histórico EF e contém somente a adição de `atalhos_home` e o
registro desta migration. Pressupõe a tabela de usuários e o histórico de
Segurança já instalados. Depois, recompilar e publicar/reiniciar a API com a
nova versão. A aplicação em ambientes persistentes e a publicação não foram
executadas nesta entrega; o teste de persistência usa PostgreSQL descartável.

## Implementação e Cobertura

- `apps/web/src/app/navigation/navigation.ts` centraliza os grupos e destinos
  existentes; `useAppNavigation.ts` compartilha o filtro de autorização sem
  alterar seus critérios. AppNavigation passa a consumir esse catálogo.
- HomePage usa os componentes PageHeader, Card, Checkbox, Modal, Alert,
  EmptyState e Skeleton existentes. O layout usa Tailwind e tokens semânticos;
  o CSS exclusivo anterior da Home foi removido.
- `atalhosHome.api.ts` usa o cliente HTTP compartilhado; `useAtalhosHome.ts`
  reutiliza useConsulta para carregar a seleção, controla o salvamento e cancela
  requisições ao desmontar ou trocar de conta. Respostas antigas são descartadas.
  O conteúdo da Home é identificado pela conta para não reaproveitar sua edição
  ao trocar de usuário.
- No backend, o caso de uso PreferenciasHomeUseCase resolve apenas o sub atual;
  PreferenciasHomeRepository carrega apenas usuário ativo. A entidade valida
  destinos, duplicidades e limite antes de alterar a preferência.
- Os testes da Home cobrem temas, seleção, cancelamento da edição, escolha vazia,
  perda de acesso, carregamento, erro, nova tentativa, bloqueio durante o envio,
  cancelamento de requisições e isolamento entre sessões. Os testes HTTP do
  cliente verificam o endpoint pessoal e a preservação das rotas técnicas.
- Os testes backend cobrem JWT obrigatório, ausência de permissão administrativa,
  recusa de usuário inativo/ausente, rejeição de destinos inválidos, isolamento
  pelo sub e rejeição de seleção null/ausente. O teste PostgreSQL verifica a
  migration, roundtrip, null versus vazio, isolamento entre contas e preservação
  de texto histórico intocado.

## Validações

Todos os seis checks obrigatórios do frontend foram aprovados:

| Check | Resultado |
| --- | --- |
| `npm.cmd run format:check` | Sem arquivos pendentes |
| `npm.cmd run typecheck` | Zero erros |
| `npm.cmd run lint` | Zero erros e avisos |
| `npm.cmd run lint:design-system` | Zero ocorrências |
| `npm.cmd run test:run -- --maxWorkers=2` | 1.329 testes aprovados em 87 arquivos |
| `npm.cmd run build` | Aprovado |

A concorrência de testes foi limitada a dois workers para respeitar os recursos
locais; nenhum teste, prazo ou configuração de verificação foi flexibilizado.
O build mantém o aviso preexistente de chunk acima de 500 kB e o aviso informativo
de tempo nos plugins CSS. Os limites não foram alterados.

Backend: solução compilada sem erros ou avisos; 156 testes da API e 12 testes
específicos de domínio/persistência aprovados, além de 28 testes de arquitetura.
O script SQL específico foi gerado pelo EF e conferido. `git diff --check` passou.
A validação [Required] do record
de entrada foi corrigida para o parâmetro do construtor, conforme o binding ASP.NET,
após o teste HTTP detectar erro de validação em tempo de execução. As verificações
de null nos testes foram explicitadas sem supressões de avisos. O lint também
detectou uma dependência da memoização do callback de fechar a janela; corrigida
sem alterar regras ou configurações.

A homologação visual e autenticada permanece pendente: o inventário de apps e
navegadores retornou vazio. Os testes em jsdom não substituem essa etapa.
