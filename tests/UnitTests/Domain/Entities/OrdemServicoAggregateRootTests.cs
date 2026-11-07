using Domain.Exceptions;
using Domain.Entities;
using Domain.ValueObjects;

namespace UnitTests.Domain.Entities;

public class OrdemServicoAggregateRootTests
{
    [Fact]
    public void CreateOrderService_SetsReceivedStatusAndCustomerAndVehicle()
    {
        // Arrange
        var cliente = new ClienteOrdemServico { Id = 1, Nome = "Maria", Email = "maria@teste.com" };
        var veiculo = new VeiculoOrdemServico { Placa = "XYZ-9876", Marca = "Toyota", Modelo = "Corolla" };

        // Act
        var ordem = OrdemServicoAggregateRoot.Criar(cliente, veiculo);

        // Assert
        Assert.Equal(StatusOrdemServico.Recebida, ordem.Status);
        Assert.Equal(0, ordem.VersaoDiagnostico);
        Assert.Equal(cliente, ordem.Cliente);
        Assert.Equal(veiculo, ordem.Veiculo);
        Assert.False(string.IsNullOrWhiteSpace(ordem.TokenAprovacao));
        Assert.True((DateTime.UtcNow - ordem.RecebidaEm).TotalSeconds < 5);
    }

    [Fact]
    public void SendToDiagnostics_WhenReceived_ChangesStatusToDiagnostics()
    {
        // Arrange
        var ordem = CriarOrdem();

        // Act
        ordem.EnviarParaDiagnostico();

        // Assert
        Assert.Equal(StatusOrdemServico.EmDiagnostico, ordem.Status);
    }

    [Fact]
    public void SendToDiagnostics_WhenNotReceived_ThrowsBusinessRuleException()
    {
        // Arrange
        var ordem = CriarOrdem();
        ordem.EnviarParaDiagnostico();

        // Act & Assert
        Assert.Throws<BusinessRuleException>(() => ordem.EnviarParaDiagnostico());
    }

    [Fact]
    public void Discard_WhenStatusIsReceived_SetsDiscardedStatus()
    {
        // Arrange
        var ordem = CriarOrdem();

        // Act
        ordem.Descartar();

        // Assert
        Assert.Equal(StatusOrdemServico.Descartada, ordem.Status);
        Assert.NotNull(ordem.DescartadaEm);
    }

    [Fact]
    public void Discard_WhenStatusIsInDiagnosis_SetsDiscardedStatus()
    {
        // Arrange
        var ordem = CriarOrdem();
        ordem.EnviarParaDiagnostico();

        // Act
        ordem.Descartar();

        // Assert
        Assert.Equal(StatusOrdemServico.Descartada, ordem.Status);
        Assert.NotNull(ordem.DescartadaEm);
    }

    [Fact]
    public void Discard_WhenAguardandoOrcamento_SetsDiscardedStatus()
    {
        // Arrange
        var ordem = CriarOrdemComDiagnosticoFinalizado();

        // Act
        ordem.Descartar();

        // Assert
        Assert.Equal(StatusOrdemServico.Descartada, ordem.Status);
    }

    [Fact]
    public void Discard_WhenLiberadaParaExecucao_SetsDiscardedStatus()
    {
        // Arrange
        var ordem = CriarOrdemComServicoAprovado();
        ordem.AplicarEstoqueReservado(new Dictionary<int, StatusItemEstoque> { [100] = StatusItemEstoque.EstoqueTravado });

        // Act
        ordem.Descartar();

        // Assert
        Assert.Equal(StatusOrdemServico.Descartada, ordem.Status);
    }

    [Fact]
    public void Discard_WhenStatusIsNotDiscardable_ThrowsBusinessRuleException()
    {
        // Arrange
        var ordem = CriarOrdem();
        ordem.Descartar();

        // Act & Assert
        Assert.Throws<BusinessRuleException>(() => ordem.Descartar());
    }

    [Fact]
    public void Discard_WhenEmExecucao_ThrowsBusinessRuleException()
    {
        // Arrange
        var servicoCatalogo = new ServicoCatalogo { Id = 1, Nome = "Serviço", Codigo = "SVR-001" };
        var ordem = CriarOrdem();
        ordem.EnviarParaDiagnostico();
        ordem.AdicionarItemServico("Serviço 1", 200m, servicoCatalogo, []);
        ordem.AdicionarItemServico("Serviço 2", 150m, servicoCatalogo, []);
        ordem.FinalizarDiagnostico();
        ordem.AplicarOrcamentoGerado(1);
        ordem.AprovarServicosSugeridos();
        ordem.AplicarEstoqueReservado(new Dictionary<int, StatusItemEstoque>());
        var primeiroId = ordem.Servicos.First().Id;
        ordem.ConfirmarExecucao(
        [
            new ServicoExecutado
            {
                IdServico = primeiroId,
                IniciadoEm = DateTime.UtcNow.AddHours(-1),
                FinalizadoEm = DateTime.UtcNow
            }
        ]);

        // Act & Assert
        Assert.Throws<BusinessRuleException>(() => ordem.Descartar());
    }

    [Fact]
    public void AddServiceItem_WhenRecebida_AddsServiceSuccessfully()
    {
        // Arrange
        var servicoCatalogo = new ServicoCatalogo { Id = 1, Nome = "Serviço", Codigo = "SVR-001" };
        var ordem = CriarOrdem();

        // Act
        ordem.AdicionarItemServico("Troca de pneus", 200m, servicoCatalogo, []);

        // Assert
        Assert.Single(ordem.Servicos);
        Assert.Equal(StatusOrdemServico.Recebida, ordem.Status);
    }

    [Fact]
    public void AddServiceItem_WhenAguardandoOrcamento_ThrowsBusinessRuleException()
    {
        // Arrange
        var servicoCatalogo = new ServicoCatalogo { Id = 1, Nome = "Serviço", Codigo = "SVR-001" };
        var ordem = CriarOrdemComDiagnosticoFinalizado();

        // Act & Assert
        Assert.Throws<BusinessRuleException>(() => ordem.AdicionarItemServico("Alinhamento", 150m, servicoCatalogo, []));
    }

    [Fact]
    public void FinalizeDiagnosis_WithServices_SetsAguardandoOrcamentoAndIncrementsVersion()
    {
        // Arrange
        var servicoCatalogo = new ServicoCatalogo { Id = 1, Nome = "Serviço", Codigo = "SVR-001" };
        var ordem = CriarOrdem();
        ordem.EnviarParaDiagnostico();
        ordem.AdicionarItemServico("Troca de pneus", 200m, servicoCatalogo,
        [
            new ItemNecessario.CriarItemNecessarioParams(1, 4m, new ItemEstoqueOrdemServico { Id = 100, Nome = "Pneu" })
        ]);

        // Act
        ordem.FinalizarDiagnostico();

        // Assert
        Assert.Equal(StatusOrdemServico.AguardandoOrcamento, ordem.Status);
        Assert.Equal(1, ordem.VersaoDiagnostico);
    }

