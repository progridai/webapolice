# Guia Oficial para Implementação de Novos Módulos

**Status:** Em vigor  
**Versão:** 3.1  
**Data da última revisão:** 31 de Julho de 2026  
**Finalidade:** Integração Técnica de novas funcionalidades com Segurança e Controle de Acesso  

> **REGRA CRÍTICA:** As regras funcionais, campos, telas e fluxos do novo módulo devem ser definidos em documentação própria. Este guia trata da configuração técnica de Segurança e Controle de Acesso.

## 1. Referência Técnica de Segurança (Caminhos Reais)

Para otimizar o desenvolvimento e evitar buscas desnecessárias (economizando tokens), a IA deve consultar diretamente os arquivos base abaixo ao construir um novo módulo:

**Backend (Domínio e Persistência):**
- **Entidade e configuração de Modulo**: `backend/src/WebApolice.Modulos.Seguranca/Domain/Modulo.cs`, `backend/src/WebApolice.Modulos.Seguranca/Infrastructure/Persistence/Configurations/ModuloConfiguration.cs`
- **Entidade Recurso**: `backend/src/WebApolice.Modulos.Seguranca/Domain/Recurso.cs`, `backend/src/WebApolice.Modulos.Seguranca/Infrastructure/Persistence/Configurations/RecursoConfiguration.cs`
- **Entidade Permissao**: `backend/src/WebApolice.Modulos.Seguranca/Domain/Permissao.cs`, `backend/src/WebApolice.Modulos.Seguranca/Infrastructure/Persistence/Configurations/PermissaoConfiguration.cs`
- **Entidade Perfil**: `backend/src/WebApolice.Modulos.Seguranca/Domain/Perfil.cs`, `backend/src/WebApolice.Modulos.Seguranca/Infrastructure/Persistence/Configurations/PerfilConfiguration.cs`

**Backend (Autorização e APIs):**
- **PermissoesSeguranca (constantes)**: `backend/src/WebApolice.Modulos.Seguranca/Application/Authorization/PermissoesSeguranca.cs`
- **PermissaoAuthorizationHandler**: `backend/src/WebApolice.Modulos.Seguranca/Infrastructure/Authorization/PermissaoAuthorizationHandler.cs`
- **AuthorizePermissaoAttribute**: `backend/src/WebApolice.Modulos.Seguranca/Infrastructure/Authorization/AuthorizePermissaoAttribute.cs`
- **PermissaoPolicyProvider**: `backend/src/WebApolice.Modulos.Seguranca/Infrastructure/Authorization/PermissaoPolicyProvider.cs`
- **Controller de GET /api/seguranca/me**: `backend/src/WebApolice.Modulos.Seguranca/Api/Controllers/MeController.cs`
- **Caso de uso de /api/seguranca/me**: `backend/src/WebApolice.Modulos.Seguranca/Application/UseCases/Me/ObterUsuarioAutenticadoUseCase.cs`
- **Auditoria administrativa de Segurança**: `backend/src/WebApolice.Auditoria/Infrastructure/RegistradorAuditoria.cs`
- **Testes de Segurança**: `backend/tests/WebApolice.Integration.Tests/Modulos/Seguranca/`

**Backend (Exemplos de Migrations do Catálogo):**
- `backend/src/WebApolice.Modulos.Seguranca/Migrations/20260723101318_CargaInicialSeguranca.cs`
- `backend/src/WebApolice.Modulos.Seguranca/Migrations/20260724103424_CargaAdministracaoSeguranca.cs`
- `backend/src/WebApolice.Modulos.Seguranca/Migrations/20260724194126_AdicionarHabilitacaoModulo.cs`
- `backend/src/WebApolice.Modulos.Seguranca/Migrations/20260728144458_RenomearModuloCadastroParaClientes.cs`

