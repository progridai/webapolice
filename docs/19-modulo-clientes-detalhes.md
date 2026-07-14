# Módulo Clientes: Detalhes

## Objetivo
O objetivo da tela de Detalhes do Cliente é fornecer uma visão abrangente dos dados associados a um cliente de forma segura. A interface exibe:
- Resumo do cliente (Nome, documento mascarado, status)
- Dados Pessoais
- Contatos
- Endereços
- Vínculos ativos
- Dependentes

## Decisão de Arquitetura (Identificador)
Apesar da modelagem ideal (vide `18-modelagem-clientes-core-cadastro.md`) sugerir o uso de `public_id`, o modelo de dados real atual da aplicação baseia-se unicamente em `id` (`long`). Em respeito à restrição de não alterar a modelagem de banco de dados e as migrations nesta etapa, o identificador adotado para rota e chamadas de API foi o `id` interno.

- **Rota Front-end:** `/#/clientes/:id`
- **Endpoint Back-end:** `GET /api/clientes/{id}`

## Omissões por Privacidade
Para respeitar as diretrizes de privacidade e LGPD, as seguintes informações NÃO são exibidas ou trafegadas na tela de Detalhes do Cliente:
- `documento_principal_limpo` e `cpf_limpo` (removidos do DTO).
- `legado_id` (removido do contrato).
- Documentos completos sem máscara (utiliza-se a versão ofuscada do backend `***.***.***-99`).

## Tratamento de Dados (Vínculos, Endereços e Dependentes)
No atual estado da modelagem em C# (`WebApolice.Modulos.Clientes.Domain`), as tabelas satélites de vínculos, endereços e dependentes ainda não estão persistidas nas consultas ativas de Cliente. Para evitar quebra de contrato:
- O contrato TypeScript prevê as arrays de tipagem completa para esses itens (Ex: `enderecos`, `vinculos`, `dependentes`).
- O back-end provê essas listas vazias inicialmente. Quando a equipe evoluir o repositório/banco para incluir essas informações associadas, o front-end já estará preparado para desenhar os Cards perfeitamente, graças aos componentes `ClienteVinculosCard`, `ClienteDependentesCard` e `ClienteEnderecosCard`.

## Status HTTP Trataveis
- `200 OK`: Renderização normal da página com todos os cards.
- `404 Not Found`: Exibição do componente `EmptyState`, informando que o cliente não existe ou foi excluído.
- `403 Forbidden`: Exibição de um `Alert` de erro, bloqueando a visão caso o usuário não tenha permissão de Gestão ou visualização daquele nível.
- `Erro Genérico/500/Rede`: Exibição de um `Alert` contendo a mensagem normalizada, junto com um botão de **Tentar Novamente**.

## Integração com a Listagem
Foi adicionada a coluna **Ações** na `ClientesTable` e aprimorada a navegação na `ClientesMobileList`.
Ao clicar em "Detalhes", a rota envia via estado de histórico (`location.state`) um sinalizador de navegação que preserva os filtros atuais da listagem (`search`), garantindo que o botão "Voltar" não perca a experiência de usabilidade prévia.

## Testes
Foram validados os contratos visuais, o hook de busca, e o estado de carregamento. No back-end, os testes de listagem e controlador asseguram que as requisições autenticadas respondem de acordo com a regra de negócio.

## Pendências para Próximas Etapas
- Preencher as tabelas e modelos de EF Core de Endereços e Vínculos para que sejam povoados com informações reais.
- Adicionar abas ou painéis de expansão caso os arrays de Vínculos fiquem demasiadamente extensos.
