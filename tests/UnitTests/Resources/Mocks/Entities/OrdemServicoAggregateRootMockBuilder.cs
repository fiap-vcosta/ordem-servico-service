using Domain.Entities;
using Domain.ValueObjects;

namespace UnitTests.Resources.Mocks.Entities;

public class OrdemServicoAggregateRootMockBuilder : IMockBuilder<OrdemServicoAggregateRoot>
{
    private readonly OrdemServicoAggregateRoot _ordemServico = OrdemServicoAggregateRoot.Criar(
        new ClienteOrdemServico { Id = 1, Nome = "Cliente", Email = "test@test.com" },
        new VeiculoOrdemServico { Placa = "ABC-1234", Marca = "Toyota", Modelo = "Corolla" }
    );

    public OrdemServicoAggregateRootMockBuilder WithStatus(StatusOrdemServico status)
    {
        _ordemServico
            .GetType()
            .GetProperty(nameof(OrdemServicoAggregateRoot.Status))?
            .SetValue(_ordemServico, status);

        return this;
    } 
    
    public OrdemServicoAggregateRoot Build()
    {
        return _ordemServico;
    }
}