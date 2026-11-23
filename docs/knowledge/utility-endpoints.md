# Endpoints utilitários

**Contrato HTTP complementar:** [OpenAPI derivado](../contracts/openapi.json) e [reconciliação integral](../reports/SHD-KNOWLEDGE-MAP/2026-11-23T211211Z-c31c6cb4-2d80-42cd-9fbf-9dd40921395c-openapi-reconciliation.md). O artefato é informativo, não normativo; o relatório contém o inventário integral de operações, parâmetros, schemas, respostas e divergências deste domínio.

**Natureza:** referência informativa baseada no bootstrap [Program.cs](../../src/Api/Program.cs), [TimezoneResponse](../../src/Api/Models/TimezoneResponse.cs) e [DatabaseWatchdogService](../../src/Api/Services/DatabaseWatchdogService.cs).

## HTTP

| Operação | Comportamento observado |
|---|---|
| `GET /api/v1/timezone?zone={zone}` | Constrói resposta com o texto recebido em `timezone` e campos derivados de `DateTime.Now`, `DateTime.UtcNow`, calendário e cultura correntes. Não resolve nem converte `zone` para um fuso horário. |
| `GET /api/v1/timezone/datetime` | Retorna string `yyyy-MM-dd HH:mm:ss` de `DateTimeOffset.Now`, dependente do timezone local do processo. Apesar da descrição mencionar fuso horário, a rota não recebe parâmetro. |
| `GET /api/v1/health` | Retorna `{ status: "Healthy" }`; endpoint não verifica diretamente dependências. |

Swagger/OpenAPI e OpenAPI mapeado pelo bootstrap estão habilitados no ambiente Development. [`docs/contracts/openapi.json`](../contracts/openapi.json) é um snapshot derivado fornecido pelo Arquiteto, com procedência de geração incompleta; não constitui contrato normativo completo.

## Operação hospedada

`DatabaseWatchdogService` é serviço hospedado que consulta saúde do banco periodicamente; o dossiê registra intervalo de dois minutos e encerramento do processo após falhas consecutivas. O health HTTP permanece resposta fixa, portanto não inferir que ele expõe o estado do watchdog ou garante readiness do banco.

## Limitações

Validação de timezone, comportamento para identificador inválido e política de saúde/readiness não são declaradas por especificação normativa local. Não há operação HTTP documentada aqui que altere timezone do processo.
