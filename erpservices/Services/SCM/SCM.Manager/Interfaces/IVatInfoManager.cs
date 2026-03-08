using Core;
using SCM.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SCM.Manager.Interfaces
{
    public interface IVatInfoManager
    {
        Task<IEnumerable<Dictionary<string, object>>> GetVatInfoListDic();
        Task<Dictionary<string, object>> GetVatInfo(int VatInfoId);

        Task<VatInfoDto> SaveChanges(VatInfoDto VatInfoDto);
        Task Delete(int VatInfoID );

    }
}
