# Properties de um dispositivo

Uma *property* associa um nome e um valor textual a um dispositivo SmartHome. Pode ter uma descrição. O nome identifica o significado para o consumidor. A API transporta o valor como texto, sem catálogo de nomes, enumeração de valores, unidade, interpretação de JSON ou transição de estado associada. Por exemplo, o seed contém `SubscribedTopics` e o README mostra esse nome dentro de `properties` de um dispositivo. O efeito demonstrado aqui é armazenar e expor o texto. O caminho de escrita não configura um broker, publica uma mensagem nem inscreve o dispositivo em tópicos. [Seed](../../database/init.sql) · [Exemplo de Devices](../../Readme.md) · [Modelo HTTP](../../src/Api/Models/Property.cs) · [Repositório](../../src/Data.Repositories/Repositories/PropertyRepository.cs)

## Superfície HTTP

A coleção interna é `/api/v1/devices/{device_id}/properties`. Há **seis operações** em cinco métodos de controller. A tabela descreve o caminho com o repositório registrado e um MySQL acessível. Erros de entrada podem preceder qualquer acesso ao banco.

| Operação | Identidade usada | Resultado que importa para o cliente |
|---|---|---|
| GET coleção | Identificador público do dispositivo | Sem filtro efetivo, retorna uma lista ou 204. Um filtro textual efetivo produz SQL inválido. |
| GET coleção/`{id}` | Inteiro da rota | Para um inteiro válido, o comando perde o SELECT. Uma resposta de ausência não serve para consultar a existência do item. |
| POST coleção | Dispositivo previamente consultado | Pode confirmar o INSERT e ainda responder 500 ao gerar o endereço do recurso. |
| PUT coleção | Dispositivo e chaves do banco | Tenta inserir ou atualizar por colisão de chave. Responde 204 se o comando terminar com commit. |
| PATCH coleção | Mesma resolução do PUT | Recebe um objeto Property e executa o mesmo upsert. |
| DELETE coleção/`{id}` | Dispositivo previamente consultado e inteiro da rota | Com dispositivo presente, responde 204 inclusive quando nenhuma linha é removida. |

As rotas estão nos atributos de `PropertiesController`. `MapControllers` as publica. O controller recebe `IPropertyRepository` por serviço. O MVC também resolve `IDeviceRepository` registrado por DI nos parâmetros sem atributo de GET por ID, POST e DELETE. Apenas POST e DELETE chamam esse repositório. As operações diretas vão do controller ao repositório, sem um serviço de negócio intermediário. [Controller](../../src/Api/Controllers/PropertiesController.cs) · [Bootstrap](../../src/Api/Program.cs) · [Registro MySQL](../../src/Data.Repositories/DI/MySqlDependencyInjection.cs) · [Contrato do repositório](../../src/Core/Contracts/Repositories/IPropertyRepository.cs)

## Identidade, representação e entrada

| Dado | Origem e tipo | Uso e representação |
|---|---|---|
| `device_id` | Segmento obrigatório de rota, string | Corresponde a `Devices.DeviceId`. As escritas resolvem o inteiro `Devices.Id` para a coluna filha `DeviceProperties.DeviceId`. A resposta Property não inclui nenhum dos dois identificadores. |
| `id` | Segmento obrigatório apenas nas rotas de item, Int32 com sinal | Corresponde a `DeviceProperties.Id`. Não há valor mínimo imposto. Zero e negativos chegam à ação. Texto não conversível ou overflow produz 400 antes dela. O segmento não tem constraint `:int`. |
| `name` | String anulável no DTO, padrão null | Critério na lista, dado de escrita no POST e no upsert. Sai como string quando presente. |
| `value` | String anulável no DTO, padrão null | Mesmo percurso de `name`. Pode representar externamente um número ou uma lista de tópicos, mas o modelo guarda uma única string. |
| `description` | String anulável, padrão null | Critério na lista. Entra no INSERT direto e no ramo de inserção do upsert. Uma colisão no upsert conserva a descrição armazenada. A criação dentro de Devices omite a coluna. |
| `UpdatedAt` | Expressão SQL `NOW()` | O upsert grava o horário do banco. O POST direto e a criação pelo dispositivo omitem a coluna. Nenhuma leitura de Properties projeta esse horário. |

