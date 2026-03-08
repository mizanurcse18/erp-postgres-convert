using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Security.Manager.Dto
{
    [AutoMap(typeof(PersonTrainingInfo)), Serializable]
    public class PersonTrainingInfoDto : Auditable
    {
        public int PTIID { get; set; }
        public int PersonID { get; set; }
        public string Title { get; set; }
        public string Trainer { get; set; }
        public int CountryID { get; set; }
        public string InstituteName { get; set; }
        public int TrainingYear { get; set; }
        public string DurationType { get; set; }
        public int? Duration { get; set; }
        public string Location { get; set; }
    }
}
