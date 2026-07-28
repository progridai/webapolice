# Visão Geral e Decisões

## 1. Contexto

O ecossistema WebApólice encontra-se em fase de estruturação de suas fundações. A configuração atual lida perfeitamente com a identificação do usuário e sua autenticação por meio do fluxo padrão OIDC utilizando o **Keycloak**. Todavia, até o momento, as validações de Autorização tanto no Frontend quanto no Backend baseiam-se em roles engessadas providas pelo Keycloak (`admin`, `gestor`, `operador`).

## 2. Problema Atual

O uso de "Realm Roles" estáticas atende as necessidades de um MVP (Produto Mínimo Viável), mas não suporta as complexidades reais de um sistema ERP, onde é necessário criar múltiplos grupos de acesso e definir meticulosamente quem pode interagir, alterar, criar ou visualizar registros com granularidade em nível de funcionalidade (e.g. `clientes.inativar`, `apolices.emitir`). Manter milhares de roles granulares no Keycloak adiciona complexidade de sincronização desnecessária à aplicação.

## 3. Limites entre Keycloak e PostgreSQL

Para mitigar a complexidade acima, uma fronteira arquitetural restrita foi definida:

* **Responsabilidade do Keycloak:** 
  * Gestão primária da Identidade (Autenticação).
  * Telas de login e redefinição de senhas.
  * Validação de fatores de segurança (MFA).
  * Fornecimento do token JWT (Sessão).
* **Responsabilidade do PostgreSQL (WebApólice):**
  * Gestão centralizada da Autorização.
  * Agrupamento de Perfis (Grupos de Permissão).
  * Regras de negócio de "o que o usuário X logado pode fazer no sistema Y".
  * Histórico de eventos e trilhas de auditoria das permissões geradas.

## 4. Decisões Arquiteturais Aprovadas

1. O WebApólice **não armazenará senhas** e não manterá base dupla de credenciais.
2. O WebApólice **não consumirá inicialmente a API Administrativa do Keycloak**. Toda a administração de conta deverá ser feita por um operador dentro do próprio painel administrativo do Keycloak.
3. O vínculo único e imutável que interliga os dois domínios será a "Claim OIDC `sub`", contida no Token.
4. Quando o usuário se autenticar no WebApólice (já possuindo uma conta válida no Keycloak), ele passará por um processo de **provisionamento dinâmico automático** (*Just-in-Time Provisioning*) caso ainda não exista no banco PostgreSQL, criando seu espelho interno em `seguranca.usuario`.
5. As permissões concedidas sempre serão unidas. Ex: Se um usuário possui os perfis A e B, sua permissão de tela será a soma (A ∪ B) das restrições de ambos.
6. Nunca haverá liberação "por usuário" individual (não existirá a relação técnica `usuario_permissao`), somente "por perfil".

## 5. Escopo Inicial

* Foco total da Autorização será testado sob a ótica do Módulo de **Clientes** (ver permissões mapeadas em [Perfis e Permissões Iniciais](04-perfis-permissoes-iniciais.md)).
* Serão geradas as tabelas para suportar Módulos, Recursos, Permissões, Perfis e a ligação Usuário <> Perfil.
* Criação de dois perfis base de negócio: `ADMINISTRADOR` (acesso total irrevogável) e `ADMINISTRATIVO`.

## 6. Itens Fora do Escopo

* Integração via API de administração de identidades (Keycloak Admin REST API) para bloquear, criar ou excluir contas diretamente pelo ERP.
* Autenticação Local (Basic Auth) ou gestão de senhas/recuperação de senhas na UI do WebApólice.
* Configuração e desenho da Federação de Identidade (Integração com Azure AD, Google, etc.).
