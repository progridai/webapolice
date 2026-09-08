# Guia de Tratamento, Validação, Normalização e Formatação de Campos

## 1. Objetivo

Este documento define o padrão oficial do WebApólice para tratamento de dados informados pelo usuário.

As regras deste guia devem ser observadas em todos os módulos atuais e futuros do sistema, incluindo:

- Clientes;
- Cooperados;
- Estipulantes;
- Subestipulantes;
- Apólices;
- Propostas;
- Seguradoras;
- Corretoras;
- Agenciadores;
- Financeiro;
- Comissões;
- Sinistros;
- demais cadastros e módulos futuros.

O objetivo é impedir que cada tela implemente regras próprias e evitar campos excessivamente permissivos, dados inconsistentes, problemas de integração, falhas de validação e riscos de segurança.

---

# 2. Princípio fundamental

Nenhum campo deve ser tratado simplesmente como "texto" apenas porque visualmente utiliza um `<input>`.

Todo campo deve possuir um tipo semântico conhecido.

Exemplos:

- CPF;
- CNPJ;
- documento;
- telefone;
- celular;
- CEP;
- e-mail;
- moeda;
- percentual;
- quantidade;
- data;
- código;
- matrícula;
- agência bancária;
- conta bancária;
- nome;
- observação.

O tipo semântico determina:

1. quais caracteres podem ser digitados;
2. qual tamanho mínimo e máximo é permitido;
3. se existe máscara;
4. como o valor é normalizado;
5. como o valor é validado;
6. como o valor é enviado para a API;
7. como o backend deverá validá-lo;
8. como deverá ser apresentado ao usuário.

---

# 3. Máscara não é validação

O WebApólice deve tratar separadamente os conceitos abaixo.

## 3.1 Máscara

Controla a apresentação durante a digitação.

Exemplo:

`51999999999`

é apresentado como:

`(51) 99999-9999`

A máscara é uma funcionalidade de experiência do usuário.

Ela não garante que o dado seja válido.

---

## 3.2 Normalização

Converte o valor informado para uma representação canônica utilizada pelo sistema.

Exemplo:

`(51) 99999-9999`

normalizado:

`51999999999`

Outro exemplo:

`123.456.789-00`

normalizado:

`12345678900`

---

## 3.3 Validação

Determina se o valor pode ser aceito pelo domínio.

Exemplos:

- CPF deve possuir dígitos verificadores válidos;
- CNPJ deve possuir dígitos verificadores válidos;
- CEP deve possuir 8 dígitos;
- percentual deve respeitar a faixa definida pela regra de negócio;
- data final não pode ser anterior à data inicial quando houver essa regra;
- e-mail deve possuir formato válido;
- quantidade não pode aceitar valores negativos quando isso não fizer sentido.

---

## 3.4 Formatação

Define como um valor será mostrado ao usuário.

Exemplo armazenado/normalizado:

`12345678900`

Exibição:

`123.456.789-00`

A formatação também deve ser aplicada em:

- formulários;
- grids;
- páginas de detalhes;
- filtros;
- resultados de busca;
- relatórios;
- exportações, quando aplicável.

---

# 4. Regra de segurança em camadas

A validação do frontend nunca deve ser considerada suficiente.

O fluxo correto é:

`Usuário → Input → Máscara → Normalização → Validação Frontend → DTO → API → Validação Backend → Regra de Negócio → Persistência`

O backend deve assumir que qualquer requisição pode ter sido enviada sem utilizar a interface oficial.

Portanto, toda validação relevante deve também existir no backend.

Nunca confiar somente em:

- máscara;
- atributo `maxlength`;
- componente React;
- validação Zod;
- JavaScript;
- tipo HTML do input.

Esses mecanismos melhoram a experiência do usuário, mas podem ser ignorados por uma chamada direta à API.

---

# 5. Política de componentes

Campos semânticos comuns devem utilizar componentes compartilhados.

