# Correção da Instalação dos Atalhos no Login

Data: 17/11/2026.

## Diagnóstico

O usuário relatou Erro de Autorização ao abrir a Home em produção. A mensagem do
frontend é genérica para falhas de carregamento de `/api/seguranca/me`.

A nova entidade Usuario mapeia AtalhosHome. O provisionamento executado antes das
requisições autenticadas carrega essa entidade por inteiro; portanto, a coluna
precisa existir antes de iniciar a nova API, inclusive para consultas que não
usam os atalhos diretamente. Fazer pull/push ou publicar a API não aplica a
migration no banco automaticamente.

O diagnóstico inicial foi na conexão de testes utilizada pela API local. Após o
usuário identificar `https://webapolice.bravida.com.br/#/app` como ambiente afetado,
a página e o asset públicos da implantação foram consultados para identificar
a API de produção. A consulta somente leitura na conexão específica de produção
confirmou, em `webapolice`, a ausência tanto de `seguranca.usuario.atalhos_home`
quanto da migration `20261117160039_AdicionarAtalhosHomeUsuario` no histórico EF.

## Correção Executada

Aplicado exclusivamente `backend/scripts/migrations/home/atalhos-home.sql` em
produção. A transação adicionou a coluna anulável `text[]` e registrou a migration.
Uma nova conexão confirmou coluna com tipo/nullability corretos e histórico
presente. A mesma instalação ocorreu no banco `webapolice_teste` durante o
diagnóstico local. Não foram executadas outras migrations, normalização histórica,
alterações de usuários, perfis, permissões ou configurações de autenticação.

O instalador `backend/scripts/aplicar-atalhos-home.ps1` foi acrescentado para
tornar diagnóstico e aplicação reproduzíveis:

```powershell
# Diagnóstico somente leitura em produção
& ./backend/scripts/aplicar-atalhos-home.ps1 -Ambiente Producao

# Instalação explícita e limitada à migration dos atalhos
& ./backend/scripts/aplicar-atalhos-home.ps1 -Ambiente Producao -Aplicar

# Repetir os mesmos passos no ambiente de testes
& ./backend/scripts/aplicar-atalhos-home.ps1 -Ambiente Teste
```

O script usa as conexões específicas de `backend/.env.local`, sem imprimi-las.
Recusa configurações ausentes, conexões de teste/produção iguais e nome de banco
incompatível com o ambiente. Em testes, exige ainda que a API local aponte para
a conexão de testes. Recusa divergência entre coluna e histórico ou coluna de
tipo/nullability incompatíveis. Mantém consultas em modo somente leitura e,
somente com Aplicar, permite a transação SQL específica, com limites de espera
por lock e de execução. Restaura as variáveis PG do processo ao terminar.

O SQL é enviado ao psql por stdin/arquivo para preservar identificadores entre
aspas no PowerShell para Windows. Não há credenciais em argumentos ou código
versionado. As mensagens operacionais do instalador usam ASCII para serem
legíveis também no Windows PowerShell 5.

## Verificação e Limites

- Parser PowerShell: sem erros de sintaxe.
- Diagnóstico anterior: coluna ausente e migration ausente nos dois bancos.
- Diagnóstico posterior: coluna compatível e migration presente nos dois bancos.
- Reexecução do instalador em testes: sem alterações adicionais.
- API de produção: `/api/health/live` e `/api/health/ready` retornaram 200.
- `/api/seguranca/me` e `/api/seguranca/me/atalhos-home` retornaram 401 sem token,
  preservando a exigência de autenticação.
- Nenhuma sessão autenticada de navegador estava disponível para confirmar o
  login do usuário. Os checks sem token não substituem essa conferência.

Não houve alteração de código de frontend ou backend C# nesta correção. Por isso,
não foram repetidas as suítes já aprovadas na entrega anterior; a verificação
pertinente foi a instalação efetiva, nova leitura do schema e checks da API.
Uma publicação local foi gerada durante a investigação, mas não foi ativada:
nenhum processo local ou de produção foi reiniciado por esta correção.

Arquivos alterados: instalador PowerShell novo, continuidade do documento 89 e
este registro. Os dados técnicos da coluna não recebem Title Case; não há campo
humano novo ou necessidade de reaplicar triggers.
