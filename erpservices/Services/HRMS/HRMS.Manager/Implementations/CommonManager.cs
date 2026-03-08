using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using HRMS.DAL.Entities;
using HRMS.Manager.Interfaces;
using Manager.Core;
using Manager.Core.Mapper;
using Security.DAL.Entities;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Util;

namespace HRMS.Manager.Implementations
{
    class CommonManager : ManagerBase, ICommonManager
    {

        readonly IRepository<Employee> EmployeeRepo;
        readonly IRepository<EmployeeSupervisorMap> EmployeeSupervisorMapRepo;
        readonly IRepository<Employment> EmploymentRepo;

        public CommonManager(IRepository<Employee> employeeRepo, IRepository<EmployeeSupervisorMap> supervisorRepo, IRepository<Employment> employmentRepo
            )
        {
            EmployeeRepo = employeeRepo;
            EmployeeSupervisorMapRepo = supervisorRepo;
            EmploymentRepo = employmentRepo;
        }


        public async Task DiscontinuedEmployeeAction(int EmployeeID)
        {
            using (var unitOfWork = new UnitOfWork())
            {               
                var supMapList = EmployeeSupervisorMapRepo.Entities.Where(x => x.EmployeeSupervisorID == EmployeeID).ToList();
                supMapList.ForEach(x => x.SetDeleted());
                EmployeeSupervisorMapRepo.AddRange(supMapList);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }

    }
}
