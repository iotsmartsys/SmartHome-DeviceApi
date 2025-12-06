# Objetivo
Esta Api tem como objetivo o gerenciamento dos dispositovs (devices) IoT SmartHome. As funcionalidades da api estão listadas na seção Endpoints, que são os métodos HTTP disponíveis para interagir com a API.

# OBSERVAÇÃO: A EXCLUSÃO USA O RECURSO NATIVO DO MYSQL, DE EXCLUSÃO EM CASCATA, POR ISSO NÃO DELETAMOS MAIS AS TABELAS FILHAS ANTES DE EXCLUIR A PRINCIPAL 

# Endpoints
Os endpoints estão divididos nas categorias CRUD (Create, Read, Update, Delete) e Gereciamento de dispositivos. A Api usa o recurso de versionamento, portanto a versão da API é passada na rota da requisição.
## Devices
### GET /devices
Retorna a lista de dispositivos cadastrados.
#### Request
```http
GET /v1/devices
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
 Exemplo de resposta:
 ```json
[
  {
    "device_id": "esp32s3-6AC178",
    "device_name": "Central Home Office",
    "last_active": "01/01/2025 03:26:23",
    "state": "Active",
    "capabilities": [
      {
        "capability_name": "LuzIndicadora",
        "type": "Light Actuator",
        "mode": "Write",
        "value": "ON"
      }
    ],
    "properties": [
      {
        "name": "SubscribedTopics",
        "value": "devices.status"
      }
    ]
  }
]
 ```
