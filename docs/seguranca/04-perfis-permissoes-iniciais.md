# Perfis e Permissões Iniciais

Durante a introdução deste módulo, o sistema será instanciado com permissões específicas com foco principal na rotina base de `Clientes`. 

As Permissões constituem as ações autorizáveis finais.
Os Perfis agrupam as permissões e são as entidades concedidas aos usuários reais.

## 1. Permissões Cadastradas na Inicialização

O recurso alvo inicial é a entidade técnica `clientes`.

| Módulo | Recurso | Ação (Permissão) | Chave Técnica | Endpoint Equivalente |
|--------|---------|-------------------|-------------------|----------------------|
| CADASTRO | Clientes | Visualizar Lista e Detalhes | `clientes.visualizar` | `GET /api/clientes`, `GET /api/clientes/{id}` (via `[AuthorizePermissao(PermissoesSeguranca.Clientes.Visualizar)]`) |
| CADASTRO | Clientes | Cadastrar Novo Cliente | `clientes.inserir` | `POST /api/clientes` (via `[AuthorizePermissao(PermissoesSeguranca.Clientes.Inserir)]`) |
| CADASTRO | Clientes | Atualizar Dados de Cliente | `clientes.alterar` | `PUT /api/clientes/{id}` (via `[AuthorizePermissao(PermissoesSeguranca.Clientes.Alterar)]`) |
| CADASTRO | Clientes | Inativar Cliente (Exclusão Lógica) | `clientes.inativar` | `POST /api/clientes/{id}/inativar` (via `[AuthorizePermissao(PermissoesSeguranca.Clientes.Inativar)]`) |
| CADASTRO | Clientes | Reativar Cliente Inativo | `clientes.reativar` | `POST /api/clientes/{id}/ativar` (via `[AuthorizePermissao(PermissoesSeguranca.Clientes.Reativar)]`) |

**Nota sobre a Exclusão:** Não existe exclusão física (`DELETE`) em Clientes. A inativação corresponde à finalização do ciclo vital lógico.

## 2. Perfis Básicos do Sistema

Serão injetados dois perfis técnicos (cujo atributo `is_sistema = true`), garantindo que não possam ser acidentalmente deletados na interface da aplicação.

### 2.1 Perfil: ADMINISTRADOR
* **Natureza:** Perfil de exceção máxima.
* **Propriedades Físicas:** `is_sistema = true`, `acesso_total = true`.
* **Regras Especiais:**
  * Irrestrito, tem direito a acesso transversal a todos os módulos atuais e todos os módulos e recursos que venham a ser inseridos no futuro.
  * Não poderá ser inativado ou excluído.
  * O código-fonte técnico de validação deste papel fará bypass dinâmico de chaves.

### 2.2 Perfil: ADMINISTRATIVO
* **Natureza:** Perfil funcional.
* **Propriedades Físicas:** `is_sistema = true`, `acesso_total = false`.
* **Matriz de Permissões Vinculadas (`perfil_permissao`):**
  * `clientes.visualizar`
  * `clientes.inserir`
  * `clientes.alterar`
  * `clientes.inativar`
  * `clientes.reativar`
* **Exceções Negadas (Implicitly Denied):** O perfil `ADMINISTRATIVO` **NÃO** nascerá com acessos a configurações de segurança, administração de perfis, auditoria, log e controle de acesso a não ser que um Administrador modifique o seu painel de Permissões num momento futuro.

## 3. Gestão e Soma de Permissões

### 3.1 Criação de Perfis Personalizados
Após a liberação, Gestores de TI poderão criar perfis personalizados (completamente abertos à edição):
* Definindo nomes e descrições personalizadas.
* Assinalando um conjunto arbitrário de permissões no UI.
* Atribuindo a quantos funcionários desejar.
* *Nota: Somente chaves técnicas pré-existentes listadas no DDL do sistema (fornecidas via release pelo desenvolvimento) poderão ser selecionadas. Usuários não criam "novas chaves".*

### 3.2 O Conceito de Múltiplos Perfis
Diferente da estrutura primária do Keycloak baseada em restrição unitária forte, o sistema abraça o princípio aditivo da autorização.

**Cenário Exemplo:**
* Um usuário "Rodolfo" recebe o Perfil Personalizado *'Consulta Simples'* (contendo `clientes.visualizar`).
* No dia seguinte, por motivo de cobertura de férias, ele também recebe o Perfil Personalizado *'Substituto Financeiro'* (contendo `clientes.inserir` e `faturas.editar`).
* O WebApólice varrerá ativamente os 2 perfis vigentes de Rodolfo. Rodolfo terá `clientes.visualizar`, `clientes.inserir` e `faturas.editar`. 
* Quando Rodolfo perder o perfil *'Substituto Financeiro'*, sua carga de permissões reduzirá imediatamente à baseline sem risco de resquícios pontuais pendurados nele.
