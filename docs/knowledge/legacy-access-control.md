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

Repositórios internos: [authorization codes](../../src/Data.Repositories/Repositories/OAuthAuthorizationCodeRepository.cs), [clients](../../src/Data.Repositories/Repositories/OAuthClientRepository.cs), [tokens](../../src/Data.Repositories/Repositories/OAuthAccessTokenRepository.cs); entidades em `src/Core/Entities/OAuth*.cs`; DI MySQL em `src/Data.Repositories/DI/MySqlDependencyInjection.cs`.

O endpoint `/oauth/token` retorna `400` para grant não suportado ou grant inválido, e `401` para credenciais ausentes/inválidas; `/oauth/authorize` retorna `400` para response type/campos/URI rejeitados; `/oauth/me` retorna `401` quando token ausente, inválido ou expirado. Falhas de persistência seguem o middleware geral.

## Limitações e evidência

“Operacional para compatibilidade” e “será substituído” são contexto e decisão informados pelo responsável, não demonstrados por uso no repositório. Não foram investigados consumidores, serviços externos, MQTT, Traefik ou serviço Node.js, conforme limite aprovado. O código não aplica autenticação global a todos os controllers (`Program.cs` não configura política global de autorização). A marcação de code usado é separada da consulta/validação; atomicidade de uso concorrente não está confirmada. A comparação de client secret é direta no fluxo observado. Estes fatos descrevem a implementação local e não autorizam modificá-la, desativá-la ou inferir seu uso em endpoints que não consultam token.

Preservam-se identificadores técnicos `OAuthController`, `oauth`, nomes de entidades/repos e rotas existentes. Nenhum detalhe de estratégia ou cronograma da substituição é definido aqui.
