using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Security.Manager.Dto
{
    [AutoMap(typeof(PersonAwardInfo)), Serializable]
   public class PersonAwardInfoDto: Auditable
    {
        public int PAIID { get; set; }
        public int PersonID { get; set; }
        public string AwardType { get; set; }
        public string InstituteName { get; set; }
        public int Year { get; set; }
        public string Reason { get; set; }
    }
}
