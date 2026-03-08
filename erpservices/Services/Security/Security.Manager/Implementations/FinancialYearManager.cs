using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.EntityBase;
using DAL.Core.Extension;
using DAL.Core.Repository;
using Manager.Core;
using Manager.Core.Mapper;
using Security.DAL;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Security.Manager
{
    public class FinancialYearManager : ManagerBase, IFinancialYearManager
    {
        private readonly IRepository<FinancialYear> FinancialYearRepo;
        private readonly IRepository<Period> PeriodRepo;
        //readonly IModelAdapter Adapter;
        public FinancialYearManager(IRepository<FinancialYear> financialYearRepo,
            IRepository<Period> periodRepo
            )
        {
            FinancialYearRepo = financialYearRepo;
            PeriodRepo = periodRepo;
        }

        public async Task<List<FinancialYearDto>> GetFinancialYearTables()
        {
            var financialYearTables = await FinancialYearRepo.GetAllListAsync(financialYear => financialYear.CompanyID == AppContexts.User.CompanyID.ToString());
            return financialYearTables.MapTo<List<FinancialYearDto>>();
        }

        public async Task<FinancialYearDto> GetFinancialYear(int primaryID)
        {
            var financialYearTable = await FinancialYearRepo.GetAsync(primaryID);
            return financialYearTable.MapTo<FinancialYearDto>();
        }
        public async Task<FinancialYearDto> GetFinancialYearByYear(int year)
        {
            var financialYearTable = FinancialYearRepo.Entities.Where(x => x.Year == year).FirstOrDefault();
            await Task.CompletedTask;
            return financialYearTable.MapTo<FinancialYearDto>();
        }

        public async Task<List<PeriodDto>> GetPeriods(int financialYearID)
        {
            List<PeriodDto> PeriodList = new List<PeriodDto>();

            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                PeriodList = (
                    from rules in FinancialYearRepo.Entities
                    join periods in PeriodRepo.Entities.Where(x => x.FinancialYearID == financialYearID)
                    on rules.FinancialYearID equals periods.FinancialYearID
                    select new PeriodDto
                    {
                        PeriodID = periods.PeriodID,
                        FinancialYearID = periods.FinancialYearID,
                        PeriodStartDate = periods.PeriodStartDate,
                        PeriodEndtDate = periods.PeriodEndtDate,
                        SeqNo = periods.SeqNo,
                        IsCurrent = periods.IsCurrent
                    }).ToList();
            }

            return await Task.FromResult(PeriodList);
        }


        public async Task<List<PeriodDto>> GetPeriodByID(int financialYearID)
        {
            string sql = $@"select pe.PeriodID, pe.FinancialYearID, pe.PeriodStartDate, pe.PeriodEndtDate, pe.SeqNo, pe.IsCurrent, FY.Year AS FinancialYear
                            From Period pe
                            INNER JOIN FinancialYear FY ON pe.FinancialYearID = FY.FinancialYearID
                            WHERE pe.FinancialYearID = {financialYearID}";
            var listDict = PeriodRepo.GetDataModelCollection<PeriodDto>(sql);

            await Task.CompletedTask;
            return listDict;
        }

        public GridModel GetFinancialYearRules(GridParameter parameters)
        {
            const string sql = "SELECT * FROM SecurityRuleMaster";
            var result = FinancialYearRepo.LoadGridModel(parameters, sql);
            return result;
        }

        public async Task<FinancialYearDto> SaveChanges(FinancialYearDto master, List<PeriodDto> childs = null)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                if (childs.IsNull()) childs = new List<PeriodDto>();
                var existMaster = FinancialYearRepo.Entities.SingleOrDefault(x => x.FinancialYearID == master.FinancialYearID).MapTo<FinancialYearDto>();
                if (
                    master.FinancialYearID.IsZero() || master.IsAdded)
                {
                    master.SetAdded();
                    SetNewId(master);
                }
                else
                {
                    master.CreatedBy = existMaster.CreatedBy;
                    master.CreatedDate = existMaster.CreatedDate;
                    master.CreatedIP = existMaster.CreatedIP;
                    master.RowVersion = existMaster.RowVersion;
                    master.SetModified();
                }

                master.IsCurrent = childs.Count(x => x.IsCurrent == true) > 0;

                foreach (var child in childs)
                {
                    var existsPeriod = PeriodRepo.Entities.SingleOrDefault(x => x.PeriodID == child.PeriodID && x.FinancialYearID == master.FinancialYearID).MapTo<PeriodDto>();
                    if (existsPeriod.IsNull())
                    {
                        child.SetAdded();
                        SetNewId(child);
                    }
                    else {

                        child.CreatedBy = existsPeriod.CreatedBy;
                        child.CreatedDate = existsPeriod.CreatedDate;
                        child.CreatedIP = existsPeriod.CreatedIP;
                        child.RowVersion = existsPeriod.RowVersion;
                        child.SetModified(); 
                    }

                    child.FinancialYearID = master.FinancialYearID;
                }
                var masterEnt = master.MapTo<FinancialYear>();
                var childsEnt = childs.MapTo<List<Period>>();

                //Set Audti Fields Data
                SetAuditFields(childsEnt);
                SetAuditFields(masterEnt);

                FinancialYearRepo.Add(masterEnt);
                PeriodRepo.AddRange(childsEnt);

                if (master.IsCurrent)
                    UpdateCurrentFinancialYear(master.Year);


                unitOfWork.CommitChangesWithAudit();

                master = masterEnt.MapTo<FinancialYearDto>();
                masterEnt.MapToAuditFields(master);
            }
            await Task.CompletedTask;

            return master;
        }

        public async Task RemoveFinancialYear(int FinancialYearID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var masterEnt = FinancialYearRepo.Entities.Where(x => x.FinancialYearID == FinancialYearID).FirstOrDefault();
                masterEnt.SetDeleted();
                var childEnt = PeriodRepo.Entities.Where(x => x.FinancialYearID == FinancialYearID).ToList();
                childEnt.ChangeState(ModelState.Deleted);

                PeriodRepo.AddRange(childEnt);
                FinancialYearRepo.Add(masterEnt);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }

        private void SetNewId(FinancialYearDto financialYearTable)
        {
            if (!financialYearTable.IsAdded) return;
            var code = GenerateSystemCode("FinancialYear", AppContexts.User.CompanyID);
            financialYearTable.FinancialYearID = code.MaxNumber;
        }

        private void SetNewId(PeriodDto financialYearRuleChildTable)
        {
            if (!financialYearRuleChildTable.IsAdded) return;
            var code = GenerateSystemCode("Period", AppContexts.User.CompanyID);
            financialYearRuleChildTable.PeriodID = code.MaxNumber;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetFinancialYearListWithDetails()
        {
            var sql = $@"SELECT 
	        Fin.FinancialYearID
	        ,fin.CompanyID
	        ,fin.Year
	        ,fin.IsCurrent
	        ,fin.YearDescription
	        ,CASE 
		        WHEN CLP.CLPolicyID IS NULL
			        THEN CAST(1 AS BIT)
		        ELSE CAST(0 AS BIT)
		        END IsRemovable
	        FROM 
	        Security.dbo.FinancialYear Fin
	        LEFT JOIN (
		        SELECT DISTINCT CLPolicyID, FinancialYearID
		        FROM HRMS..CompanyLeavePolicy 
	        ) CLP ON CLP.FinancialYearID= Fin.FinancialYearID
                            WHERE fin.CompanyID = '{AppContexts.User.CompanyID}'";

            var listDict = FinancialYearRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }

        public async Task<List<PeriodDto>> GetGenerateChildList(FinancialYearDto finYear)
        {
            var periodList = new List<PeriodDto>();
            int count = 1;
            int initialYear = finYear.Year;
            int initialMonth = finYear.monthID;
            bool isNextYear = false;
            for (int i = finYear.monthID; count <= 12; i++)
            {
                if (i > 12 && !isNextYear)
                {
                    initialYear = initialYear + 1;
                    initialMonth = 1;
                    isNextYear = true;
                }
                PeriodDto periodObj = new PeriodDto();
                periodObj.PeriodID = count;
                periodObj.PeriodStartDate = new DateTime(initialYear, initialMonth, 1);
                periodObj.PeriodEndtDate = periodObj.PeriodStartDate.AddMonths(1).AddSeconds(-1);
                periodObj.SeqNo = count;
                periodObj.FinancialYear = finYear.Year;
                periodList.Add(periodObj);

                ++initialMonth;
                ++count;
            }
            await Task.CompletedTask;
            return periodList;
        }

        public async Task<bool> GetExistingFinancialYear(int year)
        {
            bool isExists = FinancialYearRepo.Entities.Count(x => x.Year == year) > 0;
            await Task.CompletedTask;
            return isExists;
        }

    }
}
