using API.Core;
using Approval.API.Models;
using Approval.Manager.Dto;
using Approval.Manager.Interfaces;
using Core.AppContexts;
using Core.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Transactions;

namespace Approval.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class ApprovalPanelEmployeeController : BaseController
    {
        private readonly IApprovalPanelEmployeeManager Manager;

        public ApprovalPanelEmployeeController(IApprovalPanelEmployeeManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetApprovalPanelEmployees")]
        public async Task<IActionResult> GetApprovalPanelEmployees()
        {
            var ApprovalPanelEmployees = await Manager.GetApprovalPanelEmployeeListDic();
            return OkResult(ApprovalPanelEmployees);
        }


        [HttpPost("GetApprovalPanelEmployee")]
        public async Task<IActionResult> GetApprovalPanelEmployee(ApprovalPanelEmployeeDto ape)
        {
            return await GetApprovalPanelEmployeeByID(ape.APPanelID, ape.DivisionID, ape.DepartmentID, ape.PanelName, ape.DivisionName, ape.DepartmentName);
        }

        [HttpGet("GetApprovalPanelEmployeeByID")]
        public async Task<IActionResult> GetApprovalPanelEmployeeByID(int APPanelID, int DivisionID, int DepartmentID, string PanelName, string DivisionName, string DepartmentName)
        {
            var ApprovalPanelEmployee = await Manager.GetApprovalPanelEmployee(APPanelID, DivisionID, DepartmentID);
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
            return OkResult(new { MasterModel = mastermodel, ChildModels = ApprovalPanelEmployee });
        }

        [HttpPost("GetApprovalPanelEmployeeAllInfoByID")]
        public async Task<IActionResult> GetApprovalPanelEmployeeAllInfoByID(ApprovalPanelEmployeeDto ape)
        {
            var ApprovalPanelEmployee = await Manager.GetApprovalPanelEmployeeSingleInfoForEdit(ape);
            var ApprovalPanelEmployeeList = await Manager.GetApprovalPanelEmployee(ape.APPanelID, ape.DivisionID, ape.DepartmentID);


            return OkResult(new { MasterModel = ApprovalPanelEmployee, ChildModels = ApprovalPanelEmployeeList });
        }
        [HttpPost("SaveReorderedList")]
        public async Task<IActionResult> SaveReorderedList([FromBody] ApprovalPanelEmployeeSaveModel apeSaveModel)
        {
            await Manager.SaveReorderedList(apeSaveModel.ChildModels);
            //return OkResult(ApprovalPanelEmployee);
            return await GetApprovalPanelEmployeeByID(apeSaveModel.MasterModel.APPanelID, apeSaveModel.MasterModel.DivisionID, apeSaveModel.MasterModel.DepartmentID, apeSaveModel.MasterModel.PanelName, apeSaveModel.MasterModel.DivisionName, apeSaveModel.MasterModel.DepartmentName);
        }
        [HttpPost("CreateApprovalPanelEmployee")]
        public async Task<IActionResult> CreateApprovalPanelEmployee([FromBody] ApprovalPanelEmployeeSaveModel apeSaveModel)
        {
            await Manager.SaveChanges(apeSaveModel.MasterModel);
            //return OkResult(ApprovalPanelEmployee);
            return await GetApprovalPanelEmployeeByID(apeSaveModel.MasterModel.APPanelID, apeSaveModel.MasterModel.DivisionID, apeSaveModel.MasterModel.DepartmentID, apeSaveModel.MasterModel.PanelName, apeSaveModel.MasterModel.DivisionName, apeSaveModel.MasterModel.DepartmentName);
        }

        [HttpGet("DeleteSingle/{APPanelEmployeeID:int}")]
        public async Task<IActionResult> DeleteSingle(int APPanelEmployeeID)
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
        [HttpGet("GetApprovalPanelEmployeeListForStNfa")]
        public async Task<IActionResult> GetApprovalPanelEmployeeListForStNfa(int EmployeeID, int APPanelID, int TemplateID)
        {
            //EmployeeID = EmployeeID.IsZero() ? (int)AppContexts.User.EmployeeID : EmployeeID;
            EmployeeID = (int)AppContexts.User.EmployeeID;
            var panelEmployeeList = Manager.GetApprovalPanelEmployeeListForStNfa(EmployeeID, APPanelID, TemplateID).Result;
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


        //[HttpGet("GetApprovalPanelEmployeeListForLeave")]
        //public async Task<IActionResult> GetApprovalPanelEmployeeListForLeave(int EmployeeID, int APPanelID,int LeaveTypeID, bool IsLFA, bool IsFestival, decimal NoOfLeaveDays)
        //{
        //    try
        //    {
        //        int maxRetries = 3;
        //        int retryCount = 0;
        //        int delayMs = 1000; // Start with 1 second delay

        //        while (retryCount < maxRetries)
        //        {
        //            try
        //            {
        //                EmployeeID = EmployeeID.IsZero() ? (int)AppContexts.User.EmployeeID : EmployeeID;

        //                // Use TransactionScope with ReadUncommitted to reduce deadlocks
        //                using (var scope = new TransactionScope(TransactionScopeOption.Required,
        //                    new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }))
        //                {
        //                    var panelEmployeeList = await Manager.GetApprovalPanelEmployeeListForLeave(
        //                        EmployeeID, APPanelID, LeaveTypeID, IsLFA, IsFestival, NoOfLeaveDays);

        //                    scope.Complete();
        //                    return OkResult(panelEmployeeList);
        //                }
        //            }
        //            catch (SqlException ex) when (ex.Number == 1205) // SQL Server deadlock error number
        //            {
        //                retryCount++;
        //                if (retryCount == maxRetries)
        //                    throw;

        //                await Task.Delay(delayMs * retryCount); // Exponential backoff
        //                continue;
        //            }
        //        }

        //        throw new Exception("Max retries reached for deadlock resolution");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new
        //        {
        //            Error = true,
        //            Message = ex.Message,
        //            StatusCode = 400,
        //            ErrorId = DateTime.Now.Ticks
        //        });
        //    }
        //}


        [HttpGet("RemoveApprovalPanel/{APPanelID:int}/{DivisionID:int}/{DepartmentID:int}")]
        public async Task<IActionResult> RemoveApprovalPanel(int APPanelID, int DivisionID, int DepartmentID)
        {
            await Manager.DeleteCompleteApprovalPanel(APPanelID, DivisionID, DepartmentID);
            return OkResult(new { success = true });
        }
        [HttpPost("SaveCopiedPanel")]
        public async Task<IActionResult> SaveCopiedPanel([FromBody] CopyApprovalPanelDto copiedPanelInfo)
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



        [HttpGet("GetApprovalPanelEmployeeListByMinMaxLimit")]
        public async Task<IActionResult> GetApprovalPanelEmployeeListByMinMaxLimit(int EmployeeID, int APTypeID, double Total)
        {
            //EmployeeID = EmployeeID.IsZero() ? (int)AppContexts.User.EmployeeID : EmployeeID;
            EmployeeID = (int)AppContexts.User.EmployeeID;
            var panelEmployeeList = Manager.GetApprovalPanelEmployeeListByMinMaxLimit(EmployeeID, APTypeID, Total).Result;
            return OkResult(panelEmployeeList);
        }

        [HttpGet("GetDynamicApprovalPanelEmployeeList")]
        public async Task<IActionResult> GetDynamicApprovalPanelEmployeeList(int EmployeeID, int ApTypeID, decimal Amount)
        {
            EmployeeID = EmployeeID.IsZero() ? (int)AppContexts.User.EmployeeID : EmployeeID;
            var panelEmployeeList = Manager.GetDynamicApprovalPanelEmployeeList(EmployeeID, ApTypeID, Amount).Result;
            return OkResult(panelEmployeeList);
        }


    }
}