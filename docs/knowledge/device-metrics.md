# Device Metrics

**Natureza:** referência informativa; não existe especificação funcional específica identificada no mapa. Fontes: [controller](../../src/Api/Controllers/DeviceMetricsController.cs), [DTOs e validação](../../src/Api/Models/DeviceMetrics.cs), [entidades](../../src/Core/Entities/DeviceMetrics.cs), [repository](../../src/Data.Repositories/Repositories/DeviceMetricsRepository.cs).

## Conceitos e identificadores

As métricas usam `device_id` textual público na rota e convertem-no à chave numérica interna de `Devices` para persistência. O modelo atual inclui uptime, cores/uso de CPU, memória, temperatura, frequência, RSSI, dados de desconexão e contador de conexões. O repositório grava snapshot corrente e uma linha histórica.

## Operações e validações

| Operação | Contrato encontrado |
|---|---|
| `POST /api/v1/devices/{deviceId}/metrics` | Corpo `DeviceMetricsRequest`; valida obrigatoriedade e faixas: uptime >= 0, cores > 0, CPU/memória 0–100, temperatura -100–200, frequência > 0; network obrigatório com RSSI -150–0 e contadores não negativos. Device não encontrado `404`; sucesso `204`; inválido `400`. |
| `GET /api/v1/devices/{deviceId}/metrics/current` | Leitura atual; `200` ou `404` se não houver registro. |
| `GET /api/v1/devices/{deviceId}/metrics/history?from=&to=&page=1&pageSize=100` | Rejeita `page<1`, `pageSize` fora de 1–500 e `from>to` com validation problem; device inexistente `404`; resposta contém total e itens em ordem de `ReceivedAt` decrescente. |

O request também marca `device_id` no corpo como obrigatório, mas `ToEntity()` não o copia e o repository identifica o equipamento pelo `deviceId` da rota. A precedência quando os dois valores divergem é, portanto, a rota no caminho observado; essa redundância do contrato ainda não tem decisão normativa própria.

## Efeitos e falhas

POST faz upsert de `DeviceMetricsCurrent` e insere em `DeviceMetricsHistory` dentro de uma transação; timestamps de recepção são gerados pelo banco. A leitura histórica filtra intervalo inclusivo em `ReceivedAt` e pagina por offset. Não há fonte local para retenção, agregação, frequência garantida de coleta ou validação de valores semanticamente impossíveis além das faixas declaradas.

As tabelas e relacionamentos são evidência de queries; o schema MySQL autoritativo permanece incompleto (`EKM-GAP-0002`). Nenhum cenário de `tests/Api.Tests` é evidência; a suíte está aposentada.
