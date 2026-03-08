using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using SCM.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCM.Manager.Dto
{
    [AutoMap(typeof(VendorAssessmentMembers)), Serializable]
    public class VendorAssessmentMemberDto : Auditable
    {
        public long PrimaryID { get; set; }
        public long EmployeeID { get; set; }
        public List<AssessmentMemberCombo> AssessmentMembers { get; set; }


    }
    public class AssessmentMemberCombo
    {
        public string label { get; set; }
        public int value { get; set; }

    }
}
