# Architectural Decision Records (ADRs) (docs/adr/README.md)

Este diretório contém os Registros de Decisão de Arquitetura (ADRs) do projeto **WebApolice**. 

---

## O que é um ADR?
Um Architectural Decision Record (ADR) é um documento curto que descreve uma decisão de arquitetura tomada em relação ao sistema, contendo o contexto em que foi tomada, as consequências e os riscos associados.

## Quando criar um ADR?
Sempre que uma decisão técnica estrutural for proposta ou alterada (ex: escolha de uma biblioteca de persistência, modelo de autenticação, separação de serviços, etc.), um novo ADR deve ser criado para documentar e alinhar o time.

## Status Adotados
* **Proposto**: A decisão está em fase de planejamento ou aguardando aprovação da equipe técnica.
* **Aceito**: A decisão foi aprovada e deve ser implementada no código.
* **Substituído**: Uma decisão anterior foi anulada ou atualizada por um novo ADR.
* **Rejeitado**: A proposta foi avaliada e descartada pela equipe.

---

## Regra de Ouro
**Não altere silenciosamente uma decisão que já foi classificada como `Aceito`.** 
Se um padrão definido anteriormente precisar ser mudado ou revogado devido a novas necessidades do projeto, **não edite** o ADR original para mudar sua lógica retrospectivamente. Crie um **novo ADR** detalhando o novo cenário, marque o status do ADR original como `Substituído` e aponte para o novo documento.
