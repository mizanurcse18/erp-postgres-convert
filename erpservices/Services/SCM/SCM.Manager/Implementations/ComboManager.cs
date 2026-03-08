using Core;
using Core.AppContexts;
using DAL.Core.Repository;
using SCM.DAL.Entities;
using SCM.Manager.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SCM.Manager.Implementations
{
    public class ComboManager : IComboManager
    {

        private readonly IRepository<SupplierType> SupTypeRepo;
        private readonly IRepository<Item> ItemRepo;
        private readonly IRepository<ItemGroup> ItemGroupRepo;
        private readonly IRepository<ItemSubGroup> ItemSubGroupRepo;
        private readonly IRepository<Warehouse> WarehouseRepo;
        private readonly IRepository<Supplier> SupplierRepo;
        private readonly IRepository<VatInfo> VatInfoRepo;
        private readonly IRepository<PurchaseOrderMaster> SCMRepo;
        public ComboManager(IRepository<SupplierType> supTypeRepo, IRepository<Item> itemRepo, IRepository<ItemGroup> itemGroupRepo, IRepository<Warehouse> warehouseRepo
            , IRepository<ItemSubGroup> itemSubGroupRepo, IRepository<Supplier> supplierRep, IRepository<VatInfo> vatinfoRepo, IRepository<PurchaseOrderMaster> scmRepo)

        {
            SupTypeRepo = supTypeRepo;
            ItemRepo = itemRepo;
            ItemGroupRepo = itemGroupRepo;
            ItemSubGroupRepo = itemSubGroupRepo;
            WarehouseRepo = warehouseRepo;
            SupplierRepo = supplierRep;
            VatInfoRepo = vatinfoRepo;
            SCMRepo = scmRepo;
        }

        public async Task<List<ComboModel>> GetSupplierTypes()
        {
            var supTypeList = await SupTypeRepo.GetAllListAsync();
            return supTypeList.Select(x => new ComboModel { value = (int)x.STID, label = x.TypeName }).ToList();
        }

        public async Task<List<ComboModel>> GetItemSubGroupCombo()
        {
            var subgrpList = await ItemSubGroupRepo.GetAllListAsync();
            return subgrpList.Select(x => new ComboModel { value = (int)x.ItemSubGroupID, label = x.ItemSubGroupName }).ToList();
        }
        public async Task<List<ComboModel>> GetItemGroupCombo()
        {
            var supTypeList = await ItemGroupRepo.GetAllListAsync();
            return supTypeList.Select(x => new ComboModel { value = (int)x.ItemGroupID, label = x.ItemGroupName }).ToList();
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetItems(string param)
        {
            string sql = @$"SELECT * FROM Item WHERE (ItemName + ItemCode LIKE '%{param}%') ORDER BY ItemName";
            var listDict = ItemRepo.GetDataDictCollection(sql);
            return await Task.FromResult(listDict);
        }
        public async Task<List<ComboModel>> GetUnits()
        {
            string sql = @$"SELECT UnitID value,UnitCode label FROM Security..Unit";
            return ItemRepo.GetDataModelCollection<ComboModel>(sql);
        }
        public async Task<List<ComboModel>> GetAssessmentMembersCombo()
        {
            string sql = @$"select VSM.EmployeeID value, VA.FullName label
                            from VendorAssessmentMembers VSM
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.EmployeeID = VSM.EmployeeID";
            return ItemRepo.GetDataModelCollection<ComboModel>(sql);
        }

        public async Task<List<ComboModel>> GetVatinfoCombo()
        {
            string sql = @$"SELECT 
	                        VatInfoID AS value,
	                        'Vat Percent: '+VatPolicies+' , Rebateable: '+ CASE WHEN IsRebateable = 0 THEN 'No' ELSE 'YES' END +
	                        ', Rebateable: '+Cast(RebatePercentage as nvarchar(100)) AS label
                        FROM VatInfo";
            return VatInfoRepo.GetDataModelCollection<ComboModel>(sql);
        }

        public async Task<List<ComboModel>> GetCostCenters()
        {
            string sql = @$"SELECT CostCenterID value,CostCenterName label FROM Accounts..CostCenter";
            return ItemRepo.GetDataModelCollection<ComboModel>(sql);
        }
        public async Task<List<ComboModel>> GetDeliveryLocations()
        {
            var list = await WarehouseRepo.GetAllListAsync();
            return list.Select(x => new ComboModel { value = (int)x.WarehouseID, label = x.WarehouseName }).ToList();
        }

        public async Task<List<ComboModel>> GetSuppliers()
        {
            var list = await SupplierRepo.GetAllListAsync();
            return list.Select(x => new ComboModel { value = (int)x.SupplierID, label = x.SupplierName }).ToList();
        }

        public async Task<List<ComboModel>> GetSuppliersForInvoicePayment()
        {
            string sql = @$"SELECT 
	                            VGI.SupplierID value,
	                            SupplierName label
                            FROM SCM..viewGetPendingInvoiceForPayment VGI";
            return ItemRepo.GetDataModelCollection<ComboModel>(sql);
        }



        public async Task<List<ComboModel>> GetPreparedByCombo()
        {
            string sql = @$"SELECT PO.CreatedBy value, E.EmployeeCode + '-' + E.FullName label
                        FROM PurchaseOrderMaster PO
                        LEFT JOIN Security..Users U ON PO.CreatedBy = U.UserID
                        LEFT JOIN HRMS..ViewALLEmployee E ON U.PersonID = E.PersonID";
            return ItemRepo.GetDataModelCollection<ComboModel>(sql);
        }


        public async Task<List<ComboModel>> GetItemsCombo()
        {
            string sql = @$"select ItemID value, ItemName label from Item";
            return ItemRepo.GetDataModelCollection<ComboModel>(sql);
        }


        public async Task<List<ComboModel>> GetItemSubCategoryCombo()
        {
            string sql = @$"select ItemSubGroupID value, ItemSubGroupName label from ItemSubGroup";
            return ItemRepo.GetDataModelCollection<ComboModel>(sql);
        }


        public async Task<List<ComboModel>> GetSupplierCombo()
        {
            string sql = @$"select SupplierID value, SupplierName label from Supplier";
            return ItemRepo.GetDataModelCollection<ComboModel>(sql);
        }
        public async Task<List<ComboModel>> GetAllInvoiceDocumentCategory()
        {
            string sql = @$"select SystemVariableID value,SystemVariableCode label from Security..SystemVariable where EntityTypeName='InvoiceFileUploadCategory'";
            return ItemRepo.GetDataModelCollection<ComboModel>(sql);
        }
        public async Task<IEnumerable<Dictionary<string, object>>> GetCurrency()
        {
            string sql = @$"select * from Security..Currency";
            var listDict = ItemRepo.GetDataDictCollection(sql);
            return await Task.FromResult(listDict);
        }
    }
}
