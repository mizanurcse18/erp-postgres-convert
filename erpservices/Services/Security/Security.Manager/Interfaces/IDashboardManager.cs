using Core;
using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Security.Manager.Interfaces
{
    public interface IDashboardManager
    {
        IEnumerable<Dictionary<string, object>> GetApprovalDashboardData(int personID);
    }
}
