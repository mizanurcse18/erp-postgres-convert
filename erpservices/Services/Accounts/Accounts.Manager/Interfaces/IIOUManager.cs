using Accounts.Manager.Dto;
using Core;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Accounts.Manager
{
    public interface IIOUManager
    {
        Task<List<IOUMasterDto>> GetIOUList();
        Task<IOUMasterDto> GetIOU(int IOUID); 
        //Task<IOUDto> GetIOUForReAssessment(int NFAID);
        GridModel GetIOUClaimList(GridParameter parameters);
        Task<List<IOUChildDto>> GetIOUChild(int IOUID);
        //Task<IEnumerable<Dictionary<string, object>>> GetIOUList();
        Task<(bool, string)> SaveChanges(IOUDto iou);
        Task RemoveIOUMaster(int IOUMasterID);

        IEnumerable<Dictionary<string, object>> GetApprovalComment(int aprovalProcessId);

        Task<List<ComboModel>> GetForwardingMemberList(int ReferenceID, int APTypeID, int APPanelID);
        Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId);

    }
}
