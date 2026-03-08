using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HRMS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class initialmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "access_request_category_child",
                columns: table => new
                {
                    access_rccid = table.Column<int>(type: "integer", nullable: false),
                    srmid = table.Column<int>(type: "integer", nullable: false),
                    access_types_ids = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("pk_access_request_category_child", x => x.access_rccid);
                });

            migrationBuilder.CreateTable(
                name: "accessories_requisition_category_child",
                columns: table => new
                {
                    accessories_rccid = table.Column<int>(type: "integer", nullable: false),
                    srmid = table.Column<int>(type: "integer", nullable: false),
                    item_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("pk_accessories_requisition_category_child", x => x.accessories_rccid);
                });

            migrationBuilder.CreateTable(
                name: "annual_leave_encashment_master",
                columns: table => new
                {
                    alemaster_id = table.Column<int>(type: "integer", nullable: false),
                    alewmaster_id = table.Column<long>(type: "bigint", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    total_leave_balance_days = table.Column<decimal>(type: "numeric", nullable: false),
                    encashed_leave_days = table.Column<decimal>(type: "numeric", nullable: false),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_annual_leave_encashment_master", x => x.alemaster_id);
                });

            migrationBuilder.CreateTable(
                name: "annual_leave_encashment_policy_settings",
                columns: table => new
                {
                    alepsid = table.Column<int>(type: "integer", nullable: false),
                    hierarchy_level = table.Column<int>(type: "integer", nullable: false),
                    maximum_job_grade = table.Column<int>(type: "integer", nullable: false),
                    include_hr = table.Column<bool>(type: "boolean", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    proxy_employee_ids = table.Column<string>(type: "text", nullable: true),
                    max_encashable_percent = table.Column<decimal>(type: "numeric", nullable: false),
                    max_encashable_days = table.Column<decimal>(type: "numeric", nullable: false),
                    cut_off_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("pk_annual_leave_encashment_policy_settings", x => x.alepsid);
                });

            migrationBuilder.CreateTable(
                name: "annual_leave_encashment_window_child",
                columns: table => new
                {
                    alechild_id = table.Column<int>(type: "integer", nullable: false),
                    alewmaster_id = table.Column<long>(type: "bigint", nullable: false),
                    employee_id = table.Column<long>(type: "bigint", nullable: false),
                    is_mail_sent = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_annual_leave_encashment_window_child", x => x.alechild_id);
                });

            migrationBuilder.CreateTable(
                name: "annual_leave_encashment_window_master",
                columns: table => new
                {
                    alewmaster_id = table.Column<long>(type: "bigint", nullable: false),
                    financial_year_id = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    division_ids = table.Column<string>(type: "text", nullable: false),
                    department_ids = table.Column<string>(type: "text", nullable: true),
                    employee_type_ids = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_annual_leave_encashment_window_master", x => x.alewmaster_id);
                });

            migrationBuilder.CreateTable(
                name: "asset_requisition_category_child",
                columns: table => new
                {
                    asset_rcid = table.Column<int>(type: "integer", nullable: false),
                    srmid = table.Column<int>(type: "integer", nullable: false),
                    item_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("pk_asset_requisition_category_child", x => x.asset_rcid);
                });

            migrationBuilder.CreateTable(
                name: "attendance_summary",
                columns: table => new
                {
                    primary_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    employee_code = table.Column<string>(type: "text", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    attendance_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    in_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    out_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    total_time_in_min = table.Column<int>(type: "integer", nullable: false),
                    late_in_min = table.Column<int>(type: "integer", nullable: false),
                    over_time_in_min = table.Column<int>(type: "integer", nullable: false),
                    attendance_status = table.Column<int>(type: "integer", nullable: false),
                    card_no = table.Column<string>(type: "text", nullable: true),
                    shift_id = table.Column<int>(type: "integer", nullable: false),
                    leave_category_id = table.Column<int>(type: "integer", nullable: true),
                    is_edited = table.Column<bool>(type: "boolean", nullable: false),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    hrnote = table.Column<string>(type: "text", nullable: true),
                    approval_status_id = table.Column<int>(type: "integer", nullable: true),
                    day_status = table.Column<string>(type: "text", nullable: true),
                    actual_working_hour_in_min = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("pk_attendance_summary", x => x.primary_id);
                });

            migrationBuilder.CreateTable(
                name: "audit_approval_config",
                columns: table => new
                {
                    map_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    question_ids = table.Column<string>(type: "text", nullable: false),
                    department_ids = table.Column<string>(type: "text", nullable: false),
                    department_emails = table.Column<string>(type: "text", nullable: false),
                    external_id = table.Column<int>(type: "integer", nullable: true),
                    external_properties = table.Column<string>(type: "text", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    is_posmrequired = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_audit_approval_config", x => x.map_id);
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
                name: "audit_question",
                columns: table => new
                {
                    question_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    external_id = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_audit_question", x => x.question_id);
                });

            migrationBuilder.CreateTable(
                name: "branch_info",
                columns: table => new
                {
                    branch_id = table.Column<int>(type: "integer", nullable: false),
                    branch_name = table.Column<string>(type: "text", nullable: false),
                    branch_code = table.Column<string>(type: "text", nullable: true),
                    permanent_address = table.Column<string>(type: "text", nullable: true),
                    current_address = table.Column<string>(type: "text", nullable: true),
                    branch_email = table.Column<string>(type: "text", nullable: true),
                    region_id = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("pk_branch_info", x => x.branch_id);
                });

            migrationBuilder.CreateTable(
                name: "cluster",
                columns: table => new
                {
                    cluster_id = table.Column<int>(type: "integer", nullable: false),
                    cluster_name = table.Column<string>(type: "text", nullable: false),
                    cluster_code = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_cluster", x => x.cluster_id);
                });

            migrationBuilder.CreateTable(
                name: "company_leave_policy",
                columns: table => new
                {
                    clpolicy_id = table.Column<int>(type: "integer", nullable: false),
                    financial_year_id = table.Column<int>(type: "integer", nullable: false),
                    employee_status_id = table.Column<int>(type: "integer", nullable: false),
                    leave_category_id = table.Column<int>(type: "integer", nullable: false),
                    leave_in_days = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("pk_company_leave_policy", x => x.clpolicy_id);
                });

            migrationBuilder.CreateTable(
                name: "department",
                columns: table => new
                {
                    department_id = table.Column<int>(type: "integer", nullable: false),
                    department_name = table.Column<string>(type: "text", nullable: false),
                    department_code = table.Column<string>(type: "text", nullable: true),
                    division_id = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_department", x => x.department_id);
                });

            migrationBuilder.CreateTable(
                name: "department_head_map",
                columns: table => new
                {
                    department_hmap_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    table.PrimaryKey("pk_department_head_map", x => x.department_hmap_id);
                });

            migrationBuilder.CreateTable(
                name: "designation",
                columns: table => new
                {
                    designation_id = table.Column<int>(type: "integer", nullable: false),
                    designation_name = table.Column<string>(type: "text", nullable: false),
                    designation_code = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_designation", x => x.designation_id);
                });

            migrationBuilder.CreateTable(
                name: "division",
                columns: table => new
                {
                    division_id = table.Column<int>(type: "integer", nullable: false),
                    division_name = table.Column<string>(type: "text", nullable: false),
                    division_code = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_division", x => x.division_id);
                });

            migrationBuilder.CreateTable(
                name: "division_head_map",
                columns: table => new
                {
                    dhmap_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    division_id = table.Column<int>(type: "integer", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    budget_amount = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("pk_division_head_map", x => x.dhmap_id);
                });

            migrationBuilder.CreateTable(
                name: "document_upload",
                columns: table => new
                {
                    duid = table.Column<int>(type: "integer", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    tinnumber = table.Column<string>(type: "text", nullable: false),
                    document_type_id = table.Column<int>(type: "integer", nullable: false),
                    income_year = table.Column<int>(type: "integer", nullable: false),
                    assessment_year = table.Column<int>(type: "integer", nullable: false),
                    reg_sl_no = table.Column<string>(type: "text", nullable: false),
                    tax_zone = table.Column<string>(type: "text", nullable: true),
                    tax_circle = table.Column<string>(type: "text", nullable: true),
                    tax_unit = table.Column<string>(type: "text", nullable: true),
                    payable_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    paid_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    submission_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
                    is_draft = table.Column<bool>(type: "boolean", nullable: false),
                    is_uploaded = table.Column<bool>(type: "boolean", nullable: false),
                    api_response = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_document_upload", x => x.duid);
                });

            migrationBuilder.CreateTable(
                name: "document_upload_response",
                columns: table => new
                {
                    durid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    duid = table.Column<int>(type: "integer", nullable: false),
                    api_response = table.Column<string>(type: "text", nullable: false),
                    api_status = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_document_upload_response", x => x.durid);
                });

            migrationBuilder.CreateTable(
                name: "email_notification",
                columns: table => new
                {
                    enid = table.Column<int>(type: "integer", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    email_body = table.Column<string>(type: "text", nullable: false),
                    to = table.Column<string>(type: "text", nullable: false),
                    cc = table.Column<string>(type: "text", nullable: true),
                    bcc = table.Column<string>(type: "text", nullable: true),
                    mail_resoponse = table.Column<string>(type: "text", nullable: true),
                    group_id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_email_notification", x => x.enid);
                });

            migrationBuilder.CreateTable(
                name: "employee",
                columns: table => new
                {
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    full_name = table.Column<string>(type: "text", nullable: true),
                    employee_code = table.Column<string>(type: "text", nullable: true),
                    person_id = table.Column<int>(type: "integer", nullable: false),
                    date_of_joining = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    work_email = table.Column<string>(type: "text", nullable: true),
                    work_mobile = table.Column<string>(type: "text", nullable: true),
                    employee_status_id = table.Column<int>(type: "integer", nullable: true),
                    discontinue_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    employment_category_id = table.Column<int>(type: "integer", nullable: true),
                    confirm_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    wallet_number = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_employee", x => x.employee_id);
                });

            migrationBuilder.CreateTable(
                name: "employee_access_deactivation",
                columns: table => new
                {
                    eadid = table.Column<long>(type: "bigint", nullable: false),
                    eeiid = table.Column<int>(type: "integer", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    date_of_resignation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_working_day = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_core_functional = table.Column<bool>(type: "boolean", nullable: true),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
                    is_sent_for_division_clearance = table.Column<bool>(type: "boolean", nullable: false),
                    division_clearance_approval_status_id = table.Column<int>(type: "integer", nullable: false),
                    sent_for_division_clearance_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_draft = table.Column<bool>(type: "boolean", nullable: false),
                    is_draft_for_div_clearence = table.Column<bool>(type: "boolean", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("pk_employee_access_deactivation", x => x.eadid);
                });

            migrationBuilder.CreateTable(
                name: "employee_bank_info",
                columns: table => new
                {
                    biid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    bank_account_name = table.Column<string>(type: "text", nullable: false),
                    bank_account_number = table.Column<string>(type: "text", nullable: false),
                    bank_name = table.Column<string>(type: "text", nullable: false),
                    bank_branch_name = table.Column<string>(type: "text", nullable: false),
                    routing_number = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("pk_employee_bank_info", x => x.biid);
                });

            migrationBuilder.CreateTable(
                name: "employee_exit_interview",
                columns: table => new
                {
                    eeiid = table.Column<long>(type: "bigint", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    interview_details = table.Column<string>(type: "text", nullable: false),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_employee_exit_interview", x => x.eeiid);
                });

            migrationBuilder.CreateTable(
                name: "employee_festival_bonus_info",
                columns: table => new
                {
                    efbiid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bonus_month = table.Column<string>(type: "text", nullable: false),
                    disbursement_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    employee_code = table.Column<string>(type: "text", nullable: false),
                    designation = table.Column<string>(type: "text", nullable: false),
                    employee_name = table.Column<string>(type: "text", nullable: false),
                    earning_field1 = table.Column<string>(type: "text", nullable: false),
                    earning_value1 = table.Column<string>(type: "text", nullable: false),
                    total_earnings = table.Column<string>(type: "text", nullable: false),
                    deduction_field1 = table.Column<string>(type: "text", nullable: false),
                    deduction_value1 = table.Column<string>(type: "text", nullable: false),
                    deduction_field2 = table.Column<string>(type: "text", nullable: false),
                    deduction_value2 = table.Column<string>(type: "text", nullable: false),
                    total_deductions = table.Column<string>(type: "text", nullable: false),
                    net_payment = table.Column<string>(type: "text", nullable: false),
                    amount_in_words = table.Column<string>(type: "text", nullable: false),
                    bank_amount = table.Column<string>(type: "text", nullable: false),
                    wallet_amount = table.Column<string>(type: "text", nullable: false),
                    cash_out_charge = table.Column<string>(type: "text", nullable: false),
                    patid = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("pk_employee_festival_bonus_info", x => x.efbiid);
                });

            migrationBuilder.CreateTable(
                name: "employee_leave_account",
                columns: table => new
                {
                    elaid = table.Column<int>(type: "integer", nullable: false),
                    financial_year_id = table.Column<int>(type: "integer", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    leave_category_id = table.Column<int>(type: "integer", nullable: false),
                    leave_days = table.Column<decimal>(type: "numeric", nullable: false),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    approved_days = table.Column<decimal>(type: "numeric", nullable: false),
                    pending_days = table.Column<decimal>(type: "numeric", nullable: false),
                    remaining_days = table.Column<decimal>(type: "numeric", nullable: false),
                    previous_leave_days = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("pk_employee_leave_account", x => x.elaid);
                });

            migrationBuilder.CreateTable(
                name: "employee_leave_application",
                columns: table => new
                {
                    employee_leave_aid = table.Column<int>(type: "integer", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    application_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    request_start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    request_end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    no_of_leave_days = table.Column<decimal>(type: "numeric", nullable: false),
                    backup_employee_id = table.Column<int>(type: "integer", nullable: true),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
                    leave_category_id = table.Column<int>(type: "integer", nullable: false),
                    financial_year_id = table.Column<int>(type: "integer", nullable: false),
                    period_id = table.Column<int>(type: "integer", nullable: false),
                    purpose = table.Column<string>(type: "text", nullable: true),
                    leave_location = table.Column<string>(type: "text", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    parent_employee_leave_aid = table.Column<int>(type: "integer", nullable: true),
                    is_multiple = table.Column<bool>(type: "boolean", nullable: false),
                    date_of_joining_work = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    external_id = table.Column<string>(type: "text", nullable: true),
                    additional_filter = table.Column<string>(type: "text", nullable: true),
                    cancellation_status = table.Column<int>(type: "integer", nullable: false),
                    cancelled_by = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("pk_employee_leave_application", x => x.employee_leave_aid);
                });

            migrationBuilder.CreateTable(
                name: "employee_leave_application_day_break_down",
                columns: table => new
                {
                    eladbdid = table.Column<int>(type: "integer", nullable: false),
                    employee_leave_aid = table.Column<int>(type: "integer", nullable: false),
                    request_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    no_of_leave_days = table.Column<decimal>(type: "numeric", nullable: false),
                    half_or_full_day = table.Column<string>(type: "text", nullable: false),
                    additional_filter = table.Column<string>(type: "text", nullable: true),
                    external_id = table.Column<string>(type: "text", nullable: true),
                    is_cancelled = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_employee_leave_application_day_break_down", x => x.eladbdid);
                });

            migrationBuilder.CreateTable(
                name: "employee_monthly_incentive_info",
                columns: table => new
                {
                    emiiid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    incentive_month = table.Column<string>(type: "text", nullable: false),
                    disbursement_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    employee_code = table.Column<string>(type: "text", nullable: false),
                    designation = table.Column<string>(type: "text", nullable: false),
                    division = table.Column<string>(type: "text", nullable: false),
                    employee_name = table.Column<string>(type: "text", nullable: false),
                    adjusted_kpiperformance_score = table.Column<string>(type: "text", nullable: false),
                    essaurating = table.Column<string>(type: "text", nullable: false),
                    attendance_adherence_score = table.Column<string>(type: "text", nullable: false),
                    eligible_incentive = table.Column<string>(type: "text", nullable: false),
                    total_earnings = table.Column<string>(type: "text", nullable: false),
                    adjustment = table.Column<string>(type: "text", nullable: false),
                    total_adjustment = table.Column<string>(type: "text", nullable: false),
                    income_tax = table.Column<string>(type: "text", nullable: false),
                    total_deduction = table.Column<string>(type: "text", nullable: false),
                    net_payment = table.Column<string>(type: "text", nullable: false),
                    amount_in_words = table.Column<string>(type: "text", nullable: false),
                    wallet_amount = table.Column<string>(type: "text", nullable: false),
                    patid = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("pk_employee_monthly_incentive_info", x => x.emiiid);
                });

            migrationBuilder.CreateTable(
                name: "employee_pay_slip_info",
                columns: table => new
                {
                    epsiid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    salary_month = table.Column<string>(type: "text", nullable: false),
                    disbursement_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    employee_code = table.Column<string>(type: "text", nullable: false),
                    designation = table.Column<string>(type: "text", nullable: false),
                    division = table.Column<string>(type: "text", nullable: false),
                    employee_name = table.Column<string>(type: "text", nullable: false),
                    department = table.Column<string>(type: "text", nullable: false),
                    basic_salary = table.Column<string>(type: "text", nullable: false),
                    house_rent = table.Column<string>(type: "text", nullable: false),
                    medical_allowance = table.Column<string>(type: "text", nullable: false),
                    conveyance_allowance = table.Column<string>(type: "text", nullable: false),
                    passage_for_travel = table.Column<string>(type: "text", nullable: false),
                    payroll_card_part = table.Column<string>(type: "text", nullable: false),
                    arrear_basic_salary = table.Column<string>(type: "text", nullable: false),
                    arrear_house_rent = table.Column<string>(type: "text", nullable: false),
                    arrear_medical_allowance = table.Column<string>(type: "text", nullable: false),
                    arrear_conveyance_allowance = table.Column<string>(type: "text", nullable: false),
                    arrear_passage_for_travel = table.Column<string>(type: "text", nullable: false),
                    total_earnings = table.Column<string>(type: "text", nullable: false),
                    total_arrears = table.Column<string>(type: "text", nullable: false),
                    income_tax = table.Column<string>(type: "text", nullable: false),
                    deduction_field1 = table.Column<string>(type: "text", nullable: false),
                    deduction_field2 = table.Column<string>(type: "text", nullable: false),
                    total_deductions = table.Column<string>(type: "text", nullable: false),
                    net_payable = table.Column<string>(type: "text", nullable: false),
                    amount_in_words = table.Column<string>(type: "text", nullable: false),
                    bank_amount = table.Column<string>(type: "text", nullable: false),
                    wallet_amount = table.Column<string>(type: "text", nullable: false),
                    cash_out_charge = table.Column<string>(type: "text", nullable: false),
                    patid = table.Column<long>(type: "bigint", nullable: false),
                    mobile_allowance = table.Column<string>(type: "text", nullable: true),
                    market_bonus = table.Column<string>(type: "text", nullable: true),
                    weekend_allowance = table.Column<string>(type: "text", nullable: true),
                    festival_holiday_allowance = table.Column<string>(type: "text", nullable: true),
                    saturday_allowance = table.Column<string>(type: "text", nullable: true),
                    tax_support = table.Column<string>(type: "text", nullable: true),
                    festival_bonus_arrear = table.Column<string>(type: "text", nullable: true),
                    salary_advance = table.Column<string>(type: "text", nullable: true),
                    tax_refund = table.Column<string>(type: "text", nullable: true),
                    laptop_repairing_cost_deducted = table.Column<string>(type: "text", nullable: true),
                    provident_fund = table.Column<string>(type: "text", nullable: true),
                    mobile_bill_adjustment = table.Column<string>(type: "text", nullable: true),
                    car_allowance = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_employee_pay_slip_info", x => x.epsiid);
                });

            migrationBuilder.CreateTable(
                name: "employee_regular_incentive_info",
                columns: table => new
                {
                    eriiid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    incentive_type = table.Column<string>(type: "text", nullable: false),
                    disbursement_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    employee_code = table.Column<string>(type: "text", nullable: false),
                    designation = table.Column<string>(type: "text", nullable: false),
                    division = table.Column<string>(type: "text", nullable: false),
                    employee_name = table.Column<string>(type: "text", nullable: false),
                    particular1 = table.Column<string>(type: "text", nullable: false),
                    basic_entitlement1 = table.Column<string>(type: "text", nullable: false),
                    particular2 = table.Column<string>(type: "text", nullable: false),
                    basic_entitlement2 = table.Column<string>(type: "text", nullable: false),
                    particular3 = table.Column<string>(type: "text", nullable: false),
                    basic_entitlement3 = table.Column<string>(type: "text", nullable: false),
                    particular4 = table.Column<string>(type: "text", nullable: false),
                    basic_entitlement4 = table.Column<string>(type: "text", nullable: false),
                    eligible_bonus = table.Column<string>(type: "text", nullable: false),
                    eligible_bonus_total = table.Column<string>(type: "text", nullable: false),
                    income_tax = table.Column<string>(type: "text", nullable: false),
                    total_deduction = table.Column<string>(type: "text", nullable: false),
                    net_payable = table.Column<string>(type: "text", nullable: false),
                    amount_in_words = table.Column<string>(type: "text", nullable: false),
                    bank_amount = table.Column<string>(type: "text", nullable: false),
                    particulars5 = table.Column<string>(type: "text", nullable: false),
                    performance_rating1 = table.Column<string>(type: "text", nullable: false),
                    particulars6 = table.Column<string>(type: "text", nullable: false),
                    performance_rating2 = table.Column<string>(type: "text", nullable: false),
                    patid = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("pk_employee_regular_incentive_info", x => x.eriiid);
                });

            migrationBuilder.CreateTable(
                name: "employee_supervisor_map",
                columns: table => new
                {
                    map_id = table.Column<int>(type: "integer", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    employee_supervisor_id = table.Column<int>(type: "integer", nullable: false),
                    is_current = table.Column<bool>(type: "boolean", nullable: false),
                    supervisor_type = table.Column<int>(type: "integer", nullable: false),
                    from_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    to_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("pk_employee_supervisor_map", x => x.map_id);
                });

            migrationBuilder.CreateTable(
                name: "employment",
                columns: table => new
                {
                    employment_id = table.Column<int>(type: "integer", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    employee_type_id = table.Column<int>(type: "integer", nullable: true),
                    designation_id = table.Column<int>(type: "integer", nullable: true),
                    internal_designation_id = table.Column<int>(type: "integer", nullable: true),
                    department_id = table.Column<int>(type: "integer", nullable: true),
                    division_id = table.Column<int>(type: "integer", nullable: true),
                    job_grade_id = table.Column<int>(type: "integer", nullable: false),
                    branch_id = table.Column<int>(type: "integer", nullable: true),
                    unit_id = table.Column<int>(type: "integer", nullable: true),
                    sub_unit_id = table.Column<int>(type: "integer", nullable: true),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_current = table.Column<bool>(type: "boolean", nullable: false),
                    change_status_id = table.Column<int>(type: "integer", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    band = table.Column<string>(type: "text", nullable: true),
                    cluster_id = table.Column<int>(type: "integer", nullable: true),
                    region_id = table.Column<int>(type: "integer", nullable: true),
                    shift_id = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("pk_employment", x => x.employment_id);
                });

            migrationBuilder.CreateTable(
                name: "external_audit_child",
                columns: table => new
                {
                    eacid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    eamid = table.Column<int>(type: "integer", nullable: false),
                    audit_question_id = table.Column<int>(type: "integer", nullable: false),
                    question_feedback = table.Column<string>(type: "text", nullable: false),
                    department_id = table.Column<int>(type: "integer", nullable: false),
                    requirements = table.Column<string>(type: "text", nullable: true),
                    posmids = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_external_audit_child", x => x.eacid);
                });

            migrationBuilder.CreateTable(
                name: "external_audit_config",
                columns: table => new
                {
                    eacid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    number_of_uddokta = table.Column<int>(type: "integer", nullable: false),
                    number_of_merchant = table.Column<int>(type: "integer", nullable: false),
                    effective_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    number_of_days = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_external_audit_config", x => x.eacid);
                });

            migrationBuilder.CreateTable(
                name: "external_audit_master",
                columns: table => new
                {
                    eamid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    reference_no = table.Column<string>(type: "text", nullable: true),
                    reference_keyword = table.Column<string>(type: "text", nullable: true),
                    mercent_or_udokta_id = table.Column<int>(type: "integer", nullable: false),
                    mercent_or_udokta_number = table.Column<string>(type: "text", nullable: false),
                    audit_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    remarks = table.Column<string>(type: "text", nullable: false),
                    auditable_employee_id = table.Column<int>(type: "integer", nullable: false),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
                    is_draft = table.Column<bool>(type: "boolean", nullable: false),
                    return_department_ids = table.Column<string>(type: "text", nullable: true),
                    requirements = table.Column<string>(type: "text", nullable: true),
                    longtitude = table.Column<string>(type: "text", nullable: true),
                    latitude = table.Column<string>(type: "text", nullable: true),
                    captured_image = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_external_audit_master", x => x.eamid);
                });

            migrationBuilder.CreateTable(
                name: "holiday",
                columns: table => new
                {
                    holiday_id = table.Column<int>(type: "integer", nullable: false),
                    financial_year_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    holiday_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    image_path = table.Column<string>(type: "text", nullable: true),
                    is_festival_holiday = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_holiday", x => x.holiday_id);
                });

            migrationBuilder.CreateTable(
                name: "job_grade",
                columns: table => new
                {
                    job_grade_id = table.Column<int>(type: "integer", nullable: false),
                    job_grade_name = table.Column<string>(type: "text", nullable: false),
                    sequence_no = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("pk_job_grade", x => x.job_grade_id);
                });

            migrationBuilder.CreateTable(
                name: "leave_policy_settings",
                columns: table => new
                {
                    lpsid = table.Column<int>(type: "integer", nullable: false),
                    leave_category_id = table.Column<int>(type: "integer", nullable: false),
                    minimum_days = table.Column<decimal>(type: "numeric", nullable: false),
                    maximum_days = table.Column<decimal>(type: "numeric", nullable: false),
                    day_type = table.Column<int>(type: "integer", nullable: false),
                    employee_type_exception = table.Column<bool>(type: "boolean", nullable: false),
                    employee_types = table.Column<string>(type: "text", nullable: true),
                    tanure_exception = table.Column<bool>(type: "boolean", nullable: false),
                    eligibility_in_months = table.Column<int>(type: "integer", nullable: false),
                    is_holiday_inclusive = table.Column<bool>(type: "boolean", nullable: false),
                    is_carry_forwardable = table.Column<bool>(type: "boolean", nullable: false),
                    maximum_accumulation_days = table.Column<int>(type: "integer", nullable: false),
                    is_attachemnt_required = table.Column<bool>(type: "boolean", nullable: false),
                    will_applicable_from = table.Column<int>(type: "integer", nullable: false),
                    hierarchy_level = table.Column<int>(type: "integer", nullable: false),
                    maximum_job_grade = table.Column<int>(type: "integer", nullable: false),
                    include_hrfor_lfa = table.Column<bool>(type: "boolean", nullable: false),
                    include_hrfor_leave = table.Column<bool>(type: "boolean", nullable: false),
                    applicable_to_hrfor_days = table.Column<decimal>(type: "numeric", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    proxy_employee_ids = table.Column<string>(type: "text", nullable: true),
                    include_hrfor_festival = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_leave_policy_settings", x => x.lpsid);
                });

            migrationBuilder.CreateTable(
                name: "lfadeclaration",
                columns: table => new
                {
                    lfadid = table.Column<int>(type: "integer", nullable: false),
                    year = table.Column<string>(type: "text", nullable: true),
                    travel_type = table.Column<int>(type: "integer", nullable: false),
                    travel_destination = table.Column<string>(type: "text", nullable: true),
                    from_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    to_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    employee_leave_aid = table.Column<int>(type: "integer", nullable: false),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_lfadeclaration", x => x.lfadid);
                });

            migrationBuilder.CreateTable(
                name: "payroll_audit_trial",
                columns: table => new
                {
                    patid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    file_name = table.Column<string>(type: "text", nullable: false),
                    uploaded_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    activity_type_id = table.Column<int>(type: "integer", nullable: false),
                    activity_period = table.Column<string>(type: "text", nullable: false),
                    activity_status_id = table.Column<int>(type: "integer", nullable: false),
                    festival_bonus_type_id = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("pk_payroll_audit_trial", x => x.patid);
                });

            migrationBuilder.CreateTable(
                name: "region",
                columns: table => new
                {
                    region_id = table.Column<int>(type: "integer", nullable: false),
                    region_name = table.Column<string>(type: "text", nullable: false),
                    region_code = table.Column<string>(type: "text", nullable: true),
                    cluster_id = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("pk_region", x => x.region_id);
                });

            migrationBuilder.CreateTable(
                name: "remote_attendance",
                columns: table => new
                {
                    raid = table.Column<int>(type: "integer", nullable: false),
                    attendance_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    employee_note = table.Column<string>(type: "text", nullable: true),
                    entry_type = table.Column<string>(type: "text", nullable: true),
                    status_id = table.Column<int>(type: "integer", nullable: false),
                    approver_id = table.Column<int>(type: "integer", nullable: false),
                    approver_note = table.Column<string>(type: "text", nullable: true),
                    approval_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    channel = table.Column<string>(type: "text", nullable: true),
                    district_id = table.Column<int>(type: "integer", nullable: false),
                    division_id = table.Column<int>(type: "integer", nullable: false),
                    thana_id = table.Column<int>(type: "integer", nullable: false),
                    area = table.Column<string>(type: "text", nullable: true),
                    longitude = table.Column<string>(type: "text", nullable: true),
                    latitude = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_remote_attendance", x => x.raid);
                });

            migrationBuilder.CreateTable(
                name: "renovation_ormaintenance_category",
                columns: table => new
                {
                    romid = table.Column<int>(type: "integer", nullable: false),
                    renovation_name = table.Column<string>(type: "text", nullable: false),
                    sequence_no = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("pk_renovation_ormaintenance_category", x => x.romid);
                });

            migrationBuilder.CreateTable(
                name: "request_support_facilities_details",
                columns: table => new
                {
                    rsfdid = table.Column<int>(type: "integer", nullable: false),
                    rsmid = table.Column<int>(type: "integer", nullable: false),
                    support_category_id = table.Column<int>(type: "integer", nullable: false),
                    needed_by_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("pk_request_support_facilities_details", x => x.rsfdid);
                });

            migrationBuilder.CreateTable(
                name: "request_support_item_details",
                columns: table => new
                {
                    rsidid = table.Column<int>(type: "integer", nullable: false),
                    rsmid = table.Column<int>(type: "integer", nullable: false),
                    item_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("pk_request_support_item_details", x => x.rsidid);
                });

            migrationBuilder.CreateTable(
                name: "request_support_master",
                columns: table => new
                {
                    rsmid = table.Column<int>(type: "integer", nullable: false),
                    support_type_id = table.Column<int>(type: "integer", nullable: false),
                    location_or_floor = table.Column<string>(type: "text", nullable: true),
                    needed_by_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    remarks_or_comments_or_purpose = table.Column<string>(type: "text", nullable: true),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
                    admin_remarks = table.Column<string>(type: "text", nullable: true),
                    is_draft = table.Column<bool>(type: "boolean", nullable: false),
                    is_settle = table.Column<bool>(type: "boolean", nullable: true),
                    settlement_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    settlement_remarks = table.Column<string>(type: "text", nullable: true),
                    reference_no = table.Column<string>(type: "text", nullable: true),
                    reference_keyword = table.Column<string>(type: "text", nullable: true),
                    employee_id = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("pk_request_support_master", x => x.rsmid);
                });

            migrationBuilder.CreateTable(
                name: "request_support_renovation_ormaintenance_details",
                columns: table => new
                {
                    rsrmdid = table.Column<int>(type: "integer", nullable: false),
                    rsmid = table.Column<int>(type: "integer", nullable: false),
                    reno_or_main_category_id = table.Column<int>(type: "integer", nullable: false),
                    needed_by_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("pk_request_support_renovation_ormaintenance_details", x => x.rsrmdid);
                });

            migrationBuilder.CreateTable(
                name: "request_support_vehicle_details",
                columns: table => new
                {
                    rsvdid = table.Column<int>(type: "integer", nullable: false),
                    rsmid = table.Column<int>(type: "integer", nullable: false),
                    transport_needed_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    transport_needed_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    transport_type_id = table.Column<int>(type: "integer", nullable: false),
                    person_quantity = table.Column<int>(type: "integer", nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    duration = table.Column<string>(type: "text", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    transport_quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    from_division_id = table.Column<int>(type: "integer", nullable: true),
                    from_district_id = table.Column<int>(type: "integer", nullable: true),
                    from_thana_id = table.Column<int>(type: "integer", nullable: true),
                    from_area = table.Column<string>(type: "text", nullable: true),
                    to_division_id = table.Column<int>(type: "integer", nullable: true),
                    to_district_id = table.Column<int>(type: "integer", nullable: true),
                    to_thana_id = table.Column<int>(type: "integer", nullable: true),
                    to_area = table.Column<string>(type: "text", nullable: true),
                    is_others = table.Column<bool>(type: "boolean", nullable: true),
                    vehicle = table.Column<string>(type: "text", nullable: true),
                    driver = table.Column<string>(type: "text", nullable: true),
                    contact_number = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_request_support_vehicle_details", x => x.rsvdid);
                });

            migrationBuilder.CreateTable(
                name: "shifting_child",
                columns: table => new
                {
                    shifting_child_id = table.Column<int>(type: "integer", nullable: false),
                    shifting_master_id = table.Column<int>(type: "integer", nullable: false),
                    day = table.Column<int>(type: "integer", nullable: false),
                    start_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    end_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    is_working_day = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_shifting_child", x => x.shifting_child_id);
                });

            migrationBuilder.CreateTable(
                name: "shifting_leave_child",
                columns: table => new
                {
                    shifting_leave_child_id = table.Column<int>(type: "integer", nullable: false),
                    shifting_master_id = table.Column<int>(type: "integer", nullable: false),
                    shifting_leave_category_id = table.Column<int>(type: "integer", nullable: false),
                    start_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    end_time = table.Column<TimeSpan>(type: "interval", nullable: false),
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
                    table.PrimaryKey("pk_shifting_leave_child", x => x.shifting_leave_child_id);
                });

            migrationBuilder.CreateTable(
                name: "shifting_master",
                columns: table => new
                {
                    shifting_master_id = table.Column<int>(type: "integer", nullable: false),
                    shifting_name = table.Column<string>(type: "text", nullable: false),
                    first_day_of_week = table.Column<int>(type: "integer", nullable: true),
                    shifting_slot = table.Column<int>(type: "integer", nullable: false),
                    buffer_time_in_minute = table.Column<int>(type: "integer", nullable: false),
                    effect_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    assigned_departments = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_shifting_master", x => x.shifting_master_id);
                });

            migrationBuilder.CreateTable(
                name: "support_requisition_master",
                columns: table => new
                {
                    srmid = table.Column<int>(type: "integer", nullable: false),
                    reference_no = table.Column<string>(type: "text", nullable: true),
                    reference_keyword = table.Column<string>(type: "text", nullable: true),
                    request_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    support_category_id = table.Column<int>(type: "integer", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: true),
                    is_on_behalf = table.Column<bool>(type: "boolean", nullable: true),
                    business_justification = table.Column<string>(type: "text", nullable: true),
                    itremomandation = table.Column<string>(type: "text", nullable: true),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
                    is_draft = table.Column<bool>(type: "boolean", nullable: false),
                    itremarks = table.Column<string>(type: "text", nullable: true),
                    is_new_requirements = table.Column<bool>(type: "boolean", nullable: true),
                    is_replacement = table.Column<bool>(type: "boolean", nullable: true),
                    is_settle = table.Column<bool>(type: "boolean", nullable: true),
                    settlement_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    settlement_remarks = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_support_requisition_master", x => x.srmid);
                });

            migrationBuilder.CreateTable(
                name: "unauthorized_leave_email_date",
                columns: table => new
                {
                    uleid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enid = table.Column<int>(type: "integer", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    attendance_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("pk_unauthorized_leave_email_date", x => x.uleid);
                });

            migrationBuilder.CreateTable(
                name: "user_wise_uddokta_or_merchant_mapping",
                columns: table => new
                {
                    mapid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    wallet_number = table.Column<string>(type: "text", nullable: false),
                    wallet_name = table.Column<string>(type: "text", nullable: true),
                    type_id = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_tagged = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_user_wise_uddokta_or_merchant_mapping", x => x.mapid);
                });

            migrationBuilder.CreateTable(
                name: "wage_code_configuration",
                columns: table => new
                {
                    wage_code_configuration_id = table.Column<int>(type: "integer", nullable: false),
                    wage_code = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    type_id = table.Column<int>(type: "integer", nullable: false),
                    exception_flag = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_wage_code_configuration", x => x.wage_code_configuration_id);
                });

            migrationBuilder.CreateTable(
                name: "wallet_configuration",
                columns: table => new
                {
                    wallet_configure_id = table.Column<int>(type: "integer", nullable: false),
                    cash_out_rate = table.Column<decimal>(type: "numeric", nullable: false),
                    designation_id = table.Column<int>(type: "integer", nullable: false),
                    percentage = table.Column<decimal>(type: "numeric", nullable: false),
                    type_id = table.Column<int>(type: "integer", nullable: false),
                    exception_flag = table.Column<bool>(type: "boolean", nullable: false),
                    is_current = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_wallet_configuration", x => x.wallet_configure_id);
                });

            migrationBuilder.CreateTable(
                name: "working_day",
                columns: table => new
                {
                    working_day_id = table.Column<int>(type: "integer", nullable: false),
                    financial_year_id = table.Column<int>(type: "integer", nullable: false),
                    period_id = table.Column<int>(type: "integer", nullable: false),
                    working_day_category_id = table.Column<int>(type: "integer", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: false),
                    day = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_working_day", x => x.working_day_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "access_request_category_child");

            migrationBuilder.DropTable(
                name: "accessories_requisition_category_child");

            migrationBuilder.DropTable(
                name: "annual_leave_encashment_master");

            migrationBuilder.DropTable(
                name: "annual_leave_encashment_policy_settings");

            migrationBuilder.DropTable(
                name: "annual_leave_encashment_window_child");

            migrationBuilder.DropTable(
                name: "annual_leave_encashment_window_master");

            migrationBuilder.DropTable(
                name: "asset_requisition_category_child");

            migrationBuilder.DropTable(
                name: "attendance_summary");

            migrationBuilder.DropTable(
                name: "audit_approval_config");

            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "audit_question");

            migrationBuilder.DropTable(
                name: "branch_info");

            migrationBuilder.DropTable(
                name: "cluster");

            migrationBuilder.DropTable(
                name: "company_leave_policy");

            migrationBuilder.DropTable(
                name: "department");

            migrationBuilder.DropTable(
                name: "department_head_map");

            migrationBuilder.DropTable(
                name: "designation");

            migrationBuilder.DropTable(
                name: "division");

            migrationBuilder.DropTable(
                name: "division_head_map");

            migrationBuilder.DropTable(
                name: "document_upload");

            migrationBuilder.DropTable(
                name: "document_upload_response");

            migrationBuilder.DropTable(
                name: "email_notification");

            migrationBuilder.DropTable(
                name: "employee");

            migrationBuilder.DropTable(
                name: "employee_access_deactivation");

            migrationBuilder.DropTable(
                name: "employee_bank_info");

            migrationBuilder.DropTable(
                name: "employee_exit_interview");

            migrationBuilder.DropTable(
                name: "employee_festival_bonus_info");

            migrationBuilder.DropTable(
                name: "employee_leave_account");

            migrationBuilder.DropTable(
                name: "employee_leave_application");

            migrationBuilder.DropTable(
                name: "employee_leave_application_day_break_down");

            migrationBuilder.DropTable(
                name: "employee_monthly_incentive_info");

            migrationBuilder.DropTable(
                name: "employee_pay_slip_info");

            migrationBuilder.DropTable(
                name: "employee_regular_incentive_info");

            migrationBuilder.DropTable(
                name: "employee_supervisor_map");

            migrationBuilder.DropTable(
                name: "employment");

            migrationBuilder.DropTable(
                name: "external_audit_child");

            migrationBuilder.DropTable(
                name: "external_audit_config");

            migrationBuilder.DropTable(
                name: "external_audit_master");

            migrationBuilder.DropTable(
                name: "holiday");

            migrationBuilder.DropTable(
                name: "job_grade");

            migrationBuilder.DropTable(
                name: "leave_policy_settings");

            migrationBuilder.DropTable(
                name: "lfadeclaration");

            migrationBuilder.DropTable(
                name: "payroll_audit_trial");

            migrationBuilder.DropTable(
                name: "region");

            migrationBuilder.DropTable(
                name: "remote_attendance");

            migrationBuilder.DropTable(
                name: "renovation_ormaintenance_category");

            migrationBuilder.DropTable(
                name: "request_support_facilities_details");

            migrationBuilder.DropTable(
                name: "request_support_item_details");

            migrationBuilder.DropTable(
                name: "request_support_master");

            migrationBuilder.DropTable(
                name: "request_support_renovation_ormaintenance_details");

            migrationBuilder.DropTable(
                name: "request_support_vehicle_details");

            migrationBuilder.DropTable(
                name: "shifting_child");

            migrationBuilder.DropTable(
                name: "shifting_leave_child");

            migrationBuilder.DropTable(
                name: "shifting_master");

            migrationBuilder.DropTable(
                name: "support_requisition_master");

            migrationBuilder.DropTable(
                name: "unauthorized_leave_email_date");

            migrationBuilder.DropTable(
                name: "user_wise_uddokta_or_merchant_mapping");

            migrationBuilder.DropTable(
                name: "wage_code_configuration");

            migrationBuilder.DropTable(
                name: "wallet_configuration");

            migrationBuilder.DropTable(
                name: "working_day");
        }
    }
}
