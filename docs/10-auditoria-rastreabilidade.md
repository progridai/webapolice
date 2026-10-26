# Auditoria Persistida e Rastreabilidade

## Objetivos
Esta documentação visa alinhar a arquitetura técnica adotada para gravação persistente de eventos de auditoria no sistema WebApolice. 
Seu principal objetivo é garantir que todas as ações relevantes possuam um rastro claro e seguro sobre o autor (Quem), a ação (O que), o instante (Quando), e o contexto de rede/aplicação (Onde).

## Arquitetura
O módulo de auditoria encontra-se fisicamente no projeto `WebApolice.Auditoria`, operando como um módulo técnico (não de negócio). Ele não deve ser instanciado ou acessado diretamente pelo domínio.

Fluxo: `API / Módulo de Negócio -> IRegistradorAuditoria -> AuditoriaDbContext (PostgreSQL)`

## Estrutura do Dado (Modelo)
Os dados persistidos na tabela `auditoria.registros_auditoria` cobrem as seguintes colunas obrigatórias e rastreáveis:
- **id**: Identificador sequencial.
- **data_hora_utc**: Timestamp absoluto e técnico da ocorrência.
- **usuario_id_externo**: Identificador Keycloak (claim `sub`), pois o token JWT é a fonte da verdade para a identidade.
- **acao**: (ex: criar, atualizar, excluir, aprovar).
- **modulo**: Qual parte do monolito iniciou a ação.
- **recurso** / **recurso_id**: A entidade que foi alvo da ação (ex: modulo "clientes", recurso "cliente", recurso_id "123").
- **dados_anteriores** e **dados_posteriores**: Colunas `jsonb` de profundidade limitada, preenchidas manualmente ou via objetos serilizados que excluem referências cíclicas e proxies do EF Core.

## Diferença entre Logs, Auditoria e Histórico
1. **Logs Técnicos**: Mensagens puramente operacionais (`ILogger`), muitas vezes efêmeras, enviadas ao console, arquivos ou APM. Foco: diagnóstico técnico.
2. **Auditoria (Este módulo)**: Eventos persistidos relacionalmente visando conformidade, rastreabilidade de quem fez e responsabilização. Acesso restrito a relatórios.
3. **Histórico de Negócio**: Mudanças cruciais de estado inerentes a uma entidade (ex. "Troca de Corretor de Seguros"), que serão persistidas no próprio módulo em uma tabela como `historico_apolice` quando a regra de negócio exigir. A auditoria não substitui esse fluxo.

## Segurança e Rejeição de Dados Sensíveis
Ao interagir com o `IRegistradorAuditoria`, há um estágio estrito de validação (`ProvedorMascaramento.cs`) que procura proativamente por chaves JSON conhecidas como segredos (e.g. `senha`, `password`, `token`, `secret`, `authorization`, `numero_cartao`, `cvv`, `chave_privada`, `api_key`).
Caso encontradas (inclusive aninhadas ou em arrays), o registro é **rejeitado integralmente** e uma exceção `ValidacaoAuditoriaException` é lançada. Segredos nunca devem ser persistidos, nem mesmo com valor mascarado. 
Dados pessoais que necessitem mascaramento ou minimização serão controlados e filtrados manualmente pelo domínio antes de enviar para auditoria; o módulo técnico apenas age como última barreira para credenciais.

## Transações
Operações que exigem atomicidade devem usar o coordenador existente `OperacaoAuditada`, por uma porta do módulo de negócio. A implementação de Infrastructure compartilha a conexão e transação física entre o DbContext de negócio e AuditoriaDbContext; só confirma após as duas gravações. Não confirmar negócio antes de registrar auditoria, nem tentar compensar um commit com rollback posterior. Propagar CancellationToken até as gravações e confirmar rollback em nova conexão de teste.

Na consolidação de 23/10/2026, esse fluxo foi aplicado às quatro escritas de Clientes, Cooperados, Estipulantes, habilitação de módulo/recurso e criação/adição de item de Propostas. Outros CRUD de Cadastro ainda precisam uniformização (Seguradoras, Corretoras, Subestipulantes e Módulos); a existência do coordenador não comprova cobertura de todos os escritores.

A suíte ClienteAuditadoPostgresTests cobre sucesso e reversão por falha/cancelamento da auditoria, mas sua execução nesta consolidação falhou ao inicializar Testcontainers por indisponibilidade do Docker Engine. Não há nova comprovação de atomicidade física. Transações locais também não tornam operações externas do Keycloak atômicas: compensações e falhas remotas precisam homologação específica. Ver resultados e limites na revisão 76.

## Limitações Atuais
- Não implementamos purge automático ou particionamento de tabelas.
- O IP usa o request após forwarded headers restritos aos proxies configurados. A lista de confiança e a borda implantada precisam homologação; nunca confiar em X-Forwarded-For arbitrário.
- Não existe frontend / painel administrativo construído para exibir os eventos gravados (apenas via acesso de banco de dados).


## Continuidade — 26/10/2026

O Docker foi iniciado e as regressões físicas de Clientes (sucesso, falha/cancelamento da auditoria), Auditoria e Propostas foram executadas e aprovadas em PostgreSQL 18.4 efêmero. As verificações de rollback consultam o estado em nova conexão. O teste de histórico funcional de vínculo de Ramo voltou a gravar o evento na mesma chamada de SaveChanges do vínculo: o workaround antigo que o desabilitava não se aplica ao schema migrado. Isso não encerra a pendência de uniformizar os demais escritores mencionados acima nem cria atomicidade distribuída com o Keycloak. O documento 79 registra os resultados completos.
