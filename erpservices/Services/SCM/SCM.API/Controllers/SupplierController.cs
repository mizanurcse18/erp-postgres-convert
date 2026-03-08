using API.Core;
using Core.AppContexts;
using Core.Extensions;
using Manager.Core.CommonDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCM.Manager.Dto;
using SCM.Manager.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SCM.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class SupplierController : BaseController
    {
        private readonly ISupplierManager Manager;

        public SupplierController(ISupplierManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetSuppliers")]
        public async Task<IActionResult> GetSuppliers()
        {
            var suppliers = await Manager.GetSupplierListDic();
            return OkResult(suppliers);
        }

        [HttpGet("GetSupplier/{SupplierID:int}")]
        public async Task<IActionResult> GetSupplier(int SupplierID)
        {
            var supplier = await Manager.GetSupplier(SupplierID);
            var attachments = Manager.GetAttachments(SupplierID);
            return OkResult(new { Supplier = supplier, Attachments= attachments });
        }

        [HttpPost("CreateSupplier")]
        public async Task<IActionResult> CreateSupplier([FromBody] SupplierDto supplier)
        {
            var newData = await Manager.SaveChanges(supplier);
            if (!string.IsNullOrEmpty(newData.SupplierNameError)) return OkResult(newData);
            return OkResult(supplier);
        }
        // POST: /User/Delete

        [HttpGet("Delete/{SupplierID:int}")]
        public async Task<IActionResult> Delete(int SupplierID)
        {
            await Manager.Delete(SupplierID);
            return OkResult(new { success = true });

        }

    }
}