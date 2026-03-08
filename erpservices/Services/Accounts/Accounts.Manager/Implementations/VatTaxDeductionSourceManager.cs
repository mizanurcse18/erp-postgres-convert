using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.EntityBase;
using DAL.Core.Extension;
using DAL.Core.Repository;
using Accounts.DAL.Entities;
using Accounts.Manager.Dto;
using Accounts.Manager.Interfaces;
using Manager.Core;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.Manager.Implementations
{
    public class VatTaxDeductionSourceManager : ManagerBase, IVatTaxDeductionSourceManager
    {
        private readonly IRepository<VatTaxDeductionSource> VatTaxDeductionSourceRepo;
        public VatTaxDeductionSourceManager(IRepository<VatTaxDeductionSource> vatTaxDeductionSourceRepo)
        {
            VatTaxDeductionSourceRepo = vatTaxDeductionSourceRepo;
        }

        public async Task<List<VatTaxDeductionSourceListDto>> GetVatTaxDeductionSource(int financialYearID)
        {
            string sql = $@"SELECT        H.*, F.Year
                            FROM            VatTaxDeductionSource AS H INNER JOIN
                            {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FinancialYear AS F ON H.FinancialYearID = F.FinancialYearID WHERE H.FinancialYearID={financialYearID}";

            return await Task.FromResult(VatTaxDeductionSourceRepo.GetDataModelCollection<VatTaxDeductionSourceListDto>(sql).ToList());
        }

        public async Task<List<VatTaxDeductionSourceListDto>> GetVatTaxDeductionSourceList()
        {
            string sql = $@"SELECT  H.*, F.Year
                            FROM VatTaxDeductionSource AS H INNER JOIN
                            {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FinancialYear AS F ON H.FinancialYearID = F.FinancialYearID";
            var list = VatTaxDeductionSourceRepo.GetDataModelCollection<VatTaxDeductionSourceListDto>(sql).ToList();

            var model = list.GroupBy(x => new { x.Year, x.FinancialYearID }).Select(x => new VatTaxDeductionSourceListDto
            {
                Year = x.Key.Year,
                FinancialYearID = x.Key.FinancialYearID,
                NumberOfVatTaxDeductionSource = x.Count(),
                VatTaxDeductionSourceDetails = list.Where(y => y.FinancialYearID == x.Key.FinancialYearID).Select(a => new VatTaxDeductionSourceDto
                {
                    VTDSID = a.VTDSID,
                    FinancialYearID = a.FinancialYearID,
                    SourceTypeID = a.SourceTypeID,
                    SectionOrServiceCode = a.SectionOrServiceCode,
                    ServiceName = a.ServiceName,
                    RatePercent = a.RatePercent,
                    Year = a.Year
                }).ToList()
            }).ToList();

            return await Task.FromResult(model);
        }

        public void RemoveVatTaxDeductionSource(int financialYearID)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var existingList = VatTaxDeductionSourceRepo.Entities.Where(x => x.FinancialYearID == financialYearID).ToList();
                existingList.ChangeState(ModelState.Deleted);
                VatTaxDeductionSourceRepo.AddRange(existingList);
                unitOfWork.CommitChangesWithAudit();
            }
        }

        public void SaveChanges(List<VatTaxDeductionSourceDto> vatTaxDeductionSource)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var vatTaxDeductionSourceList = vatTaxDeductionSource.MapTo<List<VatTaxDeductionSource>>();
                var existingList = VatTaxDeductionSourceRepo.Entities.Where(x => x.FinancialYearID == vatTaxDeductionSource.First().FinancialYearID).ToList();

                if (vatTaxDeductionSourceList.IsNotNull())
                {
                    vatTaxDeductionSourceList.ForEach(x =>
                    {
                        //if (x.ServiceName.Length > 0 && x.VatTaxDeductionSourceDate.IsNotMinValue())
                        if (x.ServiceName.Length > 0)
                        {
                            if (x.VTDSID > 0)
                            {
                                x.CreatedBy = existingList.First().CreatedBy;
                                x.CreatedDate = existingList.First().CreatedDate;
                                x.CreatedIP = existingList.First().CreatedIP;
                                x.RowVersion = existingList.Where(y => y.VTDSID == x.VTDSID).First().RowVersion;
                                x.SetModified();
                            }
                            else
                            {
                                x.SetAdded();
                                SetNewVTDSID(x);
                            }
                        }

                    });

                    if (existingList.Count >= vatTaxDeductionSourceList.Count)
                    {
                        var willDeleted = existingList.Where(x => !vatTaxDeductionSourceList.Select(y => y.VTDSID).Contains(x.VTDSID)).ToList();
                        willDeleted.ForEach(x =>
                        {
                            x.SetDeleted();
                            vatTaxDeductionSourceList.Add(x);
                        });
                    }
                }
                SetAuditFields(vatTaxDeductionSourceList);
                VatTaxDeductionSourceRepo.AddRange(vatTaxDeductionSourceList);
                unitOfWork.CommitChangesWithAudit();
            }
        }

        private void SetNewVTDSID(VatTaxDeductionSource obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("VatTaxDeductionSource", AppContexts.User.CompanyID);
            obj.VTDSID = code.MaxNumber;
        }


    }
}
