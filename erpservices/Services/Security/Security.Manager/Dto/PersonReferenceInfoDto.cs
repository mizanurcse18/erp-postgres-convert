using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Security.Manager.Dto
{
    [AutoMap(typeof(PersonReferenceInfo)), Serializable]
    public class PersonReferenceInfoDto : Auditable
    {
        public int PRIID { get; set; }
        public int PersonID { get; set; }
        public int ReferenceTypeID { get; set; }
        public string ReferenceName { get; set; }
        public string Organization { get; set; }
        public string Designation { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string Relationship { get; set; }
        public bool IsCompanyEmployee { get; set; }
    }
}
