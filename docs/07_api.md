# API — collections e exploração

Artefatos para exercitar a API fora da suite .NET (Swagger + Requestly).  
Camada **e2e HTTP**: ver também [`06_testes.md`](06_testes.md).  
Índice: [`docs/README.md`](README.md) · README: [`README.md`](../README.md).

## Requestly (canônico = e2e)

O canônico do sistema está em [`e2e/docs/requestly/`](https://github.com/fiap-vcosta/e2e/tree/main/docs/requestly) (environments Docker / GCP-Gateway + suites de ponta a ponta).

A pasta local [`requestly/`](requestly/) neste repositório existe como cópia/legado de exploração do serviço; **não** é a fonte de verdade do sistema.

JWT staff e JWT cliente: [`autenticacao-service`](https://github.com/fiap-vcosta/autenticacao-service) — paths sob `/autenticacao` (login, `POST /autenticacao/clientes/token`, JWKS).

### Environments (referência)

| Nome | `baseUrl` / auth | Quando usar |
|------|------------------|-------------|
| **Docker** | `http://localhost:8083` (OS) / autenticacao local | Compose deste serviço ou compose do e2e |
| **Local** | `http://localhost:5225` | `dotnet run` deste serviço |
| **GCP-Gateway** | `https://vcosta-fiap.online` / `…/autenticacao` | Entrada oficial (API Gateway + GKE Gateway) |

### JWT cliente

Preferencial: [`autenticacao-service`](https://github.com/fiap-vcosta/autenticacao-service) — no Gateway, `POST /autenticacao/clientes/token` com `{ "documento": "…" }`.

### Ator cliente (aprovação com JWT + token opaco)

Localiza a OS pelo token opaco na query e exige **JWT de cliente** (Bearer). Ownership: documento do JWT deve ser o do dono da OS.

- `POST /ordem-servico/aprovar?token=...` (+ `Authorization: Bearer <JWT cliente>`)
- `POST /ordem-servico/rejeitar?token=...` (+ `Authorization: Bearer <JWT cliente>`)

Chamam os mesmos use cases de aprovar/rejeitar da API staff. Sem JWT cliente → `401`. Token inválido ou documento do JWT ≠ dono da OS → `404`. O token opaco não é exposto nas responses de criação/consulta.

O token opaco não vem na API. Após o passo que deixa a OS em `AguardandoAprovacao`, consulte o banco e preencha `tokenAprovacao` antes de aprovar:

```sql
SELECT "TokenAprovacao" FROM "OrdensServico" WHERE "Id" = '<ordemServicoId>';
```

### Paths deste serviço

Prefixo público `/ordem-servico` (sem `/api`). Tabela completa no [`README.md`](../README.md). Cadastro de cliente, estoque e emissão de JWT não são rotas deste serviço (API interna de cliente: `administrativo-service` em `/system/administrativo/…`).

### Alternativa

- Swagger Compose: http://localhost:8083/swagger/index.html  
- Swagger Local: http://localhost:5225/swagger/index.html
