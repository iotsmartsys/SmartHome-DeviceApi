# Especificação — Dashboard API v1

**ID:** `SHD-DASHBOARD-API-V1-001`

**Classe da fonte:** Normativa (proposta em rascunho)

**Versão:** 0.3

**Estado do workflow:** Rascunho [`Draft`]

**Implementação:** Não iniciada; esta atuação autoriza somente documentação.

**Relação normativa:** Nova [`New`], aditiva às APIs existentes. Esta revisão
substitui o conteúdo da revisão documental 0.2 desta mesma especificação.

## Contexto de registro e autoridade

Origem: conversa “Design de Dashboard IoT”, de 04/09/2026,
`6a373c18-d4a4-83e9-b423-30e62d60571a`. API v1 identifica a API; 0.3 identifica
esta revisão documental EKOM. A recuperação original terminou em 13.3;
esta revisão é autoria local e não atribui conteúdo adicional à conversa.

O Arquiteto ordenou a revisão 0.2 e definiu: dashboards globais; IDs reais;
float/integer → numeric, boolean → logical, open_closed/on_off → state;
line_chart planned/disabled; somente grid; os sete status da seção 11;
ausência nunca convertida em valor; erros HTTP padronizados; PUT com omitido
mantém, null reseta e objeto config mescla com defaults; x/y não negativos
e dimensões entre 1 e 4. As demais regras abaixo são detalhamento autoral
proposto para tornar essas decisões verificáveis, sem alegar confirmação
humana individual de cada default. A revisão permanece Draft para análise.

Na revisão 0.3, o Arquiteto determinou que a precedência da seção 11.2
governa também a listagem de capabilities, inclusive para tipo não suportado.

Escopo funcional: CRUD de dashboards/widgets, catálogo, compatibilidade e
renderização de valor atual. Esta ordem não autoriza implementação, build,
testes, HTTP, banco, migração, deploy nem desenvolvimento do app Swift.
Histórico, comandos de devices, multiusuário e mudança transversal de
identidade/autenticação ficam fora do recorte funcional.

Preservar controller/modelo HTTP → Core → repositórios Dapper → MySQL.
DashboardService, DashboardDataResolver e DashboardWidgetCompatibilityResolver
pertencem ao domínio Dashboard; não introduzem camada transversal.
`SHD-CAPABILITY-TYPE-ID-001@0.2`, `SHD-GROUPS-MAINTENANCE-SUPPORT-001@0.1` e
`SHD-SETTINGS-RESET-001@0.1` conservam rotas, identificadores e comportamento.

## 1. Objetivo

Permitir dashboards globais configuráveis, cujos widgets representam uma
capability e cuja compatibilidade depende do tipo de dado, sem inferência
pelo nome do sensor. A API fornece configuração e dados para o cliente renderizar.

## 2. Ownership, acesso e identidade

Existe um único contexto Dashboard por instalação da DeviceApi. Dashboards
não pertencem a usuário, residência ou cliente. Todas as chamadas admitidas
pela instalação operam sobre o mesmo conjunto; nenhum filtro owner/user/tenant
é aceito. No máximo um dashboard global pode ter isDefault=true. Zero padrões
é válido, inclusive após desmarcar ou excluir o padrão; não há eleição automática.

As novas rotas usam as condições de acesso já aplicadas à DeviceApi na
instalação, sem nova autenticação ou autorização por usuário. Isso não cria
isolamento entre clientes: quem tem acesso às rotas pode listar e alterar o
conjunto global. Não se altera a exposição de rede ou configuração do ingresso.
O bootstrap observado não configura autorização global; esta revisão não
promete proteção autenticada nem solicita publicação pública da API.

Identificadores JSON:

| Campo | Contrato |
|---|---|
| dashboardId, widgetId, id de dashboard/widget | Inteiro positivo Int64; gerado pelo banco, imutável. |
| capabilityId | Core.Entities.Capability.Id: inteiro positivo Int32. |
| deviceId | Core.Entities.Device.DeviceId/Capability.DeviceId: string pública, nunca Devices.Id numérico. |
| capabilityCode | Capability.Type, obtido de CapabilityTypes.Name; não é chave para consulta nem necessariamente único. |

