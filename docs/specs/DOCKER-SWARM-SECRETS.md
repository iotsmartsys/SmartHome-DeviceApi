# Especificação — Configuração da Devices API por arquivo

**ID:** `SHD-DOCKER-SECRETS-001`

**Classe da fonte:** Normativa

**Versão:** 0.1

**Estado normativo:** Aprovada [`Approved`] pela ordem explícita do Arquiteto
em 28/11/2026 para implementar esta versão.

**Technical readiness:** Pronta [`Ready`], análise em
[`docs/reports/DOCKER-SWARM-SECRETS/analysis/2026-11-28T030354Z-0.1-973053ab-82a4-4e28-b530-7510794e5cb7-implementability-analysis.md`](../reports/DOCKER-SWARM-SECRETS/analysis/2026-11-28T030354Z-0.1-973053ab-82a4-4e28-b530-7510794e5cb7-implementability-analysis.md)

**Estado da implementação:** Em andamento [`In Progress`]

**Registro da implementação:**
[`docs/reports/DOCKER-SWARM-SECRETS/implementation/2026-11-28T033045Z-f42edb4e-ec08-40e5-a283-a89b200ec823-implementation.md`](../reports/DOCKER-SWARM-SECRETS/implementation/2026-11-28T033045Z-f42edb4e-ec08-40e5-a283-a89b200ec823-implementation.md)

**Validação complementar:**
[`docs/reports/DOCKER-SWARM-SECRETS/implementation/2026-11-28T033727Z-0.1-3ec1c408-65cd-4fe8-8f7a-82fa33be08d6-implementation-addendum.md`](../reports/DOCKER-SWARM-SECRETS/implementation/2026-11-28T033727Z-0.1-3ec1c408-65cd-4fe8-8f7a-82fa33be08d6-implementation-addendum.md)

**Validação final:**
[`docs/reports/DOCKER-SWARM-SECRETS/implementation/2026-11-28T033921Z-0.1-1bf5a80e-5377-4d87-b3ca-894456a1df06-final-validation.md`](../reports/DOCKER-SWARM-SECRETS/implementation/2026-11-28T033921Z-0.1-1bf5a80e-5377-4d87-b3ca-894456a1df06-final-validation.md)

**Estado da entrega:** Não pronta [`Not Ready`]

**Relação normativa:** Nova [`New`]. Esta especificação governa o carregamento
da configuração da Devices API por arquivo e o consumo da chave lógica
`ConnectionStrings:Devices`. Não define como uma infraestrutura externa
provisiona ou disponibiliza o arquivo.

## 1. Objetivo e decisões aprovadas

Definir leitura nativa de configuração por arquivo no bootstrap local da
Devices API, preservando a execução em `localhost` sem Docker Swarm ou Docker
Secrets e exigindo a fonte por arquivo em produção.

Decisões do Arquiteto incorporadas:

- utilizar `Microsoft.Extensions.Configuration.KeyPerFile` no bootstrap da
  API, sem biblioteca compartilhada;
- preservar `ConnectionStrings:Devices` e o consumo por
  `GetConnectionString("Devices")`;
- reconhecer `/run/secrets/ConnectionStrings__Devices` como arquivo de
  configuração;
- em `Development`, permitir ausência do arquivo e utilizar os provedores de
  configuração locais já suportados pelo .NET;
- quando o arquivo existir, seu valor prevalece sobre variáveis de ambiente e
  provedores anteriores;
- arquivo presente vazio, ilegível ou inválido causa falha explícita sem
  fallback;
- em `Production`, exigir arquivo válido e não aceitar variável de ambiente
  como substituta;
- aceitar apenas ambientes explicitamente reconhecidos. Ambiente ausente,
  desconhecido ou conflitante não pode ser tratado como `Development`;
- utilizar somente valores sintéticos em testes e exemplos; não registrar
  valores sensíveis em logs, exceções ou respostas HTTP.

## 2. Contexto, escopo e arquitetura

O bootstrap atual obtém `GetConnectionString("Devices")` e passa o resultado à
composição MySQL. A mudança permanece local a `src/Api`: adicionar a fonte de
arquivo à configuração do host com a precedência requerida, validar o valor
selecionado e continuar entregando-o pela mesma chave e ao mesmo ponto de
composição. Não criar abstração ou biblioteca compartilhada.

O provider KeyPerFile usa nomes de arquivo como chaves e o conteúdo como valor;
`__` representa separador de chave. Assim, o arquivo
`/run/secrets/ConnectionStrings__Devices` configura
`ConnectionStrings:Devices`. O diretório passado ao provider é absoluto, de
acordo com a [documentação oficial Microsoft](https://learn.microsoft.com/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0#key-per-file-configuration-provider).

Escopo: configuração do host, bootstrap da API, validação da connection string
e testes automatizados sintéticos dos cenários desta especificação.

Fora de escopo: provisionamento e transporte do arquivo; mecanismos de
orquestração; alteração de infraestrutura; mudança de outras APIs ou
bibliotecas; alteração do contrato de conexão; limpeza de histórico Git; e
execução de `tests/Api.Tests` (suíte `Retired`).

Contrato construtivo: `SHD-ENGINEERING-001@0.1 Approved`. A extensão de
Repository Readiness para este recorte foi aprovada pelo Arquiteto na ordem de
28/11/2026 e está registrada em
[`Repository Readiness`](../rfc/REPOSITORY-READINESS.md) e na
[avaliação de escopo](../reports/repository-readiness/2026-11-28T031617Z-0.1-8a136f88-2e3f-4f92-9d27-4681448629a1.md). Essa habilitação cobre somente
SHD-DOCKER-SECRETS-001@0.1.

## 3. Requisitos

### `SHD-DS-001` — Identificação confiável do ambiente

O bootstrap utiliza o nome efetivo de ambiente do host ASP.NET Core. Somente
`Development` e `Production` são aceitos para este contrato. O ambiente deve
ser fornecido explicitamente por configuração de processo/host confiável; se
as fontes de ambiente suportadas pelo host estiverem simultaneamente
definidas, seus valores devem concordar. Ambiente ausente, desconhecido,
conflitante ou obtido apenas por default implícito encerra a inicialização com
falha segura. Nenhuma condição desconhecida pode selecionar as regras de
`Development`.

O ambiente efetivo deve refletir a configuração autoritativa do processo. Em
produção, o operador do runtime é responsável por definir explicitamente
`Production` e impedir sobrescritas não autorizadas. A regra é compatível com
`WebApplication.CreateBuilder`; a precedência entre as variáveis de ambiente
do host deve ser considerada conforme a
[documentação oficial ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/environments?view=aspnetcore-9.0).

### `SHD-DS-002` — Fonte KeyPerFile

O bootstrap usa `Microsoft.Extensions.Configuration.KeyPerFile` para
reconhecer `/run/secrets/ConnectionStrings__Devices` como
`ConnectionStrings:Devices`. Quando esse arquivo existe e pode ser lido, seu
valor prevalece sobre variáveis de ambiente e demais fontes de configuração
registradas anteriormente.

### `SHD-DS-003` — Desenvolvimento local sem arquivo

Em `Development`, a ausência de `/run/secrets` ou do arquivo esperado não
bloqueia o carregamento da aplicação. A API obtém `ConnectionStrings:Devices`
dos provedores locais já suportados pelo host .NET, como configuração local,
User Secrets, variáveis de ambiente ou argumentos, conforme a configuração
existente. Se nenhuma fonte fornecer valor utilizável, a API falha na validação
de configuração sem expor conteúdo sensível. Não é necessário instalar ou
iniciar Docker Swarm para executar a aplicação localmente.

### `SHD-DS-004` — Arquivo presente inválido

Se o arquivo esperado existir, vazio, ilegível ou com conteúdo que não forme
uma connection string MySQL utilizável, o bootstrap falha explicitamente em
`Development` e `Production`. Não pode usar variável de ambiente, configuração
JSON ou qualquer outra fonte como fallback para um arquivo presente inválido.

### `SHD-DS-005` — Produção exige arquivo

Em `Production`, o arquivo `/run/secrets/ConnectionStrings__Devices` deve
existir, ser legível, não vazio e conter connection string válida. A ausência
do arquivo causa falha de inicialização mesmo quando outra fonte, inclusive
`ConnectionStrings__Devices` em variável de ambiente, contém valor válido. Em
produção, o valor dessa variável e de qualquer fonte alternativa não pode
substituir o arquivo.

### `SHD-DS-006` — Validade e chave de consumo

O valor efetivamente selecionado deve ser analisável pela biblioteca MySQL já
utilizada e conter os componentes requeridos para abrir a conexão. Após
validação, permanece acessível pela chave `ConnectionStrings:Devices` e é
consumido por `GetConnectionString("Devices")` no bootstrap existente. Falha
de parsing, ausência de campos requeridos ou conteúdo vazio encerra a
inicialização com mensagem estável e sanitizada.

### `SHD-DS-007` — Não exposição

O conteúdo do arquivo, valores de configuração e connection strings não devem
aparecer em logs, exceções propagadas, respostas HTTP ou diagnósticos. Erros
podem informar uma categoria estável da falha, mas não podem incluir valores,
trechos, detalhes do parser ou exceções internas que os contenham. Respostas
HTTP relacionadas a falha de conexão continuam genéricas.

## 4. Critérios de aceite e evidências

Criar testes automatizados sintéticos para os cenários abaixo integra o escopo
de implementação desta especificação. Eles foram criados em
`tests/Api.ConfigurationTests`; em 28/11/2026, os 23 casos foram executados e
aprovados, conforme [relatório EKOM de validação final](../reports/DOCKER-SWARM-SECRETS/implementation/2026-11-28T033921Z-0.1-1bf5a80e-5377-4d87-b3ca-894456a1df06-final-validation.md). Nunca usar credenciais reais. A
suíte `tests/Api.Tests` permanece `Retired` e proibida.

| Critério | Cenário e resultado observável | Meio de validação |
|---|---|---|
| `SHD-DS-AC-001` | `Development`, sem diretório/arquivo de secret e com valor sintético fornecido por provedor local já suportado: API constrói a configuração e obtém o valor. | Teste automatizado do bootstrap/configuração, com ambiente e provedor isolados. |
| `SHD-DS-AC-002` | `Development`, sem arquivo e sem qualquer fonte de connection string: inicialização falha com erro de configuração sanitizado, sem tentar Swarm. | Teste automatizado de falha, verificando categoria/mensagem. |
| `SHD-DS-AC-003` | Arquivo válido e fontes anteriores com valores sintéticos diferentes: `GetConnectionString("Devices")` retorna o valor do arquivo. | Teste automatizado da precedência e da chave lógica. |
| `SHD-DS-AC-004` | Arquivo presente vazio ou ilegível, mesmo com valor sintético válido em variável de ambiente: inicialização falha e não seleciona a variável. | Testes automatizados separados para arquivo vazio e ilegível. |
| `SHD-DS-AC-005` | Arquivo presente malformado ou sem campos de conexão requeridos, mesmo com outra fonte válida: inicialização falha sem fallback. | Testes automatizados com valores fictícios, sem acesso ao banco. |
| `SHD-DS-AC-006` | `Production` explícito com arquivo ausente e variável de ambiente válida: inicialização falha. | Teste automatizado da política de produção. |
| `SHD-DS-AC-007` | `Production` explícito com arquivo válido e variável conflitante sintética: valor obtido por `GetConnectionString("Devices")` vem exclusivamente do arquivo. | Teste automatizado com providers concorrentes. |
| `SHD-DS-AC-008` | Ambiente ausente, desconhecido ou conflitante: bootstrap falha; não escolhe comportamento `Development`. | Testes automatizados para cada caso. |
| `SHD-DS-AC-009` | Valores sentinela sintéticos nas fontes/erros não aparecem em logs, exceções nem respostas HTTP após falha simulada. | Teste automatizado com inspeção dos canais observáveis. |
| `SHD-DS-AC-010` | A connection string validada segue disponível pela chave lógica existente e é obtida por `GetConnectionString("Devices")`. | Teste automatizado do bootstrap e inspeção estática do ponto de composição. |
| `SHD-DS-AC-011` | A API compila após a implementação. | `dotnet build src/Api/Api.csproj`; registrar estado terminal e código de saída. |

Testes não dependem de Docker, Swarm, MySQL ou credenciais reais. Os builds da
API e do projeto de testes e a execução dos 23 casos foram realizados como
parte da implementação; os resultados constam nos relatórios vinculados no
cabeçalho.

## 5. Contrato do ambiente de execução

Quando o ambiente exige configuração por arquivo, o runtime deve disponibilizar
`/run/secrets/ConnectionStrings__Devices` com conteúdo UTF-8 legível pelo
processo da API e permissões que concedam leitura somente aos principals
necessários à execução. Falta de diretório, arquivo, acesso ou valor válido em
`Production` impede a inicialização. Em `Development`, o arquivo é opcional e
os provedores locais existentes permanecem disponíveis se o arquivo não
existir.

Este contrato não prescreve como o ambiente cria, nomeia ou monta o arquivo.

## 6. Autoridades e conhecimento relacionado

- `AGENTS.md`: dados sintéticos, build canônico e proibição da suíte
  `Retired`.
- `docs/rfc/REPOSITORY-ENGINEERING-CONTRACT.md` §7–§10: segredos externos,
  falhas sem payload sensível, dependências e validação.
- `docs/rfc/REPOSITORY-READINESS.md`: qualificação existente cobre capabilities
  e dependências do estado do ar, com extensão própria aprovada e limitada a
  este escopo.
- `docs/specs/SYSTEM-DOSSIER.md`, `docs/rfc/KNOWLEDGE-MAP.md` e
  `EKM-GAP-0004`: fatos de configuração e lacuna de segurança vigentes.
- Documentação oficial Microsoft citada nos requisitos para semântica do
  provider KeyPerFile e identificação do ambiente ASP.NET Core.

## 7. Estados

O Arquiteto aprovou explicitamente SHD-DOCKER-SECRETS-001@0.1 e ordenou sua
implementação em 28/11/2026. A versão está `Approved / In Progress`; builds e
23 testes automatizados passaram, e a implementação permanece encaminhada para
revisão. A qualificação Ready restringe-se ao recorte descrito nesta
especificação. Nenhuma alteração de infraestrutura integra este contrato.
