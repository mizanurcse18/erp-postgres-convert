using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Security.DAL.Migrations
{
    /// <inheritdoc />
    public partial class initialmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "assessment_year",
                columns: table => new
                {
                    assessment_year_id = table.Column<int>(type: "integer", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    year_description = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_assessment_year", x => x.assessment_year_id);
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
                name: "bank_account_info",
                columns: table => new
                {
                    bank_account_id = table.Column<int>(type: "integer", nullable: false),
                    person_id = table.Column<int>(type: "integer", nullable: false),
                    bank_type_id = table.Column<int>(type: "integer", nullable: false),
                    bank_id = table.Column<int>(type: "integer", nullable: false),
                    bank_branch_id = table.Column<int>(type: "integer", nullable: false),
                    account_type = table.Column<int>(type: "integer", nullable: true),
                    account_no = table.Column<string>(type: "text", nullable: true),
                    account_name = table.Column<string>(type: "text", nullable: true),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_bank_account_info", x => x.bank_account_id);
                });

            migrationBuilder.CreateTable(
                name: "business_support_item",
                columns: table => new
                {
                    item_id = table.Column<long>(type: "bigint", nullable: false),
                    item_name = table.Column<string>(type: "text", nullable: false),
                    item_description = table.Column<string>(type: "text", nullable: true),
                    item_code_prefix = table.Column<string>(type: "text", nullable: true),
                    item_code_suffix = table.Column<string>(type: "text", nullable: true),
                    item_code = table.Column<string>(type: "text", nullable: true),
                    item_sub_group_id = table.Column<long>(type: "bigint", nullable: true),
                    asset_type_id = table.Column<int>(type: "integer", nullable: true),
                    inventory_type_id = table.Column<int>(type: "integer", nullable: true),
                    glid = table.Column<long>(type: "bigint", nullable: true),
                    unit_id = table.Column<int>(type: "integer", nullable: true),
                    item_nature = table.Column<string>(type: "text", nullable: true),
                    price = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("pk_business_support_item", x => x.item_id);
                });

            migrationBuilder.CreateTable(
                name: "common_interface",
                columns: table => new
                {
                    menu_id = table.Column<int>(type: "integer", nullable: false),
                    table_name = table.Column<string>(type: "text", nullable: false),
                    key_fields = table.Column<string>(type: "text", nullable: false),
                    interface_type = table.Column<string>(type: "text", nullable: false),
                    grid_header_filter = table.Column<string>(type: "text", nullable: true),
                    grid_rows = table.Column<string>(type: "text", nullable: true),
                    grid_data_source = table.Column<string>(type: "text", nullable: true),
                    grid_data_sort = table.Column<string>(type: "text", nullable: true),
                    grid_data_order = table.Column<string>(type: "text", nullable: true),
                    is_row_select_action = table.Column<bool>(type: "boolean", nullable: false),
                    tabs = table.Column<string>(type: "text", nullable: true),
                    columns_per_row = table.Column<int>(type: "integer", nullable: false),
                    assembly_info = table.Column<string>(type: "text", nullable: true),
                    get_data_get_source = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_common_interface", x => x.menu_id);
                });

            migrationBuilder.CreateTable(
                name: "common_interface_fields",
                columns: table => new
                {
                    row_id = table.Column<int>(type: "integer", nullable: false),
                    menu_id = table.Column<int>(type: "integer", nullable: false),
                    data_member = table.Column<string>(type: "text", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false),
                    data_type = table.Column<string>(type: "text", nullable: false),
                    placeholder = table.Column<string>(type: "text", nullable: true),
                    seq_no = table.Column<decimal>(type: "numeric", nullable: false),
                    tab_name = table.Column<string>(type: "text", nullable: true),
                    required = table.Column<bool>(type: "boolean", nullable: false),
                    is_visible = table.Column<bool>(type: "boolean", nullable: false),
                    is_read_only = table.Column<bool>(type: "boolean", nullable: false),
                    autocomplete_props = table.Column<string>(type: "text", nullable: true),
                    autocomplete_source = table.Column<string>(type: "text", nullable: true),
                    is_primary_key = table.Column<bool>(type: "boolean", nullable: false),
                    is_auto_increment = table.Column<bool>(type: "boolean", nullable: false),
                    rows = table.Column<int>(type: "integer", nullable: false),
                    validation_rules = table.Column<string>(type: "text", nullable: true),
                    is_child = table.Column<bool>(type: "boolean", nullable: false),
                    child_table_name = table.Column<string>(type: "text", nullable: true),
                    child_table_style = table.Column<string>(type: "text", nullable: true),
                    child_table_title = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_common_interface_fields", x => x.row_id);
                });

            migrationBuilder.CreateTable(
                name: "company",
                columns: table => new
                {
                    company_id = table.Column<string>(type: "text", nullable: false),
                    company_name = table.Column<string>(type: "text", nullable: false),
                    company_report_head = table.Column<string>(type: "text", nullable: true),
                    company_short_code = table.Column<string>(type: "text", nullable: true),
                    company_address = table.Column<string>(type: "text", nullable: true),
                    tin = table.Column<string>(type: "text", nullable: true),
                    bin = table.Column<string>(type: "text", nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    vatreg_no = table.Column<string>(type: "text", nullable: true),
                    ircno = table.Column<string>(type: "text", nullable: true),
                    company_logo = table.Column<byte[]>(type: "bytea", nullable: true),
                    default_company_in_charge = table.Column<string>(type: "text", nullable: true),
                    license_code = table.Column<string>(type: "text", nullable: true),
                    company_group_id = table.Column<string>(type: "text", nullable: true),
                    seq_no = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_company", x => x.company_id);
                });

            migrationBuilder.CreateTable(
                name: "company_terms",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    terms = table.Column<string>(type: "text", nullable: false),
                    terms_type = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("pk_company_terms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "currency",
                columns: table => new
                {
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    currency_name = table.Column<string>(type: "text", nullable: false),
                    currency_description = table.Column<string>(type: "text", nullable: false),
                    currency_rate = table.Column<decimal>(type: "numeric", nullable: false),
                    currency_code = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("pk_currency", x => x.currency_id);
                });

            migrationBuilder.CreateTable(
                name: "district",
                columns: table => new
                {
                    district_id = table.Column<int>(type: "integer", nullable: false),
                    division_id = table.Column<int>(type: "integer", nullable: true),
                    district_name = table.Column<string>(type: "text", nullable: false),
                    bangla_district_name = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("pk_district", x => x.district_id);
                });

            migrationBuilder.CreateTable(
                name: "division",
                columns: table => new
                {
                    division_id = table.Column<int>(type: "integer", nullable: false),
                    division_name = table.Column<string>(type: "text", nullable: false),
                    bangla_division_name = table.Column<string>(type: "text", nullable: true),
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
                name: "driver_details",
                columns: table => new
                {
                    driver_id = table.Column<int>(type: "integer", nullable: false),
                    driver_name = table.Column<string>(type: "text", nullable: false),
                    contact_number = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: true),
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
                    table.PrimaryKey("pk_driver_details", x => x.driver_id);
                });

            migrationBuilder.CreateTable(
                name: "employee_profile_approval",
                columns: table => new
                {
                    epaid = table.Column<int>(type: "integer", nullable: false),
                    table_name = table.Column<string>(type: "text", nullable: true),
                    column_name = table.Column<string>(type: "text", nullable: true),
                    old_value = table.Column<string>(type: "text", nullable: true),
                    new_value = table.Column<string>(type: "text", nullable: true),
                    person_id = table.Column<int>(type: "integer", nullable: false),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_employee_profile_approval", x => x.epaid);
                });

            migrationBuilder.CreateTable(
                name: "file_upload",
                columns: table => new
                {
                    fuid = table.Column<int>(type: "integer", nullable: false),
                    file_path = table.Column<string>(type: "text", nullable: false),
                    file_name = table.Column<string>(type: "text", nullable: false),
                    file_type = table.Column<string>(type: "text", nullable: false),
                    original_name = table.Column<string>(type: "text", nullable: false),
                    reference_id = table.Column<int>(type: "integer", nullable: false),
                    table_name = table.Column<string>(type: "text", nullable: false),
                    parent_fuid = table.Column<int>(type: "integer", nullable: true),
                    is_folder = table.Column<bool>(type: "boolean", nullable: false),
                    size_in_kb = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("pk_file_upload", x => x.fuid);
                });

            migrationBuilder.CreateTable(
                name: "financial_year",
                columns: table => new
                {
                    financial_year_id = table.Column<int>(type: "integer", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    year_description = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_financial_year", x => x.financial_year_id);
                });

            migrationBuilder.CreateTable(
                name: "ipaddress",
                columns: table => new
                {
                    ipaddress_id = table.Column<int>(type: "integer", nullable: false),
                    ipnumber = table.Column<string>(type: "text", nullable: false),
                    iptype = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_ipaddress", x => x.ipaddress_id);
                });

            migrationBuilder.CreateTable(
                name: "location",
                columns: table => new
                {
                    location_id = table.Column<int>(type: "integer", nullable: false),
                    location_name = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: true),
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
                    table.PrimaryKey("pk_location", x => x.location_id);
                });

            migrationBuilder.CreateTable(
                name: "menu",
                columns: table => new
                {
                    menu_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    parent_id = table.Column<int>(type: "integer", nullable: false),
                    application_id = table.Column<int>(type: "integer", nullable: true),
                    id = table.Column<string>(type: "text", nullable: true),
                    title = table.Column<string>(type: "text", nullable: true),
                    translate = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<string>(type: "text", nullable: true),
                    icon = table.Column<string>(type: "text", nullable: true),
                    url = table.Column<string>(type: "text", nullable: true),
                    badge = table.Column<string>(type: "text", nullable: true),
                    target = table.Column<string>(type: "text", nullable: true),
                    exact = table.Column<bool>(type: "boolean", nullable: false),
                    auth = table.Column<string>(type: "text", nullable: true),
                    parameters = table.Column<string>(type: "text", nullable: true),
                    is_visible = table.Column<bool>(type: "boolean", nullable: false),
                    sequence_no = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_menu", x => x.menu_id);
                });

            migrationBuilder.CreateTable(
                name: "menu_api_paths",
                columns: table => new
                {
                    mapid = table.Column<int>(type: "integer", nullable: false),
                    menu_id = table.Column<int>(type: "integer", nullable: false),
                    module = table.Column<string>(type: "text", nullable: false),
                    controller = table.Column<string>(type: "text", nullable: false),
                    api_path = table.Column<string>(type: "text", nullable: false),
                    action_type = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("pk_menu_api_paths", x => x.mapid);
                });

            migrationBuilder.CreateTable(
                name: "nfachild",
                columns: table => new
                {
                    nfacid = table.Column<int>(type: "integer", nullable: false),
                    item_name = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    unit = table.Column<decimal>(type: "numeric", nullable: true),
                    unit_type = table.Column<string>(type: "text", nullable: true),
                    unit_price = table.Column<decimal>(type: "numeric", nullable: true),
                    vat_tax_status = table.Column<string>(type: "text", nullable: true),
                    vendor = table.Column<string>(type: "text", nullable: true),
                    total_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    nfamaster_id = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "text", nullable: true),
                    duration = table.Column<string>(type: "text", nullable: true),
                    cost_type = table.Column<string>(type: "text", nullable: true),
                    estimated_budget_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    aitpercent = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("pk_nfachild", x => x.nfacid);
                });

            migrationBuilder.CreateTable(
                name: "nfachild_strategic",
                columns: table => new
                {
                    nfacsid = table.Column<int>(type: "integer", nullable: false),
                    item_id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    uom = table.Column<int>(type: "integer", nullable: false),
                    qty = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    nfamaster_id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_nfachild_strategic", x => x.nfacsid);
                });

            migrationBuilder.CreateTable(
                name: "nfamaster",
                columns: table => new
                {
                    nfaid = table.Column<int>(type: "integer", nullable: false),
                    nfadate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reference_no = table.Column<string>(type: "text", nullable: true),
                    subject = table.Column<string>(type: "text", nullable: true),
                    preamble = table.Column<string>(type: "text", nullable: true),
                    price_and_commercial = table.Column<string>(type: "text", nullable: true),
                    solicitation = table.Column<string>(type: "text", nullable: true),
                    budget_plan_remarks = table.Column<string>(type: "text", nullable: true),
                    grand_total = table.Column<decimal>(type: "numeric", nullable: false),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
                    template_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    description_image_url = table.Column<string>(type: "text", nullable: true),
                    reference_keyword = table.Column<string>(type: "text", nullable: true),
                    is_draft = table.Column<bool>(type: "boolean", nullable: false),
                    budget_plan_category_id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_nfamaster", x => x.nfaid);
                });

            migrationBuilder.CreateTable(
                name: "nominee_information",
                columns: table => new
                {
                    niid = table.Column<int>(type: "integer", nullable: false),
                    nominee_name = table.Column<string>(type: "text", nullable: false),
                    nominee_address = table.Column<string>(type: "text", nullable: false),
                    person_id = table.Column<int>(type: "integer", nullable: false),
                    relation_ship = table.Column<string>(type: "text", nullable: true),
                    date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    percentage = table.Column<decimal>(type: "numeric", nullable: false),
                    nominee_behalf = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_nominee_information", x => x.niid);
                });

            migrationBuilder.CreateTable(
                name: "onboarding_user",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    user_name = table.Column<string>(type: "text", nullable: false),
                    full_name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    password_salt = table.Column<byte[]>(type: "bytea", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_submit = table.Column<bool>(type: "boolean", nullable: false),
                    person_id = table.Column<int>(type: "integer", nullable: true),
                    is_forced_login = table.Column<bool>(type: "boolean", nullable: false),
                    division = table.Column<string>(type: "text", nullable: true),
                    department = table.Column<string>(type: "text", nullable: true),
                    designation = table.Column<string>(type: "text", nullable: true),
                    mobile_no = table.Column<string>(type: "text", nullable: true),
                    location = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_onboarding_user", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "period",
                columns: table => new
                {
                    period_id = table.Column<int>(type: "integer", nullable: false),
                    financial_year_id = table.Column<int>(type: "integer", nullable: false),
                    period_start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    period_endt_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    seq_no = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("pk_period", x => x.period_id);
                });

            migrationBuilder.CreateTable(
                name: "person",
                columns: table => new
                {
                    person_id = table.Column<int>(type: "integer", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: true),
                    mobile = table.Column<string>(type: "text", nullable: true),
                    mobile2 = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    gender_id = table.Column<int>(type: "integer", nullable: true),
                    date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    alternate_email = table.Column<string>(type: "text", nullable: true),
                    religion_id = table.Column<int>(type: "integer", nullable: true),
                    nationality = table.Column<string>(type: "text", nullable: true),
                    is_bangladeshi = table.Column<string>(type: "text", nullable: true),
                    blood_group_id = table.Column<int>(type: "integer", nullable: true),
                    person_type_id = table.Column<int>(type: "integer", nullable: true),
                    father_name = table.Column<string>(type: "text", nullable: true),
                    mother_name = table.Column<string>(type: "text", nullable: true),
                    nidnumber = table.Column<string>(type: "text", nullable: true),
                    passport_number = table.Column<string>(type: "text", nullable: true),
                    passport_issue_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    passport_expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    birth_certificate = table.Column<string>(type: "text", nullable: true),
                    driving_license = table.Column<string>(type: "text", nullable: true),
                    tinnumber = table.Column<string>(type: "text", nullable: true),
                    tax_zone = table.Column<string>(type: "text", nullable: true),
                    marital_status_id = table.Column<int>(type: "integer", nullable: true),
                    bangla_name = table.Column<string>(type: "text", nullable: true),
                    bangla_full_name = table.Column<string>(type: "text", nullable: true),
                    marriage_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    father_dob = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_father_alive = table.Column<bool>(type: "boolean", nullable: false),
                    mother_dob = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_mother_alive = table.Column<bool>(type: "boolean", nullable: false),
                    spouse_name = table.Column<string>(type: "text", nullable: true),
                    spouse_dob = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    spouse_gender_id = table.Column<int>(type: "integer", nullable: true),
                    marital_details = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_person", x => x.person_id);
                });

            migrationBuilder.CreateTable(
                name: "person_academic_info",
                columns: table => new
                {
                    paiid = table.Column<int>(type: "integer", nullable: false),
                    person_id = table.Column<int>(type: "integer", nullable: false),
                    degree_or_certification = table.Column<string>(type: "text", nullable: false),
                    institute_name = table.Column<string>(type: "text", nullable: false),
                    board_or_university = table.Column<string>(type: "text", nullable: true),
                    subject_or_area = table.Column<string>(type: "text", nullable: false),
                    passing_year = table.Column<int>(type: "integer", nullable: false),
                    result = table.Column<string>(type: "text", nullable: false),
                    is_last_academic = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_person_academic_info", x => x.paiid);
                });

            migrationBuilder.CreateTable(
                name: "person_address_info",
                columns: table => new
                {
                    paiid = table.Column<int>(type: "integer", nullable: false),
                    person_id = table.Column<int>(type: "integer", nullable: false),
                    address_type_id = table.Column<int>(type: "integer", nullable: false),
                    district_id = table.Column<int>(type: "integer", nullable: true),
                    thana_id = table.Column<int>(type: "integer", nullable: true),
                    post_code = table.Column<int>(type: "integer", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    is_same_as_present_address = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_person_address_info", x => x.paiid);
                });

            migrationBuilder.CreateTable(
                name: "person_award_info",
                columns: table => new
                {
                    paiid = table.Column<int>(type: "integer", nullable: false),
                    person_id = table.Column<int>(type: "integer", nullable: false),
                    award_type = table.Column<string>(type: "text", nullable: false),
                    institute_name = table.Column<string>(type: "text", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_person_award_info", x => x.paiid);
                });

            migrationBuilder.CreateTable(
                name: "person_emergency_contact_info",
                columns: table => new
                {
                    peciid = table.Column<int>(type: "integer", nullable: false),
                    person_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    contact_no = table.Column<string>(type: "text", nullable: true),
                    relationship = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_person_emergency_contact_info", x => x.peciid);
                });

            migrationBuilder.CreateTable(
                name: "person_employment_info",
                columns: table => new
                {
                    peiid = table.Column<int>(type: "integer", nullable: false),
                    person_id = table.Column<int>(type: "integer", nullable: false),
                    company_name = table.Column<string>(type: "text", nullable: false),
                    company_business = table.Column<string>(type: "text", nullable: false),
                    designation = table.Column<string>(type: "text", nullable: false),
                    department = table.Column<string>(type: "text", nullable: true),
                    responsibilities = table.Column<string>(type: "text", nullable: true),
                    company_location = table.Column<string>(type: "text", nullable: true),
                    from_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    to_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_currently_working = table.Column<bool>(type: "boolean", nullable: false),
                    area_of_experiences = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_person_employment_info", x => x.peiid);
                });

            migrationBuilder.CreateTable(
                name: "person_family_info",
                columns: table => new
                {
                    pfiid = table.Column<int>(type: "integer", nullable: false),
                    person_id = table.Column<int>(type: "integer", nullable: false),
                    relationship_type_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    gender_id = table.Column<int>(type: "integer", nullable: true),
                    date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("pk_person_family_info", x => x.pfiid);
                });

            migrationBuilder.CreateTable(
                name: "person_image",
                columns: table => new
                {
                    piid = table.Column<int>(type: "integer", nullable: false),
                    image_path = table.Column<string>(type: "text", nullable: true),
                    image_name = table.Column<string>(type: "text", nullable: true),
                    image_type = table.Column<string>(type: "text", nullable: true),
                    image_original_name = table.Column<string>(type: "text", nullable: true),
                    person_id = table.Column<int>(type: "integer", nullable: false),
                    image_category = table.Column<int>(type: "integer", nullable: true),
                    is_favorite = table.Column<bool>(type: "boolean", nullable: false),
                    is_signature = table.Column<bool>(type: "boolean", nullable: false),
                    gallery_id = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("pk_person_image", x => x.piid);
                });

            migrationBuilder.CreateTable(
                name: "person_professional_certification_info",
                columns: table => new
                {
                    ppciid = table.Column<int>(type: "integer", nullable: false),
                    person_id = table.Column<int>(type: "integer", nullable: false),
                    certification = table.Column<string>(type: "text", nullable: false),
                    institute_name = table.Column<string>(type: "text", nullable: false),
                    location = table.Column<string>(type: "text", nullable: true),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("pk_person_professional_certification_info", x => x.ppciid);
                });

            migrationBuilder.CreateTable(
                name: "person_reference_info",
                columns: table => new
                {
                    priid = table.Column<int>(type: "integer", nullable: false),
                    person_id = table.Column<int>(type: "integer", nullable: false),
                    reference_type_id = table.Column<int>(type: "integer", nullable: false),
                    reference_name = table.Column<string>(type: "text", nullable: false),
                    organization = table.Column<string>(type: "text", nullable: true),
                    designation = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    mobile = table.Column<string>(type: "text", nullable: true),
                    relationship = table.Column<string>(type: "text", nullable: true),
                    is_company_employee = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_person_reference_info", x => x.priid);
                });

            migrationBuilder.CreateTable(
                name: "person_training_info",
                columns: table => new
                {
                    ptiid = table.Column<int>(type: "integer", nullable: false),
                    person_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    trainer = table.Column<string>(type: "text", nullable: true),
                    country_id = table.Column<int>(type: "integer", nullable: false),
                    institute_name = table.Column<string>(type: "text", nullable: false),
                    training_year = table.Column<int>(type: "integer", nullable: false),
                    duration_type = table.Column<string>(type: "text", nullable: true),
                    duration = table.Column<int>(type: "integer", nullable: true),
                    location = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_person_training_info", x => x.ptiid);
                });

            migrationBuilder.CreateTable(
                name: "person_work_experience",
                columns: table => new
                {
                    pweid = table.Column<int>(type: "integer", nullable: false),
                    person_id = table.Column<int>(type: "integer", nullable: false),
                    company_name = table.Column<string>(type: "text", nullable: false),
                    company_business = table.Column<string>(type: "text", nullable: false),
                    responsibilities = table.Column<string>(type: "text", nullable: false),
                    designation = table.Column<string>(type: "text", nullable: false),
                    department = table.Column<string>(type: "text", nullable: true),
                    company_location = table.Column<string>(type: "text", nullable: true),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_last_employer = table.Column<bool>(type: "boolean", nullable: false),
                    leaving_reason = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_person_work_experience", x => x.pweid);
                });

            migrationBuilder.CreateTable(
                name: "report_suite",
                columns: table => new
                {
                    report_id = table.Column<int>(type: "integer", nullable: false),
                    application_id = table.Column<int>(type: "integer", nullable: true),
                    parent_id = table.Column<int>(type: "integer", nullable: true),
                    display_name = table.Column<string>(type: "text", nullable: false),
                    report_name = table.Column<string>(type: "text", nullable: true),
                    report_path = table.Column<string>(type: "text", nullable: true),
                    con_name = table.Column<string>(type: "text", nullable: true),
                    seq_no = table.Column<decimal>(type: "numeric", nullable: false),
                    is_visible = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_report_suite", x => x.report_id);
                });

            migrationBuilder.CreateTable(
                name: "report_suite_field",
                columns: table => new
                {
                    report_field_id = table.Column<int>(type: "integer", nullable: false),
                    report_id = table.Column<int>(type: "integer", nullable: false),
                    value_field = table.Column<string>(type: "text", nullable: false),
                    label_field = table.Column<string>(type: "text", nullable: true),
                    label = table.Column<string>(type: "text", nullable: false),
                    default_value = table.Column<string>(type: "text", nullable: true),
                    field_type = table.Column<string>(type: "text", nullable: true),
                    map_field = table.Column<string>(type: "text", nullable: true),
                    reference_source = table.Column<string>(type: "text", nullable: true),
                    operators = table.Column<string>(type: "text", nullable: true),
                    filter_only = table.Column<bool>(type: "boolean", nullable: false),
                    seq_no = table.Column<decimal>(type: "numeric", nullable: false),
                    is_sys_parameter = table.Column<bool>(type: "boolean", nullable: false),
                    multi_select = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_report_suite_field", x => x.report_field_id);
                });

            migrationBuilder.CreateTable(
                name: "report_suite_parent_field",
                columns: table => new
                {
                    report_parent_id = table.Column<int>(type: "integer", nullable: false),
                    report_field_id = table.Column<int>(type: "integer", nullable: false),
                    parent_field = table.Column<string>(type: "text", nullable: false),
                    condition = table.Column<string>(type: "text", nullable: true),
                    blank_allow = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_report_suite_parent_field", x => x.report_parent_id);
                });

            migrationBuilder.CreateTable(
                name: "security_group_master",
                columns: table => new
                {
                    security_group_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    security_group_name = table.Column<string>(type: "text", nullable: true),
                    sec_group_description = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_security_group_master", x => x.security_group_id);
                });

            migrationBuilder.CreateTable(
                name: "security_group_rule_child",
                columns: table => new
                {
                    security_group_rule_child_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    security_group_id = table.Column<int>(type: "integer", nullable: false),
                    security_rule_id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_security_group_rule_child", x => x.security_group_rule_child_id);
                });

            migrationBuilder.CreateTable(
                name: "security_group_user_child",
                columns: table => new
                {
                    security_group_user_child_id = table.Column<int>(type: "integer", nullable: false),
                    security_group_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_security_group_user_child", x => x.security_group_user_child_id);
                });

            migrationBuilder.CreateTable(
                name: "security_rule_master",
                columns: table => new
                {
                    security_rule_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    security_rule_name = table.Column<string>(type: "text", nullable: true),
                    security_rule_description = table.Column<string>(type: "text", nullable: true),
                    application_id = table.Column<short>(type: "smallint", nullable: true),
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
                    table.PrimaryKey("pk_security_rule_master", x => x.security_rule_id);
                });

            migrationBuilder.CreateTable(
                name: "security_rule_permission_child",
                columns: table => new
                {
                    security_rule_permission_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    security_rule_id = table.Column<int>(type: "integer", nullable: false),
                    menu_id = table.Column<int>(type: "integer", nullable: false),
                    can_read = table.Column<bool>(type: "boolean", nullable: true),
                    can_create = table.Column<bool>(type: "boolean", nullable: true),
                    can_update = table.Column<bool>(type: "boolean", nullable: true),
                    can_delete = table.Column<bool>(type: "boolean", nullable: true),
                    can_report = table.Column<bool>(type: "boolean", nullable: true),
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
                    table.PrimaryKey("pk_security_rule_permission_child", x => x.security_rule_permission_id);
                });

            migrationBuilder.CreateTable(
                name: "support_requisition_item",
                columns: table => new
                {
                    item_id = table.Column<long>(type: "bigint", nullable: false),
                    item_name = table.Column<string>(type: "text", nullable: false),
                    item_description = table.Column<string>(type: "text", nullable: true),
                    item_code_prefix = table.Column<string>(type: "text", nullable: true),
                    item_code_suffix = table.Column<string>(type: "text", nullable: true),
                    item_code = table.Column<string>(type: "text", nullable: true),
                    item_sub_group_id = table.Column<long>(type: "bigint", nullable: true),
                    asset_type_id = table.Column<int>(type: "integer", nullable: true),
                    inventory_type_id = table.Column<int>(type: "integer", nullable: true),
                    glid = table.Column<long>(type: "bigint", nullable: true),
                    unit_id = table.Column<int>(type: "integer", nullable: true),
                    item_nature = table.Column<string>(type: "text", nullable: true),
                    price = table.Column<decimal>(type: "numeric", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    external_id = table.Column<string>(type: "text", nullable: true),
                    category_id = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("pk_support_requisition_item", x => x.item_id);
                });

            migrationBuilder.CreateTable(
                name: "system_configuration",
                columns: table => new
                {
                    system_configuration_id = table.Column<int>(type: "integer", nullable: false),
                    user_account_locked_duration_in_min = table.Column<int>(type: "integer", nullable: false),
                    user_password_changed_duration_in_days = table.Column<int>(type: "integer", nullable: false),
                    access_failed_count_max = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_system_configuration", x => x.system_configuration_id);
                });

            migrationBuilder.CreateTable(
                name: "system_variable",
                columns: table => new
                {
                    system_variable_id = table.Column<int>(type: "integer", nullable: false),
                    entity_type_id = table.Column<int>(type: "integer", nullable: false),
                    entity_type_name = table.Column<string>(type: "text", nullable: true),
                    system_variable_code = table.Column<string>(type: "text", nullable: false),
                    system_variable_description = table.Column<string>(type: "text", nullable: true),
                    numeric_value = table.Column<int>(type: "integer", nullable: true),
                    sequence = table.Column<int>(type: "integer", nullable: false),
                    is_system_generated = table.Column<bool>(type: "boolean", nullable: false),
                    is_inactive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_system_variable", x => x.system_variable_id);
                });

            migrationBuilder.CreateTable(
                name: "thana",
                columns: table => new
                {
                    thana_id = table.Column<int>(type: "integer", nullable: false),
                    district_id = table.Column<int>(type: "integer", nullable: true),
                    thana_name = table.Column<string>(type: "text", nullable: false),
                    bangla_thana_name = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("pk_thana", x => x.thana_id);
                });

            migrationBuilder.CreateTable(
                name: "tutorial_master",
                columns: table => new
                {
                    tmid = table.Column<int>(type: "integer", nullable: false),
                    tutorial_type_id = table.Column<string>(type: "text", nullable: false),
                    file_name = table.Column<string>(type: "text", nullable: false),
                    file_type = table.Column<string>(type: "text", nullable: false),
                    original_name = table.Column<string>(type: "text", nullable: false),
                    url = table.Column<string>(type: "text", nullable: false),
                    table_name = table.Column<string>(type: "text", nullable: false),
                    video_id = table.Column<string>(type: "text", nullable: true),
                    color = table.Column<string>(type: "text", nullable: true),
                    title = table.Column<string>(type: "text", nullable: false),
                    department_id = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_tutorial_master", x => x.tmid);
                });

            migrationBuilder.CreateTable(
                name: "unit",
                columns: table => new
                {
                    unit_id = table.Column<int>(type: "integer", nullable: false),
                    unit_code = table.Column<string>(type: "text", nullable: false),
                    unit_short_code = table.Column<string>(type: "text", nullable: false),
                    lelative_factor = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("pk_unit", x => x.unit_id);
                });

            migrationBuilder.CreateTable(
                name: "user_company",
                columns: table => new
                {
                    company_id = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_user_company", x => new { x.user_id, x.company_id });
                });

            migrationBuilder.CreateTable(
                name: "user_log_tracker",
                columns: table => new
                {
                    loged_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    log_in_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    log_out_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_live = table.Column<bool>(type: "boolean", nullable: true),
                    ipaddress = table.Column<string>(type: "text", nullable: true),
                    company_id = table.Column<string>(type: "text", nullable: true),
                    is_login_failed = table.Column<bool>(type: "boolean", nullable: false),
                    reason_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_log_tracker", x => x.loged_id);
                });

            migrationBuilder.CreateTable(
                name: "user_profile",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    user_full_name = table.Column<string>(type: "text", nullable: true),
                    distribution_house_id = table.Column<int>(type: "integer", nullable: true),
                    region_id = table.Column<int>(type: "integer", nullable: true),
                    cluster_id = table.Column<int>(type: "integer", nullable: true),
                    position_id = table.Column<int>(type: "integer", nullable: true),
                    contact_number = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    joining_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    longitude = table.Column<string>(type: "text", nullable: true),
                    latitude = table.Column<string>(type: "text", nullable: true),
                    visit_type_id = table.Column<int>(type: "integer", nullable: false),
                    parent_code = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_user_profile", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "user_theme_setting",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    settings = table.Column<string>(type: "text", nullable: true),
                    short_cuts = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_theme_setting", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "user_token_black_list",
                columns: table => new
                {
                    utbid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    token = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("pk_user_token_black_list", x => x.utbid);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    user_name = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    password_salt = table.Column<byte[]>(type: "bytea", nullable: false),
                    default_application_id = table.Column<int>(type: "integer", nullable: true),
                    company_id = table.Column<string>(type: "text", nullable: false),
                    is_admin = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    in_active_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    access_failed_count = table.Column<int>(type: "integer", nullable: true),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    person_id = table.Column<int>(type: "integer", nullable: true),
                    is_forced_login = table.Column<bool>(type: "boolean", nullable: false),
                    forgot_password_token = table.Column<string>(type: "text", nullable: true),
                    token_validity_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    default_menu_id = table.Column<int>(type: "integer", nullable: true),
                    change_password_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_locked = table.Column<bool>(type: "boolean", nullable: false),
                    locked_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reason_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "users_otp",
                columns: table => new
                {
                    uotpid = table.Column<int>(type: "integer", nullable: false),
                    otphash = table.Column<string>(type: "text", nullable: false),
                    is_expired = table.Column<bool>(type: "boolean", nullable: false),
                    expired_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false),
                    smppresponse = table.Column<string>(type: "text", nullable: true),
                    year = table.Column<int>(type: "integer", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: false),
                    category_type = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_users_otp", x => x.uotpid);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_details",
                columns: table => new
                {
                    vehicle_id = table.Column<int>(type: "integer", nullable: false),
                    vehicle_reg_no = table.Column<string>(type: "text", nullable: false),
                    details = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: true),
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
                    table.PrimaryKey("pk_vehicle_details", x => x.vehicle_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_user_name",
                table: "users",
                column: "user_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assessment_year");

            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "bank_account_info");

            migrationBuilder.DropTable(
                name: "business_support_item");

            migrationBuilder.DropTable(
                name: "common_interface");

            migrationBuilder.DropTable(
                name: "common_interface_fields");

            migrationBuilder.DropTable(
                name: "company");

            migrationBuilder.DropTable(
                name: "company_terms");

            migrationBuilder.DropTable(
                name: "currency");

            migrationBuilder.DropTable(
                name: "district");

            migrationBuilder.DropTable(
                name: "division");

            migrationBuilder.DropTable(
                name: "driver_details");

            migrationBuilder.DropTable(
                name: "employee_profile_approval");

            migrationBuilder.DropTable(
                name: "file_upload");

            migrationBuilder.DropTable(
                name: "financial_year");

            migrationBuilder.DropTable(
                name: "ipaddress");

            migrationBuilder.DropTable(
                name: "location");

            migrationBuilder.DropTable(
                name: "menu");

            migrationBuilder.DropTable(
                name: "menu_api_paths");

            migrationBuilder.DropTable(
                name: "nfachild");

            migrationBuilder.DropTable(
                name: "nfachild_strategic");

            migrationBuilder.DropTable(
                name: "nfamaster");

            migrationBuilder.DropTable(
                name: "nominee_information");

            migrationBuilder.DropTable(
                name: "onboarding_user");

            migrationBuilder.DropTable(
                name: "period");

            migrationBuilder.DropTable(
                name: "person");

            migrationBuilder.DropTable(
                name: "person_academic_info");

            migrationBuilder.DropTable(
                name: "person_address_info");

            migrationBuilder.DropTable(
                name: "person_award_info");

            migrationBuilder.DropTable(
                name: "person_emergency_contact_info");

            migrationBuilder.DropTable(
                name: "person_employment_info");

            migrationBuilder.DropTable(
                name: "person_family_info");

            migrationBuilder.DropTable(
                name: "person_image");

            migrationBuilder.DropTable(
                name: "person_professional_certification_info");

            migrationBuilder.DropTable(
                name: "person_reference_info");

            migrationBuilder.DropTable(
                name: "person_training_info");

            migrationBuilder.DropTable(
                name: "person_work_experience");

            migrationBuilder.DropTable(
                name: "report_suite");

            migrationBuilder.DropTable(
                name: "report_suite_field");

            migrationBuilder.DropTable(
                name: "report_suite_parent_field");

            migrationBuilder.DropTable(
                name: "security_group_master");

            migrationBuilder.DropTable(
                name: "security_group_rule_child");

            migrationBuilder.DropTable(
                name: "security_group_user_child");

            migrationBuilder.DropTable(
                name: "security_rule_master");

            migrationBuilder.DropTable(
                name: "security_rule_permission_child");

            migrationBuilder.DropTable(
                name: "support_requisition_item");

            migrationBuilder.DropTable(
                name: "system_configuration");

            migrationBuilder.DropTable(
                name: "system_variable");

            migrationBuilder.DropTable(
                name: "thana");

            migrationBuilder.DropTable(
                name: "tutorial_master");

            migrationBuilder.DropTable(
                name: "unit");

            migrationBuilder.DropTable(
                name: "user_company");

            migrationBuilder.DropTable(
                name: "user_log_tracker");

            migrationBuilder.DropTable(
                name: "user_profile");

            migrationBuilder.DropTable(
                name: "user_theme_setting");

            migrationBuilder.DropTable(
                name: "user_token_black_list");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "users_otp");

            migrationBuilder.DropTable(
                name: "vehicle_details");
        }
    }
}
