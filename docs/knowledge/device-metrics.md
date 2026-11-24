# Device Metrics

**Estado descrito:** código em `43fbdc06e97a58f4ee55a84030a1053d5f1d997f`; revisão atual `48806770fdd486229981ae4973b70632710271e6` não alterou código, testes, banco, OpenAPI ou especificações. Referência informativa; não foi localizada especificação funcional própria para estas operações.

**Fontes:** [controller](../../src/Api/Controllers/DeviceMetricsController.cs), [request e response DTOs](../../src/Api/Models/DeviceMetrics.cs), [entidades](../../src/Core/Entities/DeviceMetrics.cs), [repository](../../src/Data.Repositories/Repositories/DeviceMetricsRepository.cs) e [OpenAPI derivado](../contracts/openapi.json).

## Modelo e identificação

A rota usa `deviceId` público textual. O repository procura `Devices.Id` por esse valor e persiste a chave numérica. O request `DeviceMetricsRequest` exige `device_id`, uptime, cores, CPU, memória, temperatura, frequência e objeto `network`; este contém RSSI, último uptime de desconexão, razão e contador de conexões. `ToEntity()` não copia o `device_id` do body. Portanto, quando o body e a rota divergem, a rota identifica o Device efetivamente usado.

Validações implementadas: uptime e contadores não negativos; cores e frequência positivos; CPU e memória entre 0 e 100; temperatura entre -100 e 200 °C; RSSI entre -150 e 0. O request exige membros numéricos não nulos por `[Required]`. As faixas são de aceitação da API, não validação de plausibilidade do dispositivo.

## Operações HTTP

### `POST /api/v1/devices/{deviceId}/metrics`

Body JSON `DeviceMetricsRequest`; os campos do modelo e seus limites estão na tabela acima. Após validação, a ação converte métricas e chama o repository com `deviceId` da rota. Se não existir Device, retorna `404`. Caso contrário, o repository resolve sua chave interna e executa na mesma transação:

1. upsert do snapshot em `DeviceMetricsCurrent`, substituindo todos os valores e `ReceivedAt=CURRENT_TIMESTAMP(3)`;
2. INSERT em `DeviceMetricsHistory` com uptime, CPU, memória, temperatura, frequência, RSSI e timestamp gerado pelo banco.

`cpu_cores`, último uptime de desconexão, razão e contador aparecem no snapshot atual, mas não são incluídos no INSERT do histórico. A transação confirma após os dois comandos; exceção tenta rollback e é relançada. Sucesso `204` sem body; inválido `400`; Device ausente `404`; falha de banco segue o pipeline de exceções. Não foi localizado teste vigente que execute e inspecione rollback de ambos os inserts.

Exemplo sintético de request:

```http
POST /api/v1/devices/demo-device/metrics
Content-Type: application/json
```

```json
{
  "device_id": "demo-device",
  "uptime_ms": 90000,
  "cpu_cores": 2,
  "cpu_percent": 25.5,
  "memory_percent": 40,
  "temperature_c": 42,
  "frequency_mhz": 800,
  "network": {
    "rssi": -60,
    "last_disconnection_uptime_ms": 0,
    "disconnection_reason": 0,
    "connection_count": 1
  }
}
```

Esse exemplo satisfaz as regras do DTO por inspeção; não representa execução real.

### `GET /api/v1/devices/{deviceId}/metrics/current`

Sem query/body. Faz join entre `Devices` e `DeviceMetricsCurrent` pela chave interna e filtra pelo ID público. `200` com `device_id`, todas as métricas do snapshot e `received_at`; `404` se não encontrar snapshot para o Device. Não consulta histórico nem calcula agregados. O DTO de saída agrupa os quatro valores de rede no objeto `network`.

Forma ilustrativa da resposta: mesmo objeto do request, com os valores persistidos e `received_at` incluído; a resposta não foi obtida de instância de produção. Evidências: [ação e projeção](../../src/Api/Controllers/DeviceMetricsController.cs), [response DTO](../../src/Api/Models/DeviceMetrics.cs), [query](../../src/Data.Repositories/Repositories/DeviceMetricsRepository.cs).

### `GET /api/v1/devices/{deviceId}/metrics/history`

Query opcional `from`, `to`, `page` (default 1) e `pageSize` (default 100). Datas são vinculadas a `DateTime`; intervalo é inclusivo em ambos os extremos. Valida `page >= 1`, `1 <= pageSize <= 500` e `from <= to`; invalidez resulta em `400` com Validation Problem. Antes da leitura, consulta existência do Device; ausente `404`.

Executa consulta de contagem e consulta paginada separadas. Ordena apenas por `ReceivedAt DESC`, usa `LIMIT/OFFSET`; não há critério de desempate quando timestamps coincidem nem transação comum que estabilize total e itens. `200` contém `page`, `page_size`, `total_items`, `total_pages` e `items`. Cada item histórico contém uptime, CPU, memória, temperatura, frequência, RSSI e `received_at`; não inclui cores, memória de rede, razão, contagem de conexões nem dispositivo. `total_pages` é zero quando não há itens e, caso contrário, o teto de `total_items/pageSize`.

Exemplo de consulta: `GET /api/v1/devices/demo-device/metrics/history?from=2026-11-23T00%3A00%3A00Z&to=2026-11-24T00%3A00%3A00Z&page=1&pageSize=100`. O intervalo é inclusivo e os defaults/limites estão descritos acima.

## Persistência, erros e limites

Os GETs elegíveis com resposta `200` podem vir do output cache base por até dois segundos; a chave usa método, esquema, path e todos os parâmetros da query, variando por host/porta, sem variação por identidade. Leituras logo após POST podem refletir dados anteriores durante esse intervalo. As condições e limites estão no [Dossiê §4](../specs/SYSTEM-DOSSIER.md#4-entradas-e-ciclo-de-vida).

As consultas identificam tabelas/colunas usadas, mas não substituem o schema MySQL autoritativo, que permanece lacuna `EKM-GAP-0002`. Não há garantia documentada de retenção, frequência mínima de ingestão ou deduplicação. Cada POST acrescenta uma linha histórica mesmo que atualize o snapshot corrente; repetir a chamada pode gerar mais histórico. Não se afirma atomicidade real além do uso da transação pelo código.

Os handlers não declaram `500` explicitamente, mas falhas de repositório podem chegar ao middleware geral. Nenhum security scheme local é aplicado pelo controller; a admissão de rede e autenticação dependem de configuração externa não disponível. A suíte `tests/Api.Tests` é `Retired` e não foi usada. O relatório de auditoria localizou 14 operações com algum teste/check automatizado na superfície total; Metrics não está entre elas.

Consulte [Devices](devices.md), [falhas e segurança no Dossiê](../specs/SYSTEM-DOSSIER.md#8-falhas-segurança-e-recuperação) e a [matriz consolidada](../reports/DEVICE-API-KNOWLEDGE-CONSOLIDATION/DEVICE-API-CONSOLIDATED-KNOWLEDGE-COVERAGE.md).
