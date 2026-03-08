using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Security.Manager.Interfaces
{
    public interface IMenuManager
    {
        Task<List<dynamic>> GetMenus();
    }
}
