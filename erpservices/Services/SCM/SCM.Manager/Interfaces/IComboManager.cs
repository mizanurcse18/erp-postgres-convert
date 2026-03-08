using Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SCM.Manager.Interfaces
{
    public interface IComboManager
    {

        Task<List<ComboModel>> GetItemGroupCombo();
        Task<List<ComboModel>> GetSupplierTypes();
        Task<IEnumerable<Dictionary<string, object>>> GetItems(string param);
        Task<IEnumerable<Dictionary<string, object>>> GetCurrency();
        Task<List<ComboModel>> GetItemSubGroupCombo();
        Task<List<ComboModel>> GetVatinfoCombo();
        Task<List<ComboModel>> GetUnits();
        Task<List<ComboModel>> GetAssessmentMembersCombo(); 
         Task<List<ComboModel>> GetCostCenters();
        Task<List<ComboModel>> GetDeliveryLocations();
        Task<List<ComboModel>> GetSuppliers();
        Task<List<ComboModel>> GetSuppliersForInvoicePayment();


        Task<List<ComboModel>> GetPreparedByCombo();
        Task<List<ComboModel>> GetItemsCombo();
        Task<List<ComboModel>> GetItemSubCategoryCombo();
        Task<List<ComboModel>> GetSupplierCombo();
        Task<List<ComboModel>> GetAllInvoiceDocumentCategory();
    }
}
