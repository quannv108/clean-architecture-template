using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "email_message",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    to = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    is_html = table.Column<bool>(type: "boolean", nullable: false),
                    from = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    cc = table.Column<string>(type: "jsonb", nullable: false),
                    bcc = table.Column<string>(type: "jsonb", nullable: false),
                    headers = table.Column<Dictionary<string, string>>(type: "jsonb", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_email_message", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_email_message_status",
                schema: "public",
                table: "email_message",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "email_message",
                schema: "public");
        }
    }
}
