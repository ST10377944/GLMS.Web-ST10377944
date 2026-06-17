using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ContractManagement.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AmountUSD = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountZAR = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExchangeRateUsed = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Contracts",
                columns: new[] { "Id", "Amount", "ClientName", "CreatedAt", "Description", "Status" },
                values: new object[,]
                {
                    { 1, 15000m, "Acme Corporation", new DateTime(2026, 6, 12, 6, 13, 48, 886, DateTimeKind.Utc).AddTicks(3882), "Website Development Project", "Pending" },
                    { 2, 25000m, "TechStart Solutions", new DateTime(2026, 6, 7, 6, 13, 48, 886, DateTimeKind.Utc).AddTicks(3895), "Mobile App Development", "Approved" },
                    { 3, 8000m, "Global Finance Inc", new DateTime(2026, 6, 14, 6, 13, 48, 886, DateTimeKind.Utc).AddTicks(3897), "Security Audit", "Declined" }
                });

            migrationBuilder.InsertData(
                table: "ServiceRequests",
                columns: new[] { "Id", "AmountUSD", "AmountZAR", "CompletedAt", "ContractId", "CreatedAt", "Description", "ExchangeRateUsed", "Status" },
                values: new object[,]
                {
                    { 1, 5000m, 97500m, new DateTime(2026, 6, 12, 6, 13, 48, 886, DateTimeKind.Utc).AddTicks(4089), 2, new DateTime(2026, 6, 9, 6, 13, 48, 886, DateTimeKind.Utc).AddTicks(4088), "Initial delivery of mobile app", 19.50m, "Completed" },
                    { 2, 3000m, 58500m, null, 2, new DateTime(2026, 6, 14, 6, 13, 48, 886, DateTimeKind.Utc).AddTicks(4095), "Second phase: UI improvements", 19.50m, "InProgress" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_ContractId",
                table: "ServiceRequests",
                column: "ContractId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceRequests");

            migrationBuilder.DropTable(
                name: "Contracts");
        }
    }
}
