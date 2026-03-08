using Core;
using Security.DAL.Entities;
using Security.Manager.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Security.Manager.Interfaces
{
    public interface IDemoManager
    {
        List<Person> GetPersonList(GridParameter param);
        int GetTotalPerson();
    }
}
