using API.Core;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRMS.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class DivisionDepartmentSetupController : BaseController
    {
        private readonly IDivisionManager DivManager;
        private readonly IDepartmentManager DManager;
        private readonly IDivisionDepartmentSetupManager Manager;
        public DivisionDepartmentSetupController(IDivisionDepartmentSetupManager manager, IDivisionManager divmanager, IDepartmentManager dmanager)
        {
            Manager = manager;
            DivManager = divmanager;
            DManager = dmanager;
            
        }


        [HttpGet("GetAll")]
        public async Task<IActionResult> GetDivisionDepartment()
        {
           
           var dlist = await Manager.GetDivisionDepartment();

            return OkResult(dlist);
        }

        [HttpPost("Save")]
        public IActionResult SaveDivDeptSetup(List<DivisionDepartmentHeadMapDto> list)
        {
            Manager.SaveDivDeptSetup(list);
            return OkResult(true);
        }



    }
}
