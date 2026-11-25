# Properties de um dispositivo

Uma propriedade associa um nome a um valor textual e pode ter uma descrição. A API persiste esse dado no cadastro de um dispositivo. Gravar uma propriedade não envia comando ao dispositivo, não publica mensagem e não comprova alteração física. O par nome/valor pode representar, por exemplo, um tópico de inscrição. A aplicação trata seu conteúdo como texto livre. [Exemplo de cadastro](../../Readme.md) · [Modelo HTTP](../../src/Api/Models/Property.cs) · [Persistência](../../src/Data.Repositories/Repositories/PropertyRepository.cs)

A coleção tem endereço `/api/v1/devices/{device_id}/properties`. Há seis operações diretas. O dispositivo também pode incorporar propriedades na criação e devolvê-las em suas consultas.

| Método e alvo | Identificação | Resultado material |
|---|---|---|
| GET coleção | Dispositivo e critérios de query | Sem critérios efetivos, lista ou 204. Qualquer critério efetivo produz SQL inválido. |
| GET coleção/`{id}` | Dispositivo e ID inteiro na rota | Com ID válido, o comando enviado ao MySQL é incompleto. |
| POST coleção | Dispositivo na rota | Insere e confirma antes de uma falha na geração do endereço de resposta. |
| PUT coleção | Dispositivo e campos do objeto | Tenta inserir. Uma colisão de chave do banco aciona atualização. |
| PATCH coleção | Mesmo objeto do PUT | Executa o mesmo *upsert*, inclusive para uma propriedade nova. |
| DELETE coleção/`{id}` | Dispositivo e ID inteiro na rota | Para dispositivo encontrado, 204 também quando nenhuma propriedade foi excluída. |

O dossiê e o mapa registram Properties como `Inventoried`, com escopo por dispositivo e referência ao código. Não estabelecem um contrato funcional detalhado para esses seis resultados. Os atributos `ProducesResponseType` descrevem respostas anunciadas pelo controller. As diferenças entre esses metadados e o caminho executável aparecem abaixo. A qualificação de engenharia aprovada abrange o recorte de capabilities do ar-condicionado. [Dossiê](../specs/SYSTEM-DOSSIER.md) · [Mapa](../rfc/KNOWLEDGE-MAP.md) · [Readiness](../rfc/REPOSITORY-READINESS.md) · [PropertiesController](../../src/Api/Controllers/PropertiesController.cs)

## Identidade, representação e vínculo

| Dado | Entrada e conversão | Escrita e retorno |
|---|---|---|
| `device_id` | Segmento de rota textual obrigatório. Corresponde a `Devices.DeviceId`. | O vínculo interno usa `DeviceProperties.DeviceId = Devices.Id`, numérico. O DTO não contém o identificador do pai. |
| `id` | Segmento no GET unitário e no DELETE. Inteiro de 32 bits com sinal. Zero e negativos passam pelo binding. | `Property.Id` é inteiro, inicialmente zero. A consulta de coleção lê `dp.Id`, mas a conversão para JSON descarta o campo. Nenhuma escrita aceita o ID do cliente no corpo. |
| `name` | String anulável. É copiada para `Name`. | Usada em inserção, atualização por colisão e critério de igualdade. Retornada quando presente. |
| `value` | String anulável. É copiada para `Value`, sem interpretação por tipo ou unidade. | Mesmo tratamento de escrita, projeção e critério que `name`. |
| `description` | String anulável, copiada para `Description`. | Lida da linha e enviada nas inserções diretas. A atualização por colisão preserva a descrição armazenada. A inserção dentro do cadastro do dispositivo omite a coluna. |
| `UpdatedAt` | Ausente do DTO e da entidade. | Apenas o *upsert* atribui explicitamente `NOW()`, inclusive na atualização. Não é lido ou serializado. |

O DTO posicional é o mesmo para corpo, query e representação de saída. A entidade Core declara `Name` e `Value` como strings não anuláveis, mas a conversão usa o operador de supressão de aviso de nulabilidade. Valores nulos atravessam a conversão. Não há enumeração, comprimento máximo, normalização de caixa, remoção de espaços ou conversão de unidade na entidade. A comparação textual no banco depende da collation efetiva. O GET de coleção e a representação incorporada ao dispositivo não fornecem o ID necessário para o DELETE. [Modelo e conversões](../../src/Api/Models/Property.cs) · [Entidade Core](../../src/Core/Entities/Property.cs) · [Comandos SQL](../../src/Data.Repositories/Repositories/Queries/PropertyQuery.cs)

## Contrato HTTP compartilhado

O MVC registra o controller por atributos com `MapControllers`. O identificador do dispositivo vem da rota. O `id` do item é inferido da rota, sem restrição `:int` no template. Um segmento não conversível ou fora da faixa gera 400 antes da action. A ausência do segmento final seleciona a rota de coleção, conforme o verbo. Os repositórios vêm da DI, inclusive o parâmetro `IDeviceRepository` sem atributo nas actions que o declaram. O token de cancelamento vem da requisição. Não há query obrigatória, header próprio, paginação, chave de idempotência, ETag ou precondição de escrita.

POST, PUT e PATCH recebem **um objeto** no corpo. Use `Content-Type: application/json`. O formatador Newtonsoft do MVC também atende JSON textual e tipos `application/*+json` compatíveis. Corpo obrigatório ausente, literal `null`, JSON malformado ou array de operações em lugar do objeto podem produzir 400 no binding. Com corpo e tipo sem formatador, como `text/plain`, a resposta é 415. Para erros de modelo, o MVC produz `application/problem+json` com status, título, tipo, traceId e erros por campo. Um 404 gerado por `NotFound()` usa o tratamento de erro de cliente do MVC.

Os três membros do objeto são anuláveis. Membro ausente ou nulo chega como nulo. `{}` é um objeto vinculável. `Property.Validate()` rejeitaria nome e valor em branco, mas estas actions não chamam o método. O filtro que invoca `ISelfValidate` está associado ao Dashboard. Strings vazias ou só com espaços podem, portanto, chegar à escrita. O Newtonsoft aceita nomes de membros sem distinguir caixa, ignora membros desconhecidos, inclusive `id`, e pode converter escalares para string. Por exemplo, um `value` numérico 123 chegou ao repositório isolado como texto. Restrições posteriores do banco podem rejeitar uma entrada aceita pelo MVC.

A representação JSON usa `name`, `value` e `description`. O formatador omite membros nulos. A resposta 204 encerra sem representação. Os exemplos a seguir usam dados fictícios, mostram os headers materiais e separam os corpos JSON dos cabeçalhos HTTP. [Bootstrap e configurações MVC](../../src/Api/Program.cs) · [Versões e nulabilidade](../../src/Api/Api.csproj) · [Controller](../../src/Api/Controllers/PropertiesController.cs) · [Validação do DTO](../../src/Api/Models/Property.cs) · [Filtro do Dashboard](../../src/Api/Middlewares/DashboardRequestFilter.cs)

## GET da coleção

`GetAllAsync` recebe `[FromQuery] Property`, converte o modelo em `Criteria<Property>` e chama `GetByDeviceAsync`. O SQL base seleciona `dp.Id, dp.Name, dp.Description, dp.Value` de `DeviceProperties dp INNER JOIN Devices d ON dp.DeviceId = d.Id` com `WHERE d.DeviceId = @device_id`. Não verifica antes a existência do pai. Sem linhas, tanto um dispositivo ausente quanto um existente sem propriedades produzem 204. Havendo linhas, responde 200 com um array. Não há `DISTINCT`, `ORDER BY`, limite ou preferência por um dos nomes repetidos.

Os parâmetros de query reconhecidos são `name`, `value` e `description`. O binding do modelo complexo admite o prefixo do parâmetro, como `property.name`. Critérios nulos, vazios ou só de espaços não acrescentam filtro. Parâmetros desconhecidos, como `id`, não filtram por identidade. Para cada critério efetivo, o construtor acrescenta literalmente `dp.name = @name`, `dp.value = @value` ou `dp.description LIKE '%' + @description + '%'`, nessa ordem, **sem conjunção com o predicado anterior**. Até um único critério torna a instrução inválida. A expressão da descrição também usa `+`, operador aritmético no MySQL. Uma busca por descrição não implementa pesquisa de substring utilizável. Um erro de sintaxe MySQL segue o caminho 500 de banco descrito adiante.

```http
GET /api/v1/devices/laboratorio/properties HTTP/1.1
Host: casa.example
Accept: application/json
```

```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```

```json
[{"name":"zona","value":"sul"}]
```

Neste exemplo a linha tem descrição nula. O corpo contém apenas os campos públicos. [GetAllAsync](../../src/Api/Controllers/PropertiesController.cs) · [Criteria](../../src/Core/Contracts/Repositories/Criteria.cs) · [WithCriteria e Build](../../src/Data.Repositories/Repositories/Builders/PropertyQueryBuilder.cs) · [GetAll](../../src/Data.Repositories/Repositories/Queries/PropertyQuery.cs)

## GET por ID

`GetByIdAsync` do controller anuncia 200 e 404 e implementa esses ramos para uma entidade ou um resultado nulo. Embora receba um repositório de dispositivos, não o consulta. O repositório de propriedades abre a conexão e usa `WithId(id)` seguido de `QueryFirstOrDefaultAsync`. O construtor **substitui todo o SELECT** pelo texto `dp.id = @id`. O comando final perde inclusive a tabela e o filtro de dispositivo. Com MySQL acessível, o ID sintaticamente válido encontra um erro SQL antes de se poder distinguir item existente, ausente ou pertencente a outro pai. Um ID não inteiro segue o caminho 400 do MVC.

```http
GET /api/v1/devices/laboratorio/properties/7 HTTP/1.1
Host: casa.example
```

Resposta esperada para o erro de sintaxe, com identificador ilustrativo e corpo textual literal:

```http
HTTP/1.1 500 Internal Server Error
Content-Type: application/json; charset=utf-8
X-Trace-Id: 00000000-0000-4000-8000-000000000007

Erro ao processar a requisição
```

[GetByIdAsync no repositório](../../src/Data.Repositories/Repositories/PropertyRepository.cs) · [WithId](../../src/Data.Repositories/Repositories/Builders/PropertyQueryBuilder.cs) · [Mapeamento de exceções](../../src/Api/Middlewares/ExceptionHandler.cs)

## POST da coleção

1. A action converte o objeto e chama `GetDeviceAsync(device_id)`. Essa leitura anterior à transação carrega o agregado com `LEFT JOIN` de propriedades e settings. Dispositivo ausente resulta em 404 antes da inserção.
2. Em outra conexão, `AddAsync` abre transação e executa `SELECT Id FROM Devices WHERE DeviceId = @device_id`. Resultado inteiro zero, inclusive o padrão na ausência de linha, lança `NotFoundExceptionDomain`. Assim, o desaparecimento do dispositivo entre as duas leituras ainda pode gerar 404, agora pelo middleware.
3. O comando insere `DeviceId, Name, Description, Value` e inclui `SELECT LAST_INSERT_ID() AS NewId`. O repositório usa `ExecuteAsync`, descarta o resultado e faz commit. O ID da entidade continua zero. Não há busca prévia por nome. Uma violação de unicidade 1062 gera 409 antes da confirmação.
4. O controller constrói `CreatedAtAction(nameof(GetAllAsync), new { device_id, id = entity.Id }, property)`. O corpo escolhido para o 201 anunciado é o **objeto recebido**. O nome referencia a listagem, e o MVC remove por padrão o sufixo `Async` do nome de action. Ao formatar o resultado, o MVC não encontra a action chamada `GetAllAsync` e lança `InvalidOperationException`. A resposta é 500, sem endereço do recurso. Essa falha ocorre **depois do commit**. Repetir o POST pode inserir outra linha ou encontrar conflito, conforme as chaves existentes.

```http
POST /api/v1/devices/laboratorio/properties HTTP/1.1
Host: casa.example
Content-Type: application/json
```

```json
{"name":"zona","value":"sul","description":"Ala de instalação"}
```

Resposta após uma inserção confirmada, com identificador ilustrativo:

```http
HTTP/1.1 500 Internal Server Error
Content-Type: application/json; charset=utf-8
X-Trace-Id: 00000000-0000-4000-8000-000000000008

Ocorreu um erro inesperado ao processar a requisição.
```

[AddAsync e CreatedAtAction](../../src/Api/Controllers/PropertiesController.cs) · [Transação de inclusão](../../src/Data.Repositories/Repositories/PropertyRepository.cs) · [GetIdDevice e AddForDevice](../../src/Data.Repositories/Repositories/Queries/PropertyQuery.cs) · [Leitura do pai](../../src/Data.Repositories/Repositories/DeviceRepository.cs)

## PUT da coleção

