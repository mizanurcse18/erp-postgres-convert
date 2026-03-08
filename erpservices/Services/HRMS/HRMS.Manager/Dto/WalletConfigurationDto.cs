
using DAL.Core.EntityBase;
using HRMS.DAL.Entities;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    [AutoMap(typeof(WalletConfiguration)), Serializable]
    public class WalletConfigurationDto : Auditable
    {
        public int WalletConfigureID { get; set; }
        public decimal CashOutRate { get; set; }
        public int DesignationID { get; set; }
        public string DesignationName { get; set; }
        public decimal Percentage { get; set; }
        public int TypeID { get; set; }
        public string TypeName { get; set; }
        public bool ExceptionFlag { get; set; }
        public List<WalletConfigurationDto> Configurations { get; set; }

    }
}
