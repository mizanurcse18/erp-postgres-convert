using API.Core;
using SCM.Manager.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SCM.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class ComboController : BaseController
    {
        private readonly IComboManager Manager;

        public ComboController(IComboManager manager)
        {
            Manager = manager;
        }


        [HttpGet("GetSupplierTypes")]
        public async Task<ActionResult> GetSupplierTypes()
        {
            var list = await Manager.GetSupplierTypes();
            return OkResult(list);
        }
        [HttpGet("GetItemGroupCombo")]
        public async Task<ActionResult> GetItemGroupCombo()
        {
            var list = await Manager.GetItemGroupCombo();
            return OkResult(list);
        }
        [HttpGet("GetItemSubGroupCombo")]
        public async Task<ActionResult> GetItemSubGroupCombo()
        {
            var list = await Manager.GetItemSubGroupCombo();
            return OkResult(list);
        }
        [HttpGet("GetVatinfoCombo")]
        public async Task<ActionResult> GetVatinfoCombo()
        {
            var list = await Manager.GetVatinfoCombo();
            return OkResult(list);
        }



        [HttpGet("GetItem/{param}")]
        public async Task<ActionResult> GetItem(string param)
        {
            var list = await Manager.GetItems(param);
            return OkResult(list);
        }
        [HttpGet("GetUnits")]
        public async Task<ActionResult> GetUnits()
        {
            var list = await Manager.GetUnits();
            return OkResult(list);
        }
        [HttpGet("GetAssessmentMembersCombo")]
        public async Task<ActionResult> GetAssessmentMembersCombo()
        {
            var list = await Manager.GetAssessmentMembersCombo();
            return OkResult(list);
        }
        
        [HttpGet("GetCostCenters")]
        public async Task<ActionResult> GetCostCenters()
        {
            var list = await Manager.GetCostCenters();
            return OkResult(list);
        }

        [HttpGet("GetDeliveryLocations")]
        public async Task<ActionResult> GetDeliveryLocations()
        {
            var list = await Manager.GetDeliveryLocations();
            return OkResult(list);
        }
        [HttpGet("GetSuppliers")]
        public async Task<ActionResult> GetSuppliers()
        {
            var list = await Manager.GetSuppliers();
            return OkResult(list);
        }
        [HttpGet("GetSuppliersForInvoicePayment")]
        public async Task<ActionResult> GetSuppliersForInvoicePayment()
        {
            var list = await Manager.GetSuppliersForInvoicePayment();
            return OkResult(list);
        }

        [HttpGet("GetCurrency")]
        public async Task<ActionResult> GetCurrency()
        {
            var list = await Manager.GetCurrency();
            return OkResult(list);
        }

        [HttpGet("GetPreparedByCombo")]
        public async Task<ActionResult> GetPreparedByCombo()
        {
            var list = await Manager.GetPreparedByCombo();
            return OkResult(list);
        }
        [HttpGet("GetItemsCombo")]
        public async Task<ActionResult> GetItemsCombo()
        {
            var list = await Manager.GetItemsCombo();
            return OkResult(list);
        }
        [HttpGet("GetItemSubCategoryCombo")]
        public async Task<ActionResult> GetItemSubCategoryCombo()
        {
            var list = await Manager.GetItemSubCategoryCombo();
            return OkResult(list);
        }
        [HttpGet("GetSupplierCombo")]
        public async Task<ActionResult> GetSupplierCombo()
        {
            var list = await Manager.GetSupplierCombo();
            return OkResult(list);
        }

        [HttpGet("GetAllInvoiceDocumentCategory")]
        public async Task<ActionResult> GetAllInvoiceDocumentCategory()
        {
            var list = await Manager.GetAllInvoiceDocumentCategory();
            return OkResult(list);
        }
    }
}