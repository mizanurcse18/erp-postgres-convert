using Core;
using SCM.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SCM.Manager.Interfaces
{
    public interface IItemSubGroupManager
    {
        Task<IEnumerable<Dictionary<string, object>>> GetItemSubGroupListDic();
        Task<Dictionary<string, object>> GetItemSubGroup(int itemSubGroupId);

        Task<ItemSubGroupDto> SaveChanges(ItemSubGroupDto itemSubGroupDto);
        Task Delete(int ItemSubGroupID );

    }
}
