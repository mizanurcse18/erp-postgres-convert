using Core;
using HRMS.Manager.Dto;
using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface IWalletConfigurationManager
    {
        Task<List<WalletConfigurationDto>> GetWalletConfigurationListDic();
        Task<WalletConfigurationDto> GetWalletConfiguration(decimal cashoutrate);

        void SaveChanges(WalletConfigurationDto WalletConfigurationDto);
        Task Delete(decimal cashoutrate );

    }
}
