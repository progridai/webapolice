# Consolidação de Propostas — 23/10/2026

Escopo desta correção: A04 da revisão 76. A funcionalidade continua sem homologação integrada e sem regras financeiras completas.

## Decisões Confirmadas Pelo Responsável

- Cliente/Pessoa vinculados são obrigatórios. PessoaId precisa ser resolvido pelo Cadastro; nunca copiar ClienteId. O vínculo informado na Vida deve existir para esse mesmo Cliente/Pessoa. O schema atual exige cliente_vinculo_id; ausência será rejeitada, sem substituir por zero.
- Módulo habilitável existente APOLICES, recurso PROPOSTAS, ações propostas.visualizar, propostas.inserir e propostas.alterar. Listagem/detalhes compartilham visualizar; criação usa inserir; adicionar item usa alterar. Não atribuir a perfis comuns. AcessoTotal respeita os estados do catálogo e do usuário ERP.
- Auditar criação e adição de item na mesma transação física do negócio. Não incluir dados bancários, documentos pessoais ou valores financeiros desnecessários na auditoria.
- O código do status inicial será informado pelo responsável. Não presumir ID 1, primeiro registro ou código fictício. Usar configuração externa Propostas__StatusInicialCodigo; configuração ausente ou código inexistente impede a criação.
- Regras de prêmio, parcela, capital, comissão, cobrança e demais operações serão definidas posteriormente. Nenhuma emissão, cálculo, cobrança ou pagamento será implementado nesta correção.

## Modelagem Técnica Antes da Alteração

Conforme ADR-007, adicionar `seguro.proposta_item.public_id`, UUID não nulo, índice único. A migration gera UUID para os itens existentes e as novas gravações persistem o mesmo UUID retornado. Nenhum identificador legado, dado financeiro ou evidência de migração será reescrito. Essa coluna é técnica: não pertence à classificação Title Case. Não há novos campos humanos; nomes/descrições do catálogo seguem a normalização existente.

Foi confirmado também que `proposta.legado_id` e `proposta_item.legado_id` eram inteiros obrigatórios com índice único: as novas entidades recebiam zero e a segunda gravação colidiria. Tornar esses dois campos técnicos anuláveis, preservando todos os valores existentes e a unicidade dos valores não nulos. Registros nativos não inventam um ID de legado. O rollback deve recusar a reversão se existirem registros nativos com legado_id nulo, em vez de fabricar valores ou excluir dados.

Migration estrutural do Seguro separada da migration do catálogo de Segurança. Não executar migrations no startup. Não aplicar em produção nesta execução. Após as migrations de destino, reaplicar os triggers Title Case com normalização histórica desativada conforme documento 26.

## Contratos e Liberação

Preservar os UUIDs públicos de Proposta/Apólice/Vida e o retorno `ItemId` como UUID; corrigir a origem do UUID para a coluna persistida do item. IDs internos legados recebidos nos comandos continuam pendência de evolução de contrato e não substituem a validação contextual.

Paginação: página a partir de 1, tamanho de 1 a 100 e offset representável por inteiro; valores fora do intervalo são rejeitados antes da consulta. Cancelamento HTTP deve chegar ao mediator/EF.

Permanecem requisitos de liberação: informar o código real do status inicial, homologar autenticação/permissões com o IdP e ambiente de destino, definir as regras financeiras/operacionais pendentes e comparar o fluxo com o legado. As verificações locais de migrations, associações, UUID persistido e rollback em PostgreSQL foram executadas na continuidade de 26/10 abaixo. Testes com provider em memória não substituem PostgreSQL/Keycloak reais. A matriz aprovada permite implementar as barreiras; não representa homologação financeira ou operacional.


## Continuidade — 26/10/2026

A migration estrutural foi aplicada em PostgreSQL 18.4 efêmero. PropostasPostgresTests confirmou gravações nativas sem IDs legados artificiais, resolução real de Cliente/Pessoa/vínculo, UUIDs de item persistidos e rollback por falha de auditoria, com leitura em outra conexão. Os códigos/IDs de status desses testes são artificiais e não autorizam a liberação do fluxo. Ainda faltam o código real do status inicial e as demais homologações/regras de negócio indicadas acima. Nenhuma migration foi aplicada a bases existentes.
