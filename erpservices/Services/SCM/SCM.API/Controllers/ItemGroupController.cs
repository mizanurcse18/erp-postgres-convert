using API.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCM.Manager.Dto;
using SCM.Manager.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SCM.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class ItemGroupController : BaseController
    {
        private readonly IItemGroupManager Manager;

        public ItemGroupController(IItemGroupManager manager)
        {
            Manager = manager;
        }
        [HttpGet("GetItemGroupList")]
        public async Task<ActionResult> GetItemGroupList()
        {

            var itemGroups = await Manager.GetItemGroupList();
            return OkResult(itemGroups);
        }

        [HttpPost("CreateItemGroup")]
        public IActionResult CreateItemGroup([FromBody] ItemGroupDto itemGroup)
        {
            Manager.SaveChanges(itemGroup);
            return OkResult(itemGroup);
        }

        [HttpGet("GetItemGroup/{itemGroupId:int}")]
        public async Task<IActionResult> GetItemGroup(int itemGroupId)
        {
            var itemGroups = await Manager.GetItemGroup(itemGroupId);
            return OkResult(itemGroups);
        }
        [HttpGet("DeletItemGroup/{itemGroupId:int}")]
        public IActionResult DeletItemGroup(int itemGroupId)
        {
            Manager.DeleteItemGroup(itemGroupId);
            return OkResult(new { success = true });
        }

    }
}