`CreateOrUpdateAsync` abre uma transação e executa um único `INSERT ... ON DUPLICATE KEY UPDATE`. A subconsulta escalar resolve `Devices.Id` pelo identificador externo. A inserção recebe `DeviceId, Name, Description, Value, UpdatedAt`, com hora do servidor MySQL. Em colisão de chave única ou primária, a cláusula atribui somente `Name, Value, UpdatedAt` à linha atingida. A chave que provoca a colisão é definida no banco. A aplicação não identifica uma linha por ID do corpo e não comprova unicidade de nome por dispositivo. O SQL `UpdateForDevice` definido no projeto não é utilizado por PUT nem PATCH.

A action responde 204 tanto na inserção quanto na atualização após commit. Ela não verifica o pai explicitamente. Se a subconsulta não retornar linha, fornece NULL ao INSERT. O efeito depende de nulabilidade, constraints e modo SQL implantados. Se houver mais de um pai com o mesmo identificador, a subconsulta escalar pode falhar. O 404 anunciado não decorre de um ramo de dispositivo ausente nessa action. Repetir a chamada pode criar várias linhas sem uma chave aplicável, ou voltar a atribuir valor e timestamp em caso de colisão. A resposta não discrimina esses caminhos.

```http
PUT /api/v1/devices/laboratorio/properties HTTP/1.1
Host: casa.example
Content-Type: application/json
```

```json
{"name":"zona","value":"norte","description":"Ala nova"}
```

```http
HTTP/1.1 204 No Content
```

O exemplo pressupõe que o banco aceita a operação. Uma descrição anterior sobrevive à atualização por colisão, mesmo que o corpo traga outra. [CreateOrUpdateAsync](../../src/Data.Repositories/Repositories/PropertyRepository.cs) · [UpSert](../../src/Data.Repositories/Repositories/Queries/PropertyQuery.cs)

## PATCH da coleção

PATCH resolve para a mesma action de PUT, com a mesma subconsulta, transação, atribuições e resposta. Recebe um objeto Property comum. Não há leitura do estado anterior, operação RFC 6902 ou fusão dos membros presentes. Omitir `value` envia NULL ao comando, inclusive no ramo de atualização. Omitir `description` envia NULL na inserção e deixa a descrição anterior intacta no ramo de colisão. A presença do pai, as chaves, o tratamento de duplicidade, a repetição e as falhas obedecem às condições do PUT.

```http
PATCH /api/v1/devices/laboratorio/properties HTTP/1.1
Host: casa.example
Content-Type: application/json
```

```json
{"name":"zona","value":"oeste"}
```

```http
HTTP/1.1 204 No Content
```

O exemplo pressupõe *upsert* aceito. Um array JSON Patch, mesmo sintaticamente válido como JSON, falha no binding do DTO com 400. [Atributos HttpPut e HttpPatch](../../src/Api/Controllers/PropertiesController.cs) · [UpSert](../../src/Data.Repositories/Repositories/Queries/PropertyQuery.cs)

## DELETE por ID

O controller consulta o agregado do dispositivo. Se estiver ausente, devolve 404. Caso exista, cria uma entidade contendo somente o ID da rota. Em uma nova transação, o repositório resolve de novo o ID interno do pai e executa `DELETE FROM DeviceProperties WHERE DeviceId = @deviceId AND Id = @id`. Ignora a contagem de linhas afetadas e responde 204 após commit. Item inexistente, ID pertencente a outro pai e repetição da exclusão têm a mesma resposta enquanto a pré-consulta encontra o dispositivo. Se o pai desaparecer entre as leituras, o repositório pode usar zero no segundo lookup e confirmar uma exclusão de zero linhas. Não há exclusão por nome.

```http
DELETE /api/v1/devices/laboratorio/properties/7 HTTP/1.1
Host: casa.example
```

```http
HTTP/1.1 204 No Content
```

[RemoveAsync no controller](../../src/Api/Controllers/PropertiesController.cs) · [RemoveAsync no repositório](../../src/Data.Repositories/Repositories/PropertyRepository.cs) · [RemoveForDevice](../../src/Data.Repositories/Repositories/Queries/PropertyQuery.cs)

## Propriedades incorporadas ao dispositivo

