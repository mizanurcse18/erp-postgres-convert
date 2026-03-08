using API.Core;
using Approval.API.Models;
using Approval.Manager.Dto;
using Approval.Manager.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Approval.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class ApprovalPanelForwardEmployeeController : BaseController
    {
        private readonly IApprovalPanelForwardEmployeeManager Manager;

        public ApprovalPanelForwardEmployeeController(IApprovalPanelForwardEmployeeManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetApprovalPanelForwardEmployees")]
        public async Task<IActionResult> GetApprovalPanelForwardEmployees()
        {
            var ApprovalPanelForwardEmployees = await Manager.GetApprovalPanelForwardEmployeeListDic();
            return OkResult(ApprovalPanelForwardEmployees);
        }

        [HttpPost("GetApprovalPanelForwardEmployee")]
        public async Task<IActionResult> GetApprovalPanelForwardEmployee(ApprovalPanelForwardEmployeeDto ape)
        {
                return await GetApprovalPanelForwardEmployeeByID(ape.APPanelID, ape.DivisionID, ape.DepartmentID, ape.PanelName, ape.DivisionName, ape.DepartmentName);
        }

        [HttpGet("GetApprovalPanelForwardEmployeeByID")]
        public async Task<IActionResult> GetApprovalPanelForwardEmployeeByID(int APPanelID, int DivisionID, int DepartmentID, string PanelName, string DivisionName, string DepartmentName)
        {
            var ApprovalPanelForwardEmployee = await Manager.GetApprovalPanelForwardEmployee(APPanelID, DivisionID, DepartmentID);
            var mastermodel = new
            {
                APPanelID = APPanelID,
                PanelName = PanelName,
                DivisionID = DivisionID,
                DivisionName = DivisionName,
                DepartmentID = DepartmentID,
                DepartmentName = DepartmentName,


                EmployeeID = 0,
                EmployeeName = "",
                //EmployeeID = ApprovalPanelForwardEmployee.Count > 0 ? ApprovalPanelForwardEmployee[0].EmployeeID : 0,
                //EmployeeName = ApprovalPanelForwardEmployee.Count > 0 ? ApprovalPanelForwardEmployee[0].EmployeeName : "",
                //ProxyEmployeeID = ApprovalPanelForwardEmployee.Count > 0 ? ApprovalPanelForwardEmployee[0].ProxyEmployeeID : 0,
                //SequenceNo = ApprovalPanelForwardEmployee.Count > 0 ? ApprovalPanelForwardEmployee[0].SequenceNo : 0,
                //IsProxyEmployeeEnabled = ApprovalPanelForwardEmployee.Count > 0 ? ApprovalPanelForwardEmployee[0].IsProxyEmployeeEnabled : false,
                //ProxyEmployeeName = ApprovalPanelForwardEmployee.Count > 0 ? ApprovalPanelForwardEmployee[0].ProxyEmployeeName : "",
            };
            return OkResult(new { MasterModel = mastermodel, ChildModels = ApprovalPanelForwardEmployee });
        }

        [HttpPost("GetApprovalPanelForwardEmployeeAllInfoByID")]
        public async Task<IActionResult> GetApprovalPanelForwardEmployeeAllInfoByID(ApprovalPanelForwardEmployeeDto ape)
        {
            var ApprovalPanelForwardEmployee = await Manager.GetApprovalPanelForwardEmployeeSingleInfoForEdit(ape);
            var ApprovalPanelForwardEmployeeList = await Manager.GetApprovalPanelForwardEmployee(ape.APPanelID, ape.DivisionID, ape.DepartmentID);
            
            
            return OkResult(new { MasterModel = ApprovalPanelForwardEmployee, ChildModels = ApprovalPanelForwardEmployeeList });
        }
        [HttpPost("SaveReorderedList")]
        public async Task<IActionResult> SaveReorderedList([FromBody] ApprovalPanelForwardEmployeeSaveModel apeSaveModel)
        {
            await Manager.SaveReorderedList(apeSaveModel.ChildModels);
            //return OkResult(ApprovalPanelForwardEmployee);
            return await GetApprovalPanelForwardEmployeeByID(apeSaveModel.MasterModel.APPanelID, apeSaveModel.MasterModel.DivisionID, apeSaveModel.MasterModel.DepartmentID, apeSaveModel.MasterModel.PanelName, apeSaveModel.MasterModel.DivisionName, apeSaveModel.MasterModel.DepartmentName);
        }
        [HttpPost("CreateApprovalPanelForwardEmployee")]
        public async Task<IActionResult> CreateApprovalPanelForwardEmployee([FromBody] ApprovalPanelForwardEmployeeSaveModel apeSaveModel)
        {
            await Manager.SaveChanges(apeSaveModel.MasterModel);
            //return OkResult(ApprovalPanelForwardEmployee);
            return await GetApprovalPanelForwardEmployeeByID(apeSaveModel.MasterModel.APPanelID, apeSaveModel.MasterModel.DivisionID, apeSaveModel.MasterModel.DepartmentID, apeSaveModel.MasterModel.PanelName, apeSaveModel.MasterModel.DivisionName, apeSaveModel.MasterModel.DepartmentName);
        }
        // POST: /User/Delete

        [HttpGet("DeleteSingle/{APPanelEmployeeID:int}")]
        public async Task<IActionResult> DeleteSingle(int APPanelEmployeeID)
        {
            await Manager.Delete(APPanelEmployeeID);
            return OkResult(new { success = true });

        }
        [HttpPost("SaveCopiedPanel")]
        public async Task<IActionResult> SaveCopiedPanel([FromBody] CopyApprovalPanelDto copiedPanelInfo)
        {
            await Manager.CopyPanelData(copiedPanelInfo);
            return OkResult(new { success = true });
        }
        [HttpGet("RemoveApprovalPanel/{APPanelID:int}/{DivisionID:int}/{DepartmentID:int}")]
        public async Task<IActionResult> RemoveApprovalPanel(int APPanelID, int DivisionID, int DepartmentID)
        {
            await Manager.DeleteCompleteApprovalPanel(APPanelID, DivisionID, DepartmentID);
            return OkResult(new { success = true });
        }

    }
}