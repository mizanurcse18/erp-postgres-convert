using Accounts.Manager.Dto;
using Core;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Accounts.Manager
{
    public interface IPettyCashDisbursementManager
    {

        GridModel GetAllPettyCashDisbursementClaimList(GridParameter parameters);
        Task<(bool, string)> DisburseClaim(int MasterID,int ClaimTypeID, string DisbursementRemarks);

    }
}
