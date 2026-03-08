using Core;
using Manager.Core.CommonDto;
using SCM.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SCM.Manager.Interfaces
{
    public interface ISupplierManager
    {
        Task<IEnumerable<Dictionary<string, object>>> GetSupplierListDic();
        Task<Dictionary<string, object>> GetSupplier(int supplierId);

        Task<SupplierDto> SaveChanges(SupplierDto supplierDto);
        Task Delete(int SupplierID );
        List<Attachments> GetAttachments(int SupplierID);

    }
}
