using Core;
using Manager.Core.CommonDto;
using SCM.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SCM.Manager.Interfaces
{
    public interface IItemGroupManager
    {
        Task<List<ItemGroupDto>> GetItemGroupList();
        void SaveChanges(ItemGroupDto itemGroupDto);
        Task DeleteItemGroup(int itemGroupId);
        Task<ItemGroupDto> GetItemGroup(int itemGroupId);


    }
}
