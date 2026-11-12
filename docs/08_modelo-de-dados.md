# Modelo de dados

Esquema PostgreSQL da OS (EF Core / Npgsql). Instância Cloud SQL própria deste serviço: [ADR no e2e](https://github.com/fiap-vcosta/e2e/blob/main/docs/adrs/001-instancia-cloud-sql-por-servico.md).

Fonte de verdade do mapeamento: [`AppDbContext`](../src/Infrastructure/Database/AppDbContext.cs) e o snapshot de migrations.

## Diagrama ER

```mermaid
erDiagram
    Clientes ||--o{ Veiculos : "IdCliente Restrict"
    OrdensServico ||--o{ ItensServicos : "IdOrdemServico Cascade"
    ItensServicos ||--o{ ItemNecessario : "IdItemOrdemServico Cascade"
    OrdensServico ||--o{ ItemNecessario : "IdOrdemServico Restrict"

    Usuarios {
        int Id PK
        text Login UK
        text Senha
        text TipoUsuario
    }

    Clientes {
        int Id PK
        text Nome
        text Email
        text TipoDocumento
        text Documento UK
    }

    Veiculos {
        int Id PK
        int IdCliente FK
        text Placa UK
        text Modelo
        text Marca
    }

    Servicos {
        int Id PK
        text Codigo UK
        text Nome
        numeric PrecoPadrao
        boolean Ativo
    }

    ItensEstoque {
        int Id PK
        text Codigo UK
        text Tipo
        text Nome
        text UnidadeMedida
        numeric PrecoVenda
        numeric Saldo
        numeric SaldoReservado
    }

    OrdensServico {
        uuid Id PK
        text Status
        timestamptz RecebidaEm
        timestamptz EntregueEm
        timestamptz DescartadaEm
        timestamptz AprovadaEm
        text TokenAprovacao UK
        int Cliente_Id
        text Cliente_Nome
        text Cliente_Email
        text Veiculo_Placa
        text Veiculo_Marca
        text Veiculo_Modelo
    }

    ItensServicos {
        int Id PK
        uuid IdOrdemServico FK
        text Status
        text Nome
        numeric ValorCobrado
        timestamptz AprovadoEm
        timestamptz RejeitadoEm
        timestamptz ExecucaoIniciadaEm
        timestamptz ExecucaoFinalizadaEm
        int ServicoCatalogo_Id
        text ServicoCatalogo_Nome
        text ServicoCatalogo_Codigo
    }

    ItemNecessario {
        int Id PK
        uuid IdOrdemServico FK
        int IdItemOrdemServico FK
        text Status
        numeric Quantidade
        int ItemEstoque_Id
        text ItemEstoque_Codigo
        text ItemEstoque_Nome
        text ItemEstoque_UnidadeMedida
    }
```

## Relacionamentos (FK)

| De | Coluna | Para | Delete |
|----|--------|------|--------|
| `Veiculos` | `IdCliente` | `Clientes` | Restrict |
| `ItensServicos` | `IdOrdemServico` | `OrdensServico` | Cascade |
| `ItemNecessario` | `IdItemOrdemServico` | `ItensServicos` | Cascade |
| `ItemNecessario` | `IdOrdemServico` | `OrdensServico` | Restrict |

Catálogo (`Servicos`, `ItensEstoque`) e cadastro (`Clientes`, `Veiculos`) **não** têm FK a partir da OS: a ordem guarda um **snapshot** (ver abaixo).

## Snapshots (`ComplexProperty`)

Na abertura/diagnóstico da OS, o domínio copia dados do cliente, veículo, serviço de catálogo e item de estoque para value objects persistidos como colunas `Cliente_*`, `Veiculo_*`, `ServicoCatalogo_*` e `ItemEstoque_*`. Não são foreign keys: a OS permanece legível mesmo se o cadastro mudar depois (preço, nome, e-mail).

## Enums (persistidos como `text`)

| Enum | Valores |
|------|---------|
| `TipoUsuario` | `Admin`, `Atendente`, `Mecanico`, `Cliente` |
| `TipoDocumento` | `Cpf`, `Cnpj` |
| `ItemTipo` | `Peca`, `Insumo` |
| `UnidadeMedida` | `Unidade`, `Jogo`, `Par`, `Litro`, `Kg`, `mL`, `Frasco` |
| `StatusOrdemServico` | `Recebida`, `EmDiagnostico`, `AguardandoAprovacao`, `ChecandoEstoque`, `AguardandoPeca`, `LiberadaParaExecucao`, `EmExecucao`, `Finalizada`, `Descartada`, `Entregue` |
| `StatusItemOrdemServico` | `Sugerido`, `Aprovado`, `Rejeitado`, `Concluido` |
| `StatusItemEstoque` | `EstoqueNaoChecado`, `EstoqueEmFalta`, `EstoqueDisponivel`, `EstoqueTravado`, `Utilizado` |

## Precisão numérica

| Tabela | Coluna | Precision |
|--------|--------|-----------|
| `Servicos` | `PrecoPadrao` | `(10,2)` |
| `ItensEstoque` | `PrecoVenda` | `(10,2)` |
| `ItensEstoque` | `Saldo`, `SaldoReservado` | `(10,3)` |
| `ItensServicos` | `ValorCobrado` | `(10,2)` |
| `ItemNecessario` | `Quantidade` | `(10,3)` |

## Naming

- Catálogo de serviços: tabela `Servicos` (`ServicoAggregateRoot`).
- Linha de serviço na OS: tabela `ItensServicos` (entidade de domínio `Servico` sob `OrdemServico`) — sem FK para o catálogo, só snapshot `ServicoCatalogo_*`.