O vínculo persistido do widget é capabilityId. deviceId é derivado da capability
nas respostas. No POST/PUT é opcional como verificação de consistência:
omitido ou null significa derivar; string deve identificar device existente e
coincidir exatamente com a capability, senão DEVICE_NOT_FOUND ou
DEVICE_CAPABILITY_MISMATCH. Não é permitido redirecionar a capability a outro device.
Quando a capability desaparece, deviceId e metadados derivados retornam null;
o capabilityId original e a configuração do widget são preservados.

## 3. Tipos e catálogo inicial

### 3.1 Layout e modo

Somente layoutType=grid e dataMode=current_value são aceitos. Omitidos na
criação assumem esses defaults. free_grid/list e history/aggregated_history
são reservados, rejeitados com INVALID_LAYOUT_TYPE/INVALID_DATA_MODE.

### 3.2 Mapeamento oficial do Dashboard

A fonte é Capability.DataType, lida de CapabilityTypes.DataType. Normalizar
somente espaços externos e caixa para comparação; não alterar a fonte persistida.

| Tipo da fonte | Tipo visual | semanticType |
|---|---|---|
| float, integer | numeric | null |
| boolean | logical | null |
| open_closed | state | open_closed |
| on_off, power | state | on_off |
| detection | logical | detection |
| press | state | press |
| text | text | null |
| time | event | time |

power, detection, press, text e time são extensões autorais explícitas para
os tipos encontrados na baseline; não mudam o contrato dessas capabilities.
Tipo ausente ou não listado não é inferido pelo valor/nome: dataType=null,
sem compatibleWidgets e rejeição de criação/atualização por
UNSUPPORTED_CAPABILITY_DATA_TYPE. A mudança posterior para tipo não suportado
produz invalid_value na renderização de widget já salvo somente quando
nenhuma condição de maior precedência da seção 11.2 for aplicável.

unit vem de CapabilityType.ValueSymbol: null ou vazio significa null. config.unit,
quando não null, prevalece apenas na renderização. capabilityName é Capability.Name;
deviceName é Device.Name. semanticType segue exclusivamente a tabela acima.

### 3.3 Catálogo e compatibilidade

| code | name | compatibleDataTypes | defaultDataMode | enabled | lifecycle |
|---|---|---|---|---|---|
| value_card | Card de valor | numeric, text | current_value | true | available |
| gauge | Gauge | numeric | current_value | true | available |
| state_icon | Ícone de estado | logical, state | current_value | true | available |
| status_card | Card de status | logical, state, text, event | current_value | true | available |
| line_chart | Gráfico de linha | numeric | history | false | planned |

O catálogo lista as cinco entradas, incluindo line_chart como planned/disabled.
Listas compatibleWidgets retornam somente entradas enabled=true com modo
suportado e tipo visual compatível. line_chart nunca aparece como utilizável;
sua criação/atualização retorna WIDGET_TYPE_DISABLED. O contrato de gráficos,
períodos e agregação permanece fora desta revisão.

## 4. Contrato JSON e ordenação

Payloads usam camelCase. Campos derivados ou de identidade não são graváveis,
exceto referências capabilityId/deviceId declaradas. Propriedade desconhecida
no request retorna INVALID_REQUEST; em config, INVALID_WIDGET_CONFIG.
Nulls declarados nas respostas são emitidos, independentemente do default de
serialização das APIs existentes. Não modificar a configuração global para isso.

Listas não são paginadas nesta v1. Dashboards e widgets são ordenados por
(displayOrder, id) ascendente; capabilities por capabilityId; catálogo e
compatíveis por code ordinal ascendente. displayOrder repetido é permitido.

## 5. Persistência e limites de integração

Persistir dashboards (id, name, description, layoutType, isDefault,
displayOrder, createdAt, updatedAt), widgets (id, dashboardId, capabilityId,
title, widgetType, dataMode, position, config, refreshIntervalSeconds,
displayOrder, createdAt, updatedAt) e catálogo com os campos da seção 3.3,
description e defaultConfig. Não há user_id funcional na v1.

Dashboard/widget id pode usar BIGINT; capabilityId deve preservar o domínio
Int32 da API existente. Não armazenar DeviceId público em coluna numérica.
Config é objeto JSON. Exclusão de dashboard remove seus widgets por
ON DELETE CASCADE; excluir widget não afeta capability/device.
Não criar FK de widget que impeça exclusão de capability/device nem cascade
que elimine o widget quando sua fonte desaparece: capability_missing exige
preservar o vínculo lógico original. Criação e atualização validam existência.

