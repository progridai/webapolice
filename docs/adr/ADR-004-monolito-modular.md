# ADR-004: Monólito Modular (ADR-004-monolito-modular.md)

* **Status**: Aceito
* **Data**: 2026-07-02
* **Autor**: Equipe de Arquitetura

---

## 1. Decisão
Adotar a arquitetura de **Monólito Modular** como modelo inicial para o desenvolvimento do backend do ERP WebApolice. O código será organizado conceitualmente e fisicamente por módulos de domínio lógicos independentes, mantendo a simplicidade de implantação sob um único processo executável.

## 2. Contexto e Motivação
Grandes sistemas ERP frequentemente enfrentam problemas de complexidade se iniciados diretamente como microsserviços. Problemas relacionados à consistência eventual de transações distribuídas, latência de rede, gerenciamento de infraestrutura distribuída e depuração local dificultam o progresso de novos times. O monólito modular une a facilidade de desenvolvimento e implantação de uma única aplicação física com a separação lógica de conceitos necessária para manter o sistema limpo.

## 3. Benefícios
* **Baixa Complexidade Operacional**: Deploy simplificado e fácil execução no ambiente de desenvolvimento local dos programadores.
* **Transações ACID**: Possibilidade de utilizar transações comuns de banco de dados inter-domínios caso seja estritamente necessário (evitando o padrão Saga).
* **Fácil Extração**: Se um domínio de negócio crescer o suficiente e exigir escalabilidade física isolada, as fronteiras modulares limpas facilitam sua separação física no futuro.

## 4. Riscos e Mitigações
* **Acoplamento Indevido Inter-Módulos**: Riscos de desenvolvedores referenciarem entidades internas de outros domínios de forma direta, ignorando interfaces públicas.
  * *Mitigação*: Uso de testes arquiteturais automatizados (`WebApolice.Architecture.Tests`) e regras estritas de revisão de código impedindo importações indevidas.

## 5. Consequências
* A pasta `backend/src/Modules` conterá as implementações autocontidas dos domínios do ERP assim que definidos.
* Nenhum módulo pode ler ou escrever diretamente no banco de dados de outro módulo.
* Os módulos executam no mesmo processo, havendo apenas uma única unidade de implantação global.
* A persistência será sempre realizada de modo relacional (PostgreSQL). Armazenamento primário em memória não é, nem será, uma decisão arquitetural para os dados do produto.

## 6. Critérios Futuros de Extração
Um módulo do monólito modular só será avaliado para extração física em um serviço separado se cumprir pelo menos um dos seguintes critérios:
1. **Divergência de Escalabilidade**: Exigir recursos computacionais (CPU/Memória) desproporcionalmente maiores ou escalabilidade elástica rápida em comparação ao restante do ERP.
2. **Ciclo de Entrega Independente**: Ser mantido por um time totalmente separado que necessite realizar implantações diárias sem impacto na integridade geral do monólito principal.
3. **Dependências Tecnológicas Conflitantes**: Exigir versões de runtime, SO ou drivers específicos incompatíveis com o core do .NET Core.
