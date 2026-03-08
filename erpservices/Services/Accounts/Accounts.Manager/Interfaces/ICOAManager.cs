using Core;
using Accounts.Manager.Dto;
using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Accounts.DAL.Entities;

namespace Accounts.Manager.Interfaces
{
    public interface ICOAManager
    {
        Task<List<Dictionary<string,object>>> GetCOAListDictionary();
        Task<List<Dictionary<string, object>>> GetCOAList();
        Task<List<Dictionary<string, object>>> GetCOAReportList();
        Task<ChartOfAccountsDto> SaveChanges(ChartOfAccountsDto model);
    }
}
