# Tech Challenge — Guia para agentes

Aplicação .NET 8 do ciclo de ordens de serviço (diagnóstico, aprovação, execução, pagamento). Clean Architecture + DDD/CQRS (MediatR). Banco PostgreSQL + EF Core. Catálogo, estoque, orçamento e JWT ficam nos outros serviços.

## Antes de mudar código

1. Ler [docs/01_requisitos.md](docs/01_requisitos.md) (canon de requisitos; paths atuais = Outubro/2026) e ADRs em [`docs/adrs/`](docs/adrs/)
2. Domínio (linguagem + ER): [`e2e` / docs/dominio/ordem-servico](https://github.com/fiap-vcosta/e2e/blob/main/docs/dominio/ordem-servico.md) — stubs locais em [docs/00_linguagem-onipresente.md](docs/00_linguagem-onipresente.md) e [docs/08_modelo-de-dados.md](docs/08_modelo-de-dados.md)
3. Espelhar padrões das pastas vizinhas; não inventar estrutura paralela
4. Não escrever testes de feature ainda não implementada; não inventar requisitos fora de `docs/01`
5. **Git:** nunca commit/push direto em `main` — branch → PR → merge (ver [`.cursor/rules/git-workflow.mdc`](.cursor/rules/git-workflow.mdc))

## Layout da solution

| Projeto | Responsabilidade |
|---------|------------------|
| `src/Domain` | Entities, VOs, eventos de domínio e exceções |
| `src/Application` | `UseCases/` (Commands/Queries/Handlers/Responses) + ports em `Abstractions/Gateways` e `Abstractions/Services` |
| `src/Infrastructure` | EF Core, Pub/Sub, HTTP para administrativo/estoque; validação JWT via JWKS (`autenticacao-service`) — sem SMTP |
| `src/Api` | Controllers, Requests/Validators, **Presenters**, ViewModels e filtro de Problem Details |
| `tests/UnitTests` | Testes unitários do comportamento existente |
| `tests/IntegrationTests` | Testes de integração HTTP (`WebApplicationFactory` + Testcontainers) |
| `terraform/` | Instância Cloud SQL própria + database/usuário/secrets + Pub/Sub subscriptions/publisher IAM |
| `k8s/` | Deployment (Proxy + Datadog), Service, HPA, KSA, HTTPRoute `/ordem-servico` |
| `.github/workflows/` | `ci` (dotnet+terraform), `build-push`, `deploy`, `tf-apply`, `tf-destroy` |

Dependências apontam para dentro: Api → Application → Domain; Infrastructure implementa ports.

`OrdemServico.Id` e as FKs `IdOrdemServico` são `Guid` (gerado em `OrdemServicoAggregateRoot.Criar()`); `Servico.Id` e `ItemNecessario.Id` continuam `int`.

Não adicionar comentários narrativos no código. Comentários só são aceitáveis quando explicam intenção, restrição ou trade-off que o código não comunica; marcadores AAA nos testes continuam obrigatórios.

Responses/ViewModels devem ser reutilizados quando o shape e a semântica forem iguais entre endpoints; não criar DTO por verbo HTTP por simetria de pasta.

Referência SOAT: https://github.com/proferickmuller/soat-cleanarch-csharp

## Testes

- Sempre **AAA** (`Arrange` / `Act` / `Assert`) — detalhes em [`.cursor/rules/tests.mdc`](.cursor/rules/tests.mdc).
- Testes unitários novos seguem o mesmo caminho relativo do arquivo de produção em `src/` e cobrem um tipo por arquivo.
- Meta de cobertura unitária: **≥ 80% line e branch** por assembly (`Api`, `Application`, `Infrastructure`, `Domain`).
- Só cobrir comportamento já implementado (não TDD de feature futura).

## Comandos

```bash
dotnet build TechChallenge.sln
dotnet test TechChallenge.sln
dotnet restore
# Cobertura + HTML: ./run-tests-with-coverage.sh
# Integração precisa de Docker (Testcontainers Postgres)
# Docker app (ver README): docker-compose --profile app up -d
# CI: .github/workflows/ci.yml (push → lint + unit + integration em paralelo)
# CD: build-push (merge em main) e deploy (manual) — nunca disparar sem pedido explícito
```

Swagger local (Compose deste serviço): `http://localhost:8083/swagger/index.html`

Collections Requestly: canônicas no [`e2e/docs/requestly/`](https://github.com/fiap-vcosta/e2e/tree/main/docs/requestly); pasta local [`docs/requestly/`](docs/requestly/) (ver [`docs/07_api.md`](docs/07_api.md))

## Regras canônicas (resumo)

- Clean Architecture: Presenters, Gateways, UseCases; Domain sem ORM
- Aprovação/rejeição: **JWT cliente + token opaco**; ownership por CPF; **mesmos** use cases
- Listagem de OS: excluir Finalizada, AguardandoPagamento, CobrancaExpirada, Entregue, Descartada; ordem evolutiva em `docs/01`
- Detalhes: [docs/01_requisitos.md](docs/01_requisitos.md)
