
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using API.Core;
using Security.API.Models;
using Security.Manager;
using System.Globalization;
using System.Collections.Generic;
using System;
using DAL.Core.Extension;
using Security.Manager.Interfaces;
using Core;
using Newtonsoft.Json;

namespace Security.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class CommonInterfaceController : BaseController
    {
        private readonly ICommonInterfaceManager Manager;

        public CommonInterfaceController(ICommonInterfaceManager manager)
        {
            Manager = manager;
        }

        // GET: /Event/GetAll
        [HttpGet("LoadCommonInterfaceUIData/{MenuID:int}")]
        public async Task<IActionResult> LoadCommonInterfaceUIData(int MenuID)
        {
            var commonInterface = await Manager.LoadCommonInterfaceUIData(MenuID);
            var commonInterfaceFields = await Manager.LoadCommonInterfaceUIFields(MenuID);
            var interfaceData = new
            {
                commonInterface,
                commonInterfaceFields
            };
            return OkResult(interfaceData);
        }

        [HttpGet("GetCommonInterfaceData/{PrimaryKeyID}/{MenuID}")]
        public async Task<IActionResult> GetCommonInterfaceData(string PrimaryKeyID, int MenuID)
        {
            var master = await Manager.GetCommonInterfaceData(PrimaryKeyID, MenuID);
            return OkResult(master);
        }

        [HttpPost("GetListForGrid")]
        public async Task<IActionResult> GetListForGrid([FromBody] GridParameter parameters)
        {
            var model = Manager.GetListForGrid(parameters);
            return OkResult(new { parentDataSource = model });
        }
        [HttpPost("Save")]
        public async Task<IActionResult> Save([FromBody] dynamic jsonData)
        {
            //dynamic data = JsonConvert.DeserializeObject<dynamic>(jsonData.ToString());
            var response = await Manager.SaveChanges(jsonData);
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }
    }
}
