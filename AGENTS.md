# Instruções permanentes e roteamento EKOM

**Modelo EKOM:** 5.0 no escopo qualificado de capabilities; histórico 4.x preservado

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
- diretriz local de adoção: `docs/rfc/EKOM-GUIDELINES.md`;
- arquitetura e contratos: `docs/specs/ISSP-Architecture.md`,
  `docs/specs/ISSP-Commissioning.md` e `components/README.md`;
- targets e execução de testes:
  `docs/specs/Repository-Test-Execution-Policy.md`;
- guarda documental: `python3 tools/validate_ekom_documents.py .`.

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
