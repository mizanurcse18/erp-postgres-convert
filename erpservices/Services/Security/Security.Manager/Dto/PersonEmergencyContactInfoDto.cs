using Manager.Core.Mapper;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Security.Manager.Dto
{
    [AutoMap(typeof(PersonEmergencyContactInfo)), Serializable]
    public class PersonEmergencyContactInfoDto
    {
        public int PECIID { get; set; }
        public int PersonID { get; set; }
        public string Name { get; set; }
        public string ContactNo { get; set; }
        public string Relationship { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
    }
}
