using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using Security.DAL.Entities;
using Security.Manager;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRMS.API.Models
{
    public class CompanyLeavePolicySaveModel
    {
        public CompanyLeavePolicyDto MasterModel { get; set; }
        public List<CompanyLeavePolicyDto> ChildModels { get; set; }
    }
}
