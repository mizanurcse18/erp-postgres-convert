using Core;
using Manager.Core.CommonDto;
using Security.DAL.Entities;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Security.Manager.Implementations.UserManager;

namespace Security.Manager.Interfaces
{
    public interface IUserManager
    {
        UserDto SignIn(string userName, string password, DateTime logInDate);
        bool CheckCurrentPasswordMatch(string password, UserDto user);
        Task<List<UserListDto>> GetUsers();
        UserDto GetAPPUserData(int userId, int personId);
        UserThemeSetting GetSettings(int userid);
        Task<UserDto> GetUser(int userId);
        Task<UserDto> GetUserLatest(int userId,int personid);
        Task<UserDto> GetUserLatest(string UserName);
        Task<UserDto> SaveChanges(UserDto userDto);
        Task<UserDto> SaveChangesPrismAmRsmToTm(UserDto userDto, UserProfileDto UserProfileDto);
        void Delete(UserDto userDto);
        Task SignOutAsync(int logedId, DateTime signOutDate);
        GridModel GetUserGroups(GridParameter parameters);
        GridModel GetUserCompanies(GridParameter parameters);
        //Task<List<MenuDto>> GetMenus(int UserId, int ApplicationId);
        Task<IEnumerable<Dictionary<string, object>>> GetMenus(int UserId, int ApplicationId, bool IsAdmin);
        Task<List<MenuApiPathsView>> MenuApiPaths(int UserId, int ApplicationId);
        Task<string> MenuApiPathsJsonString(int UserId, int ApplicationId);
        Task<string> GetMenusJson(int UserId, int ApplicationId);
        Task<IEnumerable<Dictionary<string, object>>> GetUserListDic();
        GridModel GetUserListDicGrid(GridParameter parameters);
        Task<List<ComboModel>> GetSecurityGroupUserChildList(int userID);
        Task ResetPasswordAsync(string email, string userName);
        Task<UserDto> ChangePassword(UserDto userDto);
        Task SaveUserSettings(string settings, string shortcuts);
        //EmailState ResetPasswordChange(UserDto userDto);
        EmailState RequestForForgotPassword(UserDto userDto);
        (int, string, bool) GetUserByRequestToken(UserDto userDto);
        (string, bool) ResetPassword(UserDto userDto);
        bool HasPermissionChangeUser(string employeeCode);
        bool GetIsUniqueUserName(int userId, string userName);
        UserLoginPolicyDto CheckUserLoginPolicy(string userName, string password);
        void SaveHashedTokenToBlackList(string hashedToken);
        List<UserTokenBlackList> GetHashedTokensFromBlackList();
        Task<UserAndProfileDto> SaveChangesPrismUser(UserAndProfileDto userAndProfileDto);
        GridModel GetPrismUserListDicGrid(GridParameter parameters);
        Task<List<Dictionary<string, object>>> GetPrismUser(int PositionID, int userId);
        Task<List<Dictionary<string, object>>> GetDistributionHouses();
        Task<List<SaveFileDescription>> GetExportUserList(string WhereCondition);
        Task<(bool, string)> UploadPrismUser(PrismUser_Dto dto);
        Task<List<Dictionary<string, object>>> GetBlackListToken(string token);
    }
}
