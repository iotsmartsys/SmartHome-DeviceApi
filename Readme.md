# Objetivo
Esta API gerencia dispositivos (devices) IoT SmartHome. As funcionalidades conhecidas nesta referência estão listadas na seção Endpoints.

## Limite sobre exclusão
A aplicação executa a exclusão do registro principal de Device e não remove explicitamente suas capabilities ou properties nessa operação. O efeito sobre registros relacionados depende das foreign keys, triggers e demais regras instaladas no MySQL. O schema autoritativo não está integralmente versionado; portanto, não há garantia geral de exclusão em cascata nesta referência.

# Endpoints
Os endpoints estão divididos nas categorias CRUD (Create, Read, Update, Delete) e Gereciamento de dispositivos. A Api usa o recurso de versionamento, portanto a versão da API é passada na rota da requisição.
## Devices
### GET /devices
Retorna a lista de dispositivos cadastrados.
#### Request
```http
GET /api/v1/devices
```
Exemplos de código de requisição HTTP em diferentes linguagens de programação, deserializando o JSON de resposta:

- ESP32 (C++):
```C++
#include <HTTPClient.h>
#include <ArduinoJson.h>

HTTPClient http;

#define SERVER_URL "http://iotserver.local:8081/api/v1"
#define DEVICE_URL "/devices"

void getDevices() {
  http.begin(SERVER_URL + DEVICE_URL);
  int httpCode = http.GET();
  if (httpCode > 0) {
    if (httpCode == HTTP_CODE_OK) {
      String payload = http.getString();
      StaticJsonDocument<200> doc;
      deserializeJson(doc, payload);
      JsonArray devices = doc.as<JsonArray>();
      for (JsonObject device : devices) {
        Serial.println(device["device_name"].as<String>());
      }
    }
  }
  http.end();
}
```





#### Respostas
- 200: Sucesso
Exemplo sintético da forma do DTO retornado:
 ```json
[
  {
    "device_id": "demo-device",
    "device_name": "Demo device",
    "description": "Synthetic example",
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
]
 ```

No GET comum de Device, a consulta não carrega capabilities; `capabilities: []` não prova que o Device não tenha relações persistidas. O DTO também devolve `settings: {}` nesta projeção. Consulte a [referência de Devices](docs/knowledge/devices.md) para campos, filtros, projeções e limites.