DDL e criação do catálogo são entregáveis futuros, não migrações executáveis
nesta autoria. EKM-GAP-0002 permanece aberta: tipos físicos, collation e
procedimento de aplicação devem ser confrontados com o schema autoritativo
na etapa apropriada. Esta funcionalidade não remedia o schema inteiro.

## 6. Criação, PUT, defaults e validações

### 6.1 Campos comuns e defaults

| Campo | Validação e default de criação/reset |
|---|---|
| name de dashboard | String aparada, 1–120 caracteres; obrigatória, sem default. |
| description | null ou string até 255 caracteres; default null. |
| title de widget | null ou string aparada de 1–120 caracteres; default null. |
| layoutType | grid. |
| isDefault | Booleano; false. |
| displayOrder | Inteiro 0–2147483647; 0. |
| capabilityId | Referência obrigatória, positiva; sem default. |
| widgetType | Código obrigatório existente e habilitado; sem default. |
| dataMode | current_value. |
| position | Objeto; default {x:0,y:0,width:1,height:1}. |
| position.x/y | Inteiros 0–2147483647. |
| position.width/height | Inteiros entre 1 e 4, inclusive. |
| refreshIntervalSeconds | null ou inteiro entre 1 e 86400; default null. |
| config | Objeto conforme seção 13; default do tipo. |

Colisões/sobreposição de posição são permitidas; não há deslocamento automático
nem limite global de colunas. refreshIntervalSeconds é sugestão ao cliente:
null não define polling; a API não agenda coleta nem altera a fonte.

### 6.2 POST

Dashboard exige name. Widget exige capabilityId e widgetType; demais campos
omitidos assumem defaults. Null aplica reset conforme tabela, rejeitado nos
campos obrigatórios sem default. position parcial completa com defaults.
Config objeto produz defaultConfig do tipo + propriedades informadas.

### 6.3 PUT de dashboard e widget

PUT é atualização parcial por decisão expressa desta API:

- campo omitido mantém o valor persistido;
- campo null reseta para o default da seção 6.1; campo obrigatório sem default
  retorna erro de validação, nunca remove nome, capabilityId ou widgetType;
- position omitido mantém, null reseta todo o objeto; objeto mescla seus
  membros sobre position atual; membro null reseta apenas aquele membro;
- config omitida mantém a configuração persistida; null substitui pelo
  defaultConfig do tipo efetivo; objeto substitui a configuração por
  defaultConfig do tipo efetivo + propriedades desse objeto. Não mescla com
  a configuração anterior. Merge é raso; chave null reseta ao default da chave;
- deviceId é apenas verificação da capability efetiva, conforme seção 2.

Exemplo para gauge salvo com max=50 e decimals=2: config omitida mantém ambos;
config=null restaura max=100 e decimals=1; config={"max":60} resulta em max=60,
decimals=1. Não reter decimals=2 nesse último caso.

Troca de widgetType com config omitida conserva o objeto anterior e o valida
contra o novo tipo; se incompatível, rejeita atomicamente. Para resetar na
mesma chamada, enviar config=null. Validar sempre o estado efetivo completo,
inclusive compatibilidade, existência da capability e habilitação do tipo.
Corpo {} válido não altera campos nem updatedAt; JSON null não é corpo válido.

### 6.4 Atomicidade e concorrência

POST/PUT inválido não persiste parcialmente. Atualizar o padrão para true
remove os demais padrões globalmente na mesma operação atômica. Chamadas
concorrentes não podem resultar em dois padrões. PUTs concorrentes são
serializados logicamente: o último a aplicar cada campo vence; campos omitidos
preservam o estado vigente no momento da aplicação. Não há ETag nesta v1.
DELETE de recurso ausente retorna 404. Excluir fonte não exclui widgets;
excluir dashboard é atômico com a remoção dos widgets.

## 7. Endpoints da API

### 7.1 Rotas e sucesso

