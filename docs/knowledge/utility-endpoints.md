# Endpoints utilitários

**Estado descrito:** bootstrap em `43fbdc06e97a58f4ee55a84030a1053d5f1d997f`; sem delta funcional até `48806770fdd486229981ae4973b70632710271e6`. Referência informativa, sem contrato funcional normativo próprio.

**Fontes:** [Program.cs](../../src/Api/Program.cs), [TimezoneResponse](../../src/Api/Models/TimezoneResponse.cs), [DatabaseWatchdogService](../../src/Api/Services/DatabaseWatchdogService.cs), [reconciliação OpenAPI](../reports/SHD-KNOWLEDGE-MAP/2026-11-23T211211Z-c31c6cb4-2d80-42cd-9fbf-9dd40921395c-openapi-reconciliation.md).

## Operações HTTP

### `GET /api/v1/timezone`

Query `zone` é string exigida pelo handler minimal API. Cria `TimezoneResponse` que ecoa exatamente a etiqueta recebida e calcula `day_of_week`, `day_of_year`, `datetime`, `utc_datetime` e `week_number` a partir de relógios/calendário/cultura do processo. `datetime` é `DateTime.Now` formatado com offset local; `utc_datetime` é relógio UTC. A API **não resolve nem converte** o instante ao timezone `zone`. Responde `200 application/json` com o objeto. Ausência de zone produziu 500 no ambiente Development examinado; parâmetros de binding/ambiente podem alterar a resposta. Snapshot não declara todos esses erros.

O corpo 200 elegível pode ser servido pela política base de output cache por até dois segundos. A chave inclui esquema, path e query (`zone`), variando por host/porta, sem variação por identidade; valores de relógio podem se repetir entre chamadas próximas.

### `GET /api/v1/timezone/datetime`

Sem parâmetro. Retorna `200` com string simples `DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss")`, baseada no timezone local do processo e com precisão de segundos. O texto de descrição fala em fuso específico, mas a rota não recebe fuso. A resposta é string JSON, não objeto `TimezoneResponse`.

Esse `200` elegível também pode ser reutilizado pelo output cache base até o TTL de dois segundos. A chave usa método, esquema, host/porta, path e query, sem variação por identidade; demais condições estão no [Dossiê §4](../specs/SYSTEM-DOSSIER.md#4-entradas-e-ciclo-de-vida).

### `GET /api/v1/health`

Sem parâmetros ou body. Retorna sempre `200` com `{"status":"Healthy"}` no handler. Não consulta banco, watchdog, nem readiness; resultado foi observado localmente mesmo com conexão MySQL indisponível. Não usar como prova de dependências saudáveis.

Exemplos de chamada:

```http
GET /api/v1/timezone?zone=America%2FBahia
GET /api/v1/timezone/datetime
GET /api/v1/health
```

Na primeira rota, `zone` é apenas a etiqueta de entrada/saída; não converte o relógio do processo para esse fuso.

## Outros componentes relacionados

Swagger UI, `/swagger/v1/swagger.json` e `/openapi/v1.json` são interfaces/documentos de desenvolvimento mapeados quando o ambiente é Development. Os dois geradores partem de configuração própria e divergem em metadados; o snapshot `docs/contracts/openapi.json` foi fornecido pelo Arquiteto, não tem data/revisão original identificada, e não deve ser tratado como contrato normativo completo. Sua procedência está em [contracts/README](../contracts/README.md).

`DatabaseWatchdogService` executa `SELECT 1` ao iniciar e a cada dois minutos. Após três falhas consecutivas, chama `Environment.Exit(1)` para que o ambiente de execução possa reiniciar o processo. Esse serviço hospedado é distinto do handler HTTP de health; execução prolongada não foi verificada nesta consolidação.

## Segurança, falhas e limites

`/api/v1/health` devolve o mesmo corpo fixo e uma resposta `200` elegível também pode ser servida pelo output cache base; isso não altera o fato de que o handler não consulta dependências.

O bootstrap configura CORS para qualquer origem, header e método. Isso descreve a política do middleware local, mas não comprova exposição, autenticação ou autorização no gateway. Ausência de schema de segurança no OpenAPI tampouco prova acesso anônimo. A configuração de ingresso por ambiente permanece externa.

Consulte [SYSTEM-DOSSIER §§4–8](../specs/SYSTEM-DOSSIER.md), [contrato OpenAPI](../contracts/README.md) e a [matriz de cobertura](../reports/DEVICE-API-KNOWLEDGE-CONSOLIDATION/DEVICE-API-CONSOLIDATED-KNOWLEDGE-COVERAGE.md).