**Frontend:**
- **AuthorizationProvider do frontend**: `apps/web/src/auth/AuthorizationProvider.tsx`
- **PermissionProtectedRoute**: `apps/web/src/app/routes/PermissionProtectedRoute.tsx`
- **AppNavigation**: `apps/web/src/layouts/AuthenticatedLayout/AppNavigation.tsx`
- **Arquivo de rotas de Clientes (Referência)**: `apps/web/src/features/clientes/routes/clientes.routes.tsx`
- **Arquivo de rotas de Segurança (Referência)**: `apps/web/src/features/seguranca/routes/seguranca.routes.tsx`
- **Componente de seleção de permissões**: `apps/web/src/features/seguranca/components/SelecaoPermissoes.tsx`
- **Componente de seleção de perfis**: `apps/web/src/features/seguranca/components/SelecaoPerfis.tsx`

---

## 2. Questionário Obrigatório (Focado em Segurança)

A IA deve fazer as seguintes perguntas de forma guiada, utilizando linguagem simples:

1. Essa funcionalidade pertence a um módulo já existente ou será um novo módulo habilitável? *(Exemplo: nova tela no módulo CLIENTES vs novo módulo comercial APOLICES)*
2. Qual será o nome apresentado para o módulo ou funcionalidade? *(A IA deverá propor o código técnico e pedir aprovação)*
3. Qual recurso funcional será protegido? *(Exemplo: Clientes, Apólices. A IA deverá propor o código técnico do recurso)*
4. Quais ações precisam de controle separado? *(Exemplos: visualizar, inserir, alterar, inativar, excluir, cancelar, aprovar, emitir)*
5. A listagem e os detalhes usarão a mesma permissão de visualização?
6. Quais perfis existentes receberão inicialmente cada permissão?
7. Será necessário criar algum novo perfil padrão? *(Se sim: nome do perfil e quais ações poderá realizar)*
8. O módulo poderá ser habilitado ou desabilitado?
9. A funcionalidade depende de outro módulo habilitado? *(Exemplo: Apólices depende de Clientes)*
10. Quais ações precisam ser registradas na auditoria?
11. Existem informações que não podem aparecer na auditoria? *(Senhas, tokens, dados sensíveis desnecessários)*

*Quando o usuário não souber responder, a IA deverá explicar a decisão tecnicamente, apresentar o padrão do projeto, recomendar uma opção e pedir confirmação. Não inicie a implementação enquanto houver decisão obrigatória pendente.*

## 3. Matriz de Permissões Obrigatória

A IA deverá preencher essa matriz a partir das respostas do responsável:

| Módulo | Recurso | Ação | Código da permissão | Perfil inicial | Endpoint/Tela | Auditoria |
|--------|---------|------|---------------------|----------------|---------------|-----------|
| ... | ... | ... | ... | ... | ... | ... |

### Convenção das Permissões

- **Para um único recurso principal:** `recurso.acao` (Exemplos: `clientes.visualizar`, `apolices.alterar`)
- **Para módulos com vários recursos:** `modulo.recurso.acao` (Exemplos: `financeiro.pagamentos.visualizar`)
- **Ação:** Utilizar `.alterar` e não `.editar`. A convenção deve ser aprovada antes da migration.

## 4. Regras Arquiteturais e Perfis

- **Usuários e Perfis:** O usuário pode possuir zero, um ou vários perfis. As permissões efetivas são a união dos perfis ativos. Não criar permissões diretas por usuário (não existe tabela `usuario_permissao`).
- **Acesso Total:** O perfil `ADMINISTRADOR` utiliza `acesso_total` e não precisa ter os vínculos individuais de permissão gravados. Nenhum perfil recebe novas permissões sem aprovação.
- **Módulos Habilitados:** Módulo desabilitado bloqueia menu, rota e API. A flag `acesso_total` não ignora módulo desabilitado. O módulo `SEGURANCA` permanece essencial.
- **Auditoria:** Eventos funcionais e eventos de segurança (mudanças em perfis, permissões, usuários) devem ser auditados.

---

## 5. Orientações de Codificação (Backend)

O backend é a proteção definitiva. A IA deve configurar a autorização da seguinte forma:

