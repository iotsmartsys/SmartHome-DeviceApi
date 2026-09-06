# Suíte HTTP integrada — Dashboard API

**ID:** SHD-DASHBOARD-API-INTEGRATION-TESTS-001

**Versão:** 0.1

**Workflow:** In Progress

**Autoridade:** ordem humana nesta conversa para criar testes automatizados
contra a API, com criação, consulta e alteração em uma execução e exclusão
separada, utilizável depois para limpeza pela API. A ordem proíbe executar a
suíte nesta atuação. A elaboração deste contrato e sua análise registram o
recorte explicitamente solicitado; não alteram comportamento da API.

**Relação:** aditiva a [DASHBOARD-API-V1@0.3](DASHBOARD-API-V1.md).

## Contrato da suíte

| ID | Comportamento e resultado observável |
|---|---|
| INT-01 | Projeto novo `tests/Api.IntegrationTests`, C#/.NET 9, HTTP contra API já disponível; sem referência aos assemblies da API, mocks, SQL ou inicialização de servidor. Não reativar `tests/Api.Tests`. |
| INT-02 | Comando `run` executa preparação, criação, consulta e alteração ordenadas; cada cenário possui asserções e resultado. Falha interrompe cenários dependentes e produz saída não zero. Não contém teardown nem DELETE, mesmo após sucesso/falha/cancelamento. |
| INT-03 | Comando `cleanup` independente usa o manifesto da execução e apenas HTTP. Remove widgets e dashboards da suíte, verifica respostas/ausência e aceita recursos já ausentes. Não requer execução anterior bem-sucedida e pode ser repetido. |
| INT-04 | Antes da primeira escrita HTTP, persistir URL, identificador único e marcadores exatos das fixtures. Guardar IDs e resultados progressivamente, com gravação atômica e bloqueio do manifesto. Recuperar criações cuja resposta se perdeu por listagem e correspondência exata dos marcadores; nunca usar prefixo genérico para excluir. Manter manifesto após limpeza. |
| INT-05 | Dois dashboards e seus widgets são propriedade desta execução. Não excluir ou alterar fixtures de outra execução, dashboards existentes, devices, capabilities ou catálogo. Não marcar dashboard como padrão global. Confirmar identidade antes da exclusão. |
| INT-06 | URL configurável explicitamente no run, persistida para cleanup; capability opcional por ID ou seleção automática compatível. Token opcional via variável de ambiente, nunca salvo no manifesto. Não seguir redirecionamentos HTTP automaticamente nem repetir POST automaticamente. |
| INT-07 | README com comandos separados, pré-requisitos, evidências, recuperação após interrupção e limites. Compilar somente o projeto novo; não coletar nem executar testes, HTTP, limpeza ou banco nesta entrega. |

O manifesto é local e não pertence ao controle de versão. Uma nova execução
não sobrescreve manifesto existente. Limpeza não pode concorrer com uma
execução que esteja usando o mesmo manifesto: encerrar/aguardar essa execução
libera o bloqueio. Após interrupção abrupta, repetir cleanup também resolve
uma escrita HTTP que termine tardiamente. Manifesto perdido não autoriza
varrer/excluir todos os dashboards; mantê-lo é requisito operacional.

## Cenários e relação com a API 0.3

| Cenário | Asserções HTTP planejadas | Fonte |
|---|---|---|
| IT-01 Preparação | Cinco tipos, line_chart disabled/planned, ordenação e compatibilidade; selecionar capability existente sem modificá-la. | DASH-003/004/006/010, AC-03/05 (parcial) |
| IT-02 Criação de dashboards | 201/Location, IDs, defaults, timestamps e coleção vazia. | DASH-001, AC-01 |
| IT-03 Criação de widgets | 201/Location, vínculo textual do device, tipo/config/defaults e posição. | DASH-003/005, AC-03/04 (parcial) |
| IT-04 Consulta | GET individual, lista/contagem/ordenação, dados renderizados, presença de nulls e coerência entre status e valor observado. | DASH-001/006/007, AC-01/06/09 (parcial) |
| IT-05 Alteração de dashboard | PUT, GET posterior, campos omitidos preservados, reset e objeto vazio sem alterar updatedAt. | DASH-005, AC-01/04/11 |
| IT-06 Alteração de widget | PUT, GET posterior, posição parcial e reset de membro, limites válidos e campos preservados. | DASH-005, AC-04/10 |
| IT-07 Configuração | Override, objeto com defaults e null reset; exemplo max/decimals quando capability numérica disponível. | DASH-005, AC-04 |
| IT-08 Validações | Requests inválidos, limites, layout/modo, widget desabilitado, mídia/JSON inválidos, envelope e recurso aninhado incorreto; confirmar ausência de mutação. | DASH-008/010, AC-05/07/10 |
| IT-09 Concorrência e reset | PUTs em campos distintos preservam ambos, resets e no-op. | DASH-005, AC-04/11 |
| CLEAN Exclusão separada | Widgets e dashboards: 204, ausência posterior e 404 na repetição; referências existentes permanecem consultáveis. Retomar após falha parcial. | DASH-001/009, AC-01/08 (parcial), INT-03/04/05 |

A suíte observa os dados disponíveis, sem fabricar fixtures de hardware/tempo
nem interromper infraestrutura. Não cobre exaustivamente AC-02 (padrão global),
todas as conversões/precedências de AC-03/06/09, remoção da fonte/cascade físico
em AC-08, nem todos os casos combinatórios de AC-07/10/11. Cenário sem execução
permanece Not Executed, não aprovado. A suíte usa status de saída 0 para sucesso,
1 para falha de cenário/HTTP/limpeza e 2 para configuração local inválida.

## Condições de implementação e operação

Análise de implementabilidade e implementação podem ocorrer na mesma atuação
ordenada para criar a suíte. A análise registra limitações antes do código.
A política `Repository-Test-Execution-Policy.md` citada no AGENTS não está
presente; este contrato define os comandos e limites apenas desta suíte.
A lacuna da solução legada e a autoridade de schema permanecem abertas.

Execução futura requer API com schema/catálogo da 0.3 aplicado, ao menos uma
capability compatível e destino explicitamente selecionado. Criar/compilar os
artefatos não realiza essas operações. Não alterar API, schema, dependências
do produto, solução legada ou pipeline de CI nesta entrega.
