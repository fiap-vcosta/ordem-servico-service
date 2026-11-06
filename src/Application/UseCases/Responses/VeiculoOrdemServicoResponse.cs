using Domain.ValueObjects;

namespace Application.UseCases.Responses;

public class VeiculoOrdemServicoResponse
{
    public string Placa { get; init; } = string.Empty;
    public string Marca { get; init; } = string.Empty;
    public string Modelo { get; init; } = string.Empty;

    public static VeiculoOrdemServicoResponse From(VeiculoOrdemServico veiculo)
    {
        return new VeiculoOrdemServicoResponse
        {
            Placa = veiculo.Placa,
            Marca = veiculo.Marca,
            Modelo = veiculo.Modelo
        };
    }
}
