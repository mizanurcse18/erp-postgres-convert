using Accounts.DAL.Entities;
using Accounts.Manager.Dto;
using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.EntityBase;
using DAL.Core.Extension;
using DAL.Core.Repository;
using Manager.Core;
using Manager.Core.Mapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Core.Util;

namespace Accounts.Manager
{
    public class BudgetManager : ManagerBase, IBudgetManager
    {
        private readonly IRepository<BudgetChildWithApprovalPanelMap> BudgetChildWithApprovalPanelMapRepo;
        private readonly IRepository<BudgetChild> BudgetChildRepo;
        private readonly IRepository<BudgetMaster> BudgetMasterRepo;

        public BudgetManager(IRepository<BudgetChildWithApprovalPanelMap> budgetChildWithApprovalPanelMapRepo, IRepository<BudgetChild> budgetChildRepo, IRepository<BudgetMaster> budgetMasterRepo)
        {
            BudgetChildWithApprovalPanelMapRepo = budgetChildWithApprovalPanelMapRepo;
            BudgetChildRepo = budgetChildRepo;
            BudgetMasterRepo = budgetMasterRepo;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetIOUExpenseBudget(int DepartmentID)
        {
            string sql = $@"select D.DepartmentID, D.DepartmentCode, D.DepartmentName, 0 AS MinAmount, 0 AS TotalMaxAmount, 0 AS BudgetMasterID from HRMS..Department D";
            var departments = BudgetMasterRepo.GetDataDictCollection(sql);

            return await Task.FromResult(departments);
        }
        //public async Task<IEnumerable<Dictionary<string, object>>> GetAllDeptBudgetList()
        //{
        //    string sql = $@"select D.DepartmentID, D.DepartmentCode, D.DepartmentName, 0 AS MinAmount, 0 AS TotalMaxAmount, 0 AS BudgetMasterID from HRMS..Department D";
        //    var departments = BudgetMasterRepo.GetDataDictCollection(sql);

        //    return await Task.FromResult(departments);
        //}
        public async Task<List<BudgetMasterDto>> GetAllDeptBudgetList()
        {
            List<BudgetMasterDto> bmList = new List<BudgetMasterDto>();
            string sql = $@"select D.DepartmentID, D.DepartmentCode, D.DepartmentName, Div.DivisionName, ISNULL(BM.MinAmount, 0) AS MinAmount, ISNULL(BM.TotalMaxAmount, 0) AS TotalMaxAmount, ISNULL(BM.AttachmentRequiredAmount, 0) AS AttachmentRequiredAmount,  ISNULL(BM.BudgetMasterID,0) AS BudgetMasterID 
                            from HRMS..Department D 
                            LEFT JOIN BudgetMaster BM ON D.DepartmentID = BM.DepartmentID
							LEFT JOIN HRMS..Division Div ON D.DivisionID = Div.DivisionID
                            ORDER BY D.DivisionID ASC";
            bmList = BudgetMasterRepo.GetDataModelCollection<BudgetMasterDto>(sql);

            //foreach (var de in bmList)
            //{
            //    var findChild = BudgetChildRepo.Entities.Where(x => x.BudgetMasterID == de.BudgetMasterID).MapTo<List<BudgetChildDto>>();
            //    if (findChild.Count > 0)
            //    {
            //        //de.ChildList = findChild;
            //        foreach(var ch in findChild)
            //        {
            //            var findMap = BudgetChildWithApprovalPanelMapRepo.Entities.Where(x => x.BudgetChildID == ch.BudgetChildID).MapTo<List<BudgetChildWithApprovalPanelMap>>();
            //            ch.BudgetChildPanelMap = findMap;
            //        }
            //        de.ChildList.AddRange(findChild);
            //    }
            //    else
            //    {
            //        BudgetChildDto bc = new BudgetChildDto();
            //        bc.BudgetChildID = 0;
            //        bc.BudgetMasterID = de.BudgetMasterID;
            //        bc.MinAmount = 0;
            //        bc.MaxAmount = 0;
            //        bc.BudgetChildPanelMap = new List<BudgetChildWithApprovalPanelMap>();
            //        de.ChildList.Add(bc);
            //    }
            //}

            var data = bmList.Select(x => new BudgetMasterDto
            {
                BudgetMasterID = x.BudgetMasterID,
                DepartmentID = x.DepartmentID,
                DepartmentName = x.DepartmentName,
                DivisionName = x.DivisionName,
                MinAmount = x.MinAmount,
                TotalMaxAmount = x.TotalMaxAmount,
                AttachmentRequiredAmount = x.AttachmentRequiredAmount,

                ChildList = BudgetChildRepo.Entities.Where(y => y.BudgetMasterID == x.BudgetMasterID).Select(a => new BudgetChildDto
                {
                    BudgetChildID = a.BudgetChildID,
                    BudgetMasterID = a.BudgetMasterID,
                    MinAmount = a.MinAmount,
                    MaxAmount = a.MaxAmount,

                    BudgetChildPanelMap = BudgetChildWithApprovalPanelMapRepo.Entities.Where(b => b.BudgetChildID == a.BudgetChildID).MapTo<List<BudgetChildWithApprovalPanelMap>>(),
                    BudgetChildPanelCombo = BudgetChildWithApprovalPanelMapRepo.GetDataModelCollection<ComboModel>(@$"SELECT ap.Name label, bcm.APPanelID value
                                                                   FROM BudgetChildWithApprovalPanelMap bcm
                                                                   LEFT JOIN Approval..ApprovalPanel ap on bcm.APPanelID = ap.APPanelID
                                                                   WHERE bcm.BudgetChildID = '{a.BudgetChildID}'")
                }).ToList(),

            });

            return await Task.FromResult(data.ToList());
        }
        private void SetNewMasterId(BudgetMasterDto dto)
        {
            if (!dto.IsAdded) return;
            var code = GenerateSystemCode("BudgetMaster", AppContexts.User.CompanyID);
            dto.BudgetMasterID = code.MaxNumber;
        }
        private void SetNewMapId(BudgetChildWithApprovalPanelMap dto)
        {
            if (!dto.IsAdded) return;
            var code = GenerateSystemCode("BudgetChildWithApprovalPanelMap", AppContexts.User.CompanyID);
            dto.BCWAPPMapID = code.MaxNumber;
        }
        private void SetNewChildId(BudgetChildDto dto)
        {
            if (!dto.IsAdded) return;
            var code = GenerateSystemCode("BudgetChild", AppContexts.User.CompanyID);
            dto.BudgetChildID = code.MaxNumber;
        }

        public void SaveBudget(List<BudgetMasterDto> list)
        {

            using (var unitOfWork = new UnitOfWork())
            {
                List<BudgetChildDto> newChildList = new List<BudgetChildDto>();
                List<BudgetChildDto> newChildListForDelete = new List<BudgetChildDto>();
                List<BudgetChildWithApprovalPanelMap> panelListToAdd = new List<BudgetChildWithApprovalPanelMap>();
                List<BudgetChildWithApprovalPanelMap> panelListToDelete = new List<BudgetChildWithApprovalPanelMap>();
                foreach (var item in list)
                {
                    var existMaster = BudgetMasterRepo.Entities.FirstOrDefault(x => x.BudgetMasterID == item.BudgetMasterID).MapTo<BudgetMasterDto>();
                    if (
                    item.BudgetMasterID.IsZero() || item.IsAdded)
                    {
                        item.SetAdded();
                        SetNewMasterId(item);
                    }
                    else
                    {
                        item.CreatedBy = existMaster.CreatedBy;
                        item.CreatedDate = existMaster.CreatedDate;
                        item.CreatedIP = existMaster.CreatedIP;
                        item.RowVersion = existMaster.RowVersion;
                        item.SetModified();
                    }

                    foreach (var child in item.ChildList)
                    {
                        var existsPeriod = BudgetChildRepo.Entities.FirstOrDefault(x => x.BudgetChildID == child.BudgetChildID && x.BudgetMasterID == item.BudgetMasterID).MapTo<BudgetChildDto>();
                        if (existsPeriod.IsNull())
                        {
                            child.SetAdded();
                            SetNewChildId(child);
                        } 
                        else if(child.isDeletedFromUI)
                        {
                            newChildListForDelete.Add(child);
                        }
                        else
                        {

                            child.CreatedBy = existsPeriod.CreatedBy;
                            child.CreatedDate = existsPeriod.CreatedDate;
                            child.CreatedIP = existsPeriod.CreatedIP;
                            child.RowVersion = existsPeriod.RowVersion;
                            child.SetModified();
                        }

                        child.BudgetMasterID = item.BudgetMasterID;
                        newChildList.Add(child);


                        foreach (var map in child.BudgetChildPanelCombo)
                        {
                            var obj = new BudgetChildWithApprovalPanelMap
                            {
                                BCWAPPMapID = 0,
                                BudgetChildID = child.BudgetChildID,
                                APPanelID = map.value,
                                ObjectState = ModelState.Added
                            };
                            obj.SetAdded();
                            SetNewMapId(obj);
                            panelListToAdd.Add(obj);

                        }
                        panelListToDelete.AddRange(BudgetChildWithApprovalPanelMapRepo.Entities.Where(x=>x.BudgetChildID==child.BudgetChildID).ToList());
                        
                    }

                }
                panelListToDelete.ForEach(x => x.SetDeleted());
                newChildListForDelete.ForEach(x => x.SetDeleted());
                var masterEnt = list.MapTo<List<BudgetMaster>>();

                //Set Audti Fields Data
                SetAuditFields(newChildList);
                SetAuditFields(masterEnt);

                BudgetMasterRepo.AddRange(masterEnt);
                BudgetChildRepo.AddRange(newChildListForDelete.MapTo<List<BudgetChild>>());
                BudgetChildRepo.AddRange(newChildList.MapTo<List<BudgetChild>>());
                BudgetChildWithApprovalPanelMapRepo.AddRange(panelListToDelete);
                BudgetChildWithApprovalPanelMapRepo.AddRange(panelListToAdd);

                unitOfWork.CommitChangesWithAudit();
            }
        }
    }
}
