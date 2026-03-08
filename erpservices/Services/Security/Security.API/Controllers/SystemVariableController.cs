using API.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Security.Manager.Dto;
using Security.Manager.Interfaces;
using System.Threading.Tasks;

namespace Security.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class SystemVariableController : BaseController
    {
        private readonly ISystemVariableManager Manager;

        public SystemVariableController(ISystemVariableManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetSystemVariableList")]
        public async Task<ActionResult> GetSystemVariableList()
        {
            var systemVariables = await Manager.GetSystemVariableList();
            return OkResult(systemVariables);
        }

        [HttpPost("CreateSystemVariable")]
        public IActionResult CreateSystemVariable([FromBody] SystemVariableDto systemVariable)
        {
            Manager.SaveChanges(systemVariable);
            return OkResult(systemVariable);
        }

        //[HttpPost("AddNewentityTypeName")]
        //public IActionResult AddNewentityTypeName([FromBody] SystemVariableDto systemVariable)
        //{
        //    Manager.SaveChangesNew(systemVariable);
        //    return OkResult(systemVariable);
        //}

        [HttpGet("GetSystemVariable/{systemVariableId:int}")]
        public async Task<IActionResult> GetSystemVariable(int systemVariableId)
        {
            var systemVariable = await Manager.GetSystemVariable(systemVariableId);
            return OkResult(systemVariable);
        }

        [HttpGet("DeleteSystemVariable/{systemVariableId:int}")]
        public IActionResult DeleteSystemVariable(int systemVariableId)
        {
            Manager.DeleteSystemVariable(systemVariableId);
            return Ok(new { Success = true });
        }
    }
} 