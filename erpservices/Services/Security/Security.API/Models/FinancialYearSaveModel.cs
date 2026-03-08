using Security.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Security.API.Models
{
    public class FinancialYearSaveModel
    {
        public FinancialYearDto MasterModel { get; set; }
        public List<PeriodDto> ChildModels { get; set; }
    }
}
