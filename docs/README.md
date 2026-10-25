# Documentação

Comece pelo [`README.md`](../README.md) na raiz.

| Doc | Conteúdo |
|-----|----------|
| [00_linguagem-onipresente.md](00_linguagem-onipresente.md) | Vocabulário do domínio |
| [01_requisitos.md](01_requisitos.md) | Requisitos (sessões Maio/2025, Julho/2026, Setembro/2026 e Outubro/2026) |
| `02_*.egn` | Histórias de domínio (EventStorming) |
| `03_01_event-storming-linha-do-tempo.jpg` | Linha do tempo |
| `03_02_event-storming-agregados.jpg` | Agregados |
| [06_testes.md](06_testes.md) | Unitários, integração e e2e HTTP |
| [07_api.md](07_api.md) | Swagger + Requestly |
| [08_modelo-de-dados.md](08_modelo-de-dados.md) | ER PostgreSQL + snapshots da OS |
| [adrs/001-datadog-apm.md](adrs/001-datadog-apm.md) | Observabilidade Datadog (APM) |
| [`datadog/`](datadog/) | Dashboard JSON + monitores da demo |
| [`requestly/`](requestly/) | Collections e environments HTTP |

Escolha do PostgreSQL e Cloud SQL: ADRs no repo [`infra-db`](https://github.com/fiap-vcosta/infra-db/tree/main/docs/adrs) (a ADR de engine foi migrada para lá).