O DTO posicional contém somente `name`, `value` e `description`. A conversão para o Core copia os três sem aparar espaços, preencher ausências nem verificar tamanho. `Name` e `Value` são declarados `string` no Core, mas o operador usa o supressor de nulabilidade e pode atribuir null em execução. A identidade interna `Id` começa em zero nessa conversão. Na direção inversa, até uma entidade materializada com ID real perde o ID no DTO. Assim, a listagem direta e a representação embutida em Devices não fornecem o inteiro exigido pelo DELETE. Um membro `id` enviado no JSON não escolhe uma linha. [DTO e operadores](../../src/Api/Models/Property.cs) · [Entidade](../../src/Core/Entities/Property.cs) · [Consultas](../../src/Data.Repositories/Repositories/Queries/PropertyQuery.cs)

**Corpo de POST, PUT e PATCH.** O parâmetro `[FromBody] Property property` requer um objeto desserializável. Use `Content-Type: application/json`. O formatador Newtonsoft também aceitou `text/json` e tipos `application/*+json` no ensaio local. Com corpo e mídia incompatível, como `text/plain`, a resposta é 415. Com mídia JSON, corpo vazio, literal `null`, JSON truncado, array no lugar do objeto ou um objeto no campo string produzem 400. Esses erros ocorrem antes da ação. Não há headers de negócio obrigatórios, token de versão ou parâmetros de query usados pelas escritas.

**Campos do objeto.** Um objeto vazio, campos omitidos ou explicitamente nulos passam pelo binding. Campos desconhecidos são ignorados e nomes de membros aceitam variação de caixa. Em ensaio, `{"NAME":7,"value":true}` chegou ao repositório como as strings `"7"` e `"true"`. Espaços no corpo são preservados. Não há verificação de tamanho nem unicidade em memória. A classe declara `Validate()`, que rejeitaria nome ou valor nulo, vazio ou só com espaços. A interface própria `ISelfValidate` não aciona esse método pelo MVC. Quem o chama é um filtro aplicado a Dashboard, ausente nestas rotas. Portanto, `{}` pode chegar à escrita com nome e valor nulos. A aceitação pelo banco é outra etapa.

