using Core;
using HRMS.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface IExternalAuditManager
    {
        Task<(bool, string)> SaveChanges(AuditMasterDto dto);
        GridModel GetListForGrid(GridParameter parameters);
        GridModel GetAllListForGrid(GridParameter parameters);
        Task<dynamic> GetExternalAuditMaster(int EAMID);
        Task<List<AuditChildDto>> GetExternalAuditChild(int EAMID);
        Task<dynamic> GetDepartmentDetails(int EAMID);
        Task<(bool, string)> AddNewWallet(string walletNo, string walletName, int walletTypeID);
        IEnumerable<Dictionary<string, object>> GetApprovalComment(int approvalProcessID);
        Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId);
        IEnumerable<Dictionary<string, object>> EmployeeApprovalMemberFeedbackForExternalAudit(int EAMID, int ApprovalProcessID);
        Task<List<ExternalAuditQuestionDeptPOSMDto>> GetExternalAuditQuestionDeptPOSM();
        Task<List<Attachments>> GetMasterAttachments(int EAMID);
    }
}
