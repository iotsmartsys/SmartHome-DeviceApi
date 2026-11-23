# Especificação — Consolidação do mapa de conhecimento funcional

**ID:** `SHD-KNOWLEDGE-MAP-001`

**Classe da fonte:** Normativa para a atividade documental especificada

**Versão:** 0.1

**Estado normativo:** Draft — aguardando aprovação do Arquiteto

**Análise de implementabilidade:** Ready para a versão 0.1, conforme relatório
`docs/reports/SHD-KNOWLEDGE-MAP/analysis/2026-11-23T182436Z-0.1-347bf974-2461-43e3-8c61-8010c93cb912-implementability-analysis.md`

**Relação normativa:** New. Esta especificação governa somente a consolidação
documental interna e não altera contratos funcionais da API.

## 1. Objetivo e justificativa

Consolidar uma base navegável de conhecimento funcional do `SmartHome-DeviceApi`
que permita localizar os domínios, relações, regras sustentadas, contratos HTTP,
fluxos, componentes de implementação, limitações e evidências sem repetir uma
investigação integral do código a cada atividade.

O conhecimento interno poderá servir posteriormente como referência à
documentação de desenvolvedores do `IoTSmartSysDocs`. Esta especificação não
autoriza nem governa alterações naquele repositório ou documentação pública.

## 2. Autoridades e decisões aprovadas

As decisões abaixo foram aprovadas pelo Arquiteto na ordem desta atuação:

- **DA-01 — Organização:** `docs/rfc/KNOWLEDGE-MAP.md` é o índice de descoberta,
  autoridades, estados, evidências e lacunas; `docs/specs/SYSTEM-DOSSIER.md` é a
  visão geral arquitetural e funcional; `docs/knowledge/` recebe referências
  funcionais especializadas por domínio. Estas últimas são informativas,
  rastreáveis e não constituem especificações normativas EKOM.
- **DA-02 — Escopo:** incluir Devices, Capabilities, Capability Types, Capability
  History, Device Metrics, Device Settings, Settings globais, Properties,
  Groups, Platforms, Monitored Places, Smart Home Capabilities, Dashboards,
  widgets e relações, endpoints utilitários e controle de acesso legado.
- **DA-03 — Controle de acesso legado:** classificar o componente identificado
  tecnicamente como OAuth como **Controle de acesso proprietário legado —
  Deprecated**. Preservar nomes técnicos; não atribuir conformidade oficial com
  OAuth 2.0. Registrar que permanece operacional para compatibilidade com
  consumidores existentes e que deverá ser substituído no futuro, sem definir
  estratégia, cronograma ou arquitetura da substituição. Não investigar nem
  documentar serviços externos, incluindo o serviço Node.js. Não modificar,
  remover ou descontinuar o componente.
- **DA-04 — Rastreabilidade:** afirmações técnicas relevantes apontam para
  evidência verificável; comportamento implementado, observado, normativo e
  não confirmado são categorias distintas. A existência de teste não prova sua
  execução nem aprovação.
- **DA-05 — Sustentabilidade:** mudanças futuras de comportamento funcional
  consideram o impacto sobre o mapa por meio dos mecanismos EKOM e revisão já
  existentes. Não criar ferramentas, automações ou obrigações de governança.

## 3. Escopo

A atividade autorizada por uma futura ordem de implementação deverá:

1. confrontar código, contratos OpenAPI disponíveis, especificações normativas,
   relatórios e documentação atual para todos os domínios da DA-02;
2. documentar, conforme evidência existente, responsabilidades, conceitos,
   relações, identificadores, modelos, regras, validações, operações HTTP,
   fluxos, respostas, erros, exceções, dependências, limitações e lacunas;
3. manter o `KNOWLEDGE-MAP.md` como índice navegável das fontes e do estado do
   conhecimento, e o `SYSTEM-DOSSIER.md` como visão sistêmica;
4. criar referências informativas especializadas em `docs/knowledge/`, incluindo
   `docs/knowledge/legacy-access-control.md`;
5. preservar integralmente a autoridade e o estado das especificações normativas
   existentes, sem convertê-las em descrições informativas nem declarar aceite
   operacional que suas fontes não sustentem.

## 4. Fora de escopo

- Alterar código de produção, contratos HTTP/OpenAPI ou comportamento da API.
- Modificar autenticação, gateway, infraestrutura, banco ou schema.
- Modificar, remover, migrar ou descontinuar o controle de acesso legado.
- Investigar ou documentar serviços externos, inclusive serviço Node.js, ou
  integração externa com MQTT, Traefik ou TechSys Smart ID.
- Criar, alterar, corrigir ou executar testes. `tests/Api.Tests` continua
  `Retired` e não é evidência.
- Implementar conteúdo no `IoTSmartSysDocs` ou em outro destino público.
- Introduzir novas ferramentas, automações, estrutura de governança ou obrigações
  além da atualização do conhecimento materialmente afetado no processo EKOM.
- Corrigir referências ausentes no `AGENTS.md`.

## 5. Requisitos

### 5.1 Cobertura funcional

- **KM-001:** todos os domínios e recursos listados na DA-02 devem estar
  localizáveis pelo índice principal.
- **KM-002:** cada referência especializada deve expor, quando sustentados,
  responsabilidade, relações, identificadores, regras, operações, fluxos,
  respostas/falhas, dependências, limitações e evidências da implementação.
- **KM-003:** relacionamentos e fluxos podem usar Mermaid quando o diagrama
  esclarecer a relação; o diagrama não substitui as referências às fontes.
- **KM-004:** o documento de controle de acesso deve seguir exatamente DA-03 e
  limitar-se à responsabilidade, ao funcionamento interno relevante e ao estado
  na Devices API.

### 5.2 Evidência e autoridade

- **KM-005:** toda afirmação técnica relevante terá referência local verificável
  a arquivo, classe, método, rota, query, especificação, OpenAPI ou relatório,
  conforme aplicável.
- **KM-006:** classificar conteúdo como contrato normativo, implementação
  encontrada, comportamento observado, contexto informado pelo responsável ou
  informação não confirmada; não promover uma categoria a outra por inferência.
- **KM-007:** divergências entre código, OpenAPI e documentação existente devem
  ser registradas com evidência e sem correção de comportamento nesta atividade.
- **KM-008:** lacunas do schema, políticas, integrações ou consumidores devem
  permanecer explícitas enquanto não houver fonte confirmatória.
- **KM-009:** não incluir segredos, tokens, credenciais, chaves ou dados pessoais
  sensíveis. Exemplos devem usar dados sintéticos.
- **KM-010:** nenhum documento pode afirmar execução, validação, aprovação ou
  aceite com base somente na existência de código ou testes.

### 5.3 Manutenção

- **KM-011:** mudanças futuras de comportamento funcional devem considerar e
  reconciliar o impacto material no mapa, no dossiê e nas referências afetadas
  dentro da transação EKOM aplicável.
- **KM-012:** não criar automação nem gate novo; usar os mecanismos de autoria,
  análise, implementação, revisão e atualização de conhecimento já vigentes.

## 6. Estrutura documental prevista

- `docs/rfc/KNOWLEDGE-MAP.md`: índice, autoridades, estado, evidências, lacunas e
  relações entre fontes.
- `docs/specs/SYSTEM-DOSSIER.md`: resumo sistêmico, arquitetura, domínios e
  navegação para documentos especializados.
- `docs/knowledge/devices.md`
- `docs/knowledge/capabilities.md` — inclui relações e histórico de capabilities.
- `docs/knowledge/capability-types.md`
- `docs/knowledge/device-metrics.md`
- `docs/knowledge/settings.md` — globais, específicos e efetivos.
- `docs/knowledge/properties.md`
- `docs/knowledge/groups.md`
- `docs/knowledge/platforms-and-places.md`
- `docs/knowledge/smart-home-capabilities.md`
- `docs/knowledge/dashboards-and-widgets.md`
- `docs/knowledge/utility-endpoints.md`
- `docs/knowledge/legacy-access-control.md`

Os agrupamentos de arquivos não reduzem a cobertura exigida por KM-001/KM-002.
Referências normativas existentes serão ligadas, não copiadas integralmente.

## 7. Critérios de aceite

| Critério | Cenário e resultado observável | Requisitos | Meio de validação |
|---|---|---|---|
| KM-AC-001 | Partindo do índice, localizar cada domínio da DA-02 e ao menos uma referência de implementação apropriada, ou uma lacuna declarada quando não houver fonte. | 001, 002, 005 | Inspeção do índice e de todas as referências especializadas. |
| KM-AC-002 | Para cada domínio, leitor consegue distinguir contrato, regra implementada/observada, relação, fluxo, resposta/falha e informação não confirmada. | 002, 006, 008 | Amostragem por domínio e confronto das afirmações com as fontes citadas. |
| KM-AC-003 | Rotas, parâmetros e modelos documentados correspondem aos controllers e ao OpenAPI disponível; diferenças observadas são registradas sem serem ocultadas. | 005, 007 | Confronto de cada operação documentada com controller/modelo e OpenAPI existente. |
| KM-AC-004 | A referência de controle de acesso contém a classificação e limites da DA-03 e não afirma conformidade oficial com OAuth 2.0 nem descreve serviços externos. | 004, 006 | Inspeção textual e confronto com o código local. |
| KM-AC-005 | Nenhuma afirmação deriva sucesso de mera existência de testes; nenhum dado sensível foi adicionado. | 009, 010 | Inspeção integral dos documentos alterados e do diff. |
| KM-AC-006 | O dossiê e o mapa apontam para a estrutura especializada sem replicar especificações ou registrar estados/evidências não confirmados. | 001, 005, 010, 011 | Inspeção de links, duplicação material e estados citados. |
| KM-AC-007 | A proposta de manutenção usa somente o fluxo EKOM existente, sem ferramenta ou gate novo. | 011, 012 | Inspeção do texto e do delta documental. |

## 8. Validação e evidências

A validação da implementação documental será por inspeção textual e do diff,
conferência das referências e links locais, confronto das afirmações com as
fontes e validação estrutural documental disponível no repositório. O
`AGENTS.md` atualmente referencia `tools/validate_ekom_documents.py`, mas o
script e o diretório `tools/` não foram encontrados nesta baseline; sua
execução não será simulada. Build, testes, chamadas HTTP, banco, infraestrutura
e hardware não integram esta especificação. Nenhuma execução de teste é
autorizada por ela.

## 9. Restrições e dependências

- O contrato aprovado `SHD-ENGINEERING-001@0.1` e o Repository Readiness atual
  abrangem capabilities e dependências materiais do estado do ar; não abrangem
  a implementação funcional dos demais domínios.
- Esta especificação limita-se a artefatos documentais e não implementa
  comportamentos dos domínios não qualificados.
- A fonte MySQL completa e a autoridade de views continuam lacunas registradas
  em `EKM-GAP-0002`; o conteúdo deve distingui-las de fatos dedutíveis das
  queries existentes.
- Referências apontadas no `AGENTS.md` mas ausentes nesta baseline devem ser
  registradas como inconsistências, sem correção nesta versão.
- As referências do mapa não substituem código ou contrato; verificações
  pontuais continuam necessárias quando a atividade depender de detalhe atual.

## 10. Limites da futura implementação

Uma futura implementação depende de aprovação desta versão e de ordem explícita
do Arquiteto. A ordem deverá manter o escopo documental desta especificação.
Não autoriza alteração de código, contrato funcional, OpenAPI, testes,
autenticação, infraestrutura, integração externa, IoTSmartSysDocs, commit, merge
ou deploy. Ampliação material do escopo exige atualização normativa e análise
aplicável antes da execução.

## 11. Referências e achados de governança

- `AGENTS.md`, EKOM 5.0 com habilitação de engenharia delimitada a capabilities.
- `docs/rfc/REPOSITORY-ENGINEERING-CONTRACT.md`, `SHD-ENGINEERING-001@0.1`.
- `docs/rfc/REPOSITORY-READINESS.md` e a avaliação aprovada de 24/09/2026.
- `docs/rfc/KNOWLEDGE-MAP.md` e `docs/specs/SYSTEM-DOSSIER.md`.
- Especificações e relatórios atuais em `docs/specs/` e `docs/reports/`.
- Perfis EKOM 5.0: regras comuns, Autor da Especificação e Engenheiro Analista.

O `AGENTS.md` referencia `docs/rfc/EKOM-GUIDELINES.md`,
`docs/specs/ISSP-Architecture.md`, `docs/specs/ISSP-Commissioning.md`,
`components/README.md` e `tools/validate_ekom_documents.py`; esses alvos não foram
encontrados na baseline inspecionada. Este registro não corrige o roteador nem
determina o destino futuro dessas referências.

## 12. Estado da autorização

A especificação foi registrada por ordem expressa do Arquiteto para formalizá-la.
Essa ordem não aprova automaticamente o conteúdo normativo e não autoriza sua
implementação. O estado `Draft` permanece até decisão humana explícita.
