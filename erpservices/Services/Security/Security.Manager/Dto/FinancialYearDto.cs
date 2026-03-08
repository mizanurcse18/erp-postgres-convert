using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;


namespace Security.Manager
{
    [AutoMap(typeof(FinancialYear)), Serializable]
    public class FinancialYearDto : Auditable
    {

        public int FinancialYearID { get; set; }
        public int Year { get; set; }
        public string YearDescription { get; set; }
        public bool IsCurrent { get; set; }
        public int monthID { get; set; }

    }
}
