
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using SCM.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCM.Manager.Dto
{
    [AutoMap(typeof(VatInfo)), Serializable]
    public class VatInfoDto : Auditable
    {
        public long VatInfoID { get; set; }
        public decimal VatPercent { get; set; }
        public string VatPolicies { get; set; }
        public bool IsRebateable { get; set; }
        public decimal RebatePercentage { get; set; }
        public int value { get; set; }
        public string label { get; set; }
    }
}
