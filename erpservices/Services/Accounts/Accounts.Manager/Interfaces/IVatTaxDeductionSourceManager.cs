using Accounts.DAL.Entities;
using Accounts.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.Manager.Interfaces
{
    public interface IVatTaxDeductionSourceManager
    {
        void SaveChanges(List<VatTaxDeductionSourceDto> vatTaxDeductionSource);
        Task<List<VatTaxDeductionSourceListDto>> GetVatTaxDeductionSourceList();
        Task<List<VatTaxDeductionSourceListDto>> GetVatTaxDeductionSource(int financialYearID);
        void RemoveVatTaxDeductionSource(int financialYearID);
    }
}
