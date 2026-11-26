# Homologação Parcial do Manual e Correções de Cadastros

Execuções em 25/11/2026. Registro consolidado em 26/11/2026, antes do commit solicitado pelo responsável. Continuidade do [roteiro 105](105-roteiro-homologacao-antes-manual.md) e do [índice funcional 106](106-indice-funcional-atual.md).

## Ambiente e Alcance

A conexão de navegador foi estabelecida por Playwright MCP no Codex do VS Code. O responsável realizou o login na interface. O frontend local foi acessado em `http://127.0.0.1:5173/`; a API foi conferida em `Development`, com banco `webapolice_teste`. Foram utilizados cadastros fictícios identificados como Homologação Manual, preservando os dados existentes e a Auditoria.

A homologação está em andamento. Este registro não aprova todos os cenários H01–H15 nem transforma o manual em uma edição final homologada.

## Correções Encontradas e Aplicadas

### Datas Civis do Estipulante

Ao cadastrar início em 01/01/2026 e fim em 31/12/2027, os detalhes mostraram 31/12/2025 e 30/12/2027. A resposta da API preservava `2026-01-01` e `2027-12-31`; a divergência estava na conversão local de `new Date(...).toLocaleDateString(...)` em um fuso negativo.

A tela passou a reutilizar `formatarDataOuVazio`. O teste de regressão reproduziu a falha no fuso `America/Sao_Paulo` antes da correção e passou depois. As datas corretas foram conferidas novamente no navegador. Não houve transformação de datas gravadas ou reprocessamento do histórico.

### Grupo nas Consultas de Estipulantes

A coluna Grupo ainda aparecia na listagem, embora o campo já tivesse sido retirado dos formulários e detalhes. Foram removidas a coluna desktop e a apresentação móvel. Os contratos necessários à preservação dos vínculos históricos foram mantidos. Testes nos três temas verificam que o registro continua apresentado sem expor o Grupo.

### Busca de Seguradora pelo Nome

Buscar `icatu` não retornava o cadastro `Icatu`, embora ele aparecesse na consulta sem filtro. A consulta comparava a busca em minúsculas com uma chave de nome que pode estar em maiúsculas. A comparação passou a considerar a caixa de forma consistente e também o nome cadastrado, inclusive quando a chave normalizada estiver ausente.

Oito testes em PostgreSQL efêmero cobrem diferentes caixas, acento, chave ausente, código, SUSEP, documento, situação, exclusão, paginação e preservação da chave original. Após a ativação local da API corrigida, a pesquisa pela interface retornou HTTP 200 e um resultado para `icatu`. Não foram alteradas chaves normalizadas, classificação de campos humanos, tabelas ou migrations.

## Evidências Obtidas

| Área | Conferência executada | Alcance restante |
| --- | --- | --- |
| Início e menu | Temas claro, escuro e sistema em desktop; abertura/cancelamento da personalização; menu móvel em 390 × 844 abre, navega para Ramos e fecha sem transbordamento horizontal | Persistência de novas preferências e comparação com Perfil restrito |
| Ramos | Visualizar abre o registro correto, exibe dados e ações e permite voltar; conferência nos três temas | Acesso restrito e demais combinações de dispositivo |
| Estipulantes | Criação fictícia com Seguradora; nome padronizado; remoção salva e confirmada; troca de Seguradora salva e confirmada; datas corretas após a correção | Busca corrigida no seletor após a ativação e cenário com Perfil sem consulta |
| Clientes | Criação de Cliente fictícia, nome padronizado e nascimento apresentado corretamente; salvamento do vínculo com matrícula | Reabrir e conferir a matrícula após a recarga; investigar a observação preenchida que não apareceu nos detalhes |
| Apólices | Criação de Apólice fictícia, abertura dos detalhes e acesso à aba Módulos | Vincular Módulo, preparar Plano/Coberturas e Vida e executar Propostas/financeiro |
| Falha e nova tentativa | Uma falha de conexão foi exibida nos detalhes do Estipulante; Tentar novamente recuperou o cadastro e mostrou a Seguradora salva | Falhas controladas e concorrência dos demais fluxos |
| Manual HTML | Visualização desktop e móvel; busca por MATRÍCULA sem acento/caixa encontrou 17 seções; link levou à seção e recebeu foco; menu móvel abriu e fechou após navegar; sem transbordamento horizontal nas larguras conferidas | Impressão paginada e tecnologia assistiva |

As capturas publicadas no manual apresentam o Ramo e o Estipulante fictício. Imagens de trabalho e snapshots completos permanecem em `scratch/`, fora do versionamento. A geração HTML foi ajustada para renderizar links dentro de negrito e figuras com legenda e texto alternativo.

## Verificações Automatizadas

- Os seis checks obrigatórios do frontend passaram: formatação, tipos, lint sem erros/avisos, scanner sem ocorrências, 1.468 testes em 104 arquivos e build. Foi mantido o aviso preexistente de chunk acima de 500 kB.
- O conjunto direcionado de Estipulantes passou com 17 testes no fuso de São Paulo, incluindo regressão de datas e consultas sem Grupo nos três temas.
- Backend compilado com zero erros e avisos; 837 testes passaram: 6 Auditoria Unit, 2 Auditoria Integration, 73 Cadastro, 28 Arquitetura, 173 API e 555 Integration, sem testes ignorados.
- A primeira execução completa do backend encontrou restrições do sandbox para Docker e Event Log. A execução seguinte, com o acesso necessário, passou sem desativar verificações. A correção de tipos de uma propriedade indevida no novo teste móvel também precedeu a aprovação final dos checks.

Os resultados acima correspondem ao código conferido em 25/11/2026; as alterações posteriores até este registro foram documentais. Novas alterações de produto exigem novamente as verificações pertinentes.

## Ativação Local e Ponto de Retomada

A API corrigida foi publicada em `bin/ExecucaoHomologacao-20261125`, validada na porta 5008 e ativada na 5007. Os hashes de `appsettings.json` e `appsettings.Development.json` permaneceram iguais aos da publicação anterior. A saúde respondeu HTTP 200/Healthy e a instância temporária foi encerrada. Não houve implantação em produção.

A última tentativa de abrir Vincular Módulo não foi executada: a revisão automática atingiu o limite de uso, sem concluir a avaliação. Na retomada de 26/11, o navegador estava disponível, mas a sessão havia expirado; foi aberta a tela de login para o responsável. Antes de continuar, foi solicitado commit e push das alterações pendentes.

Retomar na Apólice fictícia, aba Módulos, depois do login. A rotina ainda não gerou Propostas, parcelas ou baixas fictícias nesta homologação. Os cenários financeiros e de transição continuam pendentes, assim como a conferência com Perfil restrito.
