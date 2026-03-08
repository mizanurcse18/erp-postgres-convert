using Core;
using Manager.Core.CommonDto;
using SCM.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SCM.Manager.Interfaces
{
    public interface IMaterialReceiveManager
    {
        Task<(bool, string)> SaveChanges(MaterialReceiveDto MR);

        GridModel GetMaterialReceiveList(GridParameter parameters);
        GridModel GetAllGRNList(GridParameter parameters);
        Task<MaterialReceiveMasterDto> GetMaterialReceiveMaster(int MRID);
        Task<MaterialReceiveMasterDto> GetMaterialReceiveMasterFromAllList(int MRID); 
         Task<List<MaterialReceiveChildDto>> GetMaterialReceiveChild(int MRID);
        Task<List<ManualApprovalPanelEmployeeDto>> GetGRNApprovalPanelDefault(int QCMID);


        List<Attachments> GetAttachments(int PRMasterID);

        Task<GridModel> GetMaterialReceiveListForService(GridParameter parameters);
        //Task<List<MaterialReceiveMasterDto>> GetMaterialReceiveListForService(string filterData);
        Task<MaterialReceiveMasterDto> GetMaterialReceiveMasterForService(int MRID);
        Task<List<MaterialReceiveChildDto>> GetMaterialReceiveChildForService(int MRID);
        IEnumerable<Dictionary<string, object>> GetApprovalComment(int aprovalProcessId);


        Task<List<ComboModel>> GetForwardingMemberList(int ReferenceID, int APTypeID, int APPanelID);
        Task<List<ComboModel>> GetForwardingMemberList(int ApprovalProcessID);
        Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId);
        IEnumerable<Dictionary<string, object>> GetForwardingMemberComments(int APTypeID, int ReferenceID);
        IEnumerable<Dictionary<string, object>> ReportForGRNApprovalFeedback(int MRID);
    }
}
