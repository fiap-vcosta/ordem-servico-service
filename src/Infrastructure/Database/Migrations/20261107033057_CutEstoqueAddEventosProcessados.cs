using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class CutEstoqueAddEventosProcessados : Migration
    {
        private static readonly string[] EventosProcessadosIndexColumns = ["Tipo", "ChaveNegocio"];

        private static readonly string[] ItensEstoqueColumns =
            ["Id", "Codigo", "Nome", "PrecoVenda", "Saldo", "SaldoReservado", "Tipo", "UnidadeMedida"];

        private static readonly object[,] ItensEstoqueValues =
        {
            { 1, "INS-001", "Óleo Motor Sintético 5W30", 45.90m, 50.000m, 0m, "Insumo", "Litro" },
            { 2, "INS-002", "Óleo Motor Semissintético 15W40", 38.50m, 0.000m, 0m, "Insumo", "Litro" },
            { 3, "PEC-001", "Filtro de Óleo", 35.00m, 15.000m, 0m, "Peca", "Unidade" },
            { 4, "PEC-002", "Filtro de Ar do Motor", 42.00m, 20.000m, 0m, "Peca", "Unidade" },
            { 5, "PEC-003", "Filtro de Cabine (Ar Condicionado)", 30.00m, 0.000m, 0m, "Peca", "Jogo" },
            { 6, "PEC-004", "Pastilha de Freio Dianteira", 120.00m, 8.000m, 0m, "Peca", "Jogo" },
            { 7, "PEC-005", "Pastilha de Freio Traseira", 95.00m, 3.000m, 0m, "Peca", "Jogo" },
            { 8, "INS-003", "Fluido de Freio DOT 4", 25.00m, 30.000m, 0m, "Insumo", "Frasco" },
            { 9, "INS-004", "Chumbo para Balanceamento", 18.00m, 5.500m, 0m, "Insumo", "Kg" },
            { 10, "INS-005", "Descarbonizante Spray", 22.00m, 10.000m, 0m, "Insumo", "Frasco" },
            { 11, "PEC-006", "Kit O-ring Bico Injetor", 15.00m, 25.000m, 0m, "Peca", "Jogo" },
            { 12, "PEC-016", "Correia Dentada", 85.00m, 6.000m, 0m, "Peca", "Unidade" },
            { 13, "PEC-017", "Tensor da Correia Dentada", 110.00m, 4.000m, 0m, "Peca", "Unidade" },
            { 14, "PEC-018", "Vela de Ignição", 140.00m, 0.000m, 0m, "Peca", "Jogo" },
            { 15, "PEC-007", "Bateria 60Ah", 350.00m, 4.000m, 0m, "Peca", "Unidade" },
            { 16, "PEC-008", "Bateria 45Ah", 280.00m, 0.000m, 0m, "Peca", "Unidade" },
            { 17, "PEC-009", "Terminal de Bateria", 12.00m, 40.000m, 0m, "Peca", "Par" },
            { 18, "PEC-014", "Lâmpada H4", 25.00m, 0.000m, 0m, "Peca", "Unidade" },
            { 19, "PEC-015", "Lâmpada Pingo T10", 10.00m, 50.000m, 0m, "Peca", "Par" },
            { 20, "INS-006", "Higienizador Granada", 28.00m, 18.000m, 0m, "Insumo", "Frasco" },
            { 21, "INS-007", "Gás Refrigerante R134a", 85.00m, 3.200m, 0m, "Insumo", "Kg" },
            { 22, "PEC-010", "Amortecedor Dianteiro", 450.00m, 2.000m, 0m, "Peca", "Par" },
            { 23, "PEC-011", "Kit Batente Amortecedor Dianteiro", 80.00m, 5.000m, 0m, "Peca", "Jogo" },
            { 24, "PEC-012", "Kit Embreagem Completo", 650.00m, 0.000m, 0m, "Peca", "Jogo" },
            { 25, "INS-008", "Fluido de Transmissão Manual", 55.00m, 12.000m, 0m, "Insumo", "Litro" },
            { 26, "PEC-013", "Palheta Limpador Parabrisa", 45.00m, 10.000m, 0m, "Peca", "Par" },
            { 27, "INS-009", "Aditivo Radiador Concentrado", 32.00m, 24.000m, 0m, "Insumo", "Litro" },
            { 28, "INS-010", "Água Desmineralizada", 8.00m, 60.000m, 0m, "Insumo", "Litro" },
            { 29, "INS-011", "Estopa Polimento", 15.00m, 2.500m, 0m, "Insumo", "Kg" },
            { 30, "INS-012", "Desengraxante Concentrado", 20.00m, 15.000m, 0m, "Insumo", "Litro" }
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItensEstoque");

            migrationBuilder.CreateTable(
                name: "EventosProcessados",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    ChaveNegocio = table.Column<string>(type: "text", nullable: false),
                    ProcessadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosProcessados", x => x.EventId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventosProcessados_Tipo_ChaveNegocio",
                table: "EventosProcessados",
                columns: EventosProcessadosIndexColumns,
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventosProcessados");

            migrationBuilder.CreateTable(
                name: "ItensEstoque",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "text", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    PrecoVenda = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Saldo = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
                    SaldoReservado = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    UnidadeMedida = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensEstoque", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ItensEstoque",
                columns: ItensEstoqueColumns,
                values: ItensEstoqueValues);

            migrationBuilder.CreateIndex(
                name: "IX_ItensEstoque_Codigo",
                table: "ItensEstoque",
                column: "Codigo",
                unique: true);
        }
    }
}
