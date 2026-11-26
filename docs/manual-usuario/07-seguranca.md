# Segurança e Administração

[Início do manual](README.md) · [Anterior: financeiro](06-financeiro.md) · [Próximo: dúvidas](08-duvidas.md)

## Como o Acesso Funciona

O login identifica a conta. Os Perfis e suas permissões definem as operações de negócio disponíveis. A disponibilidade de módulos e recursos também interfere no acesso.

**Exemplo:** alguém pode consultar Propostas sem poder alterá-las. Ter acesso a Clientes não concede automaticamente criação de Propostas. Um atalho pessoal também não acrescenta permissão.

Este capítulo se destina a usuários autorizados a administrar ou consultar essas áreas. Quando uma tela não estiver disponível, solicite ao responsável a avaliação do acesso necessário para a sua função.

## Cadastrar um Usuário

**Onde:** Segurança → Usuários.

1. Confira se a conta já existe.
2. Abra a ação de cadastro.
3. Informe **Username**, **Nome Completo** e **E-mail**.
4. Preencha **Senha Temporária** e **Confirmar Senha**.
5. Confira o **Status** da conta.
6. Em **Perfis de Acesso**, selecione os Perfis adequados à função.
7. Escolha **Cadastrar Usuário** e confira o resultado.

**Resultado esperado:** usuário registrado conforme o retorno da operação. O nome de acesso deve ser conferido antes do cadastro: o campo Username não fica editável depois da criação.

Ao editar, revise os dados permitidos, a situação e os Perfis, e escolha **Salvar Alterações**. A senha temporária pertence à criação; não presuma que editar o usuário oferece o mesmo fluxo de redefinição de senha.

## Manter Perfis de Acesso

**Onde:** Segurança → Perfis.

1. Localize o Perfil ou abra um novo cadastro.
2. Confira código, nome, descrição e situação.
3. Revise as permissões apresentadas e selecione as necessárias para a função.
4. Salve.
5. Confira os usuários associados conforme as consultas disponíveis e valide as operações necessárias com o perfil apropriado.

Antes de alterar um Perfil existente, considere que a mudança pode afetar várias contas. Use permissões compatíveis com a responsabilidade operacional; não conceda acesso total apenas para eliminar uma mensagem de bloqueio.

## Consultar o Catálogo de Permissões

**Onde:** Segurança → Catálogo de Permissões.

Use a consulta para identificar os módulos, recursos e ações disponíveis no catálogo. O catálogo é uma referência de acesso; esta tela não deve ser tratada como edição livre de novas permissões.

Se uma ação está ausente, verifique tanto a permissão do Perfil quanto a disponibilidade do módulo e recurso correspondentes. Os controles de disponibilidade também se aplicam ao Acesso Total quando a regra do catálogo assim exige.

## Consultar Auditoria

**Onde:** Segurança → Auditoria.

1. Abra a listagem.
2. Informe os filtros necessários: **Ação**, **Entidade**, **Data Inicial** e **Data Final**.
3. Escolha **Filtrar**.
4. Localize o evento e abra seus detalhes.
5. Confira autor, momento, operação e dados apresentados para aquele evento.
6. Use **Limpar** para remover os filtros e iniciar outra consulta.

Os códigos de ação e entidade devem corresponder ao que você procura. Uma lista vazia pode estar relacionada ao filtro escolhido. Use informações do evento ou orientação do responsável em vez de presumir um código.

Auditoria é evidência de operações. Ela não é uma ferramenta para editar ou desfazer registros. Para entender a sequência do negócio, consulte também o histórico funcional da Apólice ou Proposta.

## Módulos do Sistema

**Onde:** Sistema → Módulos do Sistema, quando disponível ao operador autorizado.

A tela permite consultar e controlar a habilitação de módulos e recursos. Uma alteração pode interferir no acesso de vários usuários. Confira exatamente o módulo ou recurso antes de mudar sua disponibilidade e observe o resultado da operação.

**Módulos do Sistema não são Módulos Globais do seguro.** O primeiro trata da disponibilidade de funcionalidades; o segundo é um cadastro utilizado na configuração das Apólices.

## Design System

O acesso em **Sistema → Design System** depende da autorização de operador e da configuração do ambiente. Ele apresenta referências dos componentes visuais e não é uma etapa para contratar seguro, gerar parcelas ou cadastrar Clientes.

## Quando Encaminhar ao Administrador

Informe a tela e a operação necessárias, a mensagem recebida e o contexto mínimo para identificação. A ausência de um botão pode ser esperada para o seu Perfil; encaminhe a necessidade funcional, sem solicitar uma permissão técnica por tentativa.

Credenciais e senhas temporárias devem seguir o canal autorizado da organização. Não as inclua em registros de problema, imagens do manual ou evidências de Auditoria anexadas a chamados.
