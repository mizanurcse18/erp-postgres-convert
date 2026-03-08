using Core;
using HRMS.Manager.Dto;
using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface IWageCodeConfigurationManager
    {
        Task<IEnumerable<Dictionary<string, object>>> GetWageCodeConfigurationListDic();
        Task<Dictionary<string, object>> GetWageCodeConfiguration(int WageCodeConfigurationId);

        void SaveChanges(WageCodeConfigurationDto WageCodeConfigurationDto);
        Task Delete(int WageCodeConfigurationID );

    }
}
