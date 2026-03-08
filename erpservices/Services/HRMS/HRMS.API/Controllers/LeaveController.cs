using API.Core;
using API.Core.Hubs;
using API.Core.Interface;
using Core;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRMS.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class LeaveController : BaseController
    {
        private readonly ILeaveManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;
        public LeaveController(ILeaveManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }

        #region Leave Application
        [HttpGet("GetLeaveBalanceAndDetails/{id:int}/{startdate}/{enddate}/{employeeLeaveAID:int}")]
        public IActionResult GetLeaveBalanceAndDetails(int id, string startDate, string endDate, int employeeLeaveAID)
        {
            DateTime fromDate = DateTime.ParseExact(startDate, "dd-MM-yyyy", null);
            DateTime toDate = DateTime.ParseExact(endDate, "dd-MM-yyyy", null);
            var model = Manager.GetLeaveBalanceAndDetails(id, fromDate, toDate, employeeLeaveAID);
            return OkResult(model.Result);
        }

        [HttpGet("GetLeaveBalanceAndDetailsHr/{id:int}/{startdate}/{enddate}/{employeeLeaveAID:int}/{employeeID:int}")]
        public IActionResult GetLeaveBalanceAndDetailsHr(int id, string startDate, string endDate, int employeeLeaveAID, int employeeID)
        {
            //if (employeeID == 0)
            //{
            //    return OkResult(new { });
            //}

            DateTime fromDate = DateTime.ParseExact(startDate, "dd-MM-yyyy", null);
            DateTime toDate = DateTime.ParseExact(endDate, "dd-MM-yyyy", null);
            var model = Manager.GetLeaveBalanceAndDetailsHr(id, fromDate, toDate, employeeLeaveAID, employeeID);
            return OkResult(model.Result);
        }
        
        [HttpPost("CreateLeaveApplication")]
        public async Task<IActionResult> CreateLeaveApplication([FromBody] LeaveApplication application)
        {
            var response = await Manager.SaveChanges(application);
            await _notificationHub.Clients.All.ReceiveNotificationUserWise("LeaveApplication", response.EmployeeIds, response.NotificationMessage,response.CurrentNotificaitonEmplyeeID);
            return OkResult(new { status = response.Success, message = response.Message });
            //return OkResult(application);
        }

        [HttpPost("CreateLeaveApplicationHr")]
        public async Task<IActionResult> CreateLeaveApplicationHr([FromBody] LeaveApplication application)
        {
            application.IsHrApplied = true;
            var response = await Manager.SaveChanges(application);
            await _notificationHub.Clients.All.ReceiveNotificationUserWise("LeaveApplication", response.EmployeeIds, response.NotificationMessage, response.CurrentNotificaitonEmplyeeID);
            return OkResult(new { status = response.Success, message = response.Message });
            //return OkResult(application);
        }

        [HttpPost("GetLeaveApplicationList")]
        public async Task<IActionResult> GetLeaveApplicationList([FromBody] GridParameter parameters)
        {
            var model = Manager.GetLeaveApplicationList(parameters);
            return OkResult(new { parentDataSource = model });

        }

        [HttpGet("Get/{employeeLeaveAID:int}/{approvalProcessID:int}")]
        public async Task<IActionResult> Get(int employeeLeaveAID, int approvalProcessID = 0)
        {
            var application = await Manager.GetLeaveApplication(employeeLeaveAID, approvalProcessID);

            return OkResult(application);
        }
        [HttpGet("GetPendingLeaveApplicationForDashboard/{employeeLeaveAID:int}/{approvalProcessID:int}")]
        public async Task<IActionResult> GetPendingLeaveApplicationForDashboard(int employeeLeaveAID, int approvalProcessID = 0)
        {
            var application = await Manager.GetLeaveApplicationForAdmin(employeeLeaveAID, approvalProcessID);

            return OkResult(application);
        }
        [HttpGet("Remove/{EmployeeLeaveAID:int}")]
        public async Task<IActionResult> Remove(int employeeLeaveAID)
        {
            var response = await Manager.RemovLeaveApplicationAsync(employeeLeaveAID);
            await _notificationHub.Clients.All.ReceiveNotificationUserWise("LeaveApplication", response.EmployeeIds, response.NotificationMessage, response.CurrentNotificaitonEmplyeeID);
            return OkResult(employeeLeaveAID);
        }
        [HttpGet("GetLeaveApplicationDetails/{EmployeeLeaveAID:int}/{approvalProcessID:int}")]
        public async Task<IActionResult> GetLeaveApplicationDetails(int employeeLeaveAID, int approvalProcessID)
        {
            var application = await Manager.GetLeaveApplicationWithCommentsForApproval(employeeLeaveAID, approvalProcessID);

            return OkResult(application);
        }
        [HttpGet("GetLeaveApplicationDetailsForHR/{EmployeeLeaveAID:int}/{approvalProcessID:int}")]
        public async Task<IActionResult> GetLeaveApplicationDetailsForHR(int employeeLeaveAID, int approvalProcessID)
        {
            var application = await Manager.GetLeaveApplicationWithCommentsForApprovalForHR(employeeLeaveAID, approvalProcessID);

            return OkResult(application);
        }

        [HttpPost("ApplyLFA")]
        public IActionResult ApplyLFA([FromBody] LFADeclarationDto application)
        {
            Manager.SaveLFA(application);
            return OkResult(application);
        }

        [HttpPost("GetAllLeaveApplicationList")]
        public async Task<IActionResult> GetAllLeaveApplicationList([FromBody] GridParameter parameters)
        {
            var model = Manager.GetAllLeaveApplicationList(parameters);
            return OkResult(new { parentDataSource = model });
        }
        [HttpPost("GetAllPendingLeaveApplicationList")]
        public async Task<IActionResult> GetAllPendingLeaveApplicationList([FromBody] GridParameter parameters)
        {
            var model = Manager.GetAllPendingLeaveApplicationList(parameters);
            return OkResult(new { parentDataSource = model });
        }

        [HttpPost("GetAllLeaveApplicationListForHR")]
        public async Task<IActionResult> GetAllLeaveApplicationListForHR([FromBody] GridParameter parameters)
        {
            var model = Manager.GetAllLeaveApplicationListForHR(parameters, "ALL");
            return OkResult(new { parentDataSource = model });
        }

        [HttpPost("GetAllLeaveApplicationListForHROnbehalfOfEmployee")]
        public async Task<IActionResult> GetAllLeaveApplicationListForHROnbehalfOfEmployee([FromBody] GridParameter parameters)
        {
            var model = Manager.GetAllLeaveApplicationListForHROnbehalfOfEmployee(parameters, "ALL");
            return OkResult(new { parentDataSource = model });
        }

        [HttpPost("GetAllLeaveApplicationListForDashboard")]
        public async Task<IActionResult> GetAllLeaveApplicationListForDashboard([FromBody] GridParameter parameters)
        {
            var model = Manager.GetAllLeaveApplicationListForDashboard(parameters);
            return OkResult(new { parentDataSource = model });
        }

        [HttpPost("GetAllLeaveApplicationListForLeaveToday")]
        public async Task<IActionResult> GetAllLeaveApplicationListForLeaveToday([FromBody] GridParameter parameters)
        {
            var model = Manager.GetAllLeaveApplicationListForHR(parameters, "TotalLeaveToday");
            return OkResult(new { parentDataSource = model });
        }

        [HttpGet("GetLeaveCategoriesWithSettingsForPreview/{EmployeeLeaveAID:int}")]
        public async Task<IActionResult> GetLeaveCategoriesWithSettingsForPreview(int EmployeeLeaveAID)
        {
            var categories = await Manager.GetLeaveCategoriesWithSettings(EmployeeLeaveAID);

            return OkResult(categories);
        }
        [HttpGet("GetLeaveCategoriesWithSettings")]
        public async Task<IActionResult> GetLeaveCategoriesWithSettings()
        {
            var categories = await Manager.GetLeaveCategoriesWithSettings();

            return OkResult(categories);
        }

        [HttpGet("GetLeaveDetailsForHr/{EmployeeID:int}")]
        public IActionResult GetLeaveDetailsForHr(int EmployeeID)
        {

            var model = Manager.GetLeaveDetailsForHr(EmployeeID);
            return OkResult(model.Result);
        }

        #endregion

        #region Leave Policy Settings
        [HttpGet("GetLeavePolicySettingsByLeaveCategoryId/{leaveCategoryID:int}")]
        public async Task<IActionResult> GetLeavePolicySettingsByLeaveCategoryId(int leaveCategoryID)
        {
            var leavePolicySettings = await Manager.GetLeavePolicySettings(leaveCategoryID);
            return OkResult(leavePolicySettings);
        }
        [HttpPost("SaveLeavePolicySettings")]
        public async Task<IActionResult> SaveLeavePolicySettings([FromBody] LeavePolicySettingsDto policySettings)
        {
            var response = await Manager.SavePolicySettings(policySettings);
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }
        [HttpGet("CheckMultipleSupervisor/{EmployeeID:int}")]
        public async Task<IActionResult> CheckMultipleSupervisor(int EmployeeID)
        {
            var isMulti = Manager.CheckMultipleSupervisor(EmployeeID);
            return OkResult(isMulti);
        }

        [HttpPost("CancelLeave")]
        public async Task<IActionResult> CancelLeave([FromBody] LeaveApplication application)
        {
            var response = await Manager.CancelLeaveApplication(application);
            await _notificationHub.Clients.All.ReceiveNotificationUserWise("LeaveApplication",response.EmployeeIds,response.NotificationMessage,response.CurrentNotificaitonEmplyeeID);
            return OkResult(new { status = response.Success, message = response.Message});
        }

        [HttpGet("GetHolidaysWorkDetails")]
        public IActionResult GetHolidaysWorkDetails()
        {
            var workingDetails = Manager.GetHolidaysWorkDetails().Result;
            return OkResult(workingDetails);
        }

        [HttpGet("GetHolidaysWorkDetailsAll")]
        public IActionResult GetHolidaysWorkDetailsAll()
        {
            var workingDetails = Manager.GetHolidaysWorkDetailsAll().Result;
            return OkResult(workingDetails);
        }

        [HttpGet("GetHolidaysWorkDetailsById/{EmployeeID:int}")]
        public IActionResult GetHolidaysWorkDetailsById(int EmployeeID)
        {
            var workingDetails = Manager.GetHolidaysWorkDetailsById(EmployeeID).Result;
            return OkResult(workingDetails);
        }
        [HttpGet("GetHolidaysWorkDetailsAllById/{EmployeeID:int}")]
        public IActionResult GetHolidaysWorkDetailsAllById(int EmployeeID)
        {
            var workingDetails = Manager.GetHolidaysWorkDetailsAllById(EmployeeID).Result;
            return OkResult(workingDetails);
        }


        [HttpGet("GetUnauthorizedLeave/{StartDate}/{EndDate}/{DivisionID:int}/{DepartmentID:int}")]
        public IActionResult GetUnauthorizedLeave(string StartDate, string EndDate, int DivisionID, int DepartmentID)
        {
            try
            {
                DateTime fromDate = DateTime.ParseExact(StartDate, "yyyy-MM-dd", null);
                DateTime toDate = DateTime.ParseExact(EndDate, "yyyy-MM-dd", null);

                var model = Manager.GetUnauthorizedLeave(fromDate, toDate, DivisionID, DepartmentID);
                return OkResult(model.Result);
            }
            catch (Exception ex)
            {
                return BadRequest("Invalid date format or values");
            }
        }

        [HttpGet("GetUnauthorizedLeaveHr/{EmployeeID:int}")]
        public IActionResult GetUnauthorizedLeaveHr(int EmployeeID)
        {
            try
            {
                var model = Manager.GetUnauthorizedLeaveHr(EmployeeID);
                return OkResult(model.Result);
            }
            catch (Exception ex)
            {
                return BadRequest("Invalid Request");
            }
        }

        [HttpGet("GetUnauthorizedLeaveViewHr/{EmployeeLeaveAID:int}")]
        public IActionResult GetUnauthorizedLeaveViewHr(int EmployeeLeaveAID)
        {
            try
            {
                var model = Manager.GetUnauthorizedLeaveViewHr(EmployeeLeaveAID);
                return OkResult(model.Result);
            }
            catch (Exception ex)
            {
                return BadRequest("Invalid Request");
            }
        }

        [HttpPost("CreateUnauthorizedLeaveEmailNotification")]
        public IActionResult CreateUnauthorizedLeaveEmailNotification([FromBody] List<UnauthorizedLeaveEmailNotificationDto> unauthorizedLeavs)
        {
            var response = Manager.SaveEmailNotification(unauthorizedLeavs).Result;
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        [HttpGet("GetLeaveApplicationHr/{employeeLeaveAID:int}/{employeeID:int}")]
        public async Task<IActionResult> GetLeaveApplicationHr(int employeeLeaveAID, int employeeID)
        {
            var application = await Manager.GetLeaveApplicationHr(employeeLeaveAID, employeeID);

            return OkResult(application);
        }





        #endregion


    }
}
