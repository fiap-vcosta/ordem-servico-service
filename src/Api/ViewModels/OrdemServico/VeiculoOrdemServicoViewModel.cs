using Application.UseCases.Responses;

namespace Api.ViewModels.OrdemServico;

public record VeiculoOrdemServicoViewModel
{
    public required string Placa { get; init; }
    public required string Marca { get; init; }
    public required string Modelo { get; init; }

    public static VeiculoOrdemServicoViewModel From(VeiculoOrdemServicoResponse response)
    {
        return new VeiculoOrdemServicoViewModel
        {
            Placa = response.Placa,
            Marca = response.Marca,
            Modelo = response.Modelo
        };
    }
}
