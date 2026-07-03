# ADR-002: Frontend React e Vite (ADR-002-frontend-react-vite.md)

* **Status**: Aceito
* **Data**: 2026-07-02
* **Autor**: Equipe de Arquitetura

---

## 1. Decisão
Adotar React 19 com TypeScript estrito executado sobre o Vite 8 para criar uma aplicação puramente SPA (Single Page Application) e responsiva localizada na pasta `apps/web`. A decisão de escolha e estruturação do Design System definitivo (Tailwind CSS, shadcn/ui, etc.) fica formalmente **postergada** para uma etapa posterior de especificação.

## 2. Contexto e Motivação
O ERP WebApolice exige uma interface administrativa reativa, rápida de carregar e responsiva. O uso de uma SPA tradicional em React elimina o custo de renderização no servidor (SSR) para páginas que residem atrás de autenticação, simplifica o ciclo de publicação do build como conteúdo estático e oferece uma experiência de usuário de aplicativo desktop no navegador. 

## 3. Benefícios
* **HMR Ultra-rápido**: Vite fornece tempos de resposta instantâneos em ambiente de desenvolvimento local.
* **Segurança de Tipos**: TypeScript em modo estrito reduz erros de tempo de execução (*runtime*).
* **Simplicidade de Infraestrutura**: O build final gera arquivos HTML/JS/CSS estáticos que podem ser servidos de forma barata por qualquer servidor web (Nginx, IIS, S3).

## 4. Riscos e Mitigações
* **Falta de Organização de Código**: SPAs podem facilmente se tornar bagunçadas se os componentes forem criados de forma desestruturada.
* **Necessidade de Disciplina Arquitetural**: Exige a aplicação de regras estritas (como as de `AI_PROJECT_RULES.md`) para não embutir chamadas de rede externas ou regras de negócio críticas dentro dos componentes React.

## 5. Consequências
* Nenhuma biblioteca de estilização visual (ex: Tailwind CSS) será adicionada na fundação técnica inicial. O design padrão fica restrito a CSS básico para garantir a legibilidade das informações.
* O roteador e controle de estado global também foram omitidos nesta fase mínima de bootstrap técnico.

## 6. Alternativas Consideradas
* **Next.js**: Rejeitado inicialmente porque o ERP WebApolice é um sistema corporativo seguro e restrito a usuários cadastrados, onde renderização no servidor (SSR) ou otimizações de motores de busca públicos (SEO) não trazem benefícios concretos que justifiquem a complexidade operacional adicional do Next.js.
* **Angular / Vue**: Rejeitados devido à maior facilidade do time com a componentização baseada em funções do React.
