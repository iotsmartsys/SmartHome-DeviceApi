# Auditoria comparativa independente do conhecimento de Properties

**Classe da fonte:** Relatório

**Papel:** Engenheiro Revisor

**Especificação:** Ordem investigativa de auditoria comparativa independente de Properties, 24/11/2026

**Revisão confrontada:** `1f946ddcd553dbaa13ee8ad5fc9d907ebf7893c7`

**Estado:** Parecer emitido para decisão do Arquiteto

## 1. Parecer executivo

**A e B são satisfatórios com ressalvas. B é materialmente superior para avaliação de impacto e diagnóstico entre recursos.** Os dois sustentam a integração básica das seis operações e explicam o risco de repetir um POST que respondeu 500 depois do commit. A permite recuperar esse percurso com menos leitura. B explica qual linha pode mudar numa colisão entre dispositivos, como pais com o mesmo identificador podem compartilhar uma projeção e por que uma falha em Settings pode impedir um DELETE de Property.

Há limites importantes em ambos. O cache participa até da seleção do valor de um filtro repetido. Um GET pode devolver 304 ou reutilizar uma representação para outra grafia do identificador. Um PATCH aceito sobre a coleção do pai pode atualizar o horário de atividade do dispositivo enquanto as propriedades persistidas continuam iguais. Essas diferenças afetam o que o cliente vê e o que um operador pode inferir do resultado.

## 2. Escopo, revisões e evidências

| Objeto Git | Identidade verificada |
|---|---|
| Implementação comum e base direta da worktree independente | `1f946ddcd553dbaa13ee8ad5fc9d907ebf7893c7` |
| A, commit e blob de `docs/knowledge/properties.md` | `eeef0af1f5e5aaced3d39dc436e773f3255d41c0` · `f0d5812130226c5370dd4fa22a2ef38f2f0764b4` |
| B, commit e blob do mesmo caminho | `d131a703c5f1094d12ee491a48a844e2d8928554` · `a452e15d65091e2b28b62c884455c888484ad4dc` |
| Branch de entrega | `audit/properties-knowledge-comparison` |

A resolução de cada commit e de seu objeto de caminho, o merge-base de cada commit com a base e o diff de cada árvore completa excluindo somente o documento confirmaram origem e equivalência. A base também é ancestral da main local. Os dois textos foram extraídos por `git show commit:caminho`. Os números A e B abaixo são linhas dos blobs originais. Aplicam-se AGENTS.md, regras comuns, perfil Revisor, contrato SHD-ENGINEERING-001 e Readiness no seu escopo de capabilities. O dossiê e o mapa inventariam Properties. A ordem autoriza investigação documental, ensaio isolado e entrega deste relatório.

**Ordem e custódia.** Fixei perguntas, cenários, rubrica e medida seletiva. Li A e congelei suas respostas antes de abrir B. Repeti o procedimento para B antes de acessar a implementação. Em seguida busquei rotas, tabelas, usos do agregado e consumidores em toda a árvore de código, li os percursos ativos e distingui constantes sem chamador. Registrei o referencial, os confrontos individuais e só então consolidei a comparação. O mesmo leitor viu A primeiro. As linhas de sustentação limitam, mas não eliminam, esse viés. As menções a ensaios nos próprios textos fazem parte da evidência publicada. Não houve identificação de método de autoria nem consulta a relatórios de intervenções anteriores.

Registros locais preservados em `/tmp/auditoria-properties.QzCOyc`. Os arquivos `criterios`, `autonomia-A` e `autonomia-B` foram fechados às 23h18min02s, 23h18min15s e 23h18min31s de 24/11/2026, respectivamente, em America/Bahia. Prefixos SHA-256 respectivos `8feb02a47d4b`, `6e226a7cb90f` e `b60faca697d9`. O diretório contém também os blobs, `fatos`, `confronto-A`, `confronto-B`, o laboratório e sua saída final `ensaio-final.log` (SHA-256 `9042234c551d5e2d80d57cc1897ed711ecb2ebb56e56cef3e4f23243e230c94e`). Este relatório contém os resultados necessários para interpretar o parecer sem depender da retenção de /tmp.

**Graus de evidência.** Inspeção da revisão para contratos internos, chamadas e SQL. Execução do builder real para texto de comando. HTTP em TestServer 9.0.1, ASP.NET Core 9.0.9, SDK 9.0.305, MVC NewtonsoftJson 9.0.1, Newtonsoft.Json 13.0.3. A partir de um arquivo Git da base, o host temporário carregou os controllers reais, o middleware geral e as opções de MVC/cache do bootstrap. Os dois repositórios foram substituídos. O substituto de consulta registrou os valores recebidos e, no ensaio discriminante, lançou uma exceção sintética para critério efetivo. O GET unitário substituído devolveu null. A mensagem genérica dessa exceção e o 404 desse substituto não descrevem o MySQL. DI, watchdog, middleware exclusivo de Dashboard, Swarm, Dapper 2.1.35 e MySqlConnector 2.4.0 foram avaliados por inspeção. Sem MySQL, dispositivos, gateway ou suíte descontinuada. As observações históricas dos autores não foram usadas como execução independente.

## 3. Autonomia documental

Perguntas congeladas e respostas recuperáveis antes da inspeção. D = determinável, P = parcial, E = condicionada a evidência externa legítima. Indeterminável, ambígua e contraditória também eram opções. A falta de cobertura de um tema foi registrada como parcial quando havia resposta para parte da pergunta.

| Pergunta | A, resposta e passagem | B, resposta e passagem |
|---|---|---|
| Q1. Que recurso é este e como endereçá-lo? | D. Seis rotas, dois IDs do pai, ID da filha ausente na saída, valor textual livre, A3–29 | D. Mesmo modelo e separação entre significado do nome e efeito físico, B3–31 |
| Q2. Como formar uma escrita e distinguir validação de transformação? | D. Objeto, campos anuláveis, binding antes da action, confirmação e retorno distintos, A33–39,85–154 | D. Mesmas decisões e formas inválidas adicionais, B33–37,85–152 |
| Q3. Como distinguir falha de consulta, vazio e ID inválido? | D para os cenários definidos. Critério efetivo e consulta por ID geram SQL inválido, segmentos inválidos param antes, A41–83 | D para os mesmos cenários. Inclui prefixos mistos e uma regra para repetição, B43–83,185–199 |
| Q4. O que persistiu e por que o GET pode divergir? | D/E. Colisão preserva descrição, commit pode preceder 500, GET em cache, catálogo externo, A85–90,116–138,179–199 | D/E. Mesmas causas e exemplo de PUT entre GETs, B89–118,170–179,201–203 |
| Q5. Onde o efeito aparece fora do endpoint direto? | P. Criação, projeção e exclusão do pai, sem caracterizar por completo efeitos operacionais, A171–177 | P. Agregação, órfãos e distinção de Settings MQTT, sem esgotar efeitos do PATCH do pai, B174–181 |
| Q6. Quais garantias existem sob concorrência, falha e controle de acesso? | D/E. Transações locais, pré-leituras separadas, ausência de CAS e autenticação no app, gateway externo, A195–217 | D/E. Mesmas fronteiras e efeito do cancelamento após commit, B201–205 |

**Exemplos construídos na leitura cega.** Nas seis linhas, a base é `/api/v1/devices/estufa/properties`, os corpos são objetos JSON enviados como `application/json`. A coluna de previsão é comum porque os dois registros independentes chegaram à mesma resposta. O confronto posterior está na última coluna.

| Requisição | Previsão original e alternativa | A / B | Confronto |
|---|---|---|---|
| GET da base | Cache frio, uma linha sensor=8 sem descrição → 200 e `[{"name":"sensor","value":"8"}]`. Zero linhas → 204 | A39–62 / B37–63 | Correta com essas condições |
| GET da base + `/31` | MySQL acessível e rejeição do fragmento → 500 textual. `/abc` → 400 do MVC | A64–83 / B65–83 | Correta. Builder real e middleware sustentam o primeiro ramo, ensaio HTTP sustenta o segundo |
| POST da base, `{"name":"sensor","value":"8"}` | Pai presente e INSERT aceito → 500 depois do commit. Pai ausente na pré-leitura → 404 | A85–112 / B85–110 | Correta. Ordem do commit por inspeção, falha após retorno do substituto observada |
| PUT da base, `{"name":"sensor","value":"9","description":"nova"}` | 204 se aceito. Em colisão mantém descrição anterior. Sem pai depende do DDL | A114–134 / B112–133 | Corretamente condicionada |
| PATCH da base, `{"name":"sensor"}` | 204 se o banco aceitar valor nulo. Array de operações → 400 | A136–154 / B135–152 | Corretamente condicionada |
| DELETE da base + `/31` | Pai presente → 204 mesmo repetindo com zero linhas. Pai ausente na primeira leitura → 404 | A156–169 / B154–166 | Correta |

**Percursos incorporados.** Nos dois registros, POST de `/api/v1/devices` com o fragmento `"properties":[{"name":"sensor","value":"8","description":"nova"}]` cria a filha sem a descrição submetida se a transação terminar. Ambos permitem prever a resposta sem filhos, B explicita 201. GET da lista e GET de `/estufa` devolvem os filhos sem ID, inclusive coleção vazia num pai existente. B explicita 200 para lista vazia e 404 para pai ausente. No PATCH do pai, A permite prever escrita só em Devices, B também prevê 204 se a alteração em memória for aceita. DELETE do pai depende da FK em ambos, B prevê invisibilidade dos órfãos. São previsões corretas nos limites indicados. A e B isoladamente não especificam o corpo completo obrigatório de criação de Device. Os seis casos diretos têm quatro previsões corretas e duas corretamente condicionadas por documento. Nenhum erro de leitura do auditor foi identificado nesses seis. As descobertas posteriores sobre variantes da query e do PATCH não foram retroatribuídas às previsões.

## 4. Referencial factual

| Percurso ativo na revisão comum | Resultado verificável e condição |
|---|---|
| PropertiesController → DTO Property → Core → IPropertyRepository | Três strings anuláveis copiadas sem normalização. `!` suprime aviso do compilador. `Validate` existe, o filtro próprio está em Dashboard. ID do corpo não identifica a filha. Os IDs de rota inteiros podem ser zero ou negativos. Nome externo do pai resolve para FK numérica. A projeção remove ID e omite campos nulos. |
| MVC → formatter → action | Objeto vazio, nomes com caixa variada, escalares convertíveis e espaços podem chegar ao repositório. JSON truncado, literal null, membro objeto para string ou array de patch falharam com 400. Texto simples deu 415. Objeto com mídia de JSON Patch alcançou o upsert. Binding inválido produz Problem Details, distinto do texto do middleware. |
| GetAllAsync → Criteria → PropertyQueryBuilder.Build → QueryAsync | INNER JOIN por FK, filtro base por identificador do pai. Sem critério efetivo retorna todas as linhas ou 204, sem verificar pai, ordenar ou paginar. Predicados de nome, valor e descrição são anexados sem AND. O último usa + na expressão LIKE. O objeto de comando real reproduziu exatamente esses fragmentos. |
| GetByIdAsync → WithId | O comando real gerado foi somente `dp.id = @id`. O parâmetro de dispositivo ainda existe, sem uso. O 200/404 da action depende de uma consulta concluída. O MySQL que rejeite a sintaxe leva ao 500 de banco. |
| AddAsync → pré-leitura do agregado → AddForDevice → CreatedAtAction | Pai ausente dá 404 do MVC. Lookup escalar zero na transação dá 404 textual. INSERT inclui descrição. ExecuteAsync descarta LAST_INSERT_ID, a entidade permanece com ID zero. Commit precede geração de URL. O nome GetAllAsync diverge do nome registrado pelo MVC. Observado 500 textual depois da chamada de inclusão. Retry pode duplicar ou retornar 409 sob restrição única. |
| PUT/PATCH → CreateOrUpdateAsync → UpSert | Mesma action e transação. Subquery zero linhas fornece NULL, várias linhas podem gerar erro. INSERT inclui descrição e NOW(). Qualquer chave primária/única aplicável pode desviar para atualização de Name, Value e UpdatedAt. A FK e a descrição antigas sobrevivem. Sem choque, nova linha. Sem verificação de pai, CAS, merge do PATCH ou resposta distinguindo os ramos. |
| RemoveAsync → pré-leitura → lookup escalar → RemoveForDevice | Pai presente, predicado da linha usa FK e ID. Zero afetadas também confirma 204. Se o pai desaparecer, lookup zero não dispara a proteção existente no POST. Dois pais físicos com mesmo ID externo podem produzir primeiro escalar sem ordenação. |
| DeviceController → operadores Device → DeviceRepository → DeviceQuery | Criação insere pai, capabilities, properties sem descrição, vínculos de plataforma, na mesma transação. Itens novos ID zero podem coexistir no lote. Nas leituras, LEFT JOIN com settings multiplica linhas e AddProperty deduplica por ID no agregado agrupado pelo ID externo. Filhos de nomes iguais e IDs distintos sobrevivem. A busca por settings efetivos filtra pais. HasSettingsMqtt usa Settings e não lê Properties. PATCH regrava colunas do pai. DELETE emite somente DELETE no pai. |
| Transações, DI e diagnóstico | Repositórios scoped e conexões transientes. Escritas usam transação local, abertura/início antes do try, rollback e close. Lookups escalares do pai e inserção incorporada da filha usam overload sem token. Logs podem anteceder commit. Sem retentativa coordenada. Códigos 1062→409, 1451/1452→400, 1205/1213/1040/2006/2013→503. 1205/2006/2013 e TimeoutException (503) pedem parada. Outro MySQL→500 de banco. Outro erro comum→500 genérico. Se a resposta começou, o handler geral não a substitui. |
| Política HTTP e operação | Cache base de 2 s antes dos controllers, sem invalidação nas escritas. CORS aberto. Sem verificação de proprietário na aplicação. Swarm declara prefixo, remoção, middleware externo e política de reinício. Watchdog testa SELECT 1 e sai após três falhas consecutivas. O DDL SQL Server ilustra FK, comprimentos e IDENTITY. O catálogo MySQL de Properties não está integralmente no repositório. |

As fontes locais centrais permitem reconstruir esses trajetos. [Controller de Properties](../../../../src/Api/Controllers/PropertiesController.cs) · [Repositório direto](../../../../src/Data.Repositories/Repositories/PropertyRepository.cs) · [Construtor da consulta](../../../../src/Data.Repositories/Repositories/Builders/PropertyQueryBuilder.cs) · [SQL direto](../../../../src/Data.Repositories/Repositories/Queries/PropertyQuery.cs) · [Controller de Devices](../../../../src/Api/Controllers/DeviceController.cs) · [Repositório do agregado](../../../../src/Data.Repositories/Repositories/DeviceRepository.cs) · [SQL do pai](../../../../src/Data.Repositories/Repositories/Queries/DeviceQuery.cs) · [Bootstrap HTTP](../../../../src/Api/Program.cs) · [Middleware geral](../../../../src/Api/Middlewares/ExceptionHandler.cs)

## 5. Fidelidade e completude

O confronto abaixo parte das afirmações para o código. Cada linha é uma família de afirmações semanticamente relacionadas. C indica núcleo correto, P indica que uma afirmação da família requer condição ou qualificação. O confronto inverso, incluindo conhecimentos que ninguém afirmou, está na seção 6. Uma omissão não transforma automaticamente em erro uma frase verdadeira.

| Família e fonte técnica discriminante | A | B |
|---|---|---|
| 1. Finalidade, ausência de efeito físico, seis operações. Controller, SQL de escrita e exemplo do README | C 3–16 | C 3–18 |
| 2. Relação entre IDs, nulabilidade, conversão e saída. Operadores nas duas classes Property | C 18–29 | C 20–31 |
| 3. Corpo, mídia, campos ignorados, binding e Validate. Atributos das actions, filtro e ensaio MVC | C 31–39 | C 33–37 |
| 4. GET de coleção, escopo do filtro. Comando real e ensaio de query | C 41–62 para critérios já vinculados. Omissão S1 | P 43–63. O primeiro valor recebido pelo binder pode diferir do primeiro enviado |
| 5. GET unitário. WithId substitui o comando, 400 antecede a action | C 64–83 | C 65–83 |
| 6. POST, janela entre leituras, ID descartado e formatação após inclusão. AddAsync, GetIdDevice, MVC | C 85–112 | C 85–110 |
| 7. PUT, chave implantada, colunas e repetição. UpSert e CreateOrUpdateAsync | C 114–134 | C 112–133 |
| 8. PATCH direto, NULL e mídia. Mesma action e SQL do PUT | C 136–154 | C 135–152 |
| 9. DELETE, ausência, repetição e contagem ignorada. RemoveAsync e RemoveForDevice | C 156–169 | C 154–166 |
| 10. Criação incorporada, descrição, duplicados e atomicidade. AddProperties e CreateAsync | C 171–173 | C 174–176 |
| 11. Materialização, ID e projeção. GetByCommandAsync e conversão para DTO | C 174 | C 177,181 |
| 12. PATCH e DELETE do pai, destino da escrita e FK. UpdateDevice/DeleteDevice | C 175 | C 178–179 |
| 13. Persistência, DI, DDL conhecido e lacuna MySQL. Registro scoped/transient e scripts | C 179–193 | C 168–172 |
| 14. Erros e parada. ExceptionHandler e DashboardExceptionHandler | C 201–213 | C 183–199 |
| 15. Transação, cancelamento e concorrência. Blocos try/catch/finally dos repositórios | P 197, propagação sujeita à própria limpeza | P 201, mesma condição |
| 16. Cache, watchdog, acesso e implantação. Program, watchdog e Swarm | C 199–213 para os casos descritos | C 203–205 para os casos descritos |
| 17. Alcance normativo e dos ensaios. Mapa, dossiê e Readiness | C 16,217 quanto à fronteira de evidência | C 207–211 quanto à fronteira de evidência |

**Confronto de A.** O leitor sai com um modelo correto da identidade, da assimetria entre leitura e escrita e dos desvios relativos aos metadados HTTP. A ressalva de propagação em A197 importa para correlacionar o erro de banco ao status final. Nos sentidos código→documento e dependência→consequência, A para antes de explicar o efeito de uma colisão entre pais e de um agregado que deixou de distinguir dois pais físicos. A ausência de nome único no catálogo foi corretamente reconhecida como externa. A consequência de uma chave hipotética específica já é determinável por inspeção.

**Confronto de B.** O documento acerta o resultado do POST, os comandos inválidos e o ciclo de vida incorporado, inclusive a diferença entre até onde alcança a FK e o que o cliente vê. A frase sobre o primeiro valor de query precisa de uma condição material. Uma exceção que surge durante o rollback também qualifica a afirmação de que o erro é relançado. O relato de TestHost é compatível com esta reprodução, mas o arquivo indicado não identifica a versão do host de teste histórico.

## 6. Achados comparativos

Cada ID representa uma decisão distinta do leitor. Alta muda integração, atribuição de dados ou ação operacional central. Média muda diagnóstico ou decisão sob uma condição particular. Baixa dificulta rastrear ou interpretar evidência. **Confiança alta** em todos os percursos de código e observações locais abaixo. Em S3, A1 e A2, a ocorrência em uma implantação específica permanece indeterminada. Gravidade considera o efeito caso a condição ocorra. A última coluna indica a correção documental recomendada.

### Compartilhados

| ID, natureza, severidade | Passagem e evidência técnica | Efeito sobre o leitor e correção |
|---|---|---|
| S1 · interação de binding e cache · alta | A45,199 e B61–63,203. Program:78,102 e PropertyQueryBuilder.WithCriteria. No TestServer, a mesma query com valores repetidos chegou como `a` com cache elegível e como `z` com bypass quando a ordem enviada foi z, a. Com um valor vazio e um efetivo, até uma chave fria pôde produzir critério vazio. Um hit reutilizou a resposta sem nova chamada. | Pode haver uma lista sem filtro onde se previa erro SQL. A omite a seleção de repetidos. B generaliza o primeiro valor. Distinguir ordem enviada, valor vinculado e resultado servido antes da action. |
| S2 · resposta condicional omitida · média | A9,43,199 e B11,45,203. Após 200 elegível, o middleware devolveu 304, corpo vazio e zero chamadas adicionais ao repositório com If-None-Match compatível por curinga ou If-Modified-Since posterior. | Um cliente que só trata 200/204 ignora o caminho de revalidação. Incluir condição, ausência de corpo e uso da representação que o cliente já tem. |
| S3 · identidade na chave de cache · alta | A22,199 e B24,203. Duas grafias de caixa do caminho corresponderam à mesma entrada no ensaio. O contador de chamadas ficou em um. A chave padrão do framework normaliza a caixa do caminho, independentemente da collation SQL. | Se a implantação distingue esses IDs, um 200 pode exibir a representação de outro pai lógico. Explicar a condição de identidade e o alcance por instância e TTL. A distinção efetiva entre IDs no banco continua externa. |
| S4 · efeito do PATCH no pai · média | A175 e B178. DeviceController.UpdateDevice → DeviceRepository.UpdateAsync → DeviceQuery.UpdateDevice:118–132 → GetAllDevices → Device DTO. Substituir a coleção por vazia chegou ao update e retornou 204 no ensaio. O SQL atribui LastActive=NOW() e reescreve outras colunas a partir do snapshot. | O GET posterior pode mostrar novo last_active com os mesmos filhos. Uma alteração concorrente em colunas do pai pode ser sobrescrita. Documentar o efeito até a representação e a janela entre leitura e UPDATE. |
| S5 · falhas na representação incorporada · média | A173,175 e B176,178. Model Device, conversão Property e DeviceController. Com pai válido e demais dados iguais, POST com coleção null produziu 400, com [null] produziu 500 e com [{}] chegou à criação substituída. PATCH substituindo a coleção foi aceito. Editar o valor indexado na projeção enumerável ou remover toda a coleção produziu 500 no ensaio. | A unidade da falha pode ser o campo, um elemento ou o caminho de patch. B reconhece genericamente a possibilidade de falha. Documentar os limites observados e distinguir erro antes do repositório de rejeição pelo DDL. |
| S6 · exposição em logs do pai · média | A213 e B205 descrevem logs de escrita direta. DeviceController:61 serializa o agregado Core completo antes de aplicar o PATCH. Inclui IDs, nomes, descrições e valores das Properties já carregadas, mesmo se o patch trata outro campo. | Um inventário de dados registrados que só considere Property.ToString perde essa superfície. Registrar a etapa, o conteúdo potencial e a dependência do nível/destino de log. |
| S7 · cardinalidade de diagnóstico · baixa | A174 e B177 explicam a deduplicação. DeviceRepository.GetByCommandAsync:20–45 registra devices.Count() das linhas mapeadas e devolve a lista agrupada. Um pai, duas filhas e três settings podem gerar seis linhas, duas filhas na resposta e um único pai. GetDeviceAsync ainda registra a contagem agrupada. | O log interno pode inflar o número aparente de dispositivos. Explicitar qual contagem mede linhas e qual mede pais. |
| S8 · falha secundária após erro · média | A197,201–211 e B187–201. PropertyRepository:37–47,124–134,158–168. Rollback ocorre antes de throw e Close no finally. Se a limpeza lançar, outra exceção pode chegar ao handler. Os logs de operação bem-sucedida no repositório também antecedem o commit. | Código 1062 na falha original não basta para prometer 409 se a limpeza falhar. Qualificar o mapeamento pela exceção que efetivamente sai e distinguir log de comando de confirmação. |
| S9 · rastreabilidade de defaults · baixa | A33–39,90,199,217 e B33–37,91,203,211. Program e Api.csproj mostram configuração local, sem implementar seleção de valores, supressão do sufixo Async ou elegibilidade padrão do cache. Os relatos de execução identificam condições, sem fonte da biblioteca nessas passagens. | Para reconstruir o resultado em outra versão, indicar classe, versão e configuração relevantes. Em B distinguir também a versão do TestHost, ausente de Api.csproj. |

O ordenamento ordinal dos valores na geração da chave e a normalização do caminho estão no código da versão exercitada. A supressão do sufixo Async também é verificável na mesma versão. [OutputCacheKeyProvider do ASP.NET Core 9.0.9](https://github.com/dotnet/aspnetcore/blob/v9.0.9/src/Middleware/OutputCaching/src/OutputCacheKeyProvider.cs) · [MvcOptions do ASP.NET Core 9.0.9](https://github.com/dotnet/aspnetcore/blob/v9.0.9/src/Mvc/Mvc.Core/src/MvcOptions.cs)

### Exclusivos de A

| ID, natureza, severidade | Passagem e evidência técnica | Efeito sobre o leitor e correção |
|---|---|---|
| A1 · alcance da colisão · alta | A116–118 reconhece a chave externa. PropertyQuery.UpSert atualiza a linha em colisão sem comparar ou reatribuir DeviceId. B114–116 leva isso ao caso de chave global por nome. | A operação endereçada a um pai pode alterar o valor visível em outro. Incluir o exemplo condicional e o que o 204 oculta. |
| A2 · ambiguidade de identidade em agregação e escrita · média | A118,158,174. DeviceRepository.GetByCommandAsync agrupa pelo ID público. GetIdDevice e ExecuteScalarAsync usam o primeiro escalar, sem ORDER BY. B89,177 explicitam os dois mecanismos. | Com IDs públicos duplicados, o GET pode combinar filhos e a escrita direta atuar sobre um único pai físico. Explicar a divergência frente à subquery escalar do upsert, que pode falhar. |
| A3 · dependência de disponibilidade · média | A87,158,174 mencionam o agregado e o JOIN. GetDeviceAsync usa GetAllDevices com DeviceSettings antes de POST/DELETE. B87,170 deriva a consequência. | Uma falha em Settings impede que se tente incluir ou excluir uma Property. Identificar essa causa no diagnóstico da escrita. |
| A4 · combinação de prefixos · média | A45 aceita prefixo. B61 especifica seleção de prefixo para todo o modelo. Ensaio com dois campos em estilos diferentes levou apenas o prefixado ao repositório. | Campo sem prefixo pode ser descartado e, se o prefixado for branco, o GET pode listar tudo. Explicar a regra de escolha do prefixo. |
| A5 · consequência de órfãos · média | A175,193 condiciona sobrevivência à FK. PropertyQuery.GetAll requer INNER JOIN. B179 fecha a cadeia até a invisibilidade da filha. | Um 204 na leitura após remoção do pai não prova limpeza física. Explicar a diferença observável entre ausência na projeção e remoção. |

### Exclusivo de B

| ID, natureza, severidade | Passagem e evidência técnica | Efeito sobre o leitor e correção |
|---|---|---|
| B1 · alcance e elegibilidade da cópia em cache · média | B203 registra chave, expiração e bypass por Authorization. A199 explicita armazenamento local e exclusão usual de respostas com Set-Cookie. Program.AddOutputCache usa o store padrão, sem store distribuído. | Dois atendimentos dirigidos a réplicas distintas podem observar valores diferentes dentro do TTL. Indicar alcance local e a restrição de armazenamento para resposta com cookie. |

## 7. Conhecimento exclusivo e compartilhado

**Correto em ambos.** O DTO não fornece o ID do DELETE. Um 204 na listagem não distingue ausência de pai de ausência de filha. O GET de item falha antes de poder responder sobre existência. POST pode confirmar antes de devolver 500. PUT e PATCH compartilham colunas, transação e riscos de repetição. A criação incorporada perde a descrição submetida, o join deduplica por ID e o UPDATE do pai não persiste filhos. O DDL de outro dialeto não fornece a chave do MySQL.

**Adicional correto em A.** O cache padrão é local à instância e restringe armazenamento de respostas que definem cookie. A apresenta em uma tabela os comprimentos e o vínculo do DDL histórico. B dá os mesmos dados de DDL em prosa, o que conta como equivalência de conhecimento.

**Adicional correto em B.** Além de A1–A5, B distingue o corpo 415 do dicionário de validação, mostra que o objeto ainda pode ser aceito com mídia de JSON Patch, identifica o ramo de acesso que ignora GetDevicesWithCapabilities e diferencia Property de uma chave Settings para MQTT. A explicita que gravar uma Property não comanda o dispositivo. B identifica o consumidor específico HasSettingsMqtt e o campo realmente consultado. Seu exemplo de filtro por settings trata seleção de pais. Não há suporte no código lido para inferir uma decisão de broker a partir de gravar SubscribedTopics.

**Parcial, incorreto ou omitido.** S1 e S8 mostram afirmações corretas somente com condições adicionais. Tomada como regra geral sobre a ordem enviada, a previsão de B61 é incorreta para o GET elegível ensaiado. S2–S7 registram efeitos observáveis ausentes ou apenas parcialmente descritos. S9 trata sustentação documental, sem converter a ausência do link em incorreção técnica. A superioridade de B em efeitos entre recursos convive com o risco de sua regra mais específica para repetidos.

**Limitações validadas.** Três famílias externas legítimas em ambos: catálogo MySQL implantado (FK, chaves, nulabilidade, defaults, triggers, collation e modo SQL), perímetro efetivo (gateway, TLS e acesso direto) e comportamento operacional de uma execução concreta (concorrência, falha no commit, conteúdo real e reinício). O manifesto mostra o entrypoint declarado e o reinício solicitado. Efeitos condicionais de uma chave global, de um órfão, a atribuição SQL a LastActive e a composição da chave de cache já são investigáveis. A decisão de transformar algum deles em contrato normativo pertence ao Arquiteto. Transações reais, políticas externas, registros históricos dos autores e estado físico dos dispositivos permanecem sem verificação independente.

## 8. Rastreabilidade

Examinei os alvos que cada texto associa à afirmação e os símbolos que executam o comportamento. Em A há 53 ocorrências de links e 28 destinos distintos. Em B há 74 ocorrências e 35 destinos distintos, incluindo uma âncora interna. Os agrupamentos abaixo julgam **cadeias semânticas**, não o número de links. S = suficiente para localizar e reconstruir o núcleo afirmado. P = parcial, pois a última etapa depende de material além dos links da passagem. Os diretórios de scripts também foram inspecionados.

| Cadeia examinada e teste de sustentação | A | B |
|---|---|---|
| 1. README/seed → DTO → escrita. O nome de exemplo existe e o repositório só escreve dados | S 3 | S 3 |
| 2. Controller → DI → contrato. Os verbos, parâmetros, lifetimes e ausência de service coincidem | S 16,33,181 | S 18,170 |
| 3. DTO/Core → filtro → formatter. Classes sustentam ausência de chamada a Validate e saída sem ID. Pacotes e bootstrap, sozinhos, não estabelecem toda a coerção de escalares e binding | P 29–39 | P 31–37 |
| 4. Criteria → builder → GetAll. Lidos texto base, operadores e WithCriteria. A cita o repositório em outra seção | S 43–62,181 | S 43–63 |
| 5. WithId → QueryFirstOrDefaultAsync → handler. O trecho substitui o SELECT, e o número de erro não especializado cai no default MySQL | S 66–83 | S 67–83 |
| 6. POST → transação → MVC. SQL e commit sustentados. O nome efetivamente registrado e a falha ao formatar exigem o default da versão ou ensaio | P 87–112,217 | P 87–110,211 |
| 7. PUT/PATCH/DELETE → SQL ativo. Lidos os callers e parâmetros, inclusive a diferença entre escalar e subquery, colunas no UPDATE e contagem ignorada | S 116–169 | S 114–166 |
| 8. Consumers Devices → conversões → SQL/Core. JOIN ativo, splitOn, agrupamento, UpdateDevice e InsertProperty sustentam as passagens. Em B a fonte de SettingsTypes explicita a chave mqtt_primary_host | S 171–177 | S 174–181 |
| 9. DDL e lacuna. init.sql declara SQL Server, três scripts MySQL tratam índices, métricas e dashboard. README declara cascata, cuja presença em produção segue externa | S 179–193 | S 170–181,209 |
| 10. Exceções, falhas fatais e perímetro. Switch, teste HasStarted, Regex de Dashboard, Exit(1) e labels de Swarm permitem reconstruir as condições | S 195–213 | S 183–205 |
| 11. Armazenamento, query e elegibilidade do cache. Program prova expiração e posição, mas não implementa os defaults | P 199,217 | P 203,211 |
| 12. Autoridade e versão de ensaio. Dossiê §10, mapa e Readiness sustentam o escopo. O csproj prova TFM/pacotes, sem fixar o patch do runtime ou o host do ensaio histórico | P 16,39,217 | P 207–211 |

São oito cadeias suficientes e quatro parcialmente suficientes por documento. B211 contém uma referência imprecisa se lida como fonte para TestHost 9.0.1. O projeto da API referencia MVC NewtonsoftJson 9.0.1, mas não TestHost. A descrição de um ensaio continua sendo evidência declarada pelo autor, independentemente de o projeto da API citar sua infraestrutura. Nenhum destino ausente identificado. A organização por símbolos permite reconstruir a maior parte das cadeias SQL em ambos. Para B1, o armazenamento em memória e a regra de cookie foram também confrontados com a biblioteca exata. [Registro padrão do store](https://github.com/dotnet/aspnetcore/blob/v9.0.9/src/Middleware/OutputCaching/src/OutputCacheServiceCollectionExtensions.cs) · [DefaultPolicy 9.0.9](https://github.com/dotnet/aspnetcore/blob/v9.0.9/src/Middleware/OutputCaching/src/Policies/DefaultPolicy.cs)

## 9. Eficiência de consumo

| Medida do blob original | A | B |
|---|---:|---:|
| Palavras operacionais, por split em espaços em branco Unicode, incluindo exemplos e Markdown | 3.303 | 4.142 |
| Linhas, por splitlines | 217 | 211 |
| Bytes originais | 25.265 | 31.472 |
| Seções H2–H6, excluído o título H1 | 12 | 12 |

Para cada pergunta recuperei os blocos indicados abaixo, incluindo exemplos e links que caem dentro do intervalo. O volume é a soma de palavras desses blocos sem duplicatas **no cenário**. É um percurso reproduzível de consulta seletiva. Outros leitores podem usar buscas ou selecionar trechos menores. Seções de falhas, por exemplo, podem ser ignoradas ao formar a escrita, mas passam a ser indispensáveis ao diagnosticar seu retorno.

| Cenário e informação indispensável | A, linhas e palavras consultadas | B, linhas e palavras consultadas | Resposta, trechos dispensáveis e investigação residual |
|---|---|---|---|
| Integrar POST. Corpo, validação, pai, inserção e resultado | 31–39 + 85–112 · **606** | 20–37 + 85–110 · **896** | Ambos permitem construir a requisição e evitar retry cego do 500. B explicita falha do JOIN anterior à inclusão. Dispensa DELETE, GET por ID e watchdog. Unicidade real exige catálogo. |
| Diagnosticar falha de consulta. Binding, SQL e formato da falha | 41–83 + 201–211 · **589** | 43–83 + 183–199 · **779** | Ambos distinguem 400 do MVC, 500 do banco e 204 sem linhas. B melhora a busca por prefixos. Dispensa ciclo do pai e upsert. Para filtro repetido e resposta condicional é preciso investigar o cache. |
| Investigar divergência escrita/leitura. Commit, colisão, descrição e cache | 85–90 + 114–138 + 195–199 · **763** | 85–91 + 112–137 + 201–203 · **971** | Ambos distinguem resposta perdida, descrição preservada e GET antigo. B mostra PUT entre GETs, A localiza a cópia por instância. Dispensa detalhes do DDL histórico e do GET por ID. Estado real exige evidência do banco. |
| Avaliar impacto sobre recursos relacionados. Criação, JOIN, PATCH, FK | 171–177 + 179–193 · **518** | 168–181 · **812** | A identifica o efeito básico no pai. B permite prever órfãos invisíveis e filhos reunidos por identificador público. Dispensa formatos de 400 e logs do watchdog. Em ambos falta o efeito final sobre last_active. |
| Determinar segurança e concorrência. DI, transação, cache e perímetro | 179–181 + 195–213 · **607** | 168–172 + 201–205 · **664** | Ambos distinguem fronteira transacional e exposição externa. A explicita cache local. B explica a falta de reconciliação. Dispensa exemplos de payload. Regra do gateway e isolamento efetivo exigem evidência externa. |

A usa menos texto nos cinco percursos selecionados. B concentra o ciclo de vida em uma tabela e acrescenta consequências que evitam uma segunda investigação em A. Há repetição funcional nas tabelas iniciais e exemplos, útil como índice. O detalhe do contrato HTTP em A e o agrupamento de falhas em B facilitam a leitura seletiva, embora ambos exijam combinar operação e cache para diagnosticar uma leitura divergente. Sem tokenizer apropriado no Python usado para a medição. Duração cognitiva, custo financeiro e consumo de tokens do agente não foram medidos.

## 10. Consolidação comparativa

| Dimensão e unidade | A | B |
|---|---:|---:|
| Operações diretas com caminho principal corretamente explicado / inventariadas | 6/6 | 6/6 |
| Famílias materiais confrontadas documentalmente (§5) | 17 | 17 |
| Famílias corretas quanto às afirmações feitas / parcialmente corretas / inteiramente incorretas | 16 / 1 / 0 | 15 / 2 / 0 |
| Achados aplicáveis (§6), sem duplicação dentro do documento | 14 | 10 |
| Achados de cobertura ou condição funcional/operacional, excluído S9 de rastreabilidade | 13 | 9 |
| Famílias com condição insuficiente ou omitida, no recorte S1, S3, S5, S8, A1, A2, A4 e B1 | 7 | 5 |
| Interações insuficientes, no recorte S1–S5, S7, S8, A1–A5 | 12 | 7 |
| Consequências operacionais ausentes ou incompletas, no recorte S2–S8, A1–A3, A5 e B1 | 11 | 8 |
| Destinos distintos examinados / cadeias de sustentação parciais em 12 | 28 / 4 | 35 / 4 |
| Previsões diretas corretas / corretamente condicionadas / parciais / incorretas / indetermináveis, em 6 | 4 / 2 / 0 / 0 / 0 | 4 / 2 / 0 / 0 / 0 |
| Famílias de limitações externas legítimas (§7) | 3 | 3 |
| Severidade alta / média / baixa | 3 / 9 / 2 | 2 / 6 / 2 |

As categorias de interação, condição e consequência se sobrepõem. As 17 famílias agrupam o conteúdo afirmativo por decisão e fonte, sem pretensão de que cada família equivalha a uma frase ou tenha peso igual. Em B a regra absoluta de ordem em B61 tem contraexemplo, embora a família 4 também contenha várias afirmações corretas. Entre os 15 achados distintos, **nove são compartilhados** (duas altas, cinco médias, duas baixas), **cinco exclusivos de A** (uma alta, quatro médias) e **um exclusivo de B** (média). A fração de achados em cada categoria depende da granularidade declarada.

## 11. Parecer final

**Documento A — satisfatório com ressalvas.** Sustenta as decisões de integração das seis operações e favorece recuperar rapidamente o fluxo principal. Perde precisão quando o leitor precisa prever a repercussão de uma chave global, de uma falha de Settings ou de um identificador público duplicado.

**Documento B — satisfatório com ressalvas.** É mais informativo nesses três casos e no diagnóstico de órfãos. A regra de seleção de valores repetidos exige qualificação e faltam as condições de cache local úteis em instalações com várias réplicas.

**Parecer comparativo — B apresenta qualidade materialmente superior no conjunto avaliado.** A diferença decisiva é a capacidade de prever qual recurso será afetado e o que o consumidor poderá observar. Para o contrato direto simples e os seis exemplos cegos, há equivalência prática. A foi mais econômica nos cinco percursos medidos. Para cache, PATCH do pai e diagnóstico de falhas secundárias, ambos exigem as ressalvas do referencial. O parecer trata do conhecimento dos blobs identificados e não atribui causas aos seus processos de produção.

Todas as seis operações diretas e os cinco percursos relacionados em Devices foram confrontados. Não se reivindica enumeração de todas as entradas do MVC, de todas as combinações concorrentes ou de todas as regras de um catálogo externo. A determinação de status MySQL continua condicionada à exceção que chega ao handler. A única alteração versionável desta intervenção é este relatório.

A guarda local nomeada por AGENTS.md está ausente na árvore da revisão comum. A estrutura deste relatório foi verificada com o validador distribuído na raiz EKOM, aplicado somente ao arquivo entregue.
