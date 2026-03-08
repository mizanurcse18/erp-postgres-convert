using Core;
using Manager.Core.CommonDto;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Security.Manager.Interfaces
{
    public interface ICommonInterfaceManager
    {
        Task<Dictionary<string, object>> LoadCommonInterfaceUIData(int MenuID);
        Task<List<Dictionary<string, object>>> LoadCommonInterfaceUIFields(int MenuID);
        GridModel GetListForGrid(GridParameter parameters);
        Task<(bool, string)> SaveChanges(dynamic jsonData);
        Task<Dictionary<string, object>> GetCommonInterfaceData(string primaryKeyID, int menuID);
    }
}