Evitar implementar máscaras ou validações diretamente nas páginas.

Exemplos recomendados:

```tsx
<CpfInput />
<CnpjInput />
<CpfCnpjInput />
<CepInput />
<PhoneInput />
<EmailInput />
<CurrencyInput />
<PercentageInput />
<NumericInput />
<DateInput />
```

Pode existir internamente um componente genérico:

```tsx
<MaskedInput />
```

porém módulos de negócio devem preferencialmente utilizar os componentes semânticos.

---

# 6. Regra para Inputs genéricos

O componente `<Input />` genérico continua existindo, porém deve ser utilizado principalmente para campos realmente livres ou campos cujo domínio não exija tratamento especial.

Não utilizar `<Input />` diretamente para CPF, CNPJ, telefone, CEP, moeda, percentual ou outros campos que possuam componente semântico oficial.

Quando um novo tipo de campo recorrente surgir em mais de um módulo, avaliar sua inclusão no Design System ou biblioteca compartilhada.

---

# 7. Padrão oficial por tipo de campo

## CPF

Entrada:

`000.000.000-00`

Normalização:

somente 11 dígitos.

Validação:

- exatamente 11 dígitos;
- rejeitar sequências inválidas;
- validar dígitos verificadores.

Exibição:

`000.000.000-00`

---

## CNPJ

Entrada:

`00.000.000/0000-00`

Normalização:

somente 14 dígitos.

Validação:

- exatamente 14 dígitos;
- validar dígitos verificadores.

Exibição:

`00.000.000/0000-00`

---

## CPF/CNPJ

Quando um mesmo campo puder receber pessoa física ou jurídica, utilizar máscara dinâmica.

A identificação deverá considerar a quantidade de dígitos e o tipo de pessoa definido pelo domínio.

---

## CEP

O CEP no sistema possui um fluxo integrado de auto-preenchimento e validação assíncrona, tendo como referência arquitetural o módulo de Cooperados.

Entrada:

`00000-000`

Normalização:

somente 8 dígitos.

Validação:

8 dígitos.

Exibição:

`00000-000`

### Regras de Integração de Endereço

O preenchimento automático de endereço via CEP deve seguir **rigorosamente** as seguintes diretrizes em todos os formulários da aplicação:

1. **Endpoint Global:**
   - Todo módulo deve utilizar o serviço global `GET /api/enderecos/cep/{cep}` fornecido pelo backend (camada SharedKernel / Shared Infrastructure) para consultar os dados.
   - O Frontend não deve conectar diretamente a provedores externos (ex: ViaCEP).

2. **Gatilhos da Consulta:**
   - A consulta ocorre **somente** quando o usuário informa 8 dígitos numéricos válidos.
   - O `CepInput` atua exclusivamente como componente de interface visual, não contendo lógica de requisição acoplada. A requisição deve pertencer ao nível do formulário/feature.
   - A consulta é disparada apenas mediante alteração explícita pelo usuário. Evite disparos silenciosos automáticos apenas porque os dados iniciais (`initialData`) foram carregados na tela.

3. **Controle e Cancelamento de Requisições:**
   - O uso de `AbortController` é **obrigatório**. Caso o usuário altere a digitação antes da resposta finalizar, a requisição anterior deve ser cancelada para evitar preenchimentos fora de ordem ou sobrescritas tardias.

4. **Tratamento de Estado na UI:**
   - **Loading:** O campo `CepInput` **NÃO deve ser desabilitado** (`disabled`) durante a busca, mantendo a digitação livre. Utilize indicadores visuais textuais (ex: alterando o Label para `CEP (Buscando...)`).
   - **Limpeza Reativa:** Se um CEP válido for apagado ou modificado para um valor inválido/incompleto, os campos originados pela busca (Logradouro, Bairro, UF e CidadeId) devem ser **limpos**.
   - **Blindagem de Dados do Usuário:** O Número e o Complemento devem ser totalmente blindados e **nunca** sobrescritos ou limpos pelas rotinas de busca.

