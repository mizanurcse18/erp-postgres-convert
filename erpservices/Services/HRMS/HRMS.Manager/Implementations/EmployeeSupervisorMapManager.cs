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
    class EmployeeSupervisorMapManager : ManagerBase, IEmployeeSupervisorMapManager
    {

        readonly IRepository<Employee> EmployeeRepo;
        readonly IRepository<EmployeeSupervisorMap> EmployeeSupervisorMapRepo;
        readonly IRepository<Employment> EmploymentRepo;

        public EmployeeSupervisorMapManager(IRepository<Employee> employeeRepo, IRepository<EmployeeSupervisorMap> supervisorRepo, IRepository<Employment> employmentRepo
            )
        {
            EmployeeRepo = employeeRepo;
            EmployeeSupervisorMapRepo = supervisorRepo;
            EmploymentRepo = employmentRepo;
        }


        public async Task RemoveDiscontinuedEmployee(int EmployeeID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var employeeEnt = EmployeeRepo.Entities.Where(x => x.EmployeeID == EmployeeID).FirstOrDefault();
                employeeEnt.SetDeleted();
                var supMapList = EmployeeSupervisorMapRepo.Entities.Where(x => x.EmployeeSupervisorID == EmployeeID).ToList();
                supMapList.ForEach(x => x.SetDeleted());

                EmployeeRepo.Add(employeeEnt);
                EmployeeSupervisorMapRepo.AddRange(supMapList);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }

    }
}
