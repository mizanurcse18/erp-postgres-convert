using Core;
using Manager.Core.CommonDto;
using SCM.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SCM.Manager.Interfaces
{
    public interface IQCManager
    {
        Task<(bool, string)> SaveChanges(QCDto MR);

        GridModel GetQCList(GridParameter parameters);
        GridModel GetAllQCList(GridParameter parameters);
        Task<QCMasterDto> GetQCMaster(int MRID);
        Task<QCMasterDto> GetQCMasterFromAllList(int MRID); 
         Task<List<QCChildDto>> GetQCChild(int MRID);
        Task<RTVMasterDto> GetRTVMaster(int MRID);
        Task<List<RTVChildDto>> GetRTVChild(int MRID);
        List<Attachments> GetAttachments(int QCMID);
        List<ManualApprovalPanelEmployeeDto> GetGRNApprovalPanelDefault(int QCMID);
        Task<List<ManualApprovalPanelEmployeeDto>> GetQCApprovalPanelDefault(int QCMID);


        IEnumerable<Dictionary<string, object>> GetApprovalComment(int aprovalProcessId);


        Task<List<ComboModel>> GetForwardingMemberList(int ReferenceID, int APTypeID, int APPanelID);
        Task<List<ComboModel>> GetForwardingMemberList(int ApprovalProcessID);
        Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId);
        IEnumerable<Dictionary<string, object>> ReportForGRNApprovalFeedback(int POID);
        IEnumerable<Dictionary<string, object>> ReportForQCApprovalFeedback(int QCMID);
    }
}
