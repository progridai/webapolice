# WebApolice.SharedKernel

Este projeto destina-se a conter exclusivamente tipos e abstrações verdadeiramente transversais do ERP WebApolice que sejam compartilhados por múltiplos domínios e que não pertençam logicamente a um domínio de negócio específico.

> [!CAUTION]
> **Proteção Contra Depósito Genérico (Utility Dump)**:
> Este projeto **NÃO** deve ser utilizado como um repositório de classes utilitárias gerais (como `StringUtils`, `DateTimeUtils`, extensões genéricas ou classes de conversão) que não possuam relação direta com as abstrações e regras centrais compartilhadas de domínio. 
> 
> Abstrações de domínio, validações específicas ou lógicas de infraestrutura de um módulo devem residir dentro do respectivo módulo em `backend/src/Modules/` e não aqui.

## Conteúdo Permitido
* Abstrações cruciais de integração e comunicação transversal (interfaces base que se aplicam a toda a aplicação de forma uniforme).
* Tipos comuns globais regulados.

## Classe Marcadora
A classe `SharedKernelMarker` é disponibilizada apenas como ponto de referência de assembly para testes arquiteturais e varredura de reflexão (Reflection).

## Regra obrigatória de dados: Title Case

Todo texto humano deve ser normalizado em todas as entradas e na persistência. Campos técnicos e evidências históricas preservam suas regras. Consulte a [política geral](../../../docs/26-padronizacao-title-case.md) e o `AGENTS.md` da raiz antes de implementar novos campos, módulos ou importadores.
