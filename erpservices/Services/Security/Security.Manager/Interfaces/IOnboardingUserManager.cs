using Core;
using Security.DAL.Entities;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Security.Manager.Interfaces
{
    public interface IOnboardingUserManager
    {
        OnboardingUserDto SignIn(string userName, string password, DateTime logInDate);
        Task<List<UserListDto>> GetUsers();
        UserThemeSetting GetSettings(int userid);

        Task<OnboardingUserDto> GetUser(int userId);
        Task<OnboardingUserDto> GetUserLatest(int userId);
        Task<OnboardingUserDto> SaveChanges(OnboardingUserDto OnboardingUserDto);
        void Delete(OnboardingUserDto OnboardingUserDto);
        Task SignOutAsync(int logedId, DateTime signOutDate);
        GridModel GetUserGroups(GridParameter parameters);
        GridModel GetUserCompanies(GridParameter parameters);
        //Task<List<MenuDto>> GetMenus(int UserId, int ApplicationId);
        Task<IEnumerable<Dictionary<string, object>>> GetMenus(int UserId, int ApplicationId,bool IsAdmin);
        Task<string> GetMenusJson(int UserId, int ApplicationId);
        Task<IEnumerable<Dictionary<string, object>>> GetUserListDic();        
        Task ResetPasswordAsync(string email, string userName);
        Task<OnboardingUserDto> ChangePassword(OnboardingUserDto OnboardingUserDto);
        Task SaveUserSettings(string settings, string shortcuts);
        Task AddAsUser(int UserID);



        //public Task<Dictionary<string, object>> GetPersonTableDic(int primaryID);
        //Task<List<PersonImageDto>> GetPersonImageList(int PersonID);
        //Task<List<PersonWorkExperienceDto>> GetPersonWorkExperienceList(int personID);
        //Task<List<PersonAcademicInfoDto>> GetPersonAcademicInfoList(int personID);
        //Task<List<PersonTrainingInfoDto>> GetPersonTrainingInfoList(int personID);
        //Task<List<PersonAwardInfoDto>> GetPersonAwardInfoList(int personID);
        //Task<List<PersonFamilyInfoDto>> GetPersonFamilyInfoList(int personID);
        //Task<List<PersonReferenceInfoDto>> GetPersonReferenceInfoList(int personID);
        //Task<PersonEmergencyContactInfoDto> GetPersonEmergencyContactInfo(int personID);
        //Task<List<NomineeDto>> GeNomineeInfoList(int personID);

        //Task<List<ComboModel>> SystemVariable(int entityTypeID);
        //Task<List<ComboModel>> GetDistricts();
        //Task<List<ComboModel>> GetThanas(int districtId);
        //Task<PersonDto> SaveChangesPerson(PersonSaveModel personSaveModel);
        //Task UpdateOnBoardFlag(int PersonID);
    }
}
