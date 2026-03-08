using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using Manager.Core;
using Manager.Core.CommonDto;
using Manager.Core.Mapper;
using Security.DAL.Entities;
using Security.Manager.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Security.Manager.Implementations
{
    public class DashboardManager : ManagerBase, IDashboardManager
    {

        private readonly IRepository<NFAMaster> NFAMasterRepo;
        public DashboardManager(IRepository<NFAMaster> nfaMasterRepo)
        {
            NFAMasterRepo = nfaMasterRepo;
        }

        public IEnumerable<Dictionary<string, object>> GetApprovalDashboardData(int perosnID)
        {
            string sql = $@"SELECT
                        ReferenceNo,
                        AEF.EmployeeID,
                        NFA.NFAID,
                        VAE.FullName+' - '+EmployeeCode Employee,
                        ISNULL(DATEDIFF(hour, FeedbackRequestDate, ISNULL(FeedBackSubmitDate,GETDATE())),0) HourDiff
                        FROM
                        Security..NFAMaster NFA
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = NFA.NFAID AND APTypeID = 2
                        LEFT JOIN Approval..ApprovalEmployeeFeedback AEF ON AEF.ApprovalProcessID = AP.ApprovalProcessID
                        LEFT JOIN HRMS..ViewALLEmployee VAE ON VAE.EmployeeID = AEF.EmployeeID
                        WHERE NFA.ApprovalStatusID=22

                        ORDER BY AEF.EmployeeID,NFAID asc";
            var data = NFAMasterRepo.GetDataDictCollection(sql);
            return data;
        }

    }
}
