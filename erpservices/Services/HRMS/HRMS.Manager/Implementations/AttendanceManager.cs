using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Manager.Core;
using Manager.Core.CommonDto;
using Manager.Core.Mapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HRMS.Manager.Implementations
{
    public class AttendanceManager : ManagerBase, IAttendanceManager
    {

        private readonly IRepository<Employee> EmployeeRepo;
        private readonly IRepository<AttendanceSummary> AttendanceSummaryRepo;
        private readonly IRepository<ShiftingChild> ShiftingChildRepo;
        private readonly IRepository<ShiftingMaster> ShiftingMasterRepo;
        public AttendanceManager(IRepository<Employee> employeeRepo, IRepository<AttendanceSummary> attendanceSummaryRepo, IRepository<ShiftingChild> shiftingChildRepo, IRepository<ShiftingMaster> shiftingMasterRepo)
        {
            EmployeeRepo = employeeRepo;
            AttendanceSummaryRepo = attendanceSummaryRepo;
            ShiftingChildRepo = shiftingChildRepo;
            ShiftingMasterRepo = shiftingMasterRepo;
        }

        public void UpdateAttendance(AttendanceSummaryHRMSDto data)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var attData = (from atts in AttendanceSummaryRepo.GetAllList()
                               join shift in ShiftingChildRepo.GetAllList() on atts.ShiftID equals shift.ShiftingMasterID
                               join shiftM in ShiftingMasterRepo.GetAllList() on atts.ShiftID equals shiftM.ShiftingMasterID
                               where atts.PrimaryID == data.PrimaryID && atts.AttendanceDate == data.AttendanceDate
                               select new AttendanceSummaryHRMSDto
                               {
                                   PrimaryID = atts.PrimaryID,
                                   EmployeeID = atts.EmployeeID,
                                   EmployeeCode = atts.EmployeeCode,
                                   RowVersion = atts.RowVersion,
                                   CreatedBy = atts.CreatedBy,
                                   CreatedDate = atts.CreatedDate,
                                   CreatedIP = atts.CreatedIP,
                                   ShiftID = atts.ShiftID,
                                   InTime = atts.InTime,
                                   OutTime = atts.OutTime,
                                   AttendanceStatus = atts.AttendanceStatus,
                                   AttendanceDate = atts.AttendanceDate,
                                   StartTime = shift.StartTime,
                                   EndTime = shift.EndTime,
                                   BufferTimeInMinute = shiftM.BufferTimeInMinute
                               }).ToList();


                //var existdata = AttendanceSummaryRepo.Entities.SingleOrDefault(x => x.PrimaryID == data.PrimaryID).MapTo<AttendanceSummary>();

                if (attData.IsNull() || attData[0].PrimaryID.IsZero())
                {
                    data.SetAdded();
                }
                else
                {
                    data.SetModified();
                    DateTime dtIn = Convert.ToDateTime(data.AttendanceDate.ToString("yyyy-MM-dd") + " " + data.InTimeString);
                    data.InTime = dtIn;
                    DateTime dtOut = Convert.ToDateTime(data.AttendanceDate.ToString("yyyy-MM-dd") + " " + data.OutTimeString);
                    data.OutTime = dtOut;

                    //Mamun added 
                    TimeSpan varTime = (DateTime)dtOut - (DateTime)dtIn;
                    double fractionalMinutes = varTime.TotalMinutes;
                    data.ActualWorkingHourInMin = (int)fractionalMinutes;

                    TimeSpan OfficeIntime = TimeSpan.Parse(data.InTimeString);
                    TimeSpan OfficeOuttime = TimeSpan.Parse(data.OutTimeString);

                    //Late time
                    if (OfficeIntime > attData[0].StartTime)
                    {
                        DateTime dti2 = Convert.ToDateTime(data.AttendanceDate.ToString("yyyy-MM-dd") + " " + data.InTimeString);
                        DateTime dto2 = Convert.ToDateTime(data.AttendanceDate.ToString("yyyy-MM-dd") + " " + attData[0].StartTime);
                        TimeSpan lateTime = (DateTime)dti2 - (DateTime)dto2;
                        double lateTimeMinutes = lateTime.TotalMinutes;
                        data.LateInMin = (int)lateTimeMinutes;
                    }
                    //Overtime
                    if (OfficeOuttime > attData[0].EndTime)
                    {
                        DateTime dti3 = Convert.ToDateTime(data.AttendanceDate.ToString("yyyy-MM-dd") + " " + data.OutTimeString);
                        DateTime dto3 = Convert.ToDateTime(data.AttendanceDate.ToString("yyyy-MM-dd") + " " + attData[0].EndTime);
                        TimeSpan overTime = (DateTime)dti3 - (DateTime)dto3;
                        double overTimeMinutes = overTime.TotalMinutes;
                        data.OverTimeInMin = (int)overTimeMinutes;
                    }
                    //Total working time
                    DateTime dti4 = Convert.ToDateTime(data.AttendanceDate.ToString("yyyy-MM-dd") + " " + data.InTimeString);
                    DateTime dto4 = Convert.ToDateTime(data.AttendanceDate.ToString("yyyy-MM-dd") + " " + data.OutTimeString);
                    TimeSpan totalTime = (DateTime)dto4 - (DateTime)dti4;
                    double totalTimeMinutes = totalTime.TotalMinutes;
                    data.TotalTimeInMin = (int)totalTimeMinutes;
                    //Actual working time
                    TimeSpan timeDifference = attData[0].EndTime - attData[0].StartTime;
                    data.ActualWorkingHourInMin = (int)timeDifference.TotalMinutes;



                    // Extract only the hour and minute components from the time values
                    TimeSpan OfficeIntime_HourMinute = new TimeSpan(OfficeIntime.Hours, OfficeIntime.Minutes, 0);
                    TimeSpan StartTime_HourMinute = new TimeSpan(attData[0].StartTime.Hours, attData[0].StartTime.Minutes + attData[0].BufferTimeInMinute, 0);



                    switch (attData[0].AttendanceStatus)
                    {
                        case (int)Util.AttendanceStatus.Present:
                        case (int)Util.AttendanceStatus.Late:
                        case (int)Util.AttendanceStatus.Absent:

                            if (data.TotalTimeInMin >= data.ActualWorkingHourInMin && OfficeIntime_HourMinute <= StartTime_HourMinute)
                            {
                                data.AttendanceStatus = (int)Util.AttendanceStatus.Present; //Present
                                data.DayStatus = "Full Day";
                            }
                            else if (data.TotalTimeInMin >= data.ActualWorkingHourInMin && OfficeIntime_HourMinute > StartTime_HourMinute)
                            {
                                data.AttendanceStatus = (int)Util.AttendanceStatus.Late; //Late
                                data.DayStatus = "Full Day";
                            }
                            else if (data.TotalTimeInMin <= data.ActualWorkingHourInMin && OfficeIntime_HourMinute > StartTime_HourMinute)
                            {
                                data.AttendanceStatus = (int)Util.AttendanceStatus.Late; //Late
                                data.DayStatus = "Partial Day";
                            }
                            else if (data.TotalTimeInMin < data.ActualWorkingHourInMin && OfficeIntime_HourMinute <= StartTime_HourMinute)
                            {
                                data.AttendanceStatus = 88; //Partial Day
                                data.DayStatus = "Partial Day";
                            }
                            break;

                        case (int)Util.AttendanceStatus.Leave:
                        case (int)Util.AttendanceStatus.LeavePresent:
                        case (int)Util.AttendanceStatus.LeaveLate:
                            if (data.TotalTimeInMin >= data.ActualWorkingHourInMin && OfficeIntime_HourMinute <= StartTime_HourMinute)
                            {
                                data.AttendanceStatus = (int)Util.AttendanceStatus.LeavePresent; //Present
                                data.DayStatus = "Full Day";
                            }
                            else if (data.TotalTimeInMin >= data.ActualWorkingHourInMin && OfficeIntime_HourMinute > StartTime_HourMinute)
                            {
                                data.AttendanceStatus = (int)Util.AttendanceStatus.LeaveLate; //Late
                                data.DayStatus = "Full Day";
                            }
                            else if (data.TotalTimeInMin <= data.ActualWorkingHourInMin && OfficeIntime_HourMinute > StartTime_HourMinute)
                            {
                                data.AttendanceStatus = (int)Util.AttendanceStatus.LeaveLate; //Late
                                data.DayStatus = "Partial Day";
                            }
                            else if (data.TotalTimeInMin < data.ActualWorkingHourInMin && OfficeIntime_HourMinute <= StartTime_HourMinute)
                            {
                                data.AttendanceStatus = (int)Util.AttendanceStatus.LeavePresent; //Partial Day
                                data.DayStatus = "Partial Day";
                            }
                            break;
                        case (int)Util.AttendanceStatus.Holiday:
                        case (int)Util.AttendanceStatus.HolidayPresent:
                        case (int)Util.AttendanceStatus.HolidayLate:
                            if (data.TotalTimeInMin >= data.ActualWorkingHourInMin && OfficeIntime_HourMinute <= StartTime_HourMinute)
                            {
                                data.AttendanceStatus = (int)Util.AttendanceStatus.HolidayPresent; //Present
                                data.DayStatus = "Full Day";
                            }
                            else if (data.TotalTimeInMin >= data.ActualWorkingHourInMin && OfficeIntime_HourMinute > StartTime_HourMinute)
                            {
                                data.AttendanceStatus = (int)Util.AttendanceStatus.HolidayLate; //Late
                                data.DayStatus = "Full Day";
                            }
                            else if (data.TotalTimeInMin <= data.ActualWorkingHourInMin && OfficeIntime_HourMinute > StartTime_HourMinute)
                            {
                                data.AttendanceStatus = (int)Util.AttendanceStatus.HolidayLate; //Late
                                data.DayStatus = "Partial Day";
                            }
                            else if (data.TotalTimeInMin < data.ActualWorkingHourInMin && OfficeIntime_HourMinute <= StartTime_HourMinute)
                            {
                                data.AttendanceStatus = (int)Util.AttendanceStatus.HolidayPresent; //Partial Day
                                data.DayStatus = "Partial Day";
                            }
                            break;
                        case (int)Util.AttendanceStatus.OffDay:
                        case (int)Util.AttendanceStatus.OffDayPresent:
                        case (int)Util.AttendanceStatus.OffDayLate:
                            if (data.TotalTimeInMin >= data.ActualWorkingHourInMin && OfficeIntime_HourMinute <= StartTime_HourMinute)
                            {
                                data.AttendanceStatus = (int)Util.AttendanceStatus.OffDayPresent; //Present
                                data.DayStatus = "Full Day";
                            }
                            else if (data.TotalTimeInMin >= data.ActualWorkingHourInMin && OfficeIntime_HourMinute > StartTime_HourMinute)
                            {
                                data.AttendanceStatus = (int)Util.AttendanceStatus.OffDayLate; //Late
                                data.DayStatus = "Full Day";
                            }
                            else if (data.TotalTimeInMin <= data.ActualWorkingHourInMin && OfficeIntime_HourMinute > StartTime_HourMinute)
                            {
                                data.AttendanceStatus = (int)Util.AttendanceStatus.OffDayLate; //Late
                                data.DayStatus = "Partial Day";
                            }
                            else if (data.TotalTimeInMin < data.ActualWorkingHourInMin && OfficeIntime_HourMinute <= StartTime_HourMinute)
                            {
                                data.AttendanceStatus = (int)Util.AttendanceStatus.OffDayPresent; //Partial Day
                                data.DayStatus = "Partial Day";
                            }
                            break;
                        case (int)Util.AttendanceStatus.Weekend:
                        case (int)Util.AttendanceStatus.WeekendPresent:
                        case (int)Util.AttendanceStatus.WeekendLate:
                            if (data.TotalTimeInMin >= data.ActualWorkingHourInMin && OfficeIntime_HourMinute <= StartTime_HourMinute)
                            {
                                data.AttendanceStatus = (int)Util.AttendanceStatus.WeekendPresent; //Present
                                data.DayStatus = "Full Day";
                            }
                            else if (data.TotalTimeInMin >= data.ActualWorkingHourInMin && OfficeIntime_HourMinute > StartTime_HourMinute)
                            {
                                data.AttendanceStatus = (int)Util.AttendanceStatus.WeekendLate; //Late
                                data.DayStatus = "Full Day";
                            }
                            else if (data.TotalTimeInMin <= data.ActualWorkingHourInMin && OfficeIntime_HourMinute > StartTime_HourMinute)
                            {
                                data.AttendanceStatus = (int)Util.AttendanceStatus.WeekendLate; //Late
                                data.DayStatus = "Partial Day";
                            }
                            else if (data.TotalTimeInMin < data.ActualWorkingHourInMin && OfficeIntime_HourMinute <= StartTime_HourMinute)
                            {
                                data.AttendanceStatus = (int)Util.AttendanceStatus.WeekendPresent; //Partial Day
                                data.DayStatus = "Partial Day";
                            }
                            break;
                        default:
                            // Handle the default case
                            break;
                    }
                    //Mamun added End
                    //string dateString = data.FromDate.ToString();
                    //if (dateString == "1/1/0001 12:00:00 AM")
                    //{
                    //    data.FromDate = DateTime.Now.AddDays(-30);
                    //    data.ToDate = DateTime.Now;
                    //}

                    data.ApprovalStatusID = (int)Util.ApprovalStatus.Approved;
                    data.IsEdited = true;
                    data.EmployeeCode = attData[0].EmployeeCode;
                    data.EmployeeID = attData[0].EmployeeID;
                    data.ShiftID = attData[0].ShiftID;
                    data.RowVersion = attData[0].RowVersion;
                    data.CreatedBy = attData[0].CreatedBy;
                    data.CreatedDate = attData[0].CreatedDate;
                    data.CreatedIP = attData[0].CreatedIP;
                }

                var attEnt = data.MapTo<AttendanceSummary>();
                SetAuditFields(attEnt);
                AttendanceSummaryRepo.Add(attEnt);
                unitOfWork.CommitChangesWithAudit();
            }
        }

        //private static void SetAttendanceAndDayStatus(AttendanceSummaryHRMSDto data, int AttendanceStatus, string DayStatus)
        //{
        //    switch (data.AttendanceStatus)
        //    {
        //        case (int)Util.AttendanceStatus.Present:
        //        case (int)Util.AttendanceStatus.Late:
        //        case (int)Util.AttendanceStatus.Absent:
        //            data.AttendanceStatus = AttendanceStatus; //Present
        //            data.DayStatus = DayStatus;
        //            break;
        //    }

        //}

        public async Task<List<Widget>> GetAttendanceWidgets(int personID)
        {
            var widgets = new List<Widget>();

            var emp = GetEmployee(personID);
            Widget widget10 = SelfAttendance(personID);
            Widget widget11 = SupervisorTeammembers(emp.EmployeeID);
            Widget DoughnutWidget = GetEmployeeAttendanceDoughnutWidget(personID);
            widgets.Add(widget10);
            widgets.Add(widget11);
            widgets.Add(DoughnutWidget);
            return widgets;
        }
        private Widget GetEmployeeAttendanceDoughnutWidget(int personID)
        {
            string sql = $@"SELECT SUM(TotalCount) TotalCount,AttendanceStatusID,AttendanceStatus,Title
	                        ,TitleID
	                        ,SequenceNo
                        FROM 
                        (
                        SELECT TotalCount
	                        ,CASE 
		                        WHEN ATS.AttendanceStatus IN (
				                        77,78,79,80,81,82,84,85,86
				                        )
			                        THEN 77
		                        WHEN ATS.AttendanceStatus = 74
			                        THEN 74
		                        WHEN ATS.AttendanceStatus = 75
			                        THEN 75
		                        WHEN ATS.AttendanceStatus in(76,83,87,214)
			                        THEN 76
		                        WHEN ATS.AttendanceStatus = 88
			                        THEN 88
		                        ELSE 73
		                        END AttendanceStatusID
	                        ,CASE 
		                        WHEN ATS.AttendanceStatus IN (
				                        77,78,79,80,81,82,84,85,86
				                        )
			                        THEN 'Holiday & Weekend'
		                        WHEN ATS.AttendanceStatus = 74
			                        THEN 'Late'
		                        WHEN ATS.AttendanceStatus = 75
			                        THEN 'Absent'
		                        WHEN ATS.AttendanceStatus in(76,83,87,214)
			                        THEN 'Leave'
		                        WHEN ATS.AttendanceStatus = 88
			                        THEN 'Partial Entry'
		                        ELSE 'On Time'
		                        END AttendanceStatus
	                        ,Title
	                        ,TitleID
	                        ,SequenceNo
                        FROM (
	                        SELECT COUNT(AttendanceDate) TotalCount
		                        ,ASN.SystemVariableID AttendanceStatus
		                        ,'Last Week' AS Title
			                        ,'LW' AS TitleID
			                        ,1 SequenceNo
	                        FROM (
		                        SELECT TOP 7 ATS.AttendanceDate
			                        ,ATS.AttendanceStatus
			
		                        FROM AttendanceSummary AS ATS
		                        INNER JOIN Employee AS Emp ON Emp.EmployeeCode = CAST(ATS.EmployeeCode AS VARCHAR(100)) COLLATE Latin1_General_CI_AS
		                        WHERE PersonID = {personID} AND CAST(ATS.AttendanceDate as Date) BETWEEN CAST(DATEADD(DAY, -7, GETDATE()) as Date) AND CAST(GETDATE() as Date)
                                ORDER BY AttendanceDate desc
		                        ) A
	                        RIGHT JOIN Security..SystemVariable ASN ON ASN.SystemVariableID = A.AttendanceStatus
	                        WHERE ASN.EntityTypeID = 20
	                        GROUP BY ASN.SystemVariableID
	
	                        UNION
	
	                        SELECT COUNT(AttendanceDate) TotalCount,ASN.SystemVariableID AttendanceStatus,'Last 2 Weeks' AS Title
			                        ,'L2W' AS TitleID
			                        ,2 SequenceNo
	                        FROM (
		                        SELECT TOP 14 ATS.AttendanceDate
			                        ,ATS.AttendanceStatus
			
		                        FROM AttendanceSummary AS ATS
		                        INNER JOIN Employee AS Emp ON Emp.EmployeeCode = CAST(ATS.EmployeeCode AS VARCHAR(100)) COLLATE Latin1_General_CI_AS
		                        WHERE PersonID = {personID} AND CAST(ATS.AttendanceDate as Date) BETWEEN CAST(DATEADD(DAY, -14, GETDATE()) as Date) AND CAST(GETDATE() as Date)
                                ORDER BY AttendanceDate desc
		                        ) A
	                        RIGHT JOIN Security..SystemVariable ASN ON ASN.SystemVariableID = A.AttendanceStatus
	                        WHERE ASN.EntityTypeID = 20
	                        GROUP BY ASN.SystemVariableID
	
	                        UNION
	
	                        SELECT COUNT(AttendanceDate) TotalCount
		                        ,ASN.SystemVariableID AttendanceStatus,
		                        'Last 30 Days' AS Title
		                        ,'L1M' AS TitleID
		                        ,3 SequenceNo
	                        FROM (
		                        SELECT TOP 30 ATS.AttendanceDate
			                        ,ATS.AttendanceStatus
			
		                        FROM AttendanceSummary AS ATS
		                        INNER JOIN Employee AS Emp ON Emp.EmployeeCode = CAST(ATS.EmployeeCode AS VARCHAR(100)) COLLATE Latin1_General_CI_AS
		                        WHERE PersonID =  {personID} AND CAST(ATS.AttendanceDate as Date) BETWEEN CAST(DATEADD(DAY, -30, GETDATE()) as Date) AND CAST(GETDATE() as Date)
                                ORDER BY AttendanceDate desc
		                        ) A
	                        RIGHT JOIN Security..SystemVariable ASN ON ASN.SystemVariableID = A.AttendanceStatus
	                        WHERE ASN.EntityTypeID = 20
	                        GROUP BY ASN.SystemVariableID
	                        ) ATS

                        ) Main
                        GROUP BY AttendanceStatusID,AttendanceStatus,Title
	                        ,TitleID
	                        ,SequenceNo

                        ORDER BY SequenceNo,Title
                        ";
            var list = EmployeeRepo.GetDataModelCollection<AttendanceSummaryDto>(sql);
            Widget widget = new Widget
            {
                id = "DoughnutWidget",
                title = "My Attendance Summary"
            };

            if (list.Count > 0)
            {

                var listOfRanges = list.GroupBy(p => new { p.TitleID, p.Title }).Select(g => new { g.Key.TitleID, g.Key.Title }).ToList();

                widget.ranges = new Dictionary<string, string>();
                widget.mainChart = new MainChart
                {
                    datasets = new Dictionary<string, List<MainChartDatasets>>()
                };
                foreach (var obj in listOfRanges)
                {
                    widget.ranges.Add(obj.TitleID, obj.Title);

                    MainChartDatasets item = new MainChartDatasets();
                    var datasets = list.Where(x => x.TitleID.Equals(obj.TitleID));
                    item.data = datasets.Select(g => g.TotalCount).ToArray();
                    item.backgroundColor = datasets.Select(g => g.BackgroundColor).ToArray();
                    item.hoverBackgroundColor = datasets.Select(g => g.HoverBackgroundColor).ToArray();
                    widget.mainChart.datasets.Add(obj.TitleID, new List<MainChartDatasets> { item });
                }
                widget.currentRange = widget.ranges.First().Key;
                widget.mainChart.labels = list.Select(g => g.AttendanceStatus).Distinct().ToArray();
                widget.footer = new List<Footer>();

                var data = list.GroupBy(x => x.AttendanceStatus).ToDictionary(g => g.Key, g => g.Select(y => new { y.TitleID, TotalCount = y.TotalCount.ToString() }).ToDictionary(y => y.TitleID, y => y.TotalCount));

                foreach (var obj in data)
                {
                    //var footer = new Dictionary<string, string>();
                    //foreach (var value in obj.Value)
                    //{
                    //    footer.Add(value.TitleID, value.TotalCount.ToString()); ;
                    //}
                    widget.footer.Add(new Footer
                    {
                        title = obj.Key,
                        count = obj.Value
                    });
                }

                widget.mainChart.options = new MainChartOptions
                {
                    cutoutPercentage = 50,
                    spanGaps = false,
                    maintainAspectRatio = false,
                    legend = new MainChartOptionsLegend
                    {
                        display = true,
                        position = "bottom",
                        labels = new MainChartOptionsLegendLabel
                        {
                            padding = 16,
                            usePointStyle = true
                        }
                    }
                };
                return widget;
            }
            return widget;
        }
        private Widget GetEmployeeAttendanceDoughnutWidgetOld(int personID)
        {
            string sql = $@"SELECT ATTENDANCE_STATUS AttendanceStatus,COUNT(ATTENDANCE_STATUS) TotalCount,Title,TitleID,
                            CASE WHEN ATTENDANCE_STATUS = 'Late' THEN '#EA1C2D' ELSE 'green' END BackgroundColor,
                            CASE WHEN ATTENDANCE_STATUS = 'Late' THEN '#F45A4D' ELSE '#32bc39' END HoverBackgroundColor 
                            FROM 
                            (
                            SELECT TOP 7 [SIMPLE_DATE] AttendanceDate,
	                                                  [NAGAD_EMP_ID] EmployeeCode
	                                                  ,ISNULL(CONVERT(VARCHAR(10), CAST(MIN([DATE_TIME]) AS TIME), 0),'') IN_TIME
	                                                  ,ISNULL(CONVERT(VARCHAR(10), CAST(MAX([DATE_TIME]) AS TIME), 0),'') OUT_TIME
	                                                  ,CASE 
	                                                  WHEN SUBSTRING(MIN([DATE_TIME]),12,8) > '09:15:00'
	                                                  THEN 'Late'
	                                                  ELSE 'On Time'
	                                                  END ATTENDANCE_STATUS,
						                              'Last Week' Title,
						                              'LW' TitleID
                                                  FROM [attendanceService].[dbo].[EVENT_LOG]
                                                  INNER JOIN HRMS..Employee Emp ON Emp.EmployeeCode = CAST(NAGAD_EMP_ID as varchar(100))  COLLATE Latin1_General_CI_AS 
                                                  WHERE Emp.PersonID = {personID}
                                                  GROUP BY [SIMPLE_DATE]
	                                                  ,[NAGAD_EMP_ID]                      
						                              ORDER BY 1 DESC
                            )A
                            GROUP BY ATTENDANCE_STATUS,Title,TitleID

                            UNION ALL

                            SELECT ATTENDANCE_STATUS,COUNT(ATTENDANCE_STATUS) Status,Title,TitleID,
                            CASE WHEN ATTENDANCE_STATUS = 'Late' THEN '#EA1C2D' ELSE 'green' END BackgroundColor,
                            CASE WHEN ATTENDANCE_STATUS = 'Late' THEN '#F45A4D' ELSE '#32bc39' END HoverBackgroundColor  FROM 
                            (
                            SELECT TOP 14 [SIMPLE_DATE] AttendanceDate,
	                                                  [NAGAD_EMP_ID] EmployeeCode
	                                                  ,ISNULL(CONVERT(VARCHAR(10), CAST(MIN([DATE_TIME]) AS TIME), 0),'') IN_TIME
	                                                  ,ISNULL(CONVERT(VARCHAR(10), CAST(MAX([DATE_TIME]) AS TIME), 0),'') OUT_TIME
	                                                  ,CASE 
	                                                  WHEN SUBSTRING(MIN([DATE_TIME]),12,8) > '09:15:00'
	                                                  THEN 'Late'
	                                                  ELSE 'On Time'
	                                                  END ATTENDANCE_STATUS,
						                              'Last 2 Weeks' Title,
						                              'L2W' TitleID
                                                  FROM [attendanceService].[dbo].[EVENT_LOG]
                                                  INNER JOIN HRMS..Employee Emp ON Emp.EmployeeCode = CAST(NAGAD_EMP_ID as varchar(100))  COLLATE Latin1_General_CI_AS 
                                                  WHERE Emp.PersonID = {personID}
                                                  GROUP BY [SIMPLE_DATE]
	                                                  ,[NAGAD_EMP_ID]                      
						                              ORDER BY 1 DESC
                            )A
                            GROUP BY ATTENDANCE_STATUS,Title,TitleID

                            UNION ALL

                            SELECT ATTENDANCE_STATUS,COUNT(ATTENDANCE_STATUS) Status,Title,TitleID ,
                            CASE WHEN ATTENDANCE_STATUS = 'Late' THEN '#EA1C2D' ELSE 'green' END BackgroundColor,
                            CASE WHEN ATTENDANCE_STATUS = 'Late' THEN '#F45A4D' ELSE '#32bc39' END HoverBackgroundColor FROM 
                            (
                            SELECT TOP 30 [SIMPLE_DATE] AttendanceDate,
	                                                  [NAGAD_EMP_ID] EmployeeCode
	                                                  ,ISNULL(CONVERT(VARCHAR(10), CAST(MIN([DATE_TIME]) AS TIME), 0),'') IN_TIME
	                                                  ,ISNULL(CONVERT(VARCHAR(10), CAST(MAX([DATE_TIME]) AS TIME), 0),'') OUT_TIME
	                                                  ,CASE 
	                                                  WHEN SUBSTRING(MIN([DATE_TIME]),12,8) > '09:15:00'
	                                                  THEN 'Late'
	                                                  ELSE 'On Time'
	                                                  END ATTENDANCE_STATUS,
						                              'Last 30 Days' Title,
						                              'L1M' TitleID
                                                  FROM [attendanceService].[dbo].[EVENT_LOG]
                                                  INNER JOIN HRMS..Employee Emp ON Emp.EmployeeCode = CAST(NAGAD_EMP_ID as varchar(100))  COLLATE Latin1_General_CI_AS 
                                                  WHERE Emp.PersonID = {personID}
                                                  GROUP BY [SIMPLE_DATE]
	                                                  ,[NAGAD_EMP_ID]                      
						                              ORDER BY 1 DESC
                            )A
                            GROUP BY ATTENDANCE_STATUS,Title,Title,TitleID";
            var list = EmployeeRepo.GetDataModelCollection<AttendanceSummaryDto>(sql);
            Widget widget = new Widget
            {
                id = "DoughnutWidget",
                title = "My On Time Vs Late Time"
            };

            if (list.Count > 0)
            {

                var listOfRanges = list.GroupBy(p => new { p.TitleID, p.Title }).Select(g => new { g.Key.TitleID, g.Key.Title }).ToList();

                widget.ranges = new Dictionary<string, string>();
                widget.mainChart = new MainChart
                {
                    datasets = new Dictionary<string, List<MainChartDatasets>>()
                };
                foreach (var obj in listOfRanges)
                {
                    widget.ranges.Add(obj.TitleID, obj.Title);

                    MainChartDatasets item = new MainChartDatasets();
                    var datasets = list.Where(x => x.TitleID.Equals(obj.TitleID));
                    item.data = datasets.Select(g => g.TotalCount).ToArray();
                    item.backgroundColor = datasets.Select(g => g.BackgroundColor).ToArray();
                    item.hoverBackgroundColor = datasets.Select(g => g.HoverBackgroundColor).ToArray();
                    widget.mainChart.datasets.Add(obj.TitleID, new List<MainChartDatasets> { item });
                }
                widget.currentRange = widget.ranges.First().Key;
                widget.mainChart.labels = list.Select(g => g.AttendanceStatus).Distinct().ToArray();
                var footerLeft = new Dictionary<string, string>();
                var footerRight = new Dictionary<string, string>();
                foreach (var obj in list)
                {
                    if (obj.AttendanceStatus.Contains("Late"))
                    {
                        footerRight.Add(obj.TitleID, obj.TotalCount.ToString());
                    }
                    else
                    {
                        footerLeft.Add(obj.TitleID, obj.TotalCount.ToString());
                    }
                }
                widget.mainChart.options = new MainChartOptions
                {
                    cutoutPercentage = 50,
                    spanGaps = false,
                    maintainAspectRatio = false,
                    legend = new MainChartOptionsLegend
                    {
                        display = true,
                        position = "bottom",
                        labels = new MainChartOptionsLegendLabel
                        {
                            padding = 16,
                            usePointStyle = true
                        }
                    }
                };
                //widget.footerLeft = new Footer
                //{
                //    title = "On Time",
                //    count = footerLeft
                //};
                //widget.footerRight = new Footer
                //{
                //    title = "Late",
                //    count = footerRight
                //};
                return widget;
            }
            return widget;
        }

        public Widget SelfAttendance(int personID)
        {
            List<AttendanceDto> list = SelfAttendanceList(personID, 30, 1);
            string widgeId = "widget10";
            string widgeTitle = "Self Attendace Details Last 30 days";

            var columnHeaderDef = new List<(string, string)>
            {
               ("Day","Day"),
               ("Date","Date"),
               ("OfficeInTime","Office In Time"),
               ("OfficeOutTime","Last Exit Time"),
               ("ATTENDANCE_STATUS","Attendance Status"),
               ("WORK_HOUR","Work Hour"),
               ("DAY_STATUS","Day Status"),
               ("Action","Action"),
               ("PrimaryID","PrimaryID"),
               ("EmployeeCode","EmployeeCode")
            };

            var cellBodyDef = new List<(string, string, string, string)>
            {
                ("Day","AttendanceDay","font-bold",""),
                ("Date","AttendanceDateString","font-bold",""),
                ("OfficeInTime","IN_TIME","font-bold",""),
                ("OfficeOutTime","OUT_TIME","font-bold",""),
                ("ATTENDANCE_STATUS","ATTENDANCE_STATUS","Conditional:ATTENDANCE_STATUS_STRING",""),
                ("WORK_HOUR","WORK_HOUR","font-bold",""),
                ("DAY_STATUS","DAY_STATUS","font-bold",""),
                ("Action","Action","font-bold",""),
                ("PrimaryID","PrimaryID","font-bold",""),
                ("EmployeeCode","EmployeeCode","font-bold","")
            };
            return GetWidgetFormattedData(widgeId, widgeTitle, list, columnHeaderDef, cellBodyDef);


        }
        private List<AttendanceDto> SelfAttendanceListOld(int personID, int days = 7)
        {
            string sql = @$"SELECT TOP {days} [SIMPLE_DATE] AttendanceDate,
	                      [NAGAD_EMP_ID] EmployeeCode
	                      ,ISNULL(CONVERT(VARCHAR(10), CAST(MIN([DATE_TIME]) AS TIME), 0),'') IN_TIME
	                      ,ISNULL(CONVERT(VARCHAR(10), CAST(MAX([DATE_TIME]) AS TIME), 0),'') OUT_TIME
	                      ,CASE 
	                      WHEN SUBSTRING(MIN([DATE_TIME]),12,8) > '09:15:00'
	                      THEN 'Late'
	                      ELSE 'OK'
	                      END ATTENDANCE_STATUS,
	                      CONVERT(varchar(5), 
                          DATEADD(minute, DATEDIFF(minute,MIN([DATE_TIME]),MAX([DATE_TIME])),0),114) AS WORK_HOUR,
	                      CASE WHEN DATEDIFF(minute,MIN([DATE_TIME]),MAX([DATE_TIME]))>=9*60 THEN 'Full Day'
	                      ELSE 'Partial Day'
	                      END AS DAY_STATUS,
                          '' Action
                      FROM [attendanceService].[dbo].[EVENT_LOG]
                      INNER JOIN HRMS..Employee Emp ON Emp.EmployeeCode = CAST(NAGAD_EMP_ID as varchar(100))  COLLATE Latin1_General_CI_AS 
                      WHERE Emp.PersonID = {personID}
                      GROUP BY [SIMPLE_DATE]
	                      ,[NAGAD_EMP_ID]
                      ORDER BY 1 DESC";
            var list = EmployeeRepo.GetDataModelCollection<AttendanceDto>(sql);
            return list;
        }

        private EmployeeDto GetEmployee(int personID)
        {
            string sql = @$"SELECT EmployeeID FROM ViewALLEmployee where PersonID = {personID}";
            var model = EmployeeRepo.GetModelData<EmployeeDto>(sql);
            return model;
        }

        public async Task<Dictionary<string, object>> GetSelectedDateForEdit(int id)
        {
            string sql = $@"SELECT distinct asu.*, sv.SystemVariableCode AttendanceStatusName,
                        sv1.SystemVariableCode LeaveCategoryName, sv2.SystemVariableCode ApprovalStatusName
						,sc.StartTime,sc.EndTime,sm.BufferTimeInMinute
						,DATEADD(MINUTE, sm.BufferTimeInMinute, sc.StartTime) AS StartWithBuffer

                        FROM 
                        AttendanceSummary asu
						LEFT JOIN ShiftingMaster sm on sm.ShiftingMasterID=asu.ShiftID
						LEFT JOIN ShiftingChild sc on sc.ShiftingMasterID=asu.ShiftID
                        LEFT JOIN Security..SystemVariable sv on sv.SystemVariableID=asu.AttendanceStatus
                        LEFT JOIN Security..SystemVariable sv1 on sv1.SystemVariableID=asu.LeaveCategoryID
                        LEFT JOIN Security..SystemVariable sv2 on sv2.SystemVariableID=asu.ApprovalStatusID
                         WHERE asu.PrimaryID = {id}";
            var listDict = EmployeeRepo.GetData(sql);

            return await Task.FromResult(listDict);
        }
        private List<AttendanceDto> SelfAttendanceList(int personID, int days = 7, int Weekend = 0)
        {
            string concate = string.Empty;//Weekend.IsNotZero() ? @$" AND AttendanceStatus <> {(int)Util.AttendanceStatus.Weekend}" : "";
            switch (Weekend)
            {
                case 1:
                    concate = $@" AND AttendanceStatus <> {(int)Util.AttendanceStatus.Weekend}";
                    break;
                default:
                    break;
            }
            string sql = @$"SELECT TOP {days} 
                              FORMAT(AttendanceDate,'yyyy-dd-MM HH:MM:ss') AttendanceDateNew,
                              AttendanceDate,
	                          ATS.EmployeeCode
	                          ,ISNULL(CONVERT(VARCHAR(10), CAST(InTime AS TIME), 0),'') IN_TIME
	                          ,ISNULL(CONVERT(VARCHAR(10), CAST(OutTime AS TIME), 0),'') OUT_TIME	                       
						      ,SV.SystemVariableDescription ATTENDANCE_STATUS
						      ,IIF(TotalTimeInMin = 0,'00:00',dbo.MinutesToDuration(TotalTimeInMin)) WORK_HOUR
	                          ,ISNULL(DayStatus,'') DAY_STATUS
						      ,AttendanceStatus
                              ,ATS.PrimaryID,
                              '' Action
                              ,ATS.PrimaryID
                              ,HRMS.dbo.MinutesToDuration(ActualWorkingHourInMin) ActualWorkingHour
                              ,FORMAT(AttendanceDate, 'ddd') AttendanceDay
                          FROM AttendanceSummary ATS
                          --INNER JOIN HRMS..Employee Emp ON Emp.EmployeeCode = CAST(ATS.EmployeeCode as varchar(100))  COLLATE Latin1_General_CI_AS 
                          INNER JOIN HRMS..Employee Emp ON Emp.EmployeeID = ATS.EmployeeID
					      INNER JOIN Security..SystemVariable SV ON SV.SystemVariableID = ATS.AttendanceStatus
                          WHERE (Emp.PersonID = {personID} AND CAST(ATS.AttendanceDate as Date) BETWEEN CAST(DATEADD(DAY, -30, GETDATE()) as Date) AND CAST(GETDATE() as Date)) {concate}
                          ORDER BY ATS.AttendanceDate DESC";
            var list = EmployeeRepo.GetDataModelCollection<AttendanceDto>(sql);
            return list;
        }

        private async Task<List<AttendanceDto>> SelfAttendanceListAsync(int personID, int days = 7, int Weekend = 0)
        {
            string concate = string.Empty;//Weekend.IsNotZero() ? @$" AND AttendanceStatus <> {(int)Util.AttendanceStatus.Weekend}" : "";
            switch (Weekend)
            {
                case 1:
                    concate = $@" AND AttendanceStatus <> {(int)Util.AttendanceStatus.Weekend}";
                    break;
                default:
                    break;
            }
            string sql = @$"SELECT TOP {days} 
                              FORMAT(AttendanceDate,'yyyy-dd-MM HH:MM:ss') AttendanceDateNew,
                              AttendanceDate,
	                          ATS.EmployeeCode
	                          ,ISNULL(CONVERT(VARCHAR(10), CAST(InTime AS TIME), 0),'') IN_TIME
	                          ,ISNULL(CONVERT(VARCHAR(10), CAST(OutTime AS TIME), 0),'') OUT_TIME	                       
						      ,SV.SystemVariableDescription ATTENDANCE_STATUS
						      ,IIF(TotalTimeInMin = 0,'00:00',dbo.MinutesToDuration(TotalTimeInMin)) WORK_HOUR
	                          ,ISNULL(DayStatus,'') DAY_STATUS
						      ,AttendanceStatus
                              ,ATS.PrimaryID,
                              '' Action
                              ,ATS.PrimaryID
                              ,HRMS.dbo.MinutesToDuration(ActualWorkingHourInMin) ActualWorkingHour
                              ,FORMAT(AttendanceDate, 'ddd') AttendanceDay
                          FROM AttendanceSummary ATS
                          --INNER JOIN HRMS..Employee Emp ON Emp.EmployeeCode = CAST(ATS.EmployeeCode as varchar(100))  COLLATE Latin1_General_CI_AS 
                          INNER JOIN HRMS..Employee Emp ON Emp.EmployeeID = ATS.EmployeeID
					      INNER JOIN Security..SystemVariable SV ON SV.SystemVariableID = ATS.AttendanceStatus
                          WHERE (Emp.PersonID = {personID} AND CAST(ATS.AttendanceDate as Date) BETWEEN CAST(DATEADD(DAY, -30, GETDATE()) as Date) AND CAST(GETDATE() as Date)) {concate}
                          ORDER BY ATS.AttendanceDate DESC";
            var list = await EmployeeRepo.GetDataModelCollectionAsync<AttendanceDto>(sql);
            return list;
        }
        private List<AttendanceDto> SelfAttendanceListSearch(int personID, DateTime fromdate, DateTime todate)
        {
            string fdate = fromdate.ToString("yyyy-MM-dd");
            string tdate = todate.ToString("yyyy-MM-dd");
            string sql = @$"SELECT *  
                              ,AttendanceDate,
	                          ATS.EmployeeCode
	                          ,ISNULL(CONVERT(VARCHAR(10), CAST(InTime AS TIME), 0),'') IN_TIME
	                          ,ISNULL(CONVERT(VARCHAR(10), CAST(OutTime AS TIME), 0),'') OUT_TIME	                       
						      ,SV.SystemVariableDescription ATTENDANCE_STATUS
						      ,IIF(TotalTimeInMin = 0,'',dbo.MinutesToDuration(TotalTimeInMin)) WORK_HOUR
	                          ,ISNULL(DayStatus,'') DAY_STATUS
						      ,AttendanceStatus
                              ,ATS.PrimaryID,
                              '' Action
                              ,FORMAT(AttendanceDate, 'ddd') AttendanceDay
                          FROM AttendanceSummary ATS
                          INNER JOIN HRMS..Employee Emp ON Emp.EmployeeCode = CAST(ATS.EmployeeCode as varchar(100))  COLLATE Latin1_General_CI_AS 
					      INNER JOIN Security..SystemVariable SV ON SV.SystemVariableID = ATS.AttendanceStatus
                          WHERE Emp.PersonID = {personID} AND ATS.AttendanceDate BETWEEN  '{fdate}' AND '{tdate}'                   
                          ORDER BY ATS.AttendanceDate DESC";
            var list = EmployeeRepo.GetDataModelCollection<AttendanceDto>(sql);
            return list;
        }

        public Widget SupervisorTeammembers(int personID)
        {
            string fdate = DateTime.Now.ToString("yyyy-MM-dd");
            //string sql = @$"SELECT * FROM ViewEmployeeSupervisorMap s
            //                WHERE s.EmployeeSupervisorID = {personID} AND (ISNULL(EmployeeTypeID,0) <> {(int)Util.EmployeeType.Discontinued} OR CAST(DiscontinueDate as Date) >= CAST(getdate() as date))";
            string sql = @$"
                            
                            SELECT s.* ,AttendanceDate,
	                            ATS.EmployeeCode
	                            ,ISNULL(CONVERT(VARCHAR(10), CAST(InTime AS TIME), 0),'') IN_TIME
	                            ,ISNULL(CONVERT(VARCHAR(10), CAST(OutTime AS TIME), 0),'') OUT_TIME	                       
						        ,ISNULL(SV.SystemVariableDescription, '') ATTENDANCE_STATUS
						        ,IIF(TotalTimeInMin = 0,'',dbo.MinutesToDuration(TotalTimeInMin)) WORK_HOUR
	                            ,ISNULL(DayStatus,'') DAY_STATUS
						        ,AttendanceStatus
                                ,ATS.PrimaryID,
                                '' Action
                                ,FORMAT(AttendanceDate, 'ddd') AttendanceDay
                            FROM ViewEmployeeSupervisorMap s
                            left join AttendanceSummary ATS on s.EmployeeCode = CAST(ATS.EmployeeCode as varchar(100))  AND  ATS.AttendanceDate ='{fdate}'
					        left JOIN Security..SystemVariable SV ON SV.SystemVariableID = ATS.AttendanceStatus
                            WHERE  s.EmployeeSupervisorID = {personID} AND (ISNULL(EmployeeTypeID,0) <> 48 OR CAST(DiscontinueDate as Date) >= CAST(getdate() as date))";

            var list = EmployeeRepo.GetDataModelCollection<EmployeeDto>(sql);


            string widgeId = "widget11";
            string widgeTitle = "Team Members";
            string rowKey = "PersonID";

            if (list.Count > 0)
            {
                var columnHeaderDef = new List<(string, string)>
            {
               ("avatar",""),
               ("FullName","Name"),
               ("DesignationName","Designation"),
               ("IN_TIME","Office In Time"),
               ("OUT_TIME","Last Exit Time"),
               ("ATTENDANCE_STATUS","Attendance Status")
            };

                var cellBodyDef = new List<(string, string, string, string)>
            {
                ("avatar","ImagePath","",""),
                ("FullName","FullName","font-bold",""),
                ("DesignationName","DesignationName","font-bold",""),
                ("IN_TIME","IN_TIME","font-bold",""),
                ("OUT_TIME","OUT_TIME","font-bold",""),
                ("ATTENDANCE_STATUS","ATTENDANCE_STATUS","Conditional:ATTENDANCE_STATUS_STRING","")
            };


                return GetWidgetFormattedData(widgeId, widgeTitle, list, columnHeaderDef, cellBodyDef, rowKey); ;
            }
            return new Widget
            {
                id = widgeId,
                title = widgeTitle
            };
        }

        public async Task<Widget> GetAllEmployeeAttendance(string AttendanceType, string SearchText)
        {
            SearchText = SearchText.IsNullOrEmpty() ? "" : SearchText.ToLower().Trim().Replace(" ", "");
            string concate = SearchText.IsNullOrEmpty() ? "" :
                $@"WHERE ( REPLACE(TRIM(LOWER(Emp.FullName)),' ','') like '%{SearchText}%' OR DesignationName like '%{SearchText}%' OR EmployeeCode like '%{SearchText}%' )";

            switch (AttendanceType)
            {
                case "Present":
                    concate += concate.IsNullOrEmpty() ? "WHERE IN_TIME <> ''" : " AND IN_TIME <> ''";
                    break;
                case "Absent":
                    concate += concate.IsNullOrEmpty() ? "WHERE IN_TIME = ''" : " AND IN_TIME = '' ";
                    break;
                default:
                    break;
            }
            string sql = @$"SELECT * FROM ViewGetAllEmployeeAttendance EMp
                            {concate}
                            ORDER BY EMp.PersonID ASC";



            var list = EmployeeRepo.GetDataModelCollection<EmployeeDto>(sql);


            //if (list.Count > 0)
            //{
            string widgeId = "widget11";
            string widgeTitle = @$"{AttendanceType} Employee Attendance";
            string rowKey = "PersonID";

            var columnHeaderDef = new List<(string, string)>
            {
               ("avatar",""),
               ("FullName","Name"),
               ("EmployeeCode","EmployeeCode"),
               ("DesignationName","Designation"),
               ("OfficeInTime","Office In Time"),
               ("OfficeOutTime","Last Exit Time")
            };

            var cellBodyDef = new List<(string, string, string, string)>
            {
                ("avatar","ImagePath","",""),
                ("FullName","FullName","font-bold",""),
                ("EmployeeCode","EmployeeCode","font-bold",""),
                ("DesignationName","DesignationName","font-bold",""),
                ("OfficeInTime","IN_TIME","font-bold",""),
                ("OfficeOutTime","OUT_TIME","font-bold",""),
            };


            return GetWidgetFormattedData(widgeId, widgeTitle, list, columnHeaderDef, cellBodyDef, rowKey); ;
            //}
            //return null;
        }

        public List<object> GetEmployeeAttendanceSummaryBarchart(int PersonID)
        {
            var list = SelfAttendanceList(PersonID, 30);

            var ranges = new Dictionary<string, string> {
                { "TW" ,"Last Week" },
                { "L2W", "Last 2 Weeks" },
                { "L1M", "Last 30 Days" }
            };

          

            var mainChart = new
            {
                TW = new
                {
                    labels = list.Select(x => x.AttendanceDate.ToString("MMM dd")).Take(7).ToArray(),
                    datasets = new object[]
                    {
                        new
                        {
                            type="bar",
                            label="Actual Work Hour",
                            data = list.Select(x=> x.WORK_HOURDecimal = x.WORK_HOUR.Replace(":",".").ToDecimal()).Take(7).ToArray(),
                            backgroundColor = "#42BFF7",
                            hoverBackgroundColor = "#87CDF7"
                        },
                    new
                    {
                        type = "bar",
                        label = "Expected Work Hour",
                        //data = list.Select(x=> x.WORK_HOURDecimal = 9).Take(7).ToArray(),
                        data = list.Select(x=> x.WORK_HOURDecimal = x.ActualWorkingHour.ToString().Replace(":",".").ToDecimal()).Take(7).ToArray(),
                        //data = list.Take(7).Where(x => new[] { 73, 74, 75 }.Contains(x.AttendanceStatus)).Select(x=> x.WORK_HOURDecimal = x.ActualWorkingHour.ToString().Replace(":",".").ToDecimal()).ToArray(),
                        backgroundColor = "#C6ECFD",
                        hoverBackgroundColor = "#D7EFFD"
                    }
                }
                },
                L2W = new
                {
                    labels = list.Select(x => x.AttendanceDate.ToString("MMM dd")).Take(14).ToArray(),
                    datasets = new object[]
                    {
                        new
                        {
                            type="bar",
                            label="Work Hour",
                            data = list.Select(x=> x.WORK_HOURDecimal = x.WORK_HOUR.Replace(":",".").ToDecimal()).Take(14).ToArray(),
                            backgroundColor = "#42BFF7",
                            hoverBackgroundColor = "#87CDF7"
                        },
                        new
                        {
                            type="bar",
                            label="Expected Work Hour",
                            //data = list.Select(x=> x.WORK_HOURDecimal =9).Take(14).ToArray(),
                            data = list.Select(x=> x.WORK_HOURDecimal = x.ActualWorkingHour.ToString().Replace(":",".").ToDecimal()).Take(14).ToArray(),
                            //data = list.Take(14).Where(x => new[] { 73, 74, 75 }.Contains(x.AttendanceStatus)).Select(x=> x.WORK_HOURDecimal = x.ActualWorkingHour.ToString().Replace(":",".").ToDecimal()).ToArray(),

                            backgroundColor = "#C6ECFD",
                            hoverBackgroundColor = "#D7EFFD"
                        }
                    }
                },
                L1M = new
                {
                    labels = list.Select(x => x.AttendanceDate.ToString("MMM dd")).ToArray(),
                    datasets = new object[]
                    {
                        new
                        {
                            type="bar",
                            label="Work Hour",
                            data = list.Select(x=> x.WORK_HOURDecimal = x.WORK_HOUR.Replace(":",".").ToDecimal()).ToArray(),
                            backgroundColor = "#42BFF7",
                            hoverBackgroundColor = "#87CDF7"
                        },
                        new
                        {
                            type="bar",
                            label="Expected Work Hour",
                            //data = list.Select(x=> x.WORK_HOURDecimal =9 ).ToArray(),
                            data =  list.Select(x=> x.WORK_HOURDecimal = x.ActualWorkingHour.ToString().Replace(":",".").ToDecimal()).ToArray(),
                            //data = list.Where(x => new[] { 73, 74, 75 }.Contains(x.AttendanceStatus)).Select(x=> x.WORK_HOURDecimal = x.ActualWorkingHour.ToString().Replace(":",".").ToDecimal()).ToArray(),

                            backgroundColor = "#C6ECFD",
                            hoverBackgroundColor = "#D7EFFD"
                        }
                    }
                }
            };
            var scales = new
            {
                xAxes = new object[]
                {
                    new
                    {
                        stacked = false,
                        display = true,
                        gridLines = new
                        {
                            display = true
                        }
                    }
                },
                yAxes = new object[]
                {
                    new
                    {
                        stacked = false,
                        type = "linear",
                        display = true,
                        position= "left",
                        gridLines = new
                        {
                            display = true
                        },
                        labels = new
                        {
                            show = true
                        },
                        ticks = new
                        {
                            beginAtZero = true
                        }
                    }
                }
            };
            var footerLeft = new
            {
                title = "Work Hour",
                count = new
                {
                    TW = list.Select(x => x.WORK_HOURDecimal = x.WORK_HOUR.Replace(":", ".").ToDecimal()).Take(7).Sum(),
                    L2W = list.Select(x => x.WORK_HOURDecimal = x.WORK_HOUR.Replace(":", ".").ToDecimal()).Take(14).Sum(),
                    L1M = list.Select(x => x.WORK_HOURDecimal = x.WORK_HOUR.Replace(":", ".").ToDecimal()).Sum()
                }
            };

            foreach (var item in list)
            {
                item.WORK_HOURDecimal = decimal.Parse(item.ActualWorkingHour.Replace(":", "."));
            }
            var footerRight = new
            {
                title = "Expected Work Hour",
                count = new
                {

                    //TW = list.Select(x => x.WORK_HOURDecimal = x.ActualWorkingHour.Replace(":", ".").ToDecimal()).Take(7).Sum(),
                    //L2W = list.Select(x => x.WORK_HOURDecimal = x.ActualWorkingHour.Replace(":", ".").ToDecimal()).Take(14).Sum(),
                    //L1M = list.Select(x => x.WORK_HOURDecimal = x.ActualWorkingHour.Replace(":", ".").ToDecimal()).Sum()

                    TW = list.Take(7).Where(x => new[] { 73, 74, 75,88,214,215 }.Contains(x.AttendanceStatus)).Sum(x => x.WORK_HOURDecimal),

                    L2W = list.Take(14).Where(x => new[] { 73, 74, 75, 88, 214, 215 }.Contains(x.AttendanceStatus)).Sum(x => x.WORK_HOURDecimal),

                    L1M = list.Where(x => new[] { 73, 74, 75, 88, 214, 215 }.Contains(x.AttendanceStatus)).Sum(x => x.WORK_HOURDecimal)

                }
            };
            var widgets = new List<object>{
                new {
                id = "barchart",
                title = "My Actual Vs Expected Time",
                ranges,
                mainChart,
                options = new
                {
                    responsive = true,
                    maintainAspectRatio = false,
                    legend = new
                    {
                        display = false
                    },
                    tooltips = new
                    {
                        mode = "label"
                    },
                    scales
                },
                footerLeft,
                footerRight
                }
            };

            return widgets;
        }
        public async Task<List<object>> GetEmployeeAttendanceSummaryBarchartAsync(int PersonID)
        {
            var list =await SelfAttendanceListAsync(PersonID, 30);

            var ranges = new Dictionary<string, string> {
                { "TW" ,"Last Week" },
                { "L2W", "Last 2 Weeks" },
                { "L1M", "Last 30 Days" }
            };



            var mainChart = new
            {
                TW = new
                {
                    labels = list.Select(x => x.AttendanceDate.ToString("MMM dd")).Take(7).ToArray(),
                    datasets = new object[]
                    {
                        new
                        {
                            type="bar",
                            label="Actual Work Hour",
                            data = list.Select(x=> x.WORK_HOURDecimal = x.WORK_HOUR.Replace(":",".").ToDecimal()).Take(7).ToArray(),
                            backgroundColor = "#42BFF7",
                            hoverBackgroundColor = "#87CDF7"
                        },
                    new
                    {
                        type = "bar",
                        label = "Expected Work Hour",
                        //data = list.Select(x=> x.WORK_HOURDecimal = 9).Take(7).ToArray(),
                        data = list.Select(x=> x.WORK_HOURDecimal = x.ActualWorkingHour.ToString().Replace(":",".").ToDecimal()).Take(7).ToArray(),
                        //data = list.Take(7).Where(x => new[] { 73, 74, 75 }.Contains(x.AttendanceStatus)).Select(x=> x.WORK_HOURDecimal = x.ActualWorkingHour.ToString().Replace(":",".").ToDecimal()).ToArray(),
                        backgroundColor = "#C6ECFD",
                        hoverBackgroundColor = "#D7EFFD"
                    }
                }
                },
                L2W = new
                {
                    labels = list.Select(x => x.AttendanceDate.ToString("MMM dd")).Take(14).ToArray(),
                    datasets = new object[]
                    {
                        new
                        {
                            type="bar",
                            label="Work Hour",
                            data = list.Select(x=> x.WORK_HOURDecimal = x.WORK_HOUR.Replace(":",".").ToDecimal()).Take(14).ToArray(),
                            backgroundColor = "#42BFF7",
                            hoverBackgroundColor = "#87CDF7"
                        },
                        new
                        {
                            type="bar",
                            label="Expected Work Hour",
                            //data = list.Select(x=> x.WORK_HOURDecimal =9).Take(14).ToArray(),
                            data = list.Select(x=> x.WORK_HOURDecimal = x.ActualWorkingHour.ToString().Replace(":",".").ToDecimal()).Take(14).ToArray(),
                            //data = list.Take(14).Where(x => new[] { 73, 74, 75 }.Contains(x.AttendanceStatus)).Select(x=> x.WORK_HOURDecimal = x.ActualWorkingHour.ToString().Replace(":",".").ToDecimal()).ToArray(),

                            backgroundColor = "#C6ECFD",
                            hoverBackgroundColor = "#D7EFFD"
                        }
                    }
                },
                L1M = new
                {
                    labels = list.Select(x => x.AttendanceDate.ToString("MMM dd")).ToArray(),
                    datasets = new object[]
                    {
                        new
                        {
                            type="bar",
                            label="Work Hour",
                            data = list.Select(x=> x.WORK_HOURDecimal = x.WORK_HOUR.Replace(":",".").ToDecimal()).ToArray(),
                            backgroundColor = "#42BFF7",
                            hoverBackgroundColor = "#87CDF7"
                        },
                        new
                        {
                            type="bar",
                            label="Expected Work Hour",
                            //data = list.Select(x=> x.WORK_HOURDecimal =9 ).ToArray(),
                            data =  list.Select(x=> x.WORK_HOURDecimal = x.ActualWorkingHour.ToString().Replace(":",".").ToDecimal()).ToArray(),
                            //data = list.Where(x => new[] { 73, 74, 75 }.Contains(x.AttendanceStatus)).Select(x=> x.WORK_HOURDecimal = x.ActualWorkingHour.ToString().Replace(":",".").ToDecimal()).ToArray(),

                            backgroundColor = "#C6ECFD",
                            hoverBackgroundColor = "#D7EFFD"
                        }
                    }
                }
            };
            var scales = new
            {
                xAxes = new object[]
                {
                    new
                    {
                        stacked = false,
                        display = true,
                        gridLines = new
                        {
                            display = true
                        }
                    }
                },
                yAxes = new object[]
                {
                    new
                    {
                        stacked = false,
                        type = "linear",
                        display = true,
                        position= "left",
                        gridLines = new
                        {
                            display = true
                        },
                        labels = new
                        {
                            show = true
                        },
                        ticks = new
                        {
                            beginAtZero = true
                        }
                    }
                }
            };
            var footerLeft = new
            {
                title = "Work Hour",
                count = new
                {
                    TW = list.Select(x => x.WORK_HOURDecimal = x.WORK_HOUR.Replace(":", ".").ToDecimal()).Take(7).Sum(),
                    L2W = list.Select(x => x.WORK_HOURDecimal = x.WORK_HOUR.Replace(":", ".").ToDecimal()).Take(14).Sum(),
                    L1M = list.Select(x => x.WORK_HOURDecimal = x.WORK_HOUR.Replace(":", ".").ToDecimal()).Sum()
                }
            };

            foreach (var item in list)
            {
                item.WORK_HOURDecimal = decimal.Parse(item.ActualWorkingHour.Replace(":", "."));
            }
            var footerRight = new
            {
                title = "Expected Work Hour",
                count = new
                {

                    //TW = list.Select(x => x.WORK_HOURDecimal = x.ActualWorkingHour.Replace(":", ".").ToDecimal()).Take(7).Sum(),
                    //L2W = list.Select(x => x.WORK_HOURDecimal = x.ActualWorkingHour.Replace(":", ".").ToDecimal()).Take(14).Sum(),
                    //L1M = list.Select(x => x.WORK_HOURDecimal = x.ActualWorkingHour.Replace(":", ".").ToDecimal()).Sum()

                    TW = list.Take(7).Where(x => new[] { 73, 74, 75, 88, 214, 215 }.Contains(x.AttendanceStatus)).Sum(x => x.WORK_HOURDecimal),

                    L2W = list.Take(14).Where(x => new[] { 73, 74, 75, 88, 214, 215 }.Contains(x.AttendanceStatus)).Sum(x => x.WORK_HOURDecimal),

                    L1M = list.Where(x => new[] { 73, 74, 75, 88, 214, 215 }.Contains(x.AttendanceStatus)).Sum(x => x.WORK_HOURDecimal)

                }
            };
            var widgets = new List<object>{
                new {
                id = "barchart",
                title = "My Actual Vs Expected Time",
                ranges,
                mainChart,
                options = new
                {
                    responsive = true,
                    maintainAspectRatio = false,
                    legend = new
                    {
                        display = false
                    },
                    tooltips = new
                    {
                        mode = "label"
                    },
                    scales
                },
                footerLeft,
                footerRight
                }
            };

            return widgets;
        }
        public async Task<List<Widget>> GetSelfAttendanceSearch(AttendanceSummaryHRMSDto data)
        {
            var widgets = new List<Widget>();
            Widget widget10 = SelfAttendanceSearch(data);
            Widget widget11 = SupervisorTeammembers(data.EmployeeID);
            Widget DoughnutWidget = GetEmployeeAttendanceDoughnutWidget(data.PersonID);
            widgets.Add(widget10);
            widgets.Add(widget11);
            widgets.Add(DoughnutWidget);
            return widgets;
        }
        public Widget SelfAttendanceSearch(AttendanceSummaryHRMSDto data)
        {
            string dateString = data.FromDate.ToString();
            string widgeId = "";
            string widgeTitle = "";
            List<AttendanceDto> list = new List<AttendanceDto>();
            if (dateString == "1/1/0001 12:00:00 AM")
            {
                DateTime NewFromDate = DateTime.Now.AddDays(-30);
                DateTime NewToDate = DateTime.Now;
                list = SelfAttendanceListSearch(data.PersonID, NewFromDate, NewToDate);
                widgeId = "widget10";
                widgeTitle = "Self Attendace Details for 30 days";

            }
            else
            {
                list = SelfAttendanceListSearch(data.PersonID, data.FromDate, data.ToDate);
                widgeId = "widget10";
                widgeTitle = "Self Attendace Details for " + ((data.ToDate - data.FromDate).TotalDays + 1) + " days";

            }

            var columnHeaderDef = new List<(string, string)>
            {
               ("Day","Day"),
               ("Date","Date"),
               ("OfficeInTime","Office In Time"),
               ("OfficeOutTime","Last Exit Time"),
               ("ATTENDANCE_STATUS","Attendance Status"),
               ("WORK_HOUR","Work Hour"),
               ("DAY_STATUS","Day Status"),
               ("Action","Action"),
               ("PrimaryID","PrimaryID"),
               ("EmployeeCode","EmployeeCode")
            };

            var cellBodyDef = new List<(string, string, string, string)>
            {
                ("Day","AttendanceDay","font-bold",""),
                ("Date","AttendanceDateString","font-bold",""),
                ("OfficeInTime","IN_TIME","font-bold",""),
                ("OfficeOutTime","OUT_TIME","font-bold",""),
                ("ATTENDANCE_STATUS","ATTENDANCE_STATUS","Conditional:ATTENDANCE_STATUS_STRING",""),
                ("WORK_HOUR","WORK_HOUR","font-bold",""),
                ("DAY_STATUS","DAY_STATUS","font-bold",""),
                ("Action","Action","font-bold",""),
                ("PrimaryID","PrimaryID","font-bold",""),
                ("EmployeeCode","EmployeeCode","font-bold","")
            };
            return GetWidgetFormattedData(widgeId, widgeTitle, list, columnHeaderDef, cellBodyDef);


        }


        public async Task<IEnumerable<Dictionary<string, object>>> GetAttendanceSummaryDetails(string employeeCode, DateTime attendanceDate)
        {
            string sql = $@"EXEC HRMS..GetAttendanceSummaryDetails '{attendanceDate.ToString("yyyy-MM-dd")}','{employeeCode}'";
            var list = EmployeeRepo.GetDataDictCollection(sql);
            return await Task.FromResult(list);
        }
        public async Task<IEnumerable<Dictionary<string, object>>> GetUnapprovedRemoteAttendanceDetails(string employeeCode, DateTime attendanceDate)
        {
            string sql = $@"SELECT * FROM HRMS..RemoteAttendanceListExceptApproved where EmployeeCode='{employeeCode}' AND CAST(AttendanceDate as Date) = CAST('{attendanceDate.ToString("yyyy-MM-dd")}' as Date)";
            var list = EmployeeRepo.GetDataDictCollection(sql);
            return await Task.FromResult(list);
        }
        public async Task<IEnumerable<Dictionary<string, object>>> GetHrEditedAttendanceDetails(string employeeCode, DateTime attendanceDate)
        {
            string sql = $@"select EmployeeCode,CONVERT(varchar(15), CONVERT(time, InTime), 100) AS IN_TIME, 
                            CONVERT(varchar(15), CONVERT(time, OutTime), 100) AS OUT_TIME,IsEdited Gateway,CreatedDate SIMPLE_DATE,HRNote Note,RowVersion RowID,HRMS.dbo.MinutesToDuration(TotalTimeInMin) as TotalTime from AttendanceSummary where IsEdited=1 AND EmployeeCode='{employeeCode}' AND CAST(AttendanceDate as Date) = CAST('{attendanceDate.ToString("yyyy-MM-dd")}' as Date)";
            var list = EmployeeRepo.GetDataDictCollection(sql);
            return await Task.FromResult(list);
        }


    }
}