using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using BuildingBlocks.Http.Auth;
using Microsoft.IdentityModel.Tokens;

namespace IntegrationTests.Infrastructure;

public static class TestJwt
{
    public static readonly RSA Rsa = RSA.Create(2048);
    public static readonly string Issuer = "ordem-servico-integration";
    public static readonly string FuncionarioAudience = "funcionario";
    public static readonly string ClienteAudience = "cliente";

    public static string PublicKeyPem
    {
        get
        {
            var spki = Rsa.ExportSubjectPublicKeyInfo();
            var b64 = Convert.ToBase64String(spki);
            return $"-----BEGIN PUBLIC KEY-----\n{Chunk(b64)}\n-----END PUBLIC KEY-----";
        }
    }

    public static string CreateStaffToken()
    {
        var key = new RsaSecurityKey(Rsa);
        var creds = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: FuncionarioAudience,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, "integration-staff"),
                new Claim(ClaimTypes.Role, "Admin")
            ],
            notBefore: now,
            expires: now.AddHours(1),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string CreateClienteToken(string documento)
    {
        var key = new RsaSecurityKey(Rsa);
        var creds = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: ClienteAudience,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, documento),
                new Claim(ClienteJwtClaims.Documento, documento)
            ],
            notBefore: now,
            expires: now.AddHours(1),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string Chunk(string value)
    {
        return string.Join("\n", Enumerable.Range(0, (value.Length + 63) / 64)
            .Select(i => value.Substring(i * 64, Math.Min(64, value.Length - i * 64))));
    }
}
