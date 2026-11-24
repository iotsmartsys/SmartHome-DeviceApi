# Devices

**Estado descrito:** implementação no commit `43fbdc06e97a58f4ee55a84030a1053d5f1d997f`; a revisão de trabalho `48806770fdd486229981ae4973b70632710271e6` não contém delta em `src`, `tests`, `database`, contratos ou especificações. Esta referência é informativa. Não há especificação normativa geral para CRUD de Devices.

**Contrato HTTP derivado:** [OpenAPI](../contracts/openapi.json), informativo e com divergências descritas abaixo; [reconciliação histórica](../reports/SHD-KNOWLEDGE-MAP/2026-11-23T211211Z-c31c6cb4-2d80-42cd-9fbf-9dd40921395c-openapi-reconciliation.md). Evidências principais: [controller](../../src/Api/Controllers/DeviceController.cs), [DTO](../../src/Api/Models/Device.cs), [entidade Core](../../src/Core/Entities/Device.cs), [contrato do repositório e filtros](../../src/Core/Contracts/Repositories/IDeviceRepository.cs), [repositório](../../src/Data.Repositories/Repositories/DeviceRepository.cs), [SQL](../../src/Data.Repositories/Repositories/Queries/DeviceQuery.cs) e [builder de leitura](../../src/Data.Repositories/Repositories/Queries/DeviceQueryBuilder.cs).

## Identidade e representação

`device_id` é a identidade pública textual usada nas rotas. `id` é a chave interna numérica obtida na inserção e utilizada por relações. Não se deve usar o `id` enviado por um cliente para escolher a nova linha. A classe HTTP `Device` é simultaneamente modelo de entrada e saída, mas as conversões e os comandos SQL dão a campos com o mesmo nome papéis diferentes.

| Campo HTTP | Tipo e obrigatoriedade efetiva em `POST` | Leitura e escrita observadas |
|---|---|---|
| `device_id` | string não nula; obrigatória | É devolvida e identifica o recurso. Enviada ao INSERT; unicidade depende do schema MySQL instalado. |
| `device_name` | string não nula; obrigatória | Mapeia para `Name`, escrita e leitura. |
| `description` | string não nula; obrigatória | Mapeia para `Description`, escrita e leitura. |
| `state` | string não nula; obrigatória | Mapeia para `Status`; escrita e leitura. |
| `mac_address` | string não nula; obrigatória | Mapeia para `MacAddress`; não há validação geral de formato no caminho de criação. |
| `ip_address` | string não nula; obrigatória | Mapeia para `IpAddress`; não há validação geral de formato no caminho de criação. |
| `protocol` | string não nula; obrigatória | `Enum.Parse<CommunicationProtocol>` é case-sensitive. Os nomes definidos são `WebServer`, `MQTT`, `HTTP`, `AMQP`, `Zigbee` e `Espnow`; falha de parse ocorre antes do repository e segue como erro 500 no tratamento geral. |
| `platform` | string não nula; obrigatória | Gravada no Device como texto. Não há allowlist local identificada para esse campo. |
| `last_active` | string anulável e opcional | Se omitida ou inválida, a conversão envia `DateTime.UtcNow`; o INSERT, de qualquer modo, grava `NOW()` e não usa o parâmetro. O formato aceito pela conversão é `yyyy-MM-dd HH:mm:ss`, apesar do nome da constante `ISO8601`. |
| `power_on` | string anulável e opcional | Conversão por `yyyy-MM-dd HH:mm:ss`; ausência ou parse inválido resulta em null. O INSERT não inclui esta coluna. |
| `is_active` | booleano; opcional por ter valor padrão | Omitido resulta em `false` no binding observado; null explícito gera 400. O INSERT não inclui `Active`, logo a aplicação não escreve esse campo nessa operação. |
| `capabilities` | coleção não nula; obrigatória; `[]` é aceita | Cada item exige pelo menos `capability_name`, `device_id` e `type` não nulos no binding. Para criação, a chave da relação vem do pai; o `device_id` repetido no item não decide a chave interna gravada. |
| `properties` | coleção não nula; obrigatória; `[]` é aceita | Os itens aceitam name/value/description anuláveis no DTO, mas a criação embutida grava somente Name e Value e não chama `Property.Validate`. Resultado diante de constraints reais depende do banco. |
| `id` | inteiro anulável, opcional | Campo de saída/leitura. Ignorado na conversão para a entidade de criação; a chave é devolvida pelo INSERT em `entity.Id`. |
| `settings` | dicionário inicializado vazio; opcional | Objeto é aceito pelo binding observado, mas não é convertido para Core nem persistido por `POST`. A conversão de saída não copia Settings; serialização com a configuração usada no ensaio produziu `{}`. |

