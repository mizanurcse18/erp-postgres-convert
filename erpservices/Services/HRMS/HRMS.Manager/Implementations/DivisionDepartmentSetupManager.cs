using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Manager.Core;
using Manager.Core.CommonDto;
using Manager.Core.Mapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HRMS.Manager.Implementations
{
    public class DivisionDepartmentSetupManager : ManagerBase, IDivisionDepartmentSetupManager
    {

        private readonly IRepository<DivisionHeadMap> DivisionHeadMapRepo;
        private readonly IRepository<DepartmentHeadMap> DepartmentHeadMapRepo;
        private readonly IRepository<Division> DivisionRepo;
        private readonly IRepository<Department> DepartmentRepo;
        public DivisionDepartmentSetupManager(IRepository<DivisionHeadMap> divisionHeadMapRepo, IRepository<DepartmentHeadMap> departmentHeadMapRepo, IRepository<Division> divisionRepo, IRepository<Department> departmentRepo)
        {
            DivisionHeadMapRepo = divisionHeadMapRepo;
            DivisionRepo = divisionRepo;
            DepartmentRepo = departmentRepo;
            DepartmentHeadMapRepo = departmentHeadMapRepo;
        }

        public async Task<List<DivisionDepartmentHeadMapDto>> GetDivisionDepartment()
        {
            List<DivisionDepartmentHeadMapDto> bmList = new List<DivisionDepartmentHeadMapDto>();
            
            string sql = $@"select Div.DivisionID, Div.DivisionCode, Div.DivisionName, ISNULL(BM.DHMapID,0) AS DHMapID, BM.EmployeeID, emp.FullName as EmployeeName, BudgetAmount
                            from HRMS..Division Div 
                            LEFT JOIN DivisionHeadMap BM ON Div.DivisionID = BM.DivisionID
							LEFT JOIN Employee emp ON emp.EmployeeID = BM.EmployeeID
                            WHERE div.DivisionID in(select DivisionID from Department)
                            ORDER BY Div.DivisionID ASC";
            bmList = DivisionRepo.GetDataModelCollection<DivisionDepartmentHeadMapDto>(sql);

            List<DepartmentChildList> departmentChildLists = DepartmentRepo.GetDataModelCollection<DepartmentChildList>(@$"select D.DepartmentID, D.DepartmentCode, D.DepartmentName,Div.DivisionID, Div.DivisionName,ISNULL(DHM.DepartmentHMapID,0) AS DepartmentHMapID , DHM.EmployeeID, emp.FullName as EmployeeName
                            from Department D 
                            LEFT JOIN DepartmentHeadMap DHM ON D.DepartmentID = DHM.DepartmentID
							LEFT JOIN Division Div ON D.DivisionID = Div.DivisionID
							LEFT JOIN Employee emp ON emp.EmployeeID = DHM.EmployeeID
                            ORDER BY D.DivisionID ASC");

            var divList = bmList.Select(x => new DivisionDepartmentHeadMapDto
            {
                DHMapID = x.DHMapID,
                DivisionID = x.DivisionID,
                DivisionCode = x.DivisionCode,
                DivisionName = x.DivisionName,
                EmployeeID = x.EmployeeID,
                EmployeeName = x.EmployeeName,
                BudgetAmount = x.BudgetAmount,

                ChildList = departmentChildLists.Where(y => y.DivisionID == x.DivisionID).ToList<DepartmentChildList>()

            }).ToList();


            return await Task.FromResult(divList);


            //var divList = DivisionRepo.GetAllList().MapTo<List<DivisionDto>>().Select(x => new DivisionDepartmentHeadMapDto
            //{
            //    DivisionID = x.DivisionID,
            //    DivisionCode = x.DivisionCode,
            //    DivisionName = x.DivisionName,
            //    //ChildList = DepartmentRepo.GetAllList(y => y.DivisionID == x.DivisionID.ToString()).MapTo<List<DepartmentDto>>()
            //    ChildList = DepartmentRepo.GetAllList(y => y.DivisionID == x.DivisionID.ToString()).MapTo<List<DepartmentDto>>().Select(z => new DepartmentChildList { DepartmentCode = z.DepartmentCode, DepartmentName = z.DepartmentName, DepartmentID = z.DepartmentID }).ToList()
            //}).ToList();


            //return await Task.FromResult(divList);
        }

        public void SaveDivDeptSetup(List<DivisionDepartmentHeadMapDto> list)
        {
            List<DivisionHeadMap> divHeadMapList = new List<DivisionHeadMap>();
            List<DepartmentHeadMap> depHeadMapList = new List<DepartmentHeadMap>();
            using (var unitOfWork = new UnitOfWork())
            {

                foreach (var item in list)
                {
                    DivisionHeadMap divHead = new DivisionHeadMap();
                    var existDivision = DivisionHeadMapRepo.Entities.FirstOrDefault(x => x.DHMapID == item.DHMapID).MapTo<DivisionHeadMap>();
                    if (item.DHMapID.IsZero() || item.IsAdded)
                    {
                        divHead.SetAdded();
                    }
                    else
                    {
                        divHead.CreatedBy = existDivision.CreatedBy;
                        divHead.CreatedDate = existDivision.CreatedDate;
                        divHead.CreatedIP = existDivision.CreatedIP;
                        divHead.RowVersion = existDivision.RowVersion;
                        divHead.SetModified();
                    }
                    divHead.DivisionID = item.DivisionID;
                    divHead.EmployeeID = item.EmployeeID;
                    divHead.DHMapID = item.DHMapID;
                    divHead.BudgetAmount = item.BudgetAmount;
                    divHeadMapList.Add(divHead);
                    SetAuditFields(item);

                    #region Department

                    foreach (var child in item.ChildList)
                    {
                        DepartmentHeadMap depHead = new DepartmentHeadMap();
                        var existsDepartment = DepartmentHeadMapRepo.Entities.FirstOrDefault(x => x.DepartmentHMapID == child.DepartmentHMapID).MapTo<DepartmentHeadMap>();
                        if (child.DepartmentHMapID == 0)
                        {
                            depHead.SetAdded();
                        }
                        else
                        {
                            depHead.CreatedBy = existsDepartment.CreatedBy;
                            depHead.CreatedDate = existsDepartment.CreatedDate;
                            depHead.CreatedIP = existsDepartment.CreatedIP;
                            depHead.RowVersion = existsDepartment.RowVersion;
                            depHead.SetModified();
                        }

                        depHead.DepartmentHMapID = child.DepartmentHMapID;
                        depHead.EmployeeID = child.EmployeeID;
                        depHead.DepartmentID = child.DepartmentID;
                        SetAuditFields(depHead);
                        depHeadMapList.Add(depHead);
                    }

                    #endregion Department
              
                }
                DepartmentHeadMapRepo.AddRange(depHeadMapList);
                DivisionHeadMapRepo.AddRange(divHeadMapList);
                unitOfWork.CommitChangesWithAudit();
            }


        }



    }
}
