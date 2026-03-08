using Core;
using Accounts.Manager.Dto;
using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.Manager.Interfaces
{
    public interface IBankManager
    {
        Task<List<BankDto>> GetBankList();
        void SaveChanges(BankDto chequeBookDto);
        bool DeleteBank(int bankID);
        Task<BankDto> GetBank(int chequeBookId);

    }
}
