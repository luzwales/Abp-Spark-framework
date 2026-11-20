using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Abp.Spark.Migrations
{
    /// <inheritdoc />
    public partial class Add_Customer_Module : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppCustomers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ShortName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TaxNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IndustryType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CreditLevel = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    SettlementMethod = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCustomers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppCustomers_Code",
                table: "AppCustomers",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_AppCustomers_Name",
                table: "AppCustomers",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppCustomers");
        }
    }
}
