using API.Core;
using Core.AppContexts;
using Core.Extensions;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Manager.Core;
using Manager.Core.CommonDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace HRMS.API.Controllers
{
    [Authorize,ApiController, Route("[controller]")]
    public class DepartmentController : BaseController
    {
        private readonly IDepartmentManager Manager;

        public DepartmentController(IDepartmentManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetDepartments")]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await Manager.GetDepartmentListDic();
            return OkResult(departments);
        }

        [HttpGet("GetDepartment/{DepartmentID:int}")]
        public async Task<IActionResult> GetDepartment(int DepartmentID)
        {
            var department = await Manager.GetDepartment(DepartmentID);
            return OkResult(department);
        }

        [HttpPost("CreateDepartment")]
        public async Task<IActionResult> CreateDepartment([FromBody] DepartmentDto department)
        {
            var newData = await Manager.SaveChanges(department);
            if (!string.IsNullOrEmpty(newData.DepartmentNameError)) return OkResult(newData);
            if (!string.IsNullOrEmpty(newData.DepartmentCodeError)) return OkResult(newData);
            if (!string.IsNullOrEmpty(newData.DivisionNameError)) return OkResult(newData);
            return OkResult(department);
        }

        [HttpPost("UploadDepartment")]
        public async Task<IActionResult> UploadDepartment([FromForm] FileRequestDto request)
        {
            var result = await Manager.SaveChangesUploadDepartment(request.File);
            return OkResult(result);
        }
        // POST: /User/Delete

        [HttpGet("Delete/{DepartmentID:int}")]
        public async Task<IActionResult> Delete(int DepartmentID)
        {
            await Manager.Delete(DepartmentID);
            return OkResult(new { success = true });

        }

        [HttpGet("GetExportDepartments")]
        public ActionResult GetExportDepartments(string WhereCondition)
        {
            var deptList = Manager.GetExportDepartments(WhereCondition);
            return OkResult(deptList.Result);
        }
    }
}