    [Fact]
    public void AplicarOrcamentoGerado_WithSuggestedService_SetsAguardandoAprovacao()
    {
        // Arrange
        var ordem = CriarOrdemComDiagnosticoFinalizado();

        // Act
        ordem.AplicarOrcamentoGerado(1);

        // Assert
        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, ordem.Status);
    }

    [Fact]
    public void AplicarOrcamentoGerado_WithApprovedService_GoesToStockCheck()
    {
        // Arrange
        var ordem = CriarOrdemComDiagnosticoFinalizado();
        ordem.AplicarOrcamentoGerado(1);
        ordem.AprovarServicosSugeridos();

        // Act
        var status = ordem.Status;

        // Assert
        Assert.Equal(StatusOrdemServico.ChecandoEstoque, status);
    }

    [Fact]
    public void AplicarOrcamentoGerado_WithOnlyRejectedServices_SetsEntregue()
    {
        // Arrange
        var ordem = CriarOrdemComDiagnosticoFinalizado();
        ordem.AplicarOrcamentoGerado(1);
        ordem.RejeitarServicosSugeridos();
        ordem.FinalizarDiagnostico();

        // Act
        ordem.AplicarOrcamentoGerado(2);

        // Assert
        Assert.Equal(StatusOrdemServico.Entregue, ordem.Status);
        Assert.Equal(2, ordem.VersaoDiagnostico);
        Assert.NotNull(ordem.EntregueEm);
    }

    [Fact]
    public void AplicarOrcamentoGerado_WithWrongVersion_ThrowsBusinessRuleException()
    {
        // Arrange
        var ordem = CriarOrdemComDiagnosticoFinalizado();

        // Act & Assert
        Assert.Throws<BusinessRuleException>(() => ordem.AplicarOrcamentoGerado(99));
    }

    [Fact]
    public void AplicarOrcamentoGerado_WhenNotAguardandoOrcamento_ThrowsBusinessRuleException()
    {
        // Arrange
        var ordem = CriarOrdem();

        // Act & Assert
        Assert.Throws<BusinessRuleException>(() => ordem.AplicarOrcamentoGerado(1));
    }

    [Fact]
    public void AplicarOrcamentoGerado_WithOnlyApprovedServices_GoesToChecandoEstoque()
    {
        // Arrange
        var ordem = CriarOrdem();
        var catalogo = new ServicoCatalogo { Id = 1, Nome = "Serviço", Codigo = "SVR-001" };
        ordem.EnviarParaDiagnostico();
        ordem.AdicionarItemServico("A", 100m, catalogo, []);
        ordem.AdicionarItemServico("B", 50m, catalogo, []);
        ordem.FinalizarDiagnostico();
        ordem.AplicarOrcamentoGerado(1);
        var primeiro = ordem.Servicos.First().Id;
        ordem.AprovarServicosParcialmente([primeiro]);
        ordem.FinalizarDiagnostico();

        // Act
        ordem.AplicarOrcamentoGerado(2);

        // Assert
        Assert.Equal(StatusOrdemServico.ChecandoEstoque, ordem.Status);
    }

    [Fact]
    public void AplicarOrcamentoFalhou_WhenWrongStatus_ThrowsBusinessRuleException()
    {
        // Arrange
        var ordem = CriarOrdem();

        // Act & Assert
        Assert.Throws<BusinessRuleException>(() => ordem.AplicarOrcamentoFalhou(1));
    }

    [Fact]
    public void RegistrarCobrancaGerada_WhenWrongStatus_ThrowsBusinessRuleException()
    {
        // Arrange
        var ordem = CriarOrdem();

        // Act & Assert
        Assert.Throws<BusinessRuleException>(() => ordem.RegistrarCobrancaGerada());
    }

    [Fact]
    public void RegistrarPagamentoRecusado_WhenWrongStatus_ThrowsBusinessRuleException()
    {
        // Arrange
        var ordem = CriarOrdem();

        // Act & Assert
        Assert.Throws<BusinessRuleException>(() => ordem.RegistrarPagamentoRecusado());
    }

    [Fact]
    public void Discard_WhenAguardandoAprovacao_SetsDiscardedStatus()
    {
        // Arrange
        var ordem = CriarOrdemComDiagnosticoFinalizado();
        ordem.AplicarOrcamentoGerado(1);

        // Act
        ordem.Descartar();

        // Assert
        Assert.Equal(StatusOrdemServico.Descartada, ordem.Status);
    }

    [Fact]
    public void AplicarOrcamentoFalhou_ReturnsToEmDiagnostico()
    {
        // Arrange
        var ordem = CriarOrdemComDiagnosticoFinalizado();

        // Act
        ordem.AplicarOrcamentoFalhou(1);

        // Assert
        Assert.Equal(StatusOrdemServico.EmDiagnostico, ordem.Status);
        Assert.Equal(1, ordem.VersaoDiagnostico);
    }

    [Fact]
    public void RejectSuggestedServices_WhenNotAwaitingApproval_ThrowsBusinessRuleException()
    {
        // Arrange
        var ordem = CriarOrdem();

        // Act & Assert
        Assert.Throws<BusinessRuleException>(() => ordem.RejeitarServicosSugeridos());
    }

    [Fact]
    public void ApproveServicesPartially_WhenNotAwaitingApproval_ThrowsBusinessRuleException()
    {
        // Arrange
        var ordem = CriarOrdem();

        // Act & Assert
        Assert.Throws<BusinessRuleException>(() => ordem.AprovarServicosParcialmente([1]));
    }

    [Fact]
    public void AplicarEstoqueReservado_WhenStockReserved_SetsLiberatedToExecute()
    {
        // Arrange
        var ordem = CriarOrdemComServicoAprovado();
        var status = new Dictionary<int, StatusItemEstoque> { [100] = StatusItemEstoque.EstoqueTravado };

        // Act
        ordem.AplicarEstoqueReservado(status);

        // Assert
        Assert.Equal(StatusOrdemServico.LiberadaParaExecucao, ordem.Status);
        Assert.All(ordem.ItensNecessariosParaExecucao, item =>
            Assert.Equal(StatusItemEstoque.EstoqueTravado, item.Status));
    }

    [Fact]
    public void AplicarEstoqueReservado_WhenStatusIsInvalid_ThrowsBusinessRuleException()
    {
        // Arrange
        var ordem = CriarOrdem();

        // Act & Assert
        Assert.Throws<BusinessRuleException>(() =>
            ordem.AplicarEstoqueReservado(new Dictionary<int, StatusItemEstoque> { [100] = StatusItemEstoque.EstoqueTravado }));
    }

    [Fact]
    public void ConfirmExecution_WhenOrderIsNotReady_ThrowsBusinessRuleException()
    {
        // Arrange
        var ordem = CriarOrdem();

        // Act & Assert
        Assert.Throws<BusinessRuleException>(() => ordem.ConfirmarExecucao(
        [
            new ServicoExecutado
            {
                IdServico = 1,
                IniciadoEm = DateTime.UtcNow.AddHours(-1),
                FinalizadoEm = DateTime.UtcNow
            }
        ]));
    }

    [Fact]
    public void AddServiceItem_WhenInDiagnosis_AddsServiceSuccessfully()
    {
        // Arrange
        var servicoCatalogo = new ServicoCatalogo { Id = 1, Nome = "Serviço", Codigo = "SVR-001" };
        var ordem = CriarOrdem();
        ordem.EnviarParaDiagnostico();

        // Act
        ordem.AdicionarItemServico("Troca de pneus", 200m, servicoCatalogo,
        [
            new ItemNecessario.CriarItemNecessarioParams(1, 4m, new ItemEstoqueOrdemServico { Id = 100, Nome = "Pneu" })
        ]);

        // Assert
        Assert.Single(ordem.Servicos);
        Assert.Equal("Troca de pneus", ordem.Servicos.First().Nome);
    }

    [Fact]
    public void FinalizeDiagnosis_WhenNoServicesAdded_ThrowsBusinessRuleException()
    {
        // Arrange
        var ordem = CriarOrdem();
        ordem.EnviarParaDiagnostico();

        // Act & Assert
        Assert.Throws<BusinessRuleException>(() => ordem.FinalizarDiagnostico());
    }

    [Fact]
    public void RejectSuggestedServices_WhenAwaitingApproval_RejetsAllSuggestedServicesAndReturnsToEmDiagnostico()
    {
        // Arrange
        var ordem = CriarOrdemComDiagnosticoFinalizado();
        ordem.AplicarOrcamentoGerado(1);

        // Act
        ordem.RejeitarServicosSugeridos();

        // Assert
        Assert.Equal(StatusOrdemServico.EmDiagnostico, ordem.Status);
        Assert.All(ordem.Servicos, s => Assert.Equal(StatusItemOrdemServico.Rejeitado, s.Status));
    }

    [Fact]
    public void ApproveAllSuggestedServices_WhenAwaitingApproval_ApprovesAllAndGoesToStockCheck()
    {
        // Arrange
        var ordem = CriarOrdemComDiagnosticoFinalizado();
        ordem.AplicarOrcamentoGerado(1);

        // Act
        ordem.AprovarServicosSugeridos();

        // Assert
        Assert.Equal(StatusOrdemServico.ChecandoEstoque, ordem.Status);
        Assert.NotNull(ordem.AprovadaEm);
        Assert.All(ordem.Servicos, s => Assert.Equal(StatusItemOrdemServico.Aprovado, s.Status));
    }

    [Fact]
    public void ApproveServicesPartially_WhenAwaitingApproval_ApprovesSelectedAndRejetsOthers()
    {
        // Arrange
        var servicoCatalogo = new ServicoCatalogo { Id = 1, Nome = "Serviço", Codigo = "SVR-001" };
        var ordem = CriarOrdem();
        ordem.EnviarParaDiagnostico();
        ordem.AdicionarItemServico("Troca de pneus", 200m, servicoCatalogo,
        [
            new ItemNecessario.CriarItemNecessarioParams(1, 4m, new ItemEstoqueOrdemServico { Id = 100, Nome = "Pneu" })
        ]);
        ordem.AdicionarItemServico("Alinhamento", 150m, servicoCatalogo, []);
        ordem.FinalizarDiagnostico();
        ordem.AplicarOrcamentoGerado(1);
        var idPrimeiroServico = ordem.Servicos.First().Id;

        // Act
        ordem.AprovarServicosParcialmente([idPrimeiroServico]);

        // Assert
        Assert.Equal(StatusOrdemServico.EmDiagnostico, ordem.Status);
        var servicoAprovado = ordem.Servicos.First(s => s.Id == idPrimeiroServico);
        Assert.Equal(StatusItemOrdemServico.Aprovado, servicoAprovado.Status);
    }

    [Fact]
    public void AplicarEstoqueEmFalta_WhenStockMissing_SetsAguardandoPeca()
    {
        // Arrange
        var servicoCatalogo = new ServicoCatalogo { Id = 1, Nome = "Serviço", Codigo = "SVR-001" };
        var ordem = CriarOrdem();
        ordem.EnviarParaDiagnostico();
        ordem.AdicionarItemServico("Troca de pneus", 200m, servicoCatalogo,
        [
            new ItemNecessario.CriarItemNecessarioParams(1, 10m, new ItemEstoqueOrdemServico { Id = 100, Nome = "Pneu" })
        ]);
        ordem.FinalizarDiagnostico();
        ordem.AplicarOrcamentoGerado(1);
        ordem.AprovarServicosSugeridos();

        // Act
        ordem.AplicarEstoqueEmFalta(new Dictionary<int, StatusItemEstoque> { [100] = StatusItemEstoque.EstoqueEmFalta });

        // Assert
        Assert.Equal(StatusOrdemServico.AguardandoPeca, ordem.Status);
        Assert.Equal(StatusItemEstoque.EstoqueEmFalta, ordem.ItensNecessariosParaExecucao.First().Status);
    }

    [Fact]
    public void AplicarEstoqueReservado_WhenStatusIsAguardandoPeca_LiberatesOrder()
    {
        // Arrange
        var servicoCatalogo = new ServicoCatalogo { Id = 1, Nome = "Serviço", Codigo = "SVR-001" };
        var ordem = CriarOrdem();
        ordem.EnviarParaDiagnostico();
        ordem.AdicionarItemServico("Troca de pneus", 200m, servicoCatalogo,
        [
            new ItemNecessario.CriarItemNecessarioParams(1, 10m, new ItemEstoqueOrdemServico { Id = 100, Nome = "Pneu" })
        ]);
        ordem.FinalizarDiagnostico();
        ordem.AplicarOrcamentoGerado(1);
        ordem.AprovarServicosSugeridos();
        ordem.AplicarEstoqueEmFalta(new Dictionary<int, StatusItemEstoque> { [100] = StatusItemEstoque.EstoqueEmFalta });
        Assert.Equal(StatusOrdemServico.AguardandoPeca, ordem.Status);

        // Act
        ordem.AplicarEstoqueReservado(new Dictionary<int, StatusItemEstoque> { [100] = StatusItemEstoque.EstoqueTravado });

        // Assert
        Assert.Equal(StatusOrdemServico.LiberadaParaExecucao, ordem.Status);
        Assert.All(ordem.ItensNecessariosParaExecucao, item =>
            Assert.Equal(StatusItemEstoque.EstoqueTravado, item.Status));
    }

    [Fact]
    public void AplicarEstoqueEmFalta_WhenStatusIsInvalid_ThrowsBusinessRuleException()
    {
        // Arrange
        var ordem = CriarOrdem();

        // Act & Assert
        Assert.Throws<BusinessRuleException>(() =>
            ordem.AplicarEstoqueEmFalta(new Dictionary<int, StatusItemEstoque> { [100] = StatusItemEstoque.EstoqueEmFalta }));
    }

    [Fact]
    public void ConfirmExecution_WhenAllServicesCompleted_SetsFinalizedStatus()
    {
        // Arrange
        var ordem = CriarOrdemComServicoAprovado();
        ordem.AplicarEstoqueReservado(new Dictionary<int, StatusItemEstoque> { [100] = StatusItemEstoque.EstoqueTravado });
        var servicoId = ordem.Servicos.First().Id;

        // Act
        ordem.ConfirmarExecucao(
        [
            new ServicoExecutado
            {
                IdServico = servicoId,
                IniciadoEm = DateTime.UtcNow.AddHours(-1),
                FinalizadoEm = DateTime.UtcNow
            }
        ]);

        // Assert
        Assert.Equal(StatusOrdemServico.Finalizada, ordem.Status);
        Assert.All(ordem.Servicos, s => Assert.Equal(StatusItemOrdemServico.Concluido, s.Status));
        Assert.All(ordem.ItensNecessariosParaExecucao, item => Assert.Equal(StatusItemEstoque.Utilizado, item.Status));
    }

    [Fact]
    public void ConfirmExecution_WhenPartiallyCompleted_StaysInExecution()
    {
        // Arrange
        var servicoCatalogo = new ServicoCatalogo { Id = 1, Nome = "Serviço", Codigo = "SVR-001" };
        var ordem = CriarOrdem();
        ordem.EnviarParaDiagnostico();
        ordem.AdicionarItemServico("Troca de pneus", 200m, servicoCatalogo,
        [
            new ItemNecessario.CriarItemNecessarioParams(1, 4m, new ItemEstoqueOrdemServico { Id = 100, Nome = "Pneu" })
        ]);
        ordem.AdicionarItemServico("Alinhamento", 150m, servicoCatalogo, []);
        ordem.FinalizarDiagnostico();
        ordem.AplicarOrcamentoGerado(1);
        ordem.AprovarServicosSugeridos();
        ordem.AplicarEstoqueReservado(new Dictionary<int, StatusItemEstoque> { [100] = StatusItemEstoque.EstoqueTravado });
        var firstServicoId = ordem.Servicos.First().Id;

        // Act
        ordem.ConfirmarExecucao(
        [
            new ServicoExecutado
            {
                IdServico = firstServicoId,
                IniciadoEm = DateTime.UtcNow.AddHours(-1),
                FinalizadoEm = DateTime.UtcNow
            }
        ]);

        // Assert
        Assert.Equal(StatusOrdemServico.EmExecucao, ordem.Status);
    }

    [Fact]
    public void ConfirmExecution_WhenEmExecucao_ConfirmsMoreServices()
    {
        // Arrange
        var servicoCatalogo = new ServicoCatalogo { Id = 1, Nome = "Serviço", Codigo = "SVR-001" };
        var ordem = CriarOrdem();
        ordem.EnviarParaDiagnostico();
        ordem.AdicionarItemServico("Serviço 1", 200m, servicoCatalogo, []);
        ordem.AdicionarItemServico("Serviço 2", 150m, servicoCatalogo, []);
        ordem.FinalizarDiagnostico();
        ordem.AplicarOrcamentoGerado(1);
        ordem.AprovarServicosSugeridos();
        ordem.AplicarEstoqueReservado(new Dictionary<int, StatusItemEstoque>());
        var servicos = ordem.Servicos.ToList();
        servicos[0].GetType().GetProperty("Id")?.SetValue(servicos[0], 1);
        servicos[1].GetType().GetProperty("Id")?.SetValue(servicos[1], 2);
        var servicoIds = ordem.Servicos.Select(s => s.Id).ToList();
        ordem.ConfirmarExecucao(
        [
            new ServicoExecutado
            {
                IdServico = servicoIds[0],
                IniciadoEm = DateTime.UtcNow.AddHours(-2),
                FinalizadoEm = DateTime.UtcNow.AddHours(-1)
            }
        ]);
        Assert.Equal(StatusOrdemServico.EmExecucao, ordem.Status);

        // Act
        ordem.ConfirmarExecucao(
        [
            new ServicoExecutado
            {
                IdServico = servicoIds[1],
                IniciadoEm = DateTime.UtcNow.AddHours(-1),
                FinalizadoEm = DateTime.UtcNow
            }
        ]);

        // Assert
        Assert.Equal(StatusOrdemServico.Finalizada, ordem.Status);
    }

    [Fact]
    public void RegistrarCobrancaGerada_WhenFinalizada_SetsAguardandoPagamento()
    {
        // Arrange
        var ordem = CriarOrdemFinalizada();

        // Act
        ordem.RegistrarCobrancaGerada();

        // Assert
        Assert.Equal(StatusOrdemServico.AguardandoPagamento, ordem.Status);
    }

    [Fact]
    public void ConfirmarPagamentoRecebido_WhenAguardandoPagamento_SetsEntregue()
    {
        // Arrange
        var ordem = CriarOrdemFinalizada();
        ordem.RegistrarCobrancaGerada();

        // Act
        ordem.ConfirmarPagamentoRecebido();

        // Assert
        Assert.Equal(StatusOrdemServico.Entregue, ordem.Status);
        Assert.NotNull(ordem.EntregueEm);
    }

    [Fact]
    public void ConfirmarPagamentoRecebido_WhenNotAguardandoPagamento_ThrowsBusinessRuleException()
    {
        // Arrange
        var ordem = CriarOrdem();
        ordem.EnviarParaDiagnostico();

        // Act & Assert
        Assert.Throws<BusinessRuleException>(() => ordem.ConfirmarPagamentoRecebido());
    }

    [Fact]
    public void RegistrarPagamentoRecusado_WhenAguardandoPagamento_KeepsStatus()
    {
        // Arrange
        var ordem = CriarOrdemFinalizada();
        ordem.RegistrarCobrancaGerada();

        // Act
        ordem.RegistrarPagamentoRecusado();

        // Assert
        Assert.Equal(StatusOrdemServico.AguardandoPagamento, ordem.Status);
    }

    [Fact]
    public void FinalizarDiagnostico_WhenNotEmDiagnostico_ThrowsBusinessRuleException()
    {
        // Arrange
        var ordem = CriarOrdem();

        // Act & Assert
        Assert.Throws<BusinessRuleException>(() => ordem.FinalizarDiagnostico());
    }

    [Fact]
    public void AprovarServicosSugeridos_WhenNotAguardandoAprovacao_ThrowsBusinessRuleException()
    {
        // Arrange
        var ordem = CriarOrdem();
        ordem.EnviarParaDiagnostico();

        // Act & Assert
        Assert.Throws<BusinessRuleException>(() => ordem.AprovarServicosSugeridos());
    }

    [Fact]
    public void ValorTotal_ExcludesRejectedServices()
    {
        // Arrange
        var ordem = CriarOrdemComDiagnosticoFinalizado();
        ordem.AplicarOrcamentoGerado(1);
        ordem.RejeitarServicosSugeridos();

        // Act
        var valorTotal = ordem.ValorTotal;

        // Assert
        Assert.Equal(0m, valorTotal);
    }

    [Fact]
    public void ValorTotal_IncludesApprovedAndSuggestedButExcludesRejected()
    {
        // Arrange
        var servicoCatalogo = new ServicoCatalogo { Id = 1, Nome = "Serviço", Codigo = "SVR-001" };
        var ordem = CriarOrdem();
        ordem.EnviarParaDiagnostico();
        ordem.AdicionarItemServico("Serviço 1", 100m, servicoCatalogo, []);
        ordem.AdicionarItemServico("Serviço 2", 200m, servicoCatalogo, []);
        ordem.FinalizarDiagnostico();

        // Act
        var valorTotal = ordem.ValorTotal;

        // Assert
        Assert.Equal(300m, valorTotal);
    }

    private static OrdemServicoAggregateRoot CriarOrdem()
    {
        var cliente = new ClienteOrdemServico { Id = 1, Nome = "Maria", Email = "maria@teste.com" };
        var veiculo = new VeiculoOrdemServico { Placa = "XYZ-9876", Marca = "Toyota", Modelo = "Corolla" };
        return OrdemServicoAggregateRoot.Criar(cliente, veiculo);
    }

    private static OrdemServicoAggregateRoot CriarOrdemComDiagnosticoFinalizado()
    {
        var servicoCatalogo = new ServicoCatalogo { Id = 1, Nome = "Serviço", Codigo = "SVR-001" };
        var ordem = CriarOrdem();
        ordem.EnviarParaDiagnostico();
        ordem.AdicionarItemServico("Troca de pneus", 200m, servicoCatalogo,
        [
            new ItemNecessario.CriarItemNecessarioParams(1, 4m, new ItemEstoqueOrdemServico { Id = 100, Nome = "Pneu" })
        ]);
        ordem.FinalizarDiagnostico();
        return ordem;
    }

    private static OrdemServicoAggregateRoot CriarOrdemComServicoAprovado()
    {
        var ordem = CriarOrdemComDiagnosticoFinalizado();
        ordem.AplicarOrcamentoGerado(1);
        ordem.AprovarServicosSugeridos();
        return ordem;
    }

    private static OrdemServicoAggregateRoot CriarOrdemFinalizada()
    {
        var ordem = CriarOrdemComServicoAprovado();
        ordem.AplicarEstoqueReservado(new Dictionary<int, StatusItemEstoque> { [100] = StatusItemEstoque.EstoqueTravado });
        var servicoId = ordem.Servicos.First().Id;
        ordem.ConfirmarExecucao(
        [
            new ServicoExecutado
            {
                IdServico = servicoId,
                IniciadoEm = DateTime.UtcNow.AddHours(-1),
                FinalizadoEm = DateTime.UtcNow
            }
        ]);
        return ordem;
    }
}