- **Criação do dispositivo.** Em `POST /api/v1/devices`, o modelo do pai declara `properties` como coleção não anulável. Um array vazio representa ausência de propriedades iniciais. A conversão leva cada DTO para o Core. `AddProperties` compara os IDs com a coleção anterior ao lote, inicialmente vazia. Vários itens novos com ID zero passam juntos, inclusive nomes repetidos. Na mesma transação que cria o pai, o repositório insere uma linha por item com o ID interno recém-gerado, nome e valor. Uma descrição recebida chega à entidade, mas fica fora do INSERT. Uma falha dentro da transação pode desfazer também o cadastro do pai. A resposta de criação referencia o dispositivo, sem devolver as propriedades.
- **Leitura do dispositivo.** `GET /api/v1/devices` e `GET /api/v1/devices/{device_id}` projetam ID, nome, valor e descrição da propriedade por `LEFT JOIN`. O Dapper divide a linha pelos campos `Id`. O repositório agrupa por identificador externo do pai, e `AddProperty` ignora item nulo e repetição do ID já incorporado. Isso evita duplicação causada pelo join com settings, preserva propriedades de IDs diferentes mesmo com nome igual e permite `properties: []`. A saída reconverte cada item para o DTO sem ID. Esses caminhos de leitura independem do construtor SQL defeituoso do GET por ID de propriedade.
- **Alteração e exclusão do pai.** O PATCH do dispositivo pode manipular sua representação em memória. O UPDATE do repositório escreve apenas colunas de `Devices`. O DELETE do pai também só emite DELETE em `Devices`. A constante `DeleteProperties` não é chamada nesse caminho. Uma cascata, uma rejeição por FK ou a sobrevivência de linhas dependem do schema implantado.

[DeviceController](../../src/Api/Controllers/DeviceController.cs) · [Modelo Device](../../src/Api/Models/Device.cs) · [AddProperty e AddProperties](../../src/Core/Entities/Device.cs) · [DeviceRepository](../../src/Data.Repositories/Repositories/DeviceRepository.cs) · [DeviceQuery](../../src/Data.Repositories/Repositories/Queries/DeviceQuery.cs) · [DeviceQueryBuilder](../../src/Data.Repositories/Repositories/Queries/DeviceQueryBuilder.cs)

## Persistência e limites do schema conhecido

O acesso direto segue controller → `IPropertyRepository` → `PropertyRepository` → Dapper → MySqlConnector. Não há serviço intermediário de Properties. A configuração exige a connection string lógica `Devices`. Os repositórios são *scoped* e recebem conexões transientes. Os valores da rota, critérios e campos gravados são parâmetros SQL. Os defeitos de composição da consulta decorrem do texto fixo do construtor. [Contrato](../../src/Core/Contracts/Repositories/IPropertyRepository.cs) · [Registro de DI](../../src/Data.Repositories/DI/MySqlDependencyInjection.cs) · [Bootstrap](../../src/Api/Program.cs)

O script inicial versionado se identifica como **SQL Server**. Ele declara:

| Coluna | Declaração nesse script |
|---|---|
| `Devices.Id` / `Devices.DeviceId` | PK inteira identity / identificador externo `nvarchar(50) NOT NULL`, sem unicidade declarada no script. |
| `DeviceProperties.Id` | PK inteira `IDENTITY(1,1) NOT NULL`. |
| `DeviceProperties.DeviceId` | Inteiro obrigatório, FK para `Devices.Id`, sem cláusula de cascata. |
| `Name` / `Value` | `nvarchar(50) NOT NULL` em ambas. |
| `Description` | `nvarchar(100) NULL`. |

Esse DDL contém um exemplo de propriedade inicial. Ele não declara `UpdatedAt` nem índice único em `(DeviceId, Name)`. Os scripts MySQL versionados não completam o DDL dessa tabela. Consequentemente, comprimentos, defaults, autoatribuição do ID, chaves de colisão, collation, triggers, ações de FK e disponibilidade de `UpdatedAt` no ambiente dependem do catálogo efetivamente implantado. Uma coluna `UpdatedAt` ausente impede o *upsert*. As inserções direta e incorporada, que a omitem, dependem das regras de preenchimento do banco. [DDL e seed](../../database/init.sql) · [Scripts de banco](../../database) · [SQL efetivamente usado](../../src/Data.Repositories/Repositories/Queries/PropertyQuery.cs)

## Concorrência, falhas e acesso

As consultas diretas não abrem transação explícita. Cada escrita direta abre conexão, inicia transação, executa o comando e confirma. O repositório tenta rollback em exceções dentro do bloco transacional, propaga o erro e fecha a conexão no `finally`. A abertura e o início da transação precedem esse bloco. POST e DELETE fazem a primeira leitura do pai fora da transação de escrita. O token HTTP chega aos comandos construídos com `CommandDefinition`. Os lookups escalares de ID dessas duas escritas usam o overload sem token. Não há retry da aplicação, lock explícito por propriedade, comparação de versão, proteção contra repetição ou invalidação de cache após commit. Não se pode deduzir o resultado definitivo de uma escrita apenas da perda de conexão ou da ausência de uma resposta de sucesso. [Repositório](../../src/Data.Repositories/Repositories/PropertyRepository.cs)

A política base de *output cache* tem expiração de dois segundos e alcança os GETs elegíveis mesmo sem atributo no controller. Uma resposta 200 armazenada pode ser servida sem nova consulta, inclusive logo após uma escrita. A política padrão distingue as queries, não armazena 204 ou falhas e normalmente exclui pedidos autenticados ou com header Authorization e respostas com Set-Cookie. O cache em memória é local à instância. O watchdog consulta `SELECT 1` a intervalos de dois minutos e encerra o processo após três falhas consecutivas. Ele não repete a operação HTTP. [Program](../../src/Api/Program.cs) · [DatabaseWatchdogService](../../src/Api/Services/DatabaseWatchdogService.cs)

O middleware geral escreve a mensagem como **texto UTF-8**, embora declare `application/json; charset=utf-8`. Para essas exceções, quando a resposta ainda não começou, acrescenta `X-Trace-Id` com GUID. Isso é diferente de um ProblemDetails produzido pelo MVC. O handler especial de Dashboard não cobre estas rotas.

| Condição que chega ao middleware geral | Resultado |
|---|---|
| `NotFoundExceptionDomain` / `ArgumentDomainException` | 404 / 400 com a mensagem da exceção. A existência desse mapeamento não ativa a autovalidação do DTO. |
| MySQL 1062 | 409, registro com o mesmo valor. No *upsert*, uma colisão resolvida pelo próprio SQL não chega ao middleware. |
| MySQL 1451 / 1452 | 400, mensagem sobre vínculo em uso ou referência inválida. O detalhe depende da mensagem da FK. |
| MySQL 1205, 1213, 1040, 2006, 2013 | 503 por espera de lock, deadlock, excesso ou perda de conexões. Em 1205, 2006 e 2013 solicita parada do host. |
| `TimeoutException` | 503 e solicitação de parada do host. |
| Demais erros MySQL, inclusive sintaxe | 500 com o texto `Erro ao processar a requisição`. |
| Outra exceção, inclusive a falha de geração de URL | 500 com o texto `Ocorreu um erro inesperado ao processar a requisição.` |

O controller não exige credencial, escopo OAuth ou autorização por proprietário. O identificador na rota delimita a operação sobre dados, sem identificar quem a chamou. O bootstrap habilita CORS para qualquer origem, método e header. A configuração Swarm versionada encaminha o prefixo externo `/devices` via Traefik, aplica `global-sec@file` e remove o prefixo antes de encaminhar à API. As regras efetivas desse middleware externo e a exposição direta da instância dependem do ambiente. O repositório registra informações de operações nos logs, e a representação textual de Property inclui nome e valor. [ExceptionHandler](../../src/Api/Middlewares/ExceptionHandler.cs) · [DashboardExceptionHandler](../../src/Api/Middlewares/DashboardExceptionHandler.cs) · [Program](../../src/Api/Program.cs) · [Configuração Swarm](../../docker-compose.swarm.yaml) · [Property.ToString](../../src/Core/Entities/Property.cs)

## Alcance da evidência

O controller, os mapeamentos, o repositório, as consultas e os scripts vinculados sustentam a descrição implementada. Em ensaio local com ASP.NET Core 9.0.9, controller e middleware da revisão e repositórios substituídos, foram observados o 400 para ID não inteiro, array e corpo JSON nulo, o 415 para texto simples, a passagem de objeto vazio e de campos em branco, a omissão da descrição nula na saída e o atendimento de um segundo GET pelo cache. O POST chegou à inclusão substituída antes de falhar com `No route matches the supplied values` e responder 500. Em construção isolada dos comandos reais foram observados o SELECT base, a sequência de filtros sem conjunção e o fragmento isolado por ID. Os exemplos ilustram esses caminhos. Transações, concorrência, constraints, conteúdo de linhas e respostas dependentes de erro MySQL são descritos a partir do código e condicionados ao ambiente. Não houve execução contra banco, dispositivo ou infraestrutura implantada.