| Método e rota | Resposta |
|---|---|
| GET /api/v1/dashboards | 200, {items:[resumoDashboard]} |
| GET /api/v1/dashboards/{dashboardId} | 200, dashboard com widgets |
| POST /api/v1/dashboards | 201, dashboard; Location para GET individual |
| PUT /api/v1/dashboards/{dashboardId} | 200, dashboard atualizado |
| DELETE /api/v1/dashboards/{dashboardId} | 204, sem corpo |
| POST /api/v1/dashboards/{dashboardId}/widgets | 201, widget; Location para GET do dashboard proprietário |
| PUT /api/v1/dashboards/{dashboardId}/widgets/{widgetId} | 200, widget atualizado |
| DELETE /api/v1/dashboards/{dashboardId}/widgets/{widgetId} | 204, sem corpo |
| GET /api/v1/dashboard-widget-types | 200, {items:[tipoWidget]} |
| GET /api/v1/dashboard-capabilities | 200, {items:[capabilityDisponivel]} |
| GET /api/v1/dashboard-capabilities/{capabilityId}/compatible-widgets | 200, capabilityId, capabilityCode, dataType e compatibleWidgets com entradas de catálogo |
| GET /api/v1/dashboards/{dashboardId}/data | 200, dashboardId, name, layoutType, generatedAt e widgets renderizados |

Todos os IDs de rota devem estar no domínio da seção 2; texto, zero, negativo
ou overflow retorna 400 INVALID_REQUEST, sem depender de constraint de rota
que os converteria em 404. Não há endpoint de controle de device.

### 7.2 Formatos de configuração

Dashboard nas respostas POST/PUT/GET individual contém id, name, description,
layoutType, isDefault, displayOrder, createdAt, updatedAt e widgets (lista vazia
na criação). Resumo de listagem contém os mesmos campos escalares e widgetCount,
sem widgets. Widget contém todos os campos persistidos da seção 5 e os derivados
deviceId, capabilityCode e dataType. Posição é um objeto com x/y/width/height.
createdAt é imutável; updatedAt=null até alteração efetiva.

Exemplo de request para criar widget:

```json
{
  "title": "Temperatura da sala",
  "deviceId": "sensor-sala",
  "capabilityId": 22,
  "widgetType": "gauge",
  "position": {"x": 0, "y": 0, "width": 1, "height": 1},
  "config": {"unit": "°C", "min": 0, "max": 50, "decimals": 1},
  "refreshIntervalSeconds": null
}
```

TipoWidget contém code, name, description, compatibleDataTypes, defaultDataMode,
defaultConfig, enabled e lifecycle. Os textos não determinam compatibilidade.

### 7.3 Capabilities disponíveis e renderização

Listar capabilities existentes, inclusive inativas ou de tipo não suportado;
não excluir silenciosamente fontes sem leitura. Cada item contém deviceId,
deviceName, capabilityId, capabilityCode, capabilityName, dataType, unit,
semanticType, currentValue, lastUpdatedAt, status e compatibleWidgets (códigos).
Valor/status usam as mesmas regras da seção 11, sem configuração de widget.
Tipo não suportado retorna dataType=null e compatibleWidgets=[]; o status
segue estritamente a precedência da seção 11.2. Não há exceção de precedência
para essa listagem. invalid_value só é retornado quando nenhuma condição
anterior for aplicável. Assim, sem erro de maior precedência, tipo não
suportado com fonte offline retorna offline; com fonte online e sem valor,
retorna no_data.
Com fonte online, valor e instante presentes, sem condição de maior
precedência, retorna invalid_value. dataType e compatibleWidgets mantêm os
valores acima em todos esses casos.

Cada widget renderizado contém widgetId, title, deviceId, capabilityId,
capabilityCode, widgetType, dataType, dataMode, value, unit, label, icon,
status, lastUpdatedAt, position, config, displayOrder e refreshIntervalSeconds.
Não omitir widgets problemáticos. title/config/layout mantêm os dados salvos;
campos derivados indisponíveis são null. Config não perde unit na renderização.

## 8. Envelope de erro e isolamento

Nas rotas desta especificação, erros retornam application/json:

```json
{
  "error": {
    "code": "INVALID_WIDGET_FOR_CAPABILITY",
    "message": "O widget gauge não é compatível com o tipo state.",
    "details": {"widgetType": "gauge", "capabilityId": 31, "capabilityDataType": "state"}
  }
}
```

code é estável; message é texto legível não contratualmente fixo; details é
objeto, vazio quando não aplicável. Validação identifica campo em details.field.
Erros não expõem SQL, stack trace ou credenciais. Falhas de parsing/model binding
nas novas rotas também usam o envelope. APIs existentes conservam seus formatos.
Não exigir formato para rejeições anteriores à aplicação, como proxy ou TLS.

