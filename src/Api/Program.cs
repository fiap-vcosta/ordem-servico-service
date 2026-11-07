using Api.Extensions;
using BuildingBlocks.Http.Logging;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    if (builder.Environment.IsDevelopment())
    {
        TryLoadLocalJwtPublicKey(builder);
    }

    builder.Host.UseSerilog(DatadogLogging.Configure);

    builder.Services.AddApiServices(builder.Configuration);

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name);
    });

    var app = builder.Build();
    app.UseApiConfiguration();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }

    app.MapGet("/ordem-servico/health", async (AppDbContext appDbContext) =>
    {
        var isDbOk = await appDbContext.Database.CanConnectAsync();
        return isDbOk ? Results.Ok("OK") : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    });

    app.Run();

    static void TryLoadLocalJwtPublicKey(WebApplicationBuilder webApplicationBuilder)
    {
        var publicKeyPath = Path.GetFullPath(
            Path.Combine(webApplicationBuilder.Environment.ContentRootPath, "..", "..", ".local", "jwt", "public.pem"));
        if (!File.Exists(publicKeyPath))
        {
            return;
        }

        var publicPem = File.ReadAllText(publicKeyPath);
        webApplicationBuilder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Funcionario:PublicKeyPem"] = publicPem,
            ["Jwt:Cliente:PublicKeyPem"] = publicPem
        });
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação encerrada inesperadamente");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
