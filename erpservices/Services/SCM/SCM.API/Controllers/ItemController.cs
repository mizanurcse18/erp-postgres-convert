using API.Core;
using Core;
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
    public class ItemController : BaseController
    {
        private readonly IItemManager Manager;

        public ItemController(IItemManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetItems")]
        public async Task<IActionResult> GetItems()
        {
            var items = await Manager.GetItemListDic();
            return OkResult(items);
        }

        [HttpPost("GetListForGrid")]
        public IActionResult GetListForGrid([FromBody] GridParameter parameters)
        {
            var model = Manager.GetListForGrid(parameters);
            model.IsSubmittedFromPopup = parameters.IsSubmittedFromPopup;
            return OkResult(new { parentDataSource = model });
        }

        [HttpGet("GetItem/{ItemID:int}")]
        public async Task<IActionResult> GetItem(int ItemID)
        {
            var item = await Manager.GetItem(ItemID);
            return OkResult(item);
        }

        [HttpPost("CreateItem")]
        public async Task<IActionResult> CreateItem([FromBody] ItemDto item)
        {
            var newData = await Manager.SaveChanges(item);
            if (!string.IsNullOrEmpty(newData.ItemNameError)) return OkResult(newData);
            return OkResult(item);
        }
        // POST: /User/Delete

        [HttpGet("Delete/{ItemID:int}")]
        public async Task<IActionResult> Delete(int ItemID)
        {
            await Manager.Delete(ItemID);
            return OkResult(new { success = true });

        }

    }
}