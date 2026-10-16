# Nova extração oficial de clientes — 15/10/2026

O usuário restaurou a base antiga atualizada e definiu a extração de `backend/.artifacts/carga-oficial-2026-10-15` como origem desta carga. Confirmou **webapolice_teste** como destino. Os arquivos e relatórios anteriores permanecem históricos.

## Conferência da nova origem

- 530.056 cadastros de clientes.
- 484.648 registros candidatos, antes de unificação.
- 45.408 ignorados por CPF oficial ausente; os 855 sem nome já fazem parte desse grupo.
- Nenhum bloqueio nas regras da origem.
- 402.149 grupos candidatos antes da comparação com o destino; não equivale ao total final de pessoas a inserir.
- 8.543 registros com CPF preenchido, mas inválido, permanecem isolados por ID com advertência.
- 31 registros com dados de cônjuge sem nome permanecem com pendência específica; não criar dependente sem nome.

Fonte oficial do CPF: `Cli_CPF_11`. Regras de datas, sexo, maior Cliente, endereços e exclusões seguem o [mapa de migração](24-migracao-clientes-legado.md).

## Auxiliares

| Catálogo | Extraídos | Selecionados para carga |
|---|---:|---:|
| Cidades | 6.185 | 6.185 |
| Bancos | 11 | 11 |
| Lotações | 195.978 | 15.950 usadas pelos clientes candidatos |
| Órgãos SIAPE | 10 | 10 |

Não foram encontrados IDs repetidos nos catálogos nem nomes acima dos limites dos destinos. Todas as referências dos clientes têm registro no respectivo catálogo da origem.

Decisões confirmadas pelo usuário:

- Cadastrar a lotação 3714 como **Lotação não informada**. Ela é referenciada por 31.846 candidatos.
- Preservar lotações com cidades inexistentes deixando apenas `cidade_id` NULL. São 13 códigos de cidade, afetando 342 lotações selecionadas.

O importador de catálogos registra IDs antigos em `legado_id`, usa PKs novas e concilia as referências de cidade pela correspondência legada. Valores originais permanecem nas extrações.

## Rastreabilidade e próximas verificações

`manifesto-carga.json` identifica hashes da origem e destino confirmado. `conferencia-catalogos.json` descreve inconsistências da origem. `analise-origem-01` contém a lista dos ignorados e o plano inicial. `analise-destino-01` registra a situação anterior à carga dos auxiliares: schema compatível e dois registros com divergência em pessoa já existente, além dos bloqueios esperados pelos catálogos então vazios.

Após carregar os auxiliares, repetir a simulação contra o destino e revisar as divergências remanescentes. Cônjuges, contatos, documentos, vínculos e endereços ainda precisam de deduplicação no importador de escrita. A ferramenta atual de clientes é uma simulação; a conclusão da análise não constitui importação de clientes.

## Resultado da carga auxiliar e conferência

Os quatro catálogos foram carregados e conciliados em `webapolice_teste` em uma transação. A reexecução inseriu **zero registros** em todas as tabelas, comprovando a correspondência por `legado_id` para esta extração. Os relatórios são `carga-catalogos-aplicada.json` e `carga-catalogos-reexecucao.json` na pasta da extração.

`analise-destino-02` registra a situação após a carga:

- 119.590 registros prontos para revisão.
- 365.056 com advertências.
- 45.408 ignorados.
- 2 bloqueados, ambos por divergência de pessoa no destino: códigos antigos 96850 e 435698, fonte pessoal prioritária 435698, pessoa/cliente 1 na homologação.

Não restam bloqueios por referência nos clientes candidatos. O resumo de cobertura de lotação inclui também clientes ignorados, cujas lotações sem uso por candidatos não foram carregadas. Uma referência sem cobertura nessa estatística geral não representa novo bloqueio de um cliente candidato.

Não houve importação de clientes. A fonte a prevalecer no cadastro já existente permanece aguardando resposta do usuário.

## Autorização para o cadastro existente

O usuário confirmou que a nova extração oficial deve prevalecer na pessoa/cliente 1, usando o registro 435698. A prioridade foi limitada a essa correspondência; não autoriza substituir outras pessoas divergentes. O simulador passou a aceitar uma autorização explícita por registro de origem/pessoa do destino; seu padrão permanece bloquear divergências não autorizadas.

Foi preparado um [importador de clientes](../backend/tools/ClientesMigrationImporter/README.md) com transação única, mapas por registro antigo, deduplicação dos filhos e backup da pessoa/cliente existente. O piloto de 11 linhas processou 9 grupos e validou mapas e endereços antes de rollback. A carga completa deverá ter resultado registrado em `carga-clientes-01/resultado-carga.json`; não presumir sucesso apenas pela existência dos relatórios de planejamento.

## Carga completa confirmada

A carga completa foi **confirmada em webapolice_teste**. O arquivo `carga-clientes-01/resultado-carga.json` registra `Applied=true`, hash oficial e conciliação:

| Resultado | Quantidade |
|---|---:|
| Registros antigos mapeados | 484.648 |
| Clientes consolidados | 402.149 |
| Clientes antigos ignorados | 45.408 |
| Vínculos distintos | 447.228 |
| Endereços | 333.681 |
| Contatos | 522.378 |
| Documentos RG | 46.472 |
| Dependentes/cônjuges | 30.017 |
| Registros SIAPE | 1.350 |
| Registros Corsan | 1.146 |

Os 31 registros de dados de cônjuge sem nome não geraram dependente; a lista está em `carga-clientes-01/conjuges-nao-importados.json`. A lista de clientes ignorados é `carga-clientes-01/nao-importados.csv`. Os valores originais continuam na extração oficial preservada.

O backup da pessoa/cliente 1 anterior à atualização está em `carga-clientes-01/cadastro-existente-antes.json`, com acesso restrito por conter dados pessoais. A prioridade do registro 435698 foi aplicada sem mudar os IDs/UUIDs existentes.

`verificacao-pos-carga.json` confirmou zero divergências de nome e CPF em relação ao maior Cliente, zero clientes fora de Ativo, zero códigos antigos duplicados, zero falecidos sem óbito ou óbitos sem falecido e zero vínculos inválidos em SIAPE/Corsan. A conciliação transacional também confirmou um endereço padrão por pessoa com endereço ativo. `nome_normalizado` foi armazenado como nome em maiúsculas, com acentos preservados, conforme o contrato de busca atual da aplicação.

A reexecução foi validada: `carga-clientes-reexecucao-01/resultado-carga.json` registra `AlreadyImported=true` e os mesmos 484.648 mapas da extração. Nenhuma reinserção ou atualização foi executada. O relatório final legível é `carga-clientes-01/relatorio-carga.md`.
