# Repository Engineering Contract — Device API

**ID:** `SHD-ENGINEERING-001`
**Classe da fonte:** Proposta normativa
**Estado:** Proposed
**Versão:** 0.1
**Repositório:** SmartHome-DeviceApi
**Responsável pela aprovação:** Arquiteto humano solicitante; identidade e decisão devem ser registradas no aceite.
**Aprovação e habilitação:** Pendentes. A ordem de preparar estes documentos não aprova as regras.
**Escopo proposto:** capabilities: inclusão, atualização de value, PATCH genérico, consulta comum e smart home; entidades, conversões, DI, repositórios, queries e tratamento de falhas materialmente necessários a SHD-AIR-CONDITIONER-STATE-001@0.2. Demais domínios não são qualificados por este documento.

## 1. Arquitetura e limites

Preservar API/controller/modelo HTTP → contratos e regras Core → repositório Dapper → MySQL. Controller deve tratar transporte, binding e resposta HTTP; não executar SQL nem concentrar regras de estado. Regras de normalização e combinação devem permanecer no Core, independentes de HTTP e banco. Repositório implementa acesso e unidade transacional; não devolve IActionResult nem conhece DTO da API.

O uso direto de interfaces de repositório por controllers já existe e é permitido; não introduzir camada de serviço apenas para encaminhar chamadas. Serviço de domínio necessário deve seguir interface no Core e implementação registrada por DI. Não criar framework de comandos, barramento, CQRS ou infraestrutura genérica de estados para esta funcionalidade.

## 2. Organização de arquivos

- Controllers e rotas: `src/Api/Controllers`; DTOs e conversões HTTP: `src/Api/Models`.
- Entidades/valores: `src/Core/Entities`; conversores por tipo: `src/Core/Entities/DataTypes`; exceções de domínio: `src/Core/Exceptions`.
- Interfaces: `src/Core/Contracts/Repositories` ou `Services`; serviços: `src/Core/Services`; composição Core: `src/Core/DI`.
- Persistência: `src/Data.Repositories/Repositories`; SQL reutilizado: subpasta `Queries`; DI de MySQL: `src/Data.Repositories/DI`.
- Especificações: `docs/specs`; evidências: `docs/reports`; fontes e estado: `docs/rfc`.

Usar os destinos existentes; nova pasta estrutural exige decisão explícita. Não mover arquivos alheios à tarefa nem reformar o legado por estilo.

## 3. Nomenclatura e estilo

Código C#: PascalCase em tipos, métodos e propriedades de domínio; interfaces com I; métodos novos que retornam Task com sufixo Async; camelCase em variáveis e parâmetros novos. Preservar nomes públicos e nomes serializados existentes, inclusive snake_case dos DTOs e `air_condicionator`.

Usar indentação de quatro espaços e o estilo de chaves C# dos arquivos existentes. Preservar namespace de cada módulo e nullable habilitado. Não usar supressão de null para esconder erro de entrada. Não reformatar arquivos inteiros nem padronizar nomes legados fora do delta. Normalização de tokens do protocolo deve ser explícita e independente da cultura; não aplicar ToLower a todo JSON ou texto arbitrário.

## 4. Dependências

API pode referenciar Core e Data.Repositories para composição; operações usam contratos Core. Data.Repositories referencia Core. Core não pode depender de API, ASP.NET, Dapper ou MySqlConnector. Não criar dependência circular nem acrescentar acesso ao Event Gateway/MQTT no Core.

Preservar os lifetimes registrados e não compartilhar IDbConnection/transação mutável entre requisições concorrentes. Adicionar interfaces/serviços específicos quando necessários não autoriza alterar globalmente o modelo de DI.

## 5. Padrões de implementação

Preservar controllers MVC, binding explícito e respostas IActionResult/status definidos pela especificação. Manter DTOs separados das entidades e conversões explícitas ou operadores já usados no projeto. O campo externo value permanece string; não converter silenciosamente todas as capabilities em objetos.

Usar o tipo efetivo associado no banco; entrada do cliente não autoriza reclassificar a capability para contornar validação. Distinguir interpretação de estado armazenado e aplicação de comando recebido. Regras locais de domínio podem usar tipos concretos e tabelas de operações, sem cadeia crescente de condições no controller. Não exigir uma interface para cada função pura.

Propagar CancellationToken nas novas operações assíncronas de banco. Não alterar semântica funcional por conveniência de persistência. Não copiar comportamento legado incompatível com o contrato da tarefa.

## 6. Persistência e dados

Acesso ao banco deve usar Dapper e SQL parametrizado; não interpolar entradas em SQL. Preservar MySQL e schema existente; varchar(1000) de Capabilities.Value foi confirmado pelo Arquiteto. Não introduzir ORM, novas colunas ou migração em massa neste escopo.

A unidade de leitura/modificação/gravação do estado pertence ao repositório, na mesma conexão e transação quando necessária à atomicidade. Transação deve concluir antes do sucesso e ter rollback/dispose em falha. Todas as queries participantes devem compartilhar a transação; leitura anterior desprotegida não pode decidir o valor final combinado. Proteger também os caminhos concorrentes de PATCH genérico e não sobrescrever value ao editar somente metadados.

Ausência de diferença no valor não comprova ausência do registro. Repetição transitória deve reiniciar a unidade coerente, com leitura atualizada e sem reutilizar transação inválida. Não considerar commit de resultado desconhecido como sucesso comprovado. Preservar comportamento das demais capabilities sem impor-lhes normalização de ar.

Consultas não gravam normalização nem alteram timestamp. Estado e comandos não comprovam atuação física. Não usar cache em memória da instância como fonte de verdade ou lock distribuído.

## 7. Informações sensíveis e versionamento

Não introduzir segredos em código, configuração, documentos, exemplos, logs, testes ou metadados Git. Usar dados sintéticos e configuração externa já fornecida ao bootstrap; não copiar connection strings existentes. Arquivo local .env, quando utilizado pelo operador, deve permanecer não versionado; verificar o destino antes de criar configuração local.

Inspecionar somente o delta do recorte antes de commit/push. `.gitignore` não limpa histórico. A lacuna preexistente EKM-GAP-0004 não é aceita como débito nem encerrada por este contrato; não transportar seu conteúdo para novos arquivos. Remediação de credenciais, histórico ou serviços externos exige ordem própria.

## 8. Falhas e observabilidade

Validar no domínio e traduzir para HTTP na API. Usar as exceções de domínio e o middleware existentes quando compatíveis; permitir extensão específica e tipada para o conflito de estado definido na especificação, sem alterar genericamente o tratamento de erros dos outros domínios.

Não tratar todo ArgumentException como erro do cliente nem capturar todas as exceções e convertê-las em sucesso. Falha de persistência deve propagar ao tratamento vigente. Erros novos não devem conter payload bruto, tokens, connection strings ou valores sensíveis; usar mensagens estáveis e logs estruturados com identificador da operação, sem copiar o padrão legado de registrar detalhes arbitrários de exceções. O comportamento legado fora do recorte não é refeito nesta tarefa.

## 9. Tecnologias e abstrações

Usar C#/.NET 9, ASP.NET Core MVC, DI Microsoft, Dapper e MySqlConnector já referenciados. Para JSON interno do estado, usar biblioteca padrão .NET; Newtonsoft/JsonPatch existente permanece no contrato HTTP atual. Não substituir o serializador global nem adicionar pacotes sem necessidade prevista e aprovada. Não atualizar framework ou versões de dependências como efeito colateral.

## 10. Build, testes e conformidade

Build canônico: `dotnet build src/Api/Api.csproj`, com resultado terminal registrado. Não usar a solução como substituto, pois ela referencia a suíte histórica Retired. Não executar, corrigir ou ampliar `tests/Api.Tests`.

A especificação determina artefatos e cenários de validação; SHD-AIR-CONDITIONER-STATE-001@0.2 exige inspeção e evidências HTTP/MySQL, sem novos artefatos de teste. Executar testes/HTTP/banco requer autorização operacional específica. Build não comprova concorrência nem persistência. Evidências ausentes devem constar como Not Executed, sem alegar conclusão operacional.

Verificar regras por inspeção de dependências, destinos, assinaturas, consultas, unidade transacional, tratamento de falhas e delta; build verifica construção. Guardas inexistentes não devem ser simuladas como aprovadas. Não criar ferramenta de governança ou suíte genérica como parte da implementação do ar.

## 11. Precedentes candidatos a oficiais após aprovação

| Alteração | Fonte/símbolo existente | Regra exemplificada | Limite |
|---|---|---|---|
| Controller | `src/Api/Controllers/CapabilityTypeController.cs`, GetCapabilityTypeById | MVC, DTO, IActionResult, repositório por interface | Não copiar regras funcionais de tipos para estado |
| DTO | `src/Api/Models/SmartHomeCapability.cs`, conversão implícita | Separação HTTP/Core e value textual | Nova normalização deve respeitar o contrato do ar |
| Serviço | `src/Core/Services/AddCapabilityService.cs`, AddAsync | Serviço Core por contrato e DI | Não é justificativa para camadas adicionais |
| Tipo de dado | `src/Core/Entities/DataTypes/CapabilityDataType.cs`, Convert | Conversão por tipo fora do controller | Conversão atual não basta para merge; UpdateValue não participa de todo fluxo |
| Persistência | `src/Data.Repositories/Repositories/CapabilityRepository.cs`, AddAsync/UpdateAsync | Transação, parâmetros e repositório | Leitura antecipada e overwrite atuais não são precedentes de atomicidade correta do ar |
| Queries | `src/Data.Repositories/Repositories/Queries/CapabilityQuery.cs` | SQL centralizado e parametrizado | UpdateValue atual substitui o estado, comportamento a corrigir no recorte |
| DI | `src/Core/DI/CoreDI.cs`, AddCore; `src/Data.Repositories/DI/MySqlDependencyInjection.cs`, AddMySqlData | Composição e interfaces | Preservar isolamento das conexões |
| Falhas | `src/Api/Middlewares/ExceptionHandler.cs`, HandleExceptionAsync | Tradução domínio/HTTP e propagação | Logs amplos legados não autorizam exposição nova |

## 12. Evolução e exceções

Especificação governa comportamento; este contrato, construção. Precedência: especificação → contrato aprovado → precedentes oficiais → código restante → escolha local, sem revogação silenciosa de normas.

Nova abstração transversal, persistência geral, lifecycle, autenticação, dependência ou mudança de consumidor fora do recorte exige decisão arquitetural explícita. Exceção deve nomear regra, motivo, alcance e validade. Nenhuma exceção está proposta nesta revisão.

## 13. Fatos, propostas e adoção

Fatos: três projetos/camadas, Dapper/MySQL, DTO string, transações e DI já existem. Atomicidade do ar ainda não está implementada e não é requisito para qualificar a baseline para construí-la.

Todas as regras imperativas acima são propostas até aprovação humana; frequência no código não as torna norma. Não se declara auditoria global do legado. O escopo exclui dashboard funcional, OAuth, infraestrutura/deploy, consumidores externos e publicação MQTT/Alexa.

Adoção proposta: EKOM 5.0 exclusivamente para o escopo declarado e suas dependências materiais. AGENTS.md ainda declara 4.6 e referencia perfis externos 5.0; a aprovação deve autorizar reconciliar esse roteamento explicitamente, preservando o histórico 4.x e sem qualificar os outros domínios por inferência.

Pendências humanas: aprovar SHD-ENGINEERING-001@0.1 e adoção delimitada; confirmar habilitação após avaliação da mesma revisão aprovada. Aprovação pode ser registrada numa única decisão explícita com versão e alcance. Este documento não substitui a análise Ready da funcionalidade nem a ordem de implementação.
