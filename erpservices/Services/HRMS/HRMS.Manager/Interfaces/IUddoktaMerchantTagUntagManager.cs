using Core;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface IUddoktaMerchantTagUntagManager
    {
        Task<List<Dictionary<string, object>>> GetUddoktaMerchantList();
        // void SaveChanges(UserWiseUddoktaOrMerchantMapping obj);
        Task<(bool, string)> SaveChanges(UserWiseUddoktaOrMerchantMapping userWiseUddoktaOrMerchantMapping);
        bool Delete(int MapID);
        Task<Dictionary<string, object>> GetUddoktaMerchant(int MapID);

    }
}
