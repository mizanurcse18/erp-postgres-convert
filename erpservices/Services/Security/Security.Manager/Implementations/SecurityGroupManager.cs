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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Security.Manager
{
    public class SecurityGroupManager : ManagerBase, ISecurityGroupManager
    {
        private readonly IRepository<SecurityGroupMaster> SecurityGroupRepo;
        private readonly IRepository<SecurityRuleMaster> SecurityRuleRepo;
        private readonly IRepository<SecurityRulePermissionChild> RulePermissionChildRepo;
        private readonly IRepository<SecurityGroupRuleChild> SecurityGroupRuleChildRepo;
        private readonly IRepository<SecurityGroupUserChild> UserChildRepo;
        private readonly IRepository<Menu> MenuRepo;
        //readonly IModelAdapter Adapter;
        public SecurityGroupManager(IRepository<SecurityGroupMaster> securityGroupRepo,
            IRepository<SecurityRuleMaster> securityRuleRepo,
            IRepository<SecurityRulePermissionChild> rulePermissionChildRepo,
            IRepository<SecurityGroupRuleChild> ruleChildRepo,
            IRepository<SecurityGroupUserChild> userChildRepo,
            IRepository<Menu> menuRepo//,
                                      //IModelAdapter adapter
            )
        {
            SecurityGroupRepo = securityGroupRepo;
            SecurityRuleRepo = securityRuleRepo;
            RulePermissionChildRepo = rulePermissionChildRepo;
            SecurityGroupRuleChildRepo = ruleChildRepo;
            UserChildRepo = userChildRepo;
            MenuRepo = menuRepo;
            //Adapter = adapter;
        }

        public async Task<List<SecurityGroupMasterDto>> GetSecurityGroupTables()
        {
            var securityGroupTables = await SecurityGroupRepo.GetAllListAsync(securityGroup => securityGroup.CompanyID == AppContexts.User.CompanyID.ToString());
            return securityGroupTables.MapTo<List<SecurityGroupMasterDto>>();
        }

        public async Task<SecurityGroupMasterDto> GetSecurityGroup(int primaryID)
        {
            var securityGroupTable = await SecurityGroupRepo.GetAsync(primaryID);
            return securityGroupTable.MapTo<SecurityGroupMasterDto>();
        }

        public async Task<List<SecurityGroupRuleChildDto>> GetSecurityRulesForGroup(int groupID)
        {
            List<SecurityGroupRuleChildDto> securityRulesForGroup = new List<SecurityGroupRuleChildDto>();

            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                securityRulesForGroup = (
                    from rules in SecurityRuleRepo.Entities
                    join groupRules in SecurityGroupRuleChildRepo.Entities.Where(x => x.SecurityGroupID == groupID)
                    on rules.SecurityRuleID equals groupRules.SecurityRuleID
                    select new SecurityGroupRuleChildDto
                    {
                        SecurityGroupRuleChildID = groupRules.SecurityGroupRuleChildID,
                        SecurityRuleID = rules.SecurityRuleID,
                        SecurityGroupID = groupRules.SecurityGroupID,
                        SecurityRuleName = rules.SecurityRuleName
                    }).ToList();
            }

            return await Task.FromResult(securityRulesForGroup);
        }

        public GridModel GetSelectedSecurityRules(int groupID)
        {
            var securityRules = SecurityGroupRuleChildRepo.GetAllList(c => c.SecurityGroupID == groupID);

            var filter = string.Join(',', securityRules.Select(groupSecurityRule => groupSecurityRule.SecurityRuleID));

            var groupSecurityRules = GetSecurityGroupRules(new GridParameter
            {
                Offset = 0,
                Limit = 10,
                Parameters = new List<FinderParameter>
                {
                    new FinderParameter { name = "SecurityRuleID", value = filter.IsNullOrEmpty() ? "-1" : filter, operat = "in"}
                },
                ServerPagination = false,
                SortName = "SecurityRuleID",
                SortOrder = "ASC"
            });

            return groupSecurityRules;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetSecurityRulesByGroupID(int groupID)
        {
            string sql = $@"SELECT 
	                            SecurityGroupRuleChildID
	                            --,SGRC.CompanyID
	                            --,SGRC.CreatedBy
	                            --,SGRC.CreatedDate
	                            --,SGRC.CreatedIP
	                            --,SGRC.UpdatedBy
	                            --,SGRC.UpdatedDate
	                            --,SGRC.UpdatedIP
	                            --,SGRC.ROWVERSION
	                            ,SGRC.SecurityGroupID
	                            ,SGRC.SecurityRuleID
	                            ,SRM.SecurityRuleName
                            FROM SecurityGroupRuleChild SGRC
                            INNER JOIN SecurityRuleMaster SRM ON SRM.SecurityRuleID = SGRC.SecurityRuleID
                            WHERE SecurityGroupID = {groupID}";
            var listDict = SecurityGroupRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }

        public GridModel GetSecurityGroupRules(GridParameter parameters)
        {
            const string sql = "SELECT * FROM SecurityRuleMaster";
            var result = SecurityRuleRepo.LoadGridModel(parameters, sql);
            return result;
        }

        public GridModel GetMenuPermissions(GridParameter parameters)
        {
            string sql = $@"SELECT MenuID, Description, 0 AS CanCreate, 0 AS CanRead, 0 AS CanUpdate, 0 AS CanDelete FROM Menu";
            var result = SecurityRuleRepo.LoadGridModel(parameters, sql); ;
            return result;
        }

        public async Task<GridModel> GetSecurityGroupSelectedRuleMenuPermissions(GridParameter parameters)
        {
            GridModel model = new GridModel();

            using (var unitOfWork = new UnitOfWork())
            {
                var ruleIDs = parameters.Parameters[parameters.Parameters.FindIndex(x => x.name == "SecurityRuleID")].value.Split(",").ToList();

                var securityGroupMenuPermissions = (
                    from menu in MenuRepo.Entities
                    join rulePermissions in RulePermissionChildRepo.Entities
                    on menu.MenuID equals rulePermissions.MenuID into menuPermissionsWithBlank
                    from menuPermissions in menuPermissionsWithBlank.DefaultIfEmpty()
                    join rules in SecurityRuleRepo.Entities.Where(x => ruleIDs.Contains(x.SecurityRuleID.ToString()))
                    on menuPermissions.SecurityRuleID equals rules.SecurityRuleID into ruleMenuPermissionsWithBlank
                    from ruleMenuPermissions in ruleMenuPermissionsWithBlank.DefaultIfEmpty()
                    join groupRules in SecurityGroupRuleChildRepo.Entities
                    on ruleMenuPermissions.SecurityRuleID equals groupRules.SecurityRuleID into groupRuleMenuPermissionsWithBlank
                    from groupRuleMenuPermissions in groupRuleMenuPermissionsWithBlank.DefaultIfEmpty()
                    select new SecurityRulePermissionChildDto
                    {
                        MenuID = menu.MenuID,
                        Description = menu.Title,
                        CanRead = (menuPermissions == null || ruleMenuPermissions == null) ? false : (menuPermissions.CanRead ?? false),
                        CanCreate = (menuPermissions == null || ruleMenuPermissions == null) ? false : (menuPermissions.CanCreate ?? false),
                        CanUpdate = (menuPermissions == null || ruleMenuPermissions == null) ? false : (menuPermissions.CanUpdate ?? false),
                        CanDelete = (menuPermissions == null || ruleMenuPermissions == null) ? false : (menuPermissions.CanDelete ?? false)
                    }).ToList()
                    .GroupBy(x => new { x.MenuID, x.Description })
                    .Select(
                    x => new SecurityRulePermissionChildDto
                    {
                        MenuID = x.Key.MenuID,
                        Description = x.Key.Description,
                        CanRead = x.Select(y => y.CanRead).Aggregate((p, q) => (p ?? false) || (q ?? false)),
                        CanCreate = x.Select(y => y.CanCreate).Aggregate((p, q) => (p ?? false) || (q ?? false)),
                        CanUpdate = x.Select(y => y.CanUpdate).Aggregate((p, q) => (p ?? false) || (q ?? false)),
                        CanDelete = x.Select(y => y.CanDelete).Aggregate((p, q) => (p ?? false) || (q ?? false))
                    }).ToList();

                List<Dictionary<string, object>> securityGroupMenuPermissionList = new List<Dictionary<string, object>>();

                foreach (SecurityRulePermissionChildDto permissionEntry in securityGroupMenuPermissions)
                {
                    Dictionary<string, object> singleMenuPermission = new Dictionary<string, object>();

                    foreach (var singleMenuPermissionValueProperty in permissionEntry.GetType().GetProperties())
                    {
                        singleMenuPermission.Add(singleMenuPermissionValueProperty.Name, singleMenuPermissionValueProperty.GetValue(permissionEntry));
                    }

                    securityGroupMenuPermissionList.Add(singleMenuPermission);
                }

                model.Rows = securityGroupMenuPermissionList;
                model.Total = securityGroupMenuPermissionList.Count;
            }

            return await Task.FromResult(model);
        }

        public async Task SaveChanges(SecurityGroupMasterDto securityGroup, List<SecurityRuleMasterDto> securityGroupRules)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                if (securityGroupRules.IsNull()) securityGroupRules = new List<SecurityRuleMasterDto>();

                var securityGroupRuleChilds = new List<SecurityGroupRuleChildDto>();

                if (securityGroup.SecurityGroupID.IsZero())
                {
                    securityGroup.SetAdded();
                    SetNewId(securityGroup);
                    securityGroupRules.ChangeState(ModelState.Added);
                }
                else
                {
                    securityGroup.SetModified();
                }

                foreach (var securityGroupRule in securityGroupRules)
                {
                    var ruleChild = new SecurityGroupRuleChildDto
                    {
                        SecurityGroupID = securityGroup.SecurityGroupID,
                        SecurityRuleID = securityGroupRule.SecurityRuleID
                    };
                    var ruleChildEnt = SecurityGroupRuleChildRepo.Entities.Where(
                            ruleChildEnt => ruleChildEnt.SecurityGroupID == ruleChild.SecurityGroupID &&
                            ruleChildEnt.SecurityRuleID == ruleChild.SecurityRuleID
                        ).FirstOrDefault();

                    if (ruleChildEnt.IsNull())
                    {
                        ruleChild.SetAdded();
                        SetNewId(ruleChild);
                    }
                    else if (securityGroupRule.IsModified || ruleChildEnt.IsNotNull())
                    {
                        ruleChild.SetModified();
                        ruleChild.RowVersion = ruleChildEnt.RowVersion;
                        ruleChild.SecurityGroupRuleChildID = ruleChildEnt.SecurityGroupRuleChildID;
                        ruleChild.CreatedBy = ruleChildEnt.CreatedBy;
                        ruleChild.CreatedIP = ruleChildEnt.CreatedIP;
                        ruleChild.CreatedDate = ruleChildEnt.CreatedDate;
                    }
                    securityGroupRuleChilds.Add(ruleChild);
                }

                var securityGroupEnt = securityGroup.MapTo<SecurityGroupMaster>();
                var securityGroupRulesEnt = securityGroupRuleChilds.MapTo<List<SecurityGroupRuleChild>>();

                #region Delete Rules 
                if (!securityGroup.IsAdded)
                {
                    var list = SecurityGroupRuleChildRepo.Entities.Where(
                            ruleChildEnt => ruleChildEnt.SecurityGroupID == securityGroup.SecurityGroupID
                        );
                    foreach(var obj in list)
                    {
                        var exist = securityGroupRules.Find(x => x.SecurityRuleID == obj.SecurityRuleID);
                        if (exist.IsNull())
                        {
                            obj.SetDeleted();
                            securityGroupRulesEnt.Add(obj);
                        }
                    }
                }
                
                #endregion Delete Rules 

                
                SetAuditFields(securityGroupEnt);
                SetAuditFields(securityGroupRulesEnt);

                SecurityGroupRepo.Add(securityGroupEnt);
                SecurityGroupRuleChildRepo.AddRange(securityGroupRulesEnt);
                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }

        public async Task RemoveSecurityGroup(int SecurityGroupID)
        {
            if (SecurityGroupID >= 20 && SecurityGroupID <= 37)
            {
                throw new InvalidOperationException("Cannot remove security rules with IDs between 20 and 37.");
            }

            using (var unitOfWork = new UnitOfWork())
            {

                var masterEnt = SecurityGroupRepo.Entities.Where(x => x.SecurityGroupID == SecurityGroupID).FirstOrDefault();
                masterEnt.SetDeleted();
                var ruleChildsEnt = SecurityGroupRuleChildRepo.Entities.Where(x => x.SecurityGroupID == SecurityGroupID).ToList();
                ruleChildsEnt.ChangeState(ModelState.Deleted);

                var userChildsEnt = UserChildRepo.Entities.Where(x => x.SecurityGroupID == SecurityGroupID).ToList();
                userChildsEnt.ChangeState(ModelState.Deleted);

                UserChildRepo.AddRange(userChildsEnt);
                SecurityGroupRuleChildRepo.AddRange(ruleChildsEnt);
                SecurityGroupRepo.Add(masterEnt);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }

        private void SetNewId(SecurityGroupMasterDto securityGroupTable)
        {
            if (!securityGroupTable.IsAdded) return;
            var code = GenerateSystemCode("SecurityGroupMaster", AppContexts.User.CompanyID);
            securityGroupTable.SecurityGroupID = code.MaxNumber;
        }

        private void SetNewId(SecurityGroupRuleChildDto securityGroupRuleChildTable)
        {
            if (!securityGroupRuleChildTable.IsAdded) return;
            var code = GenerateSystemCode("SecurityGroupRuleChild", AppContexts.User.CompanyID);
            securityGroupRuleChildTable.SecurityGroupRuleChildID = code.MaxNumber;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetSecurityGroupMasterListWithDetails()
        {
            var sql = $@"SELECT 
	                        SecurityGroupID
	                        ,m.CompanyID
	                        ,m.CreatedBy
	                        ,m.CreatedDate
	                        --,m.CreatedIP
	                        --,m.UpdatedBy
	                        --,m.UpdatedDate
	                        --,m.UpdatedIP
	                        --,m.ROWVERSION
	                        ,m.SecurityGroupName
	                        ,m.SecGroupDescription
	                        ,u.UserName CreatedByUser
                        FROM 
	                        Security.dbo.SecurityGroupMaster m
	                        LEFT JOIN Users u on m.CreatedBy = u.UserID
                            WHERE m.CompanyID = '{AppContexts.User.CompanyID}'";

            var listDict = SecurityGroupRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }
    }
}
