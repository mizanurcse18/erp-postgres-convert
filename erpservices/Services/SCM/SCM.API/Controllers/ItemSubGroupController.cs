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
    public class ItemSubGroupController : BaseController
    {
        private readonly IItemSubGroupManager Manager;

        public ItemSubGroupController(IItemSubGroupManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetItemSubGroups")]
        public async Task<IActionResult> GetItemSubGroups()
        {
            var itemSubGroups = await Manager.GetItemSubGroupListDic();
            return OkResult(itemSubGroups);
        }

        [HttpGet("GetItemSubGroup/{ItemSubGroupID:int}")]
        public async Task<IActionResult> GetItemSubGroup(int ItemSubGroupID)
        {
            var itemSubGroup = await Manager.GetItemSubGroup(ItemSubGroupID);
            return OkResult(itemSubGroup);
        }

        [HttpPost("CreateItemSubGroup")]
        public async Task<IActionResult> CreateItemSubGroup([FromBody] ItemSubGroupDto itemSubGroup)
        {
            var newData = await Manager.SaveChanges(itemSubGroup);
            if (!string.IsNullOrEmpty(newData.ItemSubGroupNameError)) return OkResult(newData);
            return OkResult(itemSubGroup);
        }
        // POST: /User/Delete

        [HttpGet("Delete/{ItemSubGroupID:int}")]
        public async Task<IActionResult> Delete(int ItemSubGroupID)
        {
            await Manager.Delete(ItemSubGroupID);
            return OkResult(new { success = true });

        }

    }
}