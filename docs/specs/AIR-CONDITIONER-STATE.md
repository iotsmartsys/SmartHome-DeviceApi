# Estado composto de ar-condicionado na Device API

**ID:** `SHD-AIR-CONDITIONER-STATE-001`
**Classe da fonte:** Normativa em elaboração
**Versão:** 0.2
**Estado do workflow:** Rascunho [`Draft`]
**Relação normativa:** Emenda [`Amends`] da versão 0.1 para inicialização, legado, modo/energia e combinação de JSON parcial. Preserva Capability Types e Dashboard.
**Registro autorizado:** revisão e reanálise solicitadas pelo Arquiteto em 23/09/2026, após confirmação das decisões abaixo. Estado documental Draft não significa implementação iniciada nem conclusão do workflow.

## Objetivo

Armazenar e atualizar separadamente energia, modo e temperatura das capabilities `air_condicionator`, utilizando a coluna existente `Capabilities.Value`, do tipo `varchar(1000)`.

## Contrato confirmado

O estado será persistido como JSON textual:

```json
{"power":"on","mode":"cool","temperature":22}
```

| Propriedade | Valores |
|---|---|
| `power` | `"on"` ou `"off"` |
| `mode` | `"cool"`, `"heat"`, `"dry"`, `"fan"` ou `"auto"` |
| `temperature` | Inteiro de 16 a 32, em Celsius |

A API continuará expondo `value` como string, contendo o JSON serializado. Não serão adicionadas colunas nem alterado o formato das demais capabilities.

Os comandos simples continuarão aceitos no fluxo de atualização:

- `"on"` e `"off"` atualizam somente `power`.
- `"16"` a `"32"` atualizam somente `temperature`.
- Os comandos de modo atualizam `mode` e definem `power` como `"on"`, preservando a temperatura.

O identificador do tipo permanece exatamente `air_condicionator`. Sua identificação deve utilizar o tipo associado à capability no banco, sem depender de um tipo informado pelo cliente na atualização.

## Inicialização e valores legados

Os valores padrão confirmados são `{"power":"off","mode":"cool","temperature":22}`. São valores de inicialização, não uma medição do aparelho. Não persistir propriedades desconhecidas como null.

Na criação sem estado, usar os padrões. Um valor legado válido preserva a informação conhecida e completa o restante: `"22"` resulta em off/cool/22, `"on"` em on/cool/22 e `"off"` em off/cool/22. Um modo legado é interpretado como o comando correspondente: `"heat"` resulta em on/heat/22. JSON armazenado válido com propriedades ausentes ou null recebe os padrões correspondentes; normalizar um estado armazenado não executa novamente o efeito de ligar de um comando de modo.

Consultas apresentam a representação normalizada como string JSON, sem escrever no banco nem alterar UpdatedAt. Na próxima atualização válida, persistir o estado normalizado combinado com a entrada. Não executar migração em massa. Conteúdo inválido não é ausência: não substituir silenciosamente estado corrompido por padrões.

## Entrada JSON parcial e precedência

O campo externo `value` continua string. Além dos comandos simples, aceita texto contendo objeto JSON parcial com `power`, `mode` e/ou `temperature`. O resultado persistido contém as três propriedades com valores válidos não nulos.

1. Partir do estado anterior normalizado; na inclusão, partir dos padrões.
2. Propriedades omitidas preservam o estado anterior, exceto o efeito explícito de modo sobre energia abaixo.
3. Propriedade presente com null restaura seu padrão: power → off, mode → cool, temperature → 22.
4. A presença de `mode`, inclusive null, define o modo e liga o aparelho se `power` estiver omitido.
5. `power` explícito prevalece sobre o efeito de modo; null é explícito e resulta em off. A ordem das chaves no JSON não altera o resultado.
6. `temperature` isolada não altera energia ou modo. Um JSON com as três propriedades usa a mesma regra de combinação, sem protocolo separado de substituição.

Partindo de on/heat/25:

| Entrada textual dentro de value | Resultado power/mode/temperature |
|---|---|
| `{"temperature":null,"power":null}` | off/heat/22 |
| `{"mode":"cool"}` | on/cool/25 |
| `{"mode":"heat","power":"off"}` | off/heat/25 |
| `{"mode":"heat","power":null}` | off/heat/25 |
| `{"mode":null}` | on/cool/25 |
| `{"temperature":22}` | on/heat/22 |

Validar os tipos e valores da tabela: temperature no objeto é número inteiro; os comandos de temperatura são texto. Rejeitar campos desconhecidos ou duplicados, JSON malformado e valores não pertencentes ao contrato, sem atualização parcial. Objeto vazio preserva o estado. Null de uma propriedade interna não equivale a enviar o campo externo value nulo; na atualização, é necessário um comando ou objeto textual válido.

A inclusão e o PATCH genérico aplicam o mesmo contrato ao value de uma capability de ar. Alterar somente metadados não equivale a reenviar o estado anterior como um novo comando; não pode religar nem sobrescrever propriedades alteradas concorrentemente. A classificação do tipo usa a associação persistida, inclusive a associação validada na inclusão.

Estado corrompido permanece protegido por 409 nas atualizações por combinação, mesmo se o JSON recebido contiver as três propriedades. Esta revisão não introduz operação de recuperação por substituição; não descartar silenciosamente o conteúdo armazenado.

## Atualização e concorrência

A atualização deve preservar as propriedades não afetadas e ser atômica por capability. Atualizações simultâneas com conjuntos de propriedades afetadas disjuntos não podem perder dados. Um comando de modo afeta mode e power; portanto, concorre com um comando de energia e segue a ordem serializada, sem promessa de independência de ordem nesse caso.

Exemplo: partindo de `on/cool/22`, receber `"off"` e `"25"` deve resultar em `off/cool/25`, independentemente da ordem de aplicação.

Para duas atualizações da mesma propriedade, prevalece a última aplicada na ordem de serialização do banco. Esta versão não introduz ordenação por horário de origem.

Entrada inválida não deve alterar o valor nem o timestamp persistido. Uma operação só pode retornar sucesso depois da confirmação da gravação.

A gravação representa o estado informado à API; não comprova, por si só, execução física pelo aparelho.

## Escopo

- Atualização de valor pelo endpoint existente.
- Leitura do estado pelas consultas existentes, inclusive smart home.
- Validação e serialização específicas do ar-condicionado.
- Proteção dos caminhos de inclusão e PATCH genérico contra gravações incompatíveis com esse estado.
- Tratamento dos valores legados com os padrões explicitamente aprovados, preservando informações conhecidas.

Preservar a arquitetura atual de API, Core e repositórios Dapper/MySQL. Não criar infraestrutura transversal de processamento de comandos.

## Fora de escopo

- Alterações na Lambda, firmware, aplicativos ou serviço MQTT.
- Publicação de comandos e eventos Alexa.
- Temperatura ambiente medida.
- Novos widgets ou suporte visual no dashboard.
- Migração em massa, deploy e alteração do schema.

O dashboard mantém o comportamento vigente para tipos não suportados. Esta especificação não amplia seu catálogo.

## Falhas e compatibilidade

- Capability ausente: `404`.
- Comando ou estado recebido inválido: `400`, sem gravação.
- Estado armazenado corrompido: impedir a atualização parcial, preservando os dados; resposta `409`.
- Falhas de banco: manter o tratamento vigente, sem convertê-las em sucesso.
- Demais tipos de capability: preservar contratos e comportamento atuais.

## Critérios de aceite

1. Desligar preserva modo e temperatura.
2. Alterar temperatura preserva energia e modo.
3. Todos os comandos permitidos são reconhecidos; valores fora do contrato são rejeitados sem escrita.
4. Atualizações concorrentes de propriedades diferentes preservam ambas.
5. Consultas devolvem `value` como string com JSON válido.
6. Inclusão e PATCH genérico respeitam o contrato do ar.
7. Valores antigos seguem a política de inicialização: defaults off/cool/22, preservação do conhecido, leitura sem escrita e persistência na próxima atualização válida.
8. Outras capabilities mantêm o comportamento anterior.
9. Modo simples ou mode presente sem power liga o aparelho; power explícito prevalece independentemente da ordem das chaves.
10. Omissão preserva o estado; null interno restaura o padrão correspondente; conferir todos os exemplos da tabela de combinação.
11. JSON malformado, campos inválidos e estado corrompido seguem 400/409 sem mutação; repetir comando válido não produz falso 404 por ausência de diferença no valor.
12. PATCH de metadados não religa o ar nem perde atualização concorrente do estado. Comandos mode/off são serializados: prevalece a última atribuição a power, mantendo mode e temperature válidos.

**Validação requerida na implementação:** build canônico da API, inspeção dos caminhos de escrita e verificação HTTP/MySQL dos critérios, incluindo concorrência. Inspeção cobre os critérios 1–12, e verificação HTTP/MySQL deve confrontar os mesmos resultados observáveis em dados isolados; ausência de execução deve ser registrada sem alegar validação operacional. Nenhum novo artefato de teste integra este rascunho; execução contra serviços depende de autorização operacional própria. A suíte `tests/Api.Tests` permanece descontinuada.

## Relação com as fontes existentes

Contrato novo para esse comportamento, preservando as especificações de Capability Types e Dashboard. O tamanho da coluna é informação confirmada pelo Arquiteto; isso não encerra a lacuna geral do schema MySQL.

A especificação não substitui a qualificação do repositório nem a análise de implementabilidade exigidas pelo EKOM.

## Fontes, pendências e encaminhamento do registro

Fontes confrontadas: [AGENTS.md](../../AGENTS.md), [mapa de conhecimento](../rfc/KNOWLEDGE-MAP.md), [dossiê](SYSTEM-DOSSIER.md), [Capability Types 0.2](CAPABILITY-TYPE-ID.md) e [Dashboard 0.3](DASHBOARD-API-V1.md). O contrato de Capability Types governa identificação/manutenção de tipos; não define o estado do ar. Dashboard §3.2 mantém tipos não listados como não suportados, sem inferência pelo conteúdo. Não foram localizados ADRs locais para este comportamento.

Decisões confirmadas em 23/09/2026 e incorporadas nesta revisão:

- **P-01 resolvida:** inicialização off/cool/22; preservar dados conhecidos e completar ausentes com padrões.
- **P-02 resolvida:** selecionar modo liga o aparelho.
- **P-03 resolvida:** aceitar JSON parcial e combinar; omissão preserva, null restaura o padrão de cada campo. Mode presente liga somente quando power está omitido; power explícito prevalece, inclusive null → off.

O relatório 0.1 e seu snapshot permanecem históricos e imutáveis. A reanálise 0.2 reconcilia B-01/B-02/B-03 com essas decisões. Não há pendência funcional remanescente desses três itens; a qualificação do repositório é condição separada.

Não há autorização de implementação, execução de testes, acesso ao banco, migração ou deploy nesta atuação. O build canônico para uma implementação futura é `dotnet build src/Api/Api.csproj`. Compilação não comprova concorrência ou persistência real.

Contrato de engenharia aprovado e Repository Readiness do recorte não foram localizados; autoria e análise não habilitam o repositório para implementação. `EKM-GAP-0001`, `EKM-GAP-0002` e `EKM-GAP-0003` permanecem abertas; nenhum débito foi aceito. A guarda `tools/validate_ekom_documents.py` indicada no AGENTS.md não foi localizada.

A análise desta versão é registrada separadamente em `docs/reports/AIR-CONDITIONER-STATE/analysis/`, vinculada ao SHA-256 deste arquivo. A transação documental é `EKM-CHG-0007`; aceitação do rascunho e registro não equivalem a prontidão técnica ou conclusão do workflow.
