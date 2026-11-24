# Catálogo de achados da auditoria documental

**Revisão da API** `43fbdc06e97a58f4ee55a84030a1053d5f1d997f`. [Relatório e experimentos](DEVICE-API-DOCUMENTATION-AUDIT.md) · [Matriz por rota](DEVICE-API-DOCUMENTATION-COVERAGE-MATRIX.md) · [Base normativa e histórico](DEVICE-API-EKOM-EFFECTIVENESS-ASSESSMENT.md).

O esperado abaixo identifica a necessidade do leitor ou cita um requisito existente. Recomendações dependem de decisão do Arquiteto. A–G são as classes causais definidas na avaliação metodológica. Referências curtas a controllers, modelos e repositórios correspondem aos caminhos clicáveis da matriz. D-01 a D-16 remetem à [reconciliação histórica](../../../reports/SHD-KNOWLEDGE-MAP/2026-11-23T211211Z-c31c6cb4-2d80-42cd-9fbf-9dd40921395c-openapi-reconciliation.md#L55). A gravidade mede o efeito de confiar na informação. A confiança qualifica apenas a afirmação sustentada. Não há ocorrência crítica confirmada nesta investigação.

## AUD-DEV-001 — O contrato de entrada permite omissões rejeitadas pelo host

**OAPI · DRIFT · Alta · Confirmado** · 28 operações com requestBody, em particular criação de Device.

**Esperado.** Informar se o corpo pode faltar, quais campos podem faltar ou ser nulos e distinguir obrigatoriedade de tipo, MVC, regra funcional e banco.

**Documentado.** 28/28 requestBody omitem required. Device possui zero campos required e dez membros de referência relevantes aceitam null no snapshot. D-14/D-15 alertam sobre a classe do problema. A referência Devices remete à reconciliação sem fornecer o conjunto mínimo.

**Implementado ou observado.** Com o media type anunciado, corpo vazio produziu 400 nas 28 rotas. No POST de Device houve 400 para omissão e para null de cada um dos dez campos da tabela do relatório. O objeto vazio enumerou todos os dez. Em quatro aliases globais outros erros de binding podem coexistir. Um campo adicional foi tolerado apesar de additionalProperties=false no schema de Device.

**Evidência e contraprova.** Experimentos O1, H1 e H2. [Device DTO](../../../../src/Api/Models/Device.cs#L6), [ApiController](../../../../src/Api/Controllers/DeviceController.cs#L9), [formatter](../../../../src/Api/Program.cs#L84). Contraprova de generalização: métricas anotam doze propriedades required em dois modelos. O segundo gerador anuncia 26 corpos obrigatórios.

**Divergência e impacto.** Clientes gerados podem produzir requisições destinadas a falhar antes da execução. Regenerar o mesmo Swagger reproduz essa omissão.

**Causa e relação EKOM.** G, mecanismo de geração e metadados, confirmado. C, tradução incompleta dos detalhes na referência normativa/informativa local. A reconciliação satisfez parte de KM-007 ao registrar a diferença geral. O contrato de consolidação não exigia ensaio HTTP.

**Tratamento proposto.** Priorizar CreateDevice e tornar a exigência observável no contrato publicado. Distinguir regras comuns de regras especiais por ação.

**Pendente.** Constraints finais do banco e efeito de intermediários sobre 400/415.

## AUD-DEV-002 — O exemplo público de criação é rejeitado antes da persistência

**DRIFT · TEST · Alta · Confirmado** · POST /api/v1/devices no IoTSmartSysDocs.

**Esperado.** Um exemplo de cadastro ligado ao sucesso deve ao menos passar na validação de entrada.

**Documentado.** O guia em HEAD apresenta cinco propriedades no exemplo curl. A página de referência em HEAD foi cautelosa sobre required. O rascunho local reutiliza o mesmo exemplo e informa explicitamente sua natureza ilustrativa.

**Implementado ou observado.** O payload exato do guia retornou 400 por description, state, mac_address, ip_address, protocol e platform. Um payload sintético com os dez campos atingiu o repositório e produziu 201 no ensaio com duplo.

**Evidência e contraprova.** IoTSmartSysDocs@66307f2, docs/applications/devices-api/guides/devices.md:28–45, reports/DEVICE-API-DOCUMENTATION-DELIVERY.md:147–175. Experimentos H1/H2. A ausência de banco não explica o resultado do exemplo, que falha nos dois hosts.

**Divergência e impacto.** Um integrador que segue o exemplo não chega à criação. A ausência de descrições na origem dificultou a redação, mas a escolha do exemplo ocorreu no portal.

**Causa e relação EKOM.** G no processo editorial do destino, C na origem pelo conhecimento incompleto. O escopo público autorizou exemplos sintéticos, sem alegação de validação. Nenhuma infração à EKOM pelo portal, cuja governança é própria.

**Tratamento proposto.** Fornecer na origem um exemplo de entrada verificado no binding e identificar separadamente a evidência de persistência. Reconciliar o guia pelo processo do portal.

**Pendente.** Resultado de criação bem-sucedida numa instalação com o schema real.

## AUD-DEV-003 — Os campos aceitos e a identidade efetiva da escrita variam por operação

**GAP · AMB · SPEC · Alta · Confirmado para os comandos emitidos** · Devices, properties, associações e histórico.

**Esperado.** Permitir distinguir campos aceitos pelo DTO, usados na decisão e incluídos na escrita, e determinar a chave do recurso afetado.

**Documentado.** devices.md:17–27 explica transação, settings e ar. properties.md:14–23 descreve upsert. groups.md:16–23 descreve associação por nome e capabilities.md:37–42 registra a passagem de id para nome no histórico. O conjunto não explicita as projeções e riscos abaixo. O portal, em guides/groups-dashboards.md:15–19, orienta enviar só o nome na associação.

**Implementado ou observado.** No cadastro, SQL usa NOW() e omite PowerOn/Active. Property embutida grava Name/Value. No upsert autônomo, descrição entra no INSERT e fica preservada no ramo de colisão. No PATCH de Device o predicado SQL usa Id numérico, e o controller o recupera do modelo após o patch. No PUT integral de Group o repository regrava vínculos. No PUT de associação o nome passa por validação, mas o INSERT usa capability.Id. No histórico POST o INSERT resolve novamente Name com LIMIT 1.

**Evidência e contraprova.** [DeviceQuery:98–130](../../../../src/Data.Repositories/Repositories/Queries/DeviceQuery.cs#L98), [DeviceController:61–71](../../../../src/Api/Controllers/DeviceController.cs#L61), PropertyQuery.UpSert, GroupRepository.UpdateAsync/AddCapabilityToGroupAsync, GroupQuery.InsertCapabilityForGroup e CapabilityQuery.InsertHistory:129–132. A SQL do PATCH de CapabilityType, por comparação, usa id preservado a partir do registro lido. SQL do Device presente desde 4f00ac3, anterior à adoção.

**Divergência e impacto.** Pode haver sucesso sem a alteração que o consumidor imaginava ou ambiguidade sobre qual recurso é atualizado. Não se demonstrou modificação de recurso indevido. O risco de histórico depende da unicidade real dos nomes.

**Causa e relação EKOM.** E no desenho legado da escrita, C no detalhamento funcional posterior. KM-002 orienta documentar regras sustentadas. Não há especificação geral de criação de Device ou unicidade aprovada.

**Tratamento proposto.** Documentar uma projeção por operação e levar a identidade da atualização e do histórico à decisão de contrato antes de prometer garantias.

**Pendente.** DDL, triggers, defaults e comportamento de nomes repetidos. A associação de plataforma embutida possui evidência específica em AUD-DEV-012.

## AUD-DEV-004 — As consultas comuns de Devices não carregam capabilities

**DRIFT · OBS · Alta · Confirmado por inspeção** · GET /api/v1/devices e GET /api/v1/devices/{device_id}.

**Esperado.** O consumidor deve saber se a coleção aninhada representa o estado do equipamento.

**Documentado.** devices.md:9 descreve carregamento de capabilities e properties. Readme.md:49 em diante ilustra capabilities preenchidas. O schema expõe a coleção.

**Implementado ou observado.** GetDevicesAsync e GetDeviceAsync usam GetAllDevices, com JOIN apenas de properties e settings. O multimapper agrega somente esses dois. Core inicia Capabilities vazia e a conversão itera essa coleção. O campo settings de saída é inicializado vazio no DTO e não é copiado do Core.

**Evidência e contraprova.** [Repositório:11–58](../../../../src/Data.Repositories/Repositories/DeviceRepository.cs#L11), [SQL:7–33](../../../../src/Data.Repositories/Repositories/Queries/DeviceQuery.cs#L7), [DTO:23–42](../../../../src/Api/Models/Device.cs#L23). A SQL GetDevicesWithCapabilities existe, mas estes dois caminhos não a selecionam. O endpoint próprio de capabilities permanece uma fonte distinta.

**Divergência e impacto.** No H2, o GET do modelo sintético serializou settings como objeto vazio; isto distingue presença do campo de carregamento do conteúdo. Uma lista vazia no GET pode ser interpretada como equipamento sem capacidades, inclusive logo após o POST com filhos.

**Causa e relação EKOM.** A/C na afirmação da consolidação 1caf490 frente a KM-002/005/007. E na apresentação anterior do README. A presença de uma query alternativa ajuda a explicar a confusão, sem provar motivação.

**Tratamento proposto.** Descrever a projeção dos GETs e a fonte de consulta dos filhos. Submeter eventual requisito de expansão à Autoria.

**Pendente.** Leitura integrada com fixture de device que possua capabilities e confirmação de regra pretendida.

## AUD-DEV-005 — O prefixo aninhado não basta para determinar o escopo da capability

**CONTR · DRIFT · SEC · Alta · Confirmado por inspeção** · Aliases de Capability, consultas Tiny e Smart Home.

**Esperado.** Indicar de onde vem a chave efetiva de filtro e distinguir seleção funcional de autorização.

**Documentado.** capabilities.md:11–15 adverte sobre o duplo prefixo, mas a linha da coleção aninhada afirma escopo por device. D-01 já identifica quatro ações sem segmento requerido. smart-home-capabilities.md:16–18 adverte corretamente sobre lookup só por UID.

**Implementado ou observado.** GetAllCapabilities passa somente CapabilityFind de query ao repository. O filtro por device é condicional. Tiny usa o mesmo DTO e chave de cache que não inclui todos os filtros. Outras ações usam exclusivamente id, UID ou referência. SmartHome injeta o parâmetro na coleção, cujo predicado usa sh.Name, e ignora o pai na ação por UID.

**Evidência e contraprova.** [CapabilityController:8–38](../../../../src/Api/Controllers/CapabilityController.cs#L8), [builder:148–197](../../../../src/Data.Repositories/Repositories/Builders/FindCapabilityQueryBuilder.cs#L148), [SmartHomeCapabilityController:13–35](../../../../src/Api/Controllers/SmartHomeCapabilityController.cs#L13). Busca por nome no prefixo aninhado efetivamente usa device_id, contraexemplo. Revisões e785c84, 207f633 e 1caf490.

**Divergência e impacto.** Uma coleção pode ter alcance diferente do que o integrador deduz do caminho. A documentação não sustenta isolamento por usuário ou incidente de acesso.

**Causa e relação EKOM.** A/C relativamente a KM-002/007 na afirmação recente de escopo, E nos aliases e filtros anteriores. A proteção no ingresso continua F.

**Tratamento proposto.** Explicitar escopo por ação e por alias, documentar limites de cache e verificar com o responsável o contrato de isolamento desejado.

**Pendente.** Configuração do ingresso, efeitos com dados reais e comportamento esperado dos aliases.

## AUD-DEV-006 — Formato ou status da resposta diverge dos metadados de várias operações

**OAPI · DRIFT · Alta · Confirmado por inspeção e comparação dos geradores** · Capabilities, Smart Home, histórico, settings, catálogos, utilitários, acesso legado e Groups.

**Esperado.** Geradores de clientes precisam de cardinalidade, mídia, status possíveis e headers de sucesso. GRP-API-019 exige schema efetivo para Groups.

**Documentado.** Snapshot traz array em seis consultas singulares, array em text/plain para Tiny, objeto singular em Device Settings e Smart Home coleção, e não declara 204 nos GETs de places/platforms. Os três utilitários não têm schema 200. O histórico GET anuncia 404, e o Swagger omite os headers das operações. No PATCH de Group a referência dos itens aponta a Operation genérica.

**Implementado ou observado.** As buscas retornam objeto, Tiny pode produzir arquivo de texto, Device Settings alterna coleção e dicionário, Smart Home lista ou retorna 204, e histórico retorna coleção vazia com 200. Program.cs registra configuração de Group específica, mas o Swagger efetivo continua genérico. Ações de OAuth leem query, formulário ou headers manualmente e retornam respostas próprias, incluindo redirecionamento.

**Evidência e contraprova.** Ver ações e referências por linha na matriz, Program.cs:15–65,115–130, GRP-API-010/019 e AC-007. O1 reproduziu o Swagger. D-02/03/04/08/09/10/16 já descrevem vários casos. As referências funcionais corrigem parte substancial deles, em especial Tiny, settings e catálogos.

**Divergência e impacto.** Um cliente gerado pode falhar ao desserializar respostas válidas, não reconhecer ausência de conteúdo ou aceitar documentação de patch mais ampla que o servidor.

**Causa e relação EKOM.** G por metadados de ação incompatíveis e inputs manuais. A no resultado específico de GRP-API-019/AC-007, sem invalidar a decisão humana de encerramento nem atribuir a execução a uma pessoa.

**Tratamento proposto.** Priorizar cardinalidade, formas variantes e headers; verificar o artefato efetivamente consumido contra exemplos representativos.

**Pendente.** Qual gerador é contratado para distribuição futura e quais aliases/erros terão suporte normativo.

## AUD-DEV-007 — A referência de settings confunde leitura efetiva e falha de atualização

**CONTR · DRIFT · Média · Confirmado por inspeção** · GET de settings por device e PUTs globais.

**Esperado.** Identificar se a leitura mostra overrides ou valores efetivos e o resultado quando o UPDATE não encontra linha.

**Documentado.** settings.md:9,18–20 reconhece a view e KeyNotFoundException, mas descreve a leitura como específica e associa 204 à atualização sem distinguir linha ausente. O Swagger anuncia 404 nos dois PUTs globais.

**Implementado ou observado.** DeviceSettingsRepository usa v_DeviceEffectiveSettings, ordenada por Name. O reset opera somente em DeviceSettings. Em ambos os PUTs globais, zero linhas lança KeyNotFoundException; o middleware geral não trata esse tipo e usa 500. No lote há rollback. Na escrita por device, array vazio gera 400 omitido nos metadados.

**Evidência e contraprova.** [DeviceSettingsQuery:19–38](../../../../src/Data.Repositories/Repositories/Queries/DeviceSettingsQuery.cs#L19), [SettingsRepository:57–119](../../../../src/Data.Repositories/Repositories/SettingsRepository.cs#L57), ExceptionHandler.cs:29–112. Dossiê §6 identifica a view corretamente, e DSR-005/006 distingue o alcance da remoção. O guia público settings-properties.md:5–13 fala em substituir settings, mas a rotina não apaga as chaves omitidas.

**Divergência e impacto.** Um cliente pode considerar todo valor lido removível por reset ou usar 404/204 para classificar incorretamente uma falha.

**Causa e relação EKOM.** C/A quanto à precisão de KM-002, F quanto à composição real da view. Zero linhas é o critério do código, cujo significado pode depender da configuração de contagem do driver.

**Tratamento proposto.** Explicitar a view na operação e rastrear exceção até o HTTP. Vincular à fonte autoritativa de precedência quando disponível.

**Pendente.** DDL da view, presença de linhas herdadas e configuração do banco real.

## AUD-DEV-008 — A criação de property pode falhar ao gerar Location depois da escrita

**DRIFT · TEST · Alta · Confirmado no MVC isolado** · POST /api/v1/devices/{device_id}/properties.

**Esperado.** O consumidor precisa distinguir falha anterior à escrita de falha ao construir a resposta.

**Documentado.** properties.md:16–22 descreve inserção transacional seguida de 201. No snapshot há resposta 201. A operação individual usa id interno que o DTO não expõe.

**Implementado ou observado.** Após AddAsync, a ação produz CreatedAtAction(nameof(GetAllAsync), ...). No host com MVC e doubles, o repositório concluiu, a formatação da resposta falhou com “No route matches the supplied values” e o cliente recebeu 500 sem Location. No repositório concreto o commit ocorre antes do retorno.

**Evidência e contraprova.** [Controller:43–66](../../../../src/Api/Controllers/PropertiesController.cs#L43), [PropertyRepository:11–45](../../../../src/Data.Repositories/Repositories/PropertyRepository.cs#L11), PropertyQuery.AddForDevice. O SQL seleciona LAST_INSERT_ID(), mas o repositório chama ExecuteAsync e não atribui a chave à entidade. Controle H2 no relatório. O teste positivo CreateDevice retornou 201 sob as mesmas convenções MVC.

**Divergência e impacto.** Quem interpretar o 500 como ausência de escrita pode repetir uma inserção já confirmada. Persistência dupla não foi observada.

**Causa e relação EKOM.** E no caminho legado (7e9f7db), A/C na descrição de sucesso não qualificada na consolidação. KM-002/006 requerem separação de implementação e observação. O teste isola a falha de resposta.

**Tratamento proposto.** Submeter o contrato de sucesso e recuperação à decisão humana, validando separadamente persistência e geração de URL.

**Pendente.** Confirmação integrada com uma tabela isolada e a regra de unicidade efetiva.

## AUD-DEV-009 — Falta uma fonte operacional para a admissão pelo gateway e os erros compartilhados

**SEC · AMB · Alta · Parcialmente confirmado** · Integrações externas em todas as rotas.

**Esperado.** O consumidor precisa localizar quem define ingresso, credencial e permissões, e distinguir validação, regras de domínio e infraestrutura ao interpretar falhas.

**Documentado.** Dossiê §8 registra CORS e política externa incerta. Acesso legado:8–28 documenta seus próprios mecanismos. Dashboard §2 declara contexto global entre clientes admitidos. Swagger não declara securitySchemes, security, servidores nem headers. O dossiê generaliza identificador de rastreamento a cada erro tratado.

**Implementado ou observado.** Não se identificou autorização global, policies, roles ou scopes aplicados aos controllers de negócio no bootstrap. O mecanismo legado trata as próprias três ações. ExceptionHandler escreve texto simples com media type JSON e X-Trace-Id no ramo geral; há ramo próprio de conflito do ar, filtro e envelope específico do Dashboard e erros MVC problem+json. O runtime local confirmou validação 400 e dois formatos de erro na criação.

**Evidência e contraprova.** [Program.cs:84–113](../../../../src/Api/Program.cs#L84), [ExceptionHandler:16–20,104–113](../../../../src/Api/Middlewares/ExceptionHandler.cs#L104), DashboardExceptionHandler.Write, OAuth/OAuthController.Me. IoTSmartSysDocs, reports/DEVICE-API-DOCUMENTATION-INVESTIGATION.md:47–56,95–106, separa gateway legado e APISIX. CORS e uma resposta loopback não demonstram acesso público.

**Divergência e impacto.** Falta informação para obter admissão de forma autônoma ou escrever um parser único de falhas. Não há evidência de exposição pública, incidente ou ausência de proteção no APISIX.

**Causa e relação EKOM.** F para a proteção implantada, G para fronteira entre componentes e representação dos erros, C para a generalização sobre trace. KM-008 exige preservar a incerteza e DA-03 restringia a investigação anterior.

**Tratamento proposto.** Registrar a autoridade operacional por ambiente e consolidar exemplos sanitizados dos caminhos de erro. Solicitar somente evidência de política sem valores de credenciais.

**Pendente.** Configuração efetiva e versionada de rotas, plugins, políticas, TLS, consumidores e respostas do APISIX.

## AUD-DEV-010 — Fontes de entrada e estados correntes enviam sinais incompatíveis

**OBS · TRACE · GOV · CONTR · Média · Confirmado** · Navegação, ciclo de vida e descoberta.

**Esperado.** Encontrar o artefato disponível, o estado da revisão e uma rota de partida sem resolver informações incompatíveis.

**Documentado.** O dossiê §5 localiza o snapshot e §11 ainda o declara ausente. AGENTS.md aponta EKOM-CHANGELOG.md e docs/adr/, enquanto o histórico versionado usa EKM-CHANGELOG.md e não há diretório de ADR local. A especificação do ar 0.2 traz Draft e o mapa/changelog indicam In Progress. O README usa /v1/devices e afirma cascade geral.

**Implementado ou observado.** O snapshot está versionado desde eebc4a9. O build e a implementação do ar constam em da7f70f, sem validação final. A rota registrada é /api/v1/devices. Os scripts parciais sustentam algumas FKs, sem demonstrar cascade geral. As etapas de reset também conservam estados históricos e campos de fechamento ainda pendentes.

**Evidência e contraprova.** [Dossiê:103–113,213–214](../../../specs/SYSTEM-DOSSIER.md#L213), [ar:6–8](../../../specs/AIR-CONDITIONER-STATE.md#L6), [changelog:70–79](../../../rfc/EKM-CHANGELOG.md#L70), mapa:93, AGENTS.md em 43fbdc0; Readme.md:4–13 e src/Api/SmartHome-Api.http:4. Registro EKOM-ADOPTION de 23/11 já reconhece caminhos ausentes. Contraprova: CapabilityType 0.2 explicita Amends, substituição e encerramento.

**Divergência e impacto.** Um leitor pode descartar um contrato disponível, tomar o estágio errado como vigente ou usar a rota legada. Não há garantia de rewrite externo.

**Causa e relação EKOM.** C confirmado na consolidação documental, E nos exemplos legados; A na consistência do dossiê (KM-AC-006). A inexistência de ADR local por si só não demonstra omissão de decisão arquitetural obrigatória.

**Tratamento proposto.** Reconciliar as afirmações de vigência e usar links relativos para a fonte efetiva. Preservar relatórios históricos e a decisão humana de cada versão.

**Pendente.** Estado normativo a prevalecer na discrepância do ar e a política real de cascade.

## AUD-DEV-011 — A evidência automatizada identificada concentra-se em Dashboard

**TEST · TRACE · Média · Confirmado no universo pesquisado** · Cobertura de comportamento transversal.

**Esperado.** Localizar a evidência com cenário, ambiente e resultado para distinguir regra pretendida, build, execução e aceite.

**Documentado.** O mapa registra suíte Dashboard compilada e lacuna EKM-GAP-0003. Os relatórios de Groups e CapabilityType registram validação humana e build; o reset distingue Required de Not Executed. Nenhuma dessas declarações fornece automaticamente transcrições de HTTP/SQL.

**Implementado ou observado.** O runner cobre IT-01–09 e CLEAN, com condições para gauge e limpeza, e verifica status, conteúdo, permanência e concorrência seletivamente. Dois scripts k6 verificam apenas status em Capabilities. O de escrita usa o alias global cujo segmento está ausente. Nos demais pares não foi localizada asserção automatizada vigente no universo pesquisado.

**Evidência e contraprova.** [Suite:20–28,235–283,295–340](../../../../tests/Api.IntegrationTests/DashboardIntegrationSuite.cs#L20), tests/loads/use-cases-gets.js:11–13 e use-cases-update-value.js:11–21, docs/specs/DASHBOARD-API-INTEGRATION-TESTS.md:34–55, EKM-CHANGELOG.md:482–511. A aposentada ficou excluída. Os ensaios locais desta auditoria complementam a evidência e não alteram o inventário de regressão existente.

**Divergência e impacto.** Um build verde e uma aceitação humana não permitem reconstruir todas as condições de integração. O integrador continua sem exemplo de persistência confirmado para a criação.

**Causa e relação EKOM.** F para evidência externa não disponibilizada, E/G para alcance de suíte e escolha de validações. EKOM §6 prevê validação proporcional e não obriga suíte universal. Não é falha de adoção comprovada.

**Tratamento proposto.** Priorizar evidência dos fluxos de alto impacto em ambiente isolado quando houver especificação e decisão humana. Associar asserção e resultado ao comportamento relevante.

**Pendente.** Resultados de execuções externas, política de retenção de evidências e disponibilidade de fixtures.

## AUD-DEV-012 — O cadastro com plataforma embutida passa um objeto incompatível ao Dapper

**DRIFT · Alta · Confirmado para o binding** · POST /api/v1/devices, capabilities com platforms.

**Esperado.** A referência deve dizer se é possível concluir o cadastro com os relacionamentos anunciados.

**Documentado.** devices.md:23 afirma que as plataformas indicadas são associadas antes do commit. O schema expõe elementos com platform e referenceId. O texto não identifica uma restrição da biblioteca de acesso aos dados.

**Implementado ou observado.** DeviceRepository.CreateAsync passa a instância CapabilityPlatform como platformName a uma consulta que compara Platforms.Name. O caminho não extrai o nome, e o INSERT não inclui ReferenceId. No ensaio com a versão do Dapper do projeto, o objeto gerou NotSupportedException ao preparar parâmetros. O controle com string ultrapassou essa etapa.

**Evidência e contraprova.** [Repository:112–130](../../../../src/Data.Repositories/Repositories/DeviceRepository.cs#L112), [query:113–116](../../../../src/Data.Repositories/Repositories/Queries/DeviceQuery.cs#L113), [DTO:76–85](../../../../src/Api/Models/Capability.cs#L76), Core/Entities/CapabilityPlatform.cs, projeto Data.Repositories e experimento P1. Nenhum registro de TypeHandler correspondente foi localizado em src. A lista vazia evita esse ramo.

**Divergência e impacto.** Um recurso anunciado como suportado pode impedir o commit do cadastro inteiro. O erro do binder foi observado isoladamente. O rollback é evidência de código, sem banco exercitado.

**Causa e relação EKOM.** E na passagem do objeto já presente em 6ed1f2e de janeiro de 2025. A/C ao afirmar suporte em 1caf490, diante de KM-002/006. G na incompatibilidade concreta de tipo.

**Tratamento proposto.** Qualificar a descrição de suporte e decidir o contrato de plataformas embutidas, incluindo ReferenceId. Verificar em fixture controlada antes de publicar garantia.

**Pendente.** Constatação de rollback de todas as linhas num MySQL representativo e regra de catálogo pretendida.

## AUD-DEV-013 — A leitura por id e os filtros de properties não produzem a consulta documentada

**DRIFT · Alta · Confirmado para o SQL construído** · GETs de properties.

**Esperado.** O comando enviado deve permitir selecionar o item ou combinar cada filtro com o device.

**Documentado.** properties.md:14–16 apresenta a busca individual por device/id e filtros name, value e description. Os metadados anunciam respostas normais para essas leituras.

**Implementado ou observado.** O builder inicia pela consulta completa. WithId substitui todo o SELECT por um predicado isolado. Os filtros da coleção concatenam predicados sem operador de conjunção. Com filtros vazios a base continua íntegra.

**Evidência e contraprova.** [Builder:30–54,66–89](../../../../src/Data.Repositories/Repositories/Builders/PropertyQueryBuilder.cs#L30), PropertyQuery.GetAll:12–21, PropertyRepository.GetByDeviceAsync/GetByIdAsync. P2 instanciou o builder compilado e leu CommandText antes de qualquer envio. O SQL sem filtro mantém SELECT e chave do device.

**Divergência e impacto.** As afirmações sobre 200/404 individual e filtros funcionais não são sustentadas por comandos executáveis no caminho auditado. O erro HTTP da instalação não foi observado.

**Causa e relação EKOM.** E no builder de 7e9f7db, A/C no resumo de novembro que descreve suporte sem seguir a query, em confronto com KM-002/005.

**Tratamento proposto.** Retificar o estado de suporte após a decisão do Arquiteto e confrontar os dois fluxos com fixtures, incluindo coleção sem filtro como controle.

**Pendente.** Resposta efetiva da instalação em caso de erro e contrato normativo de filtros.
