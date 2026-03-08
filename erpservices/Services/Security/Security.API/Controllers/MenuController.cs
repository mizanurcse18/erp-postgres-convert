using API.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Security.Manager.Dto;
using Security.Manager.Interfaces;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Threading.Tasks;

namespace Security.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class MenuController : BaseController
    {
        private readonly IMenuManager _manager;

        public MenuController(IMenuManager manager)
        {
            _manager = manager;
        }

        [HttpGet("GetMenuList")]
        public async Task<ActionResult> GetMenus()
        {
            var menuItems = await _manager.GetMenus();
            return Ok(menuItems); 
        }
    }
}
