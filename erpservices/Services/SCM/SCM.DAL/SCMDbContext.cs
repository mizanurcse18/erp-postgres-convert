using DAL.Core;
using DAL.Core.Attribute;
using DAL.Core.Repository;
using Microsoft.EntityFrameworkCore;
using SCM.DAL.Entities;

namespace SCM.DAL
{
    [RepositoryTypes(typeof(IRepository<>), typeof(Repository<,>))]
    class SCMDbContext : BaseDbContext
    {
        public SCMDbContext(DbContextOptions<SCMDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        public virtual DbSet<Supplier> Suppliers { get; set; }
        public virtual DbSet<PurchaseRequisitionMaster> PurchaseRequisitionMasters { get; set; }
        public virtual DbSet<PurchaseRequisitionChild> PurchaseRequisitionChilds { get; set; }
        public virtual DbSet<PurchaseRequisitionQuotation> PurchaseRequisitionQuotations { get; set; }
        public virtual DbSet<SupplierType> SupplierTypes { get; set; }
        public virtual DbSet<Warehouse> Warehouses { get; set; }
        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<ItemGroup> ItemGroups { get; set; }
        public virtual DbSet<ItemSubGroup> ItemSubGroups { get; set; }
        public virtual DbSet<VatInfo> VatInfos { get; set; }
        public virtual DbSet<PurchaseOrderMaster> PurchaseOrderMasters { get; set; }
        public virtual DbSet<PurchaseOrderChild> PurchaseOrderChilds { get; set; }
        public virtual DbSet<PurchaseRequisitionChildCostCenterBudget> PurchaseRequisitionChildCostCenterBudgets { get; set; }
        public virtual DbSet<PurchaseRequisitionQuotationItemMap> PurchaseRequisitionQuotationItemMaps { get; set; }
        public virtual DbSet<MaterialReceive> MaterialReceives { get; set; }
        public virtual DbSet<MaterialReceiveChild> MaterialReceiveChilds { get; set; }
        public virtual DbSet<InventoryCurrentStock> InventoryCurrentStocks { get; set; }
        public virtual DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public virtual DbSet<InventoryWarehouseCurrentStock> InventoryWarehouseCurrentStocks { get; set; }
        public virtual DbSet<InvoiceMaster> InvoiceMasters { get; set; }
        public virtual DbSet<InvoiceChild> InvoiceChilds { get; set; }
        public virtual DbSet<InvoicePaymentMaster> InvoicePaymentMasters { get; set; }
        public virtual DbSet<InvoicePaymentChild> InvoicePaymentChilds { get; set; }
        public virtual DbSet<QCMaster> QCMasters { get; set; }
        public virtual DbSet<QCChild> QCChilds { get; set; }
        public virtual DbSet<RTVMaster> RTVMasters { get; set; }
        public virtual DbSet<RTVChild> RTVChilds { get; set; }
        public virtual DbSet<VendorAssessmentMembers> VendorAssessmentMembers { get; set; }
        public virtual DbSet<MaterialRequisitionMaster> MaterialRequisitionMasters { get; set; }
        public virtual DbSet<MaterialRequisitionChild> MaterialRequisitionChilds { get; set; }
        public virtual DbSet<SCCMaster> SCCMaster { get; set; }
        public virtual DbSet<SCCChild> SCCChildList { get; set; }
        public virtual DbSet<InvoicePaymentMethod> InvoicePaymentMethods { get; set; }
        public virtual DbSet<InvoiceChildSCC> InvoiceChildSCCList { get; set; }
        public virtual DbSet<PRNFAMap> PRNFAMapList { get; set; }

    }
}