Após validação sintática, verificar dashboard da rota antes de widget. Se não
existir, DASHBOARD_NOT_FOUND. Widget ausente ou pertencente a outro dashboard
retorna WIDGET_NOT_FOUND, sem revelar o proprietário e sem mutação. Depois,
validar referências e configuração. Havendo múltiplos erros de campo, qualquer
um dos códigos aplicáveis pode ser retornado; estado persistido permanece intacto.

## 9. Status HTTP e códigos

| HTTP | code / situação |
|---|---|
| 400 | INVALID_REQUEST: corpo ausente/null, JSON/tipo/campo/ID inválido. |
| 400 | INVALID_DASHBOARD_NAME: name ausente na criação, null ou inválido. |
| 400 | INVALID_LAYOUT_TYPE, INVALID_WIDGET_POSITION, INVALID_WIDGET_CONFIG, INVALID_DATA_MODE. |
| 400 | INVALID_REFRESH_INTERVAL, INVALID_DISPLAY_ORDER, INVALID_WIDGET_TITLE, INVALID_DASHBOARD_DESCRIPTION. |
| 404 | DASHBOARD_NOT_FOUND, WIDGET_NOT_FOUND, CAPABILITY_NOT_FOUND, DEVICE_NOT_FOUND, WIDGET_TYPE_NOT_FOUND. |
| 422 | WIDGET_TYPE_DISABLED, INVALID_WIDGET_FOR_CAPABILITY, UNSUPPORTED_CAPABILITY_DATA_TYPE, DEVICE_CAPABILITY_MISMATCH. |
| 405 | METHOD_NOT_ALLOWED: método não suportado em rota existente. |
| 415 | UNSUPPORTED_MEDIA_TYPE: escrita sem JSON suportado. |
| 500 | INTERNAL_ERROR: falha inesperada que impede a resposta global. |
| 503 | DATA_SOURCE_UNAVAILABLE: indisponibilidade/timeout da persistência que impede atender a chamada. |

Não usar 204/200 para erro de escrita. Status de dados por widget da seção 11
não é erro HTTP: uma resposta parcialmente utilizável continua 200. Não retornar
lista vazia ou capability_missing para esconder falha global de banco.

## 10. Responsabilidades de domínio

DashboardService coordena CRUD, defaults, validação e invariantes do domínio.
DashboardWidgetCompatibilityResolver aplica a tabela de tipos e o catálogo.
DashboardDataResolver resolve valores atuais, metadados e apresentação por widget.
Contratos/repositórios locais persistem o domínio seguindo precedentes Dapper.
A divisão interna dos métodos, queries e transações é escolha de implementação.

## 11. Conversão, status e tempo

### 11.1 Conversão do valor

Ausência nunca produz 0, false, off ou closed. Null, string vazia ou somente
espaços é ausência para todos os tipos. Fora disso:

| Tipo da fonte | Conversão JSON |
|---|---|
| float | Número finito com ponto decimal/cultura invariável, sinal e expoente permitidos; sem separador de milhar, NaN ou infinito. |
| integer | Inteiro com sinal opcional, domínio Int64; sem parte decimal nem expoente. |
| boolean | true/false textual sem distinção de caixa → booleano JSON; não aceitar 0/1. |
| detection | detected/undetected ou true/false → true/false. |
| open_closed | open/closed; true → closed, false → open, preservando a semântica existente. |
| on_off, power | on/off; true → on, false → off. |
| press | pressed/released; true → pressed, false → released. |
| text | String original, sem inferir número/booleano. |
| time | String de instante RFC 3339 com offset, normalizada para UTC; texto sem offset é inválido. |

Exceto text, remover espaços externos; tokens lógicos/de estado são comparados
sem distinção de caixa e retornam em minúsculas. Valor fora da regra é
invalid_value com value=null. Tipo/compatibilidade alterado depois da criação
que torne o widget inválido também resulta em invalid_value, assim como tipo
de widget posteriormente desabilitado. Essas condições de invalid_value
respeitam a precedência da seção 11.2; não se sobrepõem a error, offline ou
no_data. Não adaptar silenciosamente o widget.

### 11.2 Status e precedência

Avaliar na ordem abaixo; primeiro caso aplicável vence, tanto na renderização
de widgets quanto na listagem de capabilities. Tipo não suportado não altera
essa ordem:

| status | Condição | value e apresentação |
|---|---|---|
| capability_missing | Consulta bem-sucedida confirma capability inexistente. | value/label/icon/unit/lastUpdatedAt e metadados derivados null. |
| error | Falha isolada impede resolver este widget; ou fuso de origem necessário inválido/ausente. | value/label/icon=null; metadados conhecidos podem permanecer. |
| offline | Device ausente, Device.IsActive=false, Capability.Active=false ou Device.State aparado igual a offline, sem distinção de caixa. | value/label/icon=null; lastUpdatedAt conhecido permanece. |
| no_data | Sem valor, ou UpdatedAt ausente/default. | value/label/icon=null; lastUpdatedAt válido, se houver, permanece. |
| invalid_value | Valor/tipo incompatível, conversão inválida ou instante da leitura no futuro. | value/label/icon=null. |
| stale | Leitura válida com idade maior que 300 segundos. | Preservar valor convertido e apresentação, explicitamente marcados stale. |
| ok | Leitura válida, idade de 0 a 300 segundos, inclusive. | Valor convertido e apresentação normal. |

Não inferir offline pelo nome do sensor ou por leitura antiga; outras strings
Device.State não significam offline. Sem defaults falsos para falhas. Falha
de infraestrutura que impede o conjunto retorna HTTP 503/500, conforme seção 9.

### 11.3 Instantes

generatedAt e timestamps novos de Dashboard usam UTC em RFC 3339 com Z.
lastUpdatedAt vem de Capability.UpdatedAt, não de uma nova consulta à fonte
física. Como a baseline usa DATETIME sem offset, configurar explicitamente
Dashboard:SourceTimeZone com identificador IANA do fuso dos dados persistidos;
sem default implícito. Não alterar timestamps nem timezone das APIs existentes.
Instante com offset conhecido é convertido para UTC; DATETIME sem offset usa
essa configuração. Hora local ambígua/inexistente gera invalid_value; configuração
necessária ausente/inválida gera error. Nos dois casos, lastUpdatedAt=null,
pois não existe conversão confiável para um instante UTC. DateTime default
representa ausência.

A idade usa o mesmo generatedAt capturado para toda a resposta. stale não depende
do refreshIntervalSeconds. O limiar de 300 segundos é regra autoral fixa desta v1.

## 12. Apresentação

value representa a fonte convertida: invertState nunca altera value. Unidade
segue seção 3.2. Numeric usa decimals, arredondamento de ponto médio para longe
de zero, ponto decimal e número fixo de casas no label; adicionar espaço + unit
quando presente. text/event usam a string convertida como label.

Para logical, usar onLabel/onIcon ou offLabel/offIcon. Para state open_closed,
usar openLabel/openIcon ou closedLabel/closedIcon. on_off usa on/off;
press usa pressed/released. invertState troca apenas o par usado para label/icon.
status_card usa a mesma seleção; para text/event, icon=null. Outros widgets
retornam icon=null. Status gauge não representa alarme: warningFrom/dangerFrom
são configuração visual para o cliente e não substituem status de qualidade.

## 13. Configuração JSON

Merge é raso e validado conforme seção 6. Chaves não listadas para o tipo são
rejeitadas. Nenhum objeto/array aninhado é aceito como valor dessas chaves.
Config retornada contém todos os defaults e sobrescritas, inclusive nulls.

| Tipo | defaultConfig |
|---|---|
| value_card | unit=null, decimals=1, showLastUpdated=true |
| gauge | unit=null, min=0, max=100, warningFrom=null, dangerFrom=null, decimals=1 |
| state_icon, status_card | unit=null, invertState=false, onLabel=Ligado, offLabel=Desligado, onIcon=power-on, offIcon=power-off, openLabel=Aberta, closedLabel=Fechada, openIcon=door-open, closedIcon=door-closed, pressedLabel=Pressionado, releasedLabel=Liberado, pressedIcon=null, releasedIcon=null |
| line_chart | objeto vazio; disabled/planned, não configurável na v1 |

min/max/thresholds são números finitos; min < max. Threshold não null deve
estar dentro de [min,max]; se ambos presentes, warningFrom <= dangerFrom.
decimals é inteiro 0–6. showLastUpdated e invertState são booleanos.
unit é null ou string de 1–32 caracteres. Labels são strings de 1–120;
ícones são null ou strings de 1–120; aparar espaços externos antes de validar.
Nenhum lookup externo de ícones é realizado. Labels defaults não inferem
semântica de sensor; são selecionados pelo tipo da fonte conforme seção 12.

## 14. Requisitos rastreáveis

| ID | Contrato | Seções |
|---|---|---|
| DASH-001 | CRUD de dashboard com defaults, validações e ordenação definidos. | 4, 6, 7 |
| DASH-002 | Contexto global, acesso sem isolamento por usuário e no máximo um padrão, inclusive sob concorrência. | 2, 6.4 |
| DASH-003 | CRUD de widgets com vínculo válido, IDs reais e escopo aninhado. | 2, 6, 8 |
| DASH-004 | Compatibilidade pelo mapeamento oficial e tipo habilitado. | 3, 10 |
| DASH-005 | Persistência, PUT parcial, reset, merge e validação do estado efetivo. | 5, 6, 13 |
| DASH-006 | Catálogo, capabilities disponíveis e compatíveis com metadados definidos. | 3, 7 |
| DASH-007 | Valor atual, ausência, sete status, instantes e apresentação determinísticos. | 11, 12 |
| DASH-008 | Erros HTTP estruturados, locais às novas rotas e sem persistência parcial. | 6.4, 8, 9 |
| DASH-009 | Exclusão de dashboard/widget preserva fontes; exclusão da fonte preserva widget. | 5, 6.4 |
| DASH-010 | Somente grid/current_value utilizáveis; line_chart planned/disabled. | 3 |

## 15. Critérios de aceite e evidências futuras

Nenhum artefato de teste integra esta revisão. Não criar, reparar ou executar
`tests/Api.Tests`, globalmente Retired. Os cenários abaixo exigem validação
futura por HTTP e/ou leitura de banco isolado, com autorização operacional
própria. Nenhuma dessas execuções é autorizada por esta autoria documental.

| Critério / requisitos | Cenário e resultado observável | Meio |
|---|---|---|
| AC-01 / 001 | POST com name válido retorna 201/Location e defaults; GET/PUT recuperam os valores; DELETE retorna 204; repetição retorna 404; name vazio/null é rejeitado. Ordenação por displayOrder/id. | HTTP e banco isolado. |
| AC-02 / 002 | Dois clientes admitidos observam o mesmo conjunto global; criar e atualizar padrões, inclusive concorrentemente, deixa no máximo um; desmarcar/excluir permite zero. | HTTP e inspeção transacional no banco. |
| AC-03 / 003,004 | Exercitar todas as linhas do mapeamento: tipos visuais previstos e compatibilidade da matriz; tipo desconhecido não é inferido. DeviceId textual correto aceito; numérico, ausente no banco ou divergente rejeitado com código aplicável. | Fixtures conhecidas, HTTP e banco isolado. |
| AC-04 / 005 | PUT omitindo campo mantém; null reseta; null em obrigatório rejeita. Gauge max=50/decimals=2: config={max:60} produz max=60/decimals=1; config=null restaura defaults. Position parcial mantém membros omitidos e reseta nulls. | Comparação HTTP e persistência. |
| AC-05 / 006,010 | Catálogo mostra line_chart enabled=false/lifecycle=planned; nenhuma lista compatível o oferece. Sua criação é 422 WIDGET_TYPE_DISABLED; history/aggregated_history e free_grid/list retornam 400. | Inspeção HTTP. |
| AC-06 / 007 | Fixtures de conversão válida, aliases, metadados, unit e inversão: value preserva fonte; label/icon seguem config. Nulls são emitidos e config completa permanece. Número 0 e booleano false reais continuam valores válidos. | Fixtures e respostas HTTP. |
| AC-07 / 008 | IDs inválidos, JSON malformado, erros de config, recurso ausente e widget de outro dashboard retornam status/envelope da seção 9, sem mutação; APIs antigas conservam seus contratos. | HTTP e banco isolado. |
| AC-08 / 009 | Excluir widget/dashboard remove somente widgets pertinentes; devices/capabilities permanecem. Excluir capability preserva widget com capabilityId original e status capability_missing. | Banco isolado antes/depois e HTTP. |
| AC-09 / 007 | Cobrir os sete status e precedência: ausência não vira valor; offline prevalece sobre no_data; na listagem, tipo não suportado com fonte offline resulta em offline, com fonte online sem valor resulta em no_data, e com fonte online, valor presente e instante válido resulta em invalid_value, sempre sem erro de maior precedência, com dataType=null e compatibleWidgets=[]. Erro isolado não elimina os demais widgets. Idade 300s é ok, maior é stale; futuro é invalid_value; fuso ausente é error. Falha global do banco é 503. | Fixtures controladas e HTTP. |
| AC-10 / 005,008 | x/y=-1, dimensão 0/5, refresh 0/86401 e decimals 7 rejeitados; limites válidos aceitos, colisão permitida. Config desconhecida, thresholds fora da escala e min>=max rejeitados atomicamente. | HTTP e banco isolado. |
| AC-11 / 005 | Troca de tipo com config omitida inválida é rejeitada; config=null usa novos defaults. PUTs concorrentes em campos distintos preservam ambos; {} é no-op. | HTTP concorrente e banco isolado. |

Critério sem execução é evidência ausente, não aprovação. Não existe
classificação Ready para 0.3 nesta atuação de autoria.

## 16. Reconciliação da análise e dependências

Baseline desta revisão: `d32b419`, branch `spec/dashboard-api-v1`, árvore limpa
antes da correção. As análises 0.1 e 0.2 são históricas e imutáveis; não
classificam a revisão 0.3.

| Achado 0.1 | Disposição autoral vigente |
|---|---|
| B-01 / P-01 | Contexto global, acesso e unicidade definidos na seção 2; atomicidade em 6.4. |
| B-02 / P-02 | IDs, origem dos metadados, mapeamento e tipos desconhecidos em 2/3. |
| B-03 / P-03 | Catálogo planejado/desabilitado e rejeição de layouts/modos em 3. |
| B-04 / P-04 | Contrato de dados em 11; conflito residual da análise 0.2 corrigido em 3.2, 7.3 e 11 pela precedência única de 11.2, com cenário de aceite em AC-09. |
| B-05 / P-05 | Envelope, status e recursos aninhados em 8/9. |
| B-06 / P-06 | PUT, reset, defaults, limites, concorrência e apresentação em 6/12/13. |
| P-07 / EKM-GAP-0002 | Limitação de integração preservada em 5; não encerrada nem convertida em débito aceito. |

“Disposição autoral” registra onde o contrato foi detalhado, sem declarar os
bloqueadores tecnicamente encerrados. Isso depende de análise da revisão 0.3.
Nenhuma capacidade transversal foi incorporada para resolver ownership.
Dashboard:SourceTimeZone é configuração de leitura desta funcionalidade;
não altera relógio, sessões MySQL ou contratos de escrita de capabilities.

## 17. Fontes locais e encaminhamento

- [Mapa de conhecimento](../rfc/KNOWLEDGE-MAP.md).
- [Histórico e transações](../rfc/EKM-CHANGELOG.md).
- [Dossiê do sistema](SYSTEM-DOSSIER.md).
- [Capability Types por id](CAPABILITY-TYPE-ID.md).
- [Groups](GROUPS-MAINTENANCE-SUPPORT.md).
- [Reset de settings](DEVICE-SETTINGS-RESET.md).
- [Instruções locais](../../AGENTS.md).
- [Análise histórica 0.1](../reports/DASHBOARD-API-V1/analysis/2026-09-05T015215Z-0.1-9cf22d3a-0934-4360-8a4d-a9330c4c74f4-implementability-analysis.md).
- [Análise histórica 0.2](../reports/DASHBOARD-API-V1/analysis/2026-09-05T022820Z-0.2-7df58234-3f0d-48e0-a1e4-896030148778-implementability-analysis.md).
- Fontes factuais: src/Core/Entities/Capability.cs, Device.cs, CapabilityType.cs,
  DataTypes/CapabilityDataType.cs e conversores; src/Api/Models/Capability.cs;
  src/Data.Repositories/Repositories/Queries/CapabilityQuery.cs e DeviceQuery.cs;
  src/Api/Program.cs e Middlewares/ExceptionHandler.cs.
- Perfis externos consultados: roles/REGRAS-COMUNS.md e
  roles/AUTOR-DA-ESPECIFICACAO.md na raiz EKM-guidelines. Bootstrap local 4.6
  e perfis externos 4.7 permanecem sem alteração.

Revisão 0.3 registrada em Draft para nova Análise de Implementabilidade.
Sem Ready, implementação, integração em main ou encerramento Done nesta entrega.