5. **Sincronia Assíncrona de Cidades (UF):**
   - Ao preencher automaticamente a UF retornada pelo CEP, o frontend fará o disparo para carregamento das cidades correspondentes (API `/api/localidades/cidades`).
   - O campo `cidadeId` retornado pelo serviço de CEP deve ser atribuído determinísticamente (ex: utilizando estado como `pendingCidadeId`), garantindo que ele só seja selecionado **após** a lista de cidades terminar de carregar no componente select.

6. **Falhas e Contingências:**
   - **Não encontrado (404):** Usar o sistema de erros do próprio validador do formulário (ex: `setError` do *react-hook-form*) para notificar `CEP não encontrado` sob o campo.
   - **Erro no serviço (500/502/etc):** Informar "Serviço indisponível. Preencha manualmente".
   - Falhas no CEP ou instabilidades externas nunca devem travar o sistema ou impedir o usuário de realizar o preenchimento manual do endereço e prosseguir com a operação.

---

## Telefone

Deve aceitar dinamicamente telefone fixo ou celular.

Exemplos:

`(51) 3333-4444`

`(51) 99999-9999`

Normalização:

somente dígitos.

Não armazenar a máscara como requisito da regra de negócio.

---

## E-mail

Não aplicar máscara.

Aplicar:

- `trim`;
- validação de formato;
- tamanho máximo;
- teclado apropriado em dispositivos móveis.

Não alterar arbitrariamente o conteúdo informado pelo usuário.

---

## RG

Não deve existir uma máscara nacional fixa.

RG pode possuir formatos diferentes conforme estado e órgão emissor.

Aplicar:

- tamanho máximo;
- conjunto de caracteres permitido conforme regra adotada;
- normalização de espaços;
- validações específicas somente quando houver regra de negócio conhecida.

---

## Órgão emissor

Texto curto.

Aplicar:

- limite de tamanho;
- `trim`;
- padronização para maiúsculas quando apropriado.

Exemplo:

`ssp`

→

`SSP`

---

## UF

Nunca utilizar texto livre quando a interface puder utilizar seleção.

Aceitar apenas UFs brasileiras válidas.

Persistir a sigla oficial com duas letras maiúsculas.

---

## Cidade

Quando existir cadastro de cidades no sistema, utilizar referência à entidade de cidade.

Evitar permitir texto livre paralelo ao cadastro oficial.

---

## Banco

Quando existir cadastro de bancos, utilizar seleção/referência ao banco.

Não permitir ao usuário cadastrar livremente códigos bancários dentro de outros formulários.

---

## Agência bancária

Não utilizar uma única máscara fixa para todos os bancos.

Bancos possuem regras diferentes de agência e dígito verificador.

Aplicar:

- limite de tamanho;
- caracteres permitidos;
- normalização;
- regras específicas por banco somente quando efetivamente conhecidas.

---

## Conta corrente

Também não deve possuir uma máscara universal.

Aplicar:

- limite de tamanho;
- caracteres permitidos;
- normalização;
- possibilidade de dígito verificador;
- regras específicas conforme banco quando aplicável.

---

## Moeda

Entrada em padrão brasileiro.

Exemplo:

`R$ 1.234,56`

Valor utilizado pelo domínio:

decimal.

Nunca utilizar `float` ou `double` para valores monetários.

A máscara visual não deve transformar o valor em string dentro das regras financeiras.

---

## Decimal

Interface:

`1.234,56`

Representação de domínio/API:

valor decimal adequado ao contrato.

Definir explicitamente:

- casas decimais;
- valor mínimo;
- valor máximo.

---

## Percentual

Interface:

`12,50%`

Representação de domínio:

decimal.

A faixa permitida deverá ser definida pelo contexto.

Não assumir automaticamente que todo percentual deve estar entre 0 e 100 quando alguma regra de negócio puder utilizar outro intervalo.

---

## Inteiro / quantidade

Aceitar apenas números inteiros.

Definir:

- mínimo;
- máximo;
- possibilidade ou não de zero;
- possibilidade ou não de números negativos.

Não permitir caracteres que posteriormente sejam simplesmente descartados.

---

## Datas

Utilizar componente de data apropriado.

Formato de apresentação brasileiro:

`dd/MM/yyyy`

Transporte pela API:

formato definido pelo contrato, preferencialmente baseado em ISO.

Distinguir:

- data civil;
- data e hora;
- instante com timezone.

Não converter indiscriminadamente campos `date` para timestamp.

---

## Data e hora

Utilizar somente quando o domínio realmente exigir horário.

O tratamento de timezone deve ocorrer conforme padrão arquitetural do sistema.

---

## Nome / Razão Social

Aplicar:

- `trim`;
- tamanho mínimo quando obrigatório;
- tamanho máximo;
- rejeição de valor contendo somente espaços.

Não restringir nomes reais apenas a letras ASCII.

Acentos, hífens, apóstrofos e outros caracteres legítimos devem ser considerados.

---

## Código

Códigos administrativos devem possuir regra explícita.

Definir:

- tamanho;
- se é numérico;
- se é alfanumérico;
- se diferencia maiúsculas/minúsculas;
- caracteres especiais permitidos.

Não usar regra genérica sem conhecer a finalidade do código.

---

## Matrícula

Matrícula não deve ser considerada número automaticamente.

Muitas matrículas podem conter:

- zeros à esquerda;
- letras;
- hífen;
- outros códigos.

Persistir como texto quando o domínio assim exigir.

---

## Observações e textos longos

Não aplicar máscara.

Aplicar:

- tamanho máximo;
- `trim` quando apropriado;
- tratamento seguro na exibição.

Não executar HTML fornecido pelo usuário.

---

# 8. Limites de tamanho

Todo campo textual deve possuir limite conhecido.

O limite deve considerar:

1. regra de negócio;
2. contrato da API;
3. capacidade do banco;
4. experiência da interface.

Não permitir campos visualmente ilimitados quando a API ou banco possuem limite menor.

Frontend e backend devem possuir limites compatíveis.

---

# 9. Valores obrigatórios

A obrigatoriedade deve ser derivada da regra de negócio.

Não considerar automaticamente um campo obrigatório apenas porque a coluna do banco é `NOT NULL`, pois o modelo físico não substitui o contrato de negócio.

Campos obrigatórios devem ser claramente indicados na interface.

A API deve validar novamente a obrigatoriedade.

---

# 10. Campos opcionais

Campos opcionais vazios devem possuir comportamento padronizado.

Evitar misturar indiscriminadamente:

```text
""
" "
null
undefined
```

O contrato da API deve estabelecer a representação esperada.

Textos opcionais recebidos somente com espaços devem normalmente ser normalizados para ausência de valor, quando isso fizer sentido para o domínio.

---

# 11. Caracteres inválidos

A restrição deve ser baseada no tipo semântico do campo.

Exemplo:

CPF pode aceitar somente dígitos semanticamente, embora visualmente possua pontuação.

Por outro lado, não se deve criar filtros agressivos de caracteres em campos de nome, endereço ou observação sem necessidade.

Segurança não deve depender da remoção aleatória de caracteres.

---

# 12. SQL Injection

Máscaras de campos não são proteção contra SQL Injection.

Toda comunicação com banco deve utilizar:

- ORM corretamente configurado;
- queries parametrizadas;
- parâmetros tipados.

Nunca concatenar entrada do usuário diretamente em comandos SQL.

---

# 13. XSS e conteúdo fornecido pelo usuário

Textos informados pelo usuário devem ser tratados como dados, não como código ou HTML executável.

O frontend deve utilizar mecanismos seguros de renderização.

Não utilizar conteúdo vindo de usuário em APIs como `dangerouslySetInnerHTML` sem necessidade, sanitização e justificativa arquitetural.

---

# 14. Normalizadores compartilhados

Normalizadores não devem ser recriados dentro de cada página.

Exemplos:

```ts
normalizeCpf()
normalizeCnpj()
normalizeCpfCnpj()
normalizePhone()
normalizeCep()
normalizeEmail()
normalizeText()
normalizeUppercase()
```

Evitar espalhar pelo projeto expressões como:

```ts
value.replace(/\D/g, '')
```

Cada normalização deverá possuir implementação central e testes.

---

# 15. Validadores compartilhados

Validações reutilizáveis também devem ser centralizadas.

Exemplos:

```ts
isValidCpf()
isValidCnpj()
isValidEmail()
isValidCep()
isValidPhone()
```

Schemas Zod podem consumir esses validadores.

Não duplicar algoritmos de CPF/CNPJ dentro de vários módulos.

---

# 16. Formatadores compartilhados

A aplicação deverá possuir formatadores reutilizáveis.

Exemplos:

```ts
formatCpf()
formatCnpj()
formatCpfCnpj()
formatPhone()
formatCep()
formatCurrency()
formatPercentage()
formatDate()
```

O mesmo formatador deverá ser utilizado em diferentes módulos.

---

# 17. Backend

Toda informação relevante deve ser normalizada e validada novamente no backend.

Nunca assumir que a requisição foi originada pelo frontend oficial.

O backend deverá rejeitar payloads:

- fora do formato esperado;
- maiores que os limites permitidos;
- com tipos incompatíveis;
- semanticamente inválidos;
- incompatíveis com regras de negócio.

---

# 18. Persistência

O banco deve receber valores coerentes com o modelo de domínio.

Quando a modelagem possuir campos como:

```text
cpf
cpf_limpo
```

ou:

```text
documento_principal
documento_principal_limpo
```

a aplicação deve respeitar a estratégia arquitetural existente.

Campos auxiliares como `*_limpo`, `*_normalizado` e similares não devem ser tratados como dados editáveis diretamente pela interface.

---

# 19. APIs e DTOs

A estrutura das tabelas não deve determinar diretamente o contrato do frontend.

Controllers não devem expor entidades de persistência diretamente.

Utilizar DTOs e casos de uso.

Campos técnicos de normalização não devem ser enviados para a interface sem necessidade.

Exemplo:

não expor:

```json
{
  "cpf": "123.456.789-00",
  "cpfLimpo": "12345678900",
  "cpfValido": true
}
```

quando a interface necessita apenas do valor de negócio apropriado.

---

# 20. Mensagens de validação

Mensagens devem explicar o problema de maneira útil.

Preferir:

`Informe um CPF válido.`

em vez de:

`Invalid input.`

Preferir:

`O percentual deve estar entre 0 e 100.`

em vez de:

`Valor inválido.`

Erros técnicos internos não devem ser apresentados diretamente ao usuário.

---

# 21. Validação durante a digitação

A interface deve auxiliar o usuário sem tornar a experiência hostil.

Evitar apresentar erro de CPF inválido quando o usuário digitou apenas os primeiros números.

Validações completas devem ocorrer:

- quando o campo tiver quantidade suficiente de caracteres;
- no `blur`;
- na tentativa de envio;
- conforme padrão definido pelo Design System.

---

# 22. Colar valores

Campos mascarados devem aceitar valores copiados com ou sem máscara.

Exemplo:

CPF deve aceitar tanto:

`12345678900`

quanto:

`123.456.789-00`

O componente deverá normalizar adequadamente.

---

# 23. Mobile

Campos devem informar o teclado apropriado.

Exemplos:

