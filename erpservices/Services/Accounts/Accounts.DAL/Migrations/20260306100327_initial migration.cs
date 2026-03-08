using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Accounts.DAL.Migrations
{
    /// <inheritdoc />
    public partial class initialmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accategory",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    category_name = table.Column<string>(type: "text", nullable: false),
                    category_short_code = table.Column<string>(type: "text", nullable: false),
                    sequence_no = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_accategory", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "acclass",
                columns: table => new
                {
                    acclass_id = table.Column<int>(type: "integer", nullable: false),
                    class_name = table.Column<string>(type: "text", nullable: false),
                    short_code = table.Column<string>(type: "text", nullable: false),
                    balance_type = table.Column<string>(type: "text", nullable: true),
                    sequence_no = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_acclass", x => x.acclass_id);
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
                name: "bank",
                columns: table => new
                {
                    bank_id = table.Column<long>(type: "bigint", nullable: false),
                    bank_name = table.Column<string>(type: "text", nullable: false),
                    bank_address = table.Column<string>(type: "text", nullable: true),
                    concern_person_name = table.Column<string>(type: "text", nullable: true),
                    concern_person_phone_number = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_bank", x => x.bank_id);
                });

            migrationBuilder.CreateTable(
                name: "budget_child",
                columns: table => new
                {
                    budget_child_id = table.Column<long>(type: "bigint", nullable: false),
                    budget_master_id = table.Column<long>(type: "bigint", nullable: false),
                    min_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    max_amount = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("pk_budget_child", x => x.budget_child_id);
                });

            migrationBuilder.CreateTable(
                name: "budget_child_with_approval_panel_map",
                columns: table => new
                {
                    bcwappmap_id = table.Column<long>(type: "bigint", nullable: false),
                    budget_child_id = table.Column<long>(type: "bigint", nullable: false),
                    appanel_id = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("pk_budget_child_with_approval_panel_map", x => x.bcwappmap_id);
                });

            migrationBuilder.CreateTable(
                name: "budget_master",
                columns: table => new
                {
                    budget_master_id = table.Column<long>(type: "bigint", nullable: false),
                    department_id = table.Column<long>(type: "bigint", nullable: false),
                    min_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    total_max_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    attachment_required_amount = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("pk_budget_master", x => x.budget_master_id);
                });

            migrationBuilder.CreateTable(
                name: "chart_of_accounts",
                columns: table => new
                {
                    coaid = table.Column<long>(type: "bigint", nullable: false),
                    account_name = table.Column<string>(type: "text", nullable: false),
                    account_code = table.Column<string>(type: "text", nullable: true),
                    parent_id = table.Column<long>(type: "bigint", nullable: true),
                    acclass_id = table.Column<int>(type: "integer", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    hierarchy_level = table.Column<string>(type: "text", nullable: true),
                    level = table.Column<int>(type: "integer", nullable: false),
                    sequence_no = table.Column<int>(type: "integer", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    opening_balance = table.Column<decimal>(type: "numeric", nullable: true),
                    opening_balance_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    external_id = table.Column<string>(type: "text", nullable: true),
                    balance_type = table.Column<string>(type: "text", nullable: true),
                    is_budget_enable = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_allow_manual_posting = table.Column<bool>(type: "boolean", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("pk_chart_of_accounts", x => x.coaid);
                });

            migrationBuilder.CreateTable(
                name: "cheque_book",
                columns: table => new
                {
                    cbid = table.Column<int>(type: "integer", nullable: false),
                    glid = table.Column<long>(type: "bigint", nullable: false),
                    bank_id = table.Column<long>(type: "bigint", nullable: false),
                    cheque_book_no = table.Column<string>(type: "text", nullable: false),
                    no_of_page = table.Column<int>(type: "integer", nullable: false),
                    account_no = table.Column<long>(type: "bigint", nullable: false),
                    account_name = table.Column<string>(type: "text", nullable: false),
                    branch_name = table.Column<string>(type: "text", nullable: false),
                    routing_name = table.Column<string>(type: "text", nullable: false),
                    swift_code = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    start_leaf = table.Column<int>(type: "integer", nullable: false),
                    end_leaf = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_cheque_book", x => x.cbid);
                });

            migrationBuilder.CreateTable(
                name: "cheque_book_child",
                columns: table => new
                {
                    cbcid = table.Column<int>(type: "integer", nullable: false),
                    cbid = table.Column<int>(type: "integer", nullable: false),
                    leaf_no = table.Column<int>(type: "integer", nullable: false),
                    is_active_leaf = table.Column<bool>(type: "boolean", nullable: false),
                    is_used = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_cheque_book_child", x => x.cbcid);
                });

            migrationBuilder.CreateTable(
                name: "cost_category",
                columns: table => new
                {
                    cost_category_id = table.Column<long>(type: "bigint", nullable: false),
                    category_name = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("pk_cost_category", x => x.cost_category_id);
                });

            migrationBuilder.CreateTable(
                name: "cost_center",
                columns: table => new
                {
                    cost_center_id = table.Column<long>(type: "bigint", nullable: false),
                    cost_center_name = table.Column<string>(type: "text", nullable: false),
                    cost_category_id = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("pk_cost_center", x => x.cost_center_id);
                });

            migrationBuilder.CreateTable(
                name: "custodian_wallet",
                columns: table => new
                {
                    cwid = table.Column<long>(type: "bigint", nullable: false),
                    wallet_name = table.Column<string>(type: "text", nullable: false),
                    employee_id = table.Column<long>(type: "bigint", nullable: false),
                    reimbursement_threshold = table.Column<decimal>(type: "numeric", nullable: false),
                    opening_balance = table.Column<decimal>(type: "numeric", nullable: false),
                    current_balance = table.Column<decimal>(type: "numeric", nullable: false),
                    limit = table.Column<decimal>(type: "numeric", nullable: false),
                    division_ids = table.Column<string>(type: "text", nullable: false),
                    department_ids = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("pk_custodian_wallet", x => x.cwid);
                });

            migrationBuilder.CreateTable(
                name: "expense_claim_child",
                columns: table => new
                {
                    ecchild_id = table.Column<long>(type: "bigint", nullable: false),
                    ecmaster_id = table.Column<long>(type: "bigint", nullable: false),
                    expense_claim_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    purpose_id = table.Column<int>(type: "integer", nullable: false),
                    details = table.Column<string>(type: "text", nullable: false),
                    expense_claim_amount = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("pk_expense_claim_child", x => x.ecchild_id);
                });

            migrationBuilder.CreateTable(
                name: "expense_claim_master",
                columns: table => new
                {
                    ecmaster_id = table.Column<long>(type: "bigint", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    reference_no = table.Column<string>(type: "text", nullable: false),
                    reference_keyword = table.Column<string>(type: "text", nullable: true),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
                    ioumaster_id = table.Column<long>(type: "bigint", nullable: true),
                    grand_total = table.Column<decimal>(type: "numeric", nullable: false),
                    claim_submit_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    payment_status = table.Column<int>(type: "integer", nullable: false),
                    is_on_behalf = table.Column<bool>(type: "boolean", nullable: true),
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
                    table.PrimaryKey("pk_expense_claim_master", x => x.ecmaster_id);
                });

            migrationBuilder.CreateTable(
                name: "general_ledger",
                columns: table => new
                {
                    glid = table.Column<long>(type: "bigint", nullable: false),
                    glname = table.Column<string>(type: "text", nullable: false),
                    glcode = table.Column<string>(type: "text", nullable: true),
                    glgroup_id = table.Column<long>(type: "bigint", nullable: true),
                    gltotal_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    opening_balance = table.Column<decimal>(type: "numeric", nullable: true),
                    opening_balance_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    external_id = table.Column<string>(type: "text", nullable: true),
                    balance_type = table.Column<string>(type: "text", nullable: true),
                    is_budget_enable = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_allow_manual_posting = table.Column<bool>(type: "boolean", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    gltype_id = table.Column<int>(type: "integer", nullable: false),
                    gllayer_id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_general_ledger", x => x.glid);
                });

            migrationBuilder.CreateTable(
                name: "iouchild",
                columns: table => new
                {
                    iouchild_id = table.Column<long>(type: "bigint", nullable: false),
                    ioumaster_id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    iouamount = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("pk_iouchild", x => x.iouchild_id);
                });

            migrationBuilder.CreateTable(
                name: "ioumaster",
                columns: table => new
                {
                    ioumaster_id = table.Column<long>(type: "bigint", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    request_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    settlement_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reference_no = table.Column<string>(type: "text", nullable: false),
                    reference_keyword = table.Column<string>(type: "text", nullable: true),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
                    grand_total = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("pk_ioumaster", x => x.ioumaster_id);
                });

            migrationBuilder.CreateTable(
                name: "iouor_expense_payment_child",
                columns: table => new
                {
                    payment_child_id = table.Column<long>(type: "bigint", nullable: false),
                    payment_master_id = table.Column<long>(type: "bigint", nullable: false),
                    employee_id = table.Column<long>(type: "bigint", nullable: false),
                    department_id = table.Column<long>(type: "bigint", nullable: false),
                    iouor_expense_claim_id = table.Column<long>(type: "bigint", nullable: false),
                    glid = table.Column<long>(type: "bigint", nullable: true),
                    approved_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    receiving_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    posting_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    payment_status = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("pk_iouor_expense_payment_child", x => x.payment_child_id);
                });

            migrationBuilder.CreateTable(
                name: "iouor_expense_payment_master",
                columns: table => new
                {
                    payment_master_id = table.Column<long>(type: "bigint", nullable: false),
                    reference_no = table.Column<string>(type: "text", nullable: false),
                    reference_keyword = table.Column<string>(type: "text", nullable: true),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
                    grand_total = table.Column<decimal>(type: "numeric", nullable: false),
                    payment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    paid_by = table.Column<int>(type: "integer", nullable: true),
                    claim_type = table.Column<string>(type: "text", nullable: false),
                    is_exception = table.Column<bool>(type: "boolean", nullable: false),
                    is_settlement = table.Column<bool>(type: "boolean", nullable: false),
                    settled_by = table.Column<int>(type: "integer", nullable: true),
                    settlement_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    settlement_ref_no = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_iouor_expense_payment_master", x => x.payment_master_id);
                });

            migrationBuilder.CreateTable(
                name: "petty_cash_advance_child",
                columns: table => new
                {
                    pcacid = table.Column<long>(type: "bigint", nullable: false),
                    pcamid = table.Column<long>(type: "bigint", nullable: false),
                    details = table.Column<string>(type: "text", nullable: false),
                    project_name = table.Column<string>(type: "text", nullable: false),
                    advance_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    resubmit_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    resubmit_remarks = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_petty_cash_advance_child", x => x.pcacid);
                });

            migrationBuilder.CreateTable(
                name: "petty_cash_advance_master",
                columns: table => new
                {
                    pcamid = table.Column<long>(type: "bigint", nullable: false),
                    reference_no = table.Column<string>(type: "text", nullable: false),
                    reference_keyword = table.Column<string>(type: "text", nullable: true),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    request_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
                    grand_total = table.Column<decimal>(type: "numeric", nullable: false),
                    is_settlement = table.Column<bool>(type: "boolean", nullable: false),
                    settlement_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cwid = table.Column<long>(type: "bigint", nullable: false),
                    claim_status_id = table.Column<int>(type: "integer", nullable: true),
                    is_disbursement = table.Column<bool>(type: "boolean", nullable: true),
                    disbursement_by = table.Column<int>(type: "integer", nullable: true),
                    disbursement_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    disbursement_remarks = table.Column<string>(type: "text", nullable: true),
                    is_resubmit = table.Column<bool>(type: "boolean", nullable: true),
                    resubmit_by = table.Column<int>(type: "integer", nullable: true),
                    resubmit_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    re_submit_total_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    resubmit_approval_status_id = table.Column<int>(type: "integer", nullable: true),
                    payable_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    receiveable_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    is_settled = table.Column<bool>(type: "boolean", nullable: true),
                    settled_by = table.Column<int>(type: "integer", nullable: true),
                    settled_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    settlement_remarks = table.Column<string>(type: "text", nullable: true),
                    is_draft = table.Column<bool>(type: "boolean", nullable: false),
                    is_resubmit_disbursement = table.Column<bool>(type: "boolean", nullable: true),
                    resubmit_disbursement_by = table.Column<int>(type: "integer", nullable: true),
                    resubmit_disbursement_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resubmit_disbursement_remarks = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_petty_cash_advance_master", x => x.pcamid);
                });

            migrationBuilder.CreateTable(
                name: "petty_cash_expense_child",
                columns: table => new
                {
                    pcecid = table.Column<long>(type: "bigint", nullable: false),
                    pcemid = table.Column<long>(type: "bigint", nullable: false),
                    expense_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    purpose_id = table.Column<int>(type: "integer", nullable: false),
                    details = table.Column<string>(type: "text", nullable: false),
                    expense_amount = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("pk_petty_cash_expense_child", x => x.pcecid);
                });

            migrationBuilder.CreateTable(
                name: "petty_cash_expense_master",
                columns: table => new
                {
                    pcemid = table.Column<long>(type: "bigint", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    reference_no = table.Column<string>(type: "text", nullable: false),
                    reference_keyword = table.Column<string>(type: "text", nullable: true),
                    submit_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cwid = table.Column<long>(type: "bigint", nullable: false),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
                    grand_total = table.Column<decimal>(type: "numeric", nullable: false),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    payment_status = table.Column<int>(type: "integer", nullable: false),
                    claim_status_id = table.Column<int>(type: "integer", nullable: true),
                    is_disbursement = table.Column<bool>(type: "boolean", nullable: true),
                    disbursement_by = table.Column<int>(type: "integer", nullable: true),
                    disbursement_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    disbursement_remarks = table.Column<string>(type: "text", nullable: true),
                    is_settled = table.Column<bool>(type: "boolean", nullable: true),
                    settled_by = table.Column<int>(type: "integer", nullable: true),
                    settlement_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("pk_petty_cash_expense_master", x => x.pcemid);
                });

            migrationBuilder.CreateTable(
                name: "petty_cash_payment_child",
                columns: table => new
                {
                    pcpcid = table.Column<long>(type: "bigint", nullable: false),
                    pcpmid = table.Column<long>(type: "bigint", nullable: false),
                    pcrmid = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("pk_petty_cash_payment_child", x => x.pcpcid);
                });

            migrationBuilder.CreateTable(
                name: "petty_cash_payment_master",
                columns: table => new
                {
                    pcpmid = table.Column<long>(type: "bigint", nullable: false),
                    reference_no = table.Column<string>(type: "text", nullable: false),
                    reference_keyword = table.Column<string>(type: "text", nullable: true),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    request_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
                    grand_total = table.Column<decimal>(type: "numeric", nullable: false),
                    is_settlement = table.Column<bool>(type: "boolean", nullable: false),
                    settlement_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    settlement_by = table.Column<int>(type: "integer", nullable: true),
                    settlement_remarks = table.Column<string>(type: "text", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_petty_cash_payment_master", x => x.pcpmid);
                });

            migrationBuilder.CreateTable(
                name: "petty_cash_reimburse_child",
                columns: table => new
                {
                    pcrcid = table.Column<long>(type: "bigint", nullable: false),
                    pcrmid = table.Column<long>(type: "bigint", nullable: false),
                    pccid = table.Column<long>(type: "bigint", nullable: false),
                    claim_type_id = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("pk_petty_cash_reimburse_child", x => x.pcrcid);
                });

            migrationBuilder.CreateTable(
                name: "petty_cash_reimburse_master",
                columns: table => new
                {
                    pcrmid = table.Column<long>(type: "bigint", nullable: false),
                    reference_no = table.Column<string>(type: "text", nullable: false),
                    reference_keyword = table.Column<string>(type: "text", nullable: true),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
                    grand_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    reimburse_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("pk_petty_cash_reimburse_master", x => x.pcrmid);
                });

            migrationBuilder.CreateTable(
                name: "petty_cash_transaction_history",
                columns: table => new
                {
                    transaction_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    reference_no = table.Column<string>(type: "text", nullable: false),
                    type_id = table.Column<int>(type: "integer", nullable: false),
                    type_name = table.Column<string>(type: "text", nullable: false),
                    master_id = table.Column<int>(type: "integer", nullable: false),
                    payable_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    receivable_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    custodian_id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_petty_cash_transaction_history", x => x.transaction_id);
                });

            migrationBuilder.CreateTable(
                name: "taxation_vetting_master",
                columns: table => new
                {
                    tvmid = table.Column<long>(type: "bigint", nullable: false),
                    tvmdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    invoice_master_id = table.Column<long>(type: "bigint", nullable: false),
                    prmaster_id = table.Column<long>(type: "bigint", nullable: false),
                    pomaster_id = table.Column<long>(type: "bigint", nullable: false),
                    vatrebatable_id = table.Column<int>(type: "integer", nullable: false),
                    vatrebatable_percent = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    vatrebatable_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    vdsrate_id = table.Column<long>(type: "bigint", nullable: false),
                    vdsrate_percent = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    vdsamount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    tdsmethod_id = table.Column<int>(type: "integer", nullable: false),
                    tdsrate_id = table.Column<long>(type: "bigint", nullable: false),
                    tdsrate = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    tdsamount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    reference_no = table.Column<string>(type: "text", nullable: false),
                    reference_keyword = table.Column<string>(type: "text", nullable: true),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
                    grand_total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    remarks = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_taxation_vetting_master", x => x.tvmid);
                });

            migrationBuilder.CreateTable(
                name: "taxation_vetting_payment",
                columns: table => new
                {
                    tvpid = table.Column<long>(type: "bigint", nullable: false),
                    tvpdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tvmid = table.Column<long>(type: "bigint", nullable: false),
                    service_period = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    payment_mode_id = table.Column<int>(type: "integer", nullable: false),
                    vdsrate_percent = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    vdsamount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    tdsrate_percent = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    tdsamount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    advance_adjust_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    cash_out_rate = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    cash_out_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    payable_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    net_payable_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    reference_no = table.Column<string>(type: "text", nullable: false),
                    reference_keyword = table.Column<string>(type: "text", nullable: true),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
                    grand_total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    purpose = table.Column<string>(type: "text", nullable: true),
                    is_draft = table.Column<bool>(type: "boolean", nullable: false),
                    vat_challan_no = table.Column<string>(type: "text", nullable: true),
                    challan_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("pk_taxation_vetting_payment", x => x.tvpid);
                });

            migrationBuilder.CreateTable(
                name: "taxation_vetting_payment_child",
                columns: table => new
                {
                    tvpchild_id = table.Column<long>(type: "bigint", nullable: false),
                    tvpid = table.Column<long>(type: "bigint", nullable: false),
                    ipayment_master_id = table.Column<long>(type: "bigint", nullable: false),
                    pomaster_id = table.Column<long>(type: "bigint", nullable: true),
                    supplier_id = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("pk_taxation_vetting_payment_child", x => x.tvpchild_id);
                });

            migrationBuilder.CreateTable(
                name: "taxation_vetting_payment_method",
                columns: table => new
                {
                    payment_method_id = table.Column<long>(type: "bigint", nullable: false),
                    tvpid = table.Column<long>(type: "bigint", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    from_or_to = table.Column<int>(type: "integer", nullable: false),
                    bank_id = table.Column<int>(type: "integer", nullable: false),
                    vendor_bank_name = table.Column<string>(type: "text", nullable: true),
                    branch_name = table.Column<string>(type: "text", nullable: true),
                    account_no = table.Column<string>(type: "text", nullable: true),
                    routing_no = table.Column<string>(type: "text", nullable: true),
                    swift_code = table.Column<string>(type: "text", nullable: true),
                    cheque_book_id = table.Column<int>(type: "integer", nullable: true),
                    leaf_no = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("pk_taxation_vetting_payment_method", x => x.payment_method_id);
                });

            migrationBuilder.CreateTable(
                name: "vat_tax_deduction_source",
                columns: table => new
                {
                    vtdsid = table.Column<long>(type: "bigint", nullable: false),
                    source_type_id = table.Column<int>(type: "integer", nullable: false),
                    section_or_service_code = table.Column<string>(type: "text", nullable: false),
                    service_name = table.Column<string>(type: "text", nullable: false),
                    rate_percent = table.Column<string>(type: "text", nullable: false),
                    financial_year_id = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("pk_vat_tax_deduction_source", x => x.vtdsid);
                });

            migrationBuilder.CreateTable(
                name: "voucher_category",
                columns: table => new
                {
                    voucher_category_id = table.Column<long>(type: "bigint", nullable: false),
                    category_name = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("pk_voucher_category", x => x.voucher_category_id);
                });

            migrationBuilder.CreateTable(
                name: "voucher_child",
                columns: table => new
                {
                    voucher_child_id = table.Column<long>(type: "bigint", nullable: false),
                    voucher_master_id = table.Column<long>(type: "bigint", nullable: false),
                    txn_type_id = table.Column<int>(type: "integer", nullable: false),
                    coaid = table.Column<long>(type: "bigint", nullable: false),
                    cost_center_id = table.Column<int>(type: "integer", nullable: false),
                    budget_head_id = table.Column<int>(type: "integer", nullable: false),
                    narration = table.Column<string>(type: "text", nullable: true),
                    mode_of_payment_id = table.Column<int>(type: "integer", nullable: false),
                    cbid = table.Column<long>(type: "bigint", nullable: false),
                    cbcid = table.Column<long>(type: "bigint", nullable: false),
                    leaf_no = table.Column<long>(type: "bigint", nullable: false),
                    debit_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    credit_amount = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("pk_voucher_child", x => x.voucher_child_id);
                });

            migrationBuilder.CreateTable(
                name: "voucher_master",
                columns: table => new
                {
                    voucher_master_id = table.Column<long>(type: "bigint", nullable: false),
                    reference_no = table.Column<string>(type: "text", nullable: false),
                    is_excel_upload = table.Column<bool>(type: "boolean", nullable: false),
                    voucher_type_id = table.Column<long>(type: "bigint", nullable: false),
                    remarks = table.Column<string>(type: "text", nullable: false),
                    voucher_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reference_keyword = table.Column<string>(type: "text", nullable: true),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_voucher_master", x => x.voucher_master_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accategory");

            migrationBuilder.DropTable(
                name: "acclass");

            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "bank");

            migrationBuilder.DropTable(
                name: "budget_child");

            migrationBuilder.DropTable(
                name: "budget_child_with_approval_panel_map");

            migrationBuilder.DropTable(
                name: "budget_master");

            migrationBuilder.DropTable(
                name: "chart_of_accounts");

            migrationBuilder.DropTable(
                name: "cheque_book");

            migrationBuilder.DropTable(
                name: "cheque_book_child");

            migrationBuilder.DropTable(
                name: "cost_category");

            migrationBuilder.DropTable(
                name: "cost_center");

            migrationBuilder.DropTable(
                name: "custodian_wallet");

            migrationBuilder.DropTable(
                name: "expense_claim_child");

            migrationBuilder.DropTable(
                name: "expense_claim_master");

            migrationBuilder.DropTable(
                name: "general_ledger");

            migrationBuilder.DropTable(
                name: "iouchild");

            migrationBuilder.DropTable(
                name: "ioumaster");

            migrationBuilder.DropTable(
                name: "iouor_expense_payment_child");

            migrationBuilder.DropTable(
                name: "iouor_expense_payment_master");

            migrationBuilder.DropTable(
                name: "petty_cash_advance_child");

            migrationBuilder.DropTable(
                name: "petty_cash_advance_master");

            migrationBuilder.DropTable(
                name: "petty_cash_expense_child");

            migrationBuilder.DropTable(
                name: "petty_cash_expense_master");

            migrationBuilder.DropTable(
                name: "petty_cash_payment_child");

            migrationBuilder.DropTable(
                name: "petty_cash_payment_master");

            migrationBuilder.DropTable(
                name: "petty_cash_reimburse_child");

            migrationBuilder.DropTable(
                name: "petty_cash_reimburse_master");

            migrationBuilder.DropTable(
                name: "petty_cash_transaction_history");

            migrationBuilder.DropTable(
                name: "taxation_vetting_master");

            migrationBuilder.DropTable(
                name: "taxation_vetting_payment");

            migrationBuilder.DropTable(
                name: "taxation_vetting_payment_child");

            migrationBuilder.DropTable(
                name: "taxation_vetting_payment_method");

            migrationBuilder.DropTable(
                name: "vat_tax_deduction_source");

            migrationBuilder.DropTable(
                name: "voucher_category");

            migrationBuilder.DropTable(
                name: "voucher_child");

            migrationBuilder.DropTable(
                name: "voucher_master");
        }
    }
}
