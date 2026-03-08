using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using Manager.Core;
using Manager.Core.Mapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Security.DAL.Entities;
using Security.Manager.Dto;
using Security.Manager.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Util;

namespace Security.Manager.Implementations
{
    class DemoManager : ManagerBase, IDemoManager
    {

        readonly IRepository<Person> PersonRepo;
        public DemoManager(IRepository<Person> personRepo)
        {
            PersonRepo = personRepo;
        }


        public List<Person> GetPersonList(GridParameter param)
        {
            List<Person> psns = new List<Person>();
            //if(param.Sort != "" && param.Sort == "ASC")
            //    psns = PersonRepo.Entities.OrderBy(a => a.PersonID).Skip(param.Offset * param.Limit).Take(param.Limit).ToList();
            //else
            //    psns = PersonRepo.Entities.OrderByDescending(a => a.FirstName).Skip(param.Offset * param.Limit).Take(param.Limit).ToList();
            string orderBy = param.SortName != "" ? param.SortName : "PersonID";
            int offset = param.Offset * param.Limit;
            string query = $@"SELECT * FROM Person ORDER BY {orderBy} " + param.Sort  
                + " OFFSET "+ offset + " ROWS FETCH NEXT "+ param.Limit +" ROWS ONLY";

            psns = PersonRepo.GetDataModelCollection<Person>(query);
            
            return psns;
        }

        public int GetTotalPerson()
        {
            int totalPerson = PersonRepo.Entities.Count();
            return totalPerson;
        }

    }
}