- CPF: teclado numérico;
- telefone: teclado telefônico;
- e-mail: teclado de e-mail;
- quantidade: teclado numérico;
- valores: teclado decimal quando disponível.

---

# 24. Autocomplete

Utilizar atributos adequados quando fizer sentido.

Exemplos:

- nome;
- e-mail;
- telefone;
- endereço;
- CEP.

Evitar autocomplete em campos sensíveis quando não for apropriado.

---

# 25. Dados pessoais

CPF, CNPJ, RG, telefone, e-mail e endereço devem ser tratados como dados pessoais ou cadastrais sensíveis ao contexto da aplicação.

Evitar:

- registrar payload completo em logs;
- exibir dados sem necessidade;
- colocar documentos completos em mensagens de erro;
- utilizar documentos como identificador público de URL.

---

# 26. Pesquisas e filtros

Filtros também devem utilizar normalização.

Exemplo:

uma busca por CPF deve funcionar quando o usuário digitar:

`12345678900`

ou:

`123.456.789-00`

A camada de consulta deverá utilizar o campo normalizado apropriado quando houver.

---

# 27. Listagens e detalhes

Valores devem aparecer formatados consistentemente.

Exemplo:

CPF nunca deverá aparecer em uma tela como:

`12345678900`

e em outra como:

`123.456.789-00`

quando ambas representam o mesmo tipo de informação.

---

# 28. Importações e integrações

Dados recebidos por:

- importação;
- API externa;
- arquivos;
- migrações;
- integrações;

devem passar pelas mesmas regras conceituais de normalização e validação do domínio.

A existência de uma máscara no frontend não substitui tratamento de dados externos.

---

# 29. Regra para novos módulos

Antes de criar qualquer formulário, a implementação deverá identificar todos os campos e classificá-los pelo tipo semântico.

Exemplo:

| Campo | Tipo semântico |
|---|---|
| CPF | cpf |
| Telefone | telefone |
| CEP | cep |
| Comissão | percentual |
| Prêmio | moeda |
| Data de início | data |
| Observação | texto-longo |

Somente após essa classificação o formulário deverá ser implementado.

---

# 30. Separação Arquitetural Preservada

A arquitetura do projeto possui papéis bem definidos que devem ser rigorosamente respeitados:

- `components/ui`: primitives visuais do Design System (botões, modais, inputs genéricos puros).
- `components/fields`: componentes semânticos de entrada (ex: `CpfInput`, `PhoneInput`).
- `shared/utils`: funções reutilizáveis puras (`normalizers.ts`, `validators.ts`, `formatters.ts`).
- `features/<modulo>`: específico do domínio (`schemas`, `mappers.ts`, formulários, lógicas e chamadas à API da feature).
- `backend`: definição do contrato da API (DTOs), normalização defensiva e validação semântica real (Domain Validators e Handlers), regras de negócio centrais e persistência.

---

# 31. Regra obrigatória para a MIA (Evolução sob Demanda)

Ao criar ou modificar qualquer formulário, página de detalhes, grid ou filtro do WebApólice, a MIA deverá, impreterivelmente:

1. Consultar este guia;
2. Verificar se já existe Field semântico reutilizável;
3. Verificar se já existe normalizer;
4. Verificar se já existe validator;
5. Verificar se já existe formatter;
6. **Consultar o módulo Cooperados como exemplo arquitetural**;
7. **Não copiar regras específicas de Cooperados para outros módulos** (regras de um domínio não se aplicam aos outros);
8. **Criar novos componentes compartilhados somente quando surgir uma necessidade realmente reutilizável**.

**Crescimento sob Demanda:** Não crie antecipadamente componentes, validators ou utilitários apenas porque poderão ser necessários no futuro. A infraestrutura global deve crescer apenas sob demanda. Quando surgir um novo tipo semântico recorrente (exemplos: CNPJ, moeda, percentual, etc.) que ainda não possua implementação compartilhada, ele deverá então ser incorporado ao padrão global `components/fields` e `shared/utils`.

