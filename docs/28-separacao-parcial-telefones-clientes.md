# Separação Parcial de Telefones — 15/10/2026

Segunda etapa autorizada pelo usuário: separar os números reconhecidos e também os fragmentos incompletos, preservando os dígitos e sem inventar DDD ou prefixos. **Aplicação confirmada com COMMIT em `webapolice_teste`.** Não houve mudança na aplicação.

## Resultado

| Operação V2 | Quantidade |
|---|---:|
| Origens da V1 examinadas | 108.693 |
| Trechos vinculados a destinos | 193.579 |
| Origens compostas/reformatadas substituídas por destinos | 71.532 |
| Contatos novos criados | 147.051 |
| Alterações com imagens antes/depois | 219.536 |

Destinos distintos por classificação da V2, incluindo registros já existentes reaproveitados:

| Classificação | Contatos |
|---|---:|
| Celular completo | 48.555 |
| Fixo completo | 32.701 |
| Incompleto | 94.102 |
| Sem número, preservado | 11.624 |

Essas contagens não são todas inserções: os destinos podem ser reaproveitados de registros isolados anteriores ou da V1. A quantidade efetivamente inserida nesta etapa é 147.051. As quantidades de vínculos são maiores porque um mesmo destino pode corresponder a várias ocorrências originais.

## Comportamento

- Cada trecho é tratado individualmente. DDD isolado antes de barra é associado somente ao próximo trecho local de 7–9 dígitos; não é propagado aos demais números.
- Números separados por delimitadores explícitos e por limites reconhecíveis entre grupos são separados; sequências contínuas excedentes não são cortadas por suposição.
- Números completos recebem máscara nacional e tipo TELEFONE/CELULAR; incompletos ficam separados com o conteúdo existente, sem adicionar DDD, nono dígito ou remover zeros finais. Tipo TELEFONE em um fragmento não significa classificação confirmada como fixo.
- Anotações permanecem no original e na evidência de cada trecho; textos isolados sem número não viram novos contatos. Origens inteiramente sem dígitos permanecem intactas.
- Quando o contato já estava isolado e inalterado, o ID é mantido. Origens substituídas ficam inativas, sem reescrever os valores originais. A transferência de telefone principal respeita a existência independente de e-mail principal.

O caso de controle do cliente 11 foi consultado após COMMIT: os quatro números completos estão ativos em registros individuais, enquanto o quinto número local permanece em seu registro original, classificado como incompleto, sem DDD presumido.

## Evidências e Verificações

Os planos, destinos, situações e imagens antes/depois estão em `legado.telefone_saneamento_v2`, `legado.telefone_saneamento_destino_v2` e `legado.telefone_saneamento_alteracao_v2`. As tabelas da V1 não foram reescritas. A view operacional de pendências V1 exclui origens já tratadas pela V2; a nova `legado.telefone_saneamento_pendencia_v2` expõe os incompletos e os registros sem número.

Os novos campos são números, códigos técnicos ou evidências originais/JSON. Não foram introduzidos campos humanos de negócio; não há alteração nas listas compartilhadas de Title Case.

Foram aprovados 20 cenários do parser, conservação de todos os dígitos das evidências nos 108.693 planos, correspondência de cada trecho com destino ativo, preservação das imagens originais e do telefone principal, além da reversão exata de 219.536 alterações em simulação. A reexecução após COMMIT confirmou zero origens lidas, zero trechos processados e zero novas substituições, com verificação aprovada.

Os relatórios agregados estão em `backend/.artifacts/saneamento-telefones-v2-2026-10-15/validacao-final.txt`, `aplicacao.txt` e `reexecucao.txt`. Os artefatos locais não são versionados. As instruções de execução/reversão estão no [README da ferramenta](../backend/tools/TelefonesSaneamento/README.md).

Não executar a aplicação/reversão V1 após esta etapa. Reverter primeiro a V2, se necessário. A reversão verifica se os contatos foram editados posteriormente e bloqueia o procedimento automático nesse caso. Números estruturalmente completos não constituem confirmação de linha ativa, titularidade ou WhatsApp.
