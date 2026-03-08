using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Security.Manager
{
    [AutoMap(typeof(Period)), Serializable]
    public class PeriodDto:Auditable
    {
        public int PeriodID { get; set; }
        public int FinancialYearID { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndtDate { get; set; }
        public decimal SeqNo { get; set; }
        public bool IsCurrent { get; set; }

        public int FinancialYear { get; set; }
    }
}
