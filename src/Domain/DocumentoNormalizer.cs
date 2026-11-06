namespace Domain;

public static class DocumentoNormalizer
{
    public static string Normalize(string documento) =>
        documento.Replace(".", "").Replace("-", "").Replace("/", "").Trim();
}
