# Saneamento de Telefones de Clientes

## Versão 2 — Separação Parcial

A V2 substitui a regra de aceitar somente origens integralmente reconhecidas. Usa as origens ainda ativas que estavam em revisão na V1, sem reescrever as evidências históricas. Não executar novamente a aplicação/reversão V1 após a V2; reverter a V2 antes de qualquer reversão anterior.

`dotnet run --project backend/tools/TelefonesSaneamento -- --self-test` executa 20 cenários locais. Para a operação, usar `-- --v2` seguido de `v2-aplicar.sql`, `v2-verificar.sql` e `v2-pendencias.sql` (caminhos completos relativos à raiz, dentro desta pasta), terminando com `--validate` ou `--apply`. O planejamento e a captura da imagem original são feitos automaticamente sob o mesmo bloqueio/transação.

A V2 associa DDD isolado ao próximo trecho local de 7–9 dígitos, separa delimitadores explícitos e limites existentes entre grupos telefônicos, e reconhece anotações antes/depois de um único número. Não corta uma sequência contínua excedente em números supostos. Não herda DDD para os demais números locais da mesma origem. Trechos incompletos ficam em contatos TELEFONE com o próprio conteúdo e dígitos, sem preenchimento; os completos recebem máscara e tipo TELEFONE/CELULAR. Registros sem dígitos permanecem intactos. Anotações sem números não viram contatos novos: ficam nas evidências. Cada parsing verifica que a concatenação dos dígitos dos trechos de evidência corresponde exatamente à origem.

Registros já isolados e inalterados mantêm o próprio ID. Os compostos substituídos ficam inativos, sem mudar o valor original. Destinos completos deduplicam por pessoa/tipo/número nacional; incompletos pelo valor exato preservado. Há correspondência para cada ocorrência, inclusive incompleta. Mantém a escolha de telefone principal independentemente do e-mail principal; havendo transferência, prefere um destino completo.

As tabelas `legado.telefone_saneamento_v2`, `telefone_saneamento_destino_v2` e `telefone_saneamento_alteracao_v2` mantêm propostas, situações técnicas, correspondências e imagens antes/depois. Os únicos campos textuais novos são códigos técnicos, números e evidências originais; não há novos campos humanos de negócio. A view `legado.telefone_saneamento_pendencia_v2` expõe os contatos INCOMPLETO/SEM_NUMERO; a view operacional V1 exclui as origens já tratadas pela V2, conservando as tabelas históricas.

Para testar a reversão, usar `v2-reverter.sql` após verificação e terminar com `--validate`. Para reverter uma aplicação confirmada, executar apenas esse script no modo SQL normal com `--apply`, sem `--v2`; a reversão exige que os contatos não tenham edições posteriores. As evidências do plano permanecem. Uma nova execução V2 processa zero origens já analisadas; novas regras exigem nova versão, sem reescrever JSONs originais.

Os relatórios agregados da V2 estão em `backend/.artifacts/saneamento-telefones-v2-2026-10-15`.

Ferramenta operacional independente da aplicação. Destino restrito a `webapolice_teste`; lê a conexão de `CONNECTIONSTRINGS__POSTGRESQLTESTES` ou da chave equivalente em `backend/.env.local`, sem exibi-la. Consultas são somente leitura por padrão. `--validate` habilita escrita seguida de rollback; `--apply` confirma a transação. As credenciais nunca devem ser passadas nos argumentos.

## Regras da Versão 1

- Examina apenas contatos ativos TELEFONE/CELULAR de pessoas vinculadas a clientes; não altera e-mails, pessoas, endereços ou mapas da importação.
- Separa barras, ponto e vírgula e barra vertical; ignora separadores vazios. Espaços e hífens são máscaras, não limites presumidos entre números.
- Aceita números nacionais completos com DDD brasileiro válido: fixos convencionais de oito dígitos iniciados em 2–5 e celulares de nove dígitos iniciados em 9.
- Reconhece prefixos legados 0/00 antes do DDD e DDI brasileiro 55/0055 quando o comprimento é compatível. Sinal `+` exige `+55`. Havendo mais de uma interpretação válida, exige revisão. A convenção 00+DDD corresponde ao padrão da carga legada, não a um discador internacional genérico.
- Não acrescenta DDD, nono dígito, prefixos de fixos antigos nem remove zeros finais. Não infere WhatsApp, titularidade ou existência da linha.
- Só aplica uma origem se **todos** os seus trechos forem inequívocos. Casos parciais ficam integralmente preservados e ativos, com propostas e pendências disponíveis no banco.
- Fixo mantém tipo TELEFONE; celular usa CELULAR, ambos já aceitos pelo formulário. Valor recebe máscara nacional e `valor_normalizado` contém DDD+número, conforme o contrato existente da aplicação. A classificação FIXO/CELULAR também fica explícita no mapa operacional.
- Deduplica por pessoa, tipo e número; não mescla pessoas. Conserva registros originais como inativos, sem mudar seus valores. Transfere principal somente quando a origem era principal e não existe outro principal ativo; escolhe o primeiro número da primeira origem principal.
- Não reprocessa contatos gerados ou origens já analisadas. Mudanças posteriores a uma origem pendente exigem uma nova versão/revisão explícita.

## Evidências e Classificação de Campos

As tabelas `legado.telefone_saneamento_v1`, `legado.telefone_saneamento_destino_v1` e `legado.telefone_saneamento_alteracao_v1` guardam origem, propostas, correspondências e imagens antes/depois. JSONs e trechos originais são evidências imutáveis; números, motivos, classificação e situação são valores técnicos. Não há novos campos humanos de negócio nem necessidade de ampliar as listas de Title Case. Não modificar os mapas históricos da carga. A view `legado.telefone_saneamento_pendencia_v1` expõe os casos para revisão, sem transformá-los.

## Execução

Na raiz do repositório, usar `dotnet run --project backend/tools/TelefonesSaneamento --` seguido dos caminhos de scripts na ordem:

1. `backend/tools/TelefonesSaneamento/plano.sql`
2. `backend/tools/TelefonesSaneamento/testes.sql`
3. `backend/tools/TelefonesSaneamento/aplicar.sql`
4. `backend/tools/TelefonesSaneamento/pendencias.sql`
5. `--validate` para simular ou `--apply` para persistir.

Para testar a reversão, incluir `reverter.sql` depois de `aplicar.sql` e usar `--validate`. Para reverter uma aplicação confirmada, executar somente `reverter.sql --apply`: a reversão rejeita qualquer contato editado posteriormente, remove os números criados e restaura os flags dos registros anteriores. As evidências das origens permanecem. As sequences podem avançar mesmo em rollback.

Usa uma transação com bloqueio de escrita em `core.pessoa_contato`, advisory lock e limite de espera de 15 segundos por bloqueio. Leituras permanecem permitidas. Falhas revertem a transação inteira. Reexecução de plano/aplicação deve inserir zero origens e zero destinos; revisar o relatório antes de adotar novas regras.

Relatórios agregados ficam em `backend/.artifacts/saneamento-telefones-2026-10-15`. Não exportar dados pessoais para documentação ou logs. O diagnóstico não prova que números estejam em uso.
