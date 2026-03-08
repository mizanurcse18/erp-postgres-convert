using Core.AppContexts;
using DAL.Core;
using DAL.Core.Repository;
using Manager.Core;
using Security.DAL.Entities;
using Security.Manager.Dto;
using Security.Manager.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Security;
using System.Threading.Tasks;

namespace Security.Manager.Implementations
{
    public class EventManager : ManagerBase, IEventManager
    {
        private readonly IRepository<Menu> repository;
        public EventManager(IRepository<Menu> _repository)
        {
            repository = _repository;
        }

        public async Task<List<Dictionary<string, object>>> GetEventList()
        {
            string sql = @$"SELECT * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewHolidayEvents WHERE CompanyID = '{AppContexts.User.CompanyID}'";
            return await Task.FromResult(repository.GetDataDictCollection(sql).ToList());
        }
    }
}
