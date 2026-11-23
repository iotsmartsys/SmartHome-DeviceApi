# Endpoints utilitários

**Natureza:** referência informativa baseada no bootstrap [Program.cs](../../src/Api/Program.cs), [TimezoneResponse](../../src/Api/Models/TimezoneResponse.cs) e [DatabaseWatchdogService](../../src/Api/Services/DatabaseWatchdogService.cs).

## HTTP

| Operação | Comportamento observado |
|---|---|
| `GET /api/v1/timezone?zone={zone}` | Constrói resposta com o texto recebido em `timezone` e campos derivados de `DateTime.Now`, `DateTime.UtcNow`, calendário e cultura correntes. Não resolve nem converte `zone` para um fuso horário. |
| `GET /api/v1/timezone/datetime` | Retorna string `yyyy-MM-dd HH:mm:ss` de `DateTimeOffset.Now`, dependente do timezone local do processo. Apesar da descrição mencionar fuso horário, a rota não recebe parâmetro. |
| `GET /api/v1/health` | Retorna `{ status: "Healthy" }`; endpoint não verifica diretamente dependências. |

Swagger/OpenAPI e OpenAPI mapeado pelo bootstrap estão habilitados no ambiente Development; isso não constitui artefato contratual versionado completo.

## Operação hospedada

`DatabaseWatchdogService` é serviço hospedado que consulta saúde do banco periodicamente; o dossiê registra intervalo de dois minutos e encerramento do processo após falhas consecutivas. O health HTTP permanece resposta fixa, portanto não inferir que ele expõe o estado do watchdog ou garante readiness do banco.

## Limitações

Validação de timezone, comportamento para identificador inválido e política de saúde/readiness não são declaradas por especificação normativa local. Não há operação HTTP documentada aqui que altere timezone do processo.
