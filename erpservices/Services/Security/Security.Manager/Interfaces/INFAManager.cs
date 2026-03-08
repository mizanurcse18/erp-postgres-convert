using Core;
using Security.DAL.Entities;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Security.Manager
{
    public interface INFAManager
    {
        Task<List<NFAMasterDto>> GetNFAList(string filterData);
        Task<NFAMasterDto> GetNFA(int NFAID);
        Task<List<NFAChildDto>> GetNFAChild(int NFAID);
        //Task<IEnumerable<Dictionary<string, object>>> GetNFAList();
        Task<(bool, string)> SaveChanges(NFADto nfa);
        IEnumerable<Dictionary<string, object>> GetApprovalComment(int aprovalProcessId);

        List<Attachments> GetAttachments(int NFAID);
        Task<List<ComboModel>> GetForwardingMemberList(int ReferenceID, int APTypeID,int APPanelID);
        Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId);
        Task<NFADto> GetNFAForReAssessment(int NFAID);

        //Report
        IEnumerable<Dictionary<string, object>> ReportForNFAAttachments(int NFAID);
        Dictionary<string, object> ReportForNFAMaster(int NFAID);
        IEnumerable<Dictionary<string, object>> ReportForNFAChild(int NFAID);
        IEnumerable<Dictionary<string, object>> ReportForNFAApprovalFeedback(int NFAID);
        Task RemoveNFA(int NFAID, int aprovalProcessId);
        IEnumerable<Dictionary<string, object>> GetForwardingMemberComments(int APTypeID, int ReferenceID);

        #region Grid
        GridModel GetListForGrid(GridParameter parameters);
        GridModel GetListForGridNFADashboard(GridParameter parameters);
        #endregion

        Task<(bool, string)> SaveChangesStrategicNFA(NFADto nfa);
        Task<List<NFAChildStrategicDto>> GetNFAChildStrategic(int NFAID);
        Task<NFADto> GetStrategicNFAForReAssessment(int NFAID);

    }
}
