# Testes integrados HTTP — Dashboard

Suíte independente em C#/.NET 9 para a API Dashboard 0.3. Recebe uma API já
iniciada e executa requests reais com asserções de status, modelos, persistência
observável e regras do contrato. Não utiliza mocks, SQL, assemblies internos da
API nem `tests/Api.Tests` (Retired).

Esta entrega foi apenas compilada. **Nenhum teste, request ou cleanup foi
executado pelo agente.** Os comandos abaixo são para execução futura.

## Preparação

- SDK .NET 9 para compilar/executar.
- API com o schema e catálogo da Dashboard 0.3 já aplicados.
- Pelo menos uma capability existente, compatível e com DeviceId textual.
  A suíte não cria nem altera devices/capabilities. Pode usar leituras offline
  ou sem valor: verifica os status observados, sem fabricar leituras.
- Informe a URL raiz da instalação, sem `/api/v1`. Um prefixo de proxy pode
  fazer parte da raiz. Não há endereço de servidor predefinido.
- Se a instalação exigir Bearer, forneça `DASHBOARD_TEST_TOKEN` pelo ambiente.
  O token não vai para argumentos, manifesto ou logs da suíte.

A partir da raiz do repositório, **somente compilar**:

```sh
dotnet build tests/Api.IntegrationTests/Api.IntegrationTests.csproj
```

## Criar, consultar e alterar em uma execução

Substitua a URL abaixo pelo destino escolhido. Escolha um manifesto novo para
cada execução; um arquivo já existente nunca é sobrescrito por `run`.

```sh
dotnet run --no-build --project tests/Api.IntegrationTests -- run \
  --base-url https://sua-api.example/ \
  --state tests/Api.IntegrationTests/.runs/dashboard-001.json
```

Opcionalmente, adicione `--capability-id 22` e/ou `--timeout-seconds 60`.
Sem ID, a suíte seleciona uma capability compatível, preferindo uma numérica
para exercitar também gauge. Não seleciona apenas fontes com leitura `ok`.
O timeout padrão é 30 segundos por request; intervalo aceito: 1–300.

O comando executa IT-01 a IT-09 na ordem e preserva os dois dashboards e seus
widgets. Em falha, interrompe os cenários dependentes, registra o resultado e
mantém os dados para inspeção. **Não existe limpeza automática**, nem em
`finally`, cancelamento ou sucesso. O cliente HTTP bloqueia DELETE em `run`.

| Cenário | Conteúdo |
|---|---|
| IT-01 | Catálogo, compatibilidade, ordenação e seleção da fonte. |
| IT-02 | Criação de dois dashboards, defaults, IDs, Location e timestamps. |
| IT-03 | Criação de dois widgets, vínculos, defaults e configuração. |
| IT-04 | Consulta individual/lista/dados, contagem, ordenação e coerência dos valores/status. |
| IT-05 | Alteração de dashboard, persistência, omitidos, reset e no-op. |
| IT-06 | Alteração de widget, posição parcial, limites válidos e persistência. |
| IT-07 | Configuração sobre defaults, reset e exemplo de gauge quando aplicável. |
| IT-08 | Validações, erros HTTP/JSON/mídia, campos desconhecidos, limites e widget de outro dashboard. |
| IT-09 | PUTs concorrentes em campos distintos, reset e no-op do widget. |

## Excluir e limpar separadamente

Pode ser executado depois, em outro processo, sem repetir `run` e mesmo se
esse comando falhou ou foi interrompido:

```sh
dotnet run --no-build --project tests/Api.IntegrationTests -- cleanup \
  --state tests/Api.IntegrationTests/.runs/dashboard-001.json
```

A limpeza utiliza a URL gravada no manifesto, sem aceitar troca de destino.
Ela localiza os dashboards pelos IDs e pelo marcador exato daquela execução,
confere a identidade por GET, exclui seus widgets e depois os dashboards.
Confere 204/corpo vazio, ausência posterior e 404 nas repetições. Quando a
capability ainda existe antes da limpeza, verifica que continua consultável
ao final; uma fonte que já tenha sido removida não impede a limpeza.

`cleanup` permite apenas GET/DELETE; não cria dados nem atualiza recursos.
Nenhum DELETE é dirigido a devices, capabilities, catálogo ou dashboards de
outra execução. Dashboards de teste nunca são marcados como padrão global.
A limpeza de um dashboard que falhar não impede tentar os demais; resultado
incompleto tem saída não zero e pode ser retomado pelo mesmo comando.

## Manifesto, falhas e retomada

- Antes de qualquer escrita HTTP, o manifesto registra um RunId único, URL e
  chaves das fixtures. Nomes e marcadores são derivados dessas chaves.
- Cada criação registra o ID assim que a resposta é recebida. Resultados de
  cada cenário são salvos como `Not Executed`, `Running`, `Passed` ou `Failed`.
- Se o POST persistir e a resposta se perder, cleanup recupera o recurso por
  **igualdade exata** da descrição `dashboard-api-integration:<RunId>:<chave>`.
  Não exclui por um prefixo genérico de nome.
- A descrição é o marcador de propriedade. Não a altere manualmente antes da
  limpeza. Alteração manual do marcador exige reconciliação humana; a suíte
  recusa excluir um ID cujo marcador não confere.
- O arquivo é salvo por substituição atômica e protegido contra dois processos
  simultâneos pelo arquivo `.lock`. Aguarde/encerre `run` antes de usar cleanup
  com o mesmo manifesto. O arquivo de lock pode permanecer após o término;
  sua existência, sozinha, não significa que esteja bloqueado.
- Ctrl+C não executa teardown. Após interrupção abrupta, aguarde o término dos
  requests no servidor e repita cleanup se necessário. Ele redescobre recursos
  mesmo quando uma limpeza anterior já registrou `Cleaned`.
- O manifesto permanece após a limpeza. Recursos já ausentes são aceitos;
  repetir cleanup não recria dados. Use outro manifesto para um novo run.
- `.runs/` é ignorado pelo Git. Guarde o manifesto enquanto puder haver dados
  dessa execução; sem ele a suíte não oferece uma exclusão global alternativa.
- Não há repetição automática de POST nem redirecionamento HTTP automático.

O console mostra PASS/FAIL por cenário. O manifesto é também o relatório JSON
persistente, incluindo datas e erros; cenários dependentes não executados não
são contados como aprovados. Códigos de saída: `0` sucesso; `1` falha de
cenário/HTTP/limpeza; `2` configuração ou manifesto inválido.

Este projeto possui um executor explícito para separar o ciclo de testes da
limpeza. Use `dotnet run ... -- run|cleanup`; `dotnet test` não é seu comando
de execução. O build não contém descoberta ou execução de cenários.

## Cobertura e limites

O contrato e a rastreabilidade estão em
[DASHBOARD-API-INTEGRATION-TESTS.md](../../docs/specs/DASHBOARD-API-INTEGRATION-TESTS.md).
A suíte não promete comprovar todos os critérios da API 0.3: não altera o padrão
global, fontes, relógios ou infraestrutura para fabricar todos os casos de
conversão/precedência. Não inspeciona cascade físico no banco. O exemplo
numérico de gauge só se aplica quando a capability escolhida é numérica;
o restante do cenário de configuração continua aplicável aos outros tipos.

A API e as fontes podem mudar entre requests. Mudanças externas podem causar
falhas legítimas de asserção; os dados e o manifesto permanecem para inspeção.
Nenhuma execução futura está configurada em CI ou no startup da API.