1. **Constantes:** Declarar as novas constantes de permissão na classe `PermissoesSeguranca.cs`.
2. **Endpoints:** Proteger os novos `Controllers` ou métodos utilizando o atributo oficial de autorização:
   `[AuthorizePermissao(PermissoesSeguranca.Clientes.Visualizar)]`
3. **Módulo Central:** Validar centralmente se o módulo está habilitado (através do AuthorizationHandler).
4. **Resolução:** As validações das permissões ocorrem dinamicamente pelo `PermissaoPolicyProvider` e são liberadas ou bloqueadas pelo `PermissaoAuthorizationHandler`.
5. **Auditoria:** Invocar os serviços de auditoria quando houver alterações em dados, garantindo que dados sensíveis não sejam logados.
6. **Testes:** Escrever testes de integração (em `backend/tests/WebApolice.Integration.Tests/`) validando:
   - Request sem token.
   - Usuário com permissão.
   - Usuário sem permissão (Forbidden).
   - Usuário `ADMINISTRADOR` (`acesso_total`).
   - Módulo desabilitado (deve falhar para todos).

## 6. Orientações de Codificação (Frontend)

O frontend foca em UX, ocultando e bloqueando elementos baseados nas permissões fornecidas pelo `/api/seguranca/me`.

1. **Estado de Autorização:** Consumir os dados do `AuthorizationProvider` (via hooks de contexto).
2. **Rotas Protegidas:** Englobar as rotas do novo módulo no componente `<PermissionProtectedRoute>` passando a permissão e o módulo correspondentes.
3. **Menu de Navegação:** Atualizar `AppNavigation.tsx` adicionando o novo item no menu lateral, condicionado às permissões ou módulo.
4. **Componentes Visuais:** Controlar a exibição de botões (ex: "Novo Cadastro", "Inativar") e colunas de ação nas tabelas usando funções de validação interna do provider como `possuiPermissao('clientes.inserir')` ou `possuiModulo('CLIENTES')`.

## 7. Migration do Catálogo de Segurança

A carga dos registros de segurança para o novo módulo deve ser feita via Migrations do Entity Framework.

- **Separação:** Não misture a migration estrutural das tabelas de negócio (ex: criar tabela `apolices`) com a migration de dados do Catálogo de Segurança. Gere migrations separadas.
- **Registros Iniciais:** A migration de segurança deverá inserir registros de `Modulo`, `Recurso`, `Permissao` e relacioná-las a um `Perfil` caso aprovado.
- **UUIDs Fixos:** Utilize `Guid.Parse("...")` com UUIDs estáticos na migration de carga para garantir que os IDs sejam previsíveis, idempotentes e não dupliquem registros caso a migration seja rodada em bases diferentes.
- **Up e Down:** Forneça lógicas reversas no método `Down` da migration para excluir os registros caso seja feito um rollback.

---

## 8. Prompt Reutilizável de Implementação

Ao iniciar o desenvolvimento de um novo módulo, o usuário colará o prompt abaixo:

> "Vamos integrar uma nova funcionalidade à estrutura de Segurança e Controle de Acesso do WebApólice.
> 
> Leia o Guia Oficial para Implementação de Novos Módulos.
> 
> Faça as perguntas obrigatórias do guia limitando-se apenas a: identificação do módulo/recurso, ações, perfis, habilitação e auditoria. 
> 
> Não faça perguntas sobre regras de domínio, campos de banco, filtros ou lógicas funcionais da rotina. 
> 
> Quando eu não souber responder algo obrigatório de segurança, explique, consulte a referência técnica listada no guia e recomende a melhor alternativa.
> 
> Depois das respostas:
> 1. Monte e proponha a Matriz de Permissões.
> 2. Liste os arquivos e referências técnicas que serão modificados (Constants, Controllers, Providers, Migrations, Rotas).
> 3. Solicite aprovação explícita antes de codificar.
> 4. Uma vez aprovado, implemente a Migration do Catálogo (usando UUIDs fixos), atualize as constantes do Backend, proteja os Endpoints e configure o Frontend."
