using Core;
using HRMS.DAL.Entities;
using Security.DAL.Entities;
using Security.Manager.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface ICommonManager
    {
        Task DiscontinuedEmployeeAction(int EmployeeID);
    }
}