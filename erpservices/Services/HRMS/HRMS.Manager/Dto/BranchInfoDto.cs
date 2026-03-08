
using DAL.Core.EntityBase;
using HRMS.DAL.Entities;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    [AutoMap(typeof(BranchInfo)), Serializable]
    public class BranchInfoDto : Auditable
    {
        public int BranchID { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public int? RegionID { get; set; }
        public string RegionName { get; set; }

    }
}
