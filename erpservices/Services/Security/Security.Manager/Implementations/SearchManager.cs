using Core;
using DAL.Core.Repository;
using Security.DAL.Entities;
using Security.Manager.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Security.Manager.Implementations
{
    public class SearchManager : ISearchManager
    {
        //private readonly IModelAdapter Adapter;

        private readonly IRepository<User> UserRepo;
        private readonly IRepository<SecurityGroupMaster> SecurityGroupRepo;
        private readonly IRepository<SecurityGroupUserChild> UserGroupRepo;

        public SearchManager( IRepository<User> userRepo
            , IRepository<SecurityGroupMaster> securityGroupRepo
            , IRepository<SecurityGroupUserChild> userGroupRepo)
        {
            //Adapter = adapter;

            UserRepo = userRepo;
            SecurityGroupRepo = securityGroupRepo;
            UserGroupRepo = userGroupRepo;
        }

        public GridModel GetUsers(GridParameter parameters)
        {
            var sql = $@"SELECT * FROM Users";
            var result = UserRepo.LoadGridModel(parameters, sql);          
            return result;
        }

        public GridModel GetSecurityRules(GridParameter parameters)
        {
            var sql = $@"SELECT * FROM SecurityRuleMaster";
            var result = SecurityGroupRepo.LoadGridModel(parameters, sql);
            return result;
        }

        public GridModel GetSecurityGroups(GridParameter parameters)
        {
            var sql = $@"SELECT * FROM SecurityGroupMaster";
            var result = SecurityGroupRepo.LoadGridModel(parameters, sql);
            return result;
        }        
    }
}
