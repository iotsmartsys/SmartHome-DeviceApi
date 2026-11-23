# Devices

**Contrato HTTP complementar:** [OpenAPI derivado](../contracts/openapi.json) e [reconciliação integral](../reports/SHD-KNOWLEDGE-MAP/2026-11-23T211211Z-c31c6cb4-2d80-42cd-9fbf-9dd40921395c-openapi-reconciliation.md). O artefato é informativo, não normativo; o relatório contém o inventário integral de operações, parâmetros, schemas, respostas e divergências deste domínio.

**Natureza:** referência informativa; contratos normativos existentes prevalecem. Código inspecionado: [DeviceController](../../src/Api/Controllers/DeviceController.cs), [modelo Device](../../src/Api/Models/Device.cs), [entidade Core](../../src/Core/Entities/Device.cs), [repositório](../../src/Data.Repositories/Repositories/DeviceRepository.cs) e [queries/builder](../../src/Data.Repositories/Repositories/Queries/DeviceQueryBuilder.cs).

## Conceitos e identificadores

Um Device representa um equipamento, com estado, atividade, protocolo, plataforma, datas, capabilities, properties e settings. `device_id` é o identificador público textual usado na rota; `id` é a chave interna numérica atribuída na persistência. Relações com capabilities e properties são carregadas pelo repositório usando a chave interna.

## Operações HTTP

| Operação | Comportamento encontrado |
|---|---|
| `GET /api/v1/devices` | Lista; filtros de query suportam DeviceId, Name, Description, Platform (lista separada por vírgula), SettingsKey + SettingsValue e IsActive. Retorna `200` com os devices. |
| `GET /api/v1/devices/{device_id}` | Consulta pelo ID público; `200` ou `404`. |
| `POST /api/v1/devices` | Converte DTO para Core, cria device e recursos embutidos; `201` com Location por `device_id`. |
| `PATCH /api/v1/devices/{device_id}` | Carrega o existente, aplica JSON Patch ao DTO e persiste; `404` se ausente, `204` após atualização. A rota preserva o `device_id` recebido. |
| `DELETE /api/v1/devices/{device_id}` | Solicita exclusão e retorna `204`; controller não verifica existência antes da chamada. |

## Regras e efeitos observados

- A criação usa uma transação no repositório. Insere capabilities e properties fornecidas no payload e associa plataformas indicadas nas capabilities antes do commit.
- Embora o DTO exponha `settings`, a conversão HTTP→Core e a rotina de criação inspecionadas não persistem settings enviadas no cadastro. A conversão Core→HTTP tampouco preenche o campo de settings do DTO. O comportamento deve ser tratado como divergência encontrada, não como suporte confirmado.
- Para uma capability cujo tipo resolva a um dos data types reconhecidos como estado de ar-condicionado, a criação normaliza/inicializa value pelo Core. Isso decorre de implementação específica e não generaliza a toda capability.
- O PATCH é JSON Patch sem whitelist local no controller. Exceções de parsing/patch e persistência seguem middleware e implementação vigentes; não há declaração local de que todo caminho seja aceito.
- Exclusão executa query por `DeviceId` dentro de transação; sem inspeção do schema autoritativo não se confirma cascade ou remoção dos recursos relacionados.

## Fluxo e limites

Fluxo usual sustentado: descobrir por `GET`, criar por `POST`, então consultar ou alterar recursos aninhados usando o `device_id`. A API não documenta aqui regra de unicidade, autenticação/autorização global, compatibilidade de versões ou ciclo de vida físico do equipamento.

**Divergência/lacuna:** `GET` de coleção declara status 500 em atributo, mas o middleware pode acrescentar outras respostas. O schema completo MySQL e efeitos de exclusão permanecem em `EKM-GAP-0002`. Consulte [Device Settings](settings.md), [Capabilities](capabilities.md), [Properties](properties.md) e [Device Metrics](device-metrics.md).