No host local com `[ApiController]`, as dez referências não anuláveis da assinatura (oito strings e duas coleções) são validadas pelo model binding: ausência do corpo, `null`, `{}` e omissão/null de cada uma dessas propriedades produziram 400. Um campo JSON desconhecido passou no MVC padrão observado, embora o schema do snapshot declare `additionalProperties: false`. A política de campos desconhecidos de outros modelos e conversores não é generalizada a partir deste resultado.

`capabilities` e `properties` vazias são entradas válidas e evitam os respectivos loops de inclusão. Um item nulo em `capabilities` falha durante a conversão. A tabela acima não transforma o binding em garantia de persistência: a escrita, por campo, é especificada na operação POST.

## Operações HTTP

Todas as rotas desta seção pertencem à aplicação. Isso não comprova que sejam alcançáveis publicamente nem define admissão pelo gateway. Os exemplos são sintéticos e não demonstram escrita em banco.

### `GET /api/v1/devices`

Lista Devices. Aceita parâmetros de query `DeviceId`, `Name`, `Description`, `Platform`, `SettingsKey`, `SettingsValue` e `IsActive`, todos opcionais. `Platform` é dividido por vírgulas; filtros textuais vazios são ignorados. O filtro por settings só é acrescentado quando chave e valor estão ambos presentes, e consulta `v_DeviceEffectiveSettings`. Não há paginação nem ordenação explícita no builder.

O SELECT junta `Devices` a `DeviceProperties` e `DeviceSettings`; o agregador reúne properties/settings. **Não lê Capabilities** nesta operação, apesar de a resposta compartilhar o campo `capabilities`; a conversão em DTO percorre uma coleção Core vazia. O DTO de saída inicializa `settings` como `{}` e não copia os settings carregados ao Core. Portanto, arrays/objetos vazios na resposta não significam que não existam filhos/overrides persistidos.

| Resposta | Comportamento |
|---|---|
| `200 OK` | Array de Devices, inclusive se o repository devolver coleção vazia. Cada item é serializado conforme o DTO `Api.Models.Device`. |
| `500 Internal Server Error` | Declarado no controller; erros de conexão/query também seguem o middleware geral. Não há resposta 404 para coleção vazia. |

Evidências: [GetDevices/GetAllDevices](../../src/Api/Controllers/DeviceController.cs), [SELECT e filtros](../../src/Data.Repositories/Repositories/Queries/DeviceQuery.cs), [agregação e conversão](../../src/Data.Repositories/Repositories/DeviceRepository.cs), [DTO](../../src/Api/Models/Device.cs). O endpoint não possui corpo. O schema do banco pode alterar o resultado de settings efetivos.

Exemplos de chamada sem body:

```http
GET /api/v1/devices
GET /api/v1/devices?Name=Demo&Platform=demo,bridge&IsActive=true
GET /api/v1/devices/demo-device
```

Os filtros são opcionais; `Platform` aceita lista separada por vírgula. Os valores são sintéticos.

### `GET /api/v1/devices/{device_id}`

Busca pelo identificador público textual `device_id`. Reusa o mesmo SELECT e o mesmo agregador da lista; assim, carrega properties e settings específicos do SELECT, mas não carrega capabilities, e o DTO ainda devolve `settings: {}`. Se não houver linha principal, responde `404`; caso contrário `200` com um objeto Device. Falha de banco segue o tratamento geral. Não declara headers de cache.

Exemplo estrutural de resposta, ilustrativo e sem promessa de dados carregados:

```json
{
  "device_id": "demo-device",
  "device_name": "Demo",
  "description": "Synthetic device",
  "state": "active",
  "mac_address": "00:00:00:00:00:00",
  "ip_address": "192.0.2.10",
  "protocol": "AMQP",
  "platform": "demo",
  "last_active": "2026-11-24 12:00:00",
  "power_on": null,
  "is_active": false,
  "capabilities": [],
  "properties": [],
  "id": 17,
  "settings": {}
}
```

Evidências: [ação](../../src/Api/Controllers/DeviceController.cs), [consulta e conversões](../../src/Data.Repositories/Repositories/DeviceRepository.cs), [mapeamento de saída](../../src/Api/Models/Device.cs). Atributos de propriedade refletem o DTO no snapshot; a projeção descrita acima é a da implementação.

### `POST /api/v1/devices`

Recebe `application/json` (os formatters também anunciam `application/json-patch+json`, `text/json` e `application/*+json`). O corpo é `Device`; campos e obrigatoriedade efetiva estão na tabela desta referência. O OpenAPI versionado omite `requestBody.required` e os dez campos requeridos. No ensaio observado, os outros media types não substituem JSON; `text/plain` respondeu 415.

