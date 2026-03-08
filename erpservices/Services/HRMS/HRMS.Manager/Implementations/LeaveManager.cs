using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Manager.Core;
using Manager.Core.Mapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Util;
using static System.Net.Mime.MediaTypeNames;

namespace HRMS.Manager.Implementations
{
    public class LeaveManager : ManagerBase, ILeaveManager
    {
        private readonly IRepository<EmployeeLeaveApplication> EmployeeLeaveApplicationRepo;
        private readonly IRepository<EmployeeLeaveApplicationDayBreakDown> EmployeeLeaveApplicationDayBreakDownRepo;
        private readonly IRepository<Employment> EmploymentRepo;
        private readonly IRepository<ShiftingMaster> ShiftingMasterRepo;
        private readonly IRepository<ShiftingChild> ShiftingChildRepo;
        private readonly IRepository<Holiday> HolidayRepo;
        private readonly IRepository<LFADeclaration> LFADeclarationRepo;
        private readonly IRepository<Employee> EmployeeRepo;
        private readonly IRepository<LeavePolicySettings> LeavePolicySettingsRepo;
        private readonly IRepository<EmployeeSupervisorMap> EmployeeSupervisorMapRepo;
        private readonly IScheduleManager ScheduleManager;
        private readonly IRepository<EmailNotification> EmailNotificationRepo;
        private readonly IRepository<UnauthorizedLeaveEmailDate> UnauthorizedLeaveEmailDateRepo;
        private readonly IRepository<EmployeeLeaveAccount> EmployeeLeaveAccountRepo;
        private readonly IComboManager comboManager;


        public LeaveManager(IRepository<EmployeeLeaveApplication> employeeLeaveApplicationRepo, IRepository<EmployeeLeaveApplicationDayBreakDown> employeeLeaveApplicationDayBreakDownRepo, IRepository<Employment> employmentRepo, IRepository<ShiftingMaster> shiftingMasterRepo, IRepository<ShiftingChild> shiftingChildRepo, IRepository<Holiday> holidayRepo, IRepository<LFADeclaration> lfaRepo, IRepository<Employee> employeeRepo, IRepository<LeavePolicySettings> leavePolicySettingsRepo, IRepository<EmployeeSupervisorMap> employeeSupervisorMapRepo, IScheduleManager scheduleManager, IRepository<EmailNotification> emailNotificationRepo, IRepository<UnauthorizedLeaveEmailDate> unauthorizedLeaveEmailDateRepo, IRepository<EmployeeLeaveAccount> employeeLeaveAccountRepo, IComboManager _ComboManager)
        {
            EmployeeLeaveApplicationRepo = employeeLeaveApplicationRepo;
            EmployeeLeaveApplicationDayBreakDownRepo = employeeLeaveApplicationDayBreakDownRepo;
            EmploymentRepo = employmentRepo;
            ShiftingMasterRepo = shiftingMasterRepo;
            ShiftingChildRepo = shiftingChildRepo;
            HolidayRepo = holidayRepo;
            LFADeclarationRepo = lfaRepo;
            EmployeeRepo = employeeRepo;
            LeavePolicySettingsRepo = leavePolicySettingsRepo;
            EmployeeSupervisorMapRepo = employeeSupervisorMapRepo;
            ScheduleManager = scheduleManager;
            EmailNotificationRepo = emailNotificationRepo;
            UnauthorizedLeaveEmailDateRepo = unauthorizedLeaveEmailDateRepo;
            EmployeeLeaveAccountRepo = employeeLeaveAccountRepo;
            comboManager = _ComboManager;
        }
        public async Task<LeaveBalanceAndDetailsResponse> GetLeaveBalanceAndDetails(int leaveTypeId, DateTime startDate, DateTime endDate, int employeeLeaveAID)
        {
            LeaveBalanceAndDetailsResponse response = new LeaveBalanceAndDetailsResponse
            {
                LeaveDetails = LeaveDetailsBreakDown(leaveTypeId, startDate, endDate, employeeLeaveAID)
            };
            var employee = EmployeeRepo.Entities.SingleOrDefault(x => x.EmployeeID == AppContexts.User.EmployeeID);
            response.LeaveBalances = LeaveBalance(leaveTypeId, (decimal)(response.LeaveDetails.Count(x => x.DayStatus == Util.Fullday) * 1 + (response.LeaveDetails.Count(x => x.DayStatus == Util.Halfday || x.DayStatus == Util.OnlyHalfday || x.DayStatus == Util.FirstHalfday || x.DayStatus == Util.SecondHalfday) * 0.5)), employeeLeaveAID);
            response.TotalEmployementDays = (DateTime.Now - (employee.DateOfJoining ?? DateTime.Now)).Days;
            response.JoiningDate = employee.DateOfJoining ?? DateTime.Now;
            return await Task.FromResult(response);
        }
        public async Task<LeaveBalanceAndDetailsResponse> GetLeaveBalanceAndDetailsHr(int leaveTypeId, DateTime startDate, DateTime endDate, int employeeLeaveAID, int employeeID)
        {
            LeaveBalanceAndDetailsResponse response = new LeaveBalanceAndDetailsResponse
            {
                LeaveDetails = LeaveDetailsBreakDownHr(leaveTypeId, startDate, endDate, employeeLeaveAID, employeeID)
            };
            var employee = EmployeeRepo.Entities.SingleOrDefault(x => x.EmployeeID == employeeID);
            response.LeaveBalances = LeaveBalanceHr(leaveTypeId, (decimal)(response.LeaveDetails.Count(x => x.DayStatus == Util.Fullday) * 1 + (response.LeaveDetails.Count(x => x.DayStatus == Util.Halfday || x.DayStatus == Util.OnlyHalfday || x.DayStatus == Util.FirstHalfday || x.DayStatus == Util.SecondHalfday) * 0.5)), employeeLeaveAID, employeeID);
            //response.TotalEmployementDays = (DateTime.Now - (employee.DateOfJoining ?? DateTime.Now)).Days;
            response.TotalEmployementDays = (employee != null && employee.DateOfJoining != null) ? (DateTime.Now - employee.DateOfJoining.Value).Days : 0;
            response.JoiningDate = employee?.DateOfJoining ?? DateTime.Now;
            return await Task.FromResult(response);
        }
        private List<LeaveDetails> LeaveDetailsBreakDown(int leaveTypeId, DateTime startDate, DateTime endDate, int employeeLeaveAID)
        {

            var leaveSettings = GetLeavePolicySettings(leaveTypeId).Result;
            var leaveDetails = new List<LeaveDetails>();

            var prevLeaveDays = (from ela in EmployeeLeaveApplicationRepo.GetAllList()
                                 join elabd in EmployeeLeaveApplicationDayBreakDownRepo.GetAllList() on ela.EmployeeLeaveAID equals elabd.EmployeeLeaveAID
                                 where ela.EmployeeID == AppContexts.User.EmployeeID && ((elabd.RequestDate >= startDate && elabd.RequestDate <= endDate) && ela.ApprovalStatusID != (int)ApprovalStatus.Rejected)
                                 && (employeeLeaveAID == 0 || ela.EmployeeLeaveAID != employeeLeaveAID) && elabd.IsCancelled == false
                                 select new { elabd.RequestDate, elabd.HalfOrFullDay }).ToList();

            if (employeeLeaveAID > 0)
            {
                var leaveMaster = EmployeeLeaveApplicationRepo.Entities.Where(x => x.EmployeeLeaveAID == employeeLeaveAID).SingleOrDefault();
                if (leaveMaster.IsNotNull() && leaveMaster.RequestStartDate == startDate && leaveMaster.RequestEndDate == endDate)
                {
                    leaveDetails = EmployeeLeaveApplicationDayBreakDownRepo.Entities.Where(x => x.EmployeeLeaveAID == employeeLeaveAID).Select(y => new LeaveDetails
                    {
                        ELADBDID = y.ELADBDID,
                        Day = y.RequestDate.ToString("dd MMM yyyy"),
                        DayStatus = y.IsCancelled ? "Cancelled" : y.HalfOrFullDay
                    }).ToList();
                    leaveDetails.ForEach(x =>
                    {
                        x.DayStatus = prevLeaveDays.Any(z => z.RequestDate == x.DayDateTime && z.HalfOrFullDay == Util.Halfday) ? Util.OnlyHalfday : x.DayStatus;
                    });

                    return leaveDetails;
                }
                //New Added 09April2025
                else
                {
                    var weekend1 = (from emp in EmploymentRepo.GetAllList()
                                    join sm in ShiftingMasterRepo.GetAllList() on emp.ShiftID equals sm.ShiftingMasterID
                                    join sc in ShiftingChildRepo.GetAllList() on sm.ShiftingMasterID equals sc.ShiftingMasterID
                                    where emp.EmployeeID == AppContexts.User.EmployeeID && sc.IsWorkingDay == false
                                    select sc.Day).ToList();

                    var holiday1 = HolidayRepo.Entities.Where(x => x.HolidayDate >= startDate && x.HolidayDate <= endDate).Select(y => new { y.HolidayDate }).ToList();
                    for (var day = startDate; day <= endDate; day = day.AddDays(1))
                    {
                        var leaveDetail = new LeaveDetails
                        {
                            Day = day.ToString("dd MMM yyyy"),
                            DayStatus = prevLeaveDays.Any(x => x.RequestDate == day && x.HalfOrFullDay == Util.Fullday) || prevLeaveDays.Count(x => x.RequestDate == day && x.HalfOrFullDay == Util.Halfday) == 2 ? Util.Conflict : prevLeaveDays.Any(x => x.RequestDate == day && x.HalfOrFullDay == Util.Halfday) ? Util.OnlyHalfday : !leaveSettings.IsHolidayInclusive && (weekend1.Any(x => x == (int)day.DayOfWeek) || holiday1.Any(x => x.HolidayDate == day)) ? Util.NonWorkingDay : Util.Fullday
                        };
                        leaveDetails.Add(leaveDetail);
                    }
                    return leaveDetails;
                }
            }

            var weekend = (from emp in EmploymentRepo.GetAllList()
                           join sm in ShiftingMasterRepo.GetAllList() on emp.ShiftID equals sm.ShiftingMasterID
                           join sc in ShiftingChildRepo.GetAllList() on sm.ShiftingMasterID equals sc.ShiftingMasterID
                           where emp.EmployeeID == AppContexts.User.EmployeeID && sc.IsWorkingDay == false && emp.IsCurrent == true
                           select sc.Day).ToList();

            var holiday = HolidayRepo.Entities.Where(x => x.HolidayDate >= startDate && x.HolidayDate <= endDate).Select(y => new { y.HolidayDate }).ToList();

            for (var day = startDate; day <= endDate; day = day.AddDays(1))
            {
                var leaveDetail = new LeaveDetails
                {
                    Day = day.ToString("dd MMM yyyy"),
                    DayStatus = prevLeaveDays.Any(x => x.RequestDate == day && x.HalfOrFullDay == Util.Fullday) || prevLeaveDays.Count(x => x.RequestDate == day && x.HalfOrFullDay == Util.Halfday) == 2 ? Util.Conflict : prevLeaveDays.Any(x => x.RequestDate == day && x.HalfOrFullDay == Util.Halfday) ? Util.OnlyHalfday : !leaveSettings.IsHolidayInclusive && (weekend.Any(x => x == (int)day.DayOfWeek) || holiday.Any(x => x.HolidayDate == day)) ? Util.NonWorkingDay : Util.Fullday
                };
                //DayStatus =
                //        prevLeaveDays.Any(x => x.RequestDate == day && x.HalfOrFullDay == Util.Fullday) ||
                //        prevLeaveDays.Count(x => x.RequestDate == day && x.HalfOrFullDay == Util.FirstHalfday) == 3 ||
                //        prevLeaveDays.Count(x => x.RequestDate == day && x.HalfOrFullDay == Util.SecondHalfday) == 4
                //        ? Util.Conflict
                //        : (prevLeaveDays.Any(x => x.RequestDate == day && x.HalfOrFullDay == Util.Halfday)
                //            ? Util.OnlyHalfday
                //            : (!leaveSettings.IsHolidayInclusive &&
                //               (weekend.Contains((int)day.DayOfWeek) || holiday.Any(x => x.HolidayDate == day))
                //                 ? Util.NonWorkingDay
                //                 : Util.Fullday))
                //                    };
                leaveDetails.Add(leaveDetail);
            }
            return leaveDetails;
        }
        private List<LeaveBalance> LeaveBalance(int leaveTypeId, decimal numberOfLeaveDays, int employeeLeaveAID, int employeeId = 0)
        {
            int employee = employeeId == 0 ? AppContexts.User.EmployeeID.Value : employeeId;

            string sql = employeeLeaveAID > 0 ?
             $@"EXEC spEmployeeLeaveBalanceByEmployeeIDAndLeaveAID {employee},{employeeLeaveAID}" : $@"SELECT * FROM EmployeeLeaveBalance WHERE EmployeeID={employee} ORDER BY LeaveCategoryID ASC";

            var balance = EmployeeLeaveApplicationRepo.GetDataModelCollection<LeaveBalance>(sql);
            if (leaveTypeId > 0)
            {
                balance.Where(x => x.LeaveCategoryID == leaveTypeId || x.LeaveCategoryID == 0).ToList().ForEach(y => { y.Applying = numberOfLeaveDays; y.Balance = y.Balance - numberOfLeaveDays; });
            }
            return balance;
        }
        private List<LeaveBalance> LeaveBalanceHr(int leaveTypeId, decimal numberOfLeaveDays, int employeeLeaveAID, int employeeId = 0)
        {
            //int employee = employeeId == 0 ? AppContexts.User.EmployeeID.Value : employeeId;

            string sql = employeeLeaveAID > 0 ?
             $@"EXEC spEmployeeLeaveBalanceByEmployeeIDAndLeaveAID {employeeId},{employeeLeaveAID}" : $@"SELECT * FROM EmployeeLeaveBalance WHERE EmployeeID={employeeId} ORDER BY LeaveCategoryID ASC";

            var balance = EmployeeLeaveApplicationRepo.GetDataModelCollection<LeaveBalance>(sql);
            if (leaveTypeId > 0)
            {
                balance.Where(x => x.LeaveCategoryID == leaveTypeId || x.LeaveCategoryID == 0).ToList().ForEach(y => { y.Applying = numberOfLeaveDays; y.Balance = y.Balance - numberOfLeaveDays; });
            }
            return balance;
        }
        public async Task<NotificationResponseDto> SaveChanges(LeaveApplication application)
        {
            NotificationResponseDto responseDto = new NotificationResponseDto
            {
                Success = false,
                EmployeeIds = string.Empty,
                CurrentNotificaitonEmplyeeID = string.Empty,
                NotificationMessage = string.Empty

            };
            var returnList = comboManager.GetActiveBackUpEmployeeList().Result;
            bool hasBackupEmployee = returnList.Where(x => x.value == application.BackupEmployeeID).Any();
            if (!hasBackupEmployee && application.FormType != "hr")
            {

                responseDto.Message = "This backup employee is not associated with you.";
                return responseDto;
            }
            if (application.FormType != "hr" && (application.BackupEmployeeID.IsNull() || application.BackupEmployeeID == 0 || string.IsNullOrWhiteSpace(application.LeaveLocation) || string.IsNullOrWhiteSpace(application.Purpose)))
            {
                responseDto.Message = "You must fill the leave location, purpose and backup employee.";
                return responseDto;
            }
            var ApprovalProcessID = "0";
            string EmployeeIds = string.Empty;
            string notificationMessage = string.Empty;
            string currentNotificationEmployeeId = string.Empty;
            bool IsResubmitted = false;
            string sql = $@"SELECT FinancialYearID FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FinancialYear where Year={application.RequestStartDateTime.Year}";
            var financialYear = EmployeeLeaveApplicationRepo.GetData(sql);
            var existingApplication = EmployeeLeaveApplicationRepo.Entities.Where(x => x.EmployeeLeaveAID == application.EmployeeLeaveAID).SingleOrDefault();


            if (application.LeaveCategoryID != (int)Util.LeaveCategory.Compensatory)
            {
                var leaveBalanceCheck = EmployeeLeaveAccountRepo.Entities.Where(x => x.EmployeeID == (application.IsHrApplied == true ? application.EmployeeID : (int)AppContexts.User.EmployeeID) && x.LeaveCategoryID == application.LeaveCategoryID && x.FinancialYearID == (int)financialYear["FinancialYearID"]).SingleOrDefault();

                decimal appliedLeave = (decimal)(application.LeaveDetails.Count(x => x.DayStatus == Util.Fullday) * 1 + application.LeaveDetails.Count(x => x.DayStatus == Util.Halfday || x.DayStatus == Util.OnlyHalfday || x.DayStatus == Util.FirstHalfday || x.DayStatus == Util.SecondHalfday) * 0.5);

                if (((application.EmployeeLeaveAID > 0 ? existingApplication.NoOfLeaveDays : 0) + leaveBalanceCheck.RemainingDays - appliedLeave) < 0)
                {
                    responseDto.Message = "leave balance exceeds/expired hence you can't apply.";
                    return responseDto;
                }
                if (application.EmployeeLeaveAID > 0 && existingApplication.LeaveCategoryID != application.LeaveCategoryID)
                {
                    if ((existingApplication.LeaveCategoryID == 248 && application.LeaveCategoryID != 248) || (existingApplication.LeaveCategoryID != 248 && application.LeaveCategoryID == 248))
                    {
                        responseDto.Message = "Leave type cannot be changed from Birthday to Others.";
                        return responseDto;
                    }

                    if (leaveBalanceCheck.RemainingDays - appliedLeave < 0)
                    {
                        responseDto.Message = "leave balance exceeds/expired hence you can't apply.";
                        return responseDto;
                    }
                }
            }


            if (application.EmployeeLeaveAID > 0 && (existingApplication.IsNullOrDbNull() || existingApplication.CreatedBy != AppContexts.User.UserID))
            {
                responseDto.Message = "You don't have permission to save this Leave Application.";
                return responseDto;
            }


            var approvalProcessFeedBack = new Dictionary<string, object>();
            if ((application.EmployeeLeaveAID > 0) && application.FormType != "hr")
                approvalProcessFeedBack = GetApprovalProcessAndFeedbackByEmployee(application.EmployeeLeaveAID, (int)Util.ApprovalType.LeaveApplication);

            foreach (var item in application.Attachments)
            {
                string ext = "";
                bool fileValid = false;
                string fileValidError = "";
                if (item.FUID > 0)
                {
                    ext = item.Type.Remove(0, 1);
                }
                else
                {
                    string result = item.AttachedFile.Split(',')[1];

                    var bytes = System.Convert.FromBase64String(result);
                    fileValid = UploadUtil.IsFileValidForDocument(bytes, item.AttachedFile);


                    string err = CheckValidFileExtensionsForAttachment(ext, item.OriginalName);
                    if (fileValid == false)
                    {
                        fileValidError = "Uploaded file extension is not allowed.";
                    }
                    if (!fileValidError.IsNullOrEmpty())
                    {
                        err = fileValidError;
                    }
                    if (!err.IsNullOrEmpty())
                    {
                        responseDto.Message = err;
                        return responseDto;
                    }
                }
            }

            var removeList = RemoveAttachments(application, application.EmployeeLeaveAID);

            var applicationMaster = new EmployeeLeaveApplication
            {
                EmployeeLeaveAID = application.EmployeeLeaveAID,
                EmployeeID = application.FormType == "hr" ? application.EmployeeID : (int)AppContexts.User.EmployeeID,
                ExternalID = application.FormType == "hr" ? "hr" : "",
                ApplicationDate = DateTime.Today,
                RequestStartDate = application.RequestStartDateTime,
                RequestEndDate = application.RequestEndDateDateTime,
                //NoOfLeaveDays = (decimal)(application.LeaveDetails.Count(x => x.DayStatus == Util.Fullday) * 1 + application.LeaveDetails.Count(x => x.DayStatus == Util.Halfday || x.DayStatus == Util.OnlyHalfday) * 0.5),
                NoOfLeaveDays = (decimal)(application.LeaveDetails.Count(x => x.DayStatus == Util.Fullday) * 1 + application.LeaveDetails.Count(x => x.DayStatus == Util.Halfday || x.DayStatus == Util.OnlyHalfday || x.DayStatus == Util.FirstHalfday || x.DayStatus == Util.SecondHalfday) * 0.5),
                BackupEmployeeID = application.BackupEmployeeID,
                ApprovalStatusID = application.FormType == "hr" ? (int)ApprovalStatus.Approved : (int)ApprovalStatus.Pending,
                LeaveCategoryID = application.LeaveCategoryID,
                FinancialYearID = financialYear.Count() > 0 ? (int)financialYear["FinancialYearID"] : 0,
                PeriodID = 0,
                Purpose = application.Purpose,
                LeaveLocation = application.LeaveLocation,
                IsMultiple = false,
                DateOfJoiningWork = application.DateOfJoiningWork
            };
            using (var unitOfWork = new UnitOfWork())
            {
                var leaveBreakDown = new List<EmployeeLeaveApplicationDayBreakDown>();

                if (application.EmployeeLeaveAID.IsZero() && existingApplication.IsNull())
                {
                    applicationMaster.SetAdded();
                    SetApplicationMasterNewId(applicationMaster);
                }
                else
                {
                    applicationMaster.CreatedBy = existingApplication.CreatedBy;
                    applicationMaster.CreatedDate = existingApplication.CreatedDate;
                    applicationMaster.CreatedIP = existingApplication.CreatedIP;
                    applicationMaster.RowVersion = existingApplication.RowVersion;
                    applicationMaster.SetModified();

                    leaveBreakDown = EmployeeLeaveApplicationDayBreakDownRepo.Entities.Where(x => x.EmployeeLeaveAID == application.EmployeeLeaveAID).ToList();
                    leaveBreakDown.ForEach(y => y.SetDeleted());
                }


                string errorMessage = null;
                foreach (var x in application.LeaveDetails)
                {
                    var leaveApplied = (from breakdown in EmployeeLeaveApplicationDayBreakDownRepo.Entities
                                        join appl in EmployeeLeaveApplicationRepo.Entities
                                        on breakdown.EmployeeLeaveAID equals appl.EmployeeLeaveAID
                                        where breakdown.RequestDate.Date == x.DayDateTime.Date && breakdown.IsCancelled == false && appl.EmployeeID == (application.FormType == "hr" ? application.EmployeeID : (int)AppContexts.User.EmployeeID)
                                        select new
                                        {
                                            Breakdown = breakdown,  // All fields from EmployeeLeaveApplicationDayBreakDownRepo
                                            //ApplicationField1 = application.SomeField1,  // Specific fields from EmployeeLeaveApplicationRepo
                                            //ApplicationField2 = application.SomeField2
                                        }).ToList();



                    //var leaveApplied = EmployeeLeaveApplicationDayBreakDownRepo.Entities
                    //    .Where(a => a.RequestDate == x.DayDateTime && a.IsCancelled == false)
                    //    .ToList(); // Get all matching records

                    // Get existing leave for the current application being edited
                    var existingLeave = application.EmployeeLeaveAID > 0 ?
                        EmployeeLeaveApplicationDayBreakDownRepo.Entities
                            .Where(y => y.EmployeeLeaveAID == application.EmployeeLeaveAID &&
                                       y.RequestDate.Date == x.DayDateTime.Date &&
                                       y.IsCancelled == false)
                            .FirstOrDefault() : null;
                    // Get total half days already applied
                    var existingHalfDays = leaveApplied.Where(l => l.Breakdown.NoOfLeaveDays == 0.5m).ToList();

                    if (leaveApplied.Any() && application.EmployeeLeaveAID == 0)
                    //if (leaveApplied.Any())
                    {
                        // Check if there's already a full day leave
                        if (leaveApplied.Any(l => l.Breakdown.NoOfLeaveDays == 1m))
                        {
                            errorMessage = $"Full day leave already applied for {x.DayDateTime.ToString("dd MMM yyyy")}";
                            break;
                        }


                        // Check if total half days would exceed 1 day
                        if (existingHalfDays.Count >= 2)
                        {
                            errorMessage = $"Maximum allowed half days already applied for {x.DayDateTime.ToString("dd MMM yyyy")}";
                            break;
                        }
                        if (existingHalfDays.Any())
                        {
                            if (x.DayStatus == Util.Fullday)
                            {
                                errorMessage = $"Cannot apply full day leave as half day already exists for {x.DayDateTime.ToString("dd MMM yyyy")}";
                                break;
                            }

                            // Check if trying to apply same half of day
                            var conflictingHalf = existingHalfDays.Any(l =>
                                (l.Breakdown.HalfOrFullDay == Util.FirstHalfday && x.DayStatus == Util.FirstHalfday) ||
                                (l.Breakdown.HalfOrFullDay == Util.SecondHalfday && x.DayStatus == Util.SecondHalfday));

                            if (conflictingHalf)
                            {
                                errorMessage = $"Half day leave already applied for {x.DayDateTime.ToString("dd MMM yyyy")} ({existingHalfDays[0].Breakdown.HalfOrFullDay})";
                                break;
                            }


                        }

                        // If we get here, we can add the new half day
                        if (x.DayStatus != Util.Fullday)
                        {
                            leaveBreakDown.Add(new EmployeeLeaveApplicationDayBreakDown
                            {
                                EmployeeLeaveAID = applicationMaster.EmployeeLeaveAID,
                                RequestDate = x.DayDateTime,
                                NoOfLeaveDays = 0.5m,
                                HalfOrFullDay = x.DayStatus
                            });
                        }
                    }

                    // Validation for editing existing applications
                    else if (application.EmployeeLeaveAID > 0 && existingLeave != null)
                    {
                        // Validate full day to half day changes
                        if (existingLeave.HalfOrFullDay == Util.Fullday)
                        {
                            //if (x.DayStatus != Util.FirstHalfday && x.DayStatus != Util.SecondHalfday && application.EmployeeLeaveAID != existingLeave.EmployeeLeaveAID)
                            //{
                            //    errorMessage = $"Full day leave can only be changed to First Half or Second Half for {x.DayDateTime.ToString("dd MMM yyyy")}";
                            //    break;
                            //}
                            if (existingHalfDays.Any())
                            {
                                // Check if trying to apply same half of day
                                var conflictingHalf = existingHalfDays.Any(l =>
                                    (l.Breakdown.HalfOrFullDay == Util.FirstHalfday && x.DayStatus == Util.FirstHalfday) ||
                                    (l.Breakdown.HalfOrFullDay == Util.SecondHalfday && x.DayStatus == Util.SecondHalfday));

                                if (conflictingHalf)
                                {
                                    errorMessage = $"Half day leave already applied for {x.DayDateTime.ToString("dd MMM yyyy")} ({existingHalfDays[0].Breakdown.HalfOrFullDay})";
                                    break;
                                }


                            }
                        }
                        // Validate first half day changes
                        else if (existingLeave.HalfOrFullDay == Util.FirstHalfday)
                        {
                            //if (x.DayStatus != Util.SecondHalfday && x.DayStatus != Util.Fullday && application.EmployeeLeaveAID != existingLeave.EmployeeLeaveAID)
                            //{
                            //    errorMessage = $"First Half leave can only be changed to Second Half or Full day for {x.DayDateTime.ToString("dd MMM yyyy")}";
                            //    break;
                            //}

                            // Check if full day is possible (no other half days exist)
                            if (x.DayStatus == Util.Fullday && leaveApplied.Any(l => l.Breakdown.NoOfLeaveDays == 0.5m && l.Breakdown.EmployeeLeaveAID != application.EmployeeLeaveAID))
                            {
                                errorMessage = $"Cannot change to full day as other half day leave exists for {x.DayDateTime.ToString("dd MMM yyyy")}";
                                break;
                            }

                            // Check if trying to apply same half of day
                            var conflictingHalf = existingHalfDays.Any(l =>
                                (l.Breakdown.HalfOrFullDay == Util.SecondHalfday && x.DayStatus == Util.SecondHalfday && l.Breakdown.EmployeeLeaveAID != application.EmployeeLeaveAID));

                            if (conflictingHalf)
                            {
                                errorMessage = $"Half day leave already applied for {x.DayDateTime.ToString("dd MMM yyyy")} ({existingHalfDays[0].Breakdown.HalfOrFullDay})";
                                break;
                            }
                        }
                        // Validate second half day changes
                        else if (existingLeave.HalfOrFullDay == Util.SecondHalfday)
                        {
                            //if (x.DayStatus != Util.FirstHalfday && x.DayStatus != Util.Fullday && application.EmployeeLeaveAID != existingLeave.EmployeeLeaveAID)
                            //{
                            //    errorMessage = $"Second Half leave can only be changed to First Half or Full day for {x.DayDateTime.ToString("dd MMM yyyy")}";
                            //    break;
                            //}

                            // Check if full day is possible (no other half days exist)
                            if (x.DayStatus == Util.Fullday && leaveApplied.Any(l => l.Breakdown.NoOfLeaveDays == 0.5m && l.Breakdown.EmployeeLeaveAID != application.EmployeeLeaveAID))
                            {
                                errorMessage = $"Cannot change to full day as other half day leave exists for {x.DayDateTime.ToString("dd MMM yyyy")}";
                                break;
                            }
                            // Check if trying to apply same half of day
                            var conflictingHalf = existingHalfDays.Any(l =>
                                (l.Breakdown.HalfOrFullDay == Util.FirstHalfday && x.DayStatus == Util.FirstHalfday && l.Breakdown.EmployeeLeaveAID != application.EmployeeLeaveAID));

                            if (conflictingHalf)
                            {
                                errorMessage = $"Half day leave already applied for {x.DayDateTime.ToString("dd MMM yyyy")} ({existingHalfDays[0].Breakdown.HalfOrFullDay})";
                                break;
                            }
                        }

                        // If validation passes, update the leave breakdown
                        leaveBreakDown.Add(new EmployeeLeaveApplicationDayBreakDown
                        {
                            EmployeeLeaveAID = applicationMaster.EmployeeLeaveAID,
                            RequestDate = x.DayDateTime,
                            //NoOfLeaveDays = x.DayStatus == Util.Fullday ? 1m : 0.5m,
                            NoOfLeaveDays = x.DayStatus == Util.Fullday ? 1m :
                                          (x.DayStatus == Util.Halfday || x.DayStatus == Util.OnlyHalfday ||
                                           x.DayStatus == Util.FirstHalfday || x.DayStatus == Util.SecondHalfday) ? 0.5m : 0m,


                            HalfOrFullDay = x.DayStatus
                        });
                    }

                    //New 09April2025
                    else if (application.EmployeeLeaveAID > 0 && existingLeave == null)
                    {
                        // If validation passes, update the leave breakdown
                        leaveBreakDown.Add(new EmployeeLeaveApplicationDayBreakDown
                        {
                            EmployeeLeaveAID = applicationMaster.EmployeeLeaveAID,
                            RequestDate = x.DayDateTime,
                            NoOfLeaveDays = x.DayStatus == Util.Fullday ? 1m :
                                          (x.DayStatus == Util.Halfday || x.DayStatus == Util.OnlyHalfday ||
                                           x.DayStatus == Util.FirstHalfday || x.DayStatus == Util.SecondHalfday) ? 0.5m : 0m,
                            HalfOrFullDay = x.DayStatus
                        });
                    }


                    //New 09April2025


                    else if (application.EmployeeLeaveAID == 0)
                    {
                        // No existing leave - add new leave request
                        leaveBreakDown.Add(new EmployeeLeaveApplicationDayBreakDown
                        {
                            EmployeeLeaveAID = applicationMaster.EmployeeLeaveAID,
                            RequestDate = x.DayDateTime,
                            NoOfLeaveDays = x.DayStatus == Util.Fullday ? 1m :
                                          (x.DayStatus == Util.Halfday || x.DayStatus == Util.OnlyHalfday ||
                                           x.DayStatus == Util.FirstHalfday || x.DayStatus == Util.SecondHalfday) ? 0.5m : 0m,
                            HalfOrFullDay = x.DayStatus
                        });
                    }
                }

                if (errorMessage != null)
                {
                    responseDto.Message = errorMessage;
                    return responseDto;
                }
                leaveBreakDown.Where(x => x.IsDeleted == false).ToList().ForEach(x =>
                {
                    x.SetAdded();
                    SetpplicationBreakDownNewId(x);
                });

                application.EmployeeLeaveAID = applicationMaster.EmployeeLeaveAID;
                //LFA Disable start 11-July-2024
                //var lfaDetails = GenerateLFADeclaration(application);

                //if (lfaDetails.IsNotNull())
                //{
                //    SetAuditFields(lfaDetails);
                //    LFADeclarationRepo.Add(lfaDetails);
                //}
                //LFA Disable End 11-July-2024
                SetAuditFields(applicationMaster);
                SetAuditFields(leaveBreakDown);

                if (application.Attachments.IsNotNull() && application.Attachments.Count > 0)
                {

                    var attachmentList = AddAttachments(application.Attachments.Where(x => x.ID == 0).ToList());

                    //For Add new File
                    if (attachmentList.IsNotNull() && attachmentList.Count > 0)
                    {
                        foreach (var attachemnt in attachmentList)
                        {
                            SetAttachmentNewId(attachemnt);
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, applicationMaster.EmployeeLeaveAID, "EmployeeLeaveApplication", false, attachemnt.Size);
                        }
                    }
                    //For Remove Attachment                    
                    if (removeList.IsNotNull() && removeList.Count > 0)
                    {
                        foreach (var attachemnt in removeList)
                        {
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, applicationMaster.EmployeeLeaveAID, "EmployeeLeaveApplication", true, attachemnt.Size);
                        }
                    }
                }

                EmployeeLeaveApplicationRepo.Add(applicationMaster);
                EmployeeLeaveApplicationDayBreakDownRepo.AddRange(leaveBreakDown);

                if (application.FormType != "hr")
                {
                    notificationMessage =
                     @$"Leave Applied: {AppContexts.User.FullName} ({AppContexts.User.EmployeeCode}) - {applicationMaster.NoOfLeaveDays} day{(applicationMaster.NoOfLeaveDays != 1 ? "s" : "")} on {(applicationMaster.NoOfLeaveDays > 1 ?
                    $"{applicationMaster.RequestStartDate:dd-MMM-yyyy} to {applicationMaster.RequestEndDate:dd-MMM-yyyy}" :
                    $"{applicationMaster.RequestEndDate:dd-MMM-yyyy}")}";
                    string approvalTitle = @$"{Util.AutoLeaveAppTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}, Total Leave Request={applicationMaster.NoOfLeaveDays}, 
                                            {applicationMaster.RequestStartDate.ToString("dd-MM-yyyy")} to {applicationMaster.RequestEndDate.ToString("dd-MM-yyyy")}";
                    if (applicationMaster.IsAdded)
                    {

                        var result = await CreateApprovalProcessForLeaveApplicationNew(applicationMaster.EmployeeLeaveAID, Util.AutoLeaveAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, applicationMaster.LeaveCategoryID, application.IsLFA, application.IsFestival, applicationMaster.NoOfLeaveDays);
                        ApprovalProcessID = result.ApprovalProcessID;
                        EmployeeIds = result.EmployeeIDs;
                        IsResubmitted = false;
                    }
                    else
                    {
                        if (approvalProcessFeedBack.Count > 0)
                        {
                            notificationMessage= notificationMessage.Replace("Leave Applied", "Leave Updated");
                            UpdateApprovalProcessTitle((int)approvalProcessFeedBack["ApprovalProcessID"], approvalTitle
                                );
                            EmployeeIds = UpdateApprovalProcessFeedback((int)approvalProcessFeedBack["ApprovalProcessID"],
                                (int)approvalProcessFeedBack["APEmployeeFeedbackID"], (int)Util.ApprovalFeedback.Approved,
                                $@"Reviewed And Resubmited by {AppContexts.User.FullName} - {AppContexts.User.EmployeeCode}",
                                (int)approvalProcessFeedBack["APTypeID"],
                                (int)approvalProcessFeedBack["ReferenceID"], 0);
                            IsResubmitted = true;
                            ApprovalProcessID = approvalProcessFeedBack["ApprovalProcessID"].ToString();
                        }
                    }
                }



                unitOfWork.CommitChangesWithAudit();

                UpdateLeaveBalanceAfterSubmit(applicationMaster.EmployeeID);


                DateTime minStartDate = existingApplication.IsNullOrDbNull() ? applicationMaster.RequestStartDate : (((existingApplication.RequestStartDate < applicationMaster.RequestStartDate) || (existingApplication.RequestStartDate == applicationMaster.RequestStartDate)) ? existingApplication.RequestStartDate : applicationMaster.RequestStartDate);
                DateTime maxEndDate = existingApplication.IsNullOrDbNull() ? applicationMaster.RequestEndDate : (((existingApplication.RequestEndDate < applicationMaster.RequestEndDate) || (existingApplication.RequestEndDate == applicationMaster.RequestEndDate)) ? applicationMaster.RequestEndDate : existingApplication.RequestEndDate);
                UpdateAttendanceSummaryTable(applicationMaster.EmployeeID, minStartDate, maxEndDate);


                //DateTime minStartDate = existingApplication.IsNullOrDbNull() || existingApplication.RequestStartDate >= applicationMaster.RequestStartDate ? applicationMaster.RequestStartDate : existingApplication.RequestStartDate;

                //DateTime maxEndDate = existingApplication.IsNullOrDbNull() || existingApplication.RequestEndDate >= applicationMaster.RequestEndDate ? existingApplication.RequestEndDate : applicationMaster.RequestEndDate;

                //UpdateAttendanceSummaryTable(applicationMaster.EmployeeID, minStartDate, maxEndDate);


                //UpdateAttendanceSummaryTable(applicationMaster.EmployeeID, existingApplication.IsNullOrDbNull() ? applicationMaster.RequestStartDate : existingApplication.RequestStartDate, existingApplication.IsNullOrDbNull() ? applicationMaster.RequestEndDate : existingApplication.RequestEndDate);


                if (ApprovalProcessID.ToInt() > 0)
                    await SendMail(ApprovalProcessID, IsResubmitted, applicationMaster.EmployeeLeaveAID);
                //await SendMailToBackEmployee(ApprovalProcessID, IsResubmitted, applicationMaster.EmployeeLeaveAID);

            }
            currentNotificationEmployeeId = EmployeeIds.Split(',').ElementAtOrDefault(1);
            await Task.CompletedTask;
            responseDto.Success = true;
            responseDto.Message = "Leave Application Submitted Successfully";
            responseDto.EmployeeIds = EmployeeIds;
            responseDto.NotificationMessage = notificationMessage;
            responseDto.CurrentNotificaitonEmplyeeID = currentNotificationEmployeeId ?? string.Empty;
            return responseDto;
            //return (true, $"Leave Application Submitted Successfully", EmployeeIds, notificationMessage, currentNotificationEmployeeId ?? string.Empty);

        }
        private async Task SendMail(string ApprovalProcessID, bool IsResubmitted, int EmployeeLeaveAID)
        {
            var mail = GetAPEmployeeEmailsWithProxy(ApprovalProcessID.ToInt()).Result;
            List<string> ToEmailAddress = new List<string>() { mail.Item1 };
            List<string> CCEmailAddress = new List<string>() { mail.Item2 };

            if (ToEmailAddress.Count > 0)
                await ApprovalProcessMail(ApprovalProcessID.ToInt(), (int)Util.ApprovalType.LeaveApplication, (int)Util.MailGroupSetup.LeaveApprovalMail, IsResubmitted, ToEmailAddress, CCEmailAddress, null, EmployeeLeaveAID, 0);
            //await SendMailToBackEmployee(ApprovalProcessID, IsResubmitted, EmployeeLeaveAID);
        }
        private async Task SendMailToBackEmployee(string ApprovalProcessID, bool IsResubmitted, int EmployeeLeaveAID)
        {
            var mail = string.Empty;//GetBackupEmployeeMail(EmployeeLeaveAID).Result;
            List<string> ToEmailAddress = new List<string>() { mail };

            await ApprovalProcessMail(ApprovalProcessID.ToInt(), (int)Util.ApprovalType.LeaveApplication, (int)Util.MailGroupSetup.LeaveApplicationMailForBackupEmployee, IsResubmitted, ToEmailAddress, null, null, EmployeeLeaveAID, 0);
        }
        private void SetApplicationMasterNewId(EmployeeLeaveApplication applicationMaster)
        {
            if (!applicationMaster.IsAdded) return;
            var code = GenerateSystemCode("EmployeeLeaveApplication", AppContexts.User.CompanyID);
            applicationMaster.EmployeeLeaveAID = code.MaxNumber;
        }
        private void SetpplicationBreakDownNewId(EmployeeLeaveApplicationDayBreakDown applicationBreakdown)
        {
            if (!applicationBreakdown.IsAdded) return;
            var code = GenerateSystemCode("EmployeeLeaveApplicationDayBreakDown", AppContexts.User.CompanyID);
            applicationBreakdown.ELADBDID = code.MaxNumber;
        }
        private void SetAttachmentNewId(Attachments attachment)
        {
            var code = GenerateSystemCode("FileUpload", AppContexts.User.CompanyID);
            attachment.FUID = code.MaxNumber;
        }
        private void SetEmailNotificationNewId(EmailNotification emailNotification)
        {
            if (!emailNotification.IsAdded) return;
            var code = GenerateSystemCode("EmailNotification", AppContexts.User.CompanyID);
            emailNotification.ENID = code.MaxNumber;
        }
        private int SetEmailNotificationGroupNewId()
        {
            var code = GenerateSystemCode("EmailNotificationGroupId", AppContexts.User.CompanyID);
            int groupID = code.MaxNumber;

            return groupID;
        }
        public GridModel GetLeaveApplicationList(GridParameter parameters)
        {


            string sql = $@"SELECT 
                                DISTINCT
		                        ELA.EmployeeLeaveAID,
                                ELA.LeaveCategoryID LeaveCategoryID,
		                        SVLC.SystemVariableCode LeaveCategory,
		                        --CONCAT(CONVERT(char(11), ELA.RequestStartDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) LeaveDates,
                                CASE WHEN ELA.LeaveCategoryID=69 THEN CONCAT(CONVERT(char(11), ELA.RequestEndDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) ELSE CONCAT(CONVERT(char(11), ELA.RequestStartDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) end LeaveDates,
		                        ELA.NoOfLeaveDays NumberOfLeave,
		                        CASE WHEN ELA.CancellationStatus=1 THEN 'Partially Cancelled'
								WHEN ELA.CancellationStatus=2 THEN 'Fully Cancelled'
								ELSE SVAS.SystemVariableCode  END AS ApprovalStatus,
                                ELA.ApprovalStatusID,
                                ISNULL(AP.ApprovalProcessID,0) ApprovalProcessID,
                                ELA.CreatedDate,
								SVLC.SystemVariableCode + ( CASE WHEN ISNULL(LFA.LFADID,0) = 1 THEN ' (LFA Applied)'  ELSE '' END) LeaveType ,
                                EmployeeCode,
								FullName EmployeeName,

                            CASE 
		                        WHEN LFA.LFADID IS NULL
			                        THEN CAST(0 AS BIT)
		                        ELSE CAST(1 AS BIT)
		                        END IsLFAApplied,
								CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee,
								CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
								,ISNULL((SELECT DATEDIFF(DAY, Emp.DateOfJoining, GETDATE())), 0) AS TotalEmployementDays
	                        FROM HRMS.dbo.EmployeeLeaveApplication ELA
							LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee Emp ON Emp.EmployeeID = ELA.EmployeeID
	                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVLC ON ELA.LeaveCategoryID=SVLC.SystemVariableID
	                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVAS ON ELA.ApprovalStatusID=SVAS.SystemVariableID
							LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = ELA.EmployeeLeaveAID AND AP.APTypeID =1
                            
                            LEFT JOIN 
		                            (SELECT 
				                            APForwardInfoID,ApprovalProcessID 
			                            FROM 
				                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalForwardInfo  
			                            WHERE 
				                            EmployeeID = {AppContexts.User.EmployeeID} AND CommentSubmitDate IS NULL) 
                            APForward ON APForward.ApprovalProcessID = AP.ApprovalProcessID       
                            LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback  AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.LeaveApplication}   AND FeedbackSubmitDate IS NULL
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.LeaveApplication} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ELA.EmployeeLeaveAID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = ELA.EmployeeLeaveAID
									 LEFT JOIN LFADeclaration LFA ON ELA.EmployeeLeaveAID=LFA.EmployeeLeaveAID
							WHERE ELA.EmployeeID = {AppContexts.User.EmployeeID}";

            //return await Task.FromResult(EmployeeLeaveApplicationRepo.GetDataModelCollection<LeaveApplicationListDto>(sql));

            var applications = EmployeeLeaveApplicationRepo.LoadGridModel(parameters, sql);
            return applications;
        }
        public async Task<LeaveApplication> GetLeaveApplicationOld(int employeeLeaveAID, int approvalProcessID)
        {
            string sql = $@"SELECT 
		                        EmployeeLeaveAID,
		                        SVLC.SystemVariableCode LeaveCategory,
		                        ELA.LeaveCategoryID,
		                        CONVERT(char(10), ELA.RequestStartDate,103) RequestStartDate,
		                        CONVERT(char(10), ELA.RequestEndDate,103) RequestEndDate,
		                        --CONCAT(CONVERT(char(11), ELA.RequestStartDate,103),' - ',CONVERT(char(11), ELA.RequestEndDate,103)) LeaveDates,
                                CASE WHEN ELA.LeaveCategoryID=69 THEN CONCAT(CONVERT(char(11), ELA.RequestEndDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) ELSE CONCAT(CONVERT(char(11), ELA.RequestStartDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) end LeaveDates,
		                        ELA.NoOfLeaveDays NumberOfLeave,
		                        ELA.Purpose,
		                        ISNULL (E.FullName,'') BackupEmployeeName,
		                        ISNULL (ELA.BackupEmployeeID,0) BackupEmployeeID,
		                        ELA.LeaveLocation,
		                        ELA.DateOfJoiningWork,
                                ELA.Remarks,
								ELA.CancellationStatus,
								(EC.EmployeeCode+'-'+Ec.FullName)  CancelledBy
	                        FROM EmployeeLeaveApplication ELA
	                        JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVLC ON ELA.LeaveCategoryID=SVLC.SystemVariableID
	                        LEFT JOIN Employee E ON ELA.BackupEmployeeID=E.EmployeeID
                            LEFT JOIN viewEmployeeUserMapData EC ON EC.UserID=ELA.CancelledBy
	                        WHERE ELA.EmployeeLeaveAID={employeeLeaveAID}";

            var applicationMaster = EmployeeLeaveApplicationRepo.GetData(sql);

            var leaveApplication = new LeaveApplication
            {
                EmployeeLeaveAID = (int)applicationMaster["EmployeeLeaveAID"],
                LeaveCategory = applicationMaster["LeaveCategory"].ToString(),
                LeaveCategoryID = (int)applicationMaster["LeaveCategoryID"],
                RequestStartDate = applicationMaster["RequestStartDate"].ToString(),
                RequestEndDate = applicationMaster["RequestEndDate"].ToString(),
                LeaveDates = applicationMaster["LeaveDates"].ToString(),
                NumberOfLeave = (decimal)applicationMaster["NumberOfLeave"],
                Purpose = applicationMaster["Purpose"].ToString(),
                BackupEmployeeName = applicationMaster["BackupEmployeeName"].ToString(),
                BackupEmployeeID = (int?)applicationMaster["BackupEmployeeID"],
                CancellationStatus = (int)applicationMaster["CancellationStatus"],
                LeaveLocation = applicationMaster["LeaveLocation"].ToString(),
                Remarks = applicationMaster["Remarks"].ToString(),
                CancelledBy = applicationMaster["CancelledBy"].ToString(),
                DateOfJoiningWork = DateTime.TryParse(applicationMaster["DateOfJoiningWork"].ToString(), out var date) ? date : (DateTime?)null
            };
            leaveApplication.LeaveDetails = EmployeeLeaveApplicationDayBreakDownRepo.Entities.Where(x => x.EmployeeLeaveAID == employeeLeaveAID).Select(y => new LeaveDetails
            {
                Day = y.RequestDate.ToString("dd MMM yyyy"),
                DayStatus = y.IsCancelled ? "Cancelled" : y.HalfOrFullDay
            }).ToList();

            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='EmployeeLeaveApplication' AND ReferenceID={leaveApplication.EmployeeLeaveAID}";
            var attachment = EmployeeLeaveApplicationRepo.GetDataDictCollection(attachmentSql);
            var attachemntList = new List<Attachments>();

            foreach (var data in attachment)
            {
                attachemntList.Add(new Attachments
                {
                    FUID = (int)data["FUID"],
                    AID = data["FUID"].ToString(),
                    FilePath = data["FilePath"].ToString(),
                    OriginalName = data["OriginalName"].ToString() + data["FileType"].ToString(),
                    FileName = data["FileName"].ToString(),
                    Type = data["FileType"].ToString(),
                    Size = Convert.ToDecimal(data["SizeInKB"])
                });
            }
            leaveApplication.Attachments = attachemntList;

            if (approvalProcessID > 0)
            {
                leaveApplication.Comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.LeaveApplication).Result;
            }

            leaveApplication.LFADeclaration = LFADeclarationRepo.Entities.FirstOrDefault(x => x.EmployeeLeaveAID == leaveApplication.EmployeeLeaveAID).MapTo<LFADeclarationDto>();

            return await Task.FromResult(leaveApplication);
        }
        public async Task<LeaveApplication> GetLeaveApplication(int employeeLeaveAID, int approvalProcessID)
        {
            string sql = $@"SELECT DISTINCT
		                        EmployeeLeaveAID,
		                        SVLC.SystemVariableCode LeaveCategory,
		                        ELA.LeaveCategoryID,
		                        CONVERT(char(10), ELA.RequestStartDate,103) RequestStartDate,
		                        CONVERT(char(10), ELA.RequestEndDate,103) RequestEndDate,
		                        --CONCAT(CONVERT(char(11), ELA.RequestStartDate,103),' - ',CONVERT(char(11), ELA.RequestEndDate,103)) LeaveDates,
                                CASE WHEN ELA.LeaveCategoryID=69 THEN CONCAT(CONVERT(char(11), ELA.RequestEndDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) ELSE CONCAT(CONVERT(char(11), ELA.RequestStartDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) end LeaveDates,
		                        ELA.NoOfLeaveDays NumberOfLeave,
		                        ELA.Purpose,
		                        ISNULL (E.FullName,'') BackupEmployeeName,
		                        ISNULL (ELA.BackupEmployeeID,0) BackupEmployeeID,
		                        ELA.LeaveLocation,
		                        ELA.DateOfJoiningWork,
                                ELA.Remarks,
								ELA.CancellationStatus,
								(EC.EmployeeCode+'-'+Ec.FullName)  CancelledBy
	                        FROM EmployeeLeaveApplication ELA
	                        JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVLC ON ELA.LeaveCategoryID=SVLC.SystemVariableID
	                        LEFT JOIN Employee E ON ELA.BackupEmployeeID=E.EmployeeID
                            LEFT JOIN viewEmployeeUserMapData EC ON EC.UserID=ELA.CancelledBy
							LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = ELA.EmployeeLeaveAID AND AP.APTypeID = {(int)ApprovalType.LeaveApplication}
                           		
	                        WHERE ELA.EmployeeLeaveAID={employeeLeaveAID}  AND ELA.EmployeeID =  {AppContexts.User.EmployeeID}";

            var applicationMaster = EmployeeLeaveApplicationRepo.GetData(sql);

            var leaveApplication = new LeaveApplication
            {
                EmployeeLeaveAID = (int)applicationMaster["EmployeeLeaveAID"],
                LeaveCategory = applicationMaster["LeaveCategory"].ToString(),
                LeaveCategoryID = (int)applicationMaster["LeaveCategoryID"],
                RequestStartDate = applicationMaster["RequestStartDate"].ToString(),
                RequestEndDate = applicationMaster["RequestEndDate"].ToString(),
                LeaveDates = applicationMaster["LeaveDates"].ToString(),
                NumberOfLeave = (decimal)applicationMaster["NumberOfLeave"],
                Purpose = applicationMaster["Purpose"].ToString(),
                BackupEmployeeName = applicationMaster["BackupEmployeeName"].ToString(),
                BackupEmployeeID = (int?)applicationMaster["BackupEmployeeID"],
                CancellationStatus = (int)applicationMaster["CancellationStatus"],
                LeaveLocation = applicationMaster["LeaveLocation"].ToString(),
                Remarks = applicationMaster["Remarks"].ToString(),
                CancelledBy = applicationMaster["CancelledBy"].ToString(),
                DateOfJoiningWork = DateTime.TryParse(applicationMaster["DateOfJoiningWork"].ToString(), out var date) ? date : (DateTime?)null
            };
            leaveApplication.LeaveDetails = EmployeeLeaveApplicationDayBreakDownRepo.Entities.Where(x => x.EmployeeLeaveAID == employeeLeaveAID).Select(y => new LeaveDetails
            {
                Day = y.RequestDate.ToString("dd MMM yyyy"),
                DayStatus = y.IsCancelled ? "Cancelled" : y.HalfOrFullDay
            }).ToList();

            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='EmployeeLeaveApplication' AND ReferenceID={leaveApplication.EmployeeLeaveAID}";
            var attachment = EmployeeLeaveApplicationRepo.GetDataDictCollection(attachmentSql);
            var attachemntList = new List<Attachments>();

            foreach (var data in attachment)
            {
                attachemntList.Add(new Attachments
                {
                    FUID = (int)data["FUID"],
                    AID = data["FUID"].ToString(),
                    FilePath = data["FilePath"].ToString(),
                    OriginalName = data["OriginalName"].ToString() + data["FileType"].ToString(),
                    FileName = data["FileName"].ToString(),
                    Type = data["FileType"].ToString(),
                    Size = Convert.ToDecimal(data["SizeInKB"])
                });
            }
            leaveApplication.Attachments = attachemntList;

            if (approvalProcessID > 0)
            {
                leaveApplication.Comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.LeaveApplication).Result;
            }

            leaveApplication.LFADeclaration = LFADeclarationRepo.Entities.FirstOrDefault(x => x.EmployeeLeaveAID == leaveApplication.EmployeeLeaveAID).MapTo<LFADeclarationDto>();

            return await Task.FromResult(leaveApplication);
        }
        public async Task<LeaveApplication> GetLeaveApplicationForAdmin(int employeeLeaveAID, int approvalProcessID)
        {
            string sql = $@"SELECT DISTINCT
		                        EmployeeLeaveAID,
		                        SVLC.SystemVariableCode LeaveCategory,
		                        ELA.LeaveCategoryID,
		                        CONVERT(char(10), ELA.RequestStartDate,103) RequestStartDate,
		                        CONVERT(char(10), ELA.RequestEndDate,103) RequestEndDate,
		                        --CONCAT(CONVERT(char(11), ELA.RequestStartDate,103),' - ',CONVERT(char(11), ELA.RequestEndDate,103)) LeaveDates,
                                CASE WHEN ELA.LeaveCategoryID=69 THEN CONCAT(CONVERT(char(11), ELA.RequestEndDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) ELSE CONCAT(CONVERT(char(11), ELA.RequestStartDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) end LeaveDates,
		                        ELA.NoOfLeaveDays NumberOfLeave,
		                        ELA.Purpose,
		                        ISNULL (E.FullName,'') BackupEmployeeName,
		                        ISNULL (ELA.BackupEmployeeID,0) BackupEmployeeID,
		                        ELA.LeaveLocation,
		                        ELA.DateOfJoiningWork,
                                ELA.Remarks,
								ELA.CancellationStatus,
								(EC.EmployeeCode+'-'+Ec.FullName)  CancelledBy
	                        FROM EmployeeLeaveApplication ELA
	                        JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVLC ON ELA.LeaveCategoryID=SVLC.SystemVariableID
	                        LEFT JOIN Employee E ON ELA.BackupEmployeeID=E.EmployeeID
                            LEFT JOIN viewEmployeeUserMapData EC ON EC.UserID=ELA.CancelledBy
							LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = ELA.EmployeeLeaveAID AND AP.APTypeID = {(int)ApprovalType.LeaveApplication}
                           		
	                        WHERE ELA.EmployeeLeaveAID={employeeLeaveAID}";

            var applicationMaster = EmployeeLeaveApplicationRepo.GetData(sql);

            var leaveApplication = new LeaveApplication
            {
                EmployeeLeaveAID = (int)applicationMaster["EmployeeLeaveAID"],
                LeaveCategory = applicationMaster["LeaveCategory"].ToString(),
                LeaveCategoryID = (int)applicationMaster["LeaveCategoryID"],
                RequestStartDate = applicationMaster["RequestStartDate"].ToString(),
                RequestEndDate = applicationMaster["RequestEndDate"].ToString(),
                LeaveDates = applicationMaster["LeaveDates"].ToString(),
                NumberOfLeave = (decimal)applicationMaster["NumberOfLeave"],
                Purpose = applicationMaster["Purpose"].ToString(),
                BackupEmployeeName = applicationMaster["BackupEmployeeName"].ToString(),
                BackupEmployeeID = (int?)applicationMaster["BackupEmployeeID"],
                CancellationStatus = (int)applicationMaster["CancellationStatus"],
                LeaveLocation = applicationMaster["LeaveLocation"].ToString(),
                Remarks = applicationMaster["Remarks"].ToString(),
                CancelledBy = applicationMaster["CancelledBy"].ToString(),
                DateOfJoiningWork = DateTime.TryParse(applicationMaster["DateOfJoiningWork"].ToString(), out var date) ? date : (DateTime?)null
            };
            leaveApplication.LeaveDetails = EmployeeLeaveApplicationDayBreakDownRepo.Entities.Where(x => x.EmployeeLeaveAID == employeeLeaveAID).Select(y => new LeaveDetails
            {
                Day = y.RequestDate.ToString("dd MMM yyyy"),
                DayStatus = y.IsCancelled ? "Cancelled" : y.HalfOrFullDay
            }).ToList();

            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='EmployeeLeaveApplication' AND ReferenceID={leaveApplication.EmployeeLeaveAID}";
            var attachment = EmployeeLeaveApplicationRepo.GetDataDictCollection(attachmentSql);
            var attachemntList = new List<Attachments>();

            foreach (var data in attachment)
            {
                attachemntList.Add(new Attachments
                {
                    FUID = (int)data["FUID"],
                    AID = data["FUID"].ToString(),
                    FilePath = data["FilePath"].ToString(),
                    OriginalName = data["OriginalName"].ToString() + data["FileType"].ToString(),
                    FileName = data["FileName"].ToString(),
                    Type = data["FileType"].ToString(),
                    Size = Convert.ToDecimal(data["SizeInKB"])
                });
            }
            leaveApplication.Attachments = attachemntList;

            if (approvalProcessID > 0)
            {
                leaveApplication.Comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.LeaveApplication).Result;
            }

            leaveApplication.LFADeclaration = LFADeclarationRepo.Entities.FirstOrDefault(x => x.EmployeeLeaveAID == leaveApplication.EmployeeLeaveAID).MapTo<LFADeclarationDto>();

            return await Task.FromResult(leaveApplication);
        }
        public async Task<NotificationResponseDto> RemovLeaveApplicationAsync(int employeeLeaveAID)
        {
            NotificationResponseDto response = new NotificationResponseDto()
            {
                NotificationMessage= @$"Leave application removed by {AppContexts.User.FullName} ({AppContexts.User.EmployeeCode}) "
            };
            
            using (var unitOfWork = new UnitOfWork())
            {
                //var result = await GetApprovalEmployees(employeeLeaveAID, (int)Util.ApprovalType.LeaveApplication, AppContexts.GetDatabaseName(ConnectionName.ApprovalContext));
                //EmployeeIds = result["EmployeeIDs"].ToString();
                var result = await GetApprovalListOfEmployees(employeeLeaveAID, (int)Util.ApprovalType.LeaveApplication, AppContexts.GetDatabaseName(ConnectionName.ApprovalContext));
                if(result.Count> 0)
                {
                    response.EmployeeIds = string.Join(",", result.Select(o => o.EmployeeID));
                    response.CurrentNotificaitonEmplyeeID = result.FirstOrDefault(x => x.APFeedbackID == (int)Util.ApprovalFeedback.FeedbackRequested).EmployeeID.ToString();
                }
                
                var leaveApplicationMaster = EmployeeLeaveApplicationRepo.Entities.Where(x => x.EmployeeLeaveAID == employeeLeaveAID).FirstOrDefault();
                leaveApplicationMaster.SetDeleted();
                var breakDownList = EmployeeLeaveApplicationDayBreakDownRepo.Entities.Where(x => x.EmployeeLeaveAID == employeeLeaveAID).ToList();
                breakDownList.ForEach(x => x.SetDeleted());

                EmployeeLeaveApplicationRepo.Add(leaveApplicationMaster);
                EmployeeLeaveApplicationDayBreakDownRepo.AddRange(breakDownList);

                DeleteAllApprovalProcessRelatedData((int)ApprovalType.LeaveApplication, employeeLeaveAID);


                unitOfWork.CommitChangesWithAudit();
                UpdateLeaveBalanceAfterSubmit(leaveApplicationMaster.EmployeeID);
                UpdateAttendanceSummaryTable(leaveApplicationMaster.EmployeeID, leaveApplicationMaster.RequestStartDate, leaveApplicationMaster.RequestEndDate);


            }

            return response;
        }
        public async Task<LeaveApplicationWithComments> GetLeaveApplicationWithCommentsForApproval(int employeeLeaveAID, int approvalProcessID)
        {
            string sql = $@"SELECT DISTINCT
		                        EmployeeLeaveAID,
                                E.EmployeeID,
								E.FullName, 
								E.EmployeeCode,
								E.DepartmentName,
								E.DesignationName,
								E.DivisionName,
								E.ImagePath,
		                        SVLC.SystemVariableCode LeaveCategory,
		                        ELA.LeaveCategoryID,
		                        CONVERT(char(10), ELA.RequestStartDate,103) RequestStartDate,
		                        CONVERT(char(10), ELA.RequestEndDate,103) RequestEndDate,
		                        --CONCAT(CONVERT(char(11), ELA.RequestStartDate,103),' - ',CONVERT(char(11), ELA.RequestEndDate,103)) LeaveDates,
                                CASE WHEN ELA.LeaveCategoryID=69 THEN CONCAT(CONVERT(char(11), ELA.RequestEndDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) ELSE CONCAT(CONVERT(char(11), ELA.RequestStartDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) end LeaveDates,
		                        ELA.NoOfLeaveDays NumberOfLeave,
		                        ELA.Purpose,
                                ELA.CancellationStatus,
                                ELA.Remarks,
								(CE.EmployeeCode +'-'+CE.FullName) CancelledBy,
		                        ISNULL (BE.FullName,'') BackupEmployeeName,
		                        ISNULL (ELA.BackupEmployeeID,0) BackupEmployeeID,
		                        ELA.LeaveLocation,
		                        ELA.DateOfJoiningWork,
                                ISNULL(APForward.APForwardInfoID,0) APForwardInfoID,
                                CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL({approvalProcessID}, 0))) AS Bit) IsCurrentAPEmployee,
                                CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL({approvalProcessID}, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
	                        FROM EmployeeLeaveApplication ELA
							LEFT JOIN ViewALLEmployee  E  ON ELA.EmployeeID=E.EmployeeID
                            LEFT JOIN viewEmployeeUserMapData  CE  ON ELA.CancelledBy=CE.UserID
	                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVLC ON ELA.LeaveCategoryID=SVLC.SystemVariableID
	                        LEFT JOIN Employee BE ON ELA.BackupEmployeeID=BE.EmployeeID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = ELA.EmployeeLeaveAID AND AP.APTypeID =1
                            LEFT JOIN 
		                            (SELECT 
				                            APForwardInfoID,ApprovalProcessID 
			                            FROM 
				                            Approval..ApprovalForwardInfo  
			                            WHERE CommentSubmitDate IS NULL) 
                            APForward ON APForward.ApprovalProcessID = AP.ApprovalProcessID
                            LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = 1 AND FeedbackSubmitDate IS NULL
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = 1 AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ELA.EmployeeLeaveAID
                                LEFT JOIN 
								(
								    SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF( {AppContexts.User.EmployeeID}) 
								)							
						        F ON F.ApprovalProcessID = Ap.ApprovalProcessID

                            WHERE ELA.EmployeeLeaveAID={employeeLeaveAID}  AND (E.EmployeeID =  {AppContexts.User.EmployeeID} OR F.EmployeeID =  {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) =  {AppContexts.User.EmployeeID})";
            var applicationMaster = EmployeeLeaveApplicationRepo.GetData(sql);
            var leaveApplication = new LeaveApplicationWithComments
            {
                EmployeeLeaveAID = (int)applicationMaster["EmployeeLeaveAID"],
                EmployeeID = (int)applicationMaster["EmployeeID"],
                APForwardInfoID = (int)applicationMaster["APForwardInfoID"],
                FullName = applicationMaster["FullName"].ToString(),
                IsCurrentAPEmployee = (bool)applicationMaster["IsCurrentAPEmployee"],
                IsReassessment = (bool)applicationMaster["IsReassessment"],
                EmployeeCode = applicationMaster["EmployeeCode"].ToString(),
                DepartmentName = applicationMaster["DepartmentName"].ToString(),
                DesignationName = applicationMaster["DesignationName"].ToString(),
                DivisionName = applicationMaster["DivisionName"].ToString(),
                ImagePath = applicationMaster["ImagePath"].ToString(),
                LeaveCategory = applicationMaster["LeaveCategory"].ToString(),
                LeaveCategoryID = (int)applicationMaster["LeaveCategoryID"],
                RequestStartDate = applicationMaster["RequestStartDate"].ToString(),
                RequestEndDate = applicationMaster["RequestEndDate"].ToString(),
                LeaveDates = applicationMaster["LeaveDates"].ToString(),
                NumberOfLeave = (decimal)applicationMaster["NumberOfLeave"],
                Purpose = applicationMaster["Purpose"].ToString(),
                Remarks = applicationMaster["Remarks"].ToString(),
                CancelledBy = applicationMaster["CancelledBy"].ToString(),
                BackupEmployeeName = applicationMaster["BackupEmployeeName"].ToString(),
                BackupEmployeeID = (int?)applicationMaster["BackupEmployeeID"],
                CancellationStatus = (int)applicationMaster["CancellationStatus"],
                LeaveLocation = applicationMaster["LeaveLocation"].ToString(),
                DateOfJoiningWork = DateTime.TryParse(applicationMaster["DateOfJoiningWork"].ToString(), out var date) ? date : (DateTime?)null
            };

            leaveApplication.LeaveDetails = EmployeeLeaveApplicationDayBreakDownRepo.Entities.Where(x => x.EmployeeLeaveAID == employeeLeaveAID).Select(y => new LeaveDetails
            {
                ELADBDID = y.ELADBDID,
                Day = y.RequestDate.ToString("dd MMM yyyy"),
                DayStatus = y.IsCancelled ? "Cancelled" : y.HalfOrFullDay
            }).ToList();

            leaveApplication.LeaveBalances = LeaveBalance(leaveApplication.LeaveCategoryID, (decimal)(leaveApplication.LeaveDetails.Count(x => x.DayStatus == Util.Fullday) * 1 + (leaveApplication.LeaveDetails.Count(x => x.DayStatus == Util.Halfday || x.DayStatus == Util.OnlyHalfday || x.DayStatus == Util.FirstHalfday || x.DayStatus == Util.SecondHalfday) * 0.5)), employeeLeaveAID, leaveApplication.EmployeeID);

            leaveApplication.Comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.LeaveApplication).Result;
            leaveApplication.RejectedMembers = GetApprovalRejectedMembers(approvalProcessID).Result;
            leaveApplication.ForwardingMembers = GetApprovalForwardingMembers(employeeLeaveAID, (int)Util.ApprovalType.LeaveApplication, (int)Util.ApprovalPanel.HRLeaveApprovalPanel).Result;
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='EmployeeLeaveApplication' AND ReferenceID={leaveApplication.EmployeeLeaveAID}";
            var attachment = EmployeeLeaveApplicationRepo.GetDataDictCollection(attachmentSql);
            var attachemntList = new List<Attachments>();

            foreach (var data in attachment)
            {
                attachemntList.Add(new Attachments
                {
                    FUID = (int)data["FUID"],
                    AID = data["FUID"].ToString(),
                    FilePath = data["FilePath"].ToString(),
                    OriginalName = data["OriginalName"].ToString() + data["FileType"].ToString(),
                    FileName = data["FileName"].ToString(),
                    Type = data["FileType"].ToString(),
                    Size = Convert.ToDecimal(data["SizeInKB"])
                });
            }
            leaveApplication.Attachments = attachemntList;
            leaveApplication.LFADeclaration = LFADeclarationRepo.Entities.FirstOrDefault(x => x.EmployeeLeaveAID == employeeLeaveAID).MapTo<LFADeclarationDto>();

            return await Task.FromResult(leaveApplication);
        }
        public async Task<LeaveApplicationWithComments> GetLeaveApplicationWithCommentsForApprovalForHR(int employeeLeaveAID, int approvalProcessID)
        {
            string sql = $@"SELECT 
		                        EmployeeLeaveAID,
                                E.EmployeeID,
								E.FullName, 
								E.EmployeeCode,
								E.DepartmentName,
								E.DesignationName,
								E.DivisionName,
								E.ImagePath,
		                        SVLC.SystemVariableCode LeaveCategory,
		                        ELA.LeaveCategoryID,
		                        CONVERT(char(10), ELA.RequestStartDate,103) RequestStartDate,
		                        CONVERT(char(10), ELA.RequestEndDate,103) RequestEndDate,
                                CASE WHEN ELA.LeaveCategoryID=69 THEN CONCAT(CONVERT(char(11), ELA.RequestEndDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) ELSE CONCAT(CONVERT(char(11), ELA.RequestStartDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) end LeaveDates,
		                        ELA.NoOfLeaveDays NumberOfLeave,
		                        ELA.Purpose,
                                ELA.CancellationStatus,
                                ELA.Remarks,
								(CE.EmployeeCode +'-'+CE.FullName) CancelledBy,
		                        ISNULL (BE.FullName,'') BackupEmployeeName,
		                        ISNULL (ELA.BackupEmployeeID,0) BackupEmployeeID,
		                        ELA.LeaveLocation,
		                        ELA.DateOfJoiningWork,
                                ISNULL(APForward.APForwardInfoID,0) APForwardInfoID,
                                CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL({approvalProcessID}, 0))) AS Bit) IsCurrentAPEmployee,
                                CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL({approvalProcessID}, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
	                        FROM EmployeeLeaveApplication ELA
							LEFT JOIN ViewALLEmployee  E  ON ELA.EmployeeID=E.EmployeeID
                            LEFT JOIN viewEmployeeUserMapData  CE  ON ELA.CancelledBy=CE.UserID
	                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVLC ON ELA.LeaveCategoryID=SVLC.SystemVariableID
	                        LEFT JOIN Employee BE ON ELA.BackupEmployeeID=BE.EmployeeID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = ELA.EmployeeLeaveAID AND AP.APTypeID =1
                            LEFT JOIN 
		                            (SELECT 
				                            APForwardInfoID,ApprovalProcessID 
			                            FROM 
				                            Approval..ApprovalForwardInfo  
			                            WHERE CommentSubmitDate IS NULL) 
                            APForward ON APForward.ApprovalProcessID = AP.ApprovalProcessID
                            LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = 1 AND FeedbackSubmitDate IS NULL
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = 1 AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ELA.EmployeeLeaveAID
                            WHERE ELA.EmployeeLeaveAID={employeeLeaveAID}";
            var applicationMaster = EmployeeLeaveApplicationRepo.GetData(sql);
            var leaveApplication = new LeaveApplicationWithComments
            {
                EmployeeLeaveAID = (int)applicationMaster["EmployeeLeaveAID"],
                EmployeeID = (int)applicationMaster["EmployeeID"],
                APForwardInfoID = (int)applicationMaster["APForwardInfoID"],
                FullName = applicationMaster["FullName"].ToString(),
                IsCurrentAPEmployee = (bool)applicationMaster["IsCurrentAPEmployee"],
                IsReassessment = (bool)applicationMaster["IsReassessment"],
                EmployeeCode = applicationMaster["EmployeeCode"].ToString(),
                DepartmentName = applicationMaster["DepartmentName"].ToString(),
                DesignationName = applicationMaster["DesignationName"].ToString(),
                DivisionName = applicationMaster["DivisionName"].ToString(),
                ImagePath = applicationMaster["ImagePath"].ToString(),
                LeaveCategory = applicationMaster["LeaveCategory"].ToString(),
                LeaveCategoryID = (int)applicationMaster["LeaveCategoryID"],
                RequestStartDate = applicationMaster["RequestStartDate"].ToString(),
                RequestEndDate = applicationMaster["RequestEndDate"].ToString(),
                LeaveDates = applicationMaster["LeaveDates"].ToString(),
                NumberOfLeave = (decimal)applicationMaster["NumberOfLeave"],
                Purpose = applicationMaster["Purpose"].ToString(),
                Remarks = applicationMaster["Remarks"].ToString(),
                CancelledBy = applicationMaster["CancelledBy"].ToString(),
                BackupEmployeeName = applicationMaster["BackupEmployeeName"].ToString(),
                BackupEmployeeID = (int?)applicationMaster["BackupEmployeeID"],
                CancellationStatus = (int)applicationMaster["CancellationStatus"],
                LeaveLocation = applicationMaster["LeaveLocation"].ToString(),
                DateOfJoiningWork = DateTime.TryParse(applicationMaster["DateOfJoiningWork"].ToString(), out var date) ? date : (DateTime?)null
            };

            leaveApplication.LeaveDetails = EmployeeLeaveApplicationDayBreakDownRepo.Entities.Where(x => x.EmployeeLeaveAID == employeeLeaveAID).Select(y => new LeaveDetails
            {
                ELADBDID = y.ELADBDID,
                Day = y.RequestDate.ToString("dd MMM yyyy"),
                DayStatus = y.IsCancelled ? "Cancelled" : y.HalfOrFullDay
            }).ToList();

            leaveApplication.LeaveBalances = LeaveBalance(leaveApplication.LeaveCategoryID, (decimal)(leaveApplication.LeaveDetails.Count(x => x.DayStatus == Util.Fullday) * 1 + (leaveApplication.LeaveDetails.Count(x => x.DayStatus == Util.Halfday || x.DayStatus == Util.OnlyHalfday || x.DayStatus == Util.FirstHalfday || x.DayStatus == Util.SecondHalfday) * 0.5)), employeeLeaveAID, leaveApplication.EmployeeID);

            leaveApplication.Comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.LeaveApplication).Result;
            leaveApplication.RejectedMembers = GetApprovalRejectedMembers(approvalProcessID).Result;
            leaveApplication.ForwardingMembers = GetApprovalForwardingMembers(employeeLeaveAID, (int)Util.ApprovalType.LeaveApplication, (int)Util.ApprovalPanel.HRLeaveApprovalPanel).Result;
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='EmployeeLeaveApplication' AND ReferenceID={leaveApplication.EmployeeLeaveAID}";
            var attachment = EmployeeLeaveApplicationRepo.GetDataDictCollection(attachmentSql);
            var attachemntList = new List<Attachments>();

            foreach (var data in attachment)
            {
                attachemntList.Add(new Attachments
                {
                    FUID = (int)data["FUID"],
                    AID = data["FUID"].ToString(),
                    FilePath = data["FilePath"].ToString(),
                    OriginalName = data["OriginalName"].ToString() + data["FileType"].ToString(),
                    FileName = data["FileName"].ToString(),
                    Type = data["FileType"].ToString(),
                    Size = Convert.ToDecimal(data["SizeInKB"])
                });
            }
            leaveApplication.Attachments = attachemntList;
            leaveApplication.LFADeclaration = LFADeclarationRepo.Entities.FirstOrDefault(x => x.EmployeeLeaveAID == employeeLeaveAID).MapTo<LFADeclarationDto>();

            return await Task.FromResult(leaveApplication);
        }
        private List<Attachments> AddAttachments(List<Attachments> list)
        {
            if (list.Count > 0)
            {
                var attachemntList = new List<Attachments>();
                int sl = 0;
                foreach (var attachment in list)
                {
                    if (attachment.AttachedFile.IsNotNull())
                    {
                        string filename = $"LA-{DateTime.Now.ToString("ddMMyyHHmmss")}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "LeaveApplication\\" + AppContexts.User.PersonID + " - " + (AppContexts.User.FullName).Replace(" ", ""));

                        attachemntList.Add(new Attachments
                        {
                            FilePath = filePath,
                            OriginalName = Path.GetFileNameWithoutExtension(attachment.OriginalName),
                            FileName = filename,
                            Type = Path.GetExtension(attachment.OriginalName),
                            Size = attachment.Size
                        });

                        sl++;
                    }

                }
                return attachemntList;
            }
            return null;
        }
        private List<Attachments> RemoveAttachments(LeaveApplication application, int referenceId)
        {
            if (application.Attachments.Count > 0)
            {
                var attachemntList = new List<Attachments>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='EmployeeLeaveApplication' AND ReferenceID={referenceId}";
                var prevAttachment = EmployeeLeaveApplicationRepo.GetDataDictCollection(attachmentSql);

                foreach (var data in prevAttachment)
                {
                    attachemntList.Add(new Attachments
                    {
                        FUID = (int)data["FUID"],
                        FilePath = data["FilePath"].ToString(),
                        OriginalName = data["OriginalName"].ToString(),
                        FileName = data["FileName"].ToString(),
                        Type = data["FileType"].ToString(),
                        Size = Convert.ToDecimal(data["SizeInKB"]),

                    });
                }
                var removeList = attachemntList.Where(x => !application.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeList.Count > 0)
                {
                    foreach (var data in removeList)
                    {
                        string attachmentFolder = "upload\\attachments";
                        string folderName = "LeaveApplication";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.PersonID + " - " + (AppContexts.User.FullName).Replace(" ", ""));
                        System.IO.File.Delete(str + "\\" + data.FileName);

                    }

                }
                return removeList;
            }
            return null;
        }
        public void SaveLFA(LFADeclarationDto application)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var lfaEnt = application.MapTo<LFADeclaration>();
                lfaEnt.SetAdded();
                SetNewLFAID(lfaEnt);
                SetAuditFields(lfaEnt);
                LFADeclarationRepo.Add(lfaEnt);
                unitOfWork.CommitChangesWithAudit();
            }
        }
        private void SetNewLFAID(LFADeclaration obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("LFADeclaration", AppContexts.User.CompanyID);
            obj.LFADID = code.MaxNumber;
        }
        public GridModel GetAllLeaveApplicationList(GridParameter parameters)
        {
            // "All","Pending Action","Action Taken"
            string filter = "";
            switch (parameters.ApprovalFilterData)
            {
                case "All":
                    filter = "";
                    break;
                case "My Pending":
                    filter = $@" AND CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) = 1";
                    break;
                case "Pending":
                    filter = $@" AND ELA.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND ELA.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
                    break;
                case "My Approved":
                    filter = $@" AND AP.ApprovalProcessID IN (SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Approved}))";
                    break;
                case "MyRejectReturnForwarded":
                    filter = $@" AND AP.ApprovalProcessID IN 
                                (SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Rejected})
                                UNION  
                                SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Returned})
                                UNION 
                                SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Forwarded}))";
                    break;
                default:
                    break;
            }
            string sql = $@"SELECT 
                                DISTINCT
		                        ELA.EmployeeLeaveAID,
                                ELA.LeaveCategoryID LeaveCategoryID,
		                        SVLC.SystemVariableCode LeaveCategory,
                                Emp.FullName EmployeeName,
                                Emp.EmployeeCode EmployeeCode,
		                        --CONCAT(CONVERT(char(11), ELA.RequestStartDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) LeaveDates,
                                CASE WHEN ELA.LeaveCategoryID=69 THEN CONCAT(CONVERT(char(11), ELA.RequestEndDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) ELSE CONCAT(CONVERT(char(11), ELA.RequestStartDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) end LeaveDates,
		                        ELA.NoOfLeaveDays NumberOfLeave,
		                        CASE WHEN ELA.CancellationStatus=1 THEN 'Partially Cancelled'
								WHEN ELA.CancellationStatus=2 THEN 'Fully Cancelled'
								ELSE SVAS.SystemVariableCode  END AS ApprovalStatus,
                                ELA.ApprovalStatusID,
                                ISNULL(AP.ApprovalProcessID,0) ApprovalProcessID,
                                ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID,
								ISNULL(APForward.APForwardInfoID,0) APForwardInfoID,
                                ELA.CreatedDate,
								SVLC.SystemVariableCode + ( CASE WHEN ISNULL(LFA.LFADID,0) = 1 THEN ' (LFA Applied)'  ELSE '' END) LeaveType ,

                            CASE 
		                        WHEN LFA.LFADID IS NULL
			                        THEN CAST(0 AS BIT)
		                        ELSE CAST(1 AS BIT)
		                        END IsLFAApplied,
								CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee,
								CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,ISNULL((SELECT DATEDIFF(DAY, Emp.DateOfJoining, GETDATE())), 0) AS TotalEmployementDays
	                        FROM HRMS.dbo.EmployeeLeaveApplication ELA
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee Emp on Emp.EmployeeID=ELA.EmployeeID
	                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVLC ON ELA.LeaveCategoryID=SVLC.SystemVariableID
	                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVAS ON ELA.ApprovalStatusID=SVAS.SystemVariableID
							LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = ELA.EmployeeLeaveAID AND AP.APTypeID =1
                            LEFT JOIN (SELECT 
				                            APEmployeeFeedbackID,ApprovalProcessID,IsEditable 
			                            FROM 
				                            Approval..ApprovalEmployeeFeedback AEF
			                            WHERE (AEF.APFeedbackID = 2 OR AEF.APFeedbackID = 8 OR AEF.APFeedbackID = 9) AND (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})
                              UNION ALL 
										SELECT 
				                            EmployeeFeedbackID,ApprovalProcessID,0 IsEditable 
			                            FROM 
				                            Approval..ApprovalForwardInfo  
			                            WHERE 
				                            EmployeeID = {AppContexts.User.EmployeeID} AND CommentSubmitDate IS NULL
                            )AEF ON AEF.ApprovalProcessID = AP.ApprovalProcessID
                            LEFT JOIN 
		                            (SELECT 
				                            APForwardInfoID,ApprovalProcessID 
			                            FROM 
				                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalForwardInfo  
			                            WHERE  CommentSubmitDate IS NULL) 
                            APForward ON APForward.ApprovalProcessID = AP.ApprovalProcessID
                            LEFT JOIN 
									(
									SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM  Approval..ApprovalEmployeeFeedback
									UNION ALL 
									SELECT DISTINCT ApprovalProcessID,EmployeeID,EmployeeID FROM Approval..ApprovalForwardInfo 
									)							
							F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                            LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback  AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.LeaveApplication}   AND FeedbackSubmitDate IS NULL
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.LeaveApplication} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ELA.EmployeeLeaveAID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = ELA.EmployeeLeaveAID
									 LEFT JOIN LFADeclaration LFA ON ELA.EmployeeLeaveAID=LFA.EmployeeLeaveAID
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
							";
            var applications = EmployeeLeaveApplicationRepo.LoadGridModel(parameters, sql);

            //var testData = new EmailDtoCore {
            //    Subject="Test"
            //};
            //await Extension.Post<EmailDtoCore>($"/SendMail/SendEMailToRecipients", testData);
            return applications;
        }
        public GridModel GetAllPendingLeaveApplicationList(GridParameter parameters)
        {
            // "All","Pending Action","Action Taken"
            string filter = "";
            switch (parameters.ApprovalFilterData)
            {
                case "All":
                    filter = "";
                    break;
                case "My Pending":
                    filter = $@" AND CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) = 1";
                    break;
                case "Pending":
                    filter = $@" AND ELA.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND ELA.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
                    break;
                case "My Approved":
                    filter = $@" AND AP.ApprovalProcessID IN (SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Approved}))";
                    break;
                case "MyRejectReturnForwarded":
                    filter = $@" AND AP.ApprovalProcessID IN 
                                (SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Rejected})
                                UNION  
                                SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Returned})
                                UNION 
                                SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Forwarded}))";
                    break;
                default:
                    break;
            }
            string sql = $@"SELECT 
                                DISTINCT
		                        ELA.EmployeeLeaveAID,
                                ELA.LeaveCategoryID LeaveCategoryID,
		                        SVLC.SystemVariableCode LeaveCategory,
                                Emp.FullName EmployeeName,
                                Emp.EmployeeCode EmployeeCode,
		                        --CONCAT(CONVERT(char(11), ELA.RequestStartDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) LeaveDates,
                                CASE WHEN ELA.LeaveCategoryID=69 THEN CONCAT(CONVERT(char(11), ELA.RequestEndDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) ELSE CONCAT(CONVERT(char(11), ELA.RequestStartDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) end LeaveDates,
		                        ELA.NoOfLeaveDays NumberOfLeave,
		                        CASE WHEN ELA.CancellationStatus=1 THEN 'Partially Cancelled'
								WHEN ELA.CancellationStatus=2 THEN 'Fully Cancelled'
								ELSE SVAS.SystemVariableCode  END AS ApprovalStatus,
                                ELA.ApprovalStatusID,
                                ISNULL(AP.ApprovalProcessID,0) ApprovalProcessID,
                                ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID,
								ISNULL(APForward.APForwardInfoID,0) APForwardInfoID,
                                ELA.CreatedDate,
								SVLC.SystemVariableCode + ( CASE WHEN ISNULL(LFA.LFADID,0) = 1 THEN ' (LFA Applied)'  ELSE '' END) LeaveType ,

                            CASE 
		                        WHEN LFA.LFADID IS NULL
			                        THEN CAST(0 AS BIT)
		                        ELSE CAST(1 AS BIT)
		                        END IsLFAApplied,
								CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee,
								CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,ISNULL((SELECT DATEDIFF(DAY, Emp.DateOfJoining, GETDATE())), 0) AS TotalEmployementDays
	                        FROM HRMS.dbo.EmployeeLeaveApplication ELA
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee Emp on Emp.EmployeeID=ELA.EmployeeID
	                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVLC ON ELA.LeaveCategoryID=SVLC.SystemVariableID
	                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVAS ON ELA.ApprovalStatusID=SVAS.SystemVariableID
							LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = ELA.EmployeeLeaveAID AND AP.APTypeID =1
                            LEFT JOIN (SELECT 
				                            APEmployeeFeedbackID,ApprovalProcessID,IsEditable 
			                            FROM 
				                            Approval..ApprovalEmployeeFeedback AEF
			                            WHERE (AEF.APFeedbackID = 2 OR AEF.APFeedbackID = 8 OR AEF.APFeedbackID = 9) AND (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})
                              UNION ALL 
										SELECT 
				                            EmployeeFeedbackID,ApprovalProcessID,0 IsEditable 
			                            FROM 
				                            Approval..ApprovalForwardInfo  
			                            WHERE 
				                            EmployeeID = {AppContexts.User.EmployeeID} AND CommentSubmitDate IS NULL
                            )AEF ON AEF.ApprovalProcessID = AP.ApprovalProcessID
                            LEFT JOIN 
		                            (SELECT 
				                            APForwardInfoID,ApprovalProcessID 
			                            FROM 
				                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalForwardInfo  
			                            WHERE  CommentSubmitDate IS NULL) 
                            APForward ON APForward.ApprovalProcessID = AP.ApprovalProcessID
                            LEFT JOIN 
									(
									SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM  Approval..ApprovalEmployeeFeedback
									UNION ALL 
									SELECT DISTINCT ApprovalProcessID,EmployeeID,EmployeeID FROM Approval..ApprovalForwardInfo 
									)							
							F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                            LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback  AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.LeaveApplication}   AND FeedbackSubmitDate IS NULL
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.LeaveApplication} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ELA.EmployeeLeaveAID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = ELA.EmployeeLeaveAID
									 LEFT JOIN LFADeclaration LFA ON ELA.EmployeeLeaveAID=LFA.EmployeeLeaveAID
							WHERE ELA.ApprovalStatusID={(int)Util.ApprovalStatus.Pending} AND  CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) =1 AND (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) =  {AppContexts.User.EmployeeID}) {filter}
							";
            var applications = EmployeeLeaveApplicationRepo.LoadGridModel(parameters, sql);

            //var testData = new EmailDtoCore {
            //    Subject="Test"
            //};
            //await Extension.Post<EmailDtoCore>($"/SendMail/SendEMailToRecipients", testData);
            return applications;
        }
        public GridModel GetAllLeaveApplicationListForHR(GridParameter parameters, string type)
        {
            // "All","Pending Action","Action Taken"
            string filter = "";
            switch (parameters.ApprovalFilterData)
            {
                case "All":
                    filter = "";
                    break;
                case "Pending":
                    filter = $@" ELA.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" ELA.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
                    break;
                case "Rejected":
                    filter = $@" ELA.ApprovalStatusID = {(int)Util.ApprovalStatus.Rejected}";
                    break;
                default:
                    break;
            }
            filter = filter.IsNotNullOrEmpty() ? @$"WHERE {filter}" : "";
            if (type == "TotalLeaveToday")
            {
                string filterNew = @$" CAST(GETDATE() as Date) BETWEEN CAST(RequestStartDate as Date) AND CAST(RequestEndDate as Date) AND ELA.ApprovalStatusID <> {(int)Util.ApprovalStatus.Rejected}";
                filter += filter.IsNotNullOrEmpty() ? @$" AND {filterNew}" : $@" WHERE {filterNew}";
            }
            string sql = $@"SELECT 
                                DISTINCT
		                        ELA.EmployeeLeaveAID,
                                ELA.LeaveCategoryID LeaveCategoryID,
		                        SVLC.SystemVariableCode LeaveCategory,
                                Emp.FullName EmployeeName,
                                Emp.EmployeeCode EmployeeCode,
		                        --CONCAT(CONVERT(char(11), ELA.RequestStartDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) LeaveDates,
                                CASE WHEN ELA.LeaveCategoryID=69 THEN CONCAT(CONVERT(char(11), ELA.RequestEndDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) ELSE CONCAT(CONVERT(char(11), ELA.RequestStartDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) end LeaveDates,
		                        ELA.NoOfLeaveDays NumberOfLeave,
		                        CASE WHEN ELA.CancellationStatus=1 THEN 'Partially Cancelled'
								WHEN ELA.CancellationStatus=2 THEN 'Fully Cancelled'
								ELSE SVAS.SystemVariableCode  END AS ApprovalStatus,
                                ELA.ApprovalStatusID,
                                ISNULL(AP.ApprovalProcessID,0) ApprovalProcessID,
                                ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID,
								ISNULL(APForward.APForwardInfoID,0) APForwardInfoID,
								ELA.CreatedDate,
								SVLC.SystemVariableCode + ( CASE WHEN ISNULL(LFA.LFADID,0) = 1 THEN ' (LFA Applied)'  ELSE '' END) LeaveType ,
                            CASE 
		                        WHEN LFA.LFADID IS NULL
			                        THEN CAST(0 AS BIT)
		                        ELSE CAST(1 AS BIT)
		                        END IsLFAApplied,
								CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee,
								CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
	                        FROM HRMS.dbo.EmployeeLeaveApplication ELA
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee Emp on Emp.EmployeeID=ELA.EmployeeID
	                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVLC ON ELA.LeaveCategoryID=SVLC.SystemVariableID
	                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVAS ON ELA.ApprovalStatusID=SVAS.SystemVariableID
							LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = ELA.EmployeeLeaveAID AND AP.APTypeID =1
                            LEFT JOIN (SELECT 
				                            APEmployeeFeedbackID,ApprovalProcessID,IsEditable 
			                            FROM 
				                            Approval..ApprovalEmployeeFeedback AEF
			                            WHERE (AEF.APFeedbackID = 2 OR AEF.APFeedbackID = 8 OR AEF.APFeedbackID = 9) AND (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})
                              UNION ALL 
										SELECT 
				                            EmployeeFeedbackID,ApprovalProcessID,0 IsEditable 
			                            FROM 
				                            Approval..ApprovalForwardInfo  
			                            WHERE 
				                            EmployeeID = {AppContexts.User.EmployeeID} AND CommentSubmitDate IS NULL
                            )AEF ON AEF.ApprovalProcessID = AP.ApprovalProcessID
                            LEFT JOIN 
		                            (SELECT 
				                            APForwardInfoID,ApprovalProcessID 
			                            FROM 
				                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalForwardInfo  
			                            WHERE  CommentSubmitDate IS NULL) 
                            APForward ON APForward.ApprovalProcessID = AP.ApprovalProcessID
                            LEFT JOIN 
									(
									SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM  Approval..ApprovalEmployeeFeedback
									UNION ALL 
									SELECT DISTINCT ApprovalProcessID,EmployeeID,EmployeeID FROM Approval..ApprovalForwardInfo 
									)							
							F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                            LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback  AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.LeaveApplication}   AND FeedbackSubmitDate IS NULL
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.LeaveApplication} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ELA.EmployeeLeaveAID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = ELA.EmployeeLeaveAID
									 LEFT JOIN LFADeclaration LFA ON ELA.EmployeeLeaveAID=LFA.EmployeeLeaveAID
							        {filter}
							GROUP BY         
								ELA.EmployeeLeaveAID,
                                ELA.LeaveCategoryID,
		                        SVLC.SystemVariableCode,
                                Emp.FullName,
                                Emp.EmployeeCode,RequestStartDate,RequestEndDate,NoOfLeaveDays,
								SVAS.SystemVariableCode,ELA.ApprovalStatusID,
								AP.ApprovalProcessID,AEF.APEmployeeFeedbackID,APForward.APForwardInfoID,
								LFADID,EditableCount,Rej.Cntr,ELA.CreatedDate,
                                ELA.CancellationStatus";
            var applications = EmployeeLeaveApplicationRepo.LoadGridModel(parameters, sql);

            //var testData = new EmailDtoCore {
            //    Subject="Test"
            //};
            //await Extension.Post<EmailDtoCore>($"/SendMail/SendEMailToRecipients", testData);
            return applications;
        }
        public GridModel GetAllLeaveApplicationListForDashboard(GridParameter parameters)
        {
            string sql = $@"SELECT 
                                DISTINCT
		                        ELA.EmployeeLeaveAID,
                                ELA.LeaveCategoryID LeaveCategoryID,
		                        SVLC.SystemVariableCode LeaveCategory,
                                Emp.FullName EmployeeName,
                                Emp.EmployeeCode EmployeeCode,
		                        --CONCAT(CONVERT(char(11), ELA.RequestStartDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) LeaveDates,
                                CASE WHEN ELA.LeaveCategoryID=69 THEN CONCAT(CONVERT(char(11), ELA.RequestEndDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) ELSE CONCAT(CONVERT(char(11), ELA.RequestStartDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) end LeaveDates,
		                        ELA.NoOfLeaveDays NumberOfLeave,
		                        CASE WHEN ELA.CancellationStatus=1 THEN 'Partially Cancelled'
								WHEN ELA.CancellationStatus=2 THEN 'Fully Cancelled'
								ELSE SVAS.SystemVariableCode  END AS ApprovalStatus,
                                ELA.ApprovalStatusID,
                                ISNULL(AP.ApprovalProcessID,0) ApprovalProcessID,
                                ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID,
								ISNULL(APForward.APForwardInfoID,0) APForwardInfoID,
                                ELA.CreatedDate,
								SVLC.SystemVariableCode + ( CASE WHEN ISNULL(LFA.LFADID,0) = 1 THEN ' (LFA Applied)'  ELSE '' END) LeaveType ,

                            CASE 
		                        WHEN LFA.LFADID IS NULL
			                        THEN CAST(0 AS BIT)
		                        ELSE CAST(1 AS BIT)
		                        END IsLFAApplied,
								CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee,
								CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,ISNULL((SELECT DATEDIFF(DAY, Emp.DateOfJoining, GETDATE())), 0) AS TotalEmployementDays
	                        FROM HRMS.dbo.EmployeeLeaveApplication ELA
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee Emp on Emp.EmployeeID=ELA.EmployeeID
	                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVLC ON ELA.LeaveCategoryID=SVLC.SystemVariableID
	                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVAS ON ELA.ApprovalStatusID=SVAS.SystemVariableID
							LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = ELA.EmployeeLeaveAID AND AP.APTypeID =1
                            LEFT JOIN (SELECT 
				                            APEmployeeFeedbackID,ApprovalProcessID,IsEditable 
			                            FROM 
				                            Approval..ApprovalEmployeeFeedback AEF
			                            WHERE (AEF.APFeedbackID = 2 OR AEF.APFeedbackID = 8 OR AEF.APFeedbackID = 9) AND (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})
                              UNION ALL 
										SELECT 
				                            EmployeeFeedbackID,ApprovalProcessID,0 IsEditable 
			                            FROM 
				                            Approval..ApprovalForwardInfo  
			                            WHERE 
				                            EmployeeID = {AppContexts.User.EmployeeID} AND CommentSubmitDate IS NULL
                            )AEF ON AEF.ApprovalProcessID = AP.ApprovalProcessID
                            LEFT JOIN 
		                            (SELECT 
				                            APForwardInfoID,ApprovalProcessID 
			                            FROM 
				                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalForwardInfo  
			                            WHERE  CommentSubmitDate IS NULL) 
                            APForward ON APForward.ApprovalProcessID = AP.ApprovalProcessID
                            LEFT JOIN 
									(
									SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM  Approval..ApprovalEmployeeFeedback
									UNION ALL 
									SELECT DISTINCT ApprovalProcessID,EmployeeID,EmployeeID FROM Approval..ApprovalForwardInfo 
									)							
							F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                            LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback  AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.LeaveApplication}   AND FeedbackSubmitDate IS NULL
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.LeaveApplication} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ELA.EmployeeLeaveAID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = ELA.EmployeeLeaveAID
									 LEFT JOIN LFADeclaration LFA ON ELA.EmployeeLeaveAID=LFA.EmployeeLeaveAID
							WHERE ELA.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}
							";
            var applications = EmployeeLeaveApplicationRepo.LoadGridModel(parameters, sql);

            //var testData = new EmailDtoCore {
            //    Subject="Test"
            //};
            //await Extension.Post<EmailDtoCore>($"/SendMail/SendEMailToRecipients", testData);
            return applications;
        }
        public LFADeclaration GenerateLFADeclaration(LeaveApplication application)
        {
            var LFA = application.LFADeclaration.MapTo<LFADeclaration>();
            var existingLFA = LFADeclarationRepo.Entities.FirstOrDefault(x => x.EmployeeLeaveAID == application.EmployeeLeaveAID);
            if (application.IsLFA)
            {
                LFA.EmployeeLeaveAID = application.EmployeeLeaveAID;

                if (existingLFA.IsNull() && LFA.IsNotNull())
                {
                    LFA.SetAdded();
                    SetNewLFAID(LFA);
                }
                else if (existingLFA.IsNotNull() && LFA.IsNotNull())
                {
                    LFA.CreatedBy = existingLFA.CreatedBy;
                    LFA.CreatedDate = existingLFA.CreatedDate;
                    LFA.CreatedIP = existingLFA.CreatedIP;
                    LFA.RowVersion = existingLFA.RowVersion;
                    LFA.SetModified();
                }
            }
            else
            {
                if (existingLFA.IsNotNull())
                {
                    existingLFA.SetDeleted();
                    return existingLFA;
                }
            }
            return LFA;
        }
        public async Task<LeavePolicySettingsDto> GetLeavePolicySettings(int leaveCategoryId)
        {
            string sql = @$"SELECT  SV.SystemVariableCode LeaveCategoryName
											,SV.SystemVariableID LeaveCategoryID
											,LPS.*
                                            ,J.JobGradeName
                                            ,E.FullName EmployeeName
											,(
												 SELECT EmployeeCode+'-'+FullName label, EmployeeID value from Employee
												 WHERE EmployeeID IN (Select * from dbo.fnReturnStringArray(LPS.ProxyEmployeeIDs,','))
												 FOR JSON PATH
											) AS ProxyEmployeeStr 
                                FROM Security..SystemVariable SV
                                LEFT JOIN LeavePolicySettings LPS ON LPS.LeaveCategoryID = SV.SystemVariableID
                                LEFT JOIN Employee E ON E.EmployeeID=LPS.EmployeeID
                                LEFT JOIN JobGrade J ON J.JobGradeID=LPS.MaximumJobGrade
                                WHERE SV.SystemVariableID={leaveCategoryId}";
            var setting = Task.Run(() => LeavePolicySettingsRepo.GetModelData<LeavePolicySettingsDto>(sql));

            //var setting = Task.Run(() => LeavePolicySettingsRepo.Entities.Where(x => x.LeaveCategoryID == leaveCategoryId).SingleOrDefault().MapTo<LeavePolicySettingsDto>());
            return await setting;
        }
        public async Task<(bool, string)> SavePolicySettings(LeavePolicySettingsDto settings)
        {
            using var unitOfWork = new UnitOfWork();
            var existingSettings = LeavePolicySettingsRepo.Entities.SingleOrDefault(x => x.LPSID == settings.LPSID);
            settings.EmployeeTypes = String.Join(",", settings.EmployeeTypeList);
            if (settings.IncludeHRForLeave == false)
            {
                settings.ApplicableToHRForDays = 0;
            }
            if (settings.IncludeHRForLeave == false && settings.IncludeHRForLFA == false && settings.IncludeHRForFestival == false)
            {
                settings.EmployeeID = 0;
                settings.ProxyEmployeeID = null;
            }

            if (existingSettings.IsNull() || existingSettings.LPSID.IsZero())
            {
                settings.SetAdded();
                SetNewLPolicySettingID(settings);
            }
            else
            {
                settings.SetModified();
            }
            var settingsEnt = settings.MapTo<LeavePolicySettings>();
            settingsEnt.CompanyID = settings.CompanyID ?? AppContexts.User.CompanyID;

            LeavePolicySettingsRepo.Add(settingsEnt);
            await Task.Run(() => unitOfWork.CommitChangesWithAudit());

            return (true, $"Settings Saved Successfully");

        }
        private void SetNewLPolicySettingID(LeavePolicySettingsDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("LeavePolicySettings", AppContexts.User.CompanyID);
            obj.LPSID = code.MaxNumber;
        }
        public bool CheckMultipleSupervisor(int EmployeeID)
        {
            bool isMultiple = false;
            var supervisors = EmployeeSupervisorMapRepo.Entities.Where(x => x.EmployeeID == AppContexts.User.EmployeeID && x.IsCurrent == true && x.SupervisorType == (int)Util.SupervisorType.Regular).ToList();
            isMultiple = supervisors.Count > 1 ? true : false;
            return isMultiple;
        }
        public async Task<List<LeavePolicySettingsDto>> GetLeaveCategoriesWithSettings(int EmployeeLeaveAID = 0)
        {

            string employementSql = $@"SELECT *  FROM HRMS..ViewALLEmployee WHERE EmployeeID = {AppContexts.User.EmployeeID}";
            var employement = EmploymentRepo.GetData(employementSql);
            string employeeTypeId = employement["EmployeeTypeID"].ToString();
            string employeeId = employement["EmployeeID"].ToString();
            DateTime joiningDate = string.IsNullOrEmpty(employement["DateOfJoining"].ToString()) ? DateTime.Today : DateTime.Parse(employement["DateOfJoining"].ToString());
            int tanureInMonth = Util.GetTotalMonthsFrom(joiningDate, DateTime.Today);
            string genderId = employement["GenderID"].ToString();

            var sql = @$"SELECT SV.SystemVariableCode AS LeaveCategoryName
                            	,LPS.LPSID
                            	,SV.SystemVariableID LeaveCategoryID
                            	,LPS.MinimumDays
                            	,LPS.MaximumDays
                            	,LPS.DayType
                            	,LPS.EmployeeTypeException
                            	,LPS.EmployeeTypes
                            	,LPS.TanureException
                            	,LPS.EligibilityInMonths
                            	,LPS.IsHolidayInclusive
                            	,LPS.IsCarryForwardable
                            	,LPS.MaximumAccumulationDays
                            	,LPS.IsAttachemntRequired
                            	,LPS.WillApplicableFrom
                        FROM Security.dbo.SystemVariable AS SV
                        LEFT OUTER JOIN LeavePolicySettings AS LPS ON LPS.LeaveCategoryID = SV.SystemVariableID
						LEFT JOIN (SELECT COUNT(*) Cntr,LeaveCategoryID,EmployeeID,Year FROM HRMS..EmployeeLeaveApplication LA 
						LEFT JOIN Security..FinancialYear FY ON FY.FinancialYearID = LA.FinancialYearID where LeaveCategoryID={(int)Util.LeaveCategory.Pilgrim} and ApprovalStatusID <> {(int)Util.ApprovalStatus.Rejected}
                        AND EmployeeLeaveAID<>{EmployeeLeaveAID} 
                        group by LeaveCategoryID, EmployeeID, Year) A ON A.LeaveCategoryID = SV.SystemVariableID and A.EmployeeID = {employeeId}
                            WHERE (SV.EntityTypeID = 10)
                            AND (SV.SystemVariableID <> (CASE WHEN {genderId} = {(int)Util.Gender.Male} THEN  65 WHEN  {genderId} = {(int)Util.Gender.Female}  THEN 142 END)) AND SV.SystemVariableID <> CASE WHEN A.LeaveCategoryID > 0 THEN A.LeaveCategoryID Else 0 END
    	                        AND {employeeTypeId} NOT IN (
		                        SELECT _ID
		                        FROM dbo.fnReturnStringArray(LPS.EmployeeTypes, ',')
		                        ) AND ((ISNULL(LPS.TanureException,0)=1 AND ISNULL(LPS.EligibilityInMonths,0)<={tanureInMonth}) OR ISNULL(LPS.TanureException,0)=0)";
            var data = Task.Run(() => LeavePolicySettingsRepo.GetDataModelCollection<LeavePolicySettingsDto>(sql));
            return await data;
        }
        public async Task<NotificationResponseDto> CancelLeaveApplication(LeaveApplication application)
        {
            
            //await GetApprovalEmployees(application.EmployeeLeaveAID, (int)Util.ApprovalType.LeaveApplication);
            var existingApplication = EmployeeLeaveApplicationRepo.Entities.Where(x => x.EmployeeLeaveAID == application.EmployeeLeaveAID).SingleOrDefault();
            string cancellationMessage =
    $@"Leave Cancellation: {(existingApplication.NoOfLeaveDays > 1 ?
        $"{existingApplication.RequestStartDate:dd-MMM-yyyy} to {existingApplication.RequestEndDate:dd-MMM-yyyy}" :
        existingApplication.RequestStartDate.ToString("dd-MMM-yyyy"))} | Duration: {existingApplication.NoOfLeaveDays} day{(existingApplication.NoOfLeaveDays != 1 ? "s" : "")}";
            NotificationResponseDto response = new NotificationResponseDto()
            {
                Success = true,
                EmployeeIds = @$"{existingApplication.EmployeeID},{AppContexts.User.EmployeeID}",
                NotificationMessage = cancellationMessage,
                CurrentNotificaitonEmplyeeID = existingApplication.EmployeeID.ToString(),
                Message= "Successfully Canceled Leave Application",
            };
            // string Employeeids = ;
            int cancellationStatus = application.LeaveDetails.Count() == application.LeaveDetails.Where(y => y.IsCancel == true).Count() ? (int)CancellationStatus.FullyCancelled : (int)CancellationStatus.PartiallyCancelled;

            existingApplication.NoOfLeaveDays = (decimal)(application.LeaveDetails.Where(y => y.IsCancel == false).Count(x => x.DayStatus == Util.Fullday) * 1 + application.LeaveDetails.Where(y => y.IsCancel == false).Count(x => x.DayStatus == Util.Halfday || x.DayStatus == Util.OnlyHalfday || x.DayStatus == Util.FirstHalfday || x.DayStatus == Util.SecondHalfday) * 0.5);
            existingApplication.CancellationStatus = cancellationStatus;
            existingApplication.Remarks = application.Remarks;
            existingApplication.ApprovalStatusID = cancellationStatus == (int)CancellationStatus.FullyCancelled ? (int)ApprovalStatus.Rejected : existingApplication.ApprovalStatusID;
            existingApplication.CancelledBy = AppContexts.User.UserID;

            var leaveBreakdown = (from entBreakDown in EmployeeLeaveApplicationDayBreakDownRepo.GetAllList()
                                  join leaveDetails in application.LeaveDetails on entBreakDown.ELADBDID equals leaveDetails.ELADBDID
                                  where leaveDetails.IsCancel == true
                                  select entBreakDown).ToList();




            using (var unitOfWork = new UnitOfWork())
            {
                existingApplication.SetModified();
                leaveBreakdown.ToList().ForEach(x =>
                {
                    x.SetModified();
                    x.IsCancelled = true;
                });

                SetAuditFields(existingApplication);
                SetAuditFields(leaveBreakdown);
                EmployeeLeaveApplicationRepo.Add(existingApplication);
                EmployeeLeaveApplicationDayBreakDownRepo.AddRange(leaveBreakdown);
                unitOfWork.CommitChangesWithAudit();
                UpdateLeaveBalanceAfterSubmit(existingApplication.EmployeeID);
                await ScheduleManager.ExecuteSchedule(new ScheduleUtilityDto { ScheduleNo = 1, FromDate = existingApplication.RequestStartDate, ToDate = existingApplication.RequestEndDate });
            }



            return response;
        }
        public async Task<IEnumerable<Dictionary<string, object>>> GetLeaveDetailsForHr(int EmployeeID)
        {
            string financialYearSql = $@"SELECT FY.FinancialYearID, FY.Year, FY.YearDescription 
                              FROM {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FinancialYear FY
                              LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..EmployeeLeaveAccount ELA ON ELA.FinancialYearID = FY.FinancialYearID 
                              where ELA.EmployeeID = {EmployeeID}
                              ORDER BY FY.year DESC";
            IEnumerable<Dictionary<string, object>> financialYearData = EmployeeLeaveApplicationRepo.GetDataDictCollection(financialYearSql);

            string leaveBalanceSql = $@"SELECT 
                                        ELA.FinancialYearID,
	                                    ELA.EmployeeID,
	                                    ELA.LeaveCategoryID,
	                                    SystemVariableCode LeaveType,
	                                    LeaveDays,
	                                    ApprovedDays NoOfApprovedLeaveDays,
	                                    PendingDays NoOfPendingLeaveDays,
	                                    RemainingDays Balance,
	                                    PreviousLeaveDays
                                    FROM 
	                                    {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..EmployeeLeaveAccount ELA
	                                    LEFT JOIN 
		                                    (SELECT SystemVariableID, SystemVariableCode FROM {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable
		                                    ) SV ON ELA.LeaveCategoryID = SV.SystemVariableID
	                                    LEFT JOIN ViewALLEmployee Emp ON ELA.EmployeeID=Emp.EmployeeID
                                        LEFT JOIN Security..FinancialYear FYR ON FYR.FinancialYearID = ELA.FinancialYearID
                                        LEFT OUTER JOIN LeavePolicySettings LPS ON LPS.LeaveCategoryID = SV.SystemVariableID
	                                    LEFT JOIN (SELECT COUNT(*) Cntr,LeaveCategoryID,EmployeeID,Year FROM HRMS..EmployeeLeaveApplication LA 
					                    LEFT JOIN Security..FinancialYear FY ON FY.FinancialYearID = LA.FinancialYearID where LeaveCategoryID=68 and ApprovalStatusID <> 24
                                                   AND EmployeeLeaveAID<>0 
                                                   group by LeaveCategoryID, EmployeeID, Year) A ON A.LeaveCategoryID = ELA.LeaveCategoryID and A.EmployeeID = ELA.EmployeeID
	                                    WHERE (SV.SystemVariableID <> (CASE WHEN Emp.GenderID = 1 THEN  65 WHEN  Emp.GenderID = 2  THEN 142 END)) AND SV.SystemVariableID <> CASE WHEN A.LeaveCategoryID > 0 AND A.Year < FYR.Year THEN A.LeaveCategoryID Else 0 END
    	                                        AND Emp.EmployeeTypeID NOT IN (
		                                        SELECT _ID
		                                        FROM dbo.fnReturnStringArray(LPS.EmployeeTypes, ',')
		                                        ) AND ((ISNULL(LPS.TanureException,0)=1 AND ISNULL(LPS.EligibilityInMonths,0)<= DATEDIFF(MONTH, Emp.DateOfJoining, GETDATE())) OR ISNULL(LPS.TanureException,0)=0)
                                        AND ELA.EmployeeID={EmployeeID} ORDER BY ELA.LeaveCategoryID ASC";

            IEnumerable<Dictionary<string, object>> leaveBalanceData = EmployeeLeaveApplicationRepo.GetDataDictCollection(leaveBalanceSql).ToList();


            string totalLeaveSql = $@"SELECT ELA.FinancialYearID,ELA.LeaveCategoryID,SV.SystemVariableCode LeaveType ,ELA.ApplicationDate ,ELA.RequestStartDate, ELA.RequestEndDate ,ELA.NoOfLeaveDays 
                                    ,ELA.ApprovalStatusID, SVAS.SystemVariableCode ApprovalStatus, ELA.EmployeeLeaveAID, AP.ApprovalProcessID	
                        FROM hrms..EmployeeLeaveApplication ELA 
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = ELA.LeaveCategoryID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVAS ON ELA.ApprovalStatusID=SVAS.SystemVariableID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = ELA.EmployeeLeaveAID AND AP.APTypeID =1
						WHERE ELA.EmployeeID ={EmployeeID} order by ELA.ApplicationDate asc";
            IEnumerable<Dictionary<string, object>> totalLeaveData = EmployeeLeaveApplicationRepo.GetDataDictCollection(totalLeaveSql);

            string unauthorizeLeaveSql = $@"SELECT fy.FinancialYearID
	                                            ,ats.AttendanceDate
	                                            ,1 NoOfLeaveDays
	                                            ,'Absent' LeaveType
	                                            --,CASE WHEN YEAR(DateOfJoining) = fy.year then Cast(1 as bit) ELSE cast(0 as bit) END IsSameYear
	                                            --,CASE WHEN YEAR(DiscontinueDate) = fy.year then Cast(1 as bit) ELSE cast(0 as bit) END IsDiscontinued
	                                            ,DiscontinueDate
                                            FROM HRMS..attendancesummary ATS
                                            LEFT JOIN (
                                            SELECT EmployeeID,FullName,DiscontinueDate,DATEDIFF(MONTH,DateOfJoining,ISNULL(DiscontinueDate, GETDATE())) AgeInMonth
	                                            ,YEAR(DateOfJoining) DateOfJoingYear
	                                            ,DateOfJoining	
	                                            ,Case when DiscontinueDate IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END IsDiscontinued
                                            FROM HRMS..ViewALLEmployee 
                                            ) VAE ON VAE.EmployeeID = ATS.EmployeeID
                                            LEFT JOIN (
	                                            SELECT ELA.employeeleaveaid
		                                            ,halforfullday
		                                            ,requestdate
		                                            ,ELA.requeststartdate
		                                            ,ELA.requestenddate
		                                            ,EmployeeID
		                                            ,ELADB.noofleavedays PendingNoOfLeaveDays
		
	                                            FROM HRMS..employeeleaveapplicationdaybreakdown ELADB
	                                            INNER JOIN HRMS..employeeleaveapplication ELA ON ELA.employeeleaveaid = ELADB.employeeleaveaid
	                                            WHERE approvalstatusid = {(int)Util.ApprovalStatus.Pending}
		                                            AND ELADB.noofleavedays > 0
		                                            AND ELADB.iscancelled = 0
		                                            AND ELA.EmployeeID = {EmployeeID}
	                                            ) PendingLeave ON PendingLeave.employeeid = ATS.employeeid
	                                            AND Cast(ATS.attendancedate AS DATE) = Cast(PendingLeave.requestdate AS DATE)
                                            LEFT JOIN (
                                            SELECT fyr.FinancialYearID,Year,YearDescription,Min(PeriodStartDate) StartDate,Max(PeriodStartDate) EndDate
                                            FROM 
                                            Security..financialyear fyr
                                            INNER JOIN Security..Period P on p.FinancialYearID = fyr.FinancialYearID
                                            GROUP BY Year,YearDescription,fyr.FinancialYearID

                                            ) fy ON fy.Year = Year(ats.attendancedate)
                                            WHERE attendancestatus = {(int)Util.AttendanceStatus.Absent} and PendingLeave.EmployeeID IS NULL
	                                            AND ATS.employeeid = {EmployeeID}
	                                            --AND ATS.AttendanceDate < ISNULL(VAE.DiscontinueDate, GETDATE())
	                                            --AND ATS.AttendanceDate BETWEEN VAE.DateOfJoining AND ISNULL(VAE.DiscontinueDate, GETDATE())
	                                            AND ATS.AttendanceDate between (CASE WHEN YEAR(DateOfJoining) = fy.year THEN DateOfJoining else fy.StartDate END) AND (CASE WHEN IsDiscontinued = 1 THEN DiscontinueDate ELSE FY.EndDate END)";

            IEnumerable<Dictionary<string, object>> unauthorizeLeaveSqlLeaveData = EmployeeLeaveApplicationRepo.GetDataDictCollection(unauthorizeLeaveSql);




            var result = financialYearData.Select(financialYear => new Dictionary<string, object>
            {
                {"FinancialYearID", financialYear["FinancialYearID"]},
                {"Year", financialYear["Year"]},
                {"YearDescription", financialYear["YearDescription"]},
                {"ChildList", leaveBalanceData.Where(leaveBalance => leaveBalance["FinancialYearID"].ToString() == financialYear["FinancialYearID"].ToString())
                                               .Select(leaveBalance => new Dictionary<string, object>
                                               {
                                                   {"LeaveCategoryID", leaveBalance["LeaveCategoryID"]},
                                                   {"LeaveType", leaveBalance["LeaveType"]},
                                                   {"LeaveDays", leaveBalance["LeaveDays"]},
                                                   {"NoOfApprovedLeaveDays", leaveBalance["NoOfApprovedLeaveDays"]},
                                                   {"NoOfPendingLeaveDays", leaveBalance["NoOfPendingLeaveDays"]},
                                                   {"Balance", leaveBalance["Balance"]},
                                                   {"PreviousLeaveDays", leaveBalance["PreviousLeaveDays"]}
                                               }).ToList()
                },
                {"Leaves", totalLeaveData.Where(totalLeave => totalLeave["FinancialYearID"].ToString() == financialYear["FinancialYearID"].ToString())
                                               .Select(AnnualLeave => new Dictionary<string, object>
                                               {
                                                   {"LeaveCategoryID", AnnualLeave["LeaveCategoryID"]},
                                                   {"LeaveType", AnnualLeave["LeaveType"]},
                                                   {"ApplicationDate", AnnualLeave["ApplicationDate"]},
                                                   {"RequestStartDate", AnnualLeave["RequestStartDate"]},
                                                   {"RequestEndDate", AnnualLeave["RequestEndDate"]},
                                                   {"NoOfLeaveDays", AnnualLeave["NoOfLeaveDays"]},
                                                   {"ApprovalStatus", AnnualLeave["ApprovalStatus"]},
                                                   {"EmployeeLeaveAID", AnnualLeave["EmployeeLeaveAID"]},
                                                   {"ApprovalProcessID", AnnualLeave["ApprovalProcessID"]}
                                               }).ToList()
                },
                {"UnauthorizeLeaves", unauthorizeLeaveSqlLeaveData.Where(unauthorizeLeave => unauthorizeLeave["FinancialYearID"].ToString() == financialYear["FinancialYearID"].ToString())
                                               .Select(UnauthorizeLeave => new Dictionary<string, object>
                                               {
                                                   {"LeaveType", UnauthorizeLeave["LeaveType"]},
                                                   {"AttendanceDate", UnauthorizeLeave["AttendanceDate"]},
                                                   {"NoOfLeaveDays", UnauthorizeLeave["NoOfLeaveDays"]}
                                               }).ToList()
                }
            }

                    ).ToList();

            return result;
        }
        public async Task<IEnumerable<Dictionary<string, object>>> GetHolidaysWorkDetails()
        {
            string sql = $@"SELECT TOP 30		
                              FORMAT(AttendanceDate,'yyyy-dd-MM HH:MM:ss') AttendanceDateNew,
                              AttendanceDate,
	                          ATS.EmployeeCode
	                          ,ISNULL(CONVERT(VARCHAR(10), CAST(InTime AS TIME), 0),'') IN_TIME
	                          ,ISNULL(CONVERT(VARCHAR(10), CAST(OutTime AS TIME), 0),'') OUT_TIME	                       
						      ,SV.SystemVariableDescription ATTENDANCE_STATUS
						      ,IIF(TotalTimeInMin = 0,'00:00',dbo.MinutesToDuration(TotalTimeInMin)) TotalTime
	                          ,ISNULL(DayStatus,'') DAY_STATUS
						      ,AttendanceStatus
                              ,ATS.PrimaryID
                              ,HRMS.dbo.MinutesToDuration(ActualWorkingHourInMin) ActualWorkingHour
                              ,FORMAT(AttendanceDate, 'ddd') AttendanceDay
                              ,ISNULL(H.IsFestivalHoliday, 0) IsFestivalHoliday
                              ,ELA.RequestStartDate
                              ,LPS.MaximumDays
                          FROM AttendanceSummary ATS
                          INNER JOIN HRMS..Employee Emp ON Emp.EmployeeID = ATS.EmployeeID
					      INNER JOIN Security..SystemVariable SV ON SV.SystemVariableID = ATS.AttendanceStatus
                          LEFT JOIN HRMS..Holiday H ON H.HolidayDate = ATS.AttendanceDate
                          LEFT JOIN HRMS..EmployeeLeaveApplication ELA ON CAST(ELA.RequestStartDate AS DATE) = CAST(ATS.AttendanceDate AS DATE) AND Emp.EmployeeID = ELA.EmployeeID AND ELA.ApprovalStatusID <> {(int)Util.ApprovalStatus.Rejected}
                          LEFT JOIN HRMS..LeavePolicySettings LPS ON LPS.LeaveCategoryID = {(int)Util.LeaveCategory.Compensatory}
                          WHERE Emp.EmployeeID = {AppContexts.User.EmployeeID}  AND AttendanceStatus IN {((int)Util.AttendanceStatus.WeekendPresent, (int)Util.AttendanceStatus.WeekendLate, (int)Util.AttendanceStatus.HolidayPresent, (int)Util.AttendanceStatus.HolidayLate)}
                                AND AttendanceDate >= DATEADD(DAY, -LPS.MinimumDays , GETDATE())
                                AND TotalTimeInMin >= 360
                                ORDER BY ATS.AttendanceDate DESC";
            var list = EmployeeRepo.GetDataDictCollection(sql);
            return await Task.FromResult(list);
        }
        public async Task<IEnumerable<Dictionary<string, object>>> GetHolidaysWorkDetailsAll()
        {

            string sql = $@"SELECT TOP 30 *
                            FROM (
                                SELECT
                                    FORMAT(AttendanceDate,'yyyy-dd-MM HH:MM:ss') AttendanceDateNew,
                                    AttendanceDate,
                                    ATS.EmployeeCode,
                                    ISNULL(CONVERT(VARCHAR(10), CAST(InTime AS TIME), 0),'') IN_TIME,
                                    ISNULL(CONVERT(VARCHAR(10), CAST(OutTime AS TIME), 0),'') OUT_TIME,
                                    SV.SystemVariableDescription ATTENDANCE_STATUS,
                                    IIF(TotalTimeInMin = 0,'00:00',dbo.MinutesToDuration(TotalTimeInMin)) TotalTime,
                                    ISNULL(DayStatus,'') DAY_STATUS,
                                    AttendanceStatus,
                                    ATS.PrimaryID,
                                    HRMS.dbo.MinutesToDuration(ActualWorkingHourInMin) ActualWorkingHour,
                                    FORMAT(AttendanceDate, 'ddd') AttendanceDay,
                                    ISNULL(H.IsFestivalHoliday, 0) IsFestivalHoliday,
                                    ELA.RequestStartDate,
                                    LPS.MaximumDays,
                                    FYR.year
                                FROM AttendanceSummary ATS
                                INNER JOIN HRMS..Employee Emp ON Emp.EmployeeID = ATS.EmployeeID
	                            INNER JOIN Security..SystemVariable SV ON SV.SystemVariableID = ATS.AttendanceStatus
                                LEFT JOIN HRMS..Holiday H ON H.HolidayDate = ATS.AttendanceDate
                                LEFT JOIN HRMS..EmployeeLeaveApplication ELA ON CAST(ELA.RequestStartDate AS DATE) = CAST(ATS.AttendanceDate AS DATE) AND Emp.EmployeeID = ELA.EmployeeID AND ELA.ApprovalStatusID <> {(int)Util.ApprovalStatus.Rejected}
                                LEFT JOIN HRMS..LeavePolicySettings LPS ON LPS.LeaveCategoryID = {(int)Util.LeaveCategory.Compensatory}
                                LEFT JOIN Security..FinancialYear FYR ON FYR.IsCurrent=1
                                WHERE Emp.EmployeeID = {AppContexts.User.EmployeeID} AND AttendanceStatus IN {((int)Util.AttendanceStatus.WeekendPresent, (int)Util.AttendanceStatus.WeekendLate, (int)Util.AttendanceStatus.HolidayPresent, (int)Util.AttendanceStatus.HolidayLate)}
                                AND (
                                    (AttendanceDate < DATEADD(DAY, -LPS.MinimumDays, GETDATE()) AND RequestStartDate IS NOT NULL AND FYR.Year = FORMAT(RequestStartDate,'yyyy'))
                                    OR
                                    (AttendanceDate >= DATEADD(DAY, -LPS.MinimumDays, GETDATE()))
                                )
                                AND TotalTimeInMin >= 360
                            ) AS CombinedResult
                            ORDER BY AttendanceDate DESC";

            var list = EmployeeRepo.GetDataDictCollection(sql);
            return await Task.FromResult(list);
        }
        public async Task<IEnumerable<Dictionary<string, object>>> GetHolidaysWorkDetailsById(int EmployeeLeaveAID)
        {
            string sql = $@"SELECT TOP 30		
                              FORMAT(AttendanceDate,'yyyy-dd-MM HH:MM:ss') AttendanceDateNew,
                              AttendanceDate,
	                          ATS.EmployeeCode
	                          ,ISNULL(CONVERT(VARCHAR(10), CAST(InTime AS TIME), 0),'') IN_TIME
	                          ,ISNULL(CONVERT(VARCHAR(10), CAST(OutTime AS TIME), 0),'') OUT_TIME	                       
						      ,SV.SystemVariableDescription ATTENDANCE_STATUS
						      ,IIF(TotalTimeInMin = 0,'00:00',dbo.MinutesToDuration(TotalTimeInMin)) TotalTime
	                          ,ISNULL(DayStatus,'') DAY_STATUS
						      ,AttendanceStatus
                              ,ATS.PrimaryID
                              ,HRMS.dbo.MinutesToDuration(ActualWorkingHourInMin) ActualWorkingHour
                              ,FORMAT(AttendanceDate, 'ddd') AttendanceDay
                              ,ISNULL(H.IsFestivalHoliday, 0) IsFestivalHoliday
                              ,ELA.RequestStartDate
                              ,ELA.EmployeeLeaveAID
                              ,LPS.MaximumDays
                          FROM AttendanceSummary ATS
                          INNER JOIN HRMS..Employee Emp ON Emp.EmployeeID = ATS.EmployeeID
					      INNER JOIN Security..SystemVariable SV ON SV.SystemVariableID = ATS.AttendanceStatus
                          LEFT JOIN HRMS..Holiday H ON H.HolidayDate = ATS.AttendanceDate
                          LEFT JOIN HRMS..EmployeeLeaveApplication ELA ON CAST(ELA.RequestStartDate AS DATE) = CAST(ATS.AttendanceDate AS DATE) AND Emp.EmployeeID = ELA.EmployeeID --AND ELA.ApprovalStatusID <> {(int)Util.ApprovalStatus.Rejected}
                          LEFT JOIN HRMS..LeavePolicySettings LPS ON LPS.LeaveCategoryID = {(int)Util.LeaveCategory.Compensatory}
                          WHERE ELA.EmployeeLeaveAID = {EmployeeLeaveAID}  AND AttendanceStatus IN {((int)Util.AttendanceStatus.WeekendPresent, (int)Util.AttendanceStatus.WeekendLate, (int)Util.AttendanceStatus.HolidayPresent, (int)Util.AttendanceStatus.HolidayLate)}
                                --AND AttendanceDate >= DATEADD(DAY, - LPS.MinimumDays, GETDATE())
                                AND TotalTimeInMin >= 360
                                ORDER BY ATS.AttendanceDate DESC";
            var list = EmployeeRepo.GetDataDictCollection(sql);
            return await Task.FromResult(list);
        }
        public async Task<IEnumerable<Dictionary<string, object>>> GetHolidaysWorkDetailsAllById(int EmployeeID)
        {
            string sql = $@"SELECT TOP 30		
                              FORMAT(AttendanceDate,'yyyy-dd-MM HH:MM:ss') AttendanceDateNew,
                              AttendanceDate,
	                          ATS.EmployeeCode
	                          ,ISNULL(CONVERT(VARCHAR(10), CAST(InTime AS TIME), 0),'') IN_TIME
	                          ,ISNULL(CONVERT(VARCHAR(10), CAST(OutTime AS TIME), 0),'') OUT_TIME	                       
						      ,SV.SystemVariableDescription ATTENDANCE_STATUS
						      ,IIF(TotalTimeInMin = 0,'00:00',dbo.MinutesToDuration(TotalTimeInMin)) TotalTime
	                          ,ISNULL(DayStatus,'') DAY_STATUS
						      ,AttendanceStatus
                              ,ATS.PrimaryID
                              ,HRMS.dbo.MinutesToDuration(ActualWorkingHourInMin) ActualWorkingHour
                              ,FORMAT(AttendanceDate, 'ddd') AttendanceDay
                              ,ISNULL(H.IsFestivalHoliday, 0) IsFestivalHoliday
                              ,ELA.RequestStartDate
                              ,ELA.EmployeeLeaveAID
                              ,LPS.MaximumDays
                          FROM AttendanceSummary ATS
                          INNER JOIN HRMS..Employee Emp ON Emp.EmployeeID = ATS.EmployeeID
					      INNER JOIN Security..SystemVariable SV ON SV.SystemVariableID = ATS.AttendanceStatus
                          LEFT JOIN HRMS..Holiday H ON H.HolidayDate = ATS.AttendanceDate
                          LEFT JOIN HRMS..EmployeeLeaveApplication ELA ON CAST(ELA.RequestStartDate AS DATE) = CAST(ATS.AttendanceDate AS DATE) AND Emp.EmployeeID = ELA.EmployeeID AND ELA.ApprovalStatusID <> {(int)Util.ApprovalStatus.Rejected}
                          LEFT JOIN HRMS..LeavePolicySettings LPS ON LPS.LeaveCategoryID = {(int)Util.LeaveCategory.Compensatory}
                          LEFT JOIN Security..FinancialYear FYR ON FYR.IsCurrent=1  
                          WHERE Emp.EmployeeID = {EmployeeID}  AND AttendanceStatus IN {((int)Util.AttendanceStatus.WeekendPresent, (int)Util.AttendanceStatus.WeekendLate, (int)Util.AttendanceStatus.HolidayPresent, (int)Util.AttendanceStatus.HolidayLate)}
                                AND (
									(AttendanceDate < DATEADD(DAY, -LPS.MinimumDays, GETDATE()) AND RequestStartDate IS NOT NULL AND FYR.Year = FORMAT(RequestStartDate,'yyyy'))
									OR
									(AttendanceDate >= DATEADD(DAY, -LPS.MinimumDays, GETDATE()))
									)
                                AND TotalTimeInMin >= 360
                                ORDER BY ATS.AttendanceDate DESC";
            var list = EmployeeRepo.GetDataDictCollection(sql);
            return await Task.FromResult(list);
        }
        public async Task<IEnumerable<Dictionary<string, object>>> GetUnauthorizedLeave(DateTime fromDate, DateTime toDate, int DivisionID, int DepartmentID)
        {
            string filter = string.Empty;

            if (DivisionID > 0 && DepartmentID > 0)
            {
                filter = @$"DivisionID={DivisionID} AND DepartmentID={DepartmentID}";
            }
            else if (DivisionID > 0)
            {
                filter = @$"DivisionID={DivisionID}";
            }

            filter = filter.IsNotNullOrEmpty() ? @$"AND {filter}" : "";


            string unauthorizeLeaveSql = $@"select A.EmployeeID, A.EmployeeCode, A.FullName,A.DivisionName,A.DepartmentName,
							STRING_AGG(
									CONVERT(NVARCHAR(12), DATEPART(DAY, A.AttendanceDate)) + ' ' +
									FORMAT(A.AttendanceDate, 'MMM, yyyy'), '| '
								) AS AttendanceDates
							,COUNT(*) AS GroupCount
                            from 	
								(SELECT											
								    ats.EmployeeID,
								    ats.EmployeeCode,
									VAE.FullName,
									cast(ats.AttendanceDate as date) AttendanceDate,
	                                1 NoOfLeaveDays,
                                    VAE.DivisionName,
									VAE.DepartmentName
	                                         
                                FROM HRMS..attendancesummary ATS
                                INNER JOIN (
                                SELECT EmployeeID,FullName,DiscontinueDate,DATEDIFF(MONTH,DateOfJoining,ISNULL(DiscontinueDate, GETDATE())) AgeInMonth
	                                ,YEAR(DateOfJoining) DateOfJoingYear
	                                ,DateOfJoining	
	                                ,Case when DiscontinueDate IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END IsDiscontinued
                                    ,DivisionName
									,DepartmentName
                                    ,DivisionID
									,DepartmentID
                                FROM HRMS..viewActiveEmployee 
                                ) VAE ON VAE.EmployeeID = ATS.EmployeeID
                                LEFT JOIN (
	                                SELECT ELA.employeeleaveaid
		                                ,halforfullday
		                                ,requestdate
		                                ,ELA.requeststartdate
		                                ,ELA.requestenddate
		                                ,EmployeeID
		                                ,ELADB.noofleavedays PendingNoOfLeaveDays
		
	                                FROM HRMS..employeeleaveapplicationdaybreakdown ELADB
	                                INNER JOIN HRMS..employeeleaveapplication ELA ON ELA.employeeleaveaid = ELADB.employeeleaveaid
	                                WHERE approvalstatusid = {(int)Util.ApprovalStatus.Pending}
		                                AND ELADB.noofleavedays > 0
		                                AND ELADB.iscancelled = 0
	                                ) PendingLeave ON PendingLeave.employeeid = ATS.employeeid
	                                AND Cast(ATS.attendancedate AS DATE) = Cast(PendingLeave.requestdate AS DATE)
                                LEFT JOIN (
                                SELECT fyr.FinancialYearID,Year,YearDescription,Min(PeriodStartDate) StartDate,Max(PeriodStartDate) EndDate
                                FROM 
                                Security..financialyear fyr
                                INNER JOIN Security..Period P on p.FinancialYearID = fyr.FinancialYearID
                                GROUP BY Year,YearDescription,fyr.FinancialYearID

                                ) fy ON fy.Year = Year(ats.attendancedate)
                                WHERE attendancestatus = {(int)Util.AttendanceStatus.Absent} and PendingLeave.EmployeeID IS NULL
	                                AND ATS.AttendanceDate between '{fromDate}' AND '{toDate}' {filter}) A
								GROUP BY EmployeeID, EmployeeCode, FullName,DivisionName,DepartmentName
								ORDER BY EmployeeID";

            IEnumerable<Dictionary<string, object>> unauthorizeLeaveSqlLeaveData = EmployeeLeaveApplicationRepo.GetDataDictCollection(unauthorizeLeaveSql);

            return unauthorizeLeaveSqlLeaveData;
        }
        public async Task<IEnumerable<Dictionary<string, object>>> GetUnauthorizedLeaveHr(int employeeID)
        {
            string unauthorizeLeaveSql = $@"SELECT fy.FinancialYearID
	                                            ,ats.AttendanceDate
	                                            ,1 NoOfLeaveDays
	                                            ,'Absent' LeaveType
	                                            --,CASE WHEN YEAR(DateOfJoining) = fy.year then Cast(1 as bit) ELSE cast(0 as bit) END IsSameYear
	                                            --,CASE WHEN YEAR(DiscontinueDate) = fy.year then Cast(1 as bit) ELSE cast(0 as bit) END IsDiscontinued
	                                            ,DiscontinueDate
                                            FROM HRMS..attendancesummary ATS
                                            LEFT JOIN (
                                            SELECT EmployeeID,FullName,DiscontinueDate,DATEDIFF(MONTH,DateOfJoining,ISNULL(DiscontinueDate, GETDATE())) AgeInMonth
	                                            ,YEAR(DateOfJoining) DateOfJoingYear
	                                            ,DateOfJoining	
	                                            ,Case when DiscontinueDate IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END IsDiscontinued
                                            FROM HRMS..ViewALLEmployee 
                                            ) VAE ON VAE.EmployeeID = ATS.EmployeeID
                                            LEFT JOIN (
	                                            SELECT ELA.employeeleaveaid
		                                            ,halforfullday
		                                            ,requestdate
		                                            ,ELA.requeststartdate
		                                            ,ELA.requestenddate
		                                            ,EmployeeID
		                                            ,ELADB.noofleavedays PendingNoOfLeaveDays
		
	                                            FROM HRMS..employeeleaveapplicationdaybreakdown ELADB
	                                            INNER JOIN HRMS..employeeleaveapplication ELA ON ELA.employeeleaveaid = ELADB.employeeleaveaid
	                                            WHERE approvalstatusid = {(int)Util.ApprovalStatus.Pending}
		                                            AND ELADB.noofleavedays > 0
		                                            AND ELADB.iscancelled = 0
		                                            AND ELA.EmployeeID = {employeeID}
	                                            ) PendingLeave ON PendingLeave.employeeid = ATS.employeeid
	                                            AND Cast(ATS.attendancedate AS DATE) = Cast(PendingLeave.requestdate AS DATE)
                                            LEFT JOIN (
                                            SELECT fyr.FinancialYearID,Year,YearDescription,Min(PeriodStartDate) StartDate,Max(PeriodStartDate) EndDate
                                            FROM 
                                            Security..financialyear fyr
                                            INNER JOIN Security..Period P on p.FinancialYearID = fyr.FinancialYearID
                                            Where fyr.IsCurrent=1
                                            GROUP BY Year,YearDescription,fyr.FinancialYearID

                                            ) fy ON fy.Year = Year(ats.attendancedate)
                                            WHERE attendancestatus = {(int)Util.AttendanceStatus.Absent} and PendingLeave.EmployeeID IS NULL
	                                            AND ATS.employeeid = {employeeID}
	                                            AND ATS.AttendanceDate between (CASE WHEN YEAR(DateOfJoining) = fy.year THEN DateOfJoining else fy.StartDate END) AND (CASE WHEN IsDiscontinued = 1 THEN DiscontinueDate ELSE FY.EndDate END) ORDER BY ATS.AttendanceDate ASC";

            IEnumerable<Dictionary<string, object>> unauthorizeLeaveSqlLeaveData = EmployeeLeaveApplicationRepo.GetDataDictCollection(unauthorizeLeaveSql);

            return unauthorizeLeaveSqlLeaveData;
        }
        public async Task<IEnumerable<Dictionary<string, object>>> GetUnauthorizedLeaveViewHr(int EmployeeLeaveAID)
        {
            string unauthorizeLeaveSql = $@"											SELECT
											fy.FinancialYearID
											,requestdate AttendanceDate
	                                        ,1 NoOfLeaveDays
	                                        ,'Leave' LeaveType
											,null DiscontinueDate

	                                        FROM HRMS..employeeleaveapplicationdaybreakdown ELADB
	                                        INNER JOIN HRMS..employeeleaveapplication ELA ON ELA.employeeleaveaid = ELADB.employeeleaveaid
											LEFT JOIN (
													SELECT fyr.FinancialYearID,Year,YearDescription,Min(PeriodStartDate) StartDate,Max(PeriodStartDate) EndDate
													FROM 
													Security..financialyear fyr
													INNER JOIN Security..Period P on p.FinancialYearID = fyr.FinancialYearID
													Where fyr.IsCurrent=1
													GROUP BY Year,YearDescription,fyr.FinancialYearID
                                            ) fy ON fy.Year = Year(ela.ApplicationDate)
	                                            WHERE approvalstatusid = {(int)Util.ApprovalStatus.Approved}
		                                            AND ELADB.noofleavedays > 0
		                                            AND ELADB.iscancelled = 0
													and ELA.EmployeeLeaveAID = {EmployeeLeaveAID}";

            IEnumerable<Dictionary<string, object>> unauthorizeLeaveSqlLeaveData = EmployeeLeaveApplicationRepo.GetDataDictCollection(unauthorizeLeaveSql);

            return unauthorizeLeaveSqlLeaveData;
        }
        public async Task<(bool, string)> SaveEmailNotification(List<UnauthorizedLeaveEmailNotificationDto> unauthorizedLeaves)
        {
            try
            {
                int groupId = 0;
                var unauthorizedLeaveEmailDates = new List<UnauthorizedLeaveEmailDate>();
                var emailNotifications = new List<EmailNotification>();


                foreach (var dto in unauthorizedLeaves)
                {
                    #region Send Email
                    var mail = await GetEmailNotificationEmailAddress(dto.EmployeeID);
                    List<string> ToEmailAddress = mail.Item1;
                    List<string> CCEmailAddress = mail.Item2;

                    DateTime dateOfMonth = DateTime.Parse(dto.DateOfMonth);
                    string formattedDate = dateOfMonth.ToString("MMMM yyyy");
                    List<Dictionary<string, object>> collection = new List<Dictionary<string, object>>();
                    Dictionary<string, object> dateDict = new Dictionary<string, object>
                        {
                            { "EmployeeName", dto.FullName +'-'+ dto.EmployeeCode  },
                            { "NameOfMonth", formattedDate },
                            { "DateOfAbsence", dto.AttendanceDates },
                            { "TotalAbsentDays", dto.GroupCount }
                        };

                    collection.Add(dateDict);

                    EmailDtoCore emailData = null;
                    if (mail.Item1.Count > 0)
                    {
                        emailData = await UnauthorizedLeaveMail(dto.EmployeeID, (int)Util.ApprovalType.EmailNotification, (int)Util.MailGroupSetup.EmailNotificationMail, collection, false, ToEmailAddress, CCEmailAddress, null, 0, 0);
                    }
                    #endregion

                    var emailNotification = new EmailNotification
                    {
                        EmployeeID = dto.EmployeeID,
                        EmailBody = emailData?.EmailBody,
                        To = string.Join(",", ToEmailAddress),
                        CC = string.Join(",", CCEmailAddress)
                    };

                    emailNotifications.Add(emailNotification);

                    var dateStrings = dto.AttendanceDates.Split(new[] { "| " }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var dateString in dateStrings)
                    {
                        if (DateTime.TryParse(dateString, out var date))
                        {
                            var emailDate = new UnauthorizedLeaveEmailDate
                            {
                                EmployeeID = dto.EmployeeID,
                                AttendanceDate = date
                            };
                            unauthorizedLeaveEmailDates.Add(emailDate);
                            //UpdateAttendanceSummaryTable(dto.EmployeeID, date, date);
                        }

                    }
                }

                using (var unitOfWork = new UnitOfWork())
                {
                    groupId = SetEmailNotificationGroupNewId();
                    emailNotifications.ForEach(x =>
                    {
                        x.GroupID = groupId;
                        x.SetAdded();
                        SetEmailNotificationNewId(x);
                        SetAuditFields(x);
                        unauthorizedLeaveEmailDates.Where(y => y.EmployeeID == x.EmployeeID).ToList().ForEach(z =>
                        {
                            z.SetAdded();
                            z.ENID = x.ENID;
                            SetAuditFields(z);
                        });
                    });

                    UnauthorizedLeaveEmailDateRepo.AddRange(unauthorizedLeaveEmailDates);
                    EmailNotificationRepo.AddRange(emailNotifications);

                    unitOfWork.CommitChanges(); // Synchronously commit changes to the database.
                }

                var emailNotificationList = EmailNotificationRepo.GetAllList(x => x.GroupID == groupId);

                foreach (var item in emailNotificationList)
                {
                    string[] emailArrayTo = item.To.Split(',');
                    string[] emailArrayCC = item.CC.Split(',');
                    List<string> ToEmailAddress = new List<string>(emailArrayTo);
                    List<string> CCEmailAddress = new List<string>(emailArrayCC);
                    await UnauthorizedLeaveSentMail(item.EmployeeID, (int)Util.ApprovalType.EmailNotification, (int)Util.MailGroupSetup.EmailNotificationMail, false, item.EmailBody, ToEmailAddress, CCEmailAddress, null, 0, 0);
                }


                return (true, "Email Sent successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}");
                //return (false, $"An error occurred: {ex.Message}+ 'Inner: '+{ex.InnerException}+ 'TtracTrace: '+{ex.StackTrace}");

            }
        }
        private List<LeaveDetails> LeaveDetailsBreakDownHr(int leaveTypeId, DateTime startDate, DateTime endDate, int employeeLeaveAID, int employeeID)
        {

            var leaveSettings = GetLeavePolicySettings(leaveTypeId).Result;
            var leaveDetails = new List<LeaveDetails>();

            var prevLeaveDays = (from ela in EmployeeLeaveApplicationRepo.GetAllList()
                                 join elabd in EmployeeLeaveApplicationDayBreakDownRepo.GetAllList() on ela.EmployeeLeaveAID equals elabd.EmployeeLeaveAID
                                 where ela.EmployeeID == employeeID && ((elabd.RequestDate >= startDate && elabd.RequestDate <= endDate) && ela.ApprovalStatusID != (int)ApprovalStatus.Rejected)
                                 && (employeeLeaveAID == 0 || ela.EmployeeLeaveAID != employeeLeaveAID) && elabd.IsCancelled == false
                                 select new { elabd.RequestDate, elabd.HalfOrFullDay }).ToList();

            if (employeeLeaveAID > 0)
            {
                var leaveMaster = EmployeeLeaveApplicationRepo.Entities.Where(x => x.EmployeeLeaveAID == employeeLeaveAID).SingleOrDefault();
                if (leaveMaster.IsNotNull() && leaveMaster.RequestStartDate == startDate && leaveMaster.RequestEndDate == endDate)
                {
                    leaveDetails = EmployeeLeaveApplicationDayBreakDownRepo.Entities.Where(x => x.EmployeeLeaveAID == employeeLeaveAID).Select(y => new LeaveDetails
                    {
                        ELADBDID = y.ELADBDID,
                        Day = y.RequestDate.ToString("dd MMM yyyy"),
                        DayStatus = y.IsCancelled ? "Cancelled" : y.HalfOrFullDay
                    }).ToList();
                    leaveDetails.ForEach(x =>
                    {
                        x.DayStatus = prevLeaveDays.Any(z => z.RequestDate == x.DayDateTime && z.HalfOrFullDay == Util.Halfday) ? Util.OnlyHalfday : x.DayStatus;
                    });

                    return leaveDetails;
                }
            }

            var weekend = (from emp in EmploymentRepo.GetAllList()
                           join sm in ShiftingMasterRepo.GetAllList() on emp.ShiftID equals sm.ShiftingMasterID
                           join sc in ShiftingChildRepo.GetAllList() on sm.ShiftingMasterID equals sc.ShiftingMasterID
                           where emp.EmployeeID == employeeID && sc.IsWorkingDay == false
                           select sc.Day).ToList();

            var holiday = HolidayRepo.Entities.Where(x => x.HolidayDate >= startDate && x.HolidayDate <= endDate).Select(y => new { y.HolidayDate }).ToList();

            for (var day = startDate; day <= endDate; day = day.AddDays(1))
            {
                var leaveDetail = new LeaveDetails
                {
                    Day = day.ToString("dd MMM yyyy"),
                    DayStatus = prevLeaveDays.Any(x => x.RequestDate == day && x.HalfOrFullDay == Util.Fullday) || prevLeaveDays.Count(x => x.RequestDate == day && x.HalfOrFullDay == Util.Halfday) == 2 ? Util.Conflict : prevLeaveDays.Any(x => x.RequestDate == day && x.HalfOrFullDay == Util.Halfday) ? Util.OnlyHalfday : !leaveSettings.IsHolidayInclusive && (weekend.Any(x => x == (int)day.DayOfWeek) || holiday.Any(x => x.HolidayDate == day)) ? Util.NonWorkingDay : Util.Fullday
                };
                leaveDetails.Add(leaveDetail);
            }
            return leaveDetails;
        }
        public GridModel GetAllLeaveApplicationListForHROnbehalfOfEmployee(GridParameter parameters, string type)
        {
            // "All","Pending Action","Action Taken"
            string filter = "";
            //switch (parameters.ApprovalFilterData)
            //{
            //    case "All":
            //        filter = "";
            //        break;
            //    case "Pending":
            //        filter = $@" ELA.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
            //        break;
            //    case "Approved":
            //        filter = $@" ELA.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
            //        break;
            //    case "Rejected":
            //        filter = $@" ELA.ApprovalStatusID = {(int)Util.ApprovalStatus.Rejected}";
            //        break;
            //    default:
            //        break;
            //}
            //filter = filter.IsNotNullOrEmpty() ? @$"WHERE {filter} AND ExternalID='hr'" : "";
            filter = @$"WHERE ELA.ApprovalStatusID ={(int)Util.ApprovalStatus.Approved} AND ExternalID='hr'";
            //if (type == "TotalLeaveToday")
            //{
            //    string filterNew = @$" CAST(GETDATE() as Date) BETWEEN CAST(RequestStartDate as Date) AND CAST(RequestEndDate as Date) AND ELA.ApprovalStatusID <> {(int)Util.ApprovalStatus.Rejected}";
            //    filter += filter.IsNotNullOrEmpty() ? @$" AND {filterNew}" : $@" WHERE {filterNew}";
            //}
            string sql = $@"SELECT 
                                DISTINCT
		                        ELA.EmployeeLeaveAID,
                                ELA.LeaveCategoryID LeaveCategoryID,
                                ELA.EmployeeID,
		                        SVLC.SystemVariableCode LeaveCategory,
                                Emp.FullName EmployeeName,
                                Emp.EmployeeCode EmployeeCode,
		                        --CONCAT(CONVERT(char(11), ELA.RequestStartDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) LeaveDates,
                                CASE WHEN ELA.LeaveCategoryID=69 THEN CONCAT(CONVERT(char(11), ELA.RequestEndDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) ELSE CONCAT(CONVERT(char(11), ELA.RequestStartDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) end LeaveDates,
		                        ELA.NoOfLeaveDays NumberOfLeave,
		                        CASE WHEN ELA.CancellationStatus=1 THEN 'Partially Cancelled'
								WHEN ELA.CancellationStatus=2 THEN 'Fully Cancelled'
								ELSE SVAS.SystemVariableCode  END AS ApprovalStatus,
                                ELA.ApprovalStatusID,
                                ISNULL(AP.ApprovalProcessID,0) ApprovalProcessID,
                                ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID,
								ISNULL(APForward.APForwardInfoID,0) APForwardInfoID,
								ELA.CreatedDate,
								SVLC.SystemVariableCode + ( CASE WHEN ISNULL(LFA.LFADID,0) = 1 THEN ' (LFA Applied)'  ELSE '' END) LeaveType ,
                            CASE 
		                        WHEN LFA.LFADID IS NULL
			                        THEN CAST(0 AS BIT)
		                        ELSE CAST(1 AS BIT)
		                        END IsLFAApplied,
								CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee,
								CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
	                        FROM HRMS.dbo.EmployeeLeaveApplication ELA
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee Emp on Emp.EmployeeID=ELA.EmployeeID
	                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVLC ON ELA.LeaveCategoryID=SVLC.SystemVariableID
	                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVAS ON ELA.ApprovalStatusID=SVAS.SystemVariableID
							LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = ELA.EmployeeLeaveAID AND AP.APTypeID =1
                            LEFT JOIN (SELECT 
				                            APEmployeeFeedbackID,ApprovalProcessID,IsEditable 
			                            FROM 
				                            Approval..ApprovalEmployeeFeedback AEF
			                            WHERE (AEF.APFeedbackID = 2 OR AEF.APFeedbackID = 8 OR AEF.APFeedbackID = 9) AND (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})
                              UNION ALL 
										SELECT 
				                            EmployeeFeedbackID,ApprovalProcessID,0 IsEditable 
			                            FROM 
				                            Approval..ApprovalForwardInfo  
			                            WHERE 
				                            EmployeeID = {AppContexts.User.EmployeeID} AND CommentSubmitDate IS NULL
                            )AEF ON AEF.ApprovalProcessID = AP.ApprovalProcessID
                            LEFT JOIN 
		                            (SELECT 
				                            APForwardInfoID,ApprovalProcessID 
			                            FROM 
				                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalForwardInfo  
			                            WHERE  CommentSubmitDate IS NULL) 
                            APForward ON APForward.ApprovalProcessID = AP.ApprovalProcessID
                            LEFT JOIN 
									(
									SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM  Approval..ApprovalEmployeeFeedback
									UNION ALL 
									SELECT DISTINCT ApprovalProcessID,EmployeeID,EmployeeID FROM Approval..ApprovalForwardInfo 
									)							
							F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                            LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback  AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.LeaveApplication}   AND FeedbackSubmitDate IS NULL
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.LeaveApplication} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = ELA.EmployeeLeaveAID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = ELA.EmployeeLeaveAID
									 LEFT JOIN LFADeclaration LFA ON ELA.EmployeeLeaveAID=LFA.EmployeeLeaveAID
							        {filter}
							GROUP BY         
								ELA.EmployeeLeaveAID,
                                ELA.LeaveCategoryID,
                                ELA.EmployeeID,            
		                        SVLC.SystemVariableCode,
                                Emp.FullName,
                                Emp.EmployeeCode,RequestStartDate,RequestEndDate,NoOfLeaveDays,
								SVAS.SystemVariableCode,ELA.ApprovalStatusID,
								AP.ApprovalProcessID,AEF.APEmployeeFeedbackID,APForward.APForwardInfoID,
								LFADID,EditableCount,Rej.Cntr,ELA.CreatedDate,
                                ELA.CancellationStatus";
            var applications = EmployeeLeaveApplicationRepo.LoadGridModel(parameters, sql);

            //var testData = new EmailDtoCore {
            //    Subject="Test"
            //};
            //await Extension.Post<EmailDtoCore>($"/SendMail/SendEMailToRecipients", testData);
            return applications;
        }
        public async Task<LeaveApplication> GetLeaveApplicationHr(int employeeLeaveAID, int employeeID)
        {
            string sql = $@"SELECT DISTINCT
		                        EmployeeLeaveAID,
		                        SVLC.SystemVariableCode LeaveCategory,
		                        ELA.LeaveCategoryID,
		                        CONVERT(char(10), ELA.RequestStartDate,103) RequestStartDate,
		                        CONVERT(char(10), ELA.RequestEndDate,103) RequestEndDate,
		                        --CONCAT(CONVERT(char(11), ELA.RequestStartDate,103),' - ',CONVERT(char(11), ELA.RequestEndDate,103)) LeaveDates,
                                CASE WHEN ELA.LeaveCategoryID=69 THEN CONCAT(CONVERT(char(11), ELA.RequestEndDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) ELSE CONCAT(CONVERT(char(11), ELA.RequestStartDate,106),' - ',CONVERT(char(11), ELA.RequestEndDate,106)) end LeaveDates,
		                        ELA.NoOfLeaveDays NumberOfLeave,
		                        ELA.Purpose,
		                        ISNULL (E.FullName,'') BackupEmployeeName,
		                        ISNULL (ELA.BackupEmployeeID,0) BackupEmployeeID,
		                        ELA.LeaveLocation,
		                        ELA.DateOfJoiningWork,
                                ELA.Remarks,
								ELA.CancellationStatus,
								(EC.EmployeeCode+'-'+Ec.FullName)  CancelledBy,
                                ELA.EmployeeID,
								(VA.EmployeeCode+'-'+ VA.FullName) EmployeeWithCode
	                        FROM EmployeeLeaveApplication ELA
	                        JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SVLC ON ELA.LeaveCategoryID=SVLC.SystemVariableID
	                        LEFT JOIN Employee E ON ELA.BackupEmployeeID=E.EmployeeID
                            LEFT JOIN viewEmployeeUserMapData EC ON EC.UserID=ELA.CancelledBy
							LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = ELA.EmployeeLeaveAID AND AP.APTypeID = {(int)ApprovalType.LeaveApplication}
                           	LEFT JOIN ViewALLEmployee VA ON VA.EmployeeID = ELA.EmployeeID	
	                        WHERE ELA.EmployeeLeaveAID={employeeLeaveAID}  AND ELA.EmployeeID =  {employeeID}";

            var applicationMaster = EmployeeLeaveApplicationRepo.GetData(sql);

            var leaveApplication = new LeaveApplication
            {
                EmployeeLeaveAID = (int)applicationMaster["EmployeeLeaveAID"],
                LeaveCategory = applicationMaster["LeaveCategory"].ToString(),
                LeaveCategoryID = (int)applicationMaster["LeaveCategoryID"],
                RequestStartDate = applicationMaster["RequestStartDate"].ToString(),
                RequestEndDate = applicationMaster["RequestEndDate"].ToString(),
                LeaveDates = applicationMaster["LeaveDates"].ToString(),
                NumberOfLeave = (decimal)applicationMaster["NumberOfLeave"],
                Purpose = applicationMaster["Purpose"].ToString(),
                BackupEmployeeName = applicationMaster["BackupEmployeeName"].ToString(),
                BackupEmployeeID = (int?)applicationMaster["BackupEmployeeID"],
                CancellationStatus = (int)applicationMaster["CancellationStatus"],
                LeaveLocation = applicationMaster["LeaveLocation"].ToString(),
                Remarks = applicationMaster["Remarks"].ToString(),
                CancelledBy = applicationMaster["CancelledBy"].ToString(),
                DateOfJoiningWork = DateTime.TryParse(applicationMaster["DateOfJoiningWork"].ToString(), out var date) ? date : (DateTime?)null,
                EmployeeID = (int)applicationMaster["EmployeeID"],
                EmployeeWithCode = applicationMaster["EmployeeWithCode"].ToString(),
            };
            leaveApplication.LeaveDetails = EmployeeLeaveApplicationDayBreakDownRepo.Entities.Where(x => x.EmployeeLeaveAID == employeeLeaveAID).Select(y => new LeaveDetails
            {
                Day = y.RequestDate.ToString("dd MMM yyyy"),
                DayStatus = y.IsCancelled ? "Cancelled" : y.HalfOrFullDay
            }).ToList();

            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='EmployeeLeaveApplication' AND ReferenceID={leaveApplication.EmployeeLeaveAID}";
            var attachment = EmployeeLeaveApplicationRepo.GetDataDictCollection(attachmentSql);
            var attachemntList = new List<Attachments>();

            foreach (var data in attachment)
            {
                attachemntList.Add(new Attachments
                {
                    FUID = (int)data["FUID"],
                    AID = data["FUID"].ToString(),
                    FilePath = data["FilePath"].ToString(),
                    OriginalName = data["OriginalName"].ToString() + data["FileType"].ToString(),
                    FileName = data["FileName"].ToString(),
                    Type = data["FileType"].ToString(),
                    Size = Convert.ToDecimal(data["SizeInKB"])
                });
            }
            leaveApplication.Attachments = attachemntList;

            //if (approvalProcessID > 0)
            //{
            //    leaveApplication.Comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.LeaveApplication).Result;
            //}

            leaveApplication.LFADeclaration = LFADeclarationRepo.Entities.FirstOrDefault(x => x.EmployeeLeaveAID == leaveApplication.EmployeeLeaveAID).MapTo<LFADeclarationDto>();

            return await Task.FromResult(leaveApplication);
        }
    }
}
