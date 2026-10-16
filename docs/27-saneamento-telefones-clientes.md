# Saneamento de Telefones de Clientes — 15/10/2026

Este relatório descreve a primeira etapa. Os 108.693 registros então pendentes já foram tratados pela [separação parcial V2](28-separacao-parcial-telefones-clientes.md), incluindo fragmentos incompletos. As contagens abaixo são históricas da V1.

Saneamento autorizado pelo usuário e **confirmado em `webapolice_teste`**. Não houve alteração no código da aplicação. A ferramenta operacional está em [TelefonesSaneamento](../backend/tools/TelefonesSaneamento/README.md).

## Resultado Confirmado

| Resultado | Quantidade |
|---|---:|
| Contatos telefônicos originais analisados | 316.589 |
| Origens saneadas e mantidas como histórico inativo | 207.896 |
| Contatos individuais criados, após deduplicação por pessoa | 315.981 |
| Fixos classificados como TELEFONE | 127.016 |
| Celulares classificados como CELULAR | 188.965 |
| Origens preservadas integralmente para revisão | 108.693 |
| E-mails preservados | 205.789 |

Os novos registros têm máscara nacional e `valor_normalizado` com DDD+número, compatível com o cliente HTTP e o backend existentes. A classificação FIXO/CELULAR também fica registrada no mapa operacional. Não se deve interpretar os 235.709 contatos TELEFONE ativos como total de fixos saneados: esse número inclui as 108.693 origens pendentes, ainda preservadas no estado anterior.

As origens só foram substituídas quando **todos** os trechos eram inequívocos. Números locais, formatos antigos, anotações e sequências excedentes não foram completados por suposição. O DDD não foi inferido do endereço, nem foi acrescentado nono dígito. Não houve atribuição de WhatsApp ou comprovação de titularidade/linha ativa.

## Pendências

A view `legado.telefone_saneamento_pendencia_v1` fornece origem, pessoa, trecho original e motivo técnico. Contagens abaixo são de trechos; uma origem pode ter vários motivos e não deve ser somada entre categorias.

| Motivo técnico | Trechos |
|---|---:|
| FORMATO_OU_NUMERACAO_INDETERMINADA | 65.658 |
| SEM_DDD_OU_NUMERACAO_ANTIGA | 62.485 |
| VAZIO | 11.172 |
| ZEROS_OU_PREENCHIMENTO | 9.129 |
| ANOTACAO_OU_TEXTO | 6.334 |

Os casos parcialmente reconhecidos também continuam pendentes: suas propostas estão no JSON de evidência. Uma nova rodada exige regras específicas, nova versão e conferência; não editar a evidência da versão 1.

## Rastreabilidade e Verificação

- `legado.telefone_saneamento_v1`: imagem original, propostas e situação por origem.
- `legado.telefone_saneamento_destino_v1`: correspondências origem/destino/número/classificação.
- `legado.telefone_saneamento_alteracao_v1`: imagens antes/depois das 523.877 alterações, incluindo 315.981 inserções.

Os textos originais permanecem como evidências, sem Title Case ou outra transformação. As novas colunas são técnicas ou de evidência, sem novos campos humanos de negócio. Os mapas da importação anterior e a auditoria não foram reescritos.

Foram aprovados 15 testes de classificação, conferência das origens, destinos e pendências, além da reversão exata de todas as alterações em transação de simulação. A aplicação definitiva terminou com `COMMIT`. A reexecução posterior confirmou zero origens e zero números processados, sem duplicações, e aprovou novamente a conferência dos dados gravados. A reversão operacional rejeita contatos editados após a aplicação para não apagar mudanças posteriores.

Relatórios agregados, sem nomes nem telefones: `backend/.artifacts/saneamento-telefones-2026-10-15/validacao-reversao.txt`, `aplicacao.txt` e `reexecucao.txt`. Os artefatos locais não são versionados.
