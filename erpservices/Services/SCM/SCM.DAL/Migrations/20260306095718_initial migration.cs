using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SCM.DAL.Migrations
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
                name: "inventory_current_stock",
                columns: table => new
                {
                    icsid = table.Column<long>(type: "bigint", nullable: false),
                    item_id = table.Column<long>(type: "bigint", nullable: false),
                    current_stock_qty = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_current_stock", x => x.icsid);
                });

            migrationBuilder.CreateTable(
                name: "inventory_transaction",
                columns: table => new
                {
                    itid = table.Column<long>(type: "bigint", nullable: false),
                    warehouse_id = table.Column<long>(type: "bigint", nullable: false),
                    prmaster_id = table.Column<long>(type: "bigint", nullable: true),
                    pomaster_id = table.Column<long>(type: "bigint", nullable: true),
                    mrid = table.Column<long>(type: "bigint", nullable: true),
                    transfer_id = table.Column<long>(type: "bigint", nullable: true),
                    store_request_id = table.Column<long>(type: "bigint", nullable: true),
                    item_id = table.Column<long>(type: "bigint", nullable: false),
                    batch_no = table.Column<long>(type: "bigint", nullable: true),
                    transaction_qty = table.Column<decimal>(type: "numeric", nullable: false),
                    item_rate = table.Column<decimal>(type: "numeric", nullable: false),
                    in_or_out = table.Column<string>(type: "text", nullable: false),
                    is_transfer = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_inventory_transaction", x => x.itid);
                });

            migrationBuilder.CreateTable(
                name: "inventory_warehouse_current_stock",
                columns: table => new
                {
                    iwcsid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    item_id = table.Column<long>(type: "bigint", nullable: false),
                    warehouse_id = table.Column<long>(type: "bigint", nullable: false),
                    current_stock_qty = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_warehouse_current_stock", x => x.iwcsid);
                });

            migrationBuilder.CreateTable(
                name: "invoice_child",
                columns: table => new
                {
                    invoice_child_id = table.Column<long>(type: "bigint", nullable: false),
                    invoice_master_id = table.Column<long>(type: "bigint", nullable: false),
                    mrid = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("pk_invoice_child", x => x.invoice_child_id);
                });

            migrationBuilder.CreateTable(
                name: "invoice_child_scc",
                columns: table => new
                {
                    invoice_child_sccid = table.Column<long>(type: "bigint", nullable: false),
                    invoice_master_id = table.Column<long>(type: "bigint", nullable: false),
                    sccmid = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("pk_invoice_child_scc", x => x.invoice_child_sccid);
                });

            migrationBuilder.CreateTable(
                name: "invoice_master",
                columns: table => new
                {
                    invoice_master_id = table.Column<long>(type: "bigint", nullable: false),
                    invoice_no = table.Column<string>(type: "text", nullable: false),
                    invoice_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    invoice_receive_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    accounting_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    invoice_type_id = table.Column<int>(type: "integer", nullable: true),
                    pomaster_id = table.Column<long>(type: "bigint", nullable: true),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    is_advance_invoice = table.Column<bool>(type: "boolean", nullable: false),
                    invoice_description = table.Column<string>(type: "text", nullable: true),
                    supplier_id = table.Column<long>(type: "bigint", nullable: false),
                    project_number = table.Column<string>(type: "text", nullable: true),
                    currency_rate = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
                    base_percent = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    base_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    tax_percent = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    tax_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    total_payable_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    external_id = table.Column<string>(type: "text", nullable: true),
                    reference_no = table.Column<string>(type: "text", nullable: true),
                    advance_deduction_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    grace_period = table.Column<int>(type: "integer", nullable: true),
                    mushak_chalan_no = table.Column<string>(type: "text", nullable: true),
                    mushak_chalan_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("pk_invoice_master", x => x.invoice_master_id);
                });

            migrationBuilder.CreateTable(
                name: "invoice_payment_child",
                columns: table => new
                {
                    payment_child_id = table.Column<long>(type: "bigint", nullable: false),
                    ipayment_master_id = table.Column<long>(type: "bigint", nullable: false),
                    tvmid = table.Column<long>(type: "bigint", nullable: false),
                    pomaster_id = table.Column<long>(type: "bigint", nullable: true),
                    supplier_id = table.Column<long>(type: "bigint", nullable: true),
                    warehouse_id = table.Column<long>(type: "bigint", nullable: true),
                    invoice_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    receiving_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    posting_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    custom_deduction = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    net_payable_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
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
                    table.PrimaryKey("pk_invoice_payment_child", x => x.payment_child_id);
                });

            migrationBuilder.CreateTable(
                name: "invoice_payment_master",
                columns: table => new
                {
                    ipayment_master_id = table.Column<long>(type: "bigint", nullable: false),
                    reference_no = table.Column<string>(type: "text", nullable: false),
                    reference_keyword = table.Column<string>(type: "text", nullable: true),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
                    grand_total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    payment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    paid_by = table.Column<int>(type: "integer", nullable: true),
                    is_exception = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_invoice_payment_master", x => x.ipayment_master_id);
                });

            migrationBuilder.CreateTable(
                name: "invoice_payment_method",
                columns: table => new
                {
                    payment_method_id = table.Column<long>(type: "bigint", nullable: false),
                    ipayment_master_id = table.Column<long>(type: "bigint", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    bank_id = table.Column<int>(type: "integer", nullable: false),
                    vendor_bank_name = table.Column<string>(type: "text", nullable: true),
                    branch_name = table.Column<string>(type: "text", nullable: true),
                    account_no = table.Column<string>(type: "text", nullable: true),
                    routing_no = table.Column<string>(type: "text", nullable: true),
                    swift_code = table.Column<string>(type: "text", nullable: true),
                    cheque_book_id = table.Column<int>(type: "integer", nullable: true),
                    supplier_id = table.Column<long>(type: "bigint", nullable: true),
                    leaf_no = table.Column<int>(type: "integer", nullable: false),
                    net_payable_amount = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("pk_invoice_payment_method", x => x.payment_method_id);
                });

            migrationBuilder.CreateTable(
                name: "item",
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
                    table.PrimaryKey("pk_item", x => x.item_id);
                });

            migrationBuilder.CreateTable(
                name: "item_group",
                columns: table => new
                {
                    item_group_id = table.Column<long>(type: "bigint", nullable: false),
                    item_group_name = table.Column<string>(type: "text", nullable: false),
                    item_group_description = table.Column<string>(type: "text", nullable: true),
                    item_group_code = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_item_group", x => x.item_group_id);
                });

            migrationBuilder.CreateTable(
                name: "item_sub_group",
                columns: table => new
                {
                    item_sub_group_id = table.Column<long>(type: "bigint", nullable: false),
                    item_sub_group_name = table.Column<string>(type: "text", nullable: false),
                    item_sub_group_description = table.Column<string>(type: "text", nullable: true),
                    item_sub_group_code = table.Column<string>(type: "text", nullable: true),
                    item_group_id = table.Column<long>(type: "bigint", nullable: false),
                    glid = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("pk_item_sub_group", x => x.item_sub_group_id);
                });

            migrationBuilder.CreateTable(
                name: "material_receive",
                columns: table => new
                {
                    mrid = table.Column<long>(type: "bigint", nullable: false),
                    qcmid = table.Column<long>(type: "bigint", nullable: false),
                    reference_no = table.Column<string>(type: "text", nullable: false),
                    reference_keyword = table.Column<string>(type: "text", nullable: true),
                    mrdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    warehouse_id = table.Column<long>(type: "bigint", nullable: false),
                    supplier_id = table.Column<long>(type: "bigint", nullable: false),
                    prmaster_id = table.Column<long>(type: "bigint", nullable: false),
                    pomaster_id = table.Column<long>(type: "bigint", nullable: false),
                    total_received_qty = table.Column<decimal>(type: "numeric", nullable: false),
                    total_received_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    total_received_avg_rate = table.Column<decimal>(type: "numeric", nullable: false),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
                    budget_plan_remarks = table.Column<string>(type: "text", nullable: true),
                    chalan_no = table.Column<string>(type: "text", nullable: true),
                    chalan_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_draft = table.Column<bool>(type: "boolean", nullable: false),
                    total_vat_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    total_without_vat_amount = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("pk_material_receive", x => x.mrid);
                });

            migrationBuilder.CreateTable(
                name: "material_receive_child",
                columns: table => new
                {
                    mrcid = table.Column<long>(type: "bigint", nullable: false),
                    mrid = table.Column<long>(type: "bigint", nullable: false),
                    item_id = table.Column<long>(type: "bigint", nullable: false),
                    qccid = table.Column<long>(type: "bigint", nullable: false),
                    receive_qty = table.Column<decimal>(type: "numeric", nullable: false),
                    item_rate = table.Column<decimal>(type: "numeric", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    total_amount_including_vat = table.Column<decimal>(type: "numeric", nullable: false),
                    vat_amount = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("pk_material_receive_child", x => x.mrcid);
                });

            migrationBuilder.CreateTable(
                name: "material_requisition_child",
                columns: table => new
                {
                    mrcid = table.Column<int>(type: "integer", nullable: false),
                    item_id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    uom = table.Column<int>(type: "integer", nullable: false),
                    qty = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    price = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    mrmaster_id = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("pk_material_requisition_child", x => x.mrcid);
                });

            migrationBuilder.CreateTable(
                name: "material_requisition_master",
                columns: table => new
                {
                    mrmaster_id = table.Column<long>(type: "bigint", nullable: false),
                    mrdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reference_no = table.Column<string>(type: "text", nullable: true),
                    subject = table.Column<string>(type: "text", nullable: true),
                    preamble = table.Column<string>(type: "text", nullable: true),
                    price_and_commercial = table.Column<string>(type: "text", nullable: true),
                    solicitation = table.Column<string>(type: "text", nullable: true),
                    grand_total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    reference_keyword = table.Column<string>(type: "text", nullable: true),
                    delivery_location = table.Column<int>(type: "integer", nullable: true),
                    scmremarks = table.Column<string>(type: "text", nullable: true),
                    required_by_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("pk_material_requisition_master", x => x.mrmaster_id);
                });

            migrationBuilder.CreateTable(
                name: "prnfamap",
                columns: table => new
                {
                    prnfamap_id = table.Column<long>(type: "bigint", nullable: false),
                    prmid = table.Column<long>(type: "bigint", nullable: false),
                    nfaid = table.Column<int>(type: "integer", nullable: true),
                    nfareference_no = table.Column<string>(type: "text", nullable: true),
                    nfaamount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    is_from_system = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_prnfamap", x => x.prnfamap_id);
                });

            migrationBuilder.CreateTable(
                name: "purchase_order_child",
                columns: table => new
                {
                    pocid = table.Column<int>(type: "integer", nullable: false),
                    item_id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    uom = table.Column<int>(type: "integer", nullable: false),
                    qty = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    rate = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    vat_info_id = table.Column<long>(type: "bigint", nullable: false),
                    vat_percent = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    is_rebateable = table.Column<bool>(type: "boolean", nullable: false),
                    rebate_percentage = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    total_amount_including_vat = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    pomaster_id = table.Column<long>(type: "bigint", nullable: false),
                    prcid = table.Column<int>(type: "integer", nullable: false),
                    prqid = table.Column<long>(type: "bigint", nullable: false),
                    vat_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
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
                    table.PrimaryKey("pk_purchase_order_child", x => x.pocid);
                });

            migrationBuilder.CreateTable(
                name: "purchase_order_master",
                columns: table => new
                {
                    pomaster_id = table.Column<long>(type: "bigint", nullable: false),
                    reference_no = table.Column<string>(type: "text", nullable: false),
                    reference_keyword = table.Column<string>(type: "text", nullable: true),
                    podate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    delivery_location = table.Column<long>(type: "bigint", nullable: true),
                    supplier_id = table.Column<long>(type: "bigint", nullable: false),
                    prmaster_id = table.Column<long>(type: "bigint", nullable: false),
                    grand_total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
                    delivery_within_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    contact_person = table.Column<string>(type: "text", nullable: true),
                    contact_number = table.Column<string>(type: "text", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    quotation_no = table.Column<string>(type: "text", nullable: true),
                    quotation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    payment_terms_id = table.Column<int>(type: "integer", nullable: true),
                    inventory_type_id = table.Column<int>(type: "integer", nullable: true),
                    is_draft = table.Column<bool>(type: "boolean", nullable: false),
                    budget_plan_remarks = table.Column<string>(type: "text", nullable: true),
                    scmremarks = table.Column<string>(type: "text", nullable: true),
                    is_closed = table.Column<bool>(type: "boolean", nullable: false),
                    close_remarks = table.Column<string>(type: "text", nullable: true),
                    total_vat_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    total_without_vat_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    credit_day = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("pk_purchase_order_master", x => x.pomaster_id);
                });

            migrationBuilder.CreateTable(
                name: "purchase_requisition_child",
                columns: table => new
                {
                    prcid = table.Column<int>(type: "integer", nullable: false),
                    item_id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    for_id = table.Column<int>(type: "integer", nullable: true),
                    uom = table.Column<int>(type: "integer", nullable: false),
                    qty = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    price = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    prmaster_id = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("pk_purchase_requisition_child", x => x.prcid);
                });

            migrationBuilder.CreateTable(
                name: "purchase_requisition_child_cost_center_budget",
                columns: table => new
                {
                    prcccbid = table.Column<int>(type: "integer", nullable: false),
                    prmaster_id = table.Column<long>(type: "bigint", nullable: false),
                    for_id = table.Column<int>(type: "integer", nullable: false),
                    from_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    to_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    allocated_budget_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    remaining_budget_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_purchase_requisition_child_cost_center_budget", x => x.prcccbid);
                });

            migrationBuilder.CreateTable(
                name: "purchase_requisition_master",
                columns: table => new
                {
                    prmaster_id = table.Column<long>(type: "bigint", nullable: false),
                    prdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reference_no = table.Column<string>(type: "text", nullable: true),
                    subject = table.Column<string>(type: "text", nullable: true),
                    preamble = table.Column<string>(type: "text", nullable: true),
                    price_and_commercial = table.Column<string>(type: "text", nullable: true),
                    solicitation = table.Column<string>(type: "text", nullable: true),
                    budget_plan_remarks = table.Column<string>(type: "text", nullable: true),
                    grand_total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    reference_keyword = table.Column<string>(type: "text", nullable: true),
                    delivery_location = table.Column<int>(type: "integer", nullable: true),
                    scmremarks = table.Column<string>(type: "text", nullable: true),
                    is_single_quotation = table.Column<bool>(type: "boolean", nullable: false),
                    required_by_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_draft = table.Column<bool>(type: "boolean", nullable: false),
                    mrmaster_id = table.Column<long>(type: "bigint", nullable: false),
                    is_archive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_purchase_requisition_master", x => x.prmaster_id);
                });

            migrationBuilder.CreateTable(
                name: "purchase_requisition_quotation",
                columns: table => new
                {
                    prqid = table.Column<long>(type: "bigint", nullable: false),
                    prmaster_id = table.Column<long>(type: "bigint", nullable: false),
                    supplier_id = table.Column<long>(type: "bigint", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    tax_type_id = table.Column<int>(type: "integer", nullable: true),
                    quoted_qty = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    quoted_unit_price = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    item_id = table.Column<long>(type: "bigint", nullable: true),
                    prcid = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_purchase_requisition_quotation", x => x.prqid);
                });

            migrationBuilder.CreateTable(
                name: "purchase_requisition_quotation_item_map",
                columns: table => new
                {
                    prqitem_map_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    prmaster_id = table.Column<long>(type: "bigint", nullable: false),
                    prqid = table.Column<long>(type: "bigint", nullable: false),
                    item_id = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("pk_purchase_requisition_quotation_item_map", x => x.prqitem_map_id);
                });

            migrationBuilder.CreateTable(
                name: "qcchild",
                columns: table => new
                {
                    qccid = table.Column<long>(type: "bigint", nullable: false),
                    qcmid = table.Column<long>(type: "bigint", nullable: false),
                    item_id = table.Column<long>(type: "bigint", nullable: false),
                    pocid = table.Column<int>(type: "integer", nullable: false),
                    supplied_qty = table.Column<decimal>(type: "numeric", nullable: false),
                    accepted_qty = table.Column<decimal>(type: "numeric", nullable: false),
                    rejected_qty = table.Column<decimal>(type: "numeric", nullable: false),
                    qccnote = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_qcchild", x => x.qccid);
                });

            migrationBuilder.CreateTable(
                name: "qcmaster",
                columns: table => new
                {
                    qcmid = table.Column<long>(type: "bigint", nullable: false),
                    reference_no = table.Column<string>(type: "text", nullable: false),
                    reference_keyword = table.Column<string>(type: "text", nullable: true),
                    receipt_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    warehouse_id = table.Column<long>(type: "bigint", nullable: false),
                    supplier_id = table.Column<long>(type: "bigint", nullable: false),
                    prmaster_id = table.Column<long>(type: "bigint", nullable: false),
                    pomaster_id = table.Column<long>(type: "bigint", nullable: false),
                    total_supplied_qty = table.Column<decimal>(type: "numeric", nullable: false),
                    total_accepted_qty = table.Column<decimal>(type: "numeric", nullable: false),
                    total_rejected_qty = table.Column<decimal>(type: "numeric", nullable: false),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
                    budget_plan_remarks = table.Column<string>(type: "text", nullable: true),
                    chalan_no = table.Column<string>(type: "text", nullable: true),
                    chalan_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("pk_qcmaster", x => x.qcmid);
                });

            migrationBuilder.CreateTable(
                name: "rtvchild",
                columns: table => new
                {
                    rtvcid = table.Column<long>(type: "bigint", nullable: false),
                    rtvmid = table.Column<long>(type: "bigint", nullable: false),
                    item_id = table.Column<long>(type: "bigint", nullable: false),
                    pocid = table.Column<int>(type: "integer", nullable: false),
                    supplied_qty = table.Column<decimal>(type: "numeric", nullable: false),
                    return_qty = table.Column<decimal>(type: "numeric", nullable: false),
                    rtvcnote = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_rtvchild", x => x.rtvcid);
                });

            migrationBuilder.CreateTable(
                name: "rtvmaster",
                columns: table => new
                {
                    rtvmid = table.Column<long>(type: "bigint", nullable: false),
                    reference_no = table.Column<string>(type: "text", nullable: false),
                    reference_keyword = table.Column<string>(type: "text", nullable: true),
                    supply_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    return_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    warehouse_id = table.Column<long>(type: "bigint", nullable: false),
                    supplier_id = table.Column<long>(type: "bigint", nullable: false),
                    prmaster_id = table.Column<long>(type: "bigint", nullable: false),
                    pomaster_id = table.Column<long>(type: "bigint", nullable: false),
                    total_supplied_qty = table.Column<decimal>(type: "numeric", nullable: false),
                    total_return_qty = table.Column<decimal>(type: "numeric", nullable: false),
                    approval_status_id = table.Column<int>(type: "integer", nullable: false),
                    budget_plan_remarks = table.Column<string>(type: "text", nullable: true),
                    supplier_dcno = table.Column<string>(type: "text", nullable: true),
                    supplier_dcdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_draft = table.Column<bool>(type: "boolean", nullable: false),
                    qcmid = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("pk_rtvmaster", x => x.rtvmid);
                });

            migrationBuilder.CreateTable(
                name: "sccchild",
                columns: table => new
                {
                    scccid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sccmid = table.Column<long>(type: "bigint", nullable: false),
                    item_id = table.Column<long>(type: "bigint", nullable: false),
                    pocid = table.Column<int>(type: "integer", nullable: false),
                    received_qty = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    delivery_or_job_completion_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    invoice_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    scccnote = table.Column<string>(type: "text", nullable: true),
                    rate = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    total_amount_including_vat = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    vat_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
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
                    table.PrimaryKey("pk_sccchild", x => x.scccid);
                });

            migrationBuilder.CreateTable(
                name: "sccmaster",
                columns: table => new
                {
                    sccmid = table.Column<long>(type: "bigint", nullable: false),
                    reference_no = table.Column<string>(type: "text", nullable: false),
                    reference_keyword = table.Column<string>(type: "text", nullable: true),
                    supplier_id = table.Column<long>(type: "bigint", nullable: false),
                    prmaster_id = table.Column<long>(type: "bigint", nullable: false),
                    pomaster_id = table.Column<long>(type: "bigint", nullable: false),
                    invoice_no_from_vendor = table.Column<string>(type: "text", nullable: true),
                    invoice_date_from_vendor = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    invoice_amount_from_vendor = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    service_period_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    service_period_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    payment_type = table.Column<int>(type: "integer", nullable: false),
                    payment_fixed_or_percent = table.Column<string>(type: "text", nullable: true),
                    payment_fixed_or_percent_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    payment_fixed_or_percent_total_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    total_received_qty = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    sccamount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    is_draft = table.Column<bool>(type: "boolean", nullable: false),
                    performance_assessment1 = table.Column<bool>(type: "boolean", nullable: false),
                    performance_assessment2 = table.Column<bool>(type: "boolean", nullable: false),
                    performance_assessment3 = table.Column<bool>(type: "boolean", nullable: false),
                    performance_assessment4 = table.Column<bool>(type: "boolean", nullable: false),
                    performance_assessment5 = table.Column<bool>(type: "boolean", nullable: false),
                    performance_assessment6 = table.Column<bool>(type: "boolean", nullable: false),
                    performance_assessment_comment = table.Column<string>(type: "text", nullable: true),
                    lifecycle = table.Column<int>(type: "integer", nullable: false),
                    lifecycle_comment = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_sccmaster", x => x.sccmid);
                });

            migrationBuilder.CreateTable(
                name: "supplier",
                columns: table => new
                {
                    supplier_id = table.Column<long>(type: "bigint", nullable: false),
                    supplier_name = table.Column<string>(type: "text", nullable: false),
                    supplier_type_id = table.Column<int>(type: "integer", nullable: false),
                    supplier_category_id = table.Column<int>(type: "integer", nullable: false),
                    registered_address = table.Column<string>(type: "text", nullable: true),
                    corresponding_address = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    postal_code = table.Column<string>(type: "text", nullable: true),
                    email_address = table.Column<string>(type: "text", nullable: true),
                    tinnumber = table.Column<string>(type: "text", nullable: true),
                    vatregistration_number = table.Column<string>(type: "text", nullable: true),
                    bank_name = table.Column<string>(type: "text", nullable: true),
                    bank_branch = table.Column<string>(type: "text", nullable: true),
                    bank_account_name = table.Column<string>(type: "text", nullable: true),
                    bank_account_number = table.Column<string>(type: "text", nullable: true),
                    glid = table.Column<long>(type: "bigint", nullable: true),
                    routing_number = table.Column<string>(type: "text", nullable: true),
                    swift_code = table.Column<string>(type: "text", nullable: true),
                    contact_name1 = table.Column<string>(type: "text", nullable: true),
                    contact_email1 = table.Column<string>(type: "text", nullable: true),
                    phone_number1 = table.Column<string>(type: "text", nullable: true),
                    contact_name2 = table.Column<string>(type: "text", nullable: true),
                    contact_email2 = table.Column<string>(type: "text", nullable: true),
                    phone_number2 = table.Column<string>(type: "text", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    external_id = table.Column<string>(type: "text", nullable: true),
                    merchant_wallet_no = table.Column<string>(type: "text", nullable: true),
                    supplier_code = table.Column<string>(type: "text", nullable: true),
                    binnumber = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_supplier", x => x.supplier_id);
                });

            migrationBuilder.CreateTable(
                name: "supplier_type",
                columns: table => new
                {
                    stid = table.Column<long>(type: "bigint", nullable: false),
                    type_name = table.Column<string>(type: "text", nullable: false),
                    glid = table.Column<long>(type: "bigint", nullable: true),
                    manual_glid = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("pk_supplier_type", x => x.stid);
                });

            migrationBuilder.CreateTable(
                name: "vat_info",
                columns: table => new
                {
                    vat_info_id = table.Column<long>(type: "bigint", nullable: false),
                    vat_percent = table.Column<decimal>(type: "numeric", nullable: false),
                    vat_policies = table.Column<string>(type: "text", nullable: false),
                    is_rebateable = table.Column<bool>(type: "boolean", nullable: false),
                    rebate_percentage = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("pk_vat_info", x => x.vat_info_id);
                });

            migrationBuilder.CreateTable(
                name: "vendor_assessment_members",
                columns: table => new
                {
                    primary_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    employee_id = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("pk_vendor_assessment_members", x => x.primary_id);
                });

            migrationBuilder.CreateTable(
                name: "warehouse",
                columns: table => new
                {
                    warehouse_id = table.Column<int>(type: "integer", nullable: false),
                    warehouse_name = table.Column<string>(type: "text", nullable: false),
                    contact_person = table.Column<string>(type: "text", nullable: true),
                    warehouse_address = table.Column<string>(type: "text", nullable: true),
                    contact_no = table.Column<string>(type: "text", nullable: true),
                    authorise_person_name = table.Column<string>(type: "text", nullable: true),
                    authorise_person_designation = table.Column<string>(type: "text", nullable: true),
                    glid = table.Column<long>(type: "bigint", nullable: true),
                    sales_return_glid = table.Column<long>(type: "bigint", nullable: true),
                    able_to_id = table.Column<int>(type: "integer", nullable: false),
                    warehouse_type_id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_warehouse", x => x.warehouse_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "inventory_current_stock");

            migrationBuilder.DropTable(
                name: "inventory_transaction");

            migrationBuilder.DropTable(
                name: "inventory_warehouse_current_stock");

            migrationBuilder.DropTable(
                name: "invoice_child");

            migrationBuilder.DropTable(
                name: "invoice_child_scc");

            migrationBuilder.DropTable(
                name: "invoice_master");

            migrationBuilder.DropTable(
                name: "invoice_payment_child");

            migrationBuilder.DropTable(
                name: "invoice_payment_master");

            migrationBuilder.DropTable(
                name: "invoice_payment_method");

            migrationBuilder.DropTable(
                name: "item");

            migrationBuilder.DropTable(
                name: "item_group");

            migrationBuilder.DropTable(
                name: "item_sub_group");

            migrationBuilder.DropTable(
                name: "material_receive");

            migrationBuilder.DropTable(
                name: "material_receive_child");

            migrationBuilder.DropTable(
                name: "material_requisition_child");

            migrationBuilder.DropTable(
                name: "material_requisition_master");

            migrationBuilder.DropTable(
                name: "prnfamap");

            migrationBuilder.DropTable(
                name: "purchase_order_child");

            migrationBuilder.DropTable(
                name: "purchase_order_master");

            migrationBuilder.DropTable(
                name: "purchase_requisition_child");

            migrationBuilder.DropTable(
                name: "purchase_requisition_child_cost_center_budget");

            migrationBuilder.DropTable(
                name: "purchase_requisition_master");

            migrationBuilder.DropTable(
                name: "purchase_requisition_quotation");

            migrationBuilder.DropTable(
                name: "purchase_requisition_quotation_item_map");

            migrationBuilder.DropTable(
                name: "qcchild");

            migrationBuilder.DropTable(
                name: "qcmaster");

            migrationBuilder.DropTable(
                name: "rtvchild");

            migrationBuilder.DropTable(
                name: "rtvmaster");

            migrationBuilder.DropTable(
                name: "sccchild");

            migrationBuilder.DropTable(
                name: "sccmaster");

            migrationBuilder.DropTable(
                name: "supplier");

            migrationBuilder.DropTable(
                name: "supplier_type");

            migrationBuilder.DropTable(
                name: "vat_info");

            migrationBuilder.DropTable(
                name: "vendor_assessment_members");

            migrationBuilder.DropTable(
                name: "warehouse");
        }
    }
}
