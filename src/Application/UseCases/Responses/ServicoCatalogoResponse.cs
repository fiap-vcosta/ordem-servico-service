using Domain.ValueObjects;

namespace Application.UseCases.Responses;

public class ServicoCatalogoResponse
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Codigo { get; init; } = string.Empty;

    public static ServicoCatalogoResponse From(ServicoCatalogo servicoCatalogo)
    {
        return new ServicoCatalogoResponse
        {
            Id = servicoCatalogo.Id,
            Nome = servicoCatalogo.Nome,
            Codigo = servicoCatalogo.Codigo
        };
    }
}
