# Situação Inicial: Pré Proposta

Em 17/11/2026, o responsável definiu **Pré Proposta** como nome da situação inicial. Após a confirmação, ela passa a ser uma proposta, seguindo as regras de início de vigência existentes.

A nomenclatura foi revista nos fluxos de criação pela Vida, consulta, preenchimento, edição, confirmação, preparação de nova versão, descarte e reinclusão, incluindo rótulos acessíveis, mensagens da API e novos eventos de histórico/auditoria. As regras de negócio, numeração e permissões permanecem as mesmas.

## Catálogo e Compatibilidade

A migration `20261117120000_NomePreProposta` altera somente `seguro.proposta_status.nome` da situação inicial para `Pré Proposta`, em Title Case. Preserva ID, código, flags e todos os vínculos. Recusa situação ausente ou configuração incompatível. Não modifica propostas, beneficiários, evidências ou eventos históricos. Não há novos campos humanos nem mudança nas classificações ou triggers existentes.

O código técnico `RASCUNHO`, a configuração `Propostas__StatusInicialCodigo`, as rotas `/rascunho`, propriedades JSON, nomes internos e migrations anteriores permanecem para compatibilidade. Essas ocorrências técnicas não são o nome exibido ao usuário. Os nomes de arquivos e links da documentação também foram preservados. O estado local de texto ainda não salvo em campos genéricos não representa a situação de uma proposta.

## Implantação e Verificação

Aplicar a nova migration do contexto Seguro pelo procedimento de implantação do ambiente; migrations não executam automaticamente no startup. O rollback preserva a nomenclatura aprovada. Esta entrega não publica a API nem altera bancos persistentes.

Testes de integração verificam o nome novo, a preservação dos demais dados do catálogo, a reaplicação e a recusa de configuração incompatível. O teste de evolução do legado continua verificando a preservação das evidências originais. Os testes de interface usam os novos nomes nos fluxos existentes.

Validação concluída: os seis checks obrigatórios do frontend passaram, com **1.356 testes em 90 arquivos**, sem pendências de formatação, tipos, lint ou scanner do design system. A suíte completa do backend passou com **730 testes**, incluindo os dois novos testes da migration. O scanner de segredos e `git diff --check` também passaram. O build mantém os avisos já existentes de tamanho de chunk e tempo de plugins. A confirmação visual autenticada em navegador não foi realizada nesta entrega.
