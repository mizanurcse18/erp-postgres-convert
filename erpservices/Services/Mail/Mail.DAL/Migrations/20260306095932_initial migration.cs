using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Mail.DAL.Migrations
{
    /// <inheritdoc />
    public partial class initialmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_log",
                columns: table => new
                {
                    audit_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    table_name = table.Column<string>(type: "text", nullable: false),
                    audit_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    key_values = table.Column<string>(type: "text", nullable: false),
                    old_values = table.Column<string>(type: "text", nullable: true),
                    new_values = table.Column<string>(type: "text", nullable: true),
                    row_state = table.Column<string>(type: "text", nullable: false),
                    company_id = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_ip = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_log", x => x.audit_id);
                });

            migrationBuilder.CreateTable(
                name: "mail_configuration",
                columns: table => new
                {
                    config_id = table.Column<int>(type: "integer", nullable: false),
                    config_name = table.Column<string>(type: "text", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false),
                    port = table.Column<int>(type: "integer", nullable: false),
                    user_name = table.Column<string>(type: "text", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    seq_no = table.Column<decimal>(type: "numeric", nullable: false),
                    enable_ssl = table.Column<bool>(type: "boolean", nullable: false),
                    timeout = table.Column<int>(type: "integer", nullable: false),
                    sleep_time = table.Column<int>(type: "integer", nullable: false),
                    company_id = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_ip = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_ip = table.Column<string>(type: "text", nullable: true),
                    row_version = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mail_configuration", x => x.config_id);
                });

            migrationBuilder.CreateTable(
                name: "mail_group_setup",
                columns: table => new
                {
                    group_id = table.Column<int>(type: "integer", nullable: false),
                    group_name = table.Column<string>(type: "text", nullable: false),
                    config_id = table.Column<int>(type: "integer", nullable: false),
                    attachment_path = table.Column<string>(type: "text", nullable: true),
                    subject = table.Column<string>(type: "text", nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    sensitivity = table.Column<int>(type: "integer", nullable: false),
                    report_gen_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    mail_gen_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    interval_on = table.Column<string>(type: "text", nullable: true),
                    interval_value = table.Column<decimal>(type: "numeric", nullable: false),
                    is_from_interface = table.Column<bool>(type: "boolean", nullable: false),
                    company_id = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_ip = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_ip = table.Column<string>(type: "text", nullable: true),
                    row_version = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mail_group_setup", x => x.group_id);
                });

            migrationBuilder.CreateTable(
                name: "mail_setup",
                columns: table => new
                {
                    mail_id = table.Column<int>(type: "integer", nullable: false),
                    group_id = table.Column<int>(type: "integer", nullable: false),
                    to_cc_bcc = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    company_id = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_ip = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_ip = table.Column<string>(type: "text", nullable: true),
                    row_version = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mail_setup", x => x.mail_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "mail_configuration");

            migrationBuilder.DropTable(
                name: "mail_group_setup");

            migrationBuilder.DropTable(
                name: "mail_setup");
        }
    }
}
