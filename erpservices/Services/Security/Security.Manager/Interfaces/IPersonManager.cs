using Core;
using Security.DAL.Entities;
using Security.Manager.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Security.Manager.Interfaces
{
    public interface IPersonManager
    {
        Task<List<PersonImageDto>> GetPersonImageList(int PersonID);
        GridModel GetPersonListDic(GridParameter parameters);
        GridModel GetsMyProfileApprovalList(GridParameter parameters);
        Task<IEnumerable<Dictionary<string, object>>> GetPersonListDic();
        Task<IEnumerable<Dictionary<string, object>>> GetUnemploymentListDic();
        public Task<Dictionary<string, object>> GetPersonTableDic(int primaryID);
        public Task<Dictionary<string, object>> GetPersonInfoDic(int primaryID);
        //public Task<Dictionary<string, object>> GetPersonSupervisorInfoDic(int primaryID);
        Task<List<PersonDto>> GetPersonSupervisorInfoDic(int personID);
        Task<PersonDto> SaveChanges(PersonSaveModel personSaveModel);
        Task<PersonDto> ProfileUpdateWithApproval(PersonSaveModel personSaveModel);
        Task UpdateOnBoardFlag(int PersonID);
        Task RemovePerson(int PersonID);
        Task RemovePersonImage(int PersonImageID);
        Task<string> GetMediaList(int personID);
        Task<Dictionary<string, object>> GetPersonAboutInfo(int personID);
        Task<List<PersonWorkExperienceDto>> GetPersonWorkExperienceList(int personID);
        Task<List<PersonAcademicInfoDto>> GetPersonAcademicInfoList(int personID);
        Task<List<PersonTrainingInfoDto>> GetPersonTrainingInfoList(int personID);
        Task<List<PersonAwardInfoDto>> GetPersonAwardInfoList(int personID);
        Task<List<PersonFamilyInfoDto>> GetPersonFamilyInfoList(int personID);
        Task<List<PersonReferenceInfoDto>> GetPersonReferenceInfoList(int personID);
        Task<PersonEmergencyContactInfoDto> GetPersonEmergencyContactInfo(int personID);
        Task<List<NomineeDto>> GeNomineeInfoList(int personID);
        Task<Dictionary<string, object>> GetEmployeeUpdateApproval(int EPAID);


        IEnumerable<Dictionary<string, object>> GetApprovalComment(int aprovalProcessId);

        Task<List<ComboModel>> GetForwardingMemberList(int ReferenceID, int APTypeID, int APPanelID);
        Task<List<ComboModel>> GetForwardingMemberList(int ApprovalProcessID);
        Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId);

        IEnumerable<Dictionary<string, object>> ReportForEPAApprovalFeedback(int POID);
        IEnumerable<Dictionary<string, object>> GetForwardingMemberComments(int APTypeID, int ReferenceID);
    }
}
