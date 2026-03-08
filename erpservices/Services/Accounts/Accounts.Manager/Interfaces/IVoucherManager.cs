using Accounts.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.Manager.Interfaces
{
    public interface IVoucherManager
    {
        Task<(bool, string)> SaveChanges(VoucherDto voucher);
    }
}
