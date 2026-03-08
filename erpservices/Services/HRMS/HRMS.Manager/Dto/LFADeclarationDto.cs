
using Core;
using DAL.Core.EntityBase;
using HRMS.DAL.Entities;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    [AutoMap(typeof(LFADeclaration)), Serializable]
    public class LFADeclarationDto : Auditable
    {
        public int LFADID { get; set; }
        public string Year { get; set; }
        public int TravelType { get; set; }
        public string TravelDestination { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int EmployeeLeaveAID { get; set; }
        public int ApprovalStatusID { get; set; } = (int)Util.ApprovalStatus.Approved;

    }
}
