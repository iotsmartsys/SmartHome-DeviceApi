# Capability Types

Um Capability Type é uma entrada no catálogo de tipos de capability. Reúne o nome do tipo, o modo de atuação, o tipo de dado e metadados de valor e apresentação. Uma capability é uma instância associada a um tipo pela coluna `Capabilities.CapabilityTypeId`. Os ícones do catálogo pertencem ao tipo e têm nome e duas cores opcionais. O catálogo mantém esses metadados. A criação ou edição de um tipo não grava valores de capabilities, recalcula estado ou envia comandos a dispositivos. [Entidade do tipo](../../src/Core/Entities/CapabilityType.cs#L5-L40), [queries de tipos](../../src/Data.Repositories/Repositories/Queries/CapabilityTypeQuery.cs), [queries consumidoras](../../src/Data.Repositories/Repositories/Queries/CapabilityQuery.cs#L5-L79).

A chave externa do catálogo é o `id` inteiro do registro. A criação obtém o id do MySQL, a consulta individual usa essa chave, a edição preserva a identidade ao renomear e a exclusão seleciona o mesmo id. O nome continua sendo um filtro da coleção e participa da resolução de tipos em outros fluxos. O DTO oferece uma lista de ícones sem identificador próprio ou status de ciclo de vida. A única exclusão oferecida é física. [Controller](../../src/Api/Controllers/CapabilityTypeController.cs), [modelo HTTP](../../src/Api/Models/CapabilityType.cs), [exclusão SQL](../../src/Data.Repositories/Repositories/Queries/CapabilityTypeQuery.cs#L66-L68).

## Interface HTTP

| Operação | Rota na aplicação | Resultado da action |
|---|---|---|
| Listar tipos | `GET /api/v1/capabilities-types` | `200` com array ou `204` quando vazio |
| Consultar um tipo | `GET /api/v1/capabilities-types/{id}` | `200` com objeto ou `404` quando ausente |
| Criar um tipo | `POST /api/v1/capabilities-types` | `201` com objeto e `Location` após o commit |
| Editar um tipo | `PATCH /api/v1/capabilities-types/{id}` | `204` após o commit ou `404` quando ausente na leitura inicial |
| Excluir um tipo | `DELETE /api/v1/capabilities-types/{id}` | `204` após o commit ou quando ausente na leitura inicial |

O ASP.NET Core registra as actions pelo roteamento de atributos de `MapControllers`. O seletor `{id:int}` aceita um segmento representável como `Int32`, inclusive zero e negativos. Um texto numérico seleciona o id, mesmo que também seja nome de outro tipo. Nomes não inteiros, inteiros fora da faixa e o formato antigo `/id/{id}` não correspondem à rota e recebem `404` do roteador. Um método não anunciado em caminho reconhecido recebe `405` com `Allow`. Isso inclui `HEAD` e `OPTIONS` comum. O preflight CORS válido pode receber `204` antes das actions. [Rotas e seletores](../../src/Api/Controllers/CapabilityTypeController.cs#L6-L74), [pipeline](../../src/Api/Program.cs#L78-L105).

Somente o GET da coleção lê query, por meio de `name`. As outras actions ignoram parâmetros extras. GET e DELETE dispensam corpo e `Content-Type` e não fazem binding do corpo. POST e PATCH exigem corpo. O MVC com Newtonsoft.Json usa `application/json`, `text/json` ou um subtipo `application/*+json`, em UTF-8 ou UTF-16 LE conforme o charset. `application/json-patch+json` tem formatter específico para o PATCH tipado. O formatter geral também consegue ler o array de operações quando o media type é `application/json` ou outro JSON aceito e consegue ler o objeto do POST sob `application/json-patch+json`. O tipo do corpo continua definido pela action. [Bootstrap MVC](../../src/Api/Program.cs#L84-L88), [versões dos pacotes](../../src/Api/Api.csproj#L3-L16), [registro dos formatters](https://github.com/dotnet/aspnetcore/blob/v9.0.1/src/Mvc/Mvc.NewtonsoftJson/src/DependencyInjection/NewtonsoftJsonMvcOptionsSetup.cs#L40-L67), [formatter de entrada](https://github.com/dotnet/aspnetcore/blob/v9.0.1/src/Mvc/Mvc.NewtonsoftJson/src/NewtonsoftJsonInputFormatter.cs#L39-L63).

### Modelos e representação

O POST recebe `Api.Models.CapabilityType`. O PATCH recebe `JsonPatchDocument<CapabilityType>`, que opera sobre uma representação do mesmo modelo. A conversão da API para o Core constrói uma entidade nova, copia os campos de metadados e constrói os ícones. As leituras fazem a conversão inversa. A entidade Core inicializa `Id` em zero, `ComputedValue` em `false` e `Icons` como lista vazia. A conversão de entrada deixa a lista inicial quando `icons` é nulo. [Modelo e conversões](../../src/Api/Models/CapabilityType.cs#L6-L49), [entidades Core](../../src/Core/Entities/CapabilityType.cs#L5-L40), [assinaturas das actions](../../src/Api/Controllers/CapabilityTypeController.cs#L36-L67).

| Campo JSON | Tipo e regra no POST | Origem nas respostas e efeito no PATCH |
|---|---|---|
| `id` | `Int32` opcional, padrão `0`. O binder exige valor conversível e não nulo se informado. A conversão descarta o id do cliente. | GET usa a chave lida e POST usa a chave gerada. O PATCH pode alterar ou testar o campo em memória, mas restaura o id carregado antes de gravar. |
| `name` | String obrigatória e não nula por validação implícita do MVC. | Lido ou gravado em `Name`. Uma string nula ao final do PATCH preserva a coluna via `COALESCE`. String vazia é um valor de atualização. |
| `actuator_mode` | String obrigatória e não nula por validação implícita. | Lido ou gravado em `ActuatorMode`. O PATCH com resultado nulo preserva a coluna. |
| `data_type` | String obrigatória e não nula por validação implícita. | Lido ou gravado em `DataType`. O PATCH com resultado nulo preserva a coluna. |
| `computed_value` | Booleano opcional, `false` se omitido. Um `null` explícito falha na desserialização. | Lido ou gravado em `DynamicComputedValue`. É um indicador armazenado. No PATCH, `remove` o redefine para `false` e `replace` com `null` falha. |
| `value_symbol` | String opcional e anulável, padrão `null`. | Lido ou gravado em `ValueSymbol`. O PATCH envia o valor resultante diretamente ao SQL, inclusive `null`. |
| `active_value` | String opcional e anulável, padrão `null`. A conversão copia para a entidade e o `201` pode ecoar o valor. | O INSERT omite `ActiveValue`. GET lê a coluna do banco. PATCH a grava diretamente, inclusive com `null`. |
| `icons` | Array opcional e anulável. Omissão, `null` e `[]` produzem lista vazia na entidade e no `201`. | GET recompõe o array pelas linhas de ícones e devolve `[]` quando não há linhas. PATCH regrava o conjunto final e `null`, remoção da propriedade ou `[]` esvaziam o conjunto. |

Cada ícone tem `name` (string não nula exigida pelo MVC quando o objeto é recebido no POST), `primary_color` e `secondary_color` (strings opcionais, default `null`). O catálogo não valida paleta, formato das cores, quantidade, nomes repetidos ou identidade própria dos ícones. Um elemento `null` no array passa pela validação de entrada e falha na conversão que tenta acessar `i.name`. O PATCH pode produzir inclusive um objeto de ícone com nome nulo, que chega aos parâmetros do INSERT e fica sujeito ao schema. A API não configura enum, normalização, trim, tamanho ou validação semântica para os textos do catálogo. A validação implícita do POST aceita `""` e espaços como valor de string. O formato canônico usa os nomes em snake_case, com correspondência sem distinção de maiúsculas no binding JSON. Propriedades extras comuns são ignoradas. A conversão de tipos do Newtonsoft.Json pode aceitar entradas como `"true"` para o booleano ou um número para uma string. [DTO de ícone](../../src/Api/Models/CapabilityIcon.cs#L3-L17), [conversão de entrada](../../src/Api/Models/CapabilityType.cs#L28-L46), [SQL](../../src/Data.Repositories/Repositories/Queries/CapabilityTypeQuery.cs#L23-L64), [validação implícita MVC](https://github.com/dotnet/aspnetcore/blob/v9.0.9/src/Mvc/Mvc.DataAnnotations/src/DataAnnotationsMetadataProvider.cs).

O MVC serializa os objetos em JSON com os nomes do modelo. A opção global `NullValueHandling.Ignore` omite propriedades nulas, inclusive cores, símbolo e valor ativo. Zero, `false` e `[]` continuam sendo valores da representação. A resposta usual de objeto ou coleção é `application/json; charset=utf-8` e `Accept` pode selecionar outro formato JSON compatível. O projeto usa as opções padrão do MVC para negociação, com retorno ao formatter disponível se o `Accept` não puder ser atendido. Todos os `204` das actions saem sem corpo. [Configuração JSON](../../src/Api/Program.cs#L84-L88), [conversão de saída](../../src/Api/Models/CapabilityType.cs#L16-L26), [formatter de saída](https://github.com/dotnet/aspnetcore/blob/v9.0.1/src/Mvc/Mvc.NewtonsoftJson/src/NewtonsoftJsonOutputFormatter.cs).

## Operações

Os exemplos usam dados fictícios e omitem headers gerais de transporte. Os exemplos de escrita bem-sucedida pressupõem que o MySQL aceite os comandos e conclua a transação. Os GETs com dados pressupõem o estado persistido indicado e uma leitura que possa vê-lo.

### Listar tipos

`GET /api/v1/capabilities-types?name={texto}` filtra por nome e agrupa os ícones por id do tipo. O binder converte `name` ausente, vazio ou só de espaços em nulo. Preserva os espaços de um valor que também contém outros caracteres. O binder usa o primeiro valor que recebe. Na política de cache padrão aplicada a um GET elegível, o cálculo da chave ordena em ordem ordinal os valores repetidos antes do binding. Assim, `name=zeta&name=alpha` pode chegar ao binder com `alpha` primeiro. Sem esse processamento, vale a ordem recebida. Outras chaves de query não filtram o SQL. [Action de listagem](../../src/Api/Controllers/CapabilityTypeController.cs#L10-L21), [chave de cache ASP.NET Core](https://github.com/dotnet/aspnetcore/blob/v9.0.9/src/Middleware/OutputCaching/src/OutputCacheKeyProvider.cs#L173-L213), [`StringValues.ToArray`](https://github.com/dotnet/runtime/blob/v9.0.9/src/libraries/Microsoft.Extensions.Primitives/src/StringValues.cs#L247-L269).

Em uma leitura ao banco, o repositório envia `%{name}%` como parâmetro de `Name LIKE @name`. Ausência produz `%%`, inclusive com a semântica SQL de excluir `Name` nulo do predicado. `%` e `_` recebidos conservam função de curingas conforme o MySQL. A aplicação não os escapa nem altera maiúsculas ou acentos. Correspondência e ordenação `ORDER BY Name` dependem da collation. Não há paginação, limite explícito, critério de desempate por id ou deduplicação por nome. Se houver tipos, o repositório faz uma segunda consulta em lote por `CapabilityTypeId IN @ids`, junta o resultado em memória e devolve `200`. Sem tipos, pula essa consulta e devolve `204` sem array. Falha em qualquer consulta segue o tratamento comum. [`GetAllAsync`](../../src/Data.Repositories/Repositories/CapabilityTypeRepository.cs#L56-L77), [`SelectBase`, `GetAll` e `SelectIconsByTypeIds`](../../src/Data.Repositories/Repositories/Queries/CapabilityTypeQuery.cs#L5-L42).

Exemplo de filtro com uma correspondência e sem ícones cadastrados.

```http
GET /api/v1/capabilities-types?name=Nivel HTTP/1.1
Host: api.example.test
Accept: application/json
```

```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```

```json
[
  {
    "name": "Nivel demo",
    "actuator_mode": "Read",
    "data_type": "float",
    "computed_value": false,
    "value_symbol": "%",
    "icons": [],
    "id": 72
  }
]
```

### Consultar um tipo

`GET /api/v1/capabilities-types/{id}` executa `GetByIdAsync` em caso de leitura ao banco. A primeira consulta seleciona pela chave e materializa a primeira linha ou ausência. Se há linha, outra consulta busca os ícones do mesmo id. O resultado é `200` com objeto completo, inclusive id e array de ícones. Sem linha, o repositório pula a leitura de ícones e a action retorna `404` com Problem Details. Falha em qualquer consulta segue o tratamento comum. Uma entrada ainda válida em cache pode atender o GET sem consultar essas tabelas. [`GetCapabilityTypeById`](../../src/Api/Controllers/CapabilityTypeController.cs#L23-L34), [`GetByIdAsync`](../../src/Data.Repositories/Repositories/CapabilityTypeRepository.cs#L89-L97), [projeção e filtro](../../src/Data.Repositories/Repositories/Queries/CapabilityTypeQuery.cs#L5-L21), [consulta de ícones](../../src/Data.Repositories/Repositories/Queries/CapabilityTypeQuery.cs#L44-L52).

Exemplo de um tipo existente cujo `ActiveValue` já foi gravado com `ON`.

```http
GET /api/v1/capabilities-types/42 HTTP/1.1
Host: api.example.test
Accept: application/json
```

```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```

```json
{
  "name": "Indicador demo",
  "actuator_mode": "Read",
  "data_type": "text",
  "computed_value": false,
  "value_symbol": "u",
  "active_value": "ON",
  "icons": [{"name": "circle", "primary_color": "#88AACC"}],
  "id": 42
}
```

### Criar um tipo

`POST /api/v1/capabilities-types` recebe um objeto conforme as regras do modelo. Após o binding e a validação, a action converte o objeto e chama `CreateAsync`. O repositório abre uma transação, insere a linha de tipo, lê `LAST_INSERT_ID()` como `Int32` na mesma conexão e atribui o resultado à entidade. Insere cada ícone usando esse id e faz commit. A aplicação sempre tenta inserir, sem busca prévia por nome, upsert, deduplicação ou chave de idempotência. Repetir o POST pode criar outra linha com novo id se o schema permitir ou resultar em conflito quando houver violação de unicidade. Falha nos INSERTs ou no commit propaga ao middleware. [`CreateCapabilityType`](../../src/Api/Controllers/CapabilityTypeController.cs#L36-L48), [`CreateAsync`](../../src/Data.Repositories/Repositories/CapabilityTypeRepository.cs#L21-L54), [INSERTs](../../src/Data.Repositories/Repositories/Queries/CapabilityTypeQuery.cs#L23-L32).

O `201` usa a entidade de entrada com o id gerado. A action não relê valores de defaults, triggers ou outras alterações do banco. O caso de `active_value` é particularmente visível. O valor recebido aparece no `201` se não for nulo, embora o INSERT não envie `ActiveValue`. Seu valor lido em um GET posterior depende do banco ou de uma atualização posterior. Até o sucesso da criação depende de o banco aceitar a omissão da coluna. O `Location` é gerado por `CreatedAtRoute` com o id atribuído, o esquema, o host e o `PathBase` percebidos pela aplicação. Um prefixo externo depende de como o proxy encaminha a requisição ou reescreve a resposta. [Action e rota nomeada](../../src/Api/Controllers/CapabilityTypeController.cs#L23-L47), [INSERT](../../src/Data.Repositories/Repositories/Queries/CapabilityTypeQuery.cs#L23-L27), [execução de `CreatedAtRoute`](https://github.com/dotnet/aspnetcore/blob/v9.0.9/src/Mvc/Mvc.Core/src/CreatedAtRouteResult.cs).

```http
POST /api/v1/capabilities-types HTTP/1.1
Host: api.example.test
Content-Type: application/json
Accept: application/json
Content-Length: 169

{"id":999,"name":"Indicador demo","actuator_mode":"Read","data_type":"text","value_symbol":"u","active_value":"ON","icons":[{"name":"circle","primary_color":"#88AACC"}]}
```

```http
HTTP/1.1 201 Created
Content-Type: application/json; charset=utf-8
Location: http://api.example.test/api/v1/capabilities-types/42
```

```json
{
  "name": "Indicador demo",
  "actuator_mode": "Read",
  "data_type": "text",
  "computed_value": false,
  "value_symbol": "u",
  "active_value": "ON",
  "icons": [{"name": "circle", "primary_color": "#88AACC"}],
  "id": 42
}
```

### Editar um tipo

`PATCH /api/v1/capabilities-types/{id}` recebe um array JSON Patch. O MVC primeiro desserializa o documento. Um objeto no lugar do array ou JSON malformado resulta em `400` antes da consulta. Com um documento desserializado, a action lê o tipo e seus ícones pelo id. Se o tipo estiver ausente, retorna `404` antes de aplicar operações, mesmo se um `path` desserializado seria inválido. Para um alvo existente, a action projeta o tipo para o DTO, aplica as operações em ordem, converte o resultado para uma entidade nova e repõe o id lido. Não há nova validação MVC do objeto modificado. [`UpdateCapabilityType`](../../src/Api/Controllers/CapabilityTypeController.cs#L50-L67), [`GetByIdAsync`](../../src/Data.Repositories/Repositories/CapabilityTypeRepository.cs#L89-L97), [validação do documento no MVC](https://github.com/dotnet/aspnetcore/blob/v9.0.1/src/Mvc/Mvc.NewtonsoftJson/src/DependencyInjection/NewtonsoftJsonMvcOptionsSetup.cs#L40-L67).

- O adaptador oferece `add`, `remove`, `replace`, `move`, `copy` e `test` para caminhos que consegue resolver. A action não impõe uma lista própria de operações ou propriedades. Os caminhos são os nomes JSON, como `/data_type`, `/icons/0/primary_color` e `/icons/-` ao anexar à lista. `copy` e `move` também usam `from`. O adaptador aceita variação de maiúsculas em propriedades do DTO.
- O documento permite testar e manipular `/id` em memória, inclusive usá-lo como origem de outra operação. O destino no banco continua sendo o id da rota carregado pelo GET. Um valor de id incompatível ainda pode falhar durante a aplicação.
- Em propriedade de objeto, `remove` redefine a referência para `null` e redefine tipos não anuláveis para seu default, como `false` para `computed_value`. Em array, remove o elemento. Para strings, `replace` com `null` pode ter o mesmo efeito de descarte no modelo, com o efeito de persistência da tabela de campos. Uma lista removida fica nula durante o patch, portanto um próximo passo não consegue percorrê-la até que seja recriada.
- Campos de operação não recebem validação estrita de presença. Na biblioteca versionada, um `op` omitido deixa o enum no default `add`, efeito também observado com `op: null` ignorado pelo formatter. Um `value` omitido pode chegar como `null`. Uma operação desconhecida, um caminho inexistente, índice inválido, `test` que falha ou valor sem conversão válida pode lançar na aplicação. Nesse caso a action não chama a escrita e o middleware emite `500`, sem resposta específica de erro de patch.

[Aplicação e erros do JSON Patch](https://github.com/dotnet/aspnetcore/blob/v9.0.1/src/Features/JsonPatch/src/JsonPatchDocumentOfT.cs#L517-L574), [modelo de operação e enum](https://github.com/dotnet/aspnetcore/tree/v9.0.1/src/Features/JsonPatch/src/Operations), [adaptador de propriedades](https://github.com/dotnet/aspnetcore/blob/v9.0.1/src/Features/JsonPatch/src/Internal/PocoAdapter.cs), [middleware](../../src/Api/Middlewares/ExceptionHandler.cs#L21-L38).

A escrita abre uma transação após a leitura e a aplicação do patch. Executa um `UPDATE` por id com os campos resultantes, apaga todas as linhas de ícones desse id e insere os ícones finais. Isso ocorre também para `[]` ou um documento apenas com `test` bem-sucedido. Os campos não tocados normalmente voltam com os valores lidos, inclusive ícones, mas seus registros são apagados e recriados. Um conflito de restrição em qualquer comando pode impedir o `204`. A resposta de sucesso sai sem representação e sem `Location`. O `test` examina a cópia carregada antes da transação de escrita e não protege contra alteração concorrente. [`UpdateAsync(CapabilityType)`](../../src/Data.Repositories/Repositories/CapabilityTypeRepository.cs#L175-L218), [UPDATE e exclusão de ícones](../../src/Data.Repositories/Repositories/Queries/CapabilityTypeQuery.cs#L54-L66).

Exemplo que renomeia o tipo e grava o valor ativo, mantendo o id `42` e os campos restantes na ausência de alterações concorrentes.

```http
PATCH /api/v1/capabilities-types/42 HTTP/1.1
Host: api.example.test
Content-Type: application/json-patch+json
Content-Length: 126

[
  {"op":"replace","path":"/name","value":"Indicador de sala demo"},
  {"op":"replace","path":"/active_value","value":"ON"}
]
```

```http
HTTP/1.1 204 No Content
```

### Excluir um tipo

`DELETE /api/v1/capabilities-types/{id}` recebe apenas a chave. O repositório começa com `GetByIdAsync`, inclusive leitura dos ícones se o tipo existir. Uma falha nessa leitura pode impedir a exclusão. Quando o tipo está ausente, o repositório retorna sem iniciar transação. Se há tipo, abre uma transação e executa somente `DELETE FROM CapabilityTypes WHERE Id = @id` com commit. Filhos, referências de capabilities e mapeamentos de smart home ficam sujeitos às regras de FK, cascata ou triggers do banco. A aplicação não percorre ou apaga essas linhas diretamente. Se o MySQL rejeitar a exclusão por FK, o erro `1451` chega ao middleware e vira `400`. [`DeleteCapabilityType`](../../src/Api/Controllers/CapabilityTypeController.cs#L69-L76), [`DeleteAsync`](../../src/Data.Repositories/Repositories/CapabilityTypeRepository.cs#L151-L173), [DELETE](../../src/Data.Repositories/Repositories/Queries/CapabilityTypeQuery.cs#L68), [tratamento de FK](../../src/Api/Middlewares/ExceptionHandler.cs#L58-L65).

Ausência inicial e exclusão confirmada dão o mesmo `204` sem corpo. Uma repetição após a exclusão também retorna `204` enquanto o id estiver ausente e a consulta puder ser executada. Como o resultado em linhas afetadas é descartado, remoção concorrente da linha após a leitura inicial também pode terminar com `204` sem que a própria requisição tenha excluído uma linha. Um GET subsequente ainda pode receber uma representação anterior em cache. [Fluxo e transação do DELETE](../../src/Data.Repositories/Repositories/CapabilityTypeRepository.cs#L151-L173), [política de cache](../../src/Api/Program.cs#L78-L102).

```http
DELETE /api/v1/capabilities-types/42 HTTP/1.1
Host: api.example.test
```

```http
HTTP/1.1 204 No Content
```

## Persistência e relacionamentos

| Estrutura usada no SQL | Colunas e papel |
|---|---|
| `CapabilityTypes` | `Id` identifica o registro e é gerado pelo banco para a criação. `Name` serve a filtros, ordenação e resolução de tipo por nome em outros fluxos. `ActuatorMode`, `DataType`, `DynamicComputedValue`, `ValueSymbol` e `ActiveValue` são as demais colunas da projeção. O SELECT usa `DynamicComputedValue AS ComputedValue`. |
| `CapabilityTypeIcons` | `CapabilityTypeId` associa zero ou mais linhas ao tipo. A leitura Dapper materializa `CapabilityTypeIconRow(int CapabilityTypeId, string Name, string? PrimaryColor, string? SecondaryColor)` e projeta só o nome e as duas cores. Os tipos da leitura precisam ser compatíveis com o mapeamento. As consultas não selecionam id de ícone nem usam `ORDER BY` para ícones. |
| `Capabilities` e `CapabilityTypesSmartHome` | Referências por `CapabilityTypeId` aparecem em consultas consumidoras. A associação é demonstrada pelo SQL. A definição e as ações de FK de cada relacionamento dependem do schema implantado. |

[Queries de Capability Type](../../src/Data.Repositories/Repositories/Queries/CapabilityTypeQuery.cs), [materialização e associação de ícones](../../src/Data.Repositories/Repositories/CapabilityTypeRepository.cs#L9-L13), [joins consumidores](../../src/Data.Repositories/Repositories/Queries/CapabilityQuery.cs#L5-L42).

`database/mysql-indexes.sql` prevê índices simples para `CapabilityTypes(Name)` e `Capabilities(CapabilityTypeId)` se forem aplicados. O primeiro é criado por `CREATE INDEX` sem `UNIQUE`. Isso não resolve a identidade física, a unicidade do nome ou a existência de um índice equivalente no ambiente. `database/init.sql` contém estruturas legadas com sintaxe de SQL Server e um trecho MySQL de operadores. Nenhum dos scripts versionados define o DDL de `CapabilityTypes` ou `CapabilityTypeIcons`. O catálogo usa `Id` como chave de aplicação e espera geração compatível com `LAST_INSERT_ID()`. Tipo exato, `AUTO_INCREMENT`, `PRIMARY KEY`, limites, nulabilidade, defaults, collation, engine, constraints e cascatas efetivas exigem o DDL aplicado. A nota geral do README menciona cascade nativo, sem especificar as FKs dessas tabelas. A ausência do schema autoritativo está registrada em `EKM-GAP-0002`. [Índices MySQL](../../database/mysql-indexes.sql#L1-L30), [script legado](../../database/init.sql), [README](../../Readme.md#L1-L6), [mapa de conhecimento](../rfc/KNOWLEDGE-MAP.md#L101-L107).

### Efeitos em consumidores do catálogo

- Capabilities já associadas retêm o mesmo `CapabilityTypeId` quando um tipo é renomeado. Consultas que juntam por id passam a projetar o novo nome, modo ou tipo de dado conforme os campos que leem. A escrita do catálogo não atualiza as linhas de capability. As projeções de capability leem os ícones próprios da instância nas colunas `IconName`, `IconActiveColor` e `IconInactiveColor`, sem consultar `CapabilityTypeIcons`. Resolução por nome em inclusão e atualização de capabilities usa `LIMIT 1` sem ordenação. Se o schema permitir nomes duplicados, a escolha por nome nesses caminhos não é determinística por contrato de aplicação. [Consultas de capabilities](../../src/Data.Repositories/Repositories/Queries/CapabilityQuery.cs#L5-L98), [projeção de devices](../../src/Data.Repositories/Repositories/Queries/DeviceQuery.cs#L60-L105), [projeção de groups](../../src/Data.Repositories/Repositories/Queries/GroupQuery.cs#L7-L36).
- A mudança de `DataType` pode alterar como operações futuras interpretam valores nas capabilities associadas, inclusive os fluxos que carregam o tipo para o tratamento de estado do ar-condicionado. A criação de device com capabilities embutidas também consulta o tipo pelo nome. Seu INSERT usa `CapabilityTypeId` no caminho de ar-condicionado e `CapabilityId` no caminho legado, embora as consultas de device juntem pelo primeiro. O efeito sobre a associação do caminho legado depende do schema. Os tipos reconhecidos pelos consumidores não constituem validação no catálogo. [Seleção de tipo para escrita](../../src/Data.Repositories/Repositories/Queries/CapabilityQuery.cs#L53-L79), [repositório consumidor](../../src/Data.Repositories/Repositories/CapabilityRepository.cs), [criação de device](../../src/Data.Repositories/Repositories/DeviceRepository.cs#L61-L95), [SQL de device](../../src/Data.Repositories/Repositories/Queries/DeviceQuery.cs#L88-L106), [conversor Core](../../src/Core/Entities/DataTypes/CapabilityDataType.cs).
- O Dashboard lê `Name`, `DataType` e `ValueSymbol` para código, classificação de visualização e unidade de origem. A unidade de origem vazia ou nula resulta em nulo e a configuração do widget pode substituí-la. [Projeção do Dashboard](../../src/Data.Repositories/Repositories/Queries/DashboardCapabilityQuery.cs#L5-L14), [classificação do Dashboard](../../src/Core/Entities/DashboardCapability.cs#L8-L30), [resolução de unidade](../../src/Core/Services/DashboardDataResolver.cs#L14-L24).
- Se o schema permitir remover o pai e conservar uma referência, a capability deixa de corresponder ao `INNER JOIN` de tipos nas consultas comuns. As projeções que usam `LEFT JOIN` podem receber campos de tipo nulos onde a linha principal permanece. Mesmo quando uma FK real aciona cascade, a action de exclusão não especifica seu alcance. [Consulta comum](../../src/Data.Repositories/Repositories/Queries/CapabilityQuery.cs#L35-L43), [projeção do Dashboard](../../src/Data.Repositories/Repositories/Queries/DashboardCapabilityQuery.cs#L5-L14), [DELETE executado](../../src/Data.Repositories/Repositories/Queries/CapabilityTypeQuery.cs#L68).

### Consulta e atualização internas por nome

O repositório conserva `GetByNameAsync(string)`, que usa igualdade `Name = @name` e lê os ícones da primeira linha, sem ordenação entre nomes equivalentes. A sobrecarga `UpdateAsync(currentName, ...)` usa essa leitura e lança `KeyNotFoundException` se ausente. Ela compõe os parâmetros para o mesmo UPDATE por id, mas omite `@ActiveValue`. Com o comportamento padrão do MySqlConnector, um parâmetro ausente falha. Se a conexão habilitar `Allow User Variables`, ele pode seguir como variável de sessão, sem um valor definido pela operação. No fluxo dessa sobrecarga, `icons == null` evita substituir ícones, ao passo que coleção vazia limpa. Essas rotinas não têm chamadores no fluxo HTTP do catálogo. `CapabilityTypePatch` também permanece declarado, sem uso no binding de qualquer das cinco actions. [Contrato interno](../../src/Core/Contracts/Repositories/ICapabilityTypeRepository.cs), [sobrecarga legada](../../src/Data.Repositories/Repositories/CapabilityTypeRepository.cs#L79-L149), [queries](../../src/Data.Repositories/Repositories/Queries/CapabilityTypeQuery.cs#L19-L21), [UPDATE](../../src/Data.Repositories/Repositories/Queries/CapabilityTypeQuery.cs#L54-L64), [DTO sem action](../../src/Api/Models/CapabilityTypePatch.cs), [binding de parâmetros MySqlConnector](https://github.com/mysql-net/MySqlConnector/blob/2.4.0/src/MySqlConnector/Core/StatementPreparer.cs#L34-L60).

## Consistência, cache e falhas

**Conexões e transações.** O bootstrap seleciona MySQL via `ConnectionStrings:Devices`, exige configuração não vazia e registra `ICapabilityTypeRepository` com escopo da requisição e `IDbConnection` transiente. As leituras são consultas Dapper sem transação explícita. Dapper abre e fecha a conexão por comando quando a recebe fechada. O SELECT do tipo e o SELECT dos ícones podem ver estados diferentes durante uma alteração concorrente. Cada escrita tem sua própria transação. No POST e no PATCH, os comandos do tipo e dos ícones participam da mesma transação e o efeito atômico depende das tabelas e do engine respeitarem a transação. O DELETE participa de uma transação depois da consulta. Existe uma extensão de DI para SQL Server no código, mas o bootstrap não a chama. [Bootstrap](../../src/Api/Program.cs#L11-L16), [registro MySQL](../../src/Data.Repositories/DI/MySqlDependencyInjection.cs#L9-L26), [composição da API](../../src/Api/Program.cs#L71-L82), [DI SQL Server](../../src/Data.Repositories/DI/SqlServerDependencyInjection.cs#L9-L20), [repositório](../../src/Data.Repositories/Repositories/CapabilityTypeRepository.cs), [execução assíncrona Dapper](https://github.com/DapperLib/Dapper/blob/2.1.35/Dapper/SqlMapper.Async.cs#L422-L510).

**Concorrência e repetição.** O PATCH escreve a partir de uma leitura anterior à transação, sem lock de leitura, versão, `If-Match` ou verificação de linhas afetadas. Alterações concorrentes, mesmo em campos diferentes, podem sobrescrever valores e o conjunto de ícones. Se o tipo desaparecer nesse intervalo, o UPDATE que afetou zero linhas não é detectado. Ícones ainda podem provocar falha de integridade durante o INSERT ou a operação pode terminar com `204`, conforme os comandos e restrições envolvidos. Operações de array como `add` podem produzir estado diferente quando repetidas. O controller não implementa precondições, idempotência, retry ou encaminhamento do cancelamento HTTP ao repositório do catálogo. [Controller](../../src/Api/Controllers/CapabilityTypeController.cs#L50-L76), [leituras](../../src/Data.Repositories/Repositories/CapabilityTypeRepository.cs#L56-L97), [escritas](../../src/Data.Repositories/Repositories/CapabilityTypeRepository.cs#L151-L218).

**Cache de respostas.** A política base do Output Cache aplica expiração de dois segundos aos GETs elegíveis, mesmo sem atributo no controller. O armazenamento é por instância e considera a URL e todos os parâmetros de query, inclusive os ignorados pela action. GETs concorrentes para a mesma chave podem compartilhar uma resposta. A política padrão guarda `200` sem `Set-Cookie`. Requisição com `Authorization` não vazio ou identidade autenticada pula consulta e armazenamento do cache. `204`, `404` e falhas não são guardados. POST, PATCH e DELETE não invalidam entradas. É possível ler uma lista anterior após criar ou continuar recebendo `200` com dados anteriores após editar ou excluir até a entrada expirar. Um hit normal pode incluir `Age`. O middleware pode devolver `304` sem corpo para uma entrada cacheada válida com `If-None-Match: *`, ou com `If-Modified-Since` satisfeito pela data cacheada, na ausência de `If-None-Match`. Um GET elegível com `Cache-Control: only-if-cached` sem resultado em cache pode receber `504` antes da action. O catálogo não gera ETag de versão. [Configuração e middleware da API](../../src/Api/Program.cs#L78-L104), [política padrão](https://github.com/dotnet/aspnetcore/blob/v9.0.9/src/Middleware/OutputCaching/src/Policies/DefaultPolicy.cs), [middleware de cache](https://github.com/dotnet/aspnetcore/blob/v9.0.9/src/Middleware/OutputCaching/src/OutputCacheMiddleware.cs), [chave de cache](https://github.com/dotnet/aspnetcore/blob/v9.0.9/src/Middleware/OutputCaching/src/OutputCacheKeyProvider.cs).

**Falhas de escrita e resposta.** O repositório abre a conexão e inicia a transação antes de entrar no `try`. Uma falha durante essa abertura/início não passa pelo rollback e fechamento explícitos daquele bloco. Dentro dele, tenta rollback e propaga, com fechamento no `finally` e descarte da transação. Se rollback ou fechamento lançar, a exceção resultante pode substituir a falha original. A aplicação não reconcilia o resultado de um commit que falhou na comunicação. A resposta é formada após o retorno do repositório. Uma falha após o commit, como na geração de `Location`, serialização ou envio, pode deixar a escrita aplicada sem sucesso observável pelo cliente. [Criação](../../src/Data.Repositories/Repositories/CapabilityTypeRepository.cs#L21-L54), [exclusão e atualização](../../src/Data.Repositories/Repositories/CapabilityTypeRepository.cs#L151-L218), [respostas do controller](../../src/Api/Controllers/CapabilityTypeController.cs#L40-L75).

### Respostas de erro

- **Binding e roteamento.** Com corpo requerido e formatter compatível, corpo vazio, JSON `null`, desserialização inválida ou campo exigido ausente devolvem `400` automático com `ValidationProblemDetails`, normalmente `application/problem+json; charset=utf-8`. Inclui `type`, `title`, `status`, `traceId` e `errors` por campo ou corpo. Content-Type ausente ou incompatível e charset não suportado podem retornar `415` em Problem Details. O `404` retornado pela action tem Problem Details com `type`, `title`, `status` e `traceId`. Um `404` do roteador por falta de rota ou `405` de método incompatível normalmente sai sem corpo. [`ApiController` e assinaturas](../../src/Api/Controllers/CapabilityTypeController.cs#L6-L72), [configuração MVC](../../src/Api/Program.cs#L84-L105), [fábrica de Problem Details do MVC](https://github.com/dotnet/aspnetcore/blob/v9.0.9/src/Mvc/Mvc.Core/src/Infrastructure/DefaultProblemDetailsFactory.cs).
- **Exceções.** `ExceptionHandler` trata as exceções que escapam das actions. Se a resposta ainda não começou, define o status, `X-Trace-Id` (GUID) e `Content-Type: application/json; charset=utf-8`, e escreve uma mensagem em texto puro sem aspas ou envelope. Depois do início da resposta, o middleware registra a falha e não reescreve status ou corpo. O middleware especializado em Dashboard deixa essas rotas no fluxo geral. [`ExceptionHandler`](../../src/Api/Middlewares/ExceptionHandler.cs#L10-L118), [filtro de rotas do Dashboard](../../src/Api/Middlewares/DashboardExceptionHandler.cs#L6-L17).

| Exceção que chega ao middleware | Status | Corpo e efeito operacional |
|---|---|---|
| MySQL `1062` | `409` | `Já existe um registro com o mesmo valor`. O conflito depende de restrição no banco. |
| MySQL `1451` ou `1452` | `400` | Mensagem de vínculo, que pode nomear tabelas se a mensagem do MySQL corresponder ao padrão extraído. |
| MySQL `1205` | `503` | `Timeout ao aguardar lock de banco de dados`. Solicita parada graciosa da aplicação. |
| MySQL `1213` | `503` | `Deadlock detectado. Tente novamente`. A aplicação não reexecuta a operação. |
| MySQL `1040` | `503` | `Banco indisponível no momento (muitas conexões)`. |
| MySQL `2006` ou `2013` | `503` | `Conexão com o banco foi perdida`. Solicita parada graciosa da aplicação. |
| `TimeoutException` | `503` | `Timeout de operação com o banco de dados`. Solicita parada graciosa da aplicação. |
| Outros erros MySQL | `500` | `Erro ao processar a requisição`. |
| Exceção genérica, inclusive erro ao aplicar o patch ou converter um elemento nulo de ícones | `500` | `Ocorreu um erro inesperado ao processar a requisição.` |

O middleware também conhece `ArgumentDomainException` (400) e `NotFoundExceptionDomain` (404). As cinco actions do catálogo não lançam essas exceções como regra própria. Exceções são registradas em logs, inclusive detalhes, stack trace e mensagens MySQL. Além do middleware, o watchdog consulta `SELECT 1` com intervalo de dois minutos e encerra o processo com código 1 após três falhas consecutivas. A recuperação por reinício depende do ambiente e o manifesto Swarm prevê política de reinício. [Mapeamento e parada graciosa](../../src/Api/Middlewares/ExceptionHandler.cs#L21-L135), [mensagens de FK](../../src/Api/Middlewares/ExceptionHandler.cs#L137-L179), [watchdog](../../src/Api/Services/DatabaseWatchdogService.cs), [política Swarm](../../docker-compose.swarm.yaml#L11-L19).

Exemplo de resposta de falha de aplicação de um patch contra um id existente, como `replace` com caminho inexistente. O corpo abaixo é texto literal.

```http
HTTP/1.1 500 Internal Server Error
Content-Type: application/json; charset=utf-8
X-Trace-Id: 10000000-0000-4000-8000-000000000001

Ocorreu um erro inesperado ao processar a requisição.
```

Um POST com corpo `{}` usa a resposta de validação do MVC.

```http
HTTP/1.1 400 Bad Request
Content-Type: application/problem+json; charset=utf-8
```

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "traceId": "0H00000000000:00000001",
  "errors": {
    "name": ["The name field is required."],
    "actuator_mode": ["The actuator_mode field is required."],
    "data_type": ["The data_type field is required."]
  }
}
```

### Segurança e encaminhamento

O controller não declara autenticação ou autorização, escopos, credenciais obrigatórias ou verificação de propriedade. O bootstrap não registra nem aplica uma política global de autenticação/autorização a essas actions. Os comandos Dapper usam parâmetros. A comparação `LIKE` conserva os curingas como parte do filtro. A política CORS `AllowAll` permite qualquer origem, header de requisição e método no preflight, sem habilitar credenciais nem expor headers de resposta adicionais ao JavaScript cross-origin. Em requisição cross-origin, o servidor pode devolver `Access-Control-Allow-Origin: *`. [Controller](../../src/Api/Controllers/CapabilityTypeController.cs), [CORS e pipeline](../../src/Api/Program.cs#L84-L105), [queries](../../src/Data.Repositories/Repositories/Queries/CapabilityTypeQuery.cs).

O manifesto Swarm referencia o Traefik, a remoção do prefixo externo `/devices` e o middleware externo `global-sec@file`. Seu conteúdo, a exposição efetiva, a aplicação de autenticação externa, as políticas TLS e a reescrita efetiva de headers dependem da configuração implantada. Os caminhos desta página são os da aplicação após eventual encaminhamento. [Labels Traefik](../../docker-compose.swarm.yaml#L27-L38), [integrações no dossiê](../specs/SYSTEM-DOSSIER.md#L118-L147).

## Contrato, evidência e condições externas

A especificação [SHD-CAPABILITY-TYPE-ID-001@0.2](../specs/CAPABILITY-TYPE-ID.md) governa a identificação por id e a preservação do filtro `name` e dos resultados previstos para as cinco operações. O controller e o SQL implementam a chave inteira nas ações individuais, a chave gerada no POST e a preservação do id no PATCH. As rotas individuais por nome ou sob `/id/{id}` foram removidas. A expectativa de consultar o novo nome após renomear (CT-AC-004) depende, pela implementação, de um GET que leia o estado atual. Um hit de cache pode devolver o nome anterior durante o TTL. O contrato de id não determina defaults do banco, regras de cascade ou uso de `active_value` no INSERT. [Requisitos e critérios](../specs/CAPABILITY-TYPE-ID.md#L47-L115), [política de cache do bootstrap](../../src/Api/Program.cs#L78-L102).

Swagger e OpenAPI são expostos no ambiente de desenvolvimento. No Swagger gerado com a configuração versionada, `id` de rota é `integer/int32`, o modelo inclui `id` e o PATCH é descrito como array de operações. As anotações de `ProducesResponseType` e os schemas gerados têm cobertura parcial. Por exemplo, a action PATCH declara `204` e `404`, enquanto binding, aplicação do patch e banco produzem outras respostas. O schema Swagger padrão também admite nulos nas strings de entrada que o MVC exige no POST. O schema genérico de operação inclui `operationType`, uma propriedade interna ignorada pelo JSON Patch. A customização de schema no bootstrap se refere ao DTO de Groups. [Metadata do controller](../../src/Api/Controllers/CapabilityTypeController.cs#L10-L72), [configuração Swagger e exposição em desenvolvimento](../../src/Api/Program.cs#L13-L66), [rotas de desenvolvimento](../../src/Api/Program.cs#L107-L113), [modelo de operação do JSON Patch](https://github.com/dotnet/aspnetcore/blob/v9.0.1/src/Features/JsonPatch/src/Operations/OperationBase.cs#L9-L44).

O comportamento de persistência acima decorre da inspeção de código, queries e scripts. Em execução isolada com ASP.NET Core 9, os controllers e a configuração de transporte usaram um repositório em memória. Foram observados roteamento, binding, validação, conversões HTTP/Core, formas de resposta, comportamento de JSON Patch, metadados do Swagger e cache. Nesse recorte, os cinco exemplos de operação corresponderam ao status, ao corpo e ao `Location` indicado quando aplicável. Entre os casos observados estão a mudança da ordem de parâmetros repetidos durante o cálculo da chave e o `200` antigo após edição ou exclusão seguida de GET. Confirmação de defaults, restrições, cascatas, rollback real, concorrência no banco ou proteção de ingresso requer evidência do MySQL e do ambiente efetivamente implantados. A especificação registra validação histórica aceita pelo Arquiteto, sem comandos, cenários ou logs desses testes. [Registro histórico de validação](../specs/CAPABILITY-TYPE-ID.md#L138-L152), [lacuna de schema](../rfc/KNOWLEDGE-MAP.md#L104-L107).
