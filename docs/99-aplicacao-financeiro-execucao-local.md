# Aplica??o do Ciclo Financeiro e Execu??o Local

Data: 18/11/2026. Executada ap?s autoriza??o expressa para aplicar as migrations e disponibilizar backend/frontend locais para teste.

## Banco e Migrations

A configura??o padr?o da API foi comparada ? conex?o espec?fica de testes, ambas apontando para **webapolice_teste**, PostgreSQL 17.11. O contexto Seguro estava no baseline `20261118103000_IncluirConjugeProposta`. A API anterior foi interrompida antes do backup e da instala??o.

Aplicadas exclusivamente:

1. `20261118140000_IdentidadeMovimentoNativo`.
2. `20261118194921_CicloFinanceiroPropostas`.

O SQL EF idempotente foi reunido em uma transa??o com guarda de banco/baseline, lock timeout de 10 segundos, statement timeout de 120 segundos e bloqueio das tabelas Seguro existentes. As assinaturas anteriores/posteriores compararam **28 tabelas e 47 registros**, excluindo somente as cinco novas colunas t?cnicas de movimento e as duas entradas esperadas do hist?rico EF. Os demais campos foram preservados. As tabelas financeiras novas permaneceram vazias; n?o houve cobran?a presumida nem preenchimento de UUIDs em movimentos legados.

Backup custom de estrutura e dados do Seguro: `scratch/migrations-financeiro-local-20261118/seguro-anterior.dump`, **154.397 bytes**, SHA-256 `058C00B3756D9ABE33DBE099555A25AB41216811FD4CEB3E58951781C27F0653`. Cat?logo conferido com pg_restore. Arquivos de backup, SQL, instalador e logs cont?m evid?ncias locais e permanecem ignorados pelo Git; n?o s?o inclu?dos em commits.

Uma nova conex?o somente leitura confirmou as duas migrations e as 28 colunas consultadas de plano financeiro, compet?ncia e novos metadados de movimento. O EF confirmou **35 migrations aplicadas e zero pendentes** no contexto Seguro. N?o foi necess?rio reaplicar triggers de Title Case: n?o foram adicionados campos humanos nem alterados textos de neg?cio. Produ??o n?o foi alterada.

## Servi?os Dispon?veis

- Frontend: `http://127.0.0.1:5173/`, Vite existente do pr?prio workspace mantido em execu??o, PID 37460.
- API atualizada: `http://127.0.0.1:5007/`, processo dotnet PID 33772, iniciado em segundo plano com janela oculta e configura??o Development do arquivo local n?o versionado.
- Sa?de da API: `http://127.0.0.1:5007/api/health/ready`, HTTP 200, estado Healthy e PostgreSQL Healthy.
- Vers?o da API: endpoint p?blico confirmou WebApolice.Api em Development.

O frontend respondeu HTTP 200, com cliente Vite e m?dulo de lote mensal atual. O conte?do original no sourcemap foi comparado ao arquivo local e corresponde integralmente; a configura??o servida aponta para a API `127.0.0.1:5007`. A rota de parcelas da Ap?lice respondeu HTTP 401 sem token, confirmando a exig?ncia de autentica??o. O login usa o provedor Keycloak j? configurado, sem modificar realm, usu?rios ou permiss?es.

Logs de inicializa??o: `scratch/migrations-financeiro-local-20261118/api.stdout.log` e `api.stderr.log`. Evid?ncias de instala??o: `instalacao.log`, `aplicar-verificado.sql`, `verificar.sql`, `backup-catalogo.txt` e `ef-migrations.json` no mesmo diret?rio. PIDs registram esta execu??o e podem mudar ap?s rein?cio.

## Limites da Verifica??o

Foram verificadas implanta??o, integridade dos registros anteriores, consist?ncia do hist?rico EF, sa?de do banco/API, c?digo servido pelo frontend e exig?ncia de autentica??o. N?o foi realizado login no navegador nem criada massa financeira para simular a??es do usu?rio. Os testes automatizados do ciclo est?o registrados no [documento 98](98-lotes-mensais-baixa-parcelas.md). A homologa??o autenticada ser? feita pelo usu?rio no ambiente disponibilizado. Nenhum c?digo de produto foi alterado nesta instala??o; n?o houve commit, push, publica??o de produ??o ou opera??o banc?ria.
