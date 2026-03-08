using Core;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
   public interface IAnnualLeaveEncashmentWindowManager
    {
        
        Task<AnnualLeaveEncashmentWindowMasterDto> GetAnnualLeaveEncashmentWindowMaster(int id);
        Task<List<AnnualLeaveEncashmentWindowChildDto>> GetAnnualLeaveEncashmentWindowChild(int id);
        Task<AnnualLeaveEncashmentPolicySettingsDto> GetAnnualLeaveEncashmentSettings();
        Task<(bool, string)> Save(AnnualLeaveEncashmentPolicySettingsDto settings);
        Task<List<AnnualLeaveEncashmentWindowMasterDto>> GetAll();
        void UpdateLeaveEncashmentStatus(long ALEWMasterID,int Status);
    }
}
