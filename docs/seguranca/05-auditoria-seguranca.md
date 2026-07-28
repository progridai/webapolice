# Auditoria de Segurança

A mudança crítica na concessão de permissões demanda controle rigoroso e um registro em "Trilha de Auditoria" estrito para saber quem concedeu quais privilégios a qual funcionário. 

Diferente do atual log técnico infraestrutural existente no projeto, este modelo diz respeito aos **Eventos de Governança de Acesso**, vitais para a segurança da informação do cliente e que poderão vir a compor uma interface visual amigável aos administradores do sistema.

## 1. Eventos Auditáveis Obrigatórios

O módulo emitirá eventos rastreáveis para os seguintes atos:
* `USUARIO_CRIADO` (Provisionamento Automático ou Sincronização)
* `USUARIO_ATIVADO`
* `USUARIO_INATIVADO` (Suspensão temporária do back-office)
* `PERFIL_CRIADO`
* `PERFIL_ALTERADO` (Nome e descrição)
* `PERFIL_ATIVADO`
* `PERFIL_INATIVADO`
* `PERFIL_ATRIBUIDO_USUARIO`
* `PERFIL_REMOVIDO_USUARIO`
* `PERMISSAO_CONCEDIDA_PERFIL`
* `PERMISSAO_REMOVIDA_PERFIL`
* `ACESSO_NEGADO` (Opcional — quando o sistema barra um usuário tentando acionar chaves indevidas; útil para detecção de fraudes).

## 2. Estrutura Prevista (`seguranca.auditoria_permissao`)

Para viabilizar a rastreabilidade contextual e visualização em interface administrativa (telas e data tables), os eventos salvarão uma "fotografia" da transação.

Proposta de schema:
* `id` (bigserial, PK)
* `usuario_executor_id` (bigint, nullable) - *O administrador logado que realizou o ato (nulo se via rotina automática).*
* `acao` (varchar) - *Um dos Eventos citados (ex: PERMISSAO_CONCEDIDA_PERFIL).*
* `entidade_tipo` (varchar) - *Qual tabela (ex: usuario, perfil, perfil_permissao).*
* `entidade_id` (bigint) - *O ID da entidade mutada.*
* `usuario_afetado_id` (bigint, nullable) - *Caso a ação afete as permissões de uma pessoa.*
* `perfil_id` (bigint, nullable) - *Caso a ação envolva os acessos base de um perfil.*
* `permissao_id` (bigint, nullable) - *Para mapear exatamente qual permissão.*
* `dados_anteriores` (jsonb) - *Objeto inteiro antes da alteração.*
* `dados_novos` (jsonb) - *Objeto resultante pós-alteração.*
* `motivo` (varchar) - *Opcional.*
* `ip_origem` (varchar) - *O IP de requisição.*
* `user_agent` (varchar) - *Fingerprint basilar HTTP.*
* `correlation_id` (varchar) - *O traceId injetado nativamente no WebApolice.Api.*
* `created_at` (timestamp, indexado)

## 3. Proteção de Dados e Conformidade (LGPD)

Em hipótese alguma a trilha de segurança conterá, em seus payloads (`jsonb`), campos vitais sensíveis para a segurança em repouso. São eles:
* Hash de Senha (embora não aplicável via Keycloak, regra global).
* JWT, Refresh Token ou Client Secrets (se houverem num futuro).
* Dados Pessoais sensíveis diretos (como Códigos de Documentos Pessoais) sem anonimização, limitando as identificações aos IDs técnicos (`usuario_id`, `sub`) e/ou emails ofuscados se essencial.

## 4. Retenção e Visualização

1. **Gestão Técnica:** Serão retidos perpetuamente. Não sofrem "soft delete", são *append-only*.
2. **Interface Administrativa:** Está prevista, de acordo com as telas macro a serem estruturadas (aba "Auditoria de Segurança"), a disponibilização para pesquisa combinada por: Data, Executor, Perfil/Usuário afetado e Ação. O campo JSON proverá detalhes visuais no formato *(Antes -> Depois)*.
