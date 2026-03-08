using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Approval.DAL.Migrations
{
    /// <inheritdoc />
    public partial class initialmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "approval_employee_feedback",
                columns: table => new
                {
                    apemployee_feedback_id = table.Column<int>(type: "integer", nullable: false),
                    approval_process_id = table.Column<int>(type: "integer", nullable: false),
                    division_id = table.Column<int>(type: "integer", nullable: false),
                    department_id = table.Column<int>(type: "integer", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    sequence_no = table.Column<decimal>(type: "numeric", nullable: false),
                    apfeedback_id = table.Column<int>(type: "integer", nullable: false),
                    feedback_request_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    feedback_last_response_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    feedback_submit_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    proxy_employee_id = table.Column<int>(type: "integer", nullable: true),
                    is_proxy_employee_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    nfaapproval_sequence_type = table.Column<int>(type: "integer", nullable: true),
                    is_editable = table.Column<bool>(type: "boolean", nullable: false),
                    is_auto_approved = table.Column<bool>(type: "boolean", nullable: false),
                    is_scm = table.Column<bool>(type: "boolean", nullable: false),
                    is_multi_proxy = table.Column<bool>(type: "boolean", nullable: false),
                    particulars = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_approval_employee_feedback", x => x.apemployee_feedback_id);
                });

            migrationBuilder.CreateTable(
                name: "approval_employee_feedback_remarks",
                columns: table => new
                {
                    apemployee_feedback_remarks_id = table.Column<int>(type: "integer", nullable: false),
                    approval_process_id = table.Column<int>(type: "integer", nullable: false),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    remarks_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    apfeedback_id = table.Column<int>(type: "integer", nullable: false),
                    proxy_employee_id = table.Column<int>(type: "integer", nullable: true),
                    is_proxy_employee_remarks = table.Column<bool>(type: "boolean", nullable: false),
                    proxy_employee_remarks = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_approval_employee_feedback_remarks", x => x.apemployee_feedback_remarks_id);
                });

            migrationBuilder.CreateTable(
                name: "approval_feedback",
                columns: table => new
                {
                    apfeedback_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    hex_color_code = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_approval_feedback", x => x.apfeedback_id);
                });

            migrationBuilder.CreateTable(
                name: "approval_forward_info",
                columns: table => new
                {
                    apforward_info_id = table.Column<int>(type: "integer", nullable: false),
                    employee_feedback_id = table.Column<int>(type: "integer", nullable: false),
                    approval_process_id = table.Column<int>(type: "integer", nullable: false),
                    employee_feedback_remarks_id = table.Column<int>(type: "integer", nullable: false),
                    division_id = table.Column<int>(type: "integer", nullable: false),
                    department_id = table.Column<int>(type: "integer", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    comment_request_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    comment_submit_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    apemployee_comment = table.Column<string>(type: "text", nullable: true),
                    apforward_employee_comment = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_approval_forward_info", x => x.apforward_info_id);
                });

            migrationBuilder.CreateTable(
                name: "approval_multi_proxy_employee_info",
                columns: table => new
                {
                    ampeiid = table.Column<long>(type: "bigint", nullable: false),
                    apemployee_feedback_id = table.Column<int>(type: "integer", nullable: false),
                    approval_process_id = table.Column<int>(type: "integer", nullable: false),
                    division_id = table.Column<int>(type: "integer", nullable: false),
                    department_id = table.Column<int>(type: "integer", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_approval_multi_proxy_employee_info", x => x.ampeiid);
                });

            migrationBuilder.CreateTable(
                name: "approval_panel",
                columns: table => new
                {
                    appanel_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    aptype_id = table.Column<int>(type: "integer", nullable: true),
                    is_parallel_approval = table.Column<bool>(type: "boolean", nullable: false),
                    is_dynamic_approval = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_approval_panel", x => x.appanel_id);
                });

            migrationBuilder.CreateTable(
                name: "approval_panel_employee",
                columns: table => new
                {
                    appanel_employee_id = table.Column<int>(type: "integer", nullable: false),
                    division_id = table.Column<int>(type: "integer", nullable: false),
                    department_id = table.Column<int>(type: "integer", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    appanel_id = table.Column<int>(type: "integer", nullable: false),
                    sequence_no = table.Column<decimal>(type: "numeric", nullable: false),
                    proxy_employee_id = table.Column<int>(type: "integer", nullable: true),
                    is_proxy_employee_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    nfaapproval_sequence_type = table.Column<int>(type: "integer", nullable: true),
                    is_editable = table.Column<bool>(type: "boolean", nullable: false),
                    is_scm = table.Column<bool>(type: "boolean", nullable: false),
                    is_multi_proxy = table.Column<bool>(type: "boolean", nullable: false),
                    particulars = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_approval_panel_employee", x => x.appanel_employee_id);
                });

            migrationBuilder.CreateTable(
                name: "approval_panel_employee_config",
                columns: table => new
                {
                    appeconfig_id = table.Column<int>(type: "integer", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    appanel_id = table.Column<int>(type: "integer", nullable: false),
                    sequence_no = table.Column<decimal>(type: "numeric", nullable: false),
                    proxy_employee_id = table.Column<int>(type: "integer", nullable: true),
                    is_proxy_employee_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    nfaapproval_sequence_type = table.Column<int>(type: "integer", nullable: true),
                    is_editable = table.Column<bool>(type: "boolean", nullable: false),
                    is_multi_proxy = table.Column<bool>(type: "boolean", nullable: false),
                    template_id = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("pk_approval_panel_employee_config", x => x.appeconfig_id);
                });

            migrationBuilder.CreateTable(
                name: "approval_panel_forward_employee",
                columns: table => new
                {
                    appanel_forward_employee_id = table.Column<int>(type: "integer", nullable: false),
                    appanel_id = table.Column<int>(type: "integer", nullable: false),
                    division_id = table.Column<int>(type: "integer", nullable: false),
                    department_id = table.Column<int>(type: "integer", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_approval_panel_forward_employee", x => x.appanel_forward_employee_id);
                });

            migrationBuilder.CreateTable(
                name: "approval_panel_proxy_employee",
                columns: table => new
                {
                    appanel_proxy_employee_id = table.Column<int>(type: "integer", nullable: false),
                    appanel_employee_id = table.Column<int>(type: "integer", nullable: false),
                    appanel_id = table.Column<int>(type: "integer", nullable: false),
                    division_id = table.Column<int>(type: "integer", nullable: false),
                    department_id = table.Column<int>(type: "integer", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_approval_panel_proxy_employee", x => x.appanel_proxy_employee_id);
                });

            migrationBuilder.CreateTable(
                name: "approval_panel_proxy_employee_config",
                columns: table => new
                {
                    apppecid = table.Column<int>(type: "integer", nullable: false),
                    appeconfig_id = table.Column<int>(type: "integer", nullable: false),
                    appanel_id = table.Column<int>(type: "integer", nullable: false),
                    division_id = table.Column<int>(type: "integer", nullable: false),
                    department_id = table.Column<int>(type: "integer", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_approval_panel_proxy_employee_config", x => x.apppecid);
                });

            migrationBuilder.CreateTable(
                name: "approval_process",
                columns: table => new
                {
                    approval_process_id = table.Column<int>(type: "integer", nullable: false),
                    aptype_id = table.Column<int>(type: "integer", nullable: false),
                    reference_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    apstatus_id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_approval_process", x => x.approval_process_id);
                });

            migrationBuilder.CreateTable(
                name: "approval_process_panel_map",
                columns: table => new
                {
                    approval_process_panel_map_id = table.Column<int>(type: "integer", nullable: false),
                    approval_process_id = table.Column<int>(type: "integer", nullable: false),
                    appanel_id = table.Column<int>(type: "integer", nullable: false),
                    remarks = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_approval_process_panel_map", x => x.approval_process_panel_map_id);
                });

            migrationBuilder.CreateTable(
                name: "approval_type",
                columns: table => new
                {
                    aptype_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_approval_type", x => x.aptype_id);
                });

            migrationBuilder.CreateTable(
                name: "apstatus",
                columns: table => new
                {
                    apstatus_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_apstatus", x => x.apstatus_id);
                });

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
                name: "doaapproval_panel_employee",
                columns: table => new
                {
                    doaapproval_panel_employee_id = table.Column<long>(type: "bigint", nullable: false),
                    doamaster_id = table.Column<long>(type: "bigint", nullable: false),
                    assignee_employee_id = table.Column<long>(type: "bigint", nullable: false),
                    type_id = table.Column<int>(type: "integer", nullable: false),
                    appanel_id = table.Column<int>(type: "integer", nullable: false),
                    group_id = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("pk_doaapproval_panel_employee", x => x.doaapproval_panel_employee_id);
                });

            migrationBuilder.CreateTable(
                name: "doamaster",
                columns: table => new
                {
                    doamaster_id = table.Column<long>(type: "bigint", nullable: false),
                    employee_id = table.Column<long>(type: "bigint", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status_id = table.Column<int>(type: "integer", nullable: false),
                    remarks = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_doamaster", x => x.doamaster_id);
                });

            migrationBuilder.CreateTable(
                name: "document_approval_master",
                columns: table => new
                {
                    damid = table.Column<long>(type: "bigint", nullable: false),
                    request_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reference_no = table.Column<string>(type: "text", nullable: true),
                    reference_keyword = table.Column<string>(type: "text", nullable: true),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
                    template_id = table.Column<int>(type: "integer", nullable: false),
                    template_body = table.Column<string>(type: "text", nullable: true),
                    external_id = table.Column<string>(type: "text", nullable: true),
                    is_draft = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_document_approval_master", x => x.damid);
                });

            migrationBuilder.CreateTable(
                name: "document_approval_template",
                columns: table => new
                {
                    datid = table.Column<long>(type: "bigint", nullable: false),
                    datname = table.Column<string>(type: "text", nullable: true),
                    template_body = table.Column<string>(type: "text", nullable: true),
                    category_type = table.Column<int>(type: "integer", nullable: false),
                    keywords = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_document_approval_template", x => x.datid);
                });

            migrationBuilder.CreateTable(
                name: "dynamic_approval_panel_employee",
                columns: table => new
                {
                    dapeid = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    hierarchy_level = table.Column<int>(type: "integer", nullable: false),
                    maximum_job_grade = table.Column<int>(type: "integer", nullable: false),
                    division_ids = table.Column<string>(type: "text", nullable: false),
                    department_ids = table.Column<string>(type: "text", nullable: true),
                    employee_ids = table.Column<string>(type: "text", nullable: true),
                    include_hr = table.Column<bool>(type: "boolean", nullable: false),
                    hremployee_id = table.Column<int>(type: "integer", nullable: false),
                    hrproxy_employee_ids = table.Column<string>(type: "text", nullable: true),
                    min_limit_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    max_limit_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    include_division_head = table.Column<bool>(type: "boolean", nullable: false),
                    include_department_head = table.Column<bool>(type: "boolean", nullable: false),
                    approval_panels = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    external_id = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_dynamic_approval_panel_employee", x => x.dapeid);
                });

            migrationBuilder.CreateTable(
                name: "manual_approval_panel_employee",
                columns: table => new
                {
                    mappanel_employee_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    appanel_id = table.Column<int>(type: "integer", nullable: false),
                    sequence_no = table.Column<decimal>(type: "numeric", nullable: false),
                    proxy_employee_id = table.Column<int>(type: "integer", nullable: true),
                    is_proxy_employee_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    nfaapproval_sequence_type = table.Column<int>(type: "integer", nullable: true),
                    is_editable = table.Column<bool>(type: "boolean", nullable: false),
                    is_scm = table.Column<bool>(type: "boolean", nullable: false),
                    is_multi_proxy = table.Column<bool>(type: "boolean", nullable: false),
                    aptype_id = table.Column<int>(type: "integer", nullable: false),
                    reference_id = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("pk_manual_approval_panel_employee", x => x.mappanel_employee_id);
                });

            migrationBuilder.CreateTable(
                name: "manual_approval_panel_proxy_employee",
                columns: table => new
                {
                    mappanel_proxy_employee_id = table.Column<int>(type: "integer", nullable: false),
                    mappanel_employee_id = table.Column<int>(type: "integer", nullable: false),
                    appanel_id = table.Column<int>(type: "integer", nullable: false),
                    division_id = table.Column<int>(type: "integer", nullable: false),
                    department_id = table.Column<int>(type: "integer", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_manual_approval_panel_proxy_employee", x => x.mappanel_proxy_employee_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "approval_employee_feedback");

            migrationBuilder.DropTable(
                name: "approval_employee_feedback_remarks");

            migrationBuilder.DropTable(
                name: "approval_feedback");

            migrationBuilder.DropTable(
                name: "approval_forward_info");

            migrationBuilder.DropTable(
                name: "approval_multi_proxy_employee_info");

            migrationBuilder.DropTable(
                name: "approval_panel");

            migrationBuilder.DropTable(
                name: "approval_panel_employee");

            migrationBuilder.DropTable(
                name: "approval_panel_employee_config");

            migrationBuilder.DropTable(
                name: "approval_panel_forward_employee");

            migrationBuilder.DropTable(
                name: "approval_panel_proxy_employee");

            migrationBuilder.DropTable(
                name: "approval_panel_proxy_employee_config");

            migrationBuilder.DropTable(
                name: "approval_process");

            migrationBuilder.DropTable(
                name: "approval_process_panel_map");

            migrationBuilder.DropTable(
                name: "approval_type");

            migrationBuilder.DropTable(
                name: "apstatus");

            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "doaapproval_panel_employee");

            migrationBuilder.DropTable(
                name: "doamaster");

            migrationBuilder.DropTable(
                name: "document_approval_master");

            migrationBuilder.DropTable(
                name: "document_approval_template");

            migrationBuilder.DropTable(
                name: "dynamic_approval_panel_employee");

            migrationBuilder.DropTable(
                name: "manual_approval_panel_employee");

            migrationBuilder.DropTable(
                name: "manual_approval_panel_proxy_employee");
        }
    }
}
