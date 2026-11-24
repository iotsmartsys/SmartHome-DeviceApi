# Controle de acesso proprietário legado — Deprecated

**Contrato HTTP complementar:** [OpenAPI derivado](../contracts/openapi.json) e [reconciliação integral](../reports/SHD-KNOWLEDGE-MAP/2026-11-23T211211Z-c31c6cb4-2d80-42cd-9fbf-9dd40921395c-openapi-reconciliation.md). O artefato é informativo, não normativo; o relatório contém o inventário integral de operações, parâmetros, schemas, respostas e divergências deste domínio.

**Classificação aprovada:** controle de acesso proprietário legado — **Deprecated**.
**Natureza:** referência interna informativa, não contrato de OAuth 2.0.

## Responsabilidade e estado

A Devices API contém controller e persistência identificados tecnicamente como `OAuth`. Segundo o contexto operacional informado pelo responsável, o mecanismo permanece operacional para compatibilidade com consumidores e dispositivos atuais e deverá ser substituído no futuro. A substituição não integra este documento. Esta classificação não declara conformidade oficial com OAuth 2.0.

## Rotas e fluxo interno

| Rota | Comportamento encontrado no código local |
|---|---|
| `GET /oauth/authorize` | Lê `response_type`, `client_id`, `redirect_uri`, `state`, `scope`; aceita apenas `code`, exige os campos essenciais e URI absoluta HTTPS, grava code com validade de dois minutos e associa user fixo `user-1`; redireciona acrescentando code/state. Não se observou validação do client nem allowlist da URI nesta ação. |
| `POST /oauth/token` | Lê form; suporta grant `authorization_code`; obtém client credentials de Basic Auth ou formulário; valida client ativo/segredo, code, validade, uso, client e URI; marca code usado e grava token opaco aleatório com validade de uma hora. Retorna token Bearer e `expires_in`. |
| `GET /oauth/me` | Lê Bearer token, procura registro e verifica expiração; retorna user_id/client_id/scope ou `401`. O controller não consulta `RevokedAt`. |

#### `GET /oauth/authorize`

Requisição sem body. Query: `response_type=code`, `client_id`, `redirect_uri`, `state` e `scope` opcional. A comparação de `response_type` é ordinal/case-sensitive. `client_id`, `redirect_uri` e `state` não podem ser vazios. `redirect_uri` precisa ser absoluta e usar HTTPS, mas o handler não consulta o client nem sua lista de redirect URIs antes de redirecionar. Gera code aleatório hexadecimal, grava expiração UTC de dois minutos e `user_id` fixo `user-1`; scope ausente vira string vazia. Responde `302` com `Location` para a URI recebida acrescida de `code` e `state`, e também tenta inserir o code em header `code`. Campos inválidos respondem `400` com texto simples `invalid_request: ...`. A ação não tem transação junto à montagem da resposta.

Forma de chamada ilustrativa (URI e client fictícios):

```http
GET /oauth/authorize?response_type=code&client_id=demo-client&redirect_uri=https%3A%2F%2Fexample.invalid%2Fcallback&state=opaque-state&scope=read
```

#### `POST /oauth/token`

Lê campos de formulário `grant_type`, `code` e `redirect_uri`; client credentials vêm preferencialmente de `Authorization: Basic`, ou dos campos `client_id` e `client_secret` quando não há Basic. Aceita somente `grant_type=authorization_code`. Valida client existente/ativo e segredo por comparação direta, code não utilizado/não expirado, correspondência de client e igualdade literal da `redirect_uri` registrada. Em sucesso marca code como usado em chamada separada, gera token opaco aleatório hex, grava expiração em uma hora e retorna `200 application/json` com `access_token`, `token_type: Bearer` e `expires_in: 3600`.

`400` cobre grant não suportado, campos ausentes e code inválido/expirado/usado ou incompatível; `401` cobre credenciais ausentes/inválidas. As falhas retornam objetos `error` em alguns caminhos e podem retornar body vazio em 401. A validação/leitura, marcação de uso e gravação de token não compartilham transação atômica; requests concorrentes podem observar estado antes da marcação. Falha depois de marcar code pode impedir novo uso sem emitir token. Erros de persistência seguem o middleware geral.

Exemplo de form (valores de code/redirect fictícios; credencial não incluída no artefato):

```http
POST /oauth/token
Content-Type: application/x-www-form-urlencoded
Authorization: Basic <credenciais-do-cliente>
```

```text
grant_type=authorization_code&code=<authorization-code>&redirect_uri=https%3A%2F%2Fexample.invalid%2Fcallback
```

#### `GET /oauth/me`

Exige header `Authorization: Bearer <token>` conforme o handler. Sem header, esquema diferente, token vazio, token não encontrado ou token expirado responde `401` sem body específico. Token válido retorna `200 application/json` com `user_id`, `client_id` e `scope`. A ação não consulta `RevokedAt`, não valida scopes pedidos contra uma política e não altera token.

```http
GET /oauth/me
Authorization: Bearer <access-token>
```

Repositórios internos: [authorization codes](../../src/Data.Repositories/Repositories/OAuthAuthorizationCodeRepository.cs), [clients](../../src/Data.Repositories/Repositories/OAuthClientRepository.cs), [tokens](../../src/Data.Repositories/Repositories/OAuthAccessTokenRepository.cs); entidades em `src/Core/Entities/OAuth*.cs`; DI MySQL em `src/Data.Repositories/DI/MySqlDependencyInjection.cs`.

O endpoint `/oauth/token` retorna `400` para grant não suportado ou grant inválido, e `401` para credenciais ausentes/inválidas; `/oauth/authorize` retorna `400` para response type/campos/URI rejeitados; `/oauth/me` retorna `401` quando token ausente, inválido ou expirado. Falhas de persistência seguem o middleware geral.

## Limitações e evidência

`GET /oauth/authorize` escreve um authorization code e retorna `302`, portanto não é armazenado pelo output cache padrão (que guarda somente `200` GET/HEAD). `GET /oauth/me` usa `Authorization: Bearer`; requisições com header Authorization são excluídas pelo padrão de output cache. A configuração global de cache e o limite se o gateway alterar/remover headers estão no [Dossiê §4](../specs/SYSTEM-DOSSIER.md#4-entradas-e-ciclo-de-vida).

“Operacional para compatibilidade” e “será substituído” são contexto e decisão informados pelo responsável, não demonstrados por uso no repositório. Não foram investigados consumidores, serviços externos, MQTT, Traefik ou serviço Node.js, conforme limite aprovado. O código não aplica autenticação global a todos os controllers (`Program.cs` não configura política global de autorização). A marcação de code usado é separada da consulta/validação; atomicidade de uso concorrente não está confirmada. A comparação de client secret é direta no fluxo observado. Estes fatos descrevem a implementação local e não autorizam modificá-la, desativá-la ou inferir seu uso em endpoints que não consultam token.

Preservam-se identificadores técnicos `OAuthController`, `oauth`, nomes de entidades/repos e rotas existentes. Nenhum detalhe de estratégia ou cronograma da substituição é definido aqui.
