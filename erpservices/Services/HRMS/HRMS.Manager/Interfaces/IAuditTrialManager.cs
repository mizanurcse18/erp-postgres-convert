using Core;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface IAuditTrialManager
    {

        GridModel GetAllAuditTrialList(GridParameter parameters);
        Task<List<Dictionary<string, object>>> GetAllAuditTrialListForExcel(GridParameter parameters);
        Task<List<Dictionary<string, object>>> GetAuditTrialData(int patID);
        
    }
}
