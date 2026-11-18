# Aplicação das Migrations de Propostas

Data: 18/11/2026. Executada após autorização para aplicar as migrations necessárias e sincronizar o desenvolvimento com o Git.

## Ambiente e Escopo

A conexão padrão da API local coincide com `ConnectionStrings__PostgreSqlTestes`, do arquivo local não versionado. O destino conferido foi **webapolice_teste**, servidor PostgreSQL 17.11. Produção não foi alterada nesta execução. O contexto Seguro estava no baseline `20261112194300_DefinirDescartadaProposta`.

Aplicadas exclusivamente as três migrations pendentes do contexto Seguro:

1. `20261117120000_NomePreProposta`: nome de negócio da situação inicial passa a Pré Proposta; código técnico RASCUNHO preservado.
2. `20261117180000_CondicoesPropriasProposta`: booleano obrigatório `condicoes_proprias`, padrão falso.
3. `20261118103000_IncluirConjugeProposta`: booleano anulável `inclui_conjuge`, sem padrão ou preenchimento presumido.

## Backup e Execução

Foi criado backup custom do schema Seguro e seus dados com o cliente pg_dump 17, compatível com o servidor, e conferido o catálogo com pg_restore. Arquivo local ignorado pelo Git: `scratch/migrations-propostas-20261118/seguro-anterior.dump`, 154.231 bytes. SHA-256: `63400A6C97891CBD1033826A592498FCCA30A843EC32A295A0612759D2DBAE3C`. O backup contém dados locais e não foi incluído no commit.

O SQL foi gerado pelo Entity Framework 10.0.11, limitado ao intervalo entre o baseline conferido e `IncluirConjugeProposta`, sem atualizar outros contextos. A aplicação reuniu as três migrations em uma transação, com verificação explícita do banco/baseline, lock timeout de 10 segundos e statement timeout de 120 segundos. As tabelas operacionais foram bloqueadas durante a comparação das assinaturas anteriores e posteriores.

As assinaturas conferiram propostas, itens, coberturas, beneficiários, histórico, movimentos e situações. Somente as novas colunas e o nome aprovado da situação inicial foram excluídos da comparação; os demais campos permaneceram idênticos. Antes do commit transacional, foram exigidos padrão falso, ausência de escolhas presumidas de cônjuge, nome Pré Proposta e as três entradas no histórico EF. Qualquer diferença recusaria a instalação com rollback integral. O SQL gerado e o envelope de verificação permanecem junto ao backup no diretório local ignorado.

## Resultado Verificado

Uma nova conexão somente leitura confirmou:

- Três migrations presentes no histórico do contexto Seguro.
- `condicoes_proprias`: boolean, NOT NULL, default false.
- `inclui_conjuge`: boolean, nullable, sem default.
- Uma proposta preexistente preservada, com condições próprias falsas e contratação de cônjuge não informada.
- Situação técnica RASCUNHO com nome Pré Proposta.

`dotnet ef migrations list --json`, usando a conexão específica de testes, confirmou **zero migrations pendentes no contexto Seguro**. Não foi necessário reaplicar triggers de Title Case: as colunas novas são técnicas e a alteração de catálogo utiliza campo humano já protegido.

As suítes aprovadas imediatamente antes desta instalação cobriram 783 testes de backend e 1.381 de frontend, além dos seis checks obrigatórios, build backend sem avisos/erros, scanner local de segredos e diff check. O pull de origin/main informou Already up to date, sem alterações de código posteriores a esses checks; as alterações desta instalação são de documentação. O parser PowerShell do instalador de atalhos também foi conferido para inclusão no Git.

Não houve publicação, reinício de serviços, geração financeira ou alteração de perfis/permissões. A homologação visual autenticada permanece pendente. A instalação anterior dos atalhos em testes/produção está no [documento 90](90-correcao-login-migration-atalhos.md) e corresponde a outra execução.
