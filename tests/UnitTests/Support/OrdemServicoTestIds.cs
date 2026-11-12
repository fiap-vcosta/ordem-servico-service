namespace UnitTests.Support;

public static class OrdemServicoTestIds
{
    public static Guid Of(int n) => new(n, 0, 0, new byte[8]);
}
