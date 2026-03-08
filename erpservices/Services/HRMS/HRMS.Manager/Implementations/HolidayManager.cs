using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.EntityBase;
using DAL.Core.Extension;
using DAL.Core.Repository;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Manager.Core;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Implementations
{
    public class HolidayManager : ManagerBase, IHolidayManager
    {
        private readonly IRepository<Holiday> HolidayRepo;
        public HolidayManager(IRepository<Holiday> holidayRepo)
        {
            HolidayRepo = holidayRepo;
        }

        public async Task<List<HolidayListDto>> GetHoliday(int financialYearID)
        {
            string sql = $@"SELECT        H.HolidayID, H.FinancialYearID, H.Name, H.HolidayDate, H.Remarks, H.ImagePath, F.Year, H.IsFestivalHoliday
                            FROM            Holiday AS H INNER JOIN
                            {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FinancialYear AS F ON H.FinancialYearID = F.FinancialYearID WHERE H.FinancialYearID={financialYearID}";

            return await Task.FromResult(HolidayRepo.GetDataModelCollection<HolidayListDto>(sql).ToList());
        }

        public async Task<List<HolidayListDto>> GetHolidayList()
        {
            string sql = $@"SELECT        H.HolidayID, H.FinancialYearID, H.Name, H.HolidayDate, H.Remarks, H.ImagePath, F.Year,H.IsFestivalHoliday
                            FROM            Holiday AS H INNER JOIN
                            {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FinancialYear AS F ON H.FinancialYearID = F.FinancialYearID";
            var list = HolidayRepo.GetDataModelCollection<HolidayListDto>(sql).ToList();

            var model = list.GroupBy(x => new { x.Year, x.FinancialYearID }).Select(x => new HolidayListDto
            {
                Year = x.Key.Year,
                FinancialYearID = x.Key.FinancialYearID,
                NumberOfHoliday = x.Count(),
                HolidayDetails = list.Where(y => y.FinancialYearID == x.Key.FinancialYearID).Select(a => new HolidayDto
                {
                    HolidayID = a.HolidayID,
                    FinancialYearID = a.FinancialYearID,
                    HolidayDate = a.HolidayDate,
                    ImagePath = a.ImagePath,
                    Name = a.Name,
                    Remarks = a.Remarks
                }).ToList()
            }).ToList();

            return await Task.FromResult(model);
        }

        public void RemoveHoliday(int financialYearID)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var existingList = HolidayRepo.Entities.Where(x => x.FinancialYearID == financialYearID).ToList();
                existingList.ChangeState(ModelState.Deleted);
                HolidayRepo.AddRange(existingList);
                unitOfWork.CommitChangesWithAudit();
            }
        }

        public void SaveChanges(List<HolidayDto> holiday)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var holidayList = holiday.MapTo<List<Holiday>>();
                var existingList = HolidayRepo.Entities.Where(x => x.FinancialYearID == holiday.First().FinancialYearID).ToList();

                if (holidayList.IsNotNull())
                {
                    holidayList.ForEach(x =>
                    {
                        if (x.Name.Length > 0 && x.HolidayDate.IsNotMinValue())
                        {
                            if (x.HolidayID > 0)
                            {
                                x.CreatedBy = existingList.First().CreatedBy;
                                x.CreatedDate = existingList.First().CreatedDate;
                                x.CreatedIP = existingList.First().CreatedIP;
                                x.RowVersion = existingList.Where(y => y.HolidayID == x.HolidayID).First().RowVersion;
                                x.SetModified();
                            }
                            else
                            {
                                x.SetAdded();
                                SetNewHolidayID(x);
                            }
                        }

                    });

                    if (existingList.Count >= holidayList.Count)
                    {
                        var willDeleted = existingList.Where(x => !holidayList.Select(y => y.HolidayID).Contains(x.HolidayID)).ToList();
                        willDeleted.ForEach(x =>
                        {
                            x.SetDeleted();
                            holidayList.Add(x);
                        });
                    }
                }
                SetAuditFields(holidayList);
                HolidayRepo.AddRange(holidayList);
                unitOfWork.CommitChangesWithAudit();
            }
        }

        private void SetNewHolidayID(Holiday obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("Holiday", AppContexts.User.CompanyID);
            obj.HolidayID = code.MaxNumber;
        }


    }
}
