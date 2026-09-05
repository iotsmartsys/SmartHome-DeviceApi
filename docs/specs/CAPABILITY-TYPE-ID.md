# Especificação — Identificação de CapabilityType pelo id

**ID:** `SHD-CAPABILITY-TYPE-ID-001`

**Classe da fonte:** Normativa

**Versão:** 0.1

**Estado do workflow:** Rascunho [`Draft`]

**Implementação:** Não iniciada [`Not Started`]

**Relação normativa:** Nova [`New`] — primeiro contrato local para este
recorte de CapabilityType, anteriormente inventariado em `EKM-GAP-0001`.

## 1. Objetivo e decisões confirmadas

Identificar CapabilityType pela chave inteira do banco nas operações PATCH e
DELETE, acrescentar consulta individual por id e expor essa chave no modelo
HTTP `Api.Models.CapabilityType`.

O Arquiteto confirmou o rascunho conversacional e autorizou seu registro e a
análise de implementabilidade em 04/09/2026. A confirmação inclui a rota de GET
separada, a preservação da consulta por nome, a possibilidade de renomear pelo
PATCH e os status de ausência descritos abaixo. Esta atuação é documental;
a implementação da versão analisada depende de sua autorização correspondente.

## 2. Contexto, escopo e arquitetura

Na baseline, a entidade `Core.Entities.CapabilityType` já possui `int Id` e o
repositório preenche essa propriedade após o INSERT. O modelo HTTP não a expõe
e o POST devolve o modelo recebido, descartando a entidade com o id gerado.
PATCH e DELETE recebem nome; a persistência já atualiza e exclui por Id.
O PATCH sobrescreve o nome aplicado com o nome da rota, impedindo renomeação.

O recorte abrange controller, modelo HTTP e conversões de CapabilityType,
contrato do repositório, implementação Dapper e queries necessárias. Preserva
o fluxo controller/modelo HTTP → Core → repositório Dapper → MySQL, sem nova
camada, dependência, configuração, migração ou mudança arquitetural transversal.
Os ícones continuam pertencendo ao tipo consultado e às transações existentes.

Ficam fora do recorte mudanças em outros domínios, aplicativos consumidores,
políticas gerais de validação ou versionamento, autenticação, middleware,
schema, regras de cascade e demais comportamentos de persistência. Nenhum
artefato de teste integra esta versão; `tests/Api.Tests` permanece `Retired`.

## 3. Requisitos e contratos

Prefixo de todas as rotas: `/api/v1/capabilities-types`.

- **CT-ID-001:** Toda representação de `Api.Models.CapabilityType` devolvida
  pela API deve incluir `id` inteiro igual ao `Id` persistido. Os demais campos
  e ícones permanecem no formato vigente. O identificador é atribuído pelo banco;
  o POST continua aceitando payload sem `id` e um id enviado pelo cliente não
  pode determinar a identidade criada.
- **CT-ID-002:** A listagem `GET /`, inclusive seu filtro `name` e resposta
  `204` quando vazia, e a consulta `GET /{name}` devem ser preservadas.
  A consulta por nome continua retornando `200` ou `404`, inclusive para nomes
  numéricos; não deve ser redirecionada para consulta por id.
- **CT-ID-003:** Acrescentar `GET /id/{id:int}`: retornar `200` com a
  representação completa, inclusive id e ícones, quando o registro existir;
  retornar `404` quando não existir.
- **CT-ID-004:** O POST deve continuar retornando `201` após a persistência,
  agora com o id efetivamente gerado no corpo. Preservar o `Location` vigente
  para a consulta por nome do recurso criado.
- **CT-ID-005:** Substituir `PATCH /{name}` por `PATCH /{id:int}`, procurando
  exclusivamente pela chave do banco. Para registro ausente, retornar `404`
  sem atualização; para patch válido de registro existente, retornar `204`
  após a persistência.
- **CT-ID-006:** O PATCH deve permitir alteração de `name` e preservar o id
  do recurso da rota, independentemente de qualquer valor de id no documento.
  Nenhuma operação do patch pode alterar a chave persistida ou redirecionar a
  atualização para outro tipo. Preservar os demais campos e comportamentos de
  patch existentes, inclusive o tratamento dos ícones.
- **CT-ID-007:** Substituir `DELETE /{name}` por `DELETE /{id:int}`, excluindo
  exclusivamente pela chave informada. Retornar `204` após exclusão bem-sucedida
  ou quando o registro não existir, inclusive na repetição. Preservar as
  restrições de relacionamento, transações e tratamento de falhas existentes;
  uma falha de persistência não pode ser convertida em sucesso.
- **CT-ID-008:** Rotas e schemas declarados pelo controller devem refletir os
  novos parâmetros inteiros, o novo GET e o campo `id`. As operações PATCH e
  DELETE deixam de aceitar nomes como identificadores; um segmento numérico
  nessas rotas sempre significa id, mesmo se existir nome com o mesmo texto.

## 4. Bordas e compatibilidade

O seletor `int` delimita as novas rotas. Segmentos não representáveis como
inteiro não correspondem a essas rotas. Inteiros sem registro, inclusive zero
ou negativos quando ausentes, seguem CT-ID-003, CT-ID-005 e CT-ID-007; esta
versão não introduz uma validação de positividade com novo status `400`.

Renomear mantém a identidade: o GET por id continua localizando o mesmo tipo;
o GET pelo novo nome deve encontrá-lo. Conflitos de persistência seguem o
middleware vigente, sem nova regra de unicidade. O id devolvido no POST não
pode ser um valor padrão usado apenas para preencher o novo campo.

A troca dos seletores de PATCH e DELETE é uma quebra intencional do contrato
por nome, expressamente solicitada. Consumidores dessas operações precisam
usar o id devolvido pela API; sua alteração não integra este repositório.
Não se exige alias legado para essas mutações.

## 5. Critérios de aceite e evidências

| Critério | Cenário, ação e resultado observável | Requisitos | Meio de validação |
|---|---|---|---|
| CT-AC-001 | Listar tipos com ícones e consultar por nome, inclusive nome numérico: cada objeto contém a chave do registro e os campos anteriores; coleção vazia mantém `204` e nome ausente mantém `404`. | 001, 002, 008 | Inspeção dos modelos, conversões, rotas e consultas. |
| CT-AC-002 | Consultar id existente com ícones: `200` com objeto correspondente; consultar inteiro ausente: `404`; o GET por nome permanece sem ambiguidade. | 001, 002, 003, 008 | Inspeção do roteamento e fluxo de leitura até as queries de tipo e ícones. |
| CT-AC-003 | Criar sem id ou com id fornecido pelo cliente: `201` contém a chave gerada pelo banco, após commit, e `Location` da consulta por nome. | 001, 004 | Inspeção do fluxo de identidade do INSERT até a resposta e do ponto de commit. |
| CT-AC-004 | Aplicar patch válido renomeando tipo existente: `204`, mesmo id, novo nome consultável e nenhum outro tipo atualizado; id presente no patch nunca altera chave nem destino. Tipo ausente: `404`, sem UPDATE. | 005, 006, 008 | Inspeção do seletor, conversões, aplicação do patch e parâmetros do UPDATE. |
| CT-AC-005 | Excluir id existente sem impedimento de persistência e repetir: `204` em ambas; somente o tipo selecionado é excluído. Falha de banco mantém tratamento vigente e não retorna sucesso. | 007, 008 | Inspeção do seletor, DELETE, transação e propagação de exceções. |

A implementação exige inspeção dos cinco critérios, do delta e build canônico
`dotnet build src/Api/Api.csproj`. Compilação e inspeção não comprovam execução
HTTP nem persistência real. Validação funcional dos mesmos cenários por chamadas
HTTP e comparação do banco é evidência posterior, dependente de autorização
operacional própria; sem autorização, registrar `Not Executed`. Sua suficiência
para conclusão do workflow permanece decisão do Arquiteto. Não criar nem
executar suíte automatizada como consequência desta especificação.

Nesta etapa documental, verificar integridade textual e referências. A guarda
indicada no `AGENTS.md`, `tools/validate_ekom_documents.py`, não está presente
na baseline; registrar essa ausência sem inventar substituto ou ampliar escopo.

## 6. Autoridades, conhecimento e encaminhamento

Fontes confrontadas: `AGENTS.md`, `docs/rfc/KNOWLEDGE-MAP.md`,
`docs/specs/SYSTEM-DOSSIER.md` e `docs/specs/GROUPS-MAINTENANCE-SUPPORT.md`.
A especificação de Groups exclui mudanças funcionais em Capability Types e
não governa este contrato; sua autoridade permanece preservada. Não foram
localizados ADRs locais ou outra especificação funcional deste recorte.

Esta fonte cobre incrementalmente `EKM-GAP-0001`, sem encerrar a lacuna geral.
`EKM-GAP-0002` permanece aberta: esta versão não depende de introduzir cascade
ou reconstruir o schema completo. `EKM-GAP-0003` continua aplicável à suíte
retirada; nenhum débito técnico foi aceito ou criado nesta atuação.

Atualizar mapa, dossiê e `docs/rfc/EKM-CHANGELOG.md`, transação `EKM-CHG-0004`.
A análise formal desta versão é registrada separadamente em
`docs/reports/CAPABILITY-TYPE-ID/analysis/`, vinculada ao SHA-256 exato deste
arquivo. Não há decisão funcional pendente no rascunho confirmado.
