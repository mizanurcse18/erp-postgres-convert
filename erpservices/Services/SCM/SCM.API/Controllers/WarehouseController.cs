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
    public class WarehouseController : BaseController
    {
        private readonly IWarehouseManager Manager;

        public WarehouseController(IWarehouseManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetWarehouses")]
        public async Task<IActionResult> GetWarehouses()
        {
            var warehouses = await Manager.GetWarehouseListDic();
            return OkResult(warehouses);
        }

        [HttpGet("GetWarehouse/{WarehouseID:int}")]
        public async Task<IActionResult> GetWarehouse(int WarehouseID)
        {
            var warehouse = await Manager.GetWarehouse(WarehouseID);
            var attachments = Manager.GetAttachments(WarehouseID);
            return OkResult(new { Warehouse = warehouse, Attachments = attachments });
        }

        [HttpPost("CreateWarehouse")]
        public async Task<IActionResult> CreateWarehouse([FromBody] WarehouseDto warehouse)
        {
            var newData = await Manager.SaveChanges(warehouse);
            if (!string.IsNullOrEmpty(newData.WarehouseNameError)) return OkResult(newData);
            return OkResult(warehouse);
        }
        // POST: /User/Delete

        [HttpGet("Delete/{WarehouseID:int}")]
        public async Task<IActionResult> Delete(int WarehouseID)
        {
            await Manager.Delete(WarehouseID);
            return OkResult(new { success = true });

        }

    }
}