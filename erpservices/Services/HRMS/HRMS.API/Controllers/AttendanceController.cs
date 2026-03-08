using API.Core;
using API.Core.Hubs;
using API.Core.Interface;
using Core;
using Core.AppContexts;
using Core.Extensions;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Manager.Core.CommonDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRMS.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class AttendanceController : BaseController
    {
        private readonly IAttendanceManager Manager;
        private readonly IRemoteAttendanceManager RemoteManager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;

        public AttendanceController(IAttendanceManager manager, IRemoteAttendanceManager remoteManager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            RemoteManager = remoteManager;
            _notificationHub = notificationHub;
        }

        [HttpGet("GetAttendanceWidgets/{PersonID:int}")]
        public async Task<ActionResult> GetAttendanceWidgets(int PersonID)
        {

            var widgets = Manager.GetAttendanceWidgets(PersonID.IsZero() ? AppContexts.User.PersonID : PersonID);
            return OkResult(widgets);
        }

        [HttpGet("GetSelectedDateForEdit/{id:int}")]
        public async Task<ActionResult> GetSelectedDateForEdit(int id)
        {
            var data = Manager.GetSelectedDateForEdit(id).Result;

            return OkResult(data);
        }
        [HttpGet("GetSelfAttendanceWidget/{PersonID:int}")]
        public async Task<ActionResult> GetSelfAttendanceWidget(int PersonID)
        {
            var widget = Manager.SelfAttendance(PersonID.IsZero() ? AppContexts.User.PersonID : PersonID);
            var widgets = new List<Widget>
            {
                widget
            };
            return OkResult(widgets);
        }

        [HttpGet("GetAllEmployeeAttendance")]
        public async Task<IActionResult> GetAllEmployeeAttendance(string AttendanceType, string SearchText)
        {
            var widget = await Manager.GetAllEmployeeAttendance(AttendanceType, SearchText);
            var widgets = new List<Widget>
            {
                widget
            };
            return OkResult(widgets);
        }

        [HttpGet("GetEmployeeAttendanceSummaryBarchart/{PersonID:int}")]
        public async Task<ActionResult> GetEmployeeAttendanceSummaryBarchart(int PersonID)
        {
            var widgets = await Manager.GetEmployeeAttendanceSummaryBarchartAsync(PersonID.IsZero() ? AppContexts.User.PersonID : PersonID);
            return OkResult(widgets);
        }

        [HttpPost("SaveRemoteAttendance")]
        public async Task<IActionResult> SaveRemoteAttendance([FromBody] RemoteAttendanceDto data)
        {
            var newData = await RemoteManager.SaveChanges(data);
            if (!string.IsNullOrEmpty(newData.InTimeError)) return OkResult(newData);

            //await RemoteManager.SaveChanges(data);
            await _notificationHub.Clients.All.ReceiveNotification("RemoteAttendance");
            return await GetRemoteAttendance();
        }

        [HttpPost("SaveApproverRemoteAttendanceStatus")]
        public async Task<IActionResult> SaveApproverRemoteAttendanceStatus([FromBody] RemoteAttendanceDto data)
        {
            await RemoteManager.ApproverStatusChange(data);
            await _notificationHub.Clients.All.ReceiveNotification("RemoteAttendance");
            return await GetRemoteAttendance();
        }

        [HttpGet("GetRemoteAttendance")]
        public async Task<IActionResult> GetRemoteAttendance()
        {
            //var attendanceObjMachine = RemoteManager.GetPresentRemoteAttendanceFromMachine().Result;
            var attendanceObj = RemoteManager.GetPresentRemoteAttendance().Result;
            var AttendanceList = RemoteManager.GetRemoteAttendanceListExceptApproved().Result;
            var lastEntry = RemoteManager.GetLastEntryType().Result;
            var attendance = new
            {
                //In_Time = attendanceObjMachine.Count > 0 ? attendanceObjMachine["IN_TIME"] : attendanceObj["IN_TIME"],
                //Out_Time = attendanceObjMachine.Count > 0 && attendanceObjMachine["OUT_TIME"].ToString() == attendanceObjMachine["IN_TIME"].ToString() ? attendanceObj["OUT_TIME"].ToString()
                //: attendanceObjMachine.Count > 0 && attendanceObjMachine["OUT_TIME"].ToString() != attendanceObjMachine["IN_TIME"].ToString() ? attendanceObjMachine["OUT_TIME"].ToString()
                //: attendanceObj["OUT_TIME"],
                In_Time = attendanceObj.Count > 0 ? attendanceObj["IN_TIME"] : "",
                Out_Time = attendanceObj.Count > 0 ? attendanceObj["OUT_TIME"].ToString() == "" ? attendanceObj["IN_TIME"] : attendanceObj["OUT_TIME"] : "",

                IsRemoteIP = AppContexts.IsRemoteIP(RemoteManager.GetIPAddressList().Result.Select(x => x["IPNumber"].ToString()).ToList()),
                AttendanceList = AttendanceList,
                EmployeeNote = string.Empty,
                Division = new { value = 30, label = "DHAKA DIVISION" },
                District = new { value = 26, label = "DHAKA" },
                IPAddress = AppContexts.GetIPAddress(),
                LastEntryType = lastEntry.Count > 0 ? lastEntry["EntryType"] : ""
            };
            return OkResult(attendance);
        }

        [HttpPost("GetListForGrid")]
        public async Task<IActionResult> GetListForGrid([FromBody] GridParameter parameters)
        {
            var model = await RemoteManager.GetListForGrid(parameters);
            model.IsSubmittedFromPopup = parameters.IsSubmittedFromPopup;
            return OkResult(new { parentDataSource = model });
        }

        [HttpPost("GetListForGridAll")]
        public async Task<IActionResult> GetListForGridAll([FromBody] GridParameter parameters)
        {
            var model = await RemoteManager.GetListForGridAll(parameters);
            model.IsSubmittedFromPopup = parameters.IsSubmittedFromPopup;
            return OkResult(new { parentDataSource = model });
        }
        [HttpPost("GetListRemoteAttendanceExcel")]
        public async Task<IActionResult> GetListRemoteAttendanceExcel([FromBody] GridParameter parameters)
        {
            var model = RemoteManager.GetListRemoteAttendanceExcel(parameters);
            //model.IsSubmittedFromPopup = parameters.IsSubmittedFromPopup;
            return OkResult(model.Result);
        }

        [HttpGet("GetPendingRemoteAttendance")]
        public async Task<IActionResult> GetPendingRemoteAttendance()
        {
            var AttendanceList = RemoteManager.GetPendingRemoteAttendanceList().Result;
            var attendance = new
            {
                AttendanceList = AttendanceList
            };
            return OkResult(attendance);
        }
        [HttpGet("GetPendingRemoteAttendanceListForDashboard")]
        public async Task<IActionResult> GetPendingRemoteAttendanceListForDashboard()
        {
            var AttendanceList = RemoteManager.GetPendingRemoteAttendanceListForDashboard().Result;
            var attendance = new
            {
                AttendanceList = AttendanceList
            };
            return OkResult(attendance);
        }

        [HttpPost("GetRemoteAttendanceDetails")]
        public async Task<IActionResult> GetRemoteAttendanceDetails([FromBody] RemoteAttendanceDto data)
        {
            var res = RemoteManager.GetRemoteAttendanceDetails(data).Result;
            return OkResult(res);
        }

        [HttpPost("SaveSelectedRemoteAttendanceStatus")]
        public async Task<IActionResult> SaveSelectedRemoteAttendanceStatus([FromBody] RemoteAttendanceDto data)
        {
            await RemoteManager.SelectedApproverStatusChange(data);
            await _notificationHub.Clients.All.ReceiveNotification("RemoteAttendance");
            return await GetRemoteAttendance();
        }

        [HttpGet("GetAttendanceDetails/{employeeCode}/{attendanceDate}")]
        public IActionResult GetAttendanceDetails(string employeeCode, string attendanceDate)
        {
            DateTime dt = attendanceDate.ToDate();

            var attendanceSummary = Manager.GetAttendanceSummaryDetails(employeeCode, dt).Result;
            var unapprovedRemoteAttendanceDetails = Manager.GetUnapprovedRemoteAttendanceDetails(employeeCode, dt).Result;
            var hrEditedAttendanceDetails = Manager.GetHrEditedAttendanceDetails(employeeCode, dt).Result;
            var attendance = new
            {
                AttendanceSummary = attendanceSummary,
                UnapprovedRemoteAttendance = unapprovedRemoteAttendanceDetails,
                HrEditedAttendanceDetails = hrEditedAttendanceDetails
            };
            return OkResult(attendance);
        }

        [HttpPost("UpdateAttendance")]
        public async Task<IActionResult> UpdateAttendance([FromBody] AttendanceSummaryHRMSDto data)
        {
            Manager.UpdateAttendance(data);
            return OkResult(data);
        }

        [HttpPost("SeaerchAttendance")]
        public async Task<ActionResult> SeaerchAttendance(AttendanceSummaryHRMSDto data)
        {
            var widget = Manager.SelfAttendanceSearch(data);
            var widgets = new List<Widget>
            {
                widget
            };
            return OkResult(widgets);
        }

        [HttpPost("SearchSelfAttendance")]
        public async Task<ActionResult> SearchSelfAttendance(AttendanceSummaryHRMSDto data)
        {
            data.PersonID = AppContexts.User.PersonID;
            data.EmployeeID = (int)AppContexts.User.EmployeeID;
            var widgets = Manager.GetSelfAttendanceSearch(data);
            return OkResult(widgets.Result);

        }
    }
}