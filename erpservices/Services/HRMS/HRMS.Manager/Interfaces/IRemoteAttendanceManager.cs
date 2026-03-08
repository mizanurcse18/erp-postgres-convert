using Core;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface IRemoteAttendanceManager
    {
        Task<RemoteAttendanceDto> SaveChanges(RemoteAttendanceDto master);

        Task<List<RemoteAttendanceDto>> GetRemoteAttendanceListExceptApproved();
        Task<Dictionary<string, object>> GetPresentRemoteAttendance();
        Task<Dictionary<string, object>> GetLastEntryType(); 
           Task<Dictionary<string, object>> GetPresentRemoteAttendanceFromMachine();
        Task<List<RemoteAttendanceDto>> GetPendingRemoteAttendanceList();
        Task<List<RemoteAttendanceDto>> GetPendingRemoteAttendanceListForDashboard(); 
         Task<RemoteAttendanceDto> GetRemoteAttendanceDetails(RemoteAttendanceDto data);
        
        Task<RemoteAttendanceDto> ApproverStatusChange(RemoteAttendanceDto remoteAttendanceDto);
        Task<RemoteAttendanceDto> SelectedApproverStatusChange(RemoteAttendanceDto remoteAttendanceDto);
        Task<IEnumerable<Dictionary<string, object>>> GetIPAddressList();

        Task<GridModel> GetListForGrid(GridParameter parameters);
        Task<GridModel> GetListForGridAll(GridParameter parameters);
        Task<IEnumerable<Dictionary<string, object>>> GetListRemoteAttendanceExcel(GridParameter parameters);
    }
}
