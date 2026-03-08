using Accounts.DAL.Entities;
using Accounts.Manager.Dto;
using Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.Manager.Interfaces
{
   public interface ICustodianWalletManager
    {
        
        Task<CustodianWalletDto> Get(int id);
        Task<List<PettyCashTransactionHistory>> TransactionDetailsByCWID(int id);
        Task<(bool, string)> Save(CustodianWalletDto Wallets);
        GridModel GetAll(GridParameter parameters);
        bool DeleteWallet(int CWID);
        List<Attachments> GetAttachments(int CWID);
    }
}
