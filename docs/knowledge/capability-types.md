# Capability Types

**Estado descrito:** implementação da revisão `43fbdc06e97a58f4ee55a84030a1053d5f1d997f`; sem delta funcional até `48806770fdd486229981ae4973b70632710271e6`. Referência informativa subordinada ao contrato normativo [SHD-CAPABILITY-TYPE-ID-001@0.2](../specs/CAPABILITY-TYPE-ID.md), estado `Done`.

**Fontes:** [controller](../../src/Api/Controllers/CapabilityTypeController.cs), [DTO](../../src/Api/Models/CapabilityType.cs), [icon DTO](../../src/Api/Models/CapabilityIcon.cs), [Core entity](../../src/Core/Entities/CapabilityType.cs), [repository](../../src/Data.Repositories/Repositories/CapabilityTypeRepository.cs) e queries do repositório. O [OpenAPI derivado](../contracts/openapi.json) não é normativo.

## Modelo e relação

Capability Type possui chave numérica `id` e campos `name`, `actuator_mode`, `data_type`, `computed_value`, `value_symbol`, `active_value` e `icons` (`name`, `primary_color`, `secondary_color`). `name`, `actuator_mode` e `data_type` são referências não anuláveis no record; `value_symbol`, `active_value` e `icons` são opcionais; `id` default 0 no DTO de entrada, mas é atribuído pelo INSERT e enviado no retorno. Capability se associa ao tipo por nome/key efetiva do repositório; `data_type` influencia interpretação do value, e a normalização do estado do ar é normatizada separadamente.

## Operações HTTP

| Operação | Requisição e comportamento | Resposta e limites |
|---|---|---|
| `GET /api/v1/capabilities-types?name={name}` | Query `name` opcional. Repository aplica busca contendo (`%name%`) quando informado e consulta ícones associados. | `200` com array quando existem tipos; `204` vazio. Snapshot não declara corpo/schema de `204`. · [controller/repository](../../src/Api/Controllers/CapabilityTypeController.cs) · CT-ID-002 |
| `GET /api/v1/capabilities-types/{id:int}` | Path inteiro seleciona a chave persistida; consulta tipo e ícones. Não há alias por nome. | `200` com representação completa, incluindo `id`, ou `404` se não existir. Um valor numérico no path sempre é ID, mesmo que exista nome igual. · [controller](../../src/Api/Controllers/CapabilityTypeController.cs) · CT-ID-001/003/008 |
| `POST /api/v1/capabilities-types` | Body CapabilityType sem exigir `id`; conversão não copia id recebido. Repository cria tipo e ícones dentro de transação. Não há validação de unicidade local identificada. | `201` após persistência, body com id realmente gerado e `Location: /api/v1/capabilities-types/{id}` com mesma chave. Body null resulta `400`; binding pode rejeitar campos não anuláveis antes do handler. Falha de persistência não é convertida em sucesso. · [controller/repository](../../src/Api/Controllers/CapabilityTypeController.cs) · CT-ID-004 |
| `PATCH /api/v1/capabilities-types/{id:int}` | Body JSON Patch sobre DTO. Busca exclusivamente por ID; aplica operações ao modelo; ao converter, força `entity.Id = existing.Id`, preservando identidade. Repository atualiza propriedades e conjunto de ícones em transação. Controller não declara whitelist de paths; o contrato 0.2 exige manter a identidade, e o mapeamento determina campos persistidos. | `204` após gravação ou `404` se ID ausente. JSON Patch inválido, falha de binding e persistência podem produzir outros erros pelo pipeline. Não existe seletor por nome. · [controller/repository](../../src/Api/Controllers/CapabilityTypeController.cs) · CT-ID-005/006 |
| `DELETE /api/v1/capabilities-types/{id:int}` | Path inteiro é a chave exclusiva. Repository executa exclusão por ID em transação; a ação não consulta previamente a entidade. | `204` quando existente ou inexistente, conforme contrato 0.2, inclusive repetição. Falha de FK/constraint ou persistência é relançada e não deve ser descrita como sucesso. Controller não declara 404. · [controller/repository](../../src/Api/Controllers/CapabilityTypeController.cs) · CT-ID-007 |

O contrato 0.2 removeu as antigas rotas individuais por name; este documento não as apresenta como aliases vigentes. Exemplos e critérios completos de identidade/compatibilidade constam na especificação normativa.

Exemplos estruturais de criação e patch:

```http
POST /api/v1/capabilities-types
Content-Type: application/json
```

```json
{
  "name": "Sensor",
  "actuator_mode": "read_only",
  "data_type": "float",
  "computed_value": false,
  "value_symbol": "°C",
  "icons": [{"name":"thermometer","primary_color":"#ff0000","secondary_color":null}]
}
```

```http
PATCH /api/v1/capabilities-types/12
Content-Type: application/json-patch+json
```

```json
[{"op":"replace","path":"/value_symbol","value":"°C"}]
```

Os exemplos demonstram a forma do DTO; unicidade, aceitação por constraints e persistência dependem do banco. Para identidade numérica e demais regras normativas, prevalece SHD-CAPABILITY-TYPE-ID-001.

## Exemplos de consulta

```http
GET /api/v1/capabilities-types?name=Sensor
GET /api/v1/capabilities-types/12
```

Respostas GET elegíveis `200` podem ser reutilizadas pela política base de output cache por até dois segundos; a chave usa método, esquema, path e todos os parâmetros da query, variando por host/porta, sem variação por identidade. A lista após uma escrita pode, nesse intervalo, ainda refletir valor armazenado. Veja as condições e limites no [Dossiê §4](../specs/SYSTEM-DOSSIER.md#4-entradas-e-ciclo-de-vida).

## Persistência e limites

Criação/alteração de ícones usa unidade transacional com o tipo. Leitura agrupa ícones pelo ID. Detalhes de FK, cascade, constraints e DDL completo não estão versionados (`EKM-GAP-0002`). Não se presume unicidade de name além do que efetivamente declare a instalação. A referência não declara execução integrada ou implantação; os relatórios históricos conservam evidências/decisões próprias.

Consulte [Capabilities](capabilities.md), [mapa](../rfc/KNOWLEDGE-MAP.md) e a [matriz de cobertura](../reports/DEVICE-API-KNOWLEDGE-CONSOLIDATION/DEVICE-API-CONSOLIDATED-KNOWLEDGE-COVERAGE.md).
