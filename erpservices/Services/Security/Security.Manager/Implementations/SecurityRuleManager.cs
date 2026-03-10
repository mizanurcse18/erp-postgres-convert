using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.EntityBase;
using DAL.Core.Extension;
using DAL.Core.Repository;
using Manager.Core;
using Manager.Core.Mapper;
using Security.DAL;
using Security.DAL.Entities;
using Security.Manager.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Security.Manager.Implementations
{
    public class SecurityRuleManager : ManagerBase, ISecurityRuleManager
    {
        readonly IRepository<SecurityRuleMaster> SecurityRuleRepo;
        readonly IRepository<SecurityRulePermissionChild> ChildRepo;
        readonly IRepository<Menu> MenuRepo;
        //readonly IModelAdapter Adapter;
        public SecurityRuleManager(IRepository<SecurityRuleMaster> securityRuleRepo,
            IRepository<SecurityRulePermissionChild> childRepo,
            IRepository<Menu> menuRepo
            //IModelAdapter adapter
            )
        {
            SecurityRuleRepo = securityRuleRepo;
            ChildRepo = childRepo;
            MenuRepo = menuRepo;
            //Adapter = adapter;
        }

        public async Task<List<SecurityRuleMasterDto>> GetSecurityRuleTables()
        {
            var securityRuleTables = await SecurityRuleRepo.GetAllListAsync(securityRule => securityRule.CompanyID == AppContexts.User.CompanyID);
            return securityRuleTables.MapTo<List<SecurityRuleMasterDto>>();
        }

        public async Task<SecurityRuleMasterDto> GetSecurityRuleTable(int primaryID)
        {
            var securityRuleTable = await SecurityRuleRepo.GetAsync(primaryID);
            return securityRuleTable.MapTo<SecurityRuleMasterDto>();
        }

        public async Task<GridModel> GetSecurityRuleChilds(GridParameter parameters)
        {
            var ruleID = parameters.Parameters[parameters.Parameters.FindIndex(x => x.name == "SecurityRuleID")].value.ToInt();

            var securityRuleMenuPermissions = (from menus in MenuRepo.Entities
                                               join permissions in ChildRepo.Entities.Where(x => x.SecurityRuleID == ruleID)
                                               on menus.MenuID equals permissions.MenuID into menuPermissionsTemp
                                               from menuPermissions in menuPermissionsTemp.DefaultIfEmpty()
                                               select new SecurityRulePermissionChildDto
                                               {
                                                   SecurityRulePermissionID = menuPermissions.SecurityRulePermissionID,
                                                   SecurityRuleID = menuPermissions.SecurityRuleID,
                                                   MenuID = menus.MenuID,
                                                   Description = menus.Title,
                                                   CanRead = menuPermissions == null ? false : menuPermissions.CanRead,
                                                   CanCreate = menuPermissions == null ? false : menuPermissions.CanCreate,
                                                   CanUpdate = menuPermissions == null ? false : menuPermissions.CanUpdate,
                                                   CanDelete = menuPermissions == null ? false : menuPermissions.CanDelete
                                               }).ToList();

            List<Dictionary<string, object>> securityRuleMenuPermissionList = new List<Dictionary<string, object>>();

            foreach (SecurityRulePermissionChildDto permissionEntry in securityRuleMenuPermissions)
            {
                Dictionary<string, object> singleMenuPermission = new Dictionary<string, object>();

                foreach (var singleMenuPermissionValueProperty in permissionEntry.GetType().GetProperties())
                {
                    //if (singleMenuPermissionValueProperty.GetValue(permissionEntry) == null)
                    //continue;

                    singleMenuPermission.Add(singleMenuPermissionValueProperty.Name, singleMenuPermissionValueProperty.GetValue(permissionEntry));
                }

                securityRuleMenuPermissionList.Add(singleMenuPermission);
            }

            GridModel model = new GridModel();

            model.Rows = securityRuleMenuPermissionList;
            model.Total = securityRuleMenuPermissionList.Count;

            return await Task.FromResult(model);
        }
        public async Task<List<Dictionary<string, object>>> GetSecurityRuleChilds(int SecurityRuleID)
        {
            //var ruleId = SecurityRuleID.ToInt();
            var securityRuleMenuPermissions = (from menus in MenuRepo.GetAllList()
                                               join permissions in ChildRepo.GetAllList().Where(x => x.SecurityRuleID == SecurityRuleID)
                                               on menus.MenuID equals permissions.MenuID into menuPermissionsTemp
                                               from menuPermissions in menuPermissionsTemp.DefaultIfEmpty()
                                               select new SecurityRulePermissionChildDto
                                               {
                                                   SecurityRulePermissionID = menuPermissions == null ? 0 : menuPermissions.SecurityRulePermissionID,
                                                   SecurityRuleID = menuPermissions == null ? 0 : menuPermissions.SecurityRuleID,
                                                   MenuID = menus.MenuID,
                                                   Description = menus.Title,
                                                   CanRead = menuPermissions == null ? false : menuPermissions.CanRead,
                                                   CanCreate = menuPermissions == null ? false : menuPermissions.CanCreate,
                                                   CanUpdate = menuPermissions == null ? false : menuPermissions.CanUpdate,
                                                   CanDelete = menuPermissions == null ? false : menuPermissions.CanDelete,
                                                   CanReport = menuPermissions == null ? false : menuPermissions.CanReport,
                                                   CreatedBy = menuPermissions == null ? AppContexts.User.UserID : menuPermissions.CreatedBy,
                                                   CreatedDate = menuPermissions == null ? DateTime.Now : menuPermissions.CreatedDate,
                                                   CreatedIP = menuPermissions == null ? null : menuPermissions.CreatedIP,
                                                   RowVersion = menuPermissions == null ? (short)1 : menuPermissions.RowVersion,
                                                   CompanyID = AppContexts.User.CompanyID,
                                                   ParentID = menus.ParentID,
                                                   Icon = menus.Icon
                                               }).ToList();

            List<Dictionary<string, object>> securityRuleMenuPermissionList = new List<Dictionary<string, object>>();

            foreach (SecurityRulePermissionChildDto permissionEntry in securityRuleMenuPermissions)
            {
                Dictionary<string, object> singleMenuPermission = new Dictionary<string, object>();

                foreach (var singleMenuPermissionValueProperty in permissionEntry.GetType().GetProperties())
                {
                    //if (singleMenuPermissionValueProperty.GetValue(permissionEntry) == null)
                    //continue;

                    singleMenuPermission.Add(singleMenuPermissionValueProperty.Name, singleMenuPermissionValueProperty.GetValue(permissionEntry));
                }

                securityRuleMenuPermissionList.Add(singleMenuPermission);
            }

            return await Task.FromResult(securityRuleMenuPermissionList);
        }
        public async Task<SecurityRuleMasterDto> SaveChanges(SecurityRuleMasterDto master, List<SecurityRulePermissionChildDto> childs = null)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                if (childs.IsNull()) childs = new List<SecurityRulePermissionChildDto>();
                //var existSecurityRule = SecurityRuleRepo.Entities.SingleOrDefault(x => x.SecurityRuleID == master.SecurityRuleID).MapTo<SecurityRuleMaster>();
                if (                    
                    master.SecurityRuleID.IsZero() || master.IsAdded)
                {
                    master.SetAdded();
                    SetNewId(master);
                }
                else
                {
                    master.SetModified();
                }

                foreach (var child in childs)
                {
                    if (child.SecurityRulePermissionID == 0)
                    {
                        child.SetAdded();
                        SetNewId(child);
                    }
                    else child.SetModified();

                    child.SecurityRuleID = master.SecurityRuleID;
                }
                var masterEnt = master.MapTo<SecurityRuleMaster>();
                var childsEnt = childs.MapTo<List<SecurityRulePermissionChild>>();

                //Set Audti Fields Data
                SetAuditFields(childsEnt);
                SetAuditFields(masterEnt);

                SecurityRuleRepo.Add(masterEnt);
                ChildRepo.AddRange(childsEnt);
                unitOfWork.CommitChangesWithAudit();

                master = masterEnt.MapTo<SecurityRuleMasterDto>();
                masterEnt.MapToAuditFields(master);
            }
            await Task.CompletedTask;

            return master;
        }

        public async Task Delete(SecurityRuleMasterDto master, List<SecurityRulePermissionChildDto> childs)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                childs.ChangeState(ModelState.Deleted);
                master.SetDeleted();
                var childsEnt = childs.MapTo<List<SecurityRulePermissionChild>>();
                var masterEnt = master.MapTo<SecurityRuleMaster>();
                ChildRepo.AddRange(childsEnt);
                SecurityRuleRepo.Add(masterEnt);
                unitOfWork.CommitChanges();
            }
            await Task.CompletedTask;
        }

        private void SetNewId(SecurityRuleMasterDto securityRuleTable)
        {
            if (!securityRuleTable.IsAdded) return;

            var code = GenerateSystemCode("SecurityRuleMaster", AppContexts.User.CompanyID);
            securityRuleTable.SecurityRuleID = code.MaxNumber;
            //securityRuleTable.PrimaryCode = code.SystemCode;
        }
        private void SetNewId(SecurityRulePermissionChildDto securityRulePermissionTable)
        {
            if (!securityRulePermissionTable.IsAdded) return;
            var code = GenerateSystemCode("SecurityRulePermissionChild", AppContexts.User.CompanyID);
            securityRulePermissionTable.SecurityRulePermissionID = code.MaxNumber;
            //securityRuleTable.PrimaryCode = code.SystemCode;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetSecurityRuleMasterListWithDetails()
        {
            var sql = $@"SELECT
                            m.security_rule_id AS ""SecurityRuleID"",
                            m.company_id AS ""CompanyID"",
                            m.created_by AS ""CreatedBy"",
                            m.created_date AS ""CreatedDate"",
                            m.created_ip AS ""CreatedIP"",
                            m.updated_by AS ""UpdatedBy"",
                            m.updated_date AS ""UpdatedDate"",
                            m.updated_ip AS ""UpdatedIP"",
                            m.row_version AS ""ROWVERSION"",
                            m.security_rule_name AS ""SecurityRuleName"",
                            m.security_rule_description AS ""SecurityRuleDescription"",
                            m.application_id AS ""ApplicationID"",
                            u.user_name AS ""CreatedByUser""
                        FROM security_rule_master m
                        LEFT JOIN users u ON m.created_by = u.user_id
                        WHERE m.company_id = '{AppContexts.User.CompanyID}'";

            var listDict = SecurityRuleRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }

        public async Task RemoveSecurityRule(int securityRuleID)
        {
            if (securityRuleID >= 20 && securityRuleID <= 37)
            {
                throw new InvalidOperationException("Cannot remove security rules with IDs between 20 and 37.");
            }
            using (var unitOfWork = new UnitOfWork())
            {
                var master = SecurityRuleRepo.Entities.Where(x => x.SecurityRuleID == securityRuleID).FirstOrDefault();
                var childs = ChildRepo.Entities.Where(x => x.SecurityRuleID == securityRuleID).ToList();
                childs.ChangeState(ModelState.Deleted);
                master.SetDeleted();
                ChildRepo.AddRange(childs);
                SecurityRuleRepo.Add(master);
                unitOfWork.CommitChangesWithAudit();
            }
            await Task.CompletedTask;
        }
    }
}
