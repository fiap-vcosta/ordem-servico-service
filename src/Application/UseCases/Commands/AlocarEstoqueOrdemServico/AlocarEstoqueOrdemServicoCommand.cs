using MediatR;

namespace Application.UseCases.Commands.AlocarEstoqueOrdemServico;

public record AlocarEstoqueOrdemServicoCommand(int IdOrdemServico) : IRequest<Unit>;