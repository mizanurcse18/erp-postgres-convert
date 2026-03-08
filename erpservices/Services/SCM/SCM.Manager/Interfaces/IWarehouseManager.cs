using Core;
using SCM.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SCM.Manager.Interfaces
{
    public interface IWarehouseManager
    {
        Task<IEnumerable<Dictionary<string, object>>> GetWarehouseListDic();
        Task<Dictionary<string, object>> GetWarehouse(int itemId);

        Task<WarehouseDto> SaveChanges(WarehouseDto itemDto);
        Task Delete(int WarehouseID );
        List<Attachments> GetAttachments(int wid);

    }
}
