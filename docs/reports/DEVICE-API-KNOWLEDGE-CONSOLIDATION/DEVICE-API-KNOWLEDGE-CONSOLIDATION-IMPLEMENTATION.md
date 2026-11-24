# Relatório de consolidação do conhecimento da Device API

**Data:** 24/11/2026
**Branch de execução:** `task/shd-knowledge-map-001`
**Revisão funcional descrita:** `43fbdc06e97a58f4ee55a84030a1053d5f1d997f`
**Revisão local inicial:** `48806770fdd486229981ae4973b70632710271e6`
**Autorização:** ordem documental do Arquiteto no texto “Consolidação do conhecimento funcional e técnico — SmartHome-DeviceApi”; escopo qualificado e readiness documental consultados antes da intervenção.

## Escopo e procedimento

A intervenção consolidou descrição do comportamento existente em `docs/knowledge/`, atualizou os instrumentos de descoberta e criou os relatórios de cobertura, reconciliação e lacunas requeridos. A revisão de trabalho é exatamente o commit registrado pela auditoria; a comparação com a revisão `43fbdc0` não encontrou delta funcional em `src/`, `tests/`, `database/`, `docs/contracts/openapi.json` nem `docs/specs/`. As conclusões da auditoria permanecem aplicáveis sem mesclar revisões diferentes.

Foram consultados o inventário e os quatro artefatos da auditoria de 24/11/2026, as referências funcionais existentes, Knowledge Map, System Dossier, contrato de engenharia, Repository Readiness, especificações aplicáveis, OpenAPI e procedência, implementação HTTP/Core/repositórios/SQL/configuração, histórico Git, suíte HTTP Dashboard vigente e relatórios locais relacionados. `tests/Api.Tests` foi excluída por estar `Retired`. Nenhum teste ou build foi executado nesta intervenção; não eram necessários para as alterações descritivas, e a autorização exigia a verificação documental `git diff --check`.

O material preserva quatro categorias distintas: implementação determinada por inspeção; comportamento observado em ensaio local já registrado na auditoria; comportamento condicionado a MySQL/gateway/runtime; e comportamento normativo segundo especificação. Os ensaios H1/H2/P1/P2 da auditoria não foram repetidos nem promovidos a evidência de produção. Exemplos adicionados são sintéticos, vinculados a DTO/handler e identificados quando não provam persistência.

## Documentos atualizados

### Referências funcionais existentes

- [Devices](../../knowledge/devices.md): campos, nulabilidade/requerimento efetivo, binding, conversões de data/protocolo, projeção de leitura, SQL e ordem transacional de criação, capabilities/properties embutidas, PATCH/DELETE, erros e exemplo sintético.
- [Capabilities e History](../../knowledge/capabilities.md): filtros, aliases, escopo real, cache Tiny, formatos, associações, inclusão/value PATCH/genérico, ar-condicionado, histórico e exemplos.
- [Capability Types](../../knowledge/capability-types.md): modelo, identidade, cinco operações, ícones, transações, status e exemplos.
- [Device Metrics](../../knowledge/device-metrics.md): DTOs, faixas de validação, rota/body, current/history, campos não gravados no histórico, paginação e exemplo.
- [Settings](../../knowledge/settings.md): settings globais e por Device, view efetiva, leitura parseada, upsert/reset, transação, erros e exemplos.
- [Properties](../../knowledge/properties.md): modelo, validação não chamada, SQL construído, upsert, commit antes de URL e exemplos com limite explícito.
- [Groups](../../knowledge/groups.md): CRUD normativo e legado, associações, projeções, formatos e exemplos separados por autoridade.
- [Platforms e Monitored Places](../../knowledge/platforms-and-places.md): superfície somente leitura e projeções.
- [Smart Home Capabilities](../../knowledge/smart-home-capabilities.md): filtro nominal, query filters, lookup singular e exemplos.
- [Dashboards, widgets e catálogo](../../knowledge/dashboards-and-widgets.md): doze operações, relação entre modelos, regras de escrita, respostas, erros, limites operacionais e exemplos.
- [Controle de acesso proprietário legado](../../knowledge/legacy-access-control.md): três rotas, authorization code/token, validações, respostas, limites de concorrência e exemplos sanitizados.
- [Endpoints utilitários](../../knowledge/utility-endpoints.md): timezone, data/hora, health, OpenAPI de Development, watchdog e fronteira de segurança.

Nenhuma referência normativa foi alterada. As referências funcionais continuam informativas e declaram explicitamente quando a norma aplicável é parcial ou não existe.

### Índices e arquitetura

- [Knowledge Map](../../rfc/KNOWLEDGE-MAP.md): data e cobertura descritiva atualizadas; tabela por domínio agora identifica comportamento e limitações; links para os quatro artefatos de execução incluídos. Estados normativos/gaps existentes preservados.
- [System Dossier](../../specs/SYSTEM-DOSSIER.md): data atualizada; contradição sobre existência de OpenAPI versionado removida; 76 pares/22 com tratamento normativo direto referenciados; persistência e integração externa delimitadas; tratamento de erro/trace e gateway descritos sem generalização. Estado `Draft` preservado.
- [Readme.md](../../../Readme.md): rota corrigida para `/api/v1/devices`; alegação não sustentada de cascade removida; exemplo GET substituído por forma sintética compatível com a projeção atual e anotado sobre capabilities/settings.

### Relatórios criados

- [Matriz consolidada por operação](DEVICE-API-CONSOLIDATED-KNOWLEDGE-COVERAGE.md): 76 linhas e 14 dimensões solicitadas, fonte funcional, handler e achados associados.
- [Reconciliação AUD-DEV-001–013](DEVICE-API-AUDIT-KNOWLEDGE-RECONCILIATION.md): tratamento e limite de cada achado; nenhuma correção funcional declarada.
- [Lacunas residuais](DEVICE-API-RESIDUAL-KNOWLEDGE-GAPS.md): cinco lacunas externas/normativas com evidência já disponível, autoridade e critério de resolução.
- Este relatório de implementação documental.

Os quatro documentos originais da auditoria foram mantidos sem alteração, para preservar autoria e caráter histórico.

## Cobertura alcançada

| Medida | Resultado | Critério |
|---|---:|---|
| Operações inventariadas | 76/76 | Pares método + template, incluindo aliases e rotas OAuth legadas, conforme auditoria e sem delta funcional posterior. |
| Operações integralmente descritas | 76/76 | Cada uma tem entrada/identidade, requiredness/nulabilidade aplicável, validações ou ausência observada, regras, transformações, persistência/projeção, saída, erros, fronteira de segurança, efeitos/semântica, exemplo aplicável e rastreabilidade em estado D/DE/NA na [matriz](DEVICE-API-CONSOLIDATED-KNOWLEDGE-COVERAGE.md). |
| Operações parcialmente documentadas | 0/76 | Nenhuma célula P após a revisão final das páginas de domínio. |
| Operações com informação externa bloqueadora | 0/76 | Dependências que afetam garantias foram explicitadas como DE; o comportamento interno já determinável foi descrito. |
| Operações com investigação interna pendente | 0/76 | Questões resolvíveis pelos artefatos e código examinados foram incorporadas. |
| Operações com dependência externa identificada | 76/76 | Política de admissão/segurança do gateway não está no repositório; isso não afirma acesso anônimo. Além disso, 73 operações usam MySQL, cujo DDL completo não está preservado. |
| Domínios funcionais | 12/12 referências existentes | Os doze documentos de `docs/knowledge/` foram avaliados; os doze domínios inventariados estão cobertos sem nova hierarquia paralela. |
| Achados documentais tratados | 13/13 | Cada AUD-DEV tem referência, categoria de evidência e limite residual em documento próprio. |

Nas 14 dimensões por operação, as 1.064 células (76 × 14) foram classificadas como **D=876**, **DE=149** e **NA=39**; não há células P, PI ou B. Isso descreve a suficiência documental por dimensão, não uma pontuação de qualidade.

Não foi criada pontuação de qualidade. A classificação distingue documentação da aplicação, dependência externa e não aplicabilidade; não implica execução de cada operação, completude do OpenAPI, aceitação normativa ou disponibilidade pública.

## Conhecimento material consolidado

- Os dez membros requeridos no binding de criação de Device são discriminados, embora o snapshot OpenAPI omita `requestBody.required` e `required` desses campos. O exemplo de cinco campos falha antes da persistência; o exemplo substituto é compatível com binding, sem garantia de escrita real.
- GETs comuns de Device não carregam capabilities e devolvem `settings` vazio pela conversão HTTP, mesmo quando o SELECT carrega propriedades distintas.
- Dados enviados em um DTO não equivalem ao que a operação escreve: a criação não grava explicitamente `PowerOn`/`Active`; properties embutidas descartam description; certos PATCHes/associações usam chave de identidade diferente do que o consumidor pode inferir.
- O caminho de capabilities embutidas com platforms passa um objeto ao parâmetro escalar do Dapper e falha no binder antes do SQL; `ReferenceId` também não aparece no INSERT identificado.
- Properties individual/filtrada monta SQL incompatível; POST pode concluir a escrita e falhar em seguida na geração de URL. Um 500 nesse ponto não prova que não houve commit.
- A lista de settings de Device lê uma view efetiva, enquanto reset remove linhas de override; updates globais são UPDATEs e `KeyNotFoundException` produz 500 no middleware atual.
- Prefixos de rota não determinam por si o escopo: quatro actions de Capability têm alias sem parâmetro requerido; Smart Home compara o segmento chamado ID com `sh.Name`, e lookup por UID ignora esse segmento.
- Respostas e erros são específicos por fluxo; headers/status do snapshot e formatos de middleware não devem ser universalizados. O ramo geral, o binding, estado do ar e Dashboard têm envelopes diferentes.
- A política base de output cache do ASP.NET Core 9 tem TTL local de dois segundos e pode manter GET/HEAD `200` sem `Authorization`/identidade autenticada e sem `Set-Cookie`; sua chave varia por método, esquema, host/porta, path e query, sem variação por identidade. Essa semântica está documentada por domínio e marcada nas 37 operações GET; elegibilidade após o gateway segue dependente de configuração externa. Tiny também possui cache `IMemoryCache` próprio com chave que omite filtros.
- Health retorna estado fixo e não verifica MySQL; watchdog é processo hospedado separado, executa `SELECT 1` e solicita saída após falhas consecutivas.

## Lacunas remanescentes

O que permanece legítimo e sua autoridade estão enumerados em [DEVICE-API-RESIDUAL-KNOWLEDGE-GAPS](DEVICE-API-RESIDUAL-KNOWLEDGE-GAPS.md): schema MySQL efetivo; políticas APISIX/ingresso; revisão e configuração implantadas; procedência original de geração do snapshot; e decisões normativas futuras em domínios não especificados. Essas lacunas não impedem descrever o comportamento local determinado. Aceite operacional de Dashboard e resultados de testes permanecem no estado já registrado nas respectivas fontes, sem promoção nesta intervenção.

## Verificações e preservação

- `git diff --check`: aprovado, sem saída.
- Inventário documental: 76 operações na matriz consolidada e 76 na histórica; conjuntos de method + template idênticos e auditoria consultada na mesma revisão-base.
- Links: zero destinos relativos inexistentes entre os documentos alterados/criados.
- Escopo: 19 arquivos documentais no allowlist; nenhuma alteração fora dos documentos autorizados.
- Segredos: exemplos usam valores sintéticos/placeholders; nenhum segredo foi copiado.
- Código-fonte, Core, repositories, SQL versionado, DTOs, testes, `database/`, OpenAPI, especificações normativas, Repository Readiness, contrato de engenharia, EKOM e infraestrutura não foram modificados.
- Nenhum build, teste, flash, monitor ou acesso a hardware/ambiente real foi executado.

## Estado

**Consolidação documental concluída** para o comportamento local determinável nas 76 operações e comportamentos transversais investigados, sujeita às lacunas externas/normativas listadas. Esta conclusão é do executor da intervenção, não auditoria independente nem aprovação do Arquiteto. O merge/deploy e qualquer mudança funcional permanecem fora do escopo autorizado.
