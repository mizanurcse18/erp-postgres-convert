using Core;
using HRMS.Manager.Dto;
using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface IAttendanceManager
    {
        Task<List<Widget>> GetAttendanceWidgets(int personID);
        Task<Dictionary<string, object>> GetSelectedDateForEdit(int id);
        Widget SelfAttendance(int personID);
        Task<List<Widget>> GetSelfAttendanceSearch(AttendanceSummaryHRMSDto data);
        Widget SelfAttendanceSearch(AttendanceSummaryHRMSDto data);

        Widget SupervisorTeammembers(int personID);
        Task<Widget> GetAllEmployeeAttendance(string AttendanceType, string SearchText);
        List<object> GetEmployeeAttendanceSummaryBarchart(int PersonID);
        Task<List<object>> GetEmployeeAttendanceSummaryBarchartAsync(int PersonID);
        Task<IEnumerable<Dictionary<string, object>>> GetAttendanceSummaryDetails(string employeeCode, DateTime attendanceDate);

        void UpdateAttendance(AttendanceSummaryHRMSDto data);
        Task<IEnumerable<Dictionary<string, object>>> GetUnapprovedRemoteAttendanceDetails(string employeeCode, DateTime attendanceDate);
        Task<IEnumerable<Dictionary<string, object>>> GetHrEditedAttendanceDetails(string employeeCode, DateTime attendanceDate);
    }
}
