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
            
            string sql = $@"SELECT
                                div.division_id AS ""DivisionID"",
                                div.division_code AS ""DivisionCode"",
                                div.division_name AS ""DivisionName"",
                                COALESCE(bm.dhmap_id, 0) AS ""DHMapID"",
                                bm.employee_id AS ""EmployeeID"",
                                emp.full_name AS ""EmployeeName"",
                                bm.budget_amount AS ""BudgetAmount""
                            FROM
                                division div
                            LEFT JOIN
                                division_head_map bm ON div.division_id = bm.division_id
                            LEFT JOIN
                                employee emp ON emp.employee_id = bm.employee_id
                            WHERE
                                div.division_id IN (SELECT division_id FROM department)
                            ORDER BY
                                div.division_id ASC";
            bmList = DivisionRepo.GetDataModelCollection<DivisionDepartmentHeadMapDto>(sql);

            List<DepartmentChildList> departmentChildLists = DepartmentRepo.GetDataModelCollection<DepartmentChildList>(@$"SELECT
                                d.department_id AS ""DepartmentID"",
                                d.department_code AS ""DepartmentCode"",
                                d.department_name AS ""DepartmentName"",
                                div.division_id AS ""DivisionID"",
                                div.division_name AS ""DivisionName"",
                                COALESCE(dhm.department_hmap_id, 0) AS ""DepartmentHMapID"",
                                dhm.employee_id AS ""EmployeeID"",
                                emp.full_name AS ""EmployeeName""
                            FROM
                                department d
                            LEFT JOIN
                                department_head_map dhm ON d.department_id = dhm.department_id
                            LEFT JOIN
                                division div ON d.division_id = div.division_id
                            LEFT JOIN
                                employee emp ON emp.employee_id = dhm.employee_id
                            ORDER BY
                                d.division_id ASC");

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
