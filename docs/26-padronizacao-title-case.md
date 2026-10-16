# Padronização geral dos dados em Title Case

Regra vigente em 15/10/2026, obrigatória para todos os módulos atuais e futuros.

## Comportamento

Todo texto humano de negócio usa inicial maiúscula nas palavras principais e demais letras minúsculas, com cultura pt-BR: `JOÃO DA SILVA` → `João da Silva`, `MARIA D’ÁVILA-SOUZA` → `Maria d’Ávila-Souza`. Preposições, artigos, conjunções e contrações da lista compartilhada permanecem em minúsculas, inclusive no início do campo: `de`, `da`, `dos`, `a`, `o`, `e`, `ou`, `para`, `com`, `pelo`, entre outras. A partícula `d` antes de apóstrofo também fica minúscula. A classificação é lexical, pela lista explícita em C#, TypeScript e SQL, sem análise gramatical de contexto. Siglas dentro de textos humanos seguem a capitalização das palavras principais (`LTDA` → `Ltda`). Espaços, quebras de linha, pontuação, números, vazio e null são preservados. A operação é idempotente.

Aplica-se a nomes, razão social/nome fantasia, descrições, títulos, observações, logradouros, bairros, complementos, departamentos e demais campos classificados em `PadronizacaoTexto.CampoHumano`. A classificação explícita protege strings com significado técnico.

Permanecem nas regras próprias: senhas/tokens, username/sub/roles, e-mails, URLs, arquivos/caminhos/storage keys, códigos externos/internos, CPF/CNPJ/RG, UF, números de contratos/apólices, estados/enums, dados JSON, chaves de busca (`NomeNormalizado`) e campos originais de migração. Auditoria e mapas de migração são evidências históricas imutáveis. Texto digitado em campos de busca não é reformatado.

## Implementação

- `WebApolice.SharedKernel/Text/PadronizacaoTexto.cs`: fonte da política de capitalização e classificação dos campos.
- Middleware da API: normaliza JSON de POST/PUT/PATCH, inclusive estruturas aninhadas, antes do binding e da validação; erros de JSON continuam sendo tratados pelo binding padrão. Não altera query strings nem arquivos.
- `TextoPadronizadoDbContext`: normaliza inserts/updates em SaveChanges síncrono e assíncrono. Todos os contextos de negócio existentes usam essa base; auditoria permanece separada.
- `apps/web/src/services/http/titleCase.ts`: normaliza uma cópia do payload antes de enviar pelo cliente HTTP. A garantia final é do backend/banco. Não depende de transformação visual CSS, nem interfere na posição do cursor durante digitação.
- `backend/scripts/padronizar-title-case.sql`: normaliza registros existentes e instala triggers BEFORE INSERT/UPDATE para cobrir SQL direto, COPY e importadores. Restringe-se aos schemas de negócio e às colunas humanas conhecidas; exclui auditoria e migration maps. Reexecução é idempotente. Em conflito de unicidade ou erro, toda a transação é revertida.

## Aplicação no banco existente

Aplicar o script integralmente na conexão do ambiente de destino durante a implantação, após as migrations. Verificar backup recuperável antes da execução: a capitalização anterior não pode ser inferida após a conversão. O script registra somente tabela e quantidade de linhas alteradas, sem dados pessoais. Para simular, substituir o COMMIT final por ROLLBACK. Confirmar as contagens e eventuais colisões de nomes únicos na homologação antes da produção. Não executar apenas partes do arquivo.

Para instalar ou atualizar a proteção de novas gravações sem normalizar o histórico existente, executar `SET webapolice.title_case_normalizar_existentes = 'off';` na mesma sessão antes do script integral. Esse modo instala as mesmas funções e triggers e não executa os UPDATEs em lote; o padrão continua sendo normalizar os registros existentes. Restaurar a configuração com `RESET webapolice.title_case_normalizar_existentes;` ao terminar. Correções pontuais devem preservar antes uma imagem original do registro. Fazer push não atualiza processos da API em execução: recompilar e reiniciar o processo no ambiente de destino também é necessário.

A função SQL usa a classificação alfabética/capitalização do locale UTF-8 da base. Validar os exemplos com acentos no ambiente de destino. Preservar as colunas de pesquisa normalizadas; elas não recebem Title Case.

## Novas implementações

1. Classificar cada novo campo como texto humano ou técnico.
2. Para campos humanos, atualizar as listas equivalentes em C#, TypeScript e SQL, adicionando testes com caixa alta, caixa mista, acentos, null e idempotência.
3. Herdar os novos contextos de negócio da base compartilhada; não usar DbContext diretamente.
4. Reaplicar o script após migrations que adicionem tabelas/colunas humanas. SQL em lote não passa por SaveChanges e precisa dos triggers.
5. Preservar valores originais de importações em campos próprios, sem reformatação.
