using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Security.Manager.Dto
{
    [AutoMap(typeof(PersonAcademicInfo)), Serializable]
    public class PersonAcademicInfoDto: Auditable
    {
        public int PAIID { get; set; }
        public int PersonID { get; set; }
        public string DegreeOrCertification { get; set; }
        public string InstituteName { get; set; }
        public string BoardOrUniversity { get; set; }
        public string SubjectOrArea { get; set; }
        public int PassingYear { get; set; }
        public string Result { get; set; }
        public bool IsLastAcademic { get; set; }
    }
}
