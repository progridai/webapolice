# Correção da Configuração do CI Frontend — 26/10/2026

## Falha Confirmada

A [execução 40413016924](https://github.com/progridai/webapolice/actions/runs/40413016924), do commit 8b20807, reprovou o job frontend em `npm run test:run`. As anotações apontam `apps/web/src/app/config/env.ts:12`: variável obrigatória `VITE_API_BASE_URL` ausente ou vazia. Os jobs backend e segredos terminaram com sucesso.

O workflow não fornecia as quatro entradas obrigatórias de `ENV`. A validação local anterior carregava o `.env` ignorado na raiz, por meio de `envDir` do Vite; o checkout limpo do GitHub não contém esse arquivo. O resultado local registrado no documento 79 permanece válido para aquela execução, mas não comprovava a configuração do runner remoto.

## Correção

O job frontend em `.github/workflows/qualidade.yml` declara somente valores públicos fictícios:

| Entrada | Valor Exclusivo do Gate |
|---|---|
| VITE_API_BASE_URL | https://api.ci.invalid |
| VITE_KEYCLOAK_URL | https://idp.ci.invalid |
| VITE_KEYCLOAK_REALM | webapolice-ci |
| VITE_KEYCLOAK_CLIENT_ID | webapolice-web-ci |

O lançador `backend/scripts/verificar-qualidade.py` injeta os mesmos valores no ambiente dos processos filhos. A configuração do usuário não é persistida ou substituída. As entradas obrigatórias da aplicação continuam obrigatórias; não foram acrescentados fallbacks, removidos testes ou versionado um `.env` operacional.

Esses valores servem a testes e compilação do gate. O artefato compilado com eles não é um artefato de implantação. Builds destinados a ambientes reais precisam das URLs e identificadores públicos daquele ambiente; segredos nunca devem ser colocados em variáveis `VITE_*`, que são expostas ao navegador.

## Evidências e Limites

Foi criada uma cópia dos arquivos de HEAD em `scratch/ci-windows-8b20807`, sem `.env`, com dependências instaladas por `npm ci` a partir do lockfile. Node 24.18.0 e npm 11.16.0, no Windows. O teste `tests/pages/SegurancaFormErrors.test.tsx` sem as quatro entradas reproduziu a ausência de `VITE_API_BASE_URL` e a saída 1. Com os valores fictícios acima:

| Verificação | Resultado |
|---|---|
| format:check | Aprovada com os bytes LF versionados |
| typecheck | Aprovada |
| lint -- --max-warnings=0 | Aprovada, sem erros/avisos |
| lint:design-system | Aprovada, sem ocorrências |
| test:run | 1.139 testes aprovados em 74 arquivos |
| build | Aprovado; aviso existente de chunks acima de 500 kB permanece |
| npm audit --audit-level=low | Aprovada, zero vulnerabilidades reportadas |
| Scanner de segredos, sintaxe do gate Python e git diff --check | Aprovados |

A primeira cópia criada por `git archive` herdou CRLF da configuração Git local e reprovou o Prettier. A cópia foi refeita com `git -c core.autocrlf=false archive`; o conteúdo foi conferido contra o blob de HEAD e a formatação foi repetida com sucesso. Nenhuma configuração do Prettier ou arquivo de aplicação foi alterado para isso. Evidências locais em `scratch/ci-windows-*.log` e `scratch/ci-windows-resultados.json`; o resultado inicial de formatação foi preservado, e a repetição está em `scratch/ci-windows-format-check-lf.log`.

A tentativa de reprodução local em Docker aguardou o download da imagem e teve o lançador interrompido; não é contabilizada como aprovação Linux. A validação remota da correção deve ser conferida na execução GitHub Actions do novo commit. O backend não foi alterado nesta correção; o job remoto anterior aprovou build, testes e auditoria NuGet. A execução reprovada do commit anterior permanece no histórico. Proteções de branch, implantação e homologação de Keycloak/legado continuam exigindo evidências próprias.
