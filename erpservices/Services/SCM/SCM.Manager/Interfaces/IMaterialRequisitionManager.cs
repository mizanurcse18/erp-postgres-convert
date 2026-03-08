using Core;
using Manager.Core.CommonDto;
using SCM.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SCM.Manager.Interfaces
{
    public interface IMaterialRequisitionManager
    {
        Task<(bool, string)> SaveChanges(MaterialRequisitionDto MR); 
         Task<List<MaterialRequisitionMasterDto>> GetMaterialRequisitionList(string filterData);
        Task<MaterialRequisitionMasterDto> GetMaterialRequisitionMaster(int MRMasterID);
        Task<MaterialRequisitionMasterDto> GetMaterialRequisitionMasterByID(int MRMasterID);
        Task<List<MaterialRequisitionChildDto>> GetMaterialRequisitionChild(int MRMasterID);
        Task<List<MaterialRequisitionChildDto>> GetMaterialRequisitionChildForPO(int MRMasterID);
        IEnumerable<Dictionary<string, object>> GetApprovalComment(int aprovalProcessId);

        List<Attachments> GetAttachments(int MRMasterID);
        Task<List<ManualApprovalPanelEmployeeDto>> GetMRApprovalPanelDefault(int MRMasterID);
        Task<List<ComboModel>> GetForwardingMemberList(int ApprovalProcessID);
        Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId);
        Task<List<MaterialRequisitionMasterDto>> GetAllApproved();
        List<Attachments> GetAssesments(int MRMasterID);
        bool GetIsAssessmentMember(); 
         Task<IEnumerable<Dictionary<string, object>>> GetsSupplierFromMRQuotation(int MRID);
        IEnumerable<Dictionary<string, object>> ReportForMRApprovalFeedback(int MRID);
        IEnumerable<Dictionary<string, object>> GetForwardingMemberComments(int APTypeID, int ReferenceID);
        Task<List<ManualApprovalPanelEmployeeDto>> GetSCMMembersForPanel(int POMasterID);
        List<ManualApprovalPanelEmployeeDto> GetDefaultMRApprovalPanel();

        #region Grid
        Task<GridModel> GetMRListForGrid(GridParameter parameters);
        Task<GridModel> GetApproveMRListForGrid(GridParameter parameters);
        
        #endregion
    }
}