**Saída.** O formatador JSON omite membros nulos (`NullValueHandling.Ignore`), conserva strings vazias e serializa somente os três membros públicos do DTO. Um 204 não contém representação. A distinção entre erros do MVC e do middleware está em [Falhas e operação](#falhas-e-operação). [Bootstrap MVC e JSON](../../src/Api/Program.cs) · [Versões da API](../../src/Api/Api.csproj) · [Interface](../../src/Api/Models/ISelfValidate.cs) · [Filtro de Dashboard](../../src/Api/Middlewares/DashboardRequestFilter.cs) · [Assinaturas das ações](../../src/Api/Controllers/PropertiesController.cs)

## Operações da coleção e do item

Os exemplos usam dados fictícios e mostram os headers relevantes. O comportamento do MVC indicado como observado foi exercitado em memória com repositórios substitutos. Os efeitos de persistência e os erros dependentes de MySQL derivam dos comandos da aplicação.

### GET da coleção

`GetAllAsync` recebe um DTO da query, converte-o em entidade e o envolve em `Criteria<Property>`. Não consulta a existência do pai. Sem critério efetivo, o SQL faz `INNER JOIN Devices d ON dp.DeviceId = d.Id`, filtra `d.DeviceId = @device_id` e seleciona `dp.Id, dp.Name, dp.Description, dp.Value`. Não há ordenação, paginação, DISTINCT ou deduplicação em memória. Cada linha produz um elemento, mesmo que duas linhas tenham os mesmos textos. Se nenhuma linha casar, tanto o dispositivo inexistente quanto o dispositivo sem propriedades produzem 204. Uma ou mais linhas produzem 200.

```http
GET /api/v1/devices/sensor-demo/properties HTTP/1.1
Accept: application/json
```

```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```

```json
[{"name":"modo","value":"eco"},{"name":"sala","value":"norte","description":"Local de instalação"}]
```

A query aceita `name`, `value` e `description`. O binder também aceita o prefixo `property.`. Se escolher esse prefixo, os campos sem prefixo ficam de fora. Por exemplo, `?property.value=v&name=n` vinculou apenas `value`. Uma chave escalar repetida vinculou o primeiro valor. Chaves desconhecidas como `id` não filtram. Valores ausentes, vazios ou só com espaços não acrescentam predicado. Assim, uma query string presente pode resultar na mesma leitura sem filtro.

Cada critério não branco acrescenta um fragmento diretamente após `d.DeviceId = @device_id`, sem `AND`. Para nome e valor são igualdades parametrizadas. Para descrição o trecho é `dp.description LIKE '%' + @description + '%'`, com operador aritmético `+` no dialeto MySQL. A primeira falha já é a ausência de conector. Qualquer um desses filtros, sozinho ou combinado, torna o comando sintaticamente inválido. Em MySQL, uma consulta que alcança essa falha resulta no tratamento genérico de erro SQL (500), sem lista filtrada. [GetAllAsync](../../src/Api/Controllers/PropertiesController.cs) · [Criteria](../../src/Core/Contracts/Repositories/Criteria.cs) · [GetByDeviceAsync](../../src/Data.Repositories/Repositories/PropertyRepository.cs) · [GetAll](../../src/Data.Repositories/Repositories/Queries/PropertyQuery.cs) · [WithCriteria e Build](../../src/Data.Repositories/Repositories/Builders/PropertyQueryBuilder.cs)

### GET de um item

`GetByIdAsync` chama o repositório diretamente. `WithId` **substitui todo o SQL** por `dp.id = @id`. Só resta essa expressão, sem SELECT, FROM ou restrição do dispositivo. Embora `Build` também forneça `device_id` nos parâmetros, o comando não o referencia. O MySQL não consegue executar a expressão como consulta. O erro ocorre independentemente da existência do dispositivo ou do item. Os ramos do controller para 200 com DTO ou 404 quando o repositório devolve null pressupõem uma leitura concluída. O SQL registrado não oferece esse percurso. O erro de conversão de um ID inválido, por sua vez, antecede o SQL.

```http
GET /api/v1/devices/sensor-demo/properties/17 HTTP/1.1
```

Resposta prevista quando a conexão abre e o MySQL rejeita o comando:

```http
HTTP/1.1 500 Internal Server Error
Content-Type: application/json; charset=utf-8
X-Trace-Id: 00000000-0000-4000-8000-000000000017

Erro ao processar a requisição
```

[GetByIdAsync no controller](../../src/Api/Controllers/PropertiesController.cs) · [GetByIdAsync no repositório](../../src/Data.Repositories/Repositories/PropertyRepository.cs) · [WithId](../../src/Data.Repositories/Repositories/Builders/PropertyQueryBuilder.cs) · [ExceptionHandler](../../src/Api/Middlewares/ExceptionHandler.cs)

### POST de uma propriedade

A ação converte o DTO e consulta o dispositivo público com `GetDeviceAsync`. Ausência nessa consulta produz 404 do MVC antes do INSERT. Com o pai presente, `AddAsync` abre uma transação própria e consulta novamente `SELECT Id FROM Devices WHERE DeviceId = @device_id`. Se obtiver zero, inclusive por desaparecimento do pai entre as duas leituras, lança `NotFoundExceptionDomain`, tenta rollback e o middleware responde 404 com mensagem textual. Uma falha na primeira leitura das tabelas associadas também pode impedir a escrita.

Se o identificador público corresponder a mais de uma linha, essa segunda consulta usa o primeiro escalar, sem ORDER BY ou checagem de unicidade. A mesma resolução é usada pelo DELETE. O comando de inclusão insere o ID interno do dispositivo, nome, descrição e valor. Não há pesquisa prévia de nome duplicado nem atualização de linha existente. O lote termina em `SELECT LAST_INSERT_ID() AS NewId`, mas `ExecuteAsync` descarta o resultado. O ID da entidade continua zero. Se o banco aceitar a linha, o repositório faz commit e retorna.

Só então a ação cria um `CreatedAtAction` com o **DTO recebido**, `device_id`, `id = 0` e o nome `GetAllAsync` (a coleção). O MVC registra a ação como `GetAll` pela convenção de sufixo assíncrono. Na formatação, a geração da URL falha com `No route matches the supplied values`. Em memória, após retorno normal de `AddAsync`, o resultado foi 500 textual, sem `Location` e sem DTO. O 201 anunciado pelo atributo não descreve a resposta desse fluxo. A falha HTTP posterior não desfaz o commit. Repetir o POST pode criar outra linha ou encontrar restrição única e retornar 409.

```http
POST /api/v1/devices/sensor-demo/properties HTTP/1.1
Content-Type: application/json
```

```json
{"name":"modo","value":"eco","description":"Perfil local"}
```

```http
HTTP/1.1 500 Internal Server Error
Content-Type: application/json; charset=utf-8
X-Trace-Id: 00000000-0000-4000-8000-000000000018

Ocorreu um erro inesperado ao processar a requisição.
```

[AddAsync e CreatedAtAction](../../src/Api/Controllers/PropertiesController.cs) · [Consulta do pai](../../src/Data.Repositories/Repositories/DeviceRepository.cs) · [Transação AddAsync](../../src/Data.Repositories/Repositories/PropertyRepository.cs) · [GetIdDevice e AddForDevice](../../src/Data.Repositories/Repositories/Queries/PropertyQuery.cs) · [Configuração MVC](../../src/Api/Program.cs)

### PUT da coleção

Recebe o objeto Property inteiro. A ação converte uma cópia para log e passa o DTO ao repositório por conversão implícita. Apesar da mensagem sobre identificar o dispositivo, não chama `GetDeviceAsync`. Dentro de uma transação, executa um único `INSERT ... ON DUPLICATE KEY UPDATE`. O valor da FK vem da subconsulta escalar `(SELECT Id FROM Devices WHERE deviceId = @device_id)`. A inserção fornece `DeviceId, Name, Description, Value, UpdatedAt`, com `NOW()`. Em colisão de chave, o SQL atribui **somente** `Name, Value, UpdatedAt`. O `DeviceId` e a descrição da linha atingida permanecem como estavam. O comando constante `UpdateForDevice`, por ID, não participa.

A chave que dispara o update é a restrição primária ou única efetivamente encontrada pelo MySQL. A aplicação não impõe unicidade por dispositivo e nome. Por exemplo, com índice único nesse par, repetir o par pode alterar o valor da mesma linha. Sem uma chave conflitante, repetir insere outra linha. Uma chave única apenas sobre nome teria alcance diferente, inclusive entre dispositivos. O SQL não compara versão, valor anterior ou proprietário da linha em colisão. O cliente não recebe indicação de qual ramo ocorreu, nem quantas linhas mudaram. Mesmo com o mesmo valor, o ramo de atualização reatribui o horário.

Sem dispositivo correspondente, a subconsulta fornece NULL. O efeito depende da nulabilidade e das restrições implantadas. Se a coluna exigir valor, o erro de coluna nula cai em geral no 500 SQL. Uma violação de FK 1452 tem mapeamento próprio para 400. Mais de um dispositivo com o mesmo identificador público também impede a subconsulta escalar de retornar um único valor. A ação não produz deliberadamente o 404 anunciado para pai ou propriedade ausente. Após commit, responde 204.

```http
PUT /api/v1/devices/sensor-demo/properties HTTP/1.1
Content-Type: application/json
```

```json
{"name":"modo","value":"manual","description":"Perfil escolhido"}
```

```http
HTTP/1.1 204 No Content
```

[CreateOrUpdateAsync](../../src/Api/Controllers/PropertiesController.cs) · [Transação de upsert](../../src/Data.Repositories/Repositories/PropertyRepository.cs) · [UpSert](../../src/Data.Repositories/Repositories/Queries/PropertyQuery.cs)

### PATCH da coleção

O atributo PATCH seleciona o mesmo `CreateOrUpdateAsync` do PUT, na coleção sem ID. Tem a mesma transação, subconsulta, comportamento com dispositivo ausente, colisão de chave, resposta 204 e falhas do PUT. Um objeto contendo apenas `value` envia nome e descrição nulos ao repositório. Não há leitura do estado anterior para preencher campos omitidos. Se o objeto trouxer nome e omitir valor, null segue também para a atribuição de `Value` no ramo de colisão, sujeito às restrições do banco. Uma nova descrição só é gravada se houver inserção.

```http
PATCH /api/v1/devices/sensor-demo/properties HTTP/1.1
Content-Type: application/json
```

```json
{"name":"modo","value":"eco"}
```

```http
HTTP/1.1 204 No Content
```

Um array de operações JSON Patch, inclusive `[]` com `application/json-patch+json`, resultou em 400 de binding antes da ação. Com o mesmo media type, um objeto `{}` alcançou o repositório no ensaio. O formato do corpo e o tipo do parâmetro decidem esse comportamento. [HttpPatch e assinatura](../../src/Api/Controllers/PropertiesController.cs) · [Modelo do corpo](../../src/Api/Models/Property.cs) · [UpSert](../../src/Data.Repositories/Repositories/Queries/PropertyQuery.cs)

### DELETE de um item

Após o binding de `id`, a ação procura o dispositivo como no POST. Pai ausente nessa primeira leitura produz 404 do MVC. Com o pai presente, cria uma entidade contendo somente o ID da propriedade. O repositório abre transação, resolve novamente o ID interno do pai e executa `DELETE FROM DeviceProperties WHERE DeviceId = @deviceId AND Id = @id`. Não verifica se a linha existia, se pertencia a outro dispositivo ou quantas linhas foram afetadas. Se a segunda busca deixar de encontrar o pai, o escalar inteiro fica em zero e o DELETE ainda é executado com esse parâmetro. Se concluir e confirmar, responde 204. Um ID de outro dispositivo não remove sua linha. Repetir o DELETE com o mesmo pai ainda presente normalmente preserva o 204. Uma remoção concorrente do pai pode mudar qual caminho é tomado.

```http
DELETE /api/v1/devices/sensor-demo/properties/17 HTTP/1.1
```

```http
HTTP/1.1 204 No Content
```

[RemoveAsync no controller](../../src/Api/Controllers/PropertiesController.cs) · [RemoveAsync no repositório](../../src/Data.Repositories/Repositories/PropertyRepository.cs) · [GetIdDevice e RemoveForDevice](../../src/Data.Repositories/Repositories/Queries/PropertyQuery.cs)

## Persistência e ciclo de vida pelo dispositivo

A aplicação usa Dapper e MySqlConnector. `AddMySqlData` exige a connection string `Devices`, registra conexões `IDbConnection` transientes e repositórios por escopo. Cada repositório recebe sua conexão. Leituras e escritas passam os valores como parâmetros Dapper. A leitura direta trabalha com `Devices` e `DeviceProperties`. A verificação prévia de dispositivo em POST e DELETE lê também `DeviceSettings` através da projeção de Devices. Assim, uma falha nesse JOIN pode impedir até uma exclusão de propriedade cuja própria tabela esteja disponível. [DI](../../src/Data.Repositories/DI/MySqlDependencyInjection.cs) · [Dependências de dados](../../src/Data.Repositories/Data.Repositories.csproj) · [GetAllDevices](../../src/Data.Repositories/Repositories/Queries/DeviceQuery.cs)

**O que o DDL demonstra.** O script de inicialização se identifica como SQL Server. Nele, `DeviceProperties.Id` é uma PK inteira IDENTITY, `DeviceId` é inteiro obrigatório com FK para `Devices.Id`, `Name` e `Value` são `nvarchar(50) NOT NULL` e `Description` é `nvarchar(100) NULL`. `Devices.DeviceId` é texto obrigatório de até 50 caracteres. Nessas declarações não há UNIQUE para o identificador público ou para o par dispositivo/nome, `UpdatedAt` na filha, nem cláusula de cascata. O seed exemplifica uma propriedade sem descrição. Esses comprimentos e restrições descrevem o script histórico. Não são validações HTTP. Os scripts MySQL versionados de índices, métricas e dashboard não definem a tabela de Properties. O SQL executado exige um schema compatível inclusive com `UpdatedAt` para o upsert. A lacuna de autoridade do schema MySQL está registrada como `EKM-GAP-0002`. DDL, índices, collation, modo SQL, triggers, defaults e política de FK do MySQL implantado dependem do ambiente. [DDL e seed](../../database/init.sql) · [Índices MySQL](../../database/mysql-indexes.sql) · [DDL de métricas](../../database/device-metrics.mysql.sql) · [DDL de dashboard](../../database/dashboard-v1.mysql.sql) · [SQL usado](../../src/Data.Repositories/Repositories/Queries/PropertyQuery.cs)

| Caminho pelo recurso Devices | Consequência para Properties |
|---|---|
| POST `/api/v1/devices` | O DTO do dispositivo tem uma coleção `properties` não anulável. Uma lista vazia não cria filhos. A conversão copia cada DTO por `AddProperties`. Esse método compara IDs com os membros já existentes antes de concatenar o lote. No dispositivo novo, vários elementos com ID inicial zero podem passar juntos, inclusive com o mesmo nome. A transação insere o pai, processa capabilities, insere cada propriedade **só com o ID gerado do pai, Name e Value**, processa os vínculos de plataforma e faz commit. A descrição recebida não é enviada ao banco nesse caminho. Uma falha em outro item ou vínculo dentro da transação também pode desfazer os inserts de propriedades. Na conclusão normal, a resposta 201 aponta para o GET do dispositivo e não devolve o conjunto de filhos nem seus IDs. |
| GET `/api/v1/devices` e GET `/api/v1/devices/{device_id}` | `GetAllDevices` faz LEFT JOIN de propriedades e settings ao pai. Projeta `dp.Id, dp.Name, dp.Value, dp.Description`. Dapper separa os segmentos em `Id`. `GetByCommandAsync` agrupa os pais por **DeviceId público**. `AddProperty` ignora filho nulo e IDs já anexados. Uma propriedade repetida no resultado do JOIN com vários settings aparece uma vez por ID. Duas linhas de IDs diferentes e mesmo nome permanecem distintas. Sem filhos, o dispositivo permanece com `properties: []`. Se o identificador público estiver duplicado no banco, a agregação pode reunir filhos de pais físicos distintos. O DTO projeta os textos, omite ID e membros nulos. A lista externa retorna 200 com array, mesmo vazio. O GET individual devolve 404 se não houver pai. Nenhum deles depende do SQL quebrado do GET direto por ID. |
| PATCH `/api/v1/devices/{device_id}` | Este é JSON Patch sobre o modelo Device. O fluxo lê os filhos, converte para DTO, aplica o patch, reconverte e executa `UpdateDevice`. Mesmo que uma alteração de `properties` em memória seja aceita e o fluxo termine em 204, o UPDATE grava apenas colunas de `Devices`. Não insere, atualiza nem exclui `DeviceProperties`. Uma falha ao aplicar ou converter o patch pode interromper o fluxo antes do UPDATE. |
| DELETE `/api/v1/devices/{device_id}` | O comando ativo exclui o pai pelo ID público em uma transação. A constante `DeleteProperties` não é chamada. O README atribui a limpeza ao MySQL por cascata. Com FK em cascata, filhos são apagados junto com o pai. Uma FK restritiva pode impedir a exclusão (1451). Sem a FK, linhas órfãs podem permanecer e o INNER JOIN da lista direta deixa de expô-las. O 204 do DELETE do pai não informa quantos filhos existiam ou foram removidos. |

A consulta usada para ler o agregado parte de `GetAllDevices`, tanto no builder padrão da lista quanto na consulta individual. Os critérios de busca da lista filtram dispositivos, inclusive por settings efetivos quando chave e valor são informados. Não selecionam filhos por nome ou valor de Property. A constante `GetDevicesWithCapabilities` também contém um JOIN de propriedades, mas não é a consulta executada nesses caminhos. O exemplo `SubscribedTopics` chega ao consumidor no membro `properties`; já a verificação `HasSettingsMqtt()` do Core consulta a coleção **Settings** e sua chave de broker. A mera presença de uma Property com esses nomes não satisfaz essa verificação. [DeviceController](../../src/Api/Controllers/DeviceController.cs) · [Conversões de Device](../../src/Api/Models/Device.cs) · [AddProperty, AddProperties e HasSettingsMqtt](../../src/Core/Entities/Device.cs) · [Chaves de settings](../../src/Core/Entities/SettingsTypes.cs) · [DeviceRepository](../../src/Data.Repositories/Repositories/DeviceRepository.cs) · [DeviceQuery](../../src/Data.Repositories/Repositories/Queries/DeviceQuery.cs) · [DeviceQueryBuilder](../../src/Data.Repositories/Repositories/Queries/DeviceQueryBuilder.cs) · [Declaração de cascata](../../Readme.md)

## Falhas e operação

**Formato da falha.** O status sozinho não identifica a etapa. No pipeline MVC com `[ApiController]`, um erro de binding produz 400 com `application/problem+json`, `status`, `title`, `type`, `traceId` e um dicionário `errors` de listas de mensagens. O 415 de mídia e o `NotFound()` do controller também podem virar Problem Details, sem o dicionário de validação. Por exemplo, `GET .../properties/abc` resultou em 400 com `errors.id` e nenhuma chamada ao repositório. Um 404 da pré-consulta no POST ou DELETE indica que o pai não foi encontrado. Não confirma ausência isolada da propriedade.

Quando uma exceção chega a `ExceptionHandler` e a resposta ainda não começou, ele define `Content-Type: application/json; charset=utf-8`, gera um GUID em `X-Trace-Id` e escreve **texto diretamente**, sem aspas nem objeto JSON. O cliente que presumir JSON pelo header poderá falhar ao ler o corpo. O identificador no header pertence a esse caminho, distinto do `traceId` no corpo MVC. Se os headers já foram enviados, o middleware não substitui a resposta. O handler específico de Dashboard deixa as rotas de Properties passarem para o tratamento comum.

| Origem | Resultado do handler comum |
|---|---|
| `ArgumentDomainException` / `NotFoundExceptionDomain` | 400 / 404 com a mensagem da exceção. A checagem do pai na transação de POST usa o segundo caso. |
| MySQL 1062 | 409, “Já existe um registro com o mesmo valor”. Depende de uma chave de fato violada. |
| MySQL 1451 / 1452 | 400 por vínculo que impede exclusão ou por referência inválida. A mensagem pode identificar as tabelas se o texto da exceção permitir extraí-las. |
| MySQL 1205 / 1213 / 1040 | 503 por espera de lock, deadlock ou excesso de conexões. O 1205 também solicita parada graciosa da aplicação. |
| MySQL 2006 / 2013 ou `TimeoutException` | 503 e solicitação de parada graciosa. |
| Demais `MySqlException` | 500 com “Erro ao processar a requisição”, inclusive no tratamento de erro de sintaxe. |
| Outras exceções sem mapeamento próprio | 500 com “Ocorreu um erro inesperado ao processar a requisição.”, como a falha de formatação do POST. |

[ExceptionHandler.HandleExceptionAsync](../../src/Api/Middlewares/ExceptionHandler.cs) · [Seleção de rotas do handler de Dashboard](../../src/Api/Middlewares/DashboardExceptionHandler.cs) · [Ordem dos middlewares](../../src/Api/Program.cs)

**Atomicidade e repetição.** Cada escrita direta abre conexão e transação, confirma após o comando, tenta rollback em caso de exceção no bloco protegido, relança a exceção e fecha a conexão. A abertura e o início da transação precedem esse bloco. As pré-consultas de POST e DELETE, o commit e a entrega HTTP não formam uma transação única. O código não define isolamento explícito, lock aplicativo, ETag, `If-Match`, chave de idempotência, retry ou reconciliação entre resposta e escrita. Uma falha de conexão durante a confirmação pode deixar o cliente sem saber o resultado. Concorrência e collation influenciam a resolução dos nomes e colisões conforme o banco. Leituras usam comandos sem transação explícita. O token de cancelamento participa dos comandos construídos pelo builder, do upsert e do INSERT/DELETE direto, mas as buscas escalares por ID do pai usam overload sem token. O cancelamento do cliente não assegura desfazer uma escrita já confirmada. [Repositório direto](../../src/Data.Repositories/Repositories/PropertyRepository.cs) · [Comandos de Devices](../../src/Data.Repositories/Repositories/DeviceRepository.cs)

**Cache de leitura.** A política base global do Output Cache expira em dois segundos e alcança a listagem mesmo sem atributo na ação. Um GET elegível com 200 pode ser servido por chave de URL, incluindo query, sem chamar o repositório. No ensaio, o segundo GET trouxe `Age`; com header `Authorization`, houve nova chamada. Respostas 204 e 404 repetidas também voltaram ao repositório. Um PUT entre GETs não invalidou a entrada anterior, e após a expiração a leitura voltou ao repositório. Assim, uma consulta imediata pode mostrar a representação anterior à escrita. Não há invalidação coordenada das URLs do dispositivo e de suas propriedades. O watchdog de banco, separado das requisições, executa `SELECT 1` em intervalos de dois minutos e encerra o processo após três falhas consecutivas. Isso pode interromper a disponibilidade da API. O manifesto Swarm declara reinício com `condition: any`. [Política e middleware](../../src/Api/Program.cs) · [Watchdog](../../src/Api/Services/DatabaseWatchdogService.cs) · [Manifesto](../../docker-compose.swarm.yaml)

**Acesso e exposição.** O controller e o bootstrap não exigem identidade, escopo OAuth, autorização por dispositivo ou correspondência entre o solicitante e o pai. O caminho público no dispositivo limita as consultas que efetivamente o usam. Não constitui uma verificação de posse. CORS permite qualquer origem, header e método. O manifesto Traefik reconhece o prefixo externo `/devices`, remove esse prefixo e referencia `global-sec@file`. As regras efetivas dessa configuração externa, TLS e a exposição da instância implantada não são determináveis pelo controller. Operações registram identificadores e, em algumas escritas, a entidade cuja representação textual inclui nome e valor. [Controller](../../src/Api/Controllers/PropertiesController.cs) · [Property.ToString](../../src/Core/Entities/Property.cs) · [Bootstrap](../../src/Api/Program.cs) · [Roteadores do deploy](../../docker-compose.swarm.yaml)

## Alcance das evidências

O System Dossier e o Knowledge Map classificam Properties como inventariado por dispositivo. A revisão não contém especificação funcional própria que transforme as anotações de resposta do controller em garantias. O contrato de engenharia e o Repository Readiness habilitam um recorte de capabilities. A descrição de Properties não altera essa decisão. Os pontos de divergência verificáveis são concretos: os metadados anunciam respostas que o SQL por ID e a formatação de POST impedem, e a cascata descrita no README depende da FK do ambiente. [System Dossier, §§ 5, 6 e 10](../specs/SYSTEM-DOSSIER.md) · [Knowledge Map, Properties](../rfc/KNOWLEDGE-MAP.md) · [Contrato de engenharia](../rfc/REPOSITORY-ENGINEERING-CONTRACT.md) · [Repository Readiness](../rfc/REPOSITORY-READINESS.md)

Os resultados de binding, serialização, nomes das ações, geração de URL e cache identificados como observados correspondem a ASP.NET Core 9.0.9, MVC NewtonsoftJson e TestHost 9.0.1, com o controller da revisão e dependências de repositório substituídas. O texto dos comandos de leitura foi também obtido diretamente de `PropertyQueryBuilder.Build()`. Não houve acesso a MySQL, gateway, dispositivo ou instância da aplicação em produção. Sucesso de escrita, restrições de unicidade, cascata e erros SQL descritos dependem do caminho e do schema indicados em cada operação. [Versões referenciadas](../../src/Api/Api.csproj) · [Builder de leitura](../../src/Data.Repositories/Repositories/Builders/PropertyQueryBuilder.cs)
