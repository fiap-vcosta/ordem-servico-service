using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class CutAdminAddClienteDocumento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Servicos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Veiculos");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.AddColumn<string>(
                name: "Cliente_Documento",
                table: "OrdensServico",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cliente_Documento",
                table: "OrdensServico");

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Documento = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    TipoDocumento = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Servicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Codigo = table.Column<string>(type: "text", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    PrecoPadrao = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Login = table.Column<string>(type: "text", nullable: false),
                    Senha = table.Column<string>(type: "text", nullable: false),
                    TipoUsuario = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Veiculos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdCliente = table.Column<int>(type: "integer", nullable: false),
                    Marca = table.Column<string>(type: "text", nullable: false),
                    Modelo = table.Column<string>(type: "text", nullable: false),
                    Placa = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Veiculos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Veiculos_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "Documento", "Email", "Nome", "TipoDocumento" },
                values: new object[,]
                {
                    { 1, "86421537090", "joao.silva@email.com", "João Silva", "Cpf" },
                    { 2, "71284693031", "maria.oliveira@email.com", "Maria Oliveira", "Cpf" },
                    { 3, "15350946056", "carlos.santos@email.com", "Carlos Santos", "Cpf" },
                    { 4, "23100296028", "ana.costa@email.com", "Ana Costa", "Cpf" },
                    { 5, "04531783085", "lucas.ferreira@email.com", "Lucas Ferreira", "Cpf" },
                    { 6, "11222333000181", "contato@autopecascentral.com.br", "Auto Peças Central Ltda", "Cnpj" },
                    { 7, "11444777000161", "oficinatoninho@email.com", "Oficina do Toninho ME", "Cnpj" },
                    { 8, "12345678000195", "logistica@rapidosul.com.br", "Transportadora Rápido Sul", "Cnpj" },
                    { 9, "27865757000102", "frota@xyzlocadora.com.br", "Locadora de Veículos XYZ", "Cnpj" },
                    { 10, "60746948000112", "vendas@bateriaspotencia.com.br", "Comercial de Baterias Potência", "Cnpj" }
                });

            migrationBuilder.InsertData(
                table: "Servicos",
                columns: new[] { "Id", "Ativo", "Codigo", "Nome", "PrecoPadrao" },
                values: new object[,]
                {
                    { 1, true, "MTR-001", "Troca de Óleo do Motor", 150.00m },
                    { 2, true, "FLT-002", "Troca de Filtro de Ar", 50.00m },
                    { 3, true, "FRE-003", "Troca de Pastilhas de Freio", 200.00m },
                    { 4, true, "SUS-004", "Alinhamento e Balanceamento", 120.00m },
                    { 5, true, "MTR-005", "Limpeza de Bicos Injetores", 180.00m },
                    { 6, true, "ELT-006", "Troca de Bateria", 80.00m },
                    { 7, true, "ARC-007", "Higienização de Ar Condicionado", 130.00m },
                    { 8, true, "SUS-008", "Troca de Amortecedores Dianteiros", 300.00m },
                    { 9, true, "EMB-009", "Substituição do Kit de Embreagem", 600.00m },
                    { 10, true, "GRL-010", "Revisão Preventiva Geral", 250.00m }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Login", "Senha", "TipoUsuario" },
                values: new object[] { 1, "admin", "8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918", "Admin" });

            migrationBuilder.InsertData(
                table: "Veiculos",
                columns: new[] { "Id", "IdCliente", "Marca", "Modelo", "Placa" },
                values: new object[,]
                {
                    { 1, 1, "Chevrolet", "Onix", "ABC1234" },
                    { 2, 1, "Volkswagen", "Gol", "XYZ1A23" },
                    { 3, 1, "Honda", "Civic", "DEF5678" },
                    { 4, 2, "Toyota", "Corolla", "GHI9B12" },
                    { 5, 2, "Fiat", "Argo", "JKL3456" },
                    { 6, 3, "Hyundai", "HB20", "MNO4C56" },
                    { 7, 3, "Ford", "Ka", "PQR7890" },
                    { 8, 3, "Renault", "Kwid", "STU5D78" },
                    { 9, 4, "Jeep", "Compass", "VWX1234" },
                    { 10, 5, "Nissan", "Kicks", "YZA6E90" },
                    { 11, 5, "Chevrolet", "Tracker", "BCD5678" },
                    { 12, 6, "Volkswagen", "Saveiro", "EFG7F12" },
                    { 13, 6, "Fiat", "Fiorino", "HIJ9012" },
                    { 14, 6, "Peugeot", "Partner", "KLM8G34" },
                    { 15, 7, "Chevrolet", "Montana", "NOP3456" },
                    { 16, 8, "Mercedes-Benz", "Sprinter", "QRS9H56" },
                    { 17, 8, "Iveco", "Daily", "TUV7890" },
                    { 18, 8, "Ford", "Transit", "WXY0I78" },
                    { 19, 9, "Fiat", "Mobi", "ZAB1234" },
                    { 20, 10, "Volkswagen", "Kombi", "CDE1J90" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Documento",
                table: "Clientes",
                column: "Documento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_TipoDocumento",
                table: "Clientes",
                column: "TipoDocumento");

            migrationBuilder.CreateIndex(
                name: "IX_Servicos_Codigo",
                table: "Servicos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Login",
                table: "Usuarios",
                column: "Login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Veiculos_IdCliente",
                table: "Veiculos",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_Veiculos_Placa",
                table: "Veiculos",
                column: "Placa",
                unique: true);
        }
    }
}