É proibido criar uma implementação local duplicada quando já existir um padrão compartilhado.

---

# 32. Critério de Aderência para conclusão de um módulo

Uma tela ou módulo não deve ser considerada concluída apenas porque consegue salvar dados.
Para ser declarada padronizada e aderente a este guia, a implementação deve abranger e respeitar explicitamente todas as etapas do ciclo de vida da informação:

1. **Entrada de dados**: Uso de Fields semânticos na interface;
2. **Validação Frontend**: Schema (ex: Zod) utilizando validators compartilhados;
3. **Normalização Frontend**: Conversão do estado do formulário via Mapper/Serializer;
4. **Construção do DTO**: Envio do payload limpo e canônico à API;
5. **Validação Backend**: Normalização defensiva e validação real na Controller/Handler/Domain;
6. **Persistência**: Gravação de dados coerente com o modelo de domínio;
7. **Carregamento para Edição**: Carregamento adequado do banco para a tela;
8. **Formatação de Saída**: Apresentação formatada em grids e páginas de detalhes.

---

# 33. Implementação de Referência — Módulo Cooperados

O módulo **Cooperados** foi homologado como a primeira implementação de referência do *Guia de Tratamento, Validação, Normalização e Formatação de Campos*. Qualquer desenvolvedor ou a própria MIA deve utilizá-lo como inspiração arquitetural sobre como isolar regras, normalizar dados e validar defensivamente.

**Exemplos Reais e Arquivos de Referência:**
- **Fields Semânticos**: `apps/web/src/components/fields/` (CpfInput, CepInput, PhoneInput, DateInput, EmailInput).
- **Utilitários Compartilhados**:
  - `apps/web/src/shared/utils/normalizers.ts`
  - `apps/web/src/shared/utils/validators.ts`
  - `apps/web/src/shared/utils/formatters.ts`
- **Mappers de Domínio**: `apps/web/src/features/cooperados/utils/cooperados.mappers.ts`
- **Schemas e Formulários**: O schema Zod presente no componente `apps/web/src/features/cooperados/components/CooperadoForm.tsx`.
- **Validações e Backend Defensivo**:
  - DTOs/Requests: `backend/src/WebApolice.Modulos.Cadastro/Api/Requests/CooperadoRequests.cs`.
  - Centralização de regras: `backend/src/WebApolice.Modulos.Cadastro/Domain/Validators/RegrasCadastraisValidator.cs` e `DocumentoValidator.cs`.
  - Uso nas aplicações de negócio: `backend/src/WebApolice.Modulos.Cadastro/Application/UseCases/CadastrarCooperado/CadastrarCooperadoHandler.cs` e `AlterarCooperadoHandler.cs`.

---

# 34. Testes mínimos

Componentes e utilitários compartilhados devem possuir testes para:
- valor válido, inválido, vazio, valor parcial, limite máximo, caracteres indevidos;
- colagem com máscara e sem máscara;
- normalização e formatação.

CPF e CNPJ devem possuir testes específicos dos algoritmos de validação.

---

# 35. Antipadrões proibidos

Não fazer: `<Input name="cpf" />` sem tratamento semântico.
Não repetir lógicas de substituição (ex: `.replace(/\D/g, '')`) em dezenas de componentes.
Não confiar exclusivamente em Zod ou apenas no backend.
Não criar máscaras universais de RG, agência ou conta bancária sem regra real.
Não armazenar um valor inválido simplesmente porque possui o número correto de caracteres.

---

# 36. Princípio final

O WebApólice deve possuir uma única definição para cada tipo recorrente de dado.
A regra deve ser: **definir uma vez → implementar uma vez → reutilizar em todos os módulos.**
Nenhum novo módulo deve introduzir uma segunda forma de tratar um tipo de dado já padronizado.