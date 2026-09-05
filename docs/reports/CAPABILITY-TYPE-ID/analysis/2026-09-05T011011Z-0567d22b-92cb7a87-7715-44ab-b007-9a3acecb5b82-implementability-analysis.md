# Análise de implementabilidade — CapabilityType por id 0.2

**Classe da fonte:** Relatório

**Papel:** Autor e Engenheiro Analista, mesma atuação da correção humana

**Especificação:** `SHD-CAPABILITY-TYPE-ID-001@0.2`

**Revisão confrontada (SHA-256):** `0567d22b2b1a3b0a376893a6b52cf67e1d071309472bba89b70ec12b5042066d`

**Baseline:** `b310b01`

**Execução:** `92cb7a87-7715-44ab-b007-9a3acecb5b82`

**Estado:** Final

## Problemas bloqueantes

Nenhum. A consulta por id já existe no controller e no repositório
(`src/Api/Controllers/CapabilityTypeController.cs:40`,
`src/Data.Repositories/Repositories/CapabilityTypeRepository.cs:89`). A
correção da rota e do Location é local ao controller, sem dependência nova.

## Reconciliação anterior

O relatório `2026-09-05T005112Z-3f300ef9-80a95854-7035-487a-8ef9-af14afd40318-implementability-analysis.md`
não continha bloqueadores. Sua premissa de preservar GET por nome foi
substituída expressamente pela decisão humana incorporada na revisão 0.2;
não há bloqueador anterior sem disposição. As restrições de schema, testes e
guarda documental continuam não bloqueantes e não são remediadas neste recorte.

## Controle

Confrontados oito requisitos, cinco critérios e bordas da revisão inteira,
com foco nos requisitos 002, 003, 004 e 008 alterados. Nenhum débito técnico
aceito relacionado; lacunas 0001, 0002 e 0003 preservadas. O contrato anterior
é emendado explicitamente; a autoridade de Groups permanece intacta.

Challenge limitado: nenhum conflito interno, critério insatisfazível,
remediação obrigatória fora do recorte ou bloqueador anterior sem disposição.
A remoção do GET por nome e o ajuste do Location são coerentes; listagem,
PATCH, DELETE e identidade persistida continuam tecnicamente atendíveis.

## Restrições não bloqueantes

- A quebra de compatibilidade do GET individual é expressamente solicitada;
  consumidores devem usar id no caminho. Não há alias legado nesta revisão.
- Métodos internos de repositório por nome podem permanecer para seus usos
  existentes; removê-los não é necessário para retirar o endpoint HTTP.
- Evidência HTTP/banco exige autorização própria. A análise é por inspeção;
  não certifica execução. Nenhum teste integra o recorte e a suíte retirada
  permanece intocada.
- A guarda documental continua ausente na baseline.

**Classificação principal:** Pronta [`Ready`].
