# Módulo Clientes: Detalhes

Estado conferido no código em 21/10/2026. Validações gerais em [29 — Estabilização do Frontend](29-estabilizacao-frontend.md) e correções de identificadores, endereços e filtros em [34 — Estabilização de Clientes e Apólices](34-estabilizacao-clientes-referencias-apolices.md).

## Objetivo
O objetivo da tela de Detalhes do Cliente é fornecer uma visão abrangente dos dados associados a um cliente de forma segura. A interface exibe:
- Resumo do cliente (Nome, documento mascarado, status)
- Dados Pessoais
- Contatos
- Endereços
- Vínculos ativos
- Dependentes

## Decisão de Arquitetura (Identificador)
O parâmetro denominado `id` na rota é um UUID público. O controller recebe `Guid`, a consulta filtra `Cliente.PublicId` e a resposta C# usa `Guid`. PKs numéricas permanecem internas. `ClienteDetalheResponse.id` e os callbacks de navegação de tabela/mobile usam texto, preservando o UUID até os endpoints de consulta e edição.

- **Rota Front-end:** `/#/clientes/:id`
- **Endpoint Back-end:** `GET /api/clientes/{id}`

## Documentos e dados técnicos
- A página utiliza `documentoMascarado` para a exibição do documento principal e dos dependentes.
- O contrato atual também retorna `documento` completo, utilizado para preencher a edição. Portanto, a máscara de apresentação não significa que o contrato trafegue somente o valor mascarado.
- Chaves normalizadas de busca e `legado_id` não fazem parte do contrato do detalhe.

## Tratamento de Dados (Vínculos, Endereços e Dependentes)
A implementação atual está no módulo Cadastro, em `Infrastructure/Persistence/Queries/ClientesQueries.cs`. As listas são consultadas no banco e não são placeholders:

- Contatos e endereços ativos da pessoa.
- Vínculos ativos do cliente, com nomes de estipulante/grupo quando resolvidos.
- Dependentes do cliente, com nome, relação, CPF mascarado e nascimento.

A consulta ainda devolve nomes de cidade, subgrupo e lotação como strings vazias. O contrato de detalhe também não expõe todos os dados pessoais/bancários existentes nos modelos e na migração. Cadastro e edição atuais mantêm dados principais, contatos e endereços; manutenção de vínculos e dependentes precisa de entrega própria, orientada pelas rotinas do legado.

## Status HTTP Trataveis
- `200 OK`: Renderização normal da página com todos os cards.
- `404 Not Found`: Exibição do componente `EmptyState`, informando que o cliente não existe ou foi excluído.
- `403 Forbidden`: Exibição de um `Alert` de erro quando o backend nega a permissão de visualização.
- `Erro Genérico/500/Rede`: Exibição de um `Alert` contendo a mensagem normalizada, junto com um botão de **Tentar Novamente**.

## Integração com a Listagem
Foi adicionada a coluna **Ações** na `ClientesTable` e aprimorada a navegação na `ClientesMobileList`.
Ao clicar em "Detalhes", a rota envia via estado de histórico (`location.state`) um sinalizador de navegação que preserva os filtros atuais da listagem (`search`), garantindo que o botão "Voltar" não perca a experiência de usabilidade prévia.

## Testes
Há testes de listagem, cadastro e edição no frontend e de consulta/autorização no backend. Na etapa de 21/10/2026 foi executada somente a suíte do frontend, com 228 testes aprovados. Não foi realizada homologação visual autenticada ou reexecução dos testes do backend nessa etapa.

## Pendências para Próximas Etapas
- Alinhar tipos do frontend aos contratos reais da API.
- Completar a resolução de nomes e os campos necessários à operação.
- Implementar manutenção de vínculos, dependentes/cônjuge e demais dados pessoais necessários após levantar as rotinas do legado.
- Adicionar abas ou painéis de expansão caso os arrays de Vínculos fiquem demasiadamente extensos.
