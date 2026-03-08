using HRMS.DAL.Entities;
using Security.DAL.Entities;
using Security.Manager;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Security.API.Models
{
    public class EmployeeSaveModel
    {
        public Employee EmployeeModel { get; set; }
        public Employment EmploymentModel { get; set; }
        public EmployeeBankInfo BankInfoModel { get; set; }
        public List<EmployeeSupervisorMap> EmployeeSupervisorMapModel { get; set; }
    }
}
