# Instruções permanentes e roteamento EKOM

**Modelo EKOM:** 5.0 no escopo qualificado de capabilities; histórico 4.x preservado

**Fonte oficial verificada:** `https://github.com/iotsmartsys/EKOM-guidelines`,
revisão `e0498569169a46257a0f7a278cb48ff17774ded0` (branch `main`, consultada em
23/11/2026). A versão oficial vigente nessa revisão é 5.0.

**Modalidade:** capacidades referenciadas e governança proporcional

**Estado:** vigente

## Autoridade

O Arquiteto humano tem autoridade final sobre intenção, prioridade, escopo,
arquitetura, risco aceitável, relevância das críticas, suficiência das
evidências, aprovação, conclusão ou reabertura e integração. A especificação é
a fonte da verdade para comportamento e governa a execução dos agentes.

## Fonte dos perfis

**Raiz do EKOM:** `/Users/marcelocostamiranda/source/EKM-guidelines`

Antes de qualquer atuação EKOM:

1. leia integralmente `roles/REGRAS-COMUNS.md` na raiz do EKOM;
2. leia o perfil correspondente à capacidade recebida;
3. leia a especificação indicada, quando aplicável;
4. leia somente as fontes técnicas pertinentes.

| Capacidade recebida | Perfil |
|---|---|
| Autor da Especificação | `roles/AUTOR-DA-ESPECIFICACAO.md` |
| Engenheiro Analista | `roles/ENGENHEIRO-ANALISTA.md` |
| Engenheiro Implementador | `roles/ENGENHEIRO-IMPLEMENTADOR.md` |
| Crítico ou Engenheiro Revisor | `roles/ENGENHEIRO-REVISOR.md` |
| Consultor de Arquitetura | `roles/CONSULTOR-DE-ARQUITETURA.md` |

Análise de implementabilidade é obrigatória antes da implementação, mas pode
ser executada na mesma atuação quando autorizada. Challenge é consultivo e
proporcional ao risco, não um gate universal.

Implementação no escopo qualificado exige contrato aprovado e Repository
Readiness válida, análise `Ready` e ordem humana para a mesma versão. Não exige
promoção documental adicional. Com essas condições satisfeitas, o build canônico dos entregáveis
construíveis afetados integra a implementação e não exige cláusula na
especificação. Coleta ou execução de testes, flash, monitor e hardware exigem
autorização própria.

## Qualificação de engenharia

Contrato aprovado: [SHD-ENGINEERING-001@0.1](docs/rfc/REPOSITORY-ENGINEERING-CONTRACT.md).
Estado, avaliação e decisão humana: [Repository Readiness](docs/rfc/REPOSITORY-READINESS.md).
Adoção/habilitação EKOM 5.0 aprovada em 23/09/2026 para capabilities e dependências
materiais necessárias ao estado do ar-condicionado: inclusão, value, PATCH,
consultas, Core, conversões, persistência, DI e falhas específicas. Não inferir
qualificação de outros domínios; trabalho fora do alcance exige avaliação própria.
Histórico 4.x permanece rastreável e não é retroativamente reclassificado.

## Fontes locais do projeto

- especificações: `docs/specs/`;
- ADRs: `docs/adr/`;
- relatórios: `docs/reports/`;
- transações e lacunas: `docs/rfc/EKOM-CHANGELOG.md`;
- débitos técnicos aceitos: `docs/rfc/KNOWLEDGE-MAP.md`, namespace
  `EKOM-DEBT-NNNN`;
- mapa de conhecimento: `docs/rfc/KNOWLEDGE-MAP.md`;
- visão e navegação: `docs/specs/SYSTEM-DOSSIER.md`;
- arquitetura e prontidão: `docs/specs/SYSTEM-DOSSIER.md`,
  `docs/rfc/REPOSITORY-ENGINEERING-CONTRACT.md` e
  `docs/rfc/REPOSITORY-READINESS.md`;
- implementação por camada: `src/Api/`, `src/Core/` e
  `src/Data.Repositories/`;
- contratos HTTP derivados e sua procedência: `docs/contracts/openapi.json` e
  `docs/contracts/README.md`;
- conhecimento funcional: `docs/knowledge/`;
- execução da suíte de integração HTTP Dashboard:
  `tests/Api.IntegrationTests/README.md`;
- não há uma guarda estrutural documental instalada neste repositório. O
  [template opcional do EKOM](https://github.com/iotsmartsys/EKOM-guidelines/blob/e0498569169a46257a0f7a278cb48ff17774ded0/templates/tools/validate_ekom_documents.py)
  está disponível na revisão oficial registrada acima.

## Manutenção obrigatória da base de conhecimento funcional

A cada implementação realizada neste repositório, independentemente do domínio
funcional afetado, avalie o impacto sobre todos os documentos existentes em
`docs/knowledge/`. Atualize todos os documentos materialmente afetados e crie
referências funcionais quando forem introduzidos domínios, recursos ou
conceitos não adequadamente representados. Siga os precedentes de organização,
nomenclatura, estrutura, conteúdo e rastreabilidade dos documentos existentes.

Atualize `docs/rfc/KNOWLEDGE-MAP.md` quando houver mudança relevante na
organização, cobertura, referências ou lacunas do conhecimento. Atualize
`docs/specs/SYSTEM-DOSSIER.md` quando a visão arquitetural ou funcional do
sistema for afetada. Quando houver mudança em contratos HTTP, avalie
`docs/contracts/openapi.json`; registre explicitamente eventual desatualização
e preserve a procedência do artefato.

Preserve a distinção entre contrato normativo, comportamento implementado,
comportamento observado, contexto informado e informação não confirmada.
Registre divergências e lacunas identificadas, sem ocultá-las nem convertê-las
em afirmações sem suporte.

A manutenção documental integra a mesma transação EKOM da implementação.
Documentos sem conteúdo afetado não precisam ser modificados, mas a ausência de
impacto deve resultar de avaliação explícita. O relatório de implementação deve
identificar documentos atualizados, novos documentos criados e o resultado da
avaliação de impacto. A implementação não está concluída enquanto houver
atualização documental materialmente necessária pendente. Esta obrigação
também se aplica a correções, refatorações e alterações arquiteturais quando
afetarem o conhecimento documentado.

## Arquitetura da base de conhecimento

- `docs/rfc/KNOWLEDGE-MAP.md` é o índice central de descoberta, autoridades,
  evidências e lacunas.
- `docs/specs/SYSTEM-DOSSIER.md` mantém a visão sistêmica, arquitetural e
  funcional.
- `docs/knowledge/` contém referências funcionais especializadas e
  informativas; não as promova a especificações normativas.
- `docs/contracts/` contém artefatos derivados de contratos HTTP e suas
  informações de procedência.
- Não crie processo paralelo à EKOM, duplique governança nem introduza
  ferramentas sem necessidade demonstrada.

## Suite de testes descontinuada

Por decisão humana de 25/07/2026, `tests/Api.Tests` está classificada como
`Retired` em todo o repositório.

- Não execute, repare ou evolua essa suíte.
- Não use seus resultados como evidência, critério de aceite ou bloqueio em
  nenhuma atuação.
- Os arquivos permanecem apenas como registro histórico.
- A referência ainda existente em `src/SmartHome-Api.sln` é uma discrepância
  legada e não reativa a suíte; sua reconciliação está registrada em
  `EKM-GAP-0003`.
- Use `dotnet build src/Api/Api.csproj` como build canônico da API.
- Obtenha as demais validações obrigatórias da especificação aplicável à
  mudança.

## Limites

## Invariantes locais


> **Specifications orchestrate. Code implements.**
