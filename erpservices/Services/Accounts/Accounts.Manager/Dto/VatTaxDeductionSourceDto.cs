using Manager.Core.Mapper;
using DAL.Core.EntityBase;
using Accounts.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Accounts.Manager.Dto
{
    [AutoMap(typeof(VatTaxDeductionSource)), Serializable]
    public class VatTaxDeductionSourceDto : Auditable
    {
        public int VTDSID { get; set; }
        public int SourceTypeID { get; set; }
        public string SectionOrServiceCode { get; set; }
        public string ServiceName { get; set; }
        public string RatePercent { get; set; }
        public long FinancialYearID { get; set; }
        public int Year { get; set; }
    }

    public class VatTaxDeductionSourceListDto
    {
        public int VTDSID { get; set; }
        public int SourceTypeID { get; set; }
        public string SectionOrServiceCode { get; set; }
        public string ServiceName { get; set; }
        public string RatePercent { get; set; }
        public int Year { get; set; }
        public long FinancialYearID { get; set; }
        public int NumberOfVatTaxDeductionSource { get; set; }
        public List<VatTaxDeductionSourceDto> VatTaxDeductionSourceDetails { get; set; }
    }
}