Depois do binding, a conversão:

1. faz parse do protocolo como enum case-sensitive;
2. converte datas opcionais com formato exato, usando UTC atual como valor Core para `last_active` ausente/inválido e null para `power_on` ausente/inválido;
3. converte capabilities e properties embutidas; `settings` e `id` não entram na entidade.

O repositório abre conexão e transação, insere o Device e recebe a chave interna pelo `LAST_INSERT_ID()`. O INSERT usa `DeviceId`, `Name`, `Description`, `Status`, `MacAddress`, `IpAddress`, `CommunicationTypeId` e `Platform`, e define `LastActive=NOW()` no SQL. Não escreve explicitamente `PowerOn` nem `Active`. Cada capability é resolvida pelo nome de tipo persistido; para data type de estado do ar, inicializa/normaliza `value` pelo Core e usa a query específica; as demais usam a query comum. Properties embutidas escrevem somente `Name` e `Value`; description é descartada.

Capabilities com platforms entram num caminho incompatível com o parâmetro SQL atual: o repositório passa o objeto `CapabilityPlatform` onde a query espera `platformName` escalar. O ensaio P1 reproduziu `NotSupportedException` no binding Dapper antes de executar SQL. A lista vazia não aciona esse caminho. O INSERT de associação também não grava `referenceId`. Se qualquer comando falhar, o código tenta rollback e relança; o efeito do rollback no MySQL instalado não foi exercitado. Se todos os comandos concluírem, o código chama Commit.

| Resposta | Comportamento e limite |
|---|---|
| `201 Created` | `Location` é gerado para o GET individual pelo `device_id` público; corpo vazio (o valor de retorno do `CreatedAtAction` é null). MVC com repository substituto observou esse formato, sem demonstrar persistência. |
| `400 Bad Request` | Model binding, validação automática ou middleware para erro de domínio compatível. O corpo de binding observado é `application/problem+json` com `type`, `title`, `status`, `errors` e `traceId`; o middleware geral usa outro formato. |
| `415 Unsupported Media Type` | Observado para `text/plain` no host de auditoria. |
| `500 Internal Server Error` | Erro de parse de protocolo ou exceção genérica. O ramo geral usa `application/json`, texto de mensagem e `X-Trace-Id`; não há garantia de que não ocorreu escrita se a falha for posterior ao commit. |

Exemplo de entrada compatível com o binding observado (não prova aceitação pelo schema instalado nem sucesso do INSERT):

```http
POST /api/v1/devices
Content-Type: application/json
```

```json
{
  "device_id": "demo-device",
  "device_name": "Demo",
  "description": "Synthetic device",
  "state": "active",
  "mac_address": "00:00:00:00:00:00",
  "ip_address": "192.0.2.10",
  "protocol": "AMQP",
  "platform": "demo",
  "capabilities": [],
  "properties": []
}
```

Os valores de MAC/IP são exemplos sintéticos; a aplicação não os valida como formatos de rede nesse caminho. O exemplo exclui deliberadamente platforms dentro de capabilities porque o binding Dapper desse ramo falha conforme evidência local.

Não existe chave de idempotência observada. Repetir o POST tenta outra inserção; o resultado de duplicidade depende das constraints do schema não preservado. Também não há compensação se a aplicação já confirmou a transação e a construção/transmissão da resposta falhar.

Evidências: [DTO e operadores](../../src/Api/Models/Device.cs), [CreateDevice](../../src/Api/Controllers/DeviceController.cs), [transação e loops de filhos](../../src/Data.Repositories/Repositories/DeviceRepository.cs), [projeção SQL](../../src/Data.Repositories/Repositories/Queries/DeviceQuery.cs), [enum](../../src/Core/Entities/CommunicationProtocol.cs), [datas](../../src/Core/Defaults/DateTimeExtensions.cs), [experimentos H1/H2/P1 e AUD-DEV-001/002/003/012](../reports/DEVICE-API-DOCUMENTATION-AUDIT/2026-11-24-43fbdc0/DEVICE-API-DOCUMENTATION-AUDIT.md) e [achados](../reports/DEVICE-API-DOCUMENTATION-AUDIT/2026-11-24-43fbdc0/DEVICE-API-DOCUMENTATION-AUDIT-FINDINGS.md).

### `PATCH /api/v1/devices/{device_id}`

Recebe um documento JSON Patch para `Device` (media types anunciados: `application/json-patch+json`, `application/json`, `text/json` e `application/*+json`). O controller primeiro busca pelo `device_id`; ausente resulta em `404`. Converte Core para DTO, aplica as operações sem whitelist local, converte de volta, força a chave interna recuperada do registro e preserva o `device_id` da rota. O SQL atualiza por `Id` numérico e atribui todos os campos escalares de Device, com `LastActive=NOW()`; não atualiza capabilities, properties nem settings. Campos desconhecidos/paths inválidos dependem do JSON Patch/MVC e do middleware; não há garantia de que todo path enviado seja persistido.

