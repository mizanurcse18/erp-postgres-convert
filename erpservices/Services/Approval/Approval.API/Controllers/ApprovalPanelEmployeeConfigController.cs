using API.Core;
using Approval.API.Models;
using Approval.Manager.Dto;
using Approval.Manager.Interfaces;
using Core;
using Core.AppContexts;
using Core.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Approval.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class ApprovalPanelEmployeeConfigController : BaseController
    {
        private readonly IApprovalPanelEmployeeConfigManager Manager;

        public ApprovalPanelEmployeeConfigController(IApprovalPanelEmployeeConfigManager manager)
        {
            Manager = manager;
        }


        [HttpPost("GetListForGrid")]
        public IActionResult GetListForGrid([FromBody] GridParameter parameters)
        {
            var model = Manager.GetListForGrid(parameters);
            model.IsSubmittedFromPopup = parameters.IsSubmittedFromPopup;
            return OkResult(new { parentDataSource = model });
        }

        [HttpGet("GetApprovalPanelEmployees")]
        public async Task<IActionResult> GetApprovalPanelEmployees()
        {
            var ApprovalPanelEmployees = await Manager.GetApprovalPanelEmployeeListDic();
            return OkResult(ApprovalPanelEmployees);
        }

        [HttpPost("GetApprovalPanelEmployee")]
        public async Task<IActionResult> GetApprovalPanelEmployee(ApprovalPanelEmployeeConfigDto ape)
        {
            return await GetApprovalPanelEmployeeByID(ape.APPanelID, ape.DivisionID, ape.DepartmentID, ape.PanelName, ape.DivisionName, ape.DepartmentName, true);
        }

        [HttpGet("GetApprovalPanelEmployeeByID")]
        public async Task<IActionResult> GetApprovalPanelEmployeeByID(int APPanelID, int DivisionID, int DepartmentID, string PanelName, string DivisionName, string DepartmentName, bool IsSuccess)
        {
            var ApprovalPanelEmployee = await Manager.GetApprovalPanelEmployee(APPanelID);
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
                ProxyEmployeeID = 0,
                ProxyEmployeeName = "",
                NFAApprovalSequenceType = 0,
                NFAApprovalSequenceTypeName = ""

                //EmployeeID = ApprovalPanelEmployee.Count > 0 ? ApprovalPanelEmployee[0].EmployeeID : 0,
                //EmployeeName = ApprovalPanelEmployee.Count > 0 ? ApprovalPanelEmployee[0].EmployeeName : "",
                //ProxyEmployeeID = ApprovalPanelEmployee.Count > 0 ? ApprovalPanelEmployee[0].ProxyEmployeeID : 0,
                //SequenceNo = ApprovalPanelEmployee.Count > 0 ? ApprovalPanelEmployee[0].SequenceNo : 0,
                //IsProxyEmployeeEnabled = ApprovalPanelEmployee.Count > 0 ? ApprovalPanelEmployee[0].IsProxyEmployeeEnabled : false,
                //ProxyEmployeeName = ApprovalPanelEmployee.Count > 0 ? ApprovalPanelEmployee[0].ProxyEmployeeName : "",
            };
            return OkResult(new { MasterModel = mastermodel, ChildModels = ApprovalPanelEmployee, status = IsSuccess });
        }

        [HttpPost("GetApprovalPanelEmployeeConfigAllInfoByID")]
        public async Task<IActionResult> GetApprovalPanelEmployeeConfigAllInfoByID(ApprovalPanelEmployeeConfigDto ape)
        {
            var ApprovalPanelEmployee = await Manager.GetApprovalPanelEmployeeSingleInfoForEdit(ape);
            //var ApprovalPanelEmployeeList = await Manager.GetApprovalPanelEmployee(ape.APPanelID, ape.DivisionID, ape.DepartmentID);
            var ApprovalPanelEmployeeList = await Manager.GetApprovalPanelEmployee(ape.APPanelID);


            return OkResult(new { MasterModel = ApprovalPanelEmployee, ChildModels = ApprovalPanelEmployeeList});
        }
        [HttpPost("SaveReorderedList")]
        public async Task<IActionResult> SaveReorderedList([FromBody] ApprovalPanelEmployeeConfigSaveModel apeSaveModel)
        {
            await Manager.SaveReorderedList(apeSaveModel.ChildModels);
            //return OkResult(ApprovalPanelEmployee);
            return await GetApprovalPanelEmployeeByID(apeSaveModel.MasterModel.APPanelID, apeSaveModel.MasterModel.DivisionID, apeSaveModel.MasterModel.DepartmentID, apeSaveModel.MasterModel.PanelName, apeSaveModel.MasterModel.DivisionName, apeSaveModel.MasterModel.DepartmentName, true);
        }
        [HttpPost("CreateApprovalPanelEmployeeConfig")]
        public async Task<IActionResult> CreateApprovalPanelEmployeeConfig([FromBody] ApprovalPanelEmployeeConfigSaveModel apeSaveModel)
        {
           var ApprovalPanelEmployee = await Manager.SaveChanges(apeSaveModel.MasterModel);
            if (ApprovalPanelEmployee.IsNull())
            {
                //return OkResult(new { success = false });
                return await GetApprovalPanelEmployeeByID(apeSaveModel.MasterModel.APPanelID, apeSaveModel.MasterModel.DivisionID, apeSaveModel.MasterModel.DepartmentID, apeSaveModel.MasterModel.PanelName, apeSaveModel.MasterModel.DivisionName, apeSaveModel.MasterModel.DepartmentName, false);
            }
            else
            {
                //return OkResult(new { success = true });
                return await GetApprovalPanelEmployeeByID(apeSaveModel.MasterModel.APPanelID, apeSaveModel.MasterModel.DivisionID, apeSaveModel.MasterModel.DepartmentID, apeSaveModel.MasterModel.PanelName, apeSaveModel.MasterModel.DivisionName, apeSaveModel.MasterModel.DepartmentName, true);
            }
            //return OkResult(ApprovalPanelEmployee);
            //return await GetApprovalPanelEmployeeByID(apeSaveModel.MasterModel.APPanelID, apeSaveModel.MasterModel.DivisionID, apeSaveModel.MasterModel.DepartmentID, apeSaveModel.MasterModel.PanelName, apeSaveModel.MasterModel.DivisionName, apeSaveModel.MasterModel.DepartmentName);
        }

        [HttpGet("DeletePanelConfig/{APPanelEmployeeID:int}")]
        public async Task<IActionResult> DeletePanelConfig(int APPanelEmployeeID)
        {
            await Manager.Delete(APPanelEmployeeID);
            return OkResult(new { success = true });

        }

        [HttpGet("GetApprovalPanelEmployeeList")]
        public async Task<IActionResult> GetApprovalPanelEmployeeList(int EmployeeID, int APPanelID)
        {
            //EmployeeID = EmployeeID.IsZero() ? (int)AppContexts.User.EmployeeID : EmployeeID;
            EmployeeID = (int)AppContexts.User.EmployeeID;
            var panelEmployeeList = Manager.GetApprovalPanelEmployeeList(EmployeeID, APPanelID).Result;
            return OkResult(panelEmployeeList);
        }
        [HttpGet("GetApprovalPanelEmployeeListByPanelID")]
        public async Task<IActionResult> GetApprovalPanelEmployeeListByPanelID(int APPanelID)
        {
            //EmployeeID = EmployeeID.IsZero() ? (int)AppContexts.User.EmployeeID : EmployeeID;
            var EmployeeID = (int)AppContexts.User.EmployeeID;
            var panelEmployeeList = Manager.GetApprovalPanelEmployeeListByPanelID(EmployeeID, APPanelID).Result;
            return OkResult(panelEmployeeList);
        }

        [HttpGet("GetApprovalPanelEmployeeListForLeaveOld")]
        public async Task<IActionResult> GetApprovalPanelEmployeeListForLeaveOld(int EmployeeID, int APPanelID)
        {
            EmployeeID = EmployeeID.IsZero() ? (int)AppContexts.User.EmployeeID : EmployeeID;
            var panelEmployeeList = Manager.GetApprovalPanelEmployeeListForLeaveOld(EmployeeID, APPanelID).Result;
            return OkResult(panelEmployeeList);
        }
        [HttpGet("GetApprovalPanelEmployeeListForLeave")]
        public async Task<IActionResult> GetApprovalPanelEmployeeListForLeave(int EmployeeID, int APPanelID, int LeaveTypeID, bool IsLFA, bool IsFestival, decimal NoOfLeaveDays)
        {
            EmployeeID = EmployeeID.IsZero() ? (int)AppContexts.User.EmployeeID : EmployeeID;
            var panelEmployeeList = Manager.GetApprovalPanelEmployeeListForLeave(EmployeeID, APPanelID, LeaveTypeID, IsLFA, IsFestival, NoOfLeaveDays).Result;
            return OkResult(panelEmployeeList);
        }


        [HttpGet("RemoveApprovalPanel/{APPanelID:int}/{DivisionID:int}/{DepartmentID:int}")]
        public async Task<IActionResult> RemoveApprovalPanel(int APPanelID, int DivisionID, int DepartmentID)
        {
            await Manager.DeleteCompleteApprovalPanel(APPanelID, DivisionID, DepartmentID);
            return OkResult(new { success = true });
        }
        [HttpPost("SaveCopiedPanel")]
        public async Task<IActionResult> SaveCopiedPanel([FromBody] CopyApprovalPanelConfigDto copiedPanelInfo)
        {
            await Manager.CopyPanelData(copiedPanelInfo);
            return OkResult(new { success = true });
        }

        [HttpPost("ReplaceOrProxyForPendingList")]
        public async Task<IActionResult> ReplaceOrProxyForPendingList([FromBody] ReplaceOrProxyForPendingListModel model)
        {
            await Manager.SaveReplaceOrProxyForPendingList(model.MasterModel);

            return OkResult(true);
        }

        [HttpGet("GetApprovalPanelEmployeeListForLeaveEncashment")]
        public async Task<IActionResult> GetApprovalPanelEmployeeListForLeaveEncashment()
        {
            var panelEmployeeList = Manager.GetApprovalPanelEmployeeListForLeaveEncashment().Result;
            return OkResult(panelEmployeeList);
        }
    }
}