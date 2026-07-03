# Modules Directory (backend/src/Modules/README.md)

Este diretório destina-se a conter os módulos lógicos do ERP WebApolice estruturados sob os princípios de um Monólito Modular.

---

## Regras e Diretrizes para Criação de Módulos

1. **Organização por Domínio**: Cada módulo futuro deve representar um domínio delimitado de negócio do ERP.
2. **Sem Módulos Vazios**: Não devem ser criados projetos ou pastas vazias de módulos (como Clientes, Faturamento, Sinistros) antes que suas respectivas especificações de regras de negócio e modelagem de domínio estejam formalmente documentadas e aprovadas.
3. **Limites Claros e Baixo Acoplamento**: Cada módulo deve gerenciar sua própria lógica interna. Módulos não podem acessar diretamente classes internas, handlers ou repositórios de outros módulos.
4. **Comunicação por Contratos Públicos**: Qualquer interação entre módulos diferentes deve ocorrer exclusivamente através de contratos de comunicação públicos definidos de forma explícita (interfaces de serviços ou publicação/consumo de eventos).
5. **Persistência Isolada**: O banco de dados PostgreSQL deve respeitar as fronteiras modulares. Um módulo não deve ler ou escrever em tabelas pertencentes a outro módulo de forma direta.
