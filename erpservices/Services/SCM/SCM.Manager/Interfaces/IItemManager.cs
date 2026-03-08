using Core;
using SCM.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SCM.Manager.Interfaces
{
    public interface IItemManager
    {
        Task<IEnumerable<Dictionary<string, object>>> GetItemListDic();
        GridModel GetListForGrid(GridParameter parameters);
        Task<Dictionary<string, object>> GetItem(int itemId);

        Task<ItemDto> SaveChanges(ItemDto itemDto);
        Task Delete(int ItemID );

    }
}
