using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Audit.Infrastructure.Persistence.ArchiveMigrations
{
    /// <inheritdoc />
    public partial class InitialArchiveDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "archived_audit_events",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SourceEventId = table.Column<long>(type: "bigint", nullable: false),
                    SourceSequence = table.Column<long>(type: "bigint", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IngestedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ActorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ActorEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangeTypeCode = table.Column<int>(type: "int", nullable: false),
                    EntityTypeCode = table.Column<int>(type: "int", nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PrimaryKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BeforeJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfterJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangedFieldsJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_archived_audit_events", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "archived_contract_aliases",
                columns: table => new
                {
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Field = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_archived_contract_aliases", x => new { x.OrganizationId, x.ContractId, x.Field, x.Value });
                });

            migrationBuilder.CreateTable(
                name: "archived_contracts",
                columns: table => new
                {
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArchivedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastActivityAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastSourceSequence = table.Column<long>(type: "bigint", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    InternalNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ContractorName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_archived_contracts", x => new { x.OrganizationId, x.ContractId });
                });

            migrationBuilder.CreateTable(
                name: "archived_timeline_items",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceSequence = table.Column<long>(type: "bigint", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangeTypeCode = table.Column<int>(type: "int", nullable: false),
                    ChangeKind = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    EntityTypeCode = table.Column<int>(type: "int", nullable: false),
                    EntityKind = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Actor = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    ChangesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataQualityIssuesJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_archived_timeline_items", x => x.EventId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_archived_audit_events_OrganizationId_Source_SourceEventId",
                table: "archived_audit_events",
                columns: new[] { "OrganizationId", "Source", "SourceEventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_archived_audit_events_OrganizationId_SourceSequence",
                table: "archived_audit_events",
                columns: new[] { "OrganizationId", "SourceSequence" });

            migrationBuilder.CreateIndex(
                name: "IX_archived_contract_aliases_OrganizationId_Value",
                table: "archived_contract_aliases",
                columns: new[] { "OrganizationId", "Value" });

            migrationBuilder.CreateIndex(
                name: "IX_archived_contracts_InternalNumber",
                table: "archived_contracts",
                column: "InternalNumber");

            migrationBuilder.CreateIndex(
                name: "IX_archived_contracts_Number",
                table: "archived_contracts",
                column: "Number");

            migrationBuilder.CreateIndex(
                name: "IX_archived_contracts_OrganizationId_LastActivityAt",
                table: "archived_contracts",
                columns: new[] { "OrganizationId", "LastActivityAt" });

            migrationBuilder.CreateIndex(
                name: "IX_archived_timeline_items_OrganizationId_ContractId_SourceSequence",
                table: "archived_timeline_items",
                columns: new[] { "OrganizationId", "ContractId", "SourceSequence" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "archived_audit_events");

            migrationBuilder.DropTable(
                name: "archived_contract_aliases");

            migrationBuilder.DropTable(
                name: "archived_contracts");

            migrationBuilder.DropTable(
                name: "archived_timeline_items");
        }
    }
}
