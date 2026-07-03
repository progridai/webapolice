# ADR-003: Backend ASP.NET Core (ADR-003-backend-aspnet-core.md)

* **Status**: Aceito
* **Data**: 2026-07-02
* **Autor**: Equipe de Arquitetura

---

## 1. Decisão
Adotar o ASP.NET Core baseado em .NET 10 LTS como a plataforma de desenvolvimento backend principal do ERP, utilizando APIs REST como canais exclusivos de comunicação e estabelecendo o backend como a única autoridade para validação de regras de negócio e integrações externas.

## 2. Contexto e Motivação
A reescrita de um ERP em VB.NET exige uma transição suave para uma linguagem tipada, de altíssima performance e resiliente. O .NET 10 (LTS) garante o suporte de longo prazo da Microsoft, excelente suporte a contêineres Docker, e permite ao time reaproveitar a bagagem conceitual de C# e frameworks da plataforma. O isolamento de integrações externas é crítico para evitar que falhas de parceiros quebrem o frontend ou exponham segredos corporativos de conexão.

## 3. Benefícios
* **Performance Elevada**: Kestrel (servidor web embutido) está entre as engines web mais rápidas de mercado.
* **Segurança e Integridade**: Nullable reference types e tratamento rígido de warnings evitam exceções clássicas como `NullReferenceException` em produção.
* **Centralização de Regras**: Garante que o frontend seja um cliente burro e que a integridade dos dados e auditoria seja assegurada pelo core backend.

## 4. Riscos e Mitigações
* **Complexidade no Setup Inicial**: .NET exige mais configurações estruturais que ambientes interpretados como Node.js.
  * *Mitigação*: Utilização de Minimal APIs nesta fase mínima de bootstrap técnico para manter o código limpo, enxuto e livre de arquivos redundantes de controllers.

## 5. Consequências
* Nenhuma consulta direta ao banco de dados PostgreSQL pode ser realizada pelo Bravito ou sistemas externos. O backend é a única fachada de acesso.
* Configurações de OpenAPI e injeção de dependência nativas devem ser estruturadas desde o início.

## 6. Alternativas Consideradas
* **Node.js (NestJS)**: Rejeitado pela preferência da equipe técnica por uma linguagem fortemente tipada e com suporte corporativo nativo abrangente da plataforma Microsoft para grandes sistemas de ERP.
* **Java (Spring Boot)**: Rejeitado pela ausência de expertise prévia da equipe e custos adicionais de curva de aprendizado.
