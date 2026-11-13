# Testes

A suite protege o comportamento da API e do domínio. Há três camadas: **unitários**, **integração** (HTTP in-process) e **e2e HTTP** (Requestly contra API real).

Índice: [`docs/README.md`](README.md) · README: [`README.md`](../README.md).

---

## Unitários

| | |
|--|--|
| Projeto | [`tests/UnitTests`](../tests/UnitTests) |
| Escopo | Espelha `src/`: Domain, Application, Api, Infrastructure |
| Estilo | **AAA** (`Arrange` / `Act` / `Assert`) — um tipo sob teste por arquivo |
| Meta | ≥ **80%** line e branch por assembly (`Api`, `Application`, `Infrastructure`, `Domain`) |

```bash
dotnet test tests/UnitTests/UnitTests.csproj
./run-tests-with-coverage.sh
```

Relatório HTML: `test-results/coverage-report/index.html`. No CI, o job **Unit tests** publica o artifact `coverage-report` e o resumo no Job Summary.

Não escrevemos testes de feature ainda não implementada.

---

## Integração

| | |
|--|--|
| Projeto | [`tests/IntegrationTests`](../tests/IntegrationTests) |
| Stack | `WebApplicationFactory` + **Testcontainers** (PostgreSQL) |
| Requisito | Docker disponível na máquina / no runner |

Cobre fluxos HTTP reais contra a API em memória com banco efêmero, por exemplo:

- Criação de OS com serviços/peças (incluindo listas vazias)
- Listagem (ordenação + exclusões)
- Aprovação/rejeição com JWT cliente + token opaco
- Fluxos ponta a ponta do ciclo da OS

```bash
dotnet test tests/IntegrationTests/IntegrationTests.csproj
```

No CI: job **Integration tests** em paralelo com unitários e lint.

---

## E2E HTTP (Requestly)

Exercitam a API **já em execução** (Docker ou `dotnet run`), fora da suite .NET.

| | |
|--|--|
| Guia | [`07_api.md`](07_api.md) |
| Canônico (sistema) | [`e2e/docs/requestly/`](https://github.com/fiap-vcosta/e2e/tree/main/docs/requestly) |
| Pasta local | [`requestly/`](requestly/) (não é a fonte de verdade do sistema) |
| Environments | Docker · Local · **GCP-Gateway** (`https://vcosta-fiap.online`) |

Importar no [Requestly](https://requestly.com/) a partir do **e2e**, escolher o environment e as pastas de fluxo. Alternativa visual: Swagger (`http://localhost:8083/swagger` no Compose deste serviço).

Aprovação como **ator cliente**: token opaco na query **+** JWT cliente (Bearer via `autenticacao-service`); ownership por documento. Preencher `tokenAprovacao` a partir do banco entre finalizar diagnóstico e aprovar.

---

## CI

Workflow [`.github/workflows/ci.yml`](../.github/workflows/ci.yml): Lint + Unit (com gate de cobertura) + Integration em paralelo a cada `push`.
