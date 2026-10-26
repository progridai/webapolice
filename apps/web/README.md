# Frontend WebApolice

Estado em 23/10/2026: React 19/TypeScript/Vite 8, com correções de dependências e testes no Vitest 5. Seguir [o guia obrigatório](../../docs/guias/guia-padroes-frontend.md) e executar as seis verificações em package.json. A [revisão 76](../../docs/76-revisao-arquitetura-seguranca-qualidade.md) registra resultados e pendências de homologação; [operação e validação](../../docs/78-operacao-e-validacao-consolidacao.md) justifica as atualizações corretivas e o override transitivo do parser PostCSS.

O cliente HTTP cancela antes/durante o token e a rede, mantém timeout/cancelamento durante a leitura da resposta e libera listeners/timers ao concluir. Dados humanos continuam passando por normalizeHumanData; não há nova tela ou alteração de tokens/temas nesta entrega. No container Nginx sem privilégio, a porta HTTP é 8080.

## Notas do Template Inicial

## Desenvolvimento no WebApólice

Antes de criar ou alterar telas, componentes ou serviços, siga o [guia obrigatório de padrões do frontend](../../docs/guias/guia-padroes-frontend.md) e as [instruções permanentes](../../AGENTS.md). A entrega deve manter `format:check`, `typecheck`, `lint`, `lint:design-system`, `test:run` e `build` aprovados, com a documentação atualizada.

## Referência do template original

This template provides a minimal setup to get React working in Vite with HMR and some ESLint rules.

Currently, two official plugins are available:

- [@vitejs/plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react) uses [Oxc](https://oxc.rs)
- [@vitejs/plugin-react-swc](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react-swc) uses [SWC](https://swc.rs/)

## React Compiler

The React Compiler is not enabled on this template because of its impact on dev & build performances. To add it, see [this documentation](https://react.dev/learn/react-compiler/installation).

## Expanding the ESLint configuration

If you are developing a production application, we recommend updating the configuration to enable type-aware lint rules:

```js
export default defineConfig([
  globalIgnores(["dist"]),
  {
    files: ["**/*.{ts,tsx}"],
    extends: [
      // Other configs...

      // Remove tseslint.configs.recommended and replace with this
      tseslint.configs.recommendedTypeChecked,
      // Alternatively, use this for stricter rules
      tseslint.configs.strictTypeChecked,
      // Optionally, add this for stylistic rules
      tseslint.configs.stylisticTypeChecked,

      // Other configs...
    ],
    languageOptions: {
      parserOptions: {
        project: ["./tsconfig.node.json", "./tsconfig.app.json"],
        tsconfigRootDir: import.meta.dirname,
      },
      // other options...
    },
  },
]);
```

You can also install [eslint-plugin-react-x](https://github.com/Rel1cx/eslint-react/tree/main/packages/plugins/eslint-plugin-react-x) and [eslint-plugin-react-dom](https://github.com/Rel1cx/eslint-react/tree/main/packages/plugins/eslint-plugin-react-dom) for React-specific lint rules:

```js
// eslint.config.js
import reactX from "eslint-plugin-react-x";
import reactDom from "eslint-plugin-react-dom";

export default defineConfig([
  globalIgnores(["dist"]),
  {
    files: ["**/*.{ts,tsx}"],
    extends: [
      // Other configs...
      // Enable lint rules for React
      reactX.configs["recommended-typescript"],
      // Enable lint rules for React DOM
      reactDom.configs.recommended,
    ],
    languageOptions: {
      parserOptions: {
        project: ["./tsconfig.node.json", "./tsconfig.app.json"],
        tsconfigRootDir: import.meta.dirname,
      },
      // other options...
    },
  },
]);
```

## Regra obrigatória de dados: Title Case

Todo texto humano deve ser normalizado em todas as entradas e na persistência. Campos técnicos e evidências históricas preservam suas regras. Consulte a [política geral](../../docs/26-padronizacao-title-case.md) e o `AGENTS.md` da raiz antes de implementar novos campos, módulos ou importadores.
