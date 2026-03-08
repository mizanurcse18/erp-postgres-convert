using Security.DAL.Entities;
using Security.Manager;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Security.Manager.Dto
{
    public class PersonSaveModel
    {
        public PersonDto MasterModel { get; set; }
        public List<PersonImageDto> PersonImages { get; set; }
        public List<PersonWorkExperienceDto> WorkExperience { get; set; }
        public List<PersonAcademicInfoDto> AcademicQualification { get; set; }
        public List<PersonTrainingInfoDto> TrainingInfo { get; set; }
        public List<PersonAwardInfoDto> AwardInfo { get; set; }
        public List<PersonFamilyInfoDto> ChildrenInfo { get; set; }
        public List<PersonReferenceInfoDto> ReferenceInfo { get; set; }
        public List<NomineeDto> NomineeInfo { get; set; }
        public PersonEmergencyContactInfoDto EmergencyContact { get; set; }
        public EmployeeProfileApprovalDto EmployeeProfileApproval { get; set; }
        public bool IsEditProfile { get; set; }
        public bool IsChangedForApproval { get; set; }
        public string UpdatedFieldTrackerObj { get; set; }
        public string PermissionError { get; set; }
    }
}
