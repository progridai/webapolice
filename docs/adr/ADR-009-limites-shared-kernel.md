# ADR 009: Restrições do SharedKernel

## Status
Aceito

## Contexto
Durante o desenvolvimento do Monólito Modular, existe a tendência de compartilhar qualquer DTO, interface ou utilitário comum através do projeto `SharedKernel`. Isso gera dependências circulares ocultas, alto acoplamento ("dumping ground" genérico) e viola os preceitos de modularidade onde cada módulo deve possuir seus próprios contextos autônomos.

## Decisão
Decidimos impor limites estritos ao projeto `SharedKernel`:
- Conterá **somente** abstrações fundamentais transversais e agnósticas a qualquer domínio de negócio.
- O SharedKernel **não pode referenciar** nenhum módulo de negócio específico (proibido no pipeline por Testes de Arquitetura).
- Ele não deve conter: implementações concretas de HTTP, entidades base infladas (`EntityBase` acoplada), implementações completas de log de terceiros ou frameworks de validação acoplados a infraestruturas legadas.
- O SharedKernel deve se manter limpo e minúsculo. 

## Consequências
- Código de negócio "comum" (como tipos específicos usados por dois módulos) será evitado no SharedKernel. Ao invés disso, será criado em um contrato específico daquele módulo ou, se inevitável, duplicado por valor.
- A estabilidade das alterações aumenta à medida que o SharedKernel vira praticamente um projeto congelado e estrutural, minimizando efeitos colaterais em toda a aplicação.
