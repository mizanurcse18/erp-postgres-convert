using API.Core;
using API.Core.Hubs;
using API.Core.Interface;
using ClosedXML.Excel;
using Core;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HRMS.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class LeaveEncashmentApplicationController : BaseController
    {
        private readonly ILeaveEncashmentApplicationManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;
        public LeaveEncashmentApplicationController(ILeaveEncashmentApplicationManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }

        #region Leave Encashment Application
        //Get List for Grid
        [HttpPost("GetLeaveEncashmentApplicationList")]
        public async Task<IActionResult> GetLeaveEncashmentApplicationList([FromBody] GridParameter parameters)
        {
            var model = Manager.GetLeaveEncashmentApplicationList(parameters);
            return OkResult(new { parentDataSource = model });

        }
        //Get List for Grid
        [HttpPost("GetLeaveEncashmentApplicationListForApproval")]
        public async Task<IActionResult> GetLeaveEncashmentApplicationListForApproval([FromBody] GridParameter parameters)
        {
            var model = Manager.GetLeaveEncashmentApplicationListForHODApproval(parameters);
            return OkResult(new { parentDataSource = model });

        }
        [HttpGet("GetAnnualLeaveBalanceAndDetails")]
        public IActionResult GetAnnualLeaveBalanceAndDetails()
        {

            var model = Manager.GetAnnualLeaveBalanceAndDetails();
            return OkResult(model.Result);
        }
        [HttpPost("CreateLeaveEncashmentApplication")]
        public IActionResult CreateLeaveEncashmentApplication([FromBody] LeaveEncashmentApplication application)
        {
            var response = Manager.SaveChanges(application).Result;
            _notificationHub.Clients.All.ReceiveNotification("LeaveEncashmentApplication");
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }
        [HttpGet("Get/{ALEMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> Get(int ALEMasterID, int approvalProcessID = 0)
        {
            var application = await Manager.GetLeaveEncashmentApplication(ALEMasterID, approvalProcessID);

            return OkResult(application);
        }

        [HttpGet("GetLeaveEncashmentApplicationDetails/{ALEMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetLeaveEncashmentApplicationDetails(int ALEMasterID, int ApprovalProcessID)
        {
            var application = await Manager.GetLeaveEncashmentApplicationWithCommentsForApproval(ALEMasterID, ApprovalProcessID);

            return OkResult(application);
        }



        [HttpPost("GetAllLeaveEncashmentApplicationList")]
        public async Task<IActionResult> GetAllLeaveEncashmentApplicationList([FromBody] GridParameter parameters)
        {
            var model = Manager.GetAllLeaveEncashmentApplicationList(parameters);
            return OkResult(new { parentDataSource = model });
        }

        #endregion

        #region Leave Policy Settings
        [HttpGet("CheckMultipleSupervisor")]
        public async Task<IActionResult> CheckMultipleSupervisor()
        {
            var isMulti = Manager.CheckMultipleSupervisor();
            return OkResult(isMulti);
        }
        #endregion

        #region Encashment Eligible Check
        [HttpGet("CheckEncashmentEligible")]
        public async Task<IActionResult> CheckEncashmentEligible()
        {
            var isEligible = Manager.CheckEncashmentEligible();
            return OkResult(isEligible);
        }
        #endregion

        #region All Leave Encashment List
        [HttpPost("GetAllLEApplicationList")]
        public async Task<IActionResult> GetAllLEApplicationList([FromBody] GridParameter parameters)
        {
            var model = Manager.GetAllLEApplicationList(parameters);
            return OkResult(new { parentDataSource = model });
        }

        #endregion

        #region Download Enashment Application
        [HttpGet("DownloadLeaveEncashmentApplication/{ALEWMasterID:int}")]
        public async Task<ActionResult> DownloadLeaveEncashmentApplication(int ALEWMasterID)
        {

           var leaveEncashmentList = Manager.GetAllLeaveEncashmentApplications(ALEWMasterID);


            return OkResult(leaveEncashmentList);

            //DataSet leaveEncashmentDataSet = Manager.GetAllLeaveEncashmentApplicationList(ALEWMasterID).Result;
            //var stream = new MemoryStream();
            //using (var workbok = new XLWorkbook())
            //{                
            //    workbok.Worksheets.Add(leaveEncashmentDataSet.Tables[0], "Leave Encashment");
            //    workbok.SaveAs(stream);
            //}
            ////DataSet leaveEncashmentDataSet = Manager.GetAllLeaveEncashmentApplicationList(ALEWMasterID).Result;

            ////var stream = new MemoryStream();
            ////using (var package = new ExcelPackage(stream))
            ////{
            ////    #region IAS_TXN_ALL
            ////    var worksheet = package.Workbook.Worksheets.Add("Leave Encashment");
            ////    worksheet.Cells.LoadFromDataTable(leaveEncashmentDataSet.Tables[0], true);
            ////    #endregion IAS_TXN_ALL


            ////    package.Save();
            ////}
            //stream.Position = 0;
            //////string excelname = $"SOA-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.xlsx";
            //////return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelname);
            //byte[] byteArray = stream.ToArray();
            //return OkResult(byteArray);
        }

        #endregion
    }
}
