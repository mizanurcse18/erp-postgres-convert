using API.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Security.Manager.Dto;
using Security.Manager.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Security.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]

    public class CommonController : BaseController
    {
        private readonly ICommonManager Manager;

        public CommonController(ICommonManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetSystemVariableDataByEntityTypeID/{entityTypeID:int}")]
        public async Task<IActionResult> GetSystemVariableDataByEntityTypeID(int entityTypeID)
        {
            var data = await Manager.GetSystemVariableByEntityTypeID(entityTypeID);
            return OkResult(data);
        }
        [HttpPost("SaveSystemVariableData")]
        public IActionResult SaveSystemVariableData([FromBody] SystemVariableDto model)
        {
            Manager.SaveChanges(model);
            return OkResult(model);
        }
        [HttpGet("GetSystemVariable/{systemVariableID:int}")]
        public async Task<IActionResult> GetSystemVariable(int systemVariableID)
        {
            var data = await Manager.GetSystemVariable(systemVariableID);
            return OkResult(data);
        }
        [HttpGet("DeleteSystemVariable/{systemVariableID:int}")]
        public IActionResult DeleteSystemVariable(int systemVariableID)
        {
            Manager.DeleteSystemVariable(systemVariableID);
            return OkResult(new { Success = true });
        }

        [HttpGet("DeleteFile")]
        public async Task<IActionResult> DeleteFile(string folderName, string fileName)
        {
            var status = Manager.DeleteFileFromPath(folderName, fileName);
            return OkResult(new { Master = status });
        }

    }
}
