# Estado composto de ar-condicionado na Device API

**ID:** `SHD-AIR-CONDITIONER-STATE-001`
**Classe da fonte:** Normativa em elaboração
**Versão:** 0.1
**Estado do workflow:** Rascunho [`Draft`]
**Relação normativa:** Novo contrato [`New`], preservando Capability Types e Dashboard.
**Registro autorizado:** conteúdo aceito e registro/análise solicitados pelo Arquiteto em 22/09/2026. Pendências explícitas do texto aceito permanecem abertas; o aceite não determina respostas para elas.

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
- Os comandos de modo atualizam `mode`; o efeito sobre `power` está pendente de confirmação.

O identificador do tipo permanece exatamente `air_condicionator`. Sua identificação deve utilizar o tipo associado à capability no banco, sem depender de um tipo informado pelo cliente na atualização.

## Atualização e concorrência

A atualização deve preservar as propriedades não afetadas e ser atômica por capability. Atualizações simultâneas de propriedades diferentes não podem perder dados.

Exemplo: partindo de `on/cool/22`, receber `"off"` e `"25"` deve resultar em `off/cool/25`, independentemente da ordem de aplicação.

Para duas atualizações da mesma propriedade, prevalece a última aplicada na ordem de serialização do banco. Esta versão não introduz ordenação por horário de origem.

Entrada inválida não deve alterar o valor nem o timestamp persistido. Uma operação só pode retornar sucesso depois da confirmação da gravação.

A gravação representa o estado informado à API; não comprova, por si só, execução física pelo aparelho.

## Escopo

- Atualização de valor pelo endpoint existente.
- Leitura do estado pelas consultas existentes, inclusive smart home.
- Validação e serialização específicas do ar-condicionado.
- Proteção dos caminhos de inclusão e PATCH genérico contra gravações incompatíveis com esse estado.
- Tratamento dos valores legados, sem presumir informações desconhecidas.

Preservar a arquitetura atual de API, Core e repositórios Dapper/MySQL. Não criar infraestrutura transversal de processamento de comandos.

## Fora de escopo

- Alterações na Lambda, firmware, aplicativos ou serviço MQTT.
- Publicação de comandos e eventos Alexa.
- Temperatura ambiente medida.
- Novos widgets ou suporte visual no dashboard.
- Migração em massa, deploy e alteração do schema.

O dashboard mantém o comportamento vigente para tipos não suportados. Esta especificação não amplia seu catálogo.

## Falhas e compatibilidade propostas

- Capability ausente: `404`.
- Comando ou estado recebido inválido: `400`, sem gravação.
- Estado armazenado corrompido: impedir a atualização parcial, preservando os dados; resposta proposta `409`.
- Falhas de banco: manter o tratamento vigente, sem convertê-las em sucesso.
- Demais tipos de capability: preservar contratos e comportamento atuais.

## Critérios de aceite

1. Desligar preserva modo e temperatura.
2. Alterar temperatura preserva energia e modo.
3. Todos os comandos permitidos são reconhecidos; valores fora do contrato são rejeitados sem escrita.
4. Atualizações concorrentes de propriedades diferentes preservam ambas.
5. Consultas devolvem `value` como string com JSON válido.
6. Inclusão e PATCH genérico respeitam o contrato do ar.
7. Valores antigos seguem a política de inicialização aprovada.
8. Outras capabilities mantêm o comportamento anterior.

**Validação proposta:** build canônico da API, inspeção dos caminhos de escrita e verificação HTTP/MySQL dos critérios, incluindo concorrência. Nenhum novo artefato de teste integra este rascunho; execução contra serviços depende de autorização operacional própria. A suíte `tests/Api.Tests` permanece descontinuada.

## Relação com as fontes existentes

Contrato novo para esse comportamento, preservando as especificações de Capability Types e Dashboard. O tamanho da coluna é informação confirmada pelo Arquiteto; isso não encerra a lacuna geral do schema MySQL.

A especificação não substitui a qualificação do repositório nem a análise de implementabilidade exigidas pelo EKOM.

## Fontes, pendências e encaminhamento do registro

Fontes confrontadas: [AGENTS.md](../../AGENTS.md), [mapa de conhecimento](../rfc/KNOWLEDGE-MAP.md), [dossiê](SYSTEM-DOSSIER.md), [Capability Types 0.2](CAPABILITY-TYPE-ID.md) e [Dashboard 0.3](DASHBOARD-API-V1.md). O contrato de Capability Types governa identificação/manutenção de tipos; não define o estado do ar. Dashboard §3.2 mantém tipos não listados como não suportados, sem inferência pelo conteúdo. Não foram localizados ADRs locais para este comportamento.

As regras abaixo não foram determinadas no texto aceito e permanecem pendentes:

- **P-01:** política de valores legados e inicialização incompleta, incluindo representação de propriedades desconhecidas, comportamento de leitura anterior à primeira atualização e ausência de migração em massa. `null` foi recomendado na conversa, mas não consta do contrato aceito.
- **P-02:** efeito de um comando de modo sobre `power`.
- **P-03:** aceitação e semântica do JSON textual como entrada para inicialização/sincronização, incluindo substituição ou combinação e recuperação de estado corrompido. O formato de saída não determina o de entrada.

Não há autorização de implementação, execução de testes, acesso ao banco, migração ou deploy nesta atuação. O build canônico para uma implementação futura é `dotnet build src/Api/Api.csproj`. Compilação não comprova concorrência ou persistência real.

Contrato de engenharia aprovado e Repository Readiness do recorte não foram localizados; autoria e análise não habilitam o repositório para implementação. `EKM-GAP-0001`, `EKM-GAP-0002` e `EKM-GAP-0003` permanecem abertas; nenhum débito foi aceito. A guarda `tools/validate_ekom_documents.py` indicada no AGENTS.md não foi localizada.

A análise desta versão é registrada separadamente em `docs/reports/AIR-CONDITIONER-STATE/analysis/`, vinculada ao SHA-256 deste arquivo. A transação documental é `EKM-CHG-0007`; aceitação do rascunho e registro não equivalem a prontidão técnica ou conclusão do workflow.