`204 No Content` após o repository concluir; `404` para Device não encontrado. Atributos do controller não listam outros erros, mas falhas de binding, patch ou banco podem percorrer o pipeline geral. A atualização está numa transação local, com rollback/rethrow em exceção. Não há controle de versão, condição `If-Match` nem garantia de concorrência otimista identificados.

Evidências: [ação](../../src/Api/Controllers/DeviceController.cs), [UpdateDevice SQL](../../src/Data.Repositories/Repositories/Queries/DeviceQuery.cs), [transação](../../src/Data.Repositories/Repositories/DeviceRepository.cs). O snapshot documenta o corpo como array de operações genéricas; isso não equivale a uma whitelist de propriedades.

Exemplo estrutural de alteração de um campo escalar:

```http
PATCH /api/v1/devices/demo-device
Content-Type: application/json-patch+json
```

```json
[{"op":"replace","path":"/device_name","value":"Demo v2"}]
```

O JSON Patch genérico não confirma por si só que todo caminho seja aceito ou persistido; ver a regra acima.

### `DELETE /api/v1/devices/{device_id}`

Não recebe corpo. O controller chama `DeleteAsync` diretamente e responde `204` sem consultar previamente se o Device existe. O repository executa `DELETE FROM Devices WHERE DeviceId=@DeviceId` dentro de transação e confirma mesmo quando o comando não relata linhas afetadas. Logo, a aplicação não distingue recurso ausente de exclusão concluída nesta rota. Erro de banco é relançado ao middleware geral.

O repository não chama as queries constantes de exclusão de capabilities/properties. O efeito sobre filhos depende de FKs/triggers do MySQL efetivo; não se declara cascade geral. O snapshot lista `404`, mas o caminho inspecionado não produz esse status por ausência da linha. A repetição tende a retornar `204` se o comando concluir, sem garantia sobre filhos.

Evidências: [ação](../../src/Api/Controllers/DeviceController.cs), [DeleteAsync](../../src/Data.Repositories/Repositories/DeviceRepository.cs), [SQL de exclusão e queries não chamadas](../../src/Data.Repositories/Repositories/Queries/DeviceQuery.cs), `EKM-GAP-0002` em [Knowledge Map](../rfc/KNOWLEDGE-MAP.md).

## Falhas, segurança e provas

Respostas elegíveis `200` de GET podem ser servidas pela política base de output cache por até dois segundos; a chave usa método, esquema, path e todos os parâmetros da query, variando por host/porta, sem variação por identidade. Uma leitura logo após escrita pode refletir resposta anterior. As condições e limites estão no [Dossiê §4](../specs/SYSTEM-DOSSIER.md#4-entradas-e-ciclo-de-vida).

O bootstrap não declara policy/role/scope global para as ações de Devices. CORS permissivo não prova admissão anônima, acesso externo ou autorização ponta a ponta; essa política depende da configuração do gateway/instalação, não versionada no corpus. O middleware geral trata algumas exceções de argumento, chave e MySQL, e converte outras em 500. Erro de model binding produz Problem Details; erro genérico não usa necessariamente o mesmo envelope.

As evidências HTTP de criação vêm de execução local isolada, não de produção: 28/28 corpos vazios foram rejeitados; o payload público de cinco propriedades foi rejeitado por falta de seis; um payload de dez campos alcançou o repository e retornou 201 apenas com repository substituto; a falha de plataforma foi demonstrada no binding Dapper, sem SQL/MySQL; o `GET` sintético serializou settings vazio. A suíte histórica `tests/Api.Tests` não é evidência e permanece `Retired`. A auditoria não demonstra persistência bem-sucedida de criação.

**Limites externos restantes:** DDL, defaults, triggers e constraints MySQL; cascade instalado; política APISIX por ambiente; sucesso integrado de criação e rollback físico. Essas lacunas não impedem as afirmações sobre o SQL emitido e as projeções acima. Consulte [Settings](settings.md), [Capabilities](capabilities.md), [Properties](properties.md), [Metrics](device-metrics.md), [segurança e falhas transversais no Dossiê](../specs/SYSTEM-DOSSIER.md#8-falhas-segurança-e-recuperação) e a [matriz de cobertura consolidada](../reports/DEVICE-API-KNOWLEDGE-CONSOLIDATION/DEVICE-API-CONSOLIDATED-KNOWLEDGE-COVERAGE.md).
