# Consulta de propostas pelo vínculo do Cliente

Entrega concluída em 19/11/2026.

No detalhe do Cliente, a seção Vínculos oferece **Abrir propostas do vínculo {matrícula}**. A janela lista somente as propostas daquele Cliente e vínculo. O usuário escolhe a versão em Consultar e pode voltar à lista. A consulta inclui o histórico operacional visível, inclusive quando o vínculo está inativo, e identifica Apólice, situação e versão. Não escolhe uma versão automaticamente: o mesmo vínculo pode ter várias propostas e Apólices.

A ação requer módulo APOLICES, recurso PROPOSTAS e permissão propostas.visualizar. Não exige clientes.alterar. As ações de manutenção da proposta continuam sujeitas às permissões próprias. O fechamento da janela é impedido enquanto o detalhe informa uma gravação em andamento.

## Contrato e isolamento

GET /api/propostas recebe os filtros opcionais clientePublicId e clienteVinculoPublicId. Quando utilizados, ambos são obrigatórios, devem ser UUIDs não vazios e identificar um vínculo pertencente ao Cliente informado. Contextos parciais, inexistentes ou incompatíveis retornam validação; nunca são tratados como consulta geral. Chamadas existentes sem os novos filtros preservam o comportamento anterior.

Cadastro resolve os UUIDs pela porta IClientesPropostas; Seguro aplica os dois identificadores internos à consulta antes de contar e paginar. Não há leitura direta das tabelas de Cadastro pelo módulo Seguro. Busca por número e paginação preservam o contexto. A API mantém autorização de propostas.visualizar e disponibilidade do catálogo.

A janela reutiliza PropostasConsulta, PropostaDetalhes, Modal e useConsulta. Carregamento, erro com nova tentativa e ausência de propostas usam os estados existentes. Fechar ou trocar de vínculo cancela a consulta anterior e descarta respostas antigas. Os componentes usam o design system e foram exercitados nos temas claro, escuro e sistema.

## Dados e arquivos

Nenhum modelo persistido ou coluna foi criado; não há migration nem necessidade de reaplicar triggers. clientePublicId e clienteVinculoPublicId são UUIDs técnicos de consulta. Nomes, matrículas, estados e evidências preservam as classificações e os normalizadores existentes.

Frontend: ClienteDetalhePage e ClienteVinculosSection adicionam a ação; PropostasConsulta, propostas.api e proposta.types encaminham os filtros. Backend: ObterPropostasQuery e PropostasRepository filtram; IClientesPropostas e ClientesPropostas resolvem o contexto. Testes cobrem o fluxo da janela, transporte HTTP, autorização e persistência PostgreSQL.

## Validação

Os seis checks obrigatórios do frontend passaram: formatação, tipos, lint, scanner do design system, 1.432 testes em 102 arquivos e build. Nenhum erro/aviso de lint ou ocorrência do scanner. O build mantém o aviso preexistente de chunk principal acima de 500 kB (505,54 kB), sem flexibilização da configuração. Backend compilado com zero erros e avisos; 824 testes passaram nas seis suítes, incluindo isolamento de matrículas, versões, consulta de vínculo inativo e rejeição de contexto inválido.

As evidências da execução local ficam em scratch/vinculo-propostas-frontend-validation.log, scratch/vinculo-propostas-backend-build.log e scratch/vinculo-propostas-backend-tests.log. A homologação autenticada do fluxo no navegador permanece a cargo do usuário.
