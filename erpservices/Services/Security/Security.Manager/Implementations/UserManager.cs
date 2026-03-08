using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.EntityBase;
using DAL.Core.Repository;
using Manager.Core;
using Manager.Core.CommonDto;
using Manager.Core.Mapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Security.DAL.Entities;
using Security.Manager.Dto;
using Security.Manager.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Concurrent; // Add this using directive

using Newtonsoft.Json;
using System.Collections.Immutable;


namespace Security.Manager.Implementations
{
    public class UserManager : ManagerBase, IUserManager
    {
        private readonly IRepository<User> UserRepo;
        private readonly IRepository<UserLogTracker> LogRepo;
        private readonly IRepository<Menu> MenuRepo;
        private readonly IModelAdapter Adapter;
        private readonly IRepository<Person> PersonRepo;
        private readonly IRepository<PersonImage> PersonImageRepo;
        private readonly IRepository<SecurityGroupUserChild> SecurityGroupUserChildRepo;
        private readonly IRepository<UserThemeSetting> UserThemeSettingRepo;
        private readonly IRepository<SystemConfiguration> SystemConfigurationRepo;
        private readonly IRepository<UserTokenBlackList> UserTokenBlackListRepo;
        private readonly IRepository<UserProfile> UserProfileRepo;
        readonly IEmailManager EmailManager;
        private readonly IConfiguration _configuration;
        private SmtpClient client = new SmtpClient();
        //private readonly IRepository<MenuPrivilege> MenuPrivilegeRepo;

        private static readonly ConcurrentDictionary<string, EmailState> AllEmailsAndTimes = new ConcurrentDictionary<string, EmailState>();
        private static readonly ConcurrentDictionary<string, DateTime> LastPasswordResetRequestTimes = new ConcurrentDictionary<string, DateTime>();

        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserManager(IRepository<User> userRepo,
            IRepository<UserLogTracker> logRepo,
            IRepository<Menu> menuRepo,
            IModelAdapter adapter,
            IRepository<Person> PersonRepo,
            IRepository<PersonImage> PersonImageRepo,
            IRepository<SecurityGroupUserChild> securityGroupUserChildRepo,
            IRepository<UserThemeSetting> userThemeSetting,
            IEmailManager emailManager,
            IRepository<SystemConfiguration> systemConfigurationRepo,
            IRepository<UserTokenBlackList> userTokenBlackListRepo,
            IRepository<UserProfile> userProfileRepo,
            //IRepository<MenuPrivilege> menuPrivilegeRepo
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor

            )
        {
            UserRepo = userRepo;
            LogRepo = logRepo;
            MenuRepo = menuRepo;
            Adapter = adapter;
            this.PersonRepo = PersonRepo;
            this.PersonImageRepo = PersonImageRepo;
            SecurityGroupUserChildRepo = securityGroupUserChildRepo;
            EmailManager = emailManager;
            UserThemeSettingRepo = userThemeSetting;
            //MenuPrivilegeRepo = menuPrivilegeRepo;
            SystemConfigurationRepo = systemConfigurationRepo;
            UserTokenBlackListRepo = userTokenBlackListRepo;
            UserProfileRepo = userProfileRepo;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public UserThemeSetting GetSettings(int userid)
        {

            var setting = UserThemeSettingRepo.SingleOrDefault(x => x.UserID == userid);
            if (setting.IsNull()) return null;
            return setting;
        }
        public bool CheckCurrentPasswordMatch(string password, UserDto currentuser)
        {
            var user = UserRepo.SingleOrDefault(x => x.UserID == currentuser.UserID);
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt)) return false;
            return true;
        }
        public UserDto SignIn(string userName, string password, DateTime logInDate)
        {
            if (userName.IsNullOrEmpty() || password.IsNullOrEmpty()) return null;
            if (userName.Equals(Util.Integrations.superadmin.ToString()) && password.Equals(Util.integrationHashToken))
                return GetUserWhoHasPermission(userName);

            var user = UserRepo.SingleOrDefault(x => x.UserName == userName);


            if (user.IsNull()) return null;

            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt)) return null;

            var log = new UserLogTracker
            {
                UserID = user.UserID,
                IsLive = true,
                LogInDate = logInDate,
                IPAddress = AppContexts.GetIPAddress(),
                ObjectState = ModelState.Added,
                CompanyID = AppContexts.User.CompanyID
            };

            LogRepo.Add(log);
            LogRepo.SaveChanges();

            UserDto userDto = SetUserData(user, log);

            return userDto;
        }

        private UserDto SetUserData(User user, UserLogTracker log)
        {
            var userDto = user.MapTo<UserDto>();
            userDto.LogedID = log.LogedID;
            if (userDto.PersonID.IsNotNull())
            {
                var userDetail = GetUserDetails((int)userDto.UserID, (int)userDto.PersonID).Result.MapTo<UserDto>();

                //Find only Prism Roles
                string remainingRoles = GetRoleData(userDetail.Role);
                //Find only Prism Roles


                userDto.FullName = userDetail.FullName;
                userDto.ImagePath = userDetail.ImagePath;
                userDto.EmployeeID = userDetail.EmployeeID;
                userDto.EmployeeCode = userDetail.EmployeeCode ?? "Not yet set";
                userDto.DivisionID = userDetail.DivisionID ?? 0;
                userDto.DepartmentID = userDetail.DepartmentID ?? 0;
                userDto.DivisionName = userDetail.DivisionName ?? "Not yet set";
                userDto.DepartmentName = userDetail.DepartmentName ?? "Not yet set";
                userDto.DesignationName = userDetail.DesignationName ?? "Not yet set";
                userDto.Url = userDetail.Url ?? "Not yet set";
                userDto.DesignationID = userDetail.DesignationID;
                userDto.CompanyShortCode = userDetail.CompanyShortCode ?? "Not yet set";
                userDto.WorkMobile = userDetail.WorkMobile ?? "Not yet set";
                userDto.Email = userDetail.Email ?? "Not yet set";
                userDto.Role = remainingRoles;
                userDto.PhoneNumber = userDetail.PhoneNumber ?? "Not yet set";
                userDto.CompanyName = userDetail.CompanyName ?? "Not yet set";
                userDto.ShortName = userDetail.ShortName ?? "Not yet set";
                userDto.Longitude = userDetail.Longitude ?? "";
                userDto.Latitude = userDetail.Latitude ?? "";
            }

            return userDto;
        }

        private static string GetRoleData(string Role)

        {
            string roles = Role ?? "Not yet set"; // Your original roles string
            string[] roleArray = roles.Split(',');
            string formattedRoles = string.Join(",", roleArray.Select(role => $"{role}"));
            string[] substringsToFilter = { "HQ", "MC", "BP", "Uddoktas", "TMRs", "DSS", "DM", "BDO", "DSO", "DH", "TO", "AM", "RSM", "CH", "TM", "PRO" };
            List<string> filteredRoles = formattedRoles.Split(',')
                               .Where(r => substringsToFilter.Any(s => r.Contains(s)))
                               .ToList();
            // Remove filtered roles from the original roles string
            string remainingRoles = string.Join(",", filteredRoles);
            return remainingRoles;
        }

        public UserDto GetAPPUserData(int userId, int personId)
        {
            var userDto = new UserDto();
            if (userId > 0)
            {
                var userDetail = GetUserDetails(userId, personId).Result.MapTo<UserDto>();

                //Find only Prism Roles
                string roles = userDetail.Role ?? "Not yet set"; // Your original roles string
                string[] roleArray = roles.Split(',');
                string formattedRoles = string.Join(",", roleArray.Select(role => $"{role}"));
                string[] substringsToFilter = { "HQ", "MC", "BP", "Uddoktas", "TMRs", "DSS", "DM", "BDO", "DSO", "DH", "TO", "AM", "RSM", "CH", "TM", "PRO" };
                List<string> filteredRoles = new List<string>();
                if (roleArray.Length > 0)
                {
                    filteredRoles = formattedRoles.Split(',').Where(r => substringsToFilter.Any(s => r.Contains(s))).ToList();
                }
                // Remove filtered roles from the original roles string
                string remainingRoles = string.Join(",", filteredRoles);
                //Find only Prism Roles
                userDto.UserName = userDetail.UserName;
                userDto.UserID = userDetail.UserID;
                userDto.FullName = userDetail.FullName;
                userDto.ImagePath = userDetail.ImagePath;
                userDto.EmployeeID = userDetail.EmployeeID;
                userDto.EmployeeCode = userDetail.EmployeeCode ?? "Not yet set";
                userDto.DivisionID = userDetail.DivisionID ?? 0;
                userDto.DepartmentID = userDetail.DepartmentID ?? 0;
                userDto.DivisionName = userDetail.DivisionName ?? "Not yet set";
                userDto.DepartmentName = userDetail.DepartmentName ?? "Not yet set";
                userDto.DesignationName = userDetail.DesignationName ?? "Not yet set";
                userDto.Url = userDetail.Url ?? "Not yet set";
                userDto.DesignationID = userDetail.DesignationID;
                userDto.CompanyShortCode = userDetail.CompanyShortCode ?? "Not yet set";
                userDto.WorkMobile = userDetail.WorkMobile ?? "Not yet set";
                userDto.Email = userDetail.Email ?? "Not yet set";
                userDto.Role = remainingRoles;
                userDto.PhoneNumber = userDetail.PhoneNumber ?? "Not yet set";
                userDto.CompanyName = userDetail.CompanyName ?? "Not yet set";
                userDto.ShortName = userDetail.ShortName ?? "Not yet set";
            }

            return userDto;
        }
        private UserDto GetUserWhoHasPermission(string userName)
        {
            UserDto userDto = new UserDto();
            userDto.FullName = "Integration User";
            userDto.ImagePath = "";
            userDto.EmployeeID = -1;
            userDto.EmployeeCode = "IU001";
            userDto.DivisionID = 0;
            userDto.DepartmentID = 0;
            userDto.DivisionName = "Integration Division";
            userDto.DepartmentName = "Integration Department";
            userDto.DesignationName = "Integration Designation";
            userDto.Url = "";
            userDto.DesignationID = 0;
            userDto.UserName = userName;
            userDto.UserID = 0;
            userDto.LogedID = 0;
            userDto.PersonID = 0;
            userDto.IsAdmin = true;
            userDto.ApplicationID = 1;
            userDto.CompanyID = "nagad";
            userDto.CompanyName = "Nagad";
            userDto.ShortName = "IUser1";
            userDto.IsForcedLogin = false;
            userDto.IsActive = true;
            userDto.CompanyShortCode = "Nagad";
            userDto.WorkMobile = "";
            userDto.Email = "";
            userDto.Role = "PRISM ADMIN";
            return userDto;
        }

        public async Task<List<UserListDto>> GetUsers()
        {
            var users = await UserRepo.GetAllListAsync();
            return users.MapTo<List<UserListDto>>();
        }

        //public async Task<UserDto> GetUser(int userId)
        //{
        //    var user = await UserRepo.GetAsync(userId);
        //    return user.MapTo<UserDto>();
        //}

        public Task<UserDto> ChangePassword(UserDto userDto)
        {
            char[] passChArray = userDto.Password.ToCharArray();
            char[] currentpassChArray = userDto.CurrentPassword.ToCharArray();
            char[] confirmpassChArray = userDto.ConfirmPassword.ToCharArray();

            if (userDto.Password.Length < 8)
            {
                userDto.PasswordError = "Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol";
                return Task.FromResult(userDto);
            }

            //password
            if (!userDto.Password.Any(char.IsDigit))
            {
                userDto.PasswordError = "Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol";
                return Task.FromResult(userDto);
            }
            if (!userDto.Password.Any(char.IsUpper))
            {
                userDto.PasswordError = "Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol";
                return Task.FromResult(userDto);
            }
            if (!userDto.Password.Any(char.IsLower))
            {
                userDto.PasswordError = "Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol";
                return Task.FromResult(userDto);
            }
            if (!checkPasswordSpecialCharacter(userDto.Password))
            {
                userDto.PasswordError = "Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol";
                return Task.FromResult(userDto);
            }

            //current password
            //if (!userDto.CurrentPassword.Any(char.IsDigit))
            //{
            //    userDto.CurrentPasswordError = "Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol";
            //    return Task.FromResult(userDto);
            //}
            //if (!userDto.CurrentPassword.Any(char.IsUpper))
            //{
            //    userDto.CurrentPasswordError = "Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol";
            //    return Task.FromResult(userDto);
            //}
            //if (!userDto.CurrentPassword.Any(char.IsLower))
            //{
            //    userDto.CurrentPasswordError = "Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol";
            //    return Task.FromResult(userDto);
            //}
            //if (!checkPasswordSpecialCharacter(userDto.CurrentPassword))
            //{
            //    userDto.CurrentPasswordError = "Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol";
            //    return Task.FromResult(userDto);
            //}

            //confirm password
            if (!userDto.ConfirmPassword.Any(char.IsDigit))
            {
                userDto.ConfirmPasswordError = "Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol";
                return Task.FromResult(userDto);
            }
            if (!userDto.ConfirmPassword.Any(char.IsUpper))
            {
                userDto.ConfirmPasswordError = "Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol";
                return Task.FromResult(userDto);
            }
            if (!userDto.ConfirmPassword.Any(char.IsLower))
            {
                userDto.ConfirmPasswordError = "Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol";
                return Task.FromResult(userDto);
            }
            if (!checkPasswordSpecialCharacter(userDto.ConfirmPassword))
            {
                userDto.ConfirmPasswordError = "Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol";
                return Task.FromResult(userDto);
            }

            var userEnt = UserRepo.Entities.SingleOrDefault(x => x.UserID == userDto.UserID).MapTo<User>();
            userEnt.SetModified();
            //var userEnt = userDto.MapTo<User>();
            CreatePasswordHash(userDto.Password, out var passwordHash, out var passwordSalt);
            userEnt.IsForcedLogin = false;
            userEnt.IsLocked = false;
            userEnt.ReasonID = 0;
            userEnt.ChangePasswordDatetime = DateTime.Now;
            userEnt.PasswordHash = passwordHash;
            userEnt.PasswordSalt = passwordSalt;
            userEnt.CompanyID = userDto.CompanyID ?? AppContexts.User.CompanyID;
            userEnt.DefaultApplicationID = userDto.ApplicationID ?? 1;
            UserRepo.Add(userEnt);
            UserRepo.SaveChangesWithAudit();
            userDto.SetUnchanged();
            return Task.FromResult(userDto);
        }

        private bool checkPasswordSpecialCharacter(string passwd)
        {
            string specialCh = @"%!@#$%^&*()?/>.<,:;'\|}]{[_~`+=-" + "\"";
            char[] specialChArray = specialCh.ToCharArray();
            foreach (char ch in specialChArray)
            {
                if (passwd.Contains(ch))
                    return true;
            }
            return false;
        }
        public Task<UserDto> SaveChanges(UserDto userDto)
        {
            //var isExists = UserRepo.Entities.FirstOrDefault(x => x.UserID == userDto.UserID).MapTo<User>();

            if (userDto.Password.IsNotNullOrEmpty())
            {
                char[] passChArray = userDto.Password.ToCharArray();

                if (userDto.Password.Length < 8)
                {
                    userDto.PasswordError = "Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol";
                    return Task.FromResult(userDto);
                }

                if (!userDto.Password.Any(char.IsDigit))
                {
                    userDto.PasswordError = "Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol";
                    return Task.FromResult(userDto);
                }
                if (!userDto.Password.Any(char.IsUpper))
                {
                    userDto.PasswordError = "Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol";
                    return Task.FromResult(userDto);
                }
                if (!userDto.Password.Any(char.IsLower))
                {
                    userDto.PasswordError = "Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol";
                    return Task.FromResult(userDto);
                }
                if (!checkPasswordSpecialCharacter(userDto.Password))
                {
                    userDto.PasswordError = "Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol";
                    return Task.FromResult(userDto);
                }
            }

            var isExistsUserPerson = UserRepo.Entities.FirstOrDefault(x => x.UserID != userDto.UserID && x.PersonID == userDto.PersonID).MapTo<User>();

            if (isExistsUserPerson.IsNotNull())
            {
                userDto.DuplicateUserError = "This person is already used by another user.";
                return Task.FromResult(userDto);
            }

            var isExistsName = UserRepo.Entities.FirstOrDefault(x => x.UserID != userDto.UserID && x.PersonID == userDto.PersonID && x.UserName.ToLower() == userDto.UserName.ToLower()).MapTo<User>();
            if (isExistsName.IsNotNull())
            {
                userDto.DuplicateUserError = "This User already exists.";
                return Task.FromResult(userDto);
            }
            using (var unitOfWork = new UnitOfWork())
            {
                var securityGroupUserChilds = new List<SecurityGroupUserChild>();
                var existUser = UserRepo.Entities.SingleOrDefault(x => x.UserID == userDto.UserID).MapTo<User>();
                var person = PersonRepo.Entities.SingleOrDefault(x => x.PersonID == userDto.PersonID).MapTo<Person>();

                if (existUser.IsNull() || userDto.UserID.IsZero() || userDto.IsAdded)
                {
                    userDto.IsAdmin = false;
                    userDto.SetAdded();
                    SetNewUserID(userDto);
                }
                else
                {
                    userDto.IsAdmin = existUser.IsAdmin;
                    userDto.SetModified();
                }
                var userEnt = userDto.MapTo<User>();

                if (userDto.Password.IsNullOrEmpty() && userDto.IsModified)
                {

                    userEnt.PasswordHash = existUser.PasswordHash;
                    userEnt.PasswordSalt = existUser.PasswordSalt;
                }
                else
                {
                    CreatePasswordHash(userDto.Password, out var passwordHash, out var passwordSalt);
                    userEnt.PasswordHash = passwordHash;
                    userEnt.PasswordSalt = passwordSalt;
                    userEnt.IsForcedLogin = userDto.IsForcedLogin;
                }

                userEnt.CompanyID = userDto.CompanyID ?? AppContexts.User.CompanyID;
                if (userDto.ApplicationID.IsNull() && existUser.IsNull())
                    userEnt.DefaultApplicationID = 1;
                else if (userDto.ApplicationID.IsNull() && existUser.IsNotNull())
                    userEnt.DefaultApplicationID = existUser.DefaultApplicationID;
                else userEnt.DefaultApplicationID = userDto.ApplicationID;

                foreach (var securityGroupChild in userDto.SecurityGroupUserChildList)
                {
                    var groupUserChild = new SecurityGroupUserChild
                    {
                        UserID = userEnt.UserID,
                        SecurityGroupID = securityGroupChild.SecurityGroupID
                    };
                    var existingGroupUserChild = SecurityGroupUserChildRepo.Entities.Where(
                            ruleChildEnt => ruleChildEnt.SecurityGroupID == groupUserChild.SecurityGroupID &&
                            ruleChildEnt.UserID == groupUserChild.UserID
                        ).FirstOrDefault();

                    if (existingGroupUserChild.IsNull())
                    {
                        groupUserChild.SetAdded();
                        SetNewSecurityGroupUserChildID(groupUserChild);
                    }
                    else if (securityGroupChild.IsModified || existingGroupUserChild.IsNotNull())
                    {
                        groupUserChild.SetModified();
                        groupUserChild.RowVersion = existingGroupUserChild.RowVersion;
                        groupUserChild.SecurityGroupUserChildID = existingGroupUserChild.SecurityGroupUserChildID;
                        groupUserChild.CreatedBy = existingGroupUserChild.CreatedBy;
                        groupUserChild.CreatedIP = existingGroupUserChild.CreatedIP;
                        groupUserChild.CreatedDate = existingGroupUserChild.CreatedDate;
                    }
                    securityGroupUserChilds.Add(groupUserChild);
                }
                var SecurityGroupUserChildList = securityGroupUserChilds.MapTo<List<SecurityGroupUserChild>>();

                #region Delete Rules 
                if (existUser.IsNotNull())
                {
                    var list = SecurityGroupUserChildRepo.Entities.Where(
                            ruleChildEnt => ruleChildEnt.UserID == existUser.UserID
                        );
                    foreach (var obj in list)
                    {
                        var exist = SecurityGroupUserChildList.Find(x => x.SecurityGroupUserChildID == obj.SecurityGroupUserChildID);
                        if (exist.IsNull())
                        {
                            obj.SetDeleted();
                            SecurityGroupUserChildList.Add(obj);
                        }
                    }
                }

                #endregion Delete Rules 

                SetAuditFields(SecurityGroupUserChildList);

                UserRepo.Add(userEnt);
                SecurityGroupUserChildRepo.AddRange(SecurityGroupUserChildList);
                unitOfWork.CommitChangesWithAudit();

                userDto.UserID = userEnt.UserID;
                userDto.Email = person.Email;
                userDto.FullName = $@"{person.FirstName} {person.LastName}";
                //userDto.SetUnchanged();
                if (userDto.Password.IsNotNullOrEmpty())
                {
                    var mailConfigurationDto = GetMailConfiguration().Result;
                    var mailGroupSetup = MailGroupSetup((int)Util.MailGroupSetup.UserCreationEmailConfiguration).Result;

                    //string body = WelComeMailBody(userDto);
                    //string subject = UserCreationMailSubject(userDto);

                    string body = WelComeMailBodyFromSetup(userDto, mailGroupSetup.Body);
                    string subject = UserCreationMailSubject(userDto);

                    //SendMail(userDto.UserName, userDto, userDto.Password, subject, body);
                    SendMailFromSetup(mailConfigurationDto, userDto, subject, body);
                }
            }
            return Task.FromResult(userDto);
        }



        private async Task SendMailFromSetup(MailConfigurationDtoCore mailConfigurationDtoCore, UserDto user, string mailSubject, string mailBody)
        {
            EmailDtoCore resetPasswordEmailData = new EmailDtoCore
            {
                FromEmailAddress = mailConfigurationDtoCore.UserName,
                FromEmailAddressDisplayName = mailConfigurationDtoCore.DisplayName,
                ToEmailAddress = new List<string>() { user.Email },
                CCEmailAddress = new List<string>(),
                BCCEmailAddress = new List<string>(),
                EmailDate = DateTime.Now,
                UserName = user.UserName,
                GeneratedPassword = user.Password,
                Subject = mailSubject,
                EmailBody = mailBody,
                IsBodyHtml = true
            };
            client = SetMailServerConfiguration(mailConfigurationDtoCore).Result;
            await SendEmail(resetPasswordEmailData, client);
        }
        public string WelComeMailBodyFromSetup(UserDto userDto, string body)
        {
            body = body.Replace("{{UserFullName}}", userDto.FullName);
            body = body.Replace("{{UserName}}", userDto.UserName);
            body = body.Replace("{{Caption}}", userDto.IsAdded ? "Welcome to Nagad ERP. Your account has been created successfully" : "Your Password has been udpated successfully");
            body = body.Replace("{{PasswordCaption}}", userDto.IsAdded ? "Password" : "New Password");
            body = body.Replace("{{Password}}", userDto.Password);
            return body;
        }


        private static string UserCreationMailSubject(UserDto obj)
        {
            return obj.IsAdded ? @$"It's official. You're now a Nagad ERP User!" : "Trouble Logging in? I am your reset link!";
        }

        private void SetNewUserID(UserDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("User", AppContexts.User.CompanyID);
            obj.UserID = code.MaxNumber;
        }

        private void SetNewUserID(UserProfileDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("User", AppContexts.User.CompanyID);
            obj.UserID = code.MaxNumber;
        }

        private void SetNewSecurityGroupUserChildID(SecurityGroupUserChild obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("SecurityGroupUserChild", AppContexts.User.CompanyID);
            obj.SecurityGroupUserChildID = code.MaxNumber;
        }
        public void Delete(UserDto userDto)
        {
            var userEnt = userDto.MapTo<User>();
            UserRepo.Add(userEnt);
            UserRepo.SaveChanges();
        }

        public async Task SaveUserSettings(string settings, string shortcuts)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var existSettings = UserThemeSettingRepo.Entities.SingleOrDefault(x => x.UserID == AppContexts.User.UserID).MapTo<UserThemeSetting>();
                var settingsObj = new UserThemeSetting
                {
                    UserID = AppContexts.User.UserID,
                    Settings = settings,
                    ShortCuts = shortcuts
                };
                if (existSettings.IsNull())
                {
                    settingsObj.SetAdded();
                }
                else
                {
                    settingsObj.SetModified();
                }

                var settingsEnt = settingsObj.MapTo<UserThemeSetting>();
                SetAuditFields(settingsEnt);
                UserThemeSettingRepo.Add(settingsEnt);
                unitOfWork.CommitChangesWithAudit();
            }
            await Task.CompletedTask;
        }

        public async Task SignOutAsync(int logedId, DateTime signOutDate)
        {
            if (logedId > 0)
            {
                var log = new UserLogTracker
                {
                    LogedID = logedId,
                    UserID = AppContexts.User.UserID,
                    LogInDate = AppContexts.User.LogInDateTime,
                    LogOutDate = signOutDate,
                    IsLive = false,
                    IPAddress = AppContexts.GetIPAddress(),
                    CompanyID = AppContexts.User.CompanyID
                };
                log.SetModified();
                LogRepo.Add(log);
                LogRepo.SaveChanges();
            }

            //LogRepo.Update<UserLogTracker>(new { log.LogedID, log.LogOutDate, log.IsLive });
            await Task.CompletedTask;
        }

        #region Helper Methods
        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password.IsNullOrEmpty()) throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(password));

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password.IsNullOrEmpty()) throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(password));
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", nameof(storedHash));
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", nameof(storedSalt));

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                if (computedHash.Where((t, i) => t != storedHash[i]).Any())
                {
                    return false;
                }
            }

            return true;
        }

        public GridModel GetUserGroups(GridParameter parameters)
        {
            var sql = @"SELECT SGU.*, SGM.SecGroupDescription
	                        FROM SecurityGroupUserChild SGU
                        INNER JOIN SecurityGroupMaster SGM ON SGU.SecurityGroupID = SGM.SecurityGroupID";
            return UserRepo.LoadGridModel(parameters, sql);
        }

        public GridModel GetUserCompanies(GridParameter parameters)
        {
            var sql = @"SELECT ROW_NUMBER() OVER(ORDER BY (SELECT 0)) UserCompanyID, UserCompany.*, Company.CompanyAddress
	                    FROM UserCompany 
                    INNER JOIN Company ON Company.CompanyID = UserCompany.CompanyID";
            return UserRepo.LoadGridModel(parameters, sql);
        }

        //public async Task<List<MenuDto>> GetMenus(int UserId, int ApplicationId)
        public async Task<IEnumerable<Dictionary<string, object>>> GetMenus(int UserId, int ApplicationId, bool IsAdmin)
        {
            ApplicationId = ApplicationId.IsZero() ? 1 : ApplicationId;

            var sql = IsAdmin ?
                    $@"SELECT
                            menu_id AS ""MenuID"",  -- Use double quotes to preserve PascalCase in output
                            parent_id AS ""ParentID"",
                            application_id AS ""ApplicationID"",
                            id AS ""ID"",
                            title AS ""Title"",
                            translate AS ""Translate"",
                            type AS ""Type"",
                            COALESCE(icon, '') AS ""Icon"",
                            url AS ""Url"",
                            COALESCE(badge, '') AS ""Badge"",
                            COALESCE(target, '') AS ""Target"",
                            COALESCE(exact, FALSE) AS ""Exact"",
                            COALESCE(auth, '') AS ""Auth"",
                            parameters AS ""Parameters"",
                            is_visible AS ""IsVisible"",
                            sequence_no AS ""SequenceNo"",
                            CAST(1 AS BOOLEAN) AS ""CanCreate"",
                            CAST(1 AS BOOLEAN) AS ""CanRead"",
                            CAST(1 AS BOOLEAN) AS ""CanUpdate"",
                            CAST(1 AS BOOLEAN) AS ""CanDelete"",
                            CAST(1 AS BOOLEAN) AS ""CanReport""
                        FROM menu
                        WHERE is_visible = TRUE
                        ORDER BY sequence_no ASC;

"
                    :
                    $@"SELECT 
                            *
                        FROM 
                            ViewGetAllMenuPermission
                        WHERE 
                            user_id = '{UserId}' AND menu_id <> 524
                            
                        ORDER BY SequenceNo";

            var menus = UserRepo.GetDataDictCollection(sql);

            return await Task.FromResult(menus);
        }
        public async Task<List<MenuApiPathsView>> MenuApiPaths(int UserId, int ApplicationId)
        {
            ApplicationId = ApplicationId.IsZero() ? 1 : ApplicationId;

            var sql =
                    $@"SELECT 
                            *
                        FROM 
                            ViewGetAllMenuPermissionWithApiPath
                        WHERE 
                            UserID = '{UserId}' --AND Menu.ApplicationID = '{ApplicationId}' ";

            var menus = UserRepo.GetDataModelCollection<MenuApiPathsView>(sql);

            return await Task.FromResult(menus);
        }
        public async Task<string> MenuApiPathsJsonString(int UserId, int ApplicationId)
        {
            ApplicationId = ApplicationId.IsZero() ? 1 : ApplicationId;

            var sql =
                    $@"SELECT 
                            *
                        FROM 
                            ViewGetAllMenuPermissionWithApiPath
                        WHERE 
                            UserID = '{UserId}' --AND Menu.ApplicationID = '{ApplicationId}' ";

            var menus = UserRepo.GetDataDictCollection(sql);

            var jsonStr = Util.ConvertListOfDicToString(menus.ToList());

            return await Task.FromResult(jsonStr);
        }
        public async Task<IEnumerable<Dictionary<string, object>>> GetMenusOld(int UserId, int ApplicationId)
        {
            ApplicationId = ApplicationId.IsZero() ? 1 : ApplicationId;
            var sql = $@"SELECT 
	                        MenuID,
	                        ParentID,
	                        ApplicationID,
	                        ID,
	                        Title,
	                        Translate,
	                        Type,
	                        ISNULL(Icon,'') Icon,
	                        ISNULL(Url,'') Url,
	                        ISNULL(Badge,'') Badge,
	                        ISNULL(Target,'') Target,
	                        ISNULL(Exact,0) Exact,
	                        ISNULL(Auth,'') Auth,
	                        Parameters,
	                        IsVisible,
	                        SequenceNo,
	                        1 CanCreate,
	                        1 CanRead,
	                        1 CanUpdate,
	                        1 CanDelete
                        FROM Menu order by SequenceNo asc";

            var menus = UserRepo.GetDataDictCollection(sql);

            return await Task.FromResult(menus);
        }
        public async Task<string> GetMenusJson(int UserId, int ApplicationId)
        {
            ApplicationId = ApplicationId.IsZero() ? 1 : ApplicationId;
            var sql = $@"SELECT
                            parent.ParentID AS 'Id',
                            parent.Description AS 'Name',
                            parent.RouteName AS 'Age',
                            Childs.MenuID AS 'Id',
                            Childs.RouteName AS 'Email'
                        FROM Menu parent
                        LEFT JOIN Menu Childs ON Childs.ParentID = parent.MenuID AND Childs.MenuID <> Childs.ParentID
                        WHERE parent.MenuID = parent.ParentID
                        FOR JSON AUTO";
            var menus = UserRepo.GetJsonData(sql);

            return await Task.FromResult(menus);
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetUserListDic()
        {
            string sql = $@"SELECT 
	                             U.UserID
	                            ,U.UserName
	                            ,U.DefaultApplicationID
	                            ,U.CompanyID
	                            ,U.IsAdmin
	                            ,U.IsActive
	                            ,U.InActiveDate
	                            ,U.AccessFailedCount
	                            ,U.TwoFactorEnabled
	                            ,U.PhoneNumberConfirmed
	                            ,U.EmailConfirmed
	                            ,U.PersonID
	                            ,Img.ImagePath
	                            ,P.FirstName FullName
	                            ,P.Email
	                            ,P.Mobile
                                ,M.Title AS MenuURL
                            FROM Users U
                            LEFT JOIN Person P ON P.PersonID = U.PersonID
                            LEFT JOIN Menu M ON U.DefaultMenuID=M.MenuID
                            left JOIN (SELECT ImagePath,PersonID FROM PersonImage WHERE IsFavorite=1) Img
								                            ON Img.PersonID = P.PersonID";
            var listDict = UserRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }

        public GridModel GetUserListDicGrid(GridParameter parameters)
        {
            string filter = "";
            string where = "";
            switch (parameters.ApprovalFilterData)
            {
                case "All":
                    filter = "";
                    break;
                case "Active":
                    filter = $@" U.IsActive=1";
                    break;
                case "InActive":
                    filter = $@" U.IsActive=0";
                    break;
                case "Locked":
                    filter = $@" U.IsLocked=1";
                    break;

                default:
                    break;
            }
            where = filter.IsNotNullOrEmpty() ? "AND " : "";
            string sql = $@"SELECT DISTINCT
	                             U.UserID
	                            ,U.UserName
	                            ,U.DefaultApplicationID
	                            ,U.CompanyID
	                            ,U.IsAdmin
	                            ,U.IsActive
	                            ,U.InActiveDate
	                            ,U.AccessFailedCount
	                            ,U.TwoFactorEnabled
	                            ,U.PhoneNumberConfirmed
	                            ,U.EmailConfirmed
	                            ,U.PersonID
	                            ,Img.ImagePath
	                            ,P.FirstName FullName
	                            ,ISNULL(P.Email,'') Email
	                            ,ISNULL(P.Mobile,'') Mobile
                                ,M.Title AS MenuURL
                                ,IsForcedLogin
                            FROM Users U
                            LEFT JOIN Person P ON P.PersonID = U.PersonID
                            LEFT JOIN Menu M ON U.DefaultMenuID=M.MenuID
                            left JOIN (SELECT ImagePath,PersonID FROM PersonImage WHERE IsFavorite=1) Img
							ON Img.PersonID = P.PersonID
							Where U.PersonID <> 0 {where} {filter}
                            ";
            var result = UserRepo.LoadGridModel(parameters, sql);
            return result;
        }

        public async Task<Dictionary<string, object>> GetUserDetails(int UserID, int PersonID)
        {
            string sql = PersonID > 0 ? $@"SELECT 
	                             U.UserID
	                            ,U.UserName	                            
	                            ,U.PersonID
	                            ,Emp.ImagePath
	                            ,P.FirstName FullName
	                            ,P.Email
	                            ,P.Mobile
                                ,Emp.EmployeeID
                                ,Emp.EmployeeCode
								,ISNULL(DepartmentID, 0) DepartmentID
								,ISNULL(DivisionID, 0) DivisionID
                                ,ISNULL(Emp.DepartmentName, '') DepartmentName
								,ISNULL(DivisionName, '') DivisionName
								,ISNULL(DesignationName, '') DesignationName
                                ,M.Title AS MenuURL
                                ,M.Url
                                ,Emp.DesignationID
								,C.CompanyShortCode
                                ,Emp.WorkMobile
                                ,C.CompanyName
                                ,ISNULL(STUFF((
                                        SELECT ',' + gm.SecurityGroupName
                                        FROM SecurityGroupUserChild c
                                        INNER JOIN SecurityGroupMaster gm ON c.SecurityGroupID = gm.SecurityGroupID
                                        WHERE c.UserID = u.UserID
                                        FOR XML PATH('')), 1, 1, ''), '') AS Role,
                                '' Longitude,
                                '' Latitude,
								IsForcedLogin,
                                U.IsLocked,
                                IsActive
                            FROM Users U
                            INNER JOIN Person P ON P.PersonID = U.PersonID
                            LEFT JOIN Menu M ON U.DefaultMenuID=M.MenuID
                            left join {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee Emp ON Emp.PersonID = U.PersonID
							LEFT JOIN Company C ON C.CompanyID = U.CompanyID
							WHERE U.UserID  = {UserID}"
                            :
                            $@"SELECT 
	                             U.UserID
	                            ,U.UserName	                            
	                            ,U.PersonID
	                            ,'' ImagePath
	                            ,UP.UserFullName FullName
	                            ,UP.Email
	                            ,UP.ContactNumber PhoneNumber
                                ,0 EmployeeID
                                ,'' EmployeeCode
								,0 DepartmentID
								,0 DivisionID
                                ,'' DepartmentName
								,'' DivisionName
								,''  DesignationName
                                ,M.Title AS MenuURL
                                ,M.Url
                                ,0 DesignationID
								,C.CompanyShortCode
                                ,UP.ContactNumber WorkMobile
								,SGM.SecurityGroupName Role
                                ,C.CompanyName
                                ,UP.Longitude
                                ,UP.Latitude,
                                0 IsLocked,
                                1 IsActive
                            FROM Users U
                            LEFT JOIN UserProfile UP ON UP.UserID = U.UserID
							LEFT JOIN SecurityGroupMaster SGM ON SGM.SecurityGroupID = UP.PositionID
                            LEFT JOIN Menu M ON U.DefaultMenuID=M.MenuID                            
							LEFT JOIN Company C ON C.CompanyID = U.CompanyID
							WHERE U.UserID = {UserID}";
            return await Task.FromResult(UserRepo.GetData(sql));
        }

        public async Task<UserDto> GetUserLatest(int UserID, int personid)
        {

            UserDto userDto = UserID.IsZero() ? GetUserWhoHasPermission(Util.Integrations.superadmin.ToString()) : GetUserDetails(UserID, personid).Result.MapTo<UserDto>();
            //Find only Prism Roles
            string remainingRoles = GetRoleData(userDto.Role);
            //Find only Prism Roles
            userDto.Role = remainingRoles;
            return await Task.FromResult(userDto);
        }
        public async Task<UserDto> GetUserLatest(string UserName)
        {
            string sql = $@"SELECT 
                                 ISNULL(U.PersonID,0) PersonID
	                             ,U.*
	                            ,Emp.ImagePath
	                            ,P.FirstName FullName
	                            ,P.Email
	                            ,P.Mobile  
                                ,Emp.EmployeeID
                                ,Emp.EmployeeCode
								,DepartmentID
								,DivisionID    
                                ,Emp.DepartmentName
								,DivisionName
								,DesignationName      
                                ,M.Title AS MenuURL     
                                ,M.Url    
                                ,Emp.DesignationID   
								,C.CompanyShortCode
                                ,Emp.WorkMobile 
                                ,'Not yet set' Role,
								IsForcedLogin,
                                U.IsLocked,
                                IsActive
                            FROM Security..Users U
                            LEFT JOIN Security..Person P ON P.PersonID = U.PersonID
                            LEFT JOIN Menu M ON U.DefaultMenuID=M.MenuID
                            left join {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee Emp ON Emp.PersonID = U.PersonID
							LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Company C ON C.CompanyID = U.CompanyID
							WHERE U.UserName  = '{UserName}'";
            return await Task.FromResult(UserRepo.GetModelData<UserDto>(sql));
        }
        public async Task<UserDto> GetUser(int UserID)
        {
            string sql = $@"SELECT 
                                 ISNULL(U.PersonID,0) PersonID
	                             ,U.*
	                            ,Img.ImagePath
	                            ,P.FirstName FullName
	                            --,P.Email
                                ,Emp.EmployeeCode
                                ,Emp.WorkEmail Email
	                            ,P.Mobile      
                                ,M.Title AS MenuURL    
                                ,M.Url
                                ,SV.SystemVariableCode Reason
                            FROM Users U
                            LEFT JOIN Person P ON P.PersonID = U.PersonID
                            LEFT JOIN Menu M ON U.DefaultMenuID=M.MenuID
                            left join {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee Emp ON Emp.PersonID = U.PersonID
                            LEFT JOIN SystemVariable SV ON SV.SystemVariableID = U.ReasonID
                            left JOIN (SELECT ImagePath,PersonID FROM PersonImage WHERE IsFavorite=1) Img
								                            ON Img.PersonID = P.PersonID
							WHERE U.UserID  = {UserID}";
            return await Task.FromResult(UserRepo.GetModelData<UserDto>(sql));
        }

        public async Task<List<ComboModel>> GetSecurityGroupUserChildList(int UserID)
        {
            string sql = $@"SELECT SGM.SecurityGroupName label, SGM.SecurityGroupID value FROM SecurityGroupUserChild SGC
                            LEFT JOIN SecurityGroupMaster SGM ON SGM.SecurityGroupID = SGC.SecurityGroupID 
                            where UserID = {UserID}";
            return await Task.FromResult(SecurityGroupUserChildRepo.GetDataModelCollection<ComboModel>(sql));
        }

        #endregion
        public async Task<UserDto> GetUser(string email, string userName)
        {
            string sql = $@"SELECT 
	                            Email,
	                            U.*
                            FROM 
	                            Security..Person P
	                            INNER JOIN Security..Users U ON P.PersonID = U.PersonID
	                            --LEFT JOIN HRMS..Employee Emp ON Emp.PersonID = P.PersonID
                                WHERE UserName = '{userName}' AND WorkEmail = '{email}'";

            return await Task.FromResult(UserRepo.GetData(sql).MapTo<UserDto>());
        }

        public async Task ResetPasswordAsync(string email, string userName)
        {
            var user = await GetUser(email, userName);
            using (var unitOfWork = new UnitOfWork())
            {
                //UserRepo.Entities.Where(x => x.Email == email && x.UserName == userName).ToList();
                // check user
                if (user.IsNull())
                {
                    throw new ArgumentException("There is no user exists with the provided Email and User Name");
                }

                //var user = user.FirstOrDefault();

                if (!user.IsActive)
                {
                    throw new Exception("The user with this Email and User Name is currently Inactive");
                }

                // generate password 
                string generatedPassword = GenerateRandomPassword();
                CreatePasswordHash(generatedPassword, out byte[] passwordHash, out byte[] passwordSalt);



                // set generated password to user
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                user.SetModified();
                var userAdd = user.MapTo<User>();
                userAdd.DefaultApplicationID = user.ApplicationID;
                SetAuditFields(user);
                UserRepo.Add(userAdd);
                unitOfWork.CommitChangesWithAudit();
                string body = @$"Your account password is reset to <b>{generatedPassword}</b><br/>.Please visit to Nagad ERP to login using this password.";
                string subject = @"Reset Password";
                // send mail to user
                await SendMail(userName, user, generatedPassword, subject, body);

            }

            await Task.CompletedTask;
        }

        private async Task SendMail(string userName, UserDto user, string generatedPassword, string mailSubject, string mailBody)
        {
            ResetPasswordEmailDto resetPasswordEmailData = new ResetPasswordEmailDto
            {
                FromEmailAddress = "nagad.erp.test@gmail.com",
                FromEmailAddressDisplayName = "No Reply",
                ToEmailAddress = new List<string>() { user.Email },
                CCEmailAddress = new List<string>(),
                BCCEmailAddress = new List<string>(),
                EmailDate = DateTime.Now,
                UserName = userName,
                GeneratedPassword = generatedPassword,
                Subject = mailSubject,
                EmailBody = mailBody

            };

            await EmailManager.SendEmail(resetPasswordEmailData);
        }

        private static string GenerateRandomPassword()
        {
            string generatedPassword = "";
            /*string applicableCharacters =
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                "abcdefghijklmnopqrstuvwxyz" +
                "0123456789" +
                "!@#%+-|,.<>:;=";*/

            string applicableCharacters = string.Concat(
                Enumerable.Range('A', 26)
                .Concat(Enumerable.Range('a', 26))
                .Concat(Enumerable.Range('0', 10))
                .Select(i => (char)i)
                .ToArray()
                .Concat("!@#%+-|,.<>:;=".ToCharArray()));

            Random random = new Random();
            int passwordLength = random.Next(4, 10);

            var cryptoRandom = new RNGCryptoServiceProvider();
            var passwordBuilder = new StringBuilder();

            var passBuffer = new byte[passwordLength];

            cryptoRandom.GetBytes(passBuffer);

            foreach (var passPostitionByte in passBuffer)
            {
                passwordBuilder.Append(applicableCharacters[passPostitionByte % applicableCharacters.Length]);
            }

            generatedPassword = passwordBuilder.ToString();

            return generatedPassword;
        }

        public string WelComeMailBody(UserDto userDto)
        {
            return $@"<table style='background:#ffffff;min-width:520px;height:400px' border='0' width='520' cellspacing='0' cellpadding='0' align='center' bgcolor='#ffffff'>
    <tbody>
        <tr style='height:30px'>
            <td style='padding-top:20px; height:30px; width:516px;' align='center'><a rel='noopener'><img style='width: 68px;height: 60px;text-align:center;border:none;' src='https://res.cloudinary.com/dhkcpzubp/image/upload/v1606652838/nagad-pad-logo_aoo2m0.png' alt='Nagad' width='86px' height='50px' border='0'></a></td>
        </tr>
        <tr style='height:18px'>
            <td style='height:18px;width:516px' height='15'>&nbsp;</td>
        </tr>
        <tr>
            <td style='border:2px solid #e8eaed;border-radius:16px;'>
                <table style='width:100%' border='0' width='100%' cellspacing='0' cellpadding='0'>
                    <tbody>
                        <tr>
                            <td align='center'><img style='width:100%;text-align:center;border:none;height: 200px;' src='https://res.cloudinary.com/dhkcpzubp/image/upload/v1606652837/nagad-watermark_fe91ni.png' alt='banner' width='516px' border='0'></td>
                        </tr>
                        <tr>
                            <td>
                                <table border='0' width='100%' cellspacing='0' cellpadding='0' align='center'>
                                    <tbody>
                                        <tr style='height:45px'>
                                            <td dir='ltr' align='center'>Hi {userDto.FullName},</td>
                                        </tr>
                                        <tr style='height:15px'>
                                            <td style='line-height:4px;font-size:4px;height:15px' height='15'>
                                                &nbsp;</td>
                                        </tr>
                                        <tr style='height:45px'>
                                            <td align='center'>{(userDto.IsAdded ? "Welcome to Nagad ERP. Your account has been created successfully" : "Your Password has been udpated successfully")}. 
                                                               Please use below credential to login into the system.
                                        </tr>
                                        <tr style='height:15px'>
                                            <td align='center'>
                                            <a href='http://192.168.12.252:9090/' target='_blank'>Click here to login Nagad ERP</a>
                                            </td>
                                        </tr>                                        
                                    </tbody>
                                </table>

                                <table style='margin-top:15px' cellpadding='15' cellspacing='0' width='100%'>
                                    <tbody><tr>
                                    <td bgcolor='#f1f5f8' style='font-family:Helvetica,Arial,Helvetica,sans-serif;color:#111111;font-size:14px;line-height:18px;padding-left:30px;padding-right:30px;padding:23px 35px;border-radius:10px'>
                                    <table cellpadding='0' cellspacing='0' width='100%'>
                                    <tbody><tr>
                                    <td style='font-family:Helvetica,Arial,Helvetica,sans-serif;color:#111111;font-size:14px;line-height:18px'>
                                    <table cellpadding='0' cellspacing='0'>
                                    <tbody><tr>
                                    <td style='font-family:Helvetica,Arial,Helvetica,sans-serif;color:#111111;font-size:14px;line-height:18px;font:18px/20px Arial,Helvetica,sans-serif,Fira;color:#0e2f5a'>
                                    <table cellpadding='5' cellspacing='2' width='100%'>
                                    <tbody>
                                    <tr>
                                    <td style='font-family:Helvetica,Arial,Helvetica,sans-serif;color:#111111;font-size:14px;line-height:18px'>
                                    <strong>User Name: </strong>
                                    </td>
                                    <td style='font-family:Helvetica,Arial,Helvetica,sans-serif;color:#111111;font-size:14px;line-height:18px'>
                                        {userDto.UserName}
                                    </td>
                                    </tr>
                                    <tr>
                                    <td style='font-family:Helvetica,Arial,Helvetica,sans-serif;color:#111111;font-size:14px;line-height:18px'>
                                    <strong>{(userDto.IsAdded ? "Password" : "New Password")}:</strong>
                                    </td>
                                    <td style='font-family:Helvetica,Arial,Helvetica,sans-serif;color:#111111;font-size:14px;line-height:18px'>
                                        {userDto.Password}
                                    </td>
                                    </tr>
                                    </tbody></table>
                                    </td>
                                    </tr>
                                    </tbody></table>
                                    </td>
                                    </tr>
                                    <tr>
                                    <td style='font-family:Helvetica,Arial,Helvetica,sans-serif;color:#111111;font-size:14px;line-height:18px;font:14px/20px Arial,Helvetica,sans-serif,Fira;color:#0e2f5a'></td>
                                    </tr>
                                    </tbody></table>
                                    </td>
                                    </tr>
                                    </tbody></table>
                            </td>
                        </tr>
                </tbody></table>

</td></tr></tbody></table>";
        }


        //public EmailState ResetPasswordChange(UserDto userDto)
        //{
        //    var allEmailsAndTimes = AllEmailsAndTimes;

        //    string sqlQuery = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE ISNULL(IsLocked,0)=1 AND WorkEmail='{userDto.Email.Trim()}'";

        //    var lockedEmployee = UserRepo.GetData(sql1);
        //    if (lockedEmployee.Count() > 0)
        //    {
        //        // Add a locked account message if email is locked
        //        allEmailsAndTimes[userDto.Email.Trim()] = new EmailState
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new EmailResult
        //            {
        //                Msg = "Ooops! User Account is locked. Please contact HR!",
        //                IsSuccess = false,
        //                RemainingTime = 0
        //            }
        //        };
        //        return allEmailsAndTimes[userDto.Email.Trim()];
        //    }

        //    // Check if the user exists in the database
        //    string sqlExistCheck = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE WorkEmail='{userDto.Email.Trim()}'";
        //    var employee = UserRepo.GetData(sqlExistCheck);
        //    if (employee.Count() <= 0)
        //    {
        //        // Add message for invalid email
        //        allEmailsAndTimes[userDto.Email.Trim()] = new EmailState
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new EmailResult
        //            {
        //                Msg = "Ooops! No User Found With This Email!",
        //                IsSuccess = false,
        //                RemainingTime = 0
        //            }
        //        };
        //        return allEmailsAndTimes[userDto.Email.Trim()];
        //    }
        //    else
        //    {
        //        var userEmail = userDto.Email.Trim();
        //        DateTime lastRequestTime = @"SELECT *, case when IsInactive=0 then 'Active' else 'InActive' end Status FROM SystemVariable ORDER BY SystemVariableID DESC";

        //    }
        //    return { };
        //}
        public class EmailState
        {
            public string Key { get; set; }
            public EmailResult Result { get; set; }
        }

        public class EmailResult
        {
            public string Msg { get; set; }
            public bool IsSuccess { get; set; }
            public int RemainingTime { get; set; }
        }
        public EmailState RequestForForgotPassword(UserDto userDto)
        {
            var allEmailsAndTimes = AllEmailsAndTimes; // Access the in-memory storage for emails and times

            // Check if the provided email exists and is locked
            string sql1 = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE ISNULL(IsLocked,0)=1 AND WorkEmail='{userDto.Email.Trim()}'";
            var lockedEmployee = UserRepo.GetData(sql1);
            if (lockedEmployee.Count() > 0)
            {
                // Add a locked account message if email is locked
                allEmailsAndTimes[userDto.Email.Trim()] = new EmailState
                {
                    Key = userDto.Email.Trim(),
                    Result = new EmailResult
                    {
                        Msg = "Ooops! User Account is locked. Please contact HR!",
                        IsSuccess = false,
                        RemainingTime = 0
                    }
                };
                return allEmailsAndTimes[userDto.Email.Trim()];
            }

            // Check if the user exists in the database
            string sql = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE WorkEmail='{userDto.Email.Trim()}'";
            var employee = UserRepo.GetData(sql);
            if (employee.Count() <= 0)
            {
                // Add message for invalid email
                allEmailsAndTimes[userDto.Email.Trim()] = new EmailState
                {
                    Key = userDto.Email.Trim(),
                    Result = new EmailResult
                    {
                        Msg = "Ooops! No User Found With This Email!",
                        IsSuccess = false,
                        RemainingTime = 0
                    }
                };
                return allEmailsAndTimes[userDto.Email.Trim()];
            }
            else
            {
                var userEmail = userDto.Email.Trim();
                DateTime lastRequestTime;
                // Check the time since the last request
                if (LastPasswordResetRequestTimes.TryGetValue(userEmail, out lastRequestTime))
                {
                    var timeSinceLastRequest = DateTime.Now - lastRequestTime;
                    if (timeSinceLastRequest < TimeSpan.FromMinutes(3))
                    {
                        var remainingTime = TimeSpan.FromMinutes(3) - timeSinceLastRequest;

                        // Check if email exists in the list already
                        if (allEmailsAndTimes.ContainsKey(userEmail))
                        {
                            // Update the existing email's state
                            allEmailsAndTimes[userEmail].Result = new EmailResult
                            {
                                Msg = $"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
                                IsSuccess = false,
                                RemainingTime = (int)remainingTime.TotalSeconds
                            };
                        }
                        else
                        {
                            // Add new email state if not found
                            allEmailsAndTimes[userEmail] = new EmailState
                            {
                                Key = userEmail,
                                Result = new EmailResult
                                {
                                    Msg = $"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
                                    IsSuccess = false,
                                    RemainingTime = (int)remainingTime.TotalSeconds
                                }
                            };
                        }
                        return allEmailsAndTimes[userEmail]; // Return early if within cooldown
                    }
                }

                // Update the last request time for the email
                LastPasswordResetRequestTimes[userEmail] = DateTime.Now;

                // Generate a new token and set the validity time
                var userEnt = UserRepo.Entities.Where(x => x.UserID == (int)employee["UserID"]).FirstOrDefault().MapTo<User>();
                userEnt.ForgotPasswordToken = Guid.NewGuid().ToString();
                userEnt.TokenValidityTime = DateTime.Now.AddHours(1);
                userEnt.SetModified();
                UserRepo.Add(userEnt);
                UserRepo.SaveChangesWithAudit();
                userDto.SetUnchanged();

                // Prepare email data
                var mailData = new List<Dictionary<string, object>>();
                var data = new Dictionary<string, object>
            {
                { "Token", userEnt.ForgotPasswordToken },
                { "EmployeeName", employee["FullName"] }
            };
                mailData.Add(data);

                var toMail = new List<string> { employee["WorkEmail"].ToString() };
                BasicMail((int)Util.MailGroupSetup.ForgotPasswordRequest, toMail, false, null, null, mailData);

                // Check if email exists in the list already
                if (allEmailsAndTimes.ContainsKey(userEmail))
                {
                    // Update the existing email's state
                    allEmailsAndTimes[userEmail].Result = new EmailResult
                    {
                        Msg = "Password recovery request made successfully. Please check your email for reset new password",
                        IsSuccess = true,
                        RemainingTime = 180 // The time remaining for the next reset request (3 minutes)
                    };
                }
                else
                {
                    // Add new email state if not found
                    allEmailsAndTimes[userEmail] = new EmailState
                    {
                        Key = userEmail,
                        Result = new EmailResult
                        {
                            Msg = "Password recovery request made successfully. Please check your email for reset new password",
                            IsSuccess = true,
                            RemainingTime = 180 // The time remaining for the next reset request (3 minutes)
                        }
                    };
                }

                // Return the list of emails with updated states
                return allEmailsAndTimes[userEmail];
            }
        }


        //public EmailState RequestForForgotPassword(UserDto userDto)
        //{
        //    var allEmailsAndTimes = new List<EmailState>();

        //    // Retrieve the allEmailsAndTimes list from session storage if it exists
        //    var sessionData = _httpContextAccessor.HttpContext.Session.GetString("AllEmailsAndTimes");
        //    if (sessionData != null)
        //    {
        //        allEmailsAndTimes = JsonConvert.DeserializeObject<List<EmailState>>(sessionData);
        //    }

        //    // Check if the provided email exists and is locked
        //    string sql1 = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE ISNULL(IsLocked,0)=1 AND WorkEmail='{userDto.Email.Trim()}'";
        //    var lockedEmployee = UserRepo.GetData(sql1);
        //    if (lockedEmployee.Count() > 0)
        //    {
        //        // Add a locked account message if email is locked
        //        allEmailsAndTimes.Add(new EmailState
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new EmailResult
        //            {
        //                Msg = "Ooops! User Account is locked. Please contact HR!",
        //                IsSuccess = false,
        //                RemainingTime = 0
        //            }
        //        });

        //        // Store the updated list back into session
        //        _httpContextAccessor.HttpContext.Session.SetString("AllEmailsAndTimes", JsonConvert.SerializeObject(allEmailsAndTimes));

        //        EmailState filterList = allEmailsAndTimes.Where(x => x.Key == userDto.Email).FirstOrDefault();
        //        return filterList;
        //    }

        //    // Check if the user exists in the database
        //    string sql = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE WorkEmail='{userDto.Email.Trim()}'";
        //    var employee = UserRepo.GetData(sql);
        //    if (employee.Count() <= 0)
        //    {
        //        // Add message for invalid email
        //        allEmailsAndTimes.Add(new EmailState
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new EmailResult
        //            {
        //                Msg = "Ooops! No User Found With This Email!",
        //                IsSuccess = false,
        //                RemainingTime = 0
        //            }
        //        });

        //        // Store the updated list back into session
        //        _httpContextAccessor.HttpContext.Session.SetString("AllEmailsAndTimes", JsonConvert.SerializeObject(allEmailsAndTimes));

        //        EmailState filterList = allEmailsAndTimes.Where(x => x.Key == userDto.Email).FirstOrDefault();
        //        return filterList;
        //    }
        //    else
        //    {
        //        var userEmail = userDto.Email.Trim();
        //        DateTime lastRequestTime;

        //        // Retrieve the last request times from session storage
        //        var lastRequestTimes = _httpContextAccessor.HttpContext.Session.GetString("LastPasswordResetRequestTimes") != null
        //            ? JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(_httpContextAccessor.HttpContext.Session.GetString("LastPasswordResetRequestTimes"))
        //            : new Dictionary<string, DateTime>();

        //        // Check the time since the last request
        //        if (lastRequestTimes.TryGetValue(userEmail, out lastRequestTime))
        //        {
        //            var timeSinceLastRequest = DateTime.Now - lastRequestTime;
        //            if (timeSinceLastRequest < TimeSpan.FromMinutes(3))
        //            {
        //                var remainingTime = TimeSpan.FromMinutes(3) - timeSinceLastRequest;

        //                // Check if email exists in the list already
        //                var existingEmail = allEmailsAndTimes.FirstOrDefault(x => x.Key == userEmail);
        //                if (existingEmail != null)
        //                {
        //                    // Update the existing email's state
        //                    existingEmail.Result = new EmailResult
        //                    {
        //                        Msg = $"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
        //                        IsSuccess = false,
        //                        RemainingTime = (int)remainingTime.TotalSeconds
        //                    };
        //                }
        //                else
        //                {
        //                    // Add new email state if not found
        //                    allEmailsAndTimes.Add(new EmailState
        //                    {
        //                        Key = userEmail,
        //                        Result = new EmailResult
        //                        {
        //                            Msg = $"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
        //                            IsSuccess = false,
        //                            RemainingTime = (int)remainingTime.TotalSeconds
        //                        }
        //                    });
        //                }

        //                // Store the updated list back into session
        //                _httpContextAccessor.HttpContext.Session.SetString("AllEmailsAndTimes", JsonConvert.SerializeObject(allEmailsAndTimes));

        //                EmailState filterListt = allEmailsAndTimes.Where(x => x.Key == userDto.Email).FirstOrDefault();
        //                return filterListt;
        //            }
        //        }

        //        // Update the last request time for the email
        //        lastRequestTimes[userEmail] = DateTime.Now;

        //        // Store the updated request times back into session
        //        _httpContextAccessor.HttpContext.Session.SetString("LastPasswordResetRequestTimes", JsonConvert.SerializeObject(lastRequestTimes));

        //        // Generate a new token and set the validity time
        //        var userEnt = UserRepo.Entities.Where(x => x.UserID == (int)employee["UserID"]).FirstOrDefault().MapTo<User>();
        //        userEnt.ForgotPasswordToken = Guid.NewGuid().ToString();
        //        userEnt.TokenValidityTime = DateTime.Now.AddHours(1);
        //        userEnt.SetModified();
        //        UserRepo.Add(userEnt);
        //        UserRepo.SaveChangesWithAudit();
        //        userDto.SetUnchanged();

        //        // Prepare email data
        //        var mailData = new List<Dictionary<string, object>>();
        //        var data = new Dictionary<string, object>
        //{
        //    { "Token", userEnt.ForgotPasswordToken },
        //    { "EmployeeName", employee["FullName"] }
        //};
        //        mailData.Add(data);

        //        var toMail = new List<string> { employee["WorkEmail"].ToString() };
        //        BasicMail((int)Util.MailGroupSetup.ForgotPasswordRequest, toMail, false, null, null, mailData);

        //        // Check if email exists in the list already
        //        var existingEmailInList = allEmailsAndTimes.FirstOrDefault(x => x.Key == userEmail);
        //        if (existingEmailInList != null)
        //        {
        //            // Update the existing email's state
        //            existingEmailInList.Result = new EmailResult
        //            {
        //                Msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                IsSuccess = true,
        //                RemainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //            };
        //        }
        //        else
        //        {
        //            // Add new email state if not found
        //            allEmailsAndTimes.Add(new EmailState
        //            {
        //                Key = userEmail,
        //                Result = new EmailResult
        //                {
        //                    Msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                    IsSuccess = true,
        //                    RemainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //                }
        //            });
        //        }

        //        // Store the updated list back into session
        //        _httpContextAccessor.HttpContext.Session.SetString("AllEmailsAndTimes", JsonConvert.SerializeObject(allEmailsAndTimes));

        //        EmailState filterList = allEmailsAndTimes.Where(x => x.Key == userDto.Email).FirstOrDefault();
        //        return filterList;
        //    }
        //}


        //public List<object> RequestForForgotPassword(UserDto userDto)
        //{
        //    var allEmailsAndTimes = new List<object>();

        //    // Retrieve the allEmailsAndTimes list from session storage if it exists
        //    var sessionData = _httpContextAccessor.HttpContext.Session.GetString("AllEmailsAndTimes");
        //    if (sessionData != null)
        //    {
        //        allEmailsAndTimes = JsonConvert.DeserializeObject<List<object>>(sessionData);
        //    }

        //    // Check if the provided email exists and is locked
        //    string sql1 = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE ISNULL(IsLocked,0)=1 AND WorkEmail='{userDto.Email.Trim()}'";
        //    var lockedEmployee = UserRepo.GetData(sql1);
        //    if (lockedEmployee.Count() > 0)
        //    {
        //        // Add a locked account message if email is locked
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! User Account is locked. Please contact HR!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });

        //        // Store the updated list back into session
        //        _httpContextAccessor.HttpContext.Session.SetString("AllEmailsAndTimes", JsonConvert.SerializeObject(allEmailsAndTimes));

        //        return allEmailsAndTimes;
        //    }

        //    // Check if the user exists in the database
        //    string sql = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE WorkEmail='{userDto.Email.Trim()}'";
        //    var employee = UserRepo.GetData(sql);
        //    if (employee.Count() <= 0)
        //    {
        //        // Add message for invalid email
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! No User Found With This Email!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });

        //        // Store the updated list back into session
        //        _httpContextAccessor.HttpContext.Session.SetString("AllEmailsAndTimes", JsonConvert.SerializeObject(allEmailsAndTimes));

        //        return allEmailsAndTimes;
        //    }
        //    else
        //    {
        //        var userEmail = userDto.Email.Trim();
        //        DateTime lastRequestTime;

        //        // Retrieve the last request times from session storage
        //        var lastRequestTimes = _httpContextAccessor.HttpContext.Session.GetString("LastPasswordResetRequestTimes") != null
        //            ? JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(_httpContextAccessor.HttpContext.Session.GetString("LastPasswordResetRequestTimes"))
        //            : new Dictionary<string, DateTime>();

        //        // Check the time since the last request
        //        if (lastRequestTimes.TryGetValue(userEmail, out lastRequestTime))
        //        {
        //            var timeSinceLastRequest = DateTime.Now - lastRequestTime;
        //            if (timeSinceLastRequest < TimeSpan.FromMinutes(3))
        //            {
        //                var remainingTime = TimeSpan.FromMinutes(3) - timeSinceLastRequest;

        //                // Check if email exists in the list already
        //                var existingEmail = allEmailsAndTimes.FirstOrDefault(x => ((dynamic)x).Key == userEmail);
        //                if (existingEmail != null)
        //                {
        //                    // Update the existing email's state
        //                    ((dynamic)existingEmail).Result = new
        //                    {
        //                        msg = $"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
        //                        IsSuccess = false,
        //                        remainingTime = (int)remainingTime.TotalSeconds
        //                    };
        //                }
        //                else
        //                {
        //                    // Add new email state if not found
        //                    allEmailsAndTimes.Add(new
        //                    {
        //                        Key = userEmail,
        //                        Result = new
        //                        {
        //                            msg = $"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
        //                            IsSuccess = false,
        //                            remainingTime = (int)remainingTime.TotalSeconds
        //                        }
        //                    });
        //                }

        //                // Store the updated list back into session
        //                _httpContextAccessor.HttpContext.Session.SetString("AllEmailsAndTimes", JsonConvert.SerializeObject(allEmailsAndTimes));
        //                return allEmailsAndTimes; // Return early if within cooldown
        //            }
        //        }

        //        // Update the last request time for the email
        //        lastRequestTimes[userEmail] = DateTime.Now;

        //        // Store the updated request times back into session
        //        _httpContextAccessor.HttpContext.Session.SetString("LastPasswordResetRequestTimes", JsonConvert.SerializeObject(lastRequestTimes));

        //        // Generate a new token and set the validity time
        //        var userEnt = UserRepo.Entities.Where(x => x.UserID == (int)employee["UserID"]).FirstOrDefault().MapTo<User>();
        //        userEnt.ForgotPasswordToken = Guid.NewGuid().ToString();
        //        userEnt.TokenValidityTime = DateTime.Now.AddHours(1);
        //        userEnt.SetModified();
        //        UserRepo.Add(userEnt);
        //        UserRepo.SaveChangesWithAudit();
        //        userDto.SetUnchanged();

        //        // Prepare email data
        //        var mailData = new List<Dictionary<string, object>>();
        //        var data = new Dictionary<string, object>
        //{
        //    { "Token", userEnt.ForgotPasswordToken },
        //    { "EmployeeName", employee["FullName"] }
        //};
        //        mailData.Add(data);

        //        var toMail = new List<string> { employee["WorkEmail"].ToString() };
        //        BasicMail((int)Util.MailGroupSetup.ForgotPasswordRequest, toMail, false, null, null, mailData);

        //        // Check if email exists in the list already
        //        var existingEmailInList = allEmailsAndTimes.FirstOrDefault(x => ((dynamic)x).Key == userEmail);
        //        if (existingEmailInList != null)
        //        {
        //            // Update the existing email's state
        //            ((dynamic)existingEmailInList).Result = new
        //            {
        //                msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                IsSuccess = true,
        //                remainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //            };
        //        }
        //        else
        //        {
        //            // Add new email state if not found
        //            allEmailsAndTimes.Add(new
        //            {
        //                Key = userEmail,
        //                Result = new
        //                {
        //                    msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                    IsSuccess = true,
        //                    remainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //                }
        //            });
        //        }

        //        // Store the updated list back into session
        //        _httpContextAccessor.HttpContext.Session.SetString("AllEmailsAndTimes", JsonConvert.SerializeObject(allEmailsAndTimes));

        //        // Return the list of all emails with updated states
        //        return allEmailsAndTimes;
        //    }
        //}



        //public List<object> RequestForForgotPassword1(UserDto userDto)
        //{
        //    var allEmailsAndTimes = new List<object>();

        //    // Check if the provided email exists and is locked
        //    string sql1 = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE ISNULL(IsLocked,0)=1 AND WorkEmail='{userDto.Email.Trim()}'";
        //    var lockedEmployee = UserRepo.GetData(sql1);
        //    if (lockedEmployee.Count() > 0)
        //    {
        //        // Add a locked account message if email is locked
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! User Account is locked. Please contact HR!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }

        //    // Check if the user exists in the database
        //    string sql = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE WorkEmail='{userDto.Email.Trim()}'";
        //    var employee = UserRepo.GetData(sql);
        //    if (employee.Count() <= 0)
        //    {
        //        // Add message for invalid email
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! No User Found With This Email!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }
        //    else
        //    {
        //        var userEmail = userDto.Email.Trim();
        //        DateTime lastRequestTime;

        //        // Check the time since the last request
        //        if (LastPasswordResetRequestTimes.TryGetValue(userEmail, out lastRequestTime))
        //        {
        //            var timeSinceLastRequest = DateTime.Now - lastRequestTime;
        //            if (timeSinceLastRequest < TimeSpan.FromMinutes(3))
        //            {
        //                var remainingTime = TimeSpan.FromMinutes(3) - timeSinceLastRequest;

        //                // Check if email exists in the list already
        //                var existingEmail = allEmailsAndTimes.FirstOrDefault(x => ((dynamic)x).Key == userEmail);
        //                if (existingEmail != null)
        //                {
        //                    // Update the existing email's state
        //                    ((dynamic)existingEmail).Result = new
        //                    {
        //                        msg = $"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
        //                        IsSuccess = false,
        //                        remainingTime = (int)remainingTime.TotalSeconds
        //                    };
        //                }
        //                else
        //                {
        //                    // Add new email state if not found
        //                    allEmailsAndTimes.Add(new
        //                    {
        //                        Key = userEmail,
        //                        Result = new
        //                        {
        //                            msg = $"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
        //                            IsSuccess = false,
        //                            remainingTime = (int)remainingTime.TotalSeconds
        //                        }
        //                    });
        //                }
        //                return allEmailsAndTimes;
        //            }
        //        }

        //        // Update the last request time for the email
        //        LastPasswordResetRequestTimes[userEmail] = DateTime.Now;

        //        // Generate a new token and set the validity time
        //        var userEnt = UserRepo.Entities.Where(x => x.UserID == (int)employee["UserID"]).FirstOrDefault().MapTo<User>();
        //        userEnt.ForgotPasswordToken = Guid.NewGuid().ToString();
        //        userEnt.TokenValidityTime = DateTime.Now.AddHours(1);
        //        userEnt.SetModified();
        //        UserRepo.Add(userEnt);
        //        UserRepo.SaveChangesWithAudit();
        //        userDto.SetUnchanged();

        //        // Prepare email data
        //        var mailData = new List<Dictionary<string, object>>();
        //        var data = new Dictionary<string, object>
        //{
        //    { "Token", userEnt.ForgotPasswordToken },
        //    { "EmployeeName", employee["FullName"] }
        //};
        //        mailData.Add(data);

        //        var toMail = new List<string> { employee["WorkEmail"].ToString() };
        //        BasicMail((int)Util.MailGroupSetup.ForgotPasswordRequest, toMail, false, null, null, mailData);

        //        // Check if email exists in the list already
        //        var existingEmailInList = allEmailsAndTimes.FirstOrDefault(x => ((dynamic)x).Key == userEmail);
        //        if (existingEmailInList != null)
        //        {
        //            // Update the existing email's state
        //            ((dynamic)existingEmailInList).Result = new
        //            {
        //                msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                IsSuccess = true,
        //                remainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //            };
        //        }
        //        else
        //        {
        //            // Add new email state if not found
        //            allEmailsAndTimes.Add(new
        //            {
        //                Key = userEmail,
        //                Result = new
        //                {
        //                    msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                    IsSuccess = true,
        //                    remainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //                }
        //            });
        //        }

        //        // Return the list of all emails with updated states
        //        return allEmailsAndTimes;
        //    }
        //}




        //public List<object> RequestForForgotPassword(UserDto userDto)
        //{
        //    var allEmailsAndTimes = new List<object>();

        //    // Check if the provided email exists and is locked
        //    string sql1 = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE ISNULL(IsLocked,0)=1 AND WorkEmail='{userDto.Email.Trim()}'";
        //    var lockedEmployee = UserRepo.GetData(sql1);
        //    if (lockedEmployee.Count() > 0)
        //    {
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! User Account is locked. Please contact HR!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }

        //    // Check if the user exists in the database
        //    string sql = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE WorkEmail='{userDto.Email.Trim()}'";
        //    var employee = UserRepo.GetData(sql);
        //    if (employee.Count() <= 0)
        //    {
        //        // Add message for invalid email
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! No User Found With This Email!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }
        //    else
        //    {
        //        var userEmail = userDto.Email.Trim();
        //        DateTime lastRequestTime;

        //        // Check the time since the last request
        //        if (LastPasswordResetRequestTimes.TryGetValue(userEmail, out lastRequestTime))
        //        {
        //            var timeSinceLastRequest = DateTime.Now - lastRequestTime;
        //            if (timeSinceLastRequest < TimeSpan.FromMinutes(3))
        //            {
        //                var remainingTime = TimeSpan.FromMinutes(3) - timeSinceLastRequest;

        //                // Check if email exists in the list already
        //                var existingEmail = allEmailsAndTimes.FirstOrDefault(x => ((dynamic)x).Key == userEmail);
        //                if (existingEmail != null)
        //                {
        //                    // Update the existing email's state
        //                    ((dynamic)existingEmail).Result = new
        //                    {
        //                        msg = $"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
        //                        IsSuccess = false,
        //                        remainingTime = (int)remainingTime.TotalSeconds
        //                    };
        //                }
        //                else
        //                {
        //                    // Add new email state if not found
        //                    allEmailsAndTimes.Add(new
        //                    {
        //                        Key = userEmail,
        //                        Result = new
        //                        {
        //                            msg = $"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
        //                            IsSuccess = false,
        //                            remainingTime = (int)remainingTime.TotalSeconds
        //                        }
        //                    });
        //                }
        //                return allEmailsAndTimes;
        //            }
        //        }

        //        // Update the last request time for the email
        //        LastPasswordResetRequestTimes[userEmail] = DateTime.Now;

        //        // Generate a new token and set the validity time
        //        var userEnt = UserRepo.Entities.Where(x => x.UserID == (int)employee["UserID"]).FirstOrDefault().MapTo<User>();
        //        userEnt.ForgotPasswordToken = Guid.NewGuid().ToString();
        //        userEnt.TokenValidityTime = DateTime.Now.AddHours(1);
        //        userEnt.SetModified();
        //        UserRepo.Add(userEnt);
        //        UserRepo.SaveChangesWithAudit();
        //        userDto.SetUnchanged();

        //        // Prepare email data
        //        var mailData = new List<Dictionary<string, object>>();
        //        var data = new Dictionary<string, object>
        //{
        //    { "Token", userEnt.ForgotPasswordToken },
        //    { "EmployeeName", employee["FullName"] }
        //};
        //        mailData.Add(data);

        //        var toMail = new List<string> { employee["WorkEmail"].ToString() };
        //        BasicMail((int)Util.MailGroupSetup.ForgotPasswordRequest, toMail, false, null, null, mailData);

        //        // Check if email exists in the list already
        //        var existingEmailInList = allEmailsAndTimes.FirstOrDefault(x => ((dynamic)x).Key == userEmail);
        //        if (existingEmailInList != null)
        //        {
        //            // Update the existing email's state
        //            ((dynamic)existingEmailInList).Result = new
        //            {
        //                msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                IsSuccess = true,
        //                remainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //            };
        //        }
        //        else
        //        {
        //            // Add new email state if not found
        //            allEmailsAndTimes.Add(new
        //            {
        //                Key = userEmail,
        //                Result = new
        //                {
        //                    msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                    IsSuccess = true,
        //                    remainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //                }
        //            });
        //        }

        //        // Return the list of all emails with updated states
        //        return allEmailsAndTimes;
        //    }
        //}


        //public List<object> RequestForForgotPassword(UserDto userDto)
        //{
        //    var allEmailsAndTimes = new List<object>();

        //    // Check if the provided email exists and is locked
        //    string sql1 = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE ISNULL(IsLocked,0)=1 AND WorkEmail='{userDto.Email.Trim()}'";
        //    var lockedEmployee = UserRepo.GetData(sql1);
        //    if (lockedEmployee.Count() > 0)
        //    {
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! User Account is locked. Please contact HR!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }

        //    // Check if the user exists in the database
        //    string sql = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE WorkEmail='{userDto.Email.Trim()}'";
        //    var employee = UserRepo.GetData(sql);
        //    if (employee.Count() <= 0)
        //    {
        //        // Add message for invalid email
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! No User Found With This Email!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }
        //    else
        //    {
        //        var userEmail = userDto.Email.Trim();
        //        DateTime lastRequestTime;

        //        // Check if this email's reset time is already in the dictionary
        //        if (LastPasswordResetRequestTimes.TryGetValue(userEmail, out lastRequestTime))
        //        {
        //            var timeSinceLastRequest = DateTime.Now - lastRequestTime;
        //            if (timeSinceLastRequest < TimeSpan.FromMinutes(3))
        //            {
        //                var remainingTime = TimeSpan.FromMinutes(3) - timeSinceLastRequest;

        //                // Check if email exists in the list already
        //                var existingEmail = allEmailsAndTimes.FirstOrDefault(x => ((dynamic)x).Key == userEmail);
        //                if (existingEmail != null)
        //                {
        //                    // Update the existing email's state
        //                    ((dynamic)existingEmail).Result = new
        //                    {
        //                        msg = $"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
        //                        IsSuccess = false,
        //                        remainingTime = (int)remainingTime.TotalSeconds
        //                    };
        //                }
        //                else
        //                {
        //                    // Add new email state if not found
        //                    allEmailsAndTimes.Add(new
        //                    {
        //                        Key = userEmail,
        //                        Result = new
        //                        {
        //                            msg = $"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
        //                            IsSuccess = false,
        //                            remainingTime = (int)remainingTime.TotalSeconds
        //                        }
        //                    });
        //                }
        //                return allEmailsAndTimes;
        //            }
        //        }

        //        // Update the last request time for the email
        //        LastPasswordResetRequestTimes[userEmail] = DateTime.Now;

        //        // Generate a new token and set the validity time
        //        var userEnt = UserRepo.Entities.Where(x => x.UserID == (int)employee["UserID"]).FirstOrDefault().MapTo<User>();
        //        userEnt.ForgotPasswordToken = Guid.NewGuid().ToString();
        //        userEnt.TokenValidityTime = DateTime.Now.AddHours(1);
        //        userEnt.SetModified();
        //        UserRepo.Add(userEnt);
        //        UserRepo.SaveChangesWithAudit();
        //        userDto.SetUnchanged();

        //        // Prepare email data
        //        var mailData = new List<Dictionary<string, object>>();
        //        var data = new Dictionary<string, object>
        //{
        //    { "Token", userEnt.ForgotPasswordToken },
        //    { "EmployeeName", employee["FullName"] }
        //};
        //        mailData.Add(data);

        //        var toMail = new List<string> { employee["WorkEmail"].ToString() };
        //        BasicMail((int)Util.MailGroupSetup.ForgotPasswordRequest, toMail, false, null, null, mailData);

        //        // Check if email exists in the list already
        //        var existingEmailInList = allEmailsAndTimes.FirstOrDefault(x => ((dynamic)x).Key == userEmail);
        //        if (existingEmailInList != null)
        //        {
        //            // Update the existing email's state
        //            ((dynamic)existingEmailInList).Result = new
        //            {
        //                msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                IsSuccess = true,
        //                remainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //            };
        //        }
        //        else
        //        {
        //            // Add new email state if not found
        //            allEmailsAndTimes.Add(new
        //            {
        //                Key = userEmail,
        //                Result = new
        //                {
        //                    msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                    IsSuccess = true,
        //                    remainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //                }
        //            });
        //        }

        //        // Return the list of all emails with updated states
        //        return allEmailsAndTimes;
        //    }
        //}


        //public List<object> RequestForForgotPassword(UserDto userDto)
        //{
        //    var allEmailsAndTimes = new List<object>();

        //    // Check if the provided email exists and is locked
        //    string sql1 = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE ISNULL(IsLocked,0)=1 AND WorkEmail='{userDto.Email.Trim()}'";
        //    var lockedEmployee = UserRepo.GetData(sql1);
        //    if (lockedEmployee.Count() > 0)
        //    {
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! User Account is locked. Please contact HR!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }

        //    // Check if the user exists in the database
        //    string sql = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE WorkEmail='{userDto.Email.Trim()}'";
        //    var employee = UserRepo.GetData(sql);
        //    if (employee.Count() <= 0)
        //    {
        //        // Add message for invalid email
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! No User Found With This Email!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }
        //    else
        //    {
        //        var userEmail = userDto.Email.Trim();
        //        DateTime lastRequestTime;

        //        // Check if this email's reset time is already in the dictionary
        //        if (LastPasswordResetRequestTimes.TryGetValue(userEmail, out lastRequestTime))
        //        {
        //            var timeSinceLastRequest = DateTime.Now - lastRequestTime;
        //            if (timeSinceLastRequest < TimeSpan.FromMinutes(3))
        //            {
        //                var remainingTime = TimeSpan.FromMinutes(3) - timeSinceLastRequest;
        //                allEmailsAndTimes.Add(new
        //                {
        //                    Key = userEmail,
        //                    Result = new
        //                    {
        //                        msg = $"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
        //                        IsSuccess = false,
        //                        remainingTime = (int)remainingTime.TotalSeconds
        //                    }
        //                });
        //                return allEmailsAndTimes;
        //            }
        //        }

        //        // Update the last request time for the email
        //        LastPasswordResetRequestTimes[userEmail] = DateTime.Now;

        //        // Generate a new token and set the validity time
        //        var userEnt = UserRepo.Entities.Where(x => x.UserID == (int)employee["UserID"]).FirstOrDefault().MapTo<User>();
        //        userEnt.ForgotPasswordToken = Guid.NewGuid().ToString();
        //        userEnt.TokenValidityTime = DateTime.Now.AddHours(1);
        //        userEnt.SetModified();
        //        UserRepo.Add(userEnt);
        //        UserRepo.SaveChangesWithAudit();
        //        userDto.SetUnchanged();

        //        // Prepare email data
        //        var mailData = new List<Dictionary<string, object>>();
        //        var data = new Dictionary<string, object>
        //{
        //    { "Token", userEnt.ForgotPasswordToken },
        //    { "EmployeeName", employee["FullName"] }
        //};
        //        mailData.Add(data);

        //        var toMail = new List<string> { employee["WorkEmail"].ToString() };
        //        BasicMail((int)Util.MailGroupSetup.ForgotPasswordRequest, toMail, false, null, null, mailData);

        //        // Check if email exists in the list already
        //        var existingEmailInList = allEmailsAndTimes.FirstOrDefault(x => ((dynamic)x).Key == userEmail);
        //        if (existingEmailInList != null)
        //        {
        //            // Update the existing email's state
        //            ((dynamic)existingEmailInList).Result = new
        //            {
        //                msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                IsSuccess = true,
        //                remainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //            };
        //        }
        //        else
        //        {
        //            // Add new email state if not found
        //            allEmailsAndTimes.Add(new
        //            {
        //                Key = userEmail,
        //                Result = new
        //                {
        //                    msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                    IsSuccess = true,
        //                    remainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //                }
        //            });
        //        }

        //        // Return the list of all emails with updated states
        //        return allEmailsAndTimes;
        //    }
        //}


        //public List<object> RequestForForgotPassword(UserDto userDto)
        //{
        //    var allEmailsAndTimes = new List<object>();

        //    // Check if the provided email exists and is locked
        //    string sql1 = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE ISNULL(IsLocked,0)=1 AND WorkEmail='{userDto.Email.Trim()}'";
        //    var lockedEmployee = UserRepo.GetData(sql1);
        //    if (lockedEmployee.Count() > 0)
        //    {
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! User Account is locked. Please contact HR!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }

        //    // Check if the user exists in the database
        //    string sql = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE WorkEmail='{userDto.Email.Trim()}'";
        //    var employee = UserRepo.GetData(sql);
        //    if (employee.Count() <= 0)
        //    {
        //        // Add message for invalid email
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! No User Found With This Email!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }
        //    else
        //    {
        //        var userEmail = userDto.Email.Trim();
        //        DateTime lastRequestTime;

        //        // Check if this email's reset time is already in the dictionary
        //        if (LastPasswordResetRequestTimes.TryGetValue(userEmail, out lastRequestTime))
        //        {
        //            var timeSinceLastRequest = DateTime.Now - lastRequestTime;
        //            if (timeSinceLastRequest < TimeSpan.FromMinutes(3))
        //            {
        //                var remainingTime = TimeSpan.FromMinutes(3) - timeSinceLastRequest;
        //                allEmailsAndTimes.Add(new
        //                {
        //                    Key = userEmail,
        //                    Result = new
        //                    {
        //                        msg = $"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
        //                        IsSuccess = false,
        //                        remainingTime = (int)remainingTime.TotalSeconds
        //                    }
        //                });
        //                return allEmailsAndTimes;
        //            }
        //        }

        //        // Update the last request time for the email
        //        LastPasswordResetRequestTimes[userEmail] = DateTime.Now;

        //        // Generate a new token and set the validity time
        //        var userEnt = UserRepo.Entities.Where(x => x.UserID == (int)employee["UserID"]).FirstOrDefault().MapTo<User>();
        //        userEnt.ForgotPasswordToken = Guid.NewGuid().ToString();
        //        userEnt.TokenValidityTime = DateTime.Now.AddHours(1);
        //        userEnt.SetModified();
        //        UserRepo.Add(userEnt);
        //        UserRepo.SaveChangesWithAudit();
        //        userDto.SetUnchanged();

        //        // Prepare email data
        //        var mailData = new List<Dictionary<string, object>>();
        //        var data = new Dictionary<string, object>
        //{
        //    { "Token", userEnt.ForgotPasswordToken },
        //    { "EmployeeName", employee["FullName"] }
        //};
        //        mailData.Add(data);

        //        var toMail = new List<string> { employee["WorkEmail"].ToString() };
        //        BasicMail((int)Util.MailGroupSetup.ForgotPasswordRequest, toMail, false, null, null, mailData);

        //        // Check if email exists in the list already
        //        var existingEmailInList = allEmailsAndTimes.FirstOrDefault(x => ((dynamic)x).Key == userEmail);
        //        if (existingEmailInList != null)
        //        {
        //            // Update the existing email's state
        //            ((dynamic)existingEmailInList).Result = new
        //            {
        //                msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                IsSuccess = true,
        //                remainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //            };
        //        }
        //        else
        //        {
        //            // Add new email state if not found
        //            allEmailsAndTimes.Add(new
        //            {
        //                Key = userEmail,
        //                Result = new
        //                {
        //                    msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                    IsSuccess = true,
        //                    remainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //                }
        //            });
        //        }

        //        // Return the list of all emails with updated states
        //        return allEmailsAndTimes;
        //    }
        //}


        //public List<object> RequestForForgotPassword(UserDto userDto)
        //{
        //    var allEmailsAndTimes = new List<object>();

        //    // Check if the provided email exists and is locked
        //    string sql1 = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE ISNULL(IsLocked,0)=1 AND WorkEmail='{userDto.Email.Trim()}'";
        //    var lockedEmployee = UserRepo.GetData(sql1);
        //    if (lockedEmployee.Count() > 0)
        //    {
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! User Account is locked. Please contact HR!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }

        //    // Check if the user exists in the database
        //    string sql = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE WorkEmail='{userDto.Email.Trim()}'";
        //    var employee = UserRepo.GetData(sql);
        //    if (employee.Count() <= 0)
        //    {
        //        // Add message for invalid email
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! No User Found With This Email!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }
        //    else
        //    {
        //        var userEmail = userDto.Email.Trim();
        //        DateTime lastRequestTime;

        //        // Check if this email's reset time is already in the dictionary
        //        if (LastPasswordResetRequestTimes.TryGetValue(userEmail, out lastRequestTime))
        //        {
        //            var timeSinceLastRequest = DateTime.Now - lastRequestTime;
        //            if (timeSinceLastRequest < TimeSpan.FromMinutes(3))
        //            {
        //                var remainingTime = TimeSpan.FromMinutes(3) - timeSinceLastRequest;
        //                allEmailsAndTimes.Add(new
        //                {
        //                    Key = userEmail,
        //                    Result = new
        //                    {
        //                        msg = $"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
        //                        IsSuccess = false,
        //                        remainingTime = (int)remainingTime.TotalSeconds
        //                    }
        //                });
        //                return allEmailsAndTimes;
        //            }
        //        }

        //        // Update the last request time for the email
        //        LastPasswordResetRequestTimes[userEmail] = DateTime.Now;

        //        // Generate a new token and set the validity time
        //        var userEnt = UserRepo.Entities.Where(x => x.UserID == (int)employee["UserID"]).FirstOrDefault().MapTo<User>();
        //        userEnt.ForgotPasswordToken = Guid.NewGuid().ToString();
        //        userEnt.TokenValidityTime = DateTime.Now.AddHours(1);
        //        userEnt.SetModified();
        //        UserRepo.Add(userEnt);
        //        UserRepo.SaveChangesWithAudit();
        //        userDto.SetUnchanged();

        //        // Prepare email data
        //        var mailData = new List<Dictionary<string, object>>();
        //        var data = new Dictionary<string, object>
        //{
        //    { "Token", userEnt.ForgotPasswordToken },
        //    { "EmployeeName", employee["FullName"] }
        //};
        //        mailData.Add(data);

        //        var toMail = new List<string> { employee["WorkEmail"].ToString() };
        //        BasicMail((int)Util.MailGroupSetup.ForgotPasswordRequest, toMail, false, null, null, mailData);

        //        // Check if email exists in the list already
        //        var existingEmailInList = allEmailsAndTimes.FirstOrDefault(x => ((dynamic)x).Key == userEmail);
        //        if (existingEmailInList != null)
        //        {
        //            // Update the existing email's state
        //            ((dynamic)existingEmailInList).Result = new
        //            {
        //                msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                IsSuccess = true,
        //                remainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //            };
        //        }
        //        else
        //        {
        //            // Add new email state if not found
        //            allEmailsAndTimes.Add(new
        //            {
        //                Key = userEmail,
        //                Result = new
        //                {
        //                    msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                    IsSuccess = true,
        //                    remainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //                }
        //            });
        //        }

        //        // Return the list of all emails with updated states
        //        return allEmailsAndTimes;
        //    }
        //}


        //public List<object> RequestForForgotPassword(UserDto userDto)
        //{
        //    var allEmailsAndTimes = new List<object>();

        //    // Check if the provided email exists and is locked
        //    string sql1 = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE ISNULL(IsLocked,0)=1 AND WorkEmail='{userDto.Email.Trim()}'";
        //    var lockedEmployee = UserRepo.GetData(sql1);
        //    if (lockedEmployee.Count() > 0)
        //    {
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! User Account is locked. Please contact HR!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }

        //    // Check if the user exists in the database
        //    string sql = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE WorkEmail='{userDto.Email.Trim()}'";
        //    var employee = UserRepo.GetData(sql);
        //    if (employee.Count() <= 0)
        //    {
        //        // Add message for invalid email
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! No User Found With This Email!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }
        //    else
        //    {
        //        var userEmail = userDto.Email.Trim();
        //        DateTime lastRequestTime;

        //        // Check if this email's reset time is already in the dictionary
        //        if (LastPasswordResetRequestTimes.TryGetValue(userEmail, out lastRequestTime))
        //        {
        //            var timeSinceLastRequest = DateTime.Now - lastRequestTime;
        //            if (timeSinceLastRequest < TimeSpan.FromMinutes(3))
        //            {
        //                var remainingTime = TimeSpan.FromMinutes(3) - timeSinceLastRequest;
        //                allEmailsAndTimes.Add(new
        //                {
        //                    Key = userEmail,
        //                    Result = new
        //                    {
        //                        msg = $"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
        //                        IsSuccess = false,
        //                        remainingTime = (int)remainingTime.TotalSeconds
        //                    }
        //                });
        //                return allEmailsAndTimes;
        //            }
        //        }

        //        // Update the last request time for the email
        //        LastPasswordResetRequestTimes[userEmail] = DateTime.Now;

        //        // Generate a new token and set the validity time
        //        var userEnt = UserRepo.Entities.Where(x => x.UserID == (int)employee["UserID"]).FirstOrDefault().MapTo<User>();
        //        userEnt.ForgotPasswordToken = Guid.NewGuid().ToString();
        //        userEnt.TokenValidityTime = DateTime.Now.AddHours(1);
        //        userEnt.SetModified();
        //        UserRepo.Add(userEnt);
        //        UserRepo.SaveChangesWithAudit();
        //        userDto.SetUnchanged();

        //        // Prepare email data
        //        var mailData = new List<Dictionary<string, object>>();
        //        var data = new Dictionary<string, object>
        //{
        //    { "Token", userEnt.ForgotPasswordToken },
        //    { "EmployeeName", employee["FullName"] }
        //};
        //        mailData.Add(data);

        //        var toMail = new List<string> { employee["WorkEmail"].ToString() };
        //        BasicMail((int)Util.MailGroupSetup.ForgotPasswordRequest, toMail, false, null, null, mailData);

        //        // Check if email exists in the list already
        //        var existingEmailInList = allEmailsAndTimes.FirstOrDefault(x => ((dynamic)x).Key == userEmail);
        //        if (existingEmailInList != null)
        //        {
        //            // Update the existing email's state
        //            ((dynamic)existingEmailInList).Result = new
        //            {
        //                msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                IsSuccess = true,
        //                remainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //            };
        //        }
        //        else
        //        {
        //            // Add new email state if not found
        //            allEmailsAndTimes.Add(new
        //            {
        //                Key = userEmail,
        //                Result = new
        //                {
        //                    msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                    IsSuccess = true,
        //                    remainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //                }
        //            });
        //        }

        //        // Return the list of all emails with updated states
        //        return allEmailsAndTimes;
        //    }
        //}


        //public List<object> RequestForForgotPassword(UserDto userDto)
        //{
        //    var allEmailsAndTimes = new List<object>();

        //    string sql1 = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE ISNULL(IsLocked,0)=1 AND WorkEmail='{userDto.Email.Trim()}'";
        //    var lockedEmployee = UserRepo.GetData(sql1);
        //    if (lockedEmployee.Count() > 0)
        //    {
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! User Account is locked. Please contact HR!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }

        //    string sql = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE WorkEmail='{userDto.Email.Trim()}'";
        //    var employee = UserRepo.GetData(sql);
        //    if (employee.Count() <= 0)
        //    {
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! No User Found With This Email!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }
        //    else
        //    {
        //        var userEmail = userDto.Email.Trim();
        //        DateTime lastRequestTime;

        //        // Check the time since the last request
        //        if (LastPasswordResetRequestTimes.TryGetValue(userEmail, out lastRequestTime))
        //        {
        //            var timeSinceLastRequest = DateTime.Now - lastRequestTime;
        //            if (timeSinceLastRequest < TimeSpan.FromMinutes(3))
        //            {
        //                var remainingTime = TimeSpan.FromMinutes(3) - timeSinceLastRequest;
        //                allEmailsAndTimes.Add(new
        //                {
        //                    Key = userEmail,
        //                    Result = new
        //                    {
        //                        msg = $"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
        //                        IsSuccess = false,
        //                        remainingTime = (int)remainingTime.TotalSeconds
        //                    }
        //                });
        //                return allEmailsAndTimes;
        //            }
        //        }

        //        // Update the last request time
        //        LastPasswordResetRequestTimes[userEmail] = DateTime.Now;

        //        // Generate a new token and set the validity time
        //        var userEnt = UserRepo.Entities.Where(x => x.UserID == (int)employee["UserID"]).FirstOrDefault().MapTo<User>();
        //        userEnt.ForgotPasswordToken = Guid.NewGuid().ToString();
        //        userEnt.TokenValidityTime = DateTime.Now.AddHours(1);
        //        userEnt.SetModified();
        //        UserRepo.Add(userEnt);
        //        UserRepo.SaveChangesWithAudit();
        //        userDto.SetUnchanged();

        //        // Prepare email data
        //        var mailData = new List<Dictionary<string, object>>();
        //        var data = new Dictionary<string, object>
        //{
        //    { "Token", userEnt.ForgotPasswordToken },
        //    { "EmployeeName", employee["FullName"] }
        //};
        //        mailData.Add(data);

        //        var toMail = new List<string> { employee["WorkEmail"].ToString() };
        //        BasicMail((int)Util.MailGroupSetup.ForgotPasswordRequest, toMail, false, null, null, mailData);

        //        // Add this email's state to the list
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userEmail,
        //            Result = new
        //            {
        //                msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                IsSuccess = true,
        //                remainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //            }
        //        });

        //        return allEmailsAndTimes;
        //    }
        //}


        //public List<object> RequestForForgotPassword(UserDto userDto)
        //{
        //    var allEmailsAndTimes = new List<object>();

        //    string sql1 = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE ISNULL(IsLocked,0)=1 AND WorkEmail='{userDto.Email.Trim()}'";
        //    var lockedEmployee = UserRepo.GetData(sql1);
        //    if (lockedEmployee.Count() > 0)
        //    {
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! User Account is locked. Please contact HR!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }

        //    string sql = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE WorkEmail='{userDto.Email.Trim()}'";
        //    var employee = UserRepo.GetData(sql);
        //    if (employee.Count() <= 0)
        //    {
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! No User Found With This Email!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }
        //    else
        //    {
        //        var userEmail = userDto.Email.Trim();
        //        DateTime lastRequestTime;

        //        // Check the time since the last request
        //        if (LastPasswordResetRequestTimes.TryGetValue(userEmail, out lastRequestTime))
        //        {
        //            var timeSinceLastRequest = DateTime.Now - lastRequestTime;
        //            if (timeSinceLastRequest < TimeSpan.FromMinutes(3))
        //            {
        //                var remainingTime = TimeSpan.FromMinutes(3) - timeSinceLastRequest;
        //                allEmailsAndTimes.Add(new
        //                {
        //                    Key = userEmail,
        //                    Result = new
        //                    {
        //                        msg = $"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
        //                        IsSuccess = false,
        //                        remainingTime = (int)remainingTime.TotalSeconds
        //                    }
        //                });
        //                return allEmailsAndTimes;
        //            }
        //        }

        //        // Update the last request time
        //        LastPasswordResetRequestTimes[userEmail] = DateTime.Now;

        //        // Generate a new token and set the validity time
        //        var userEnt = UserRepo.Entities.Where(x => x.UserID == (int)employee["UserID"]).FirstOrDefault().MapTo<User>();
        //        userEnt.ForgotPasswordToken = Guid.NewGuid().ToString();
        //        userEnt.TokenValidityTime = DateTime.Now.AddHours(1);
        //        userEnt.SetModified();
        //        UserRepo.Add(userEnt);
        //        UserRepo.SaveChangesWithAudit();
        //        userDto.SetUnchanged();

        //        // Prepare email data
        //        var mailData = new List<Dictionary<string, object>>();
        //        var data = new Dictionary<string, object>
        //{
        //    { "Token", userEnt.ForgotPasswordToken },
        //    { "EmployeeName", employee["FullName"] }
        //};
        //        mailData.Add(data);

        //        var toMail = new List<string> { employee["WorkEmail"].ToString() };
        //        BasicMail((int)Util.MailGroupSetup.ForgotPasswordRequest, toMail, false, null, null, mailData);

        //        // Add this email's state to the list
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userEmail,
        //            Result = new
        //            {
        //                msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                IsSuccess = true,
        //                remainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //            }
        //        });

        //        return allEmailsAndTimes;
        //    }
        //}


        //public List<object> RequestForForgotPassword(UserDto userDto)
        //{
        //    var allEmailsAndTimes = new List<object>();

        //    string sql1 = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE ISNULL(IsLocked,0)=1 AND WorkEmail='{userDto.Email.Trim()}'";
        //    var lockedEmployee = UserRepo.GetData(sql1);
        //    if (lockedEmployee.Count() > 0)
        //    {
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! User Account is locked. Please contact HR!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }

        //    string sql = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE WorkEmail='{userDto.Email.Trim()}'";
        //    var employee = UserRepo.GetData(sql);
        //    if (employee.Count() <= 0)
        //    {
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! No User Found With This Email!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }
        //    else
        //    {
        //        var userEmail = userDto.Email.Trim();
        //        DateTime lastRequestTime;

        //        // Check the time since the last request
        //        if (LastPasswordResetRequestTimes.TryGetValue(userEmail, out lastRequestTime))
        //        {
        //            var timeSinceLastRequest = DateTime.Now - lastRequestTime;
        //            if (timeSinceLastRequest < TimeSpan.FromMinutes(3))
        //            {
        //                var remainingTime = TimeSpan.FromMinutes(3) - timeSinceLastRequest;
        //                allEmailsAndTimes.Add(new
        //                {
        //                    Key = userEmail,
        //                    Result = new
        //                    {
        //                        msg = $"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
        //                        IsSuccess = false,
        //                        remainingTime = (int)remainingTime.TotalSeconds
        //                    }
        //                });
        //                return allEmailsAndTimes;
        //            }
        //        }

        //        // Update the last request time
        //        LastPasswordResetRequestTimes[userEmail] = DateTime.Now;

        //        // Generate a new token and set the validity time
        //        var userEnt = UserRepo.Entities.Where(x => x.UserID == (int)employee["UserID"]).FirstOrDefault().MapTo<User>();
        //        userEnt.ForgotPasswordToken = Guid.NewGuid().ToString();
        //        userEnt.TokenValidityTime = DateTime.Now.AddHours(1);
        //        userEnt.SetModified();
        //        UserRepo.Add(userEnt);
        //        UserRepo.SaveChangesWithAudit();
        //        userDto.SetUnchanged();

        //        // Prepare email data
        //        var mailData = new List<Dictionary<string, object>>();
        //        var data = new Dictionary<string, object>
        //{
        //    { "Token", userEnt.ForgotPasswordToken },
        //    { "EmployeeName", employee["FullName"] }
        //};
        //        mailData.Add(data);

        //        var toMail = new List<string> { employee["WorkEmail"].ToString() };
        //        BasicMail((int)Util.MailGroupSetup.ForgotPasswordRequest, toMail, false, null, null, mailData);

        //        // Add this email's state to the list
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userEmail,
        //            Result = new
        //            {
        //                msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                IsSuccess = true,
        //                remainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //            }
        //        });

        //        return allEmailsAndTimes;
        //    }
        //}


        //public List<object> RequestForForgotPassword(UserDto userDto)
        //{
        //    var allEmailsAndTimes = new List<object>();

        //    string sql1 = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE ISNULL(IsLocked,0)=1 AND WorkEmail='{userDto.Email.Trim()}'";
        //    var lockedEmployee = UserRepo.GetData(sql1);
        //    if (lockedEmployee.Count() > 0)
        //    {
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! User Account is locked. Please contact HR!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }

        //    string sql = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE WorkEmail='{userDto.Email.Trim()}'";
        //    var employee = UserRepo.GetData(sql);
        //    if (employee.Count() <= 0)
        //    {
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! No User Found With This Email!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }
        //    else
        //    {
        //        var userEmail = userDto.Email.Trim();
        //        DateTime lastRequestTime;

        //        // Check the time since the last request
        //        if (LastPasswordResetRequestTimes.TryGetValue(userEmail, out lastRequestTime))
        //        {
        //            var timeSinceLastRequest = DateTime.Now - lastRequestTime;
        //            if (timeSinceLastRequest < TimeSpan.FromMinutes(3))
        //            {
        //                var remainingTime = TimeSpan.FromMinutes(3) - timeSinceLastRequest;
        //                allEmailsAndTimes.Add(new
        //                {
        //                    Key = userEmail,
        //                    Result = new
        //                    {
        //                        msg = $"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
        //                        IsSuccess = false,
        //                        remainingTime = (int)remainingTime.TotalSeconds
        //                    }
        //                });
        //                return allEmailsAndTimes;
        //            }
        //        }

        //        // Update the last request time
        //        LastPasswordResetRequestTimes[userEmail] = DateTime.Now;

        //        // Generate a new token and set the validity time
        //        var userEnt = UserRepo.Entities.Where(x => x.UserID == (int)employee["UserID"]).FirstOrDefault().MapTo<User>();
        //        userEnt.ForgotPasswordToken = Guid.NewGuid().ToString();
        //        userEnt.TokenValidityTime = DateTime.Now.AddHours(1);
        //        userEnt.SetModified();
        //        UserRepo.Add(userEnt);
        //        UserRepo.SaveChangesWithAudit();
        //        userDto.SetUnchanged();

        //        // Prepare email data
        //        var mailData = new List<Dictionary<string, object>>();
        //        var data = new Dictionary<string, object>
        //{
        //    { "Token", userEnt.ForgotPasswordToken },
        //    { "EmployeeName", employee["FullName"] }
        //};
        //        mailData.Add(data);

        //        var toMail = new List<string> { employee["WorkEmail"].ToString() };
        //        BasicMail((int)Util.MailGroupSetup.ForgotPasswordRequest, toMail, false, null, null, mailData);

        //        // Add this email's state to the list
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userEmail,
        //            Result = new
        //            {
        //                msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                IsSuccess = true,
        //                remainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //            }
        //        });

        //        return allEmailsAndTimes;
        //    }
        //}


        //public List<object> RequestForForgotPassword(UserDto userDto)
        //{
        //    var allEmailsAndTimes = new List<object>();

        //    string sql1 = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE ISNULL(IsLocked,0)=1 AND WorkEmail='{userDto.Email.Trim()}'";
        //    var lockedEmployee = UserRepo.GetData(sql1);
        //    if (lockedEmployee.Count() > 0)
        //    {
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! User Account is locked. Please contact HR!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }

        //    string sql = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE WorkEmail='{userDto.Email.Trim()}'";
        //    var employee = UserRepo.GetData(sql);
        //    if (employee.Count() <= 0)
        //    {
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! No User Found With This Email!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }
        //    else
        //    {
        //        var userEmail = userDto.Email.Trim();
        //        DateTime lastRequestTime;

        //        // Check the time since the last request
        //        if (LastPasswordResetRequestTimes.TryGetValue(userEmail, out lastRequestTime))
        //        {
        //            var timeSinceLastRequest = DateTime.Now - lastRequestTime;
        //            if (timeSinceLastRequest < TimeSpan.FromMinutes(3))
        //            {
        //                var remainingTime = TimeSpan.FromMinutes(3) - timeSinceLastRequest;
        //                allEmailsAndTimes.Add(new
        //                {
        //                    Key = userEmail,
        //                    Result = new
        //                    {
        //                        msg = $"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
        //                        IsSuccess = false,
        //                        remainingTime = (int)remainingTime.TotalSeconds
        //                    }
        //                });
        //                return allEmailsAndTimes;
        //            }
        //        }

        //        // Update the last request time
        //        LastPasswordResetRequestTimes[userEmail] = DateTime.Now;

        //        // Generate a new token and set the validity time
        //        var userEnt = UserRepo.Entities.Where(x => x.UserID == (int)employee["UserID"]).FirstOrDefault().MapTo<User>();
        //        userEnt.ForgotPasswordToken = Guid.NewGuid().ToString();
        //        userEnt.TokenValidityTime = DateTime.Now.AddHours(1);
        //        userEnt.SetModified();
        //        UserRepo.Add(userEnt);
        //        UserRepo.SaveChangesWithAudit();
        //        userDto.SetUnchanged();

        //        // Prepare email data
        //        var mailData = new List<Dictionary<string, object>>();
        //        var data = new Dictionary<string, object>
        //{
        //    { "Token", userEnt.ForgotPasswordToken },
        //    { "EmployeeName", employee["FullName"] }
        //};
        //        mailData.Add(data);

        //        var toMail = new List<string> { employee["WorkEmail"].ToString() };
        //        BasicMail((int)Util.MailGroupSetup.ForgotPasswordRequest, toMail, false, null, null, mailData);

        //        // Add this email's state to the list
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userEmail,
        //            Result = new
        //            {
        //                msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                IsSuccess = true,
        //                remainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //            }
        //        });

        //        return allEmailsAndTimes;
        //    }
        //}


        //public List<object> RequestForForgotPassword(UserDto userDto)
        //{
        //    var allEmailsAndTimes = new List<object>();

        //    string sql1 = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE ISNULL(IsLocked,0)=1 AND WorkEmail='{userDto.Email.Trim()}'";
        //    var lockedEmployee = UserRepo.GetData(sql1);
        //    if (lockedEmployee.Count() > 0)
        //    {
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! User Account is locked. Please contact HR!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }

        //    string sql = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE WorkEmail='{userDto.Email.Trim()}'";
        //    var employee = UserRepo.GetData(sql);
        //    if (employee.Count() <= 0)
        //    {
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! No User Found With This Email!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }
        //    else
        //    {
        //        var userEmail = userDto.Email.Trim();
        //        DateTime lastRequestTime;

        //        // Check the time since the last request
        //        if (LastPasswordResetRequestTimes.TryGetValue(userEmail, out lastRequestTime))
        //        {
        //            var timeSinceLastRequest = DateTime.Now - lastRequestTime;
        //            if (timeSinceLastRequest < TimeSpan.FromMinutes(3))
        //            {
        //                var remainingTime = TimeSpan.FromMinutes(3) - timeSinceLastRequest;
        //                allEmailsAndTimes.Add(new
        //                {
        //                    Key = userEmail,
        //                    Result = new
        //                    {
        //                        msg = $"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
        //                        IsSuccess = false,
        //                        remainingTime = (int)remainingTime.TotalSeconds
        //                    }
        //                });
        //                return allEmailsAndTimes;
        //            }
        //        }

        //        // Update the last request time
        //        LastPasswordResetRequestTimes[userEmail] = DateTime.Now;

        //        // Generate a new token and set the validity time
        //        var userEnt = UserRepo.Entities.Where(x => x.UserID == (int)employee["UserID"]).FirstOrDefault().MapTo<User>();
        //        userEnt.ForgotPasswordToken = Guid.NewGuid().ToString();
        //        userEnt.TokenValidityTime = DateTime.Now.AddHours(1);
        //        userEnt.SetModified();
        //        UserRepo.Add(userEnt);
        //        UserRepo.SaveChangesWithAudit();
        //        userDto.SetUnchanged();

        //        // Prepare email data
        //        var mailData = new List<Dictionary<string, object>>();
        //        var data = new Dictionary<string, object>
        //{
        //    { "Token", userEnt.ForgotPasswordToken },
        //    { "EmployeeName", employee["FullName"] }
        //};
        //        mailData.Add(data);

        //        var toMail = new List<string> { employee["WorkEmail"].ToString() };
        //        BasicMail((int)Util.MailGroupSetup.ForgotPasswordRequest, toMail, false, null, null, mailData);

        //        // Add this email's state to the list
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userEmail,
        //            Result = new
        //            {
        //                msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                IsSuccess = true,
        //                remainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //            }
        //        });

        //        return allEmailsAndTimes;
        //    }
        //}


        //public List<object> RequestForForgotPassword(UserDto userDto)
        //{
        //    var allEmailsAndTimes = new List<object>();

        //    string sql1 = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE ISNULL(IsLocked,0)=1 AND WorkEmail='{userDto.Email.Trim()}'";
        //    var lockedEmployee = UserRepo.GetData(sql1);
        //    if (lockedEmployee.Count() > 0)
        //    {
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! User Account is locked. Please contact HR!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }

        //    string sql = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE WorkEmail='{userDto.Email.Trim()}'";
        //    var employee = UserRepo.GetData(sql);
        //    if (employee.Count() <= 0)
        //    {
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! No User Found With This Email!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }
        //    else
        //    {
        //        var userEmail = userDto.Email.Trim();
        //        DateTime lastRequestTime;

        //        // Check the time since the last request
        //        if (LastPasswordResetRequestTimes.TryGetValue(userEmail, out lastRequestTime))
        //        {
        //            var timeSinceLastRequest = DateTime.Now - lastRequestTime;
        //            if (timeSinceLastRequest < TimeSpan.FromMinutes(3))
        //            {
        //                var remainingTime = TimeSpan.FromMinutes(3) - timeSinceLastRequest;
        //                allEmailsAndTimes.Add(new
        //                {
        //                    Key = userEmail,
        //                    Result = new
        //                    {
        //                        msg = $"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
        //                        IsSuccess = false,
        //                        remainingTime = (int)remainingTime.TotalSeconds
        //                    }
        //                });
        //                return allEmailsAndTimes;
        //            }
        //        }

        //        // Update the last request time
        //        LastPasswordResetRequestTimes[userEmail] = DateTime.Now;

        //        // Generate a new token and set the validity time
        //        var userEnt = UserRepo.Entities.Where(x => x.UserID == (int)employee["UserID"]).FirstOrDefault().MapTo<User>();
        //        userEnt.ForgotPasswordToken = Guid.NewGuid().ToString();
        //        userEnt.TokenValidityTime = DateTime.Now.AddHours(1);
        //        userEnt.SetModified();
        //        UserRepo.Add(userEnt);
        //        UserRepo.SaveChangesWithAudit();
        //        userDto.SetUnchanged();

        //        // Prepare email data
        //        var mailData = new List<Dictionary<string, object>>();
        //        var data = new Dictionary<string, object>
        //{
        //    { "Token", userEnt.ForgotPasswordToken },
        //    { "EmployeeName", employee["FullName"] }
        //};
        //        mailData.Add(data);

        //        var toMail = new List<string> { employee["WorkEmail"].ToString() };
        //        BasicMail((int)Util.MailGroupSetup.ForgotPasswordRequest, toMail, false, null, null, mailData);

        //        // Add this email's state to the list
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userEmail,
        //            Result = new
        //            {
        //                msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                IsSuccess = true,
        //                remainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //            }
        //        });

        //        return allEmailsAndTimes;
        //    }
        //}


        //public List<object> RequestForForgotPassword(UserDto userDto)
        //{
        //    var allEmailsAndTimes = new List<object>();

        //    string sql1 = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE ISNULL(IsLocked,0)=1 AND WorkEmail='{userDto.Email.Trim()}'";
        //    var lockedEmployee = UserRepo.GetData(sql1);
        //    if (lockedEmployee.Count() > 0)
        //    {
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! User Account is locked. Please contact HR!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }

        //    string sql = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE WorkEmail='{userDto.Email.Trim()}'";
        //    var employee = UserRepo.GetData(sql);
        //    if (employee.Count() <= 0)
        //    {
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! No User Found With This Email!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }
        //    else
        //    {
        //        var userEmail = userDto.Email.Trim();
        //        DateTime lastRequestTime;

        //        // Check the time since the last request
        //        if (LastPasswordResetRequestTimes.TryGetValue(userEmail, out lastRequestTime))
        //        {
        //            var timeSinceLastRequest = DateTime.Now - lastRequestTime;
        //            if (timeSinceLastRequest < TimeSpan.FromMinutes(3))
        //            {
        //                var remainingTime = TimeSpan.FromMinutes(3) - timeSinceLastRequest;
        //                allEmailsAndTimes.Add(new
        //                {
        //                    Key = userEmail,
        //                    Result = new
        //                    {
        //                        msg = $"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
        //                        IsSuccess = false,
        //                        remainingTime = (int)remainingTime.TotalSeconds
        //                    }
        //                });
        //                return allEmailsAndTimes;
        //            }
        //        }

        //        // Update the last request time
        //        LastPasswordResetRequestTimes[userEmail] = DateTime.Now;

        //        // Generate a new token and set the validity time
        //        var userEnt = UserRepo.Entities.Where(x => x.UserID == (int)employee["UserID"]).FirstOrDefault().MapTo<User>();
        //        userEnt.ForgotPasswordToken = Guid.NewGuid().ToString();
        //        userEnt.TokenValidityTime = DateTime.Now.AddHours(1);
        //        userEnt.SetModified();
        //        UserRepo.Add(userEnt);
        //        UserRepo.SaveChangesWithAudit();
        //        userDto.SetUnchanged();

        //        // Prepare email data
        //        var mailData = new List<Dictionary<string, object>>();
        //        var data = new Dictionary<string, object>
        //{
        //    { "Token", userEnt.ForgotPasswordToken },
        //    { "EmployeeName", employee["FullName"] }
        //};
        //        mailData.Add(data);

        //        var toMail = new List<string> { employee["WorkEmail"].ToString() };
        //        BasicMail((int)Util.MailGroupSetup.ForgotPasswordRequest, toMail, false, null, null, mailData);

        //        // Add this email's state to the list
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userEmail,
        //            Result = new
        //            {
        //                msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                IsSuccess = true,
        //                remainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //            }
        //        });

        //        return allEmailsAndTimes;
        //    }
        //}


        //public List<object> RequestForForgotPassword(UserDto userDto)
        //{
        //    var allEmailsAndTimes = new List<object>();

        //    string sql1 = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE ISNULL(IsLocked,0)=1 AND WorkEmail='{userDto.Email.Trim()}'";
        //    var lockedEmployee = UserRepo.GetData(sql1);
        //    if (lockedEmployee.Count() > 0)
        //    {
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! User Account is locked. Please contact HR!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }

        //    string sql = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE WorkEmail='{userDto.Email.Trim()}'";
        //    var employee = UserRepo.GetData(sql);
        //    if (employee.Count() <= 0)
        //    {
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! No User Found With This Email!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }
        //    else
        //    {
        //        var userEmail = userDto.Email.Trim();
        //        DateTime lastRequestTime;

        //        // Check the time since the last request
        //        if (LastPasswordResetRequestTimes.TryGetValue(userEmail, out lastRequestTime))
        //        {
        //            var timeSinceLastRequest = DateTime.Now - lastRequestTime;
        //            if (timeSinceLastRequest < TimeSpan.FromMinutes(3))
        //            {
        //                var remainingTime = TimeSpan.FromMinutes(3) - timeSinceLastRequest;
        //                allEmailsAndTimes.Add(new
        //                {
        //                    Key = userEmail,
        //                    Result = new
        //                    {
        //                        msg = $"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
        //                        IsSuccess = false,
        //                        remainingTime = (int)remainingTime.TotalSeconds
        //                    }
        //                });
        //                return allEmailsAndTimes;
        //            }
        //        }

        //        // Update the last request time
        //        LastPasswordResetRequestTimes[userEmail] = DateTime.Now;

        //        // Generate a new token and set the validity time
        //        var userEnt = UserRepo.Entities.Where(x => x.UserID == (int)employee["UserID"]).FirstOrDefault().MapTo<User>();
        //        userEnt.ForgotPasswordToken = Guid.NewGuid().ToString();
        //        userEnt.TokenValidityTime = DateTime.Now.AddHours(1);
        //        userEnt.SetModified();
        //        UserRepo.Add(userEnt);
        //        UserRepo.SaveChangesWithAudit();
        //        userDto.SetUnchanged();

        //        // Prepare email data
        //        var mailData = new List<Dictionary<string, object>>();
        //        var data = new Dictionary<string, object>
        //{
        //    { "Token", userEnt.ForgotPasswordToken },
        //    { "EmployeeName", employee["FullName"] }
        //};
        //        mailData.Add(data);

        //        var toMail = new List<string> { employee["WorkEmail"].ToString() };
        //        BasicMail((int)Util.MailGroupSetup.ForgotPasswordRequest, toMail, false, null, null, mailData);

        //        // Add this email's state to the list
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userEmail,
        //            Result = new
        //            {
        //                msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                IsSuccess = true,
        //                remainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //            }
        //        });

        //        return allEmailsAndTimes;
        //    }
        //}


        //public List<object> RequestForForgotPassword(UserDto userDto)
        //{
        //    var allEmailsAndTimes = new List<object>();

        //    string sql1 = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE ISNULL(IsLocked,0)=1 AND WorkEmail='{userDto.Email.Trim()}'";
        //    var lockedEmployee = UserRepo.GetData(sql1);
        //    if (lockedEmployee.Count() > 0)
        //    {
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! User Account is locked. Please contact HR!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }

        //    string sql = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE WorkEmail='{userDto.Email.Trim()}'";
        //    var employee = UserRepo.GetData(sql);
        //    if (employee.Count() <= 0)
        //    {
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! No User Found With This Email!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }
        //    else
        //    {
        //        var userEmail = userDto.Email.Trim();
        //        DateTime lastRequestTime;

        //        // Check the time since the last request
        //        if (LastPasswordResetRequestTimes.TryGetValue(userEmail, out lastRequestTime))
        //        {
        //            var timeSinceLastRequest = DateTime.Now - lastRequestTime;
        //            if (timeSinceLastRequest < TimeSpan.FromMinutes(3))
        //            {
        //                var remainingTime = TimeSpan.FromMinutes(3) - timeSinceLastRequest;
        //                allEmailsAndTimes.Add(new
        //                {
        //                    Key = userEmail,
        //                    Result = new
        //                    {
        //                        msg = $"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
        //                        IsSuccess = false,
        //                        remainingTime = (int)remainingTime.TotalSeconds
        //                    }
        //                });
        //                return allEmailsAndTimes;
        //            }
        //        }

        //        // Update the last request time
        //        LastPasswordResetRequestTimes[userEmail] = DateTime.Now;

        //        // Generate a new token and set the validity time
        //        var userEnt = UserRepo.Entities.Where(x => x.UserID == (int)employee["UserID"]).FirstOrDefault().MapTo<User>();
        //        userEnt.ForgotPasswordToken = Guid.NewGuid().ToString();
        //        userEnt.TokenValidityTime = DateTime.Now.AddHours(1);
        //        userEnt.SetModified();
        //        UserRepo.Add(userEnt);
        //        UserRepo.SaveChangesWithAudit();
        //        userDto.SetUnchanged();

        //        // Prepare email data
        //        var mailData = new List<Dictionary<string, object>>();
        //        var data = new Dictionary<string, object>
        //{
        //    { "Token", userEnt.ForgotPasswordToken },
        //    { "EmployeeName", employee["FullName"] }
        //};
        //        mailData.Add(data);

        //        var toMail = new List<string> { employee["WorkEmail"].ToString() };
        //        BasicMail((int)Util.MailGroupSetup.ForgotPasswordRequest, toMail, false, null, null, mailData);

        //        // Add this email's state to the list
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userEmail,
        //            Result = new
        //            {
        //                msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                IsSuccess = true,
        //                remainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //            }
        //        });

        //        return allEmailsAndTimes;
        //    }
        //}


        //public List<object> RequestForForgotPassword(UserDto userDto)
        //{
        //    var allEmailsAndTimes = new List<object>();

        //    string sql1 = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE ISNULL(IsLocked,0)=1 AND WorkEmail='{userDto.Email.Trim()}'";
        //    var lockedEmployee = UserRepo.GetData(sql1);
        //    if (lockedEmployee.Count() > 0)
        //    {
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! User Account is locked. Please contact HR!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }

        //    string sql = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE WorkEmail='{userDto.Email.Trim()}'";
        //    var employee = UserRepo.GetData(sql);
        //    if (employee.Count() <= 0)
        //    {
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userDto.Email.Trim(),
        //            Result = new
        //            {
        //                msg = "Ooops! No User Found With This Email!",
        //                IsSuccess = false,
        //                remainingTime = 0
        //            }
        //        });
        //        return allEmailsAndTimes;
        //    }
        //    else
        //    {
        //        var userEmail = userDto.Email.Trim();
        //        DateTime lastRequestTime;

        //        // Check the time since the last request
        //        if (LastPasswordResetRequestTimes.TryGetValue(userEmail, out lastRequestTime))
        //        {
        //            var timeSinceLastRequest = DateTime.Now - lastRequestTime;
        //            if (timeSinceLastRequest < TimeSpan.FromMinutes(3))
        //            {
        //                var remainingTime = TimeSpan.FromMinutes(3) - timeSinceLastRequest;
        //                allEmailsAndTimes.Add(new
        //                {
        //                    Key = userEmail,
        //                    Result = new
        //                    {
        //                        msg = $"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
        //                        IsSuccess = false,
        //                        remainingTime = (int)remainingTime.TotalSeconds
        //                    }
        //                });
        //                return allEmailsAndTimes;
        //            }
        //        }

        //        // Update the last request time
        //        LastPasswordResetRequestTimes[userEmail] = DateTime.Now;

        //        // Generate a new token and set the validity time
        //        var userEnt = UserRepo.Entities.Where(x => x.UserID == (int)employee["UserID"]).FirstOrDefault().MapTo<User>();
        //        userEnt.ForgotPasswordToken = Guid.NewGuid().ToString();
        //        userEnt.TokenValidityTime = DateTime.Now.AddHours(1);
        //        userEnt.SetModified();
        //        UserRepo.Add(userEnt);
        //        UserRepo.SaveChangesWithAudit();
        //        userDto.SetUnchanged();

        //        // Prepare email data
        //        var mailData = new List<Dictionary<string, object>>();
        //        var data = new Dictionary<string, object>
        //{
        //    { "Token", userEnt.ForgotPasswordToken },
        //    { "EmployeeName", employee["FullName"] }
        //};
        //        mailData.Add(data);

        //        var toMail = new List<string> { employee["WorkEmail"].ToString() };
        //        BasicMail((int)Util.MailGroupSetup.ForgotPasswordRequest, toMail, false, null, null, mailData);

        //        // Add this email's state to the list
        //        allEmailsAndTimes.Add(new
        //        {
        //            Key = userEmail,
        //            Result = new
        //            {
        //                msg = "Password recovery request made successfully. Please check your email for reset new password",
        //                IsSuccess = true,
        //                remainingTime = 180 // The time remaining for the next reset request (3 minutes)
        //            }
        //        });

        //        return allEmailsAndTimes;
        //    }
        //}


        //public (string, bool, int, List<KeyValuePair<string, DateTime>>) RequestForForgotPaassword(UserDto userDto)
        //{
        //    // Check if the user exists and is locked
        //    string sql1 = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE ISNULL(IsLocked,0)=1 AND WorkEmail='{userDto.Email.Trim()}'";
        //    var lockedEmployee = UserRepo.GetData(sql1);
        //    if (lockedEmployee.Count() > 0)
        //    {
        //        return ("Ooops! User Account is locked. Please contact HR!", false, 0, new List<KeyValuePair<string, DateTime>>());
        //    }

        //    // Check if the user exists
        //    string sql = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE WorkEmail='{userDto.Email.Trim()}'";
        //    var employee = UserRepo.GetData(sql);
        //    if (employee.Count() <= 0)
        //    {
        //        return ("Ooops! No User Found With This Email!", false, 0, new List<KeyValuePair<string, DateTime>>());
        //    }
        //    else
        //    {
        //        var userEmail = userDto.Email.Trim();
        //        DateTime lastRequestTime;

        //        // Declare the list to hold all emails and their corresponding last reset request times
        //        var allEmailsAndTimes = LastPasswordResetRequestTimes.ToList();

        //        // Check the time since the last request
        //        if (LastPasswordResetRequestTimes.TryGetValue(userEmail, out lastRequestTime))
        //        {
        //            var timeSinceLastRequest = DateTime.Now - lastRequestTime;
        //            if (timeSinceLastRequest < TimeSpan.FromMinutes(3))
        //            {
        //                var remainingTime = TimeSpan.FromMinutes(3) - timeSinceLastRequest;
        //                return ($"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
        //                        false,
        //                        (int)remainingTime.TotalSeconds,
        //                        allEmailsAndTimes);
        //            }
        //        }

        //        // Update the last request time
        //        LastPasswordResetRequestTimes[userEmail] = DateTime.Now;

        //        // Generate a new token and set the validity time
        //        var userEnt = UserRepo.Entities.Where(x => x.UserID == (int)employee["UserID"]).FirstOrDefault().MapTo<User>();
        //        userEnt.ForgotPasswordToken = Guid.NewGuid().ToString();
        //        userEnt.TokenValidityTime = DateTime.Now.AddHours(1);
        //        userEnt.SetModified();
        //        UserRepo.Add(userEnt);
        //        UserRepo.SaveChangesWithAudit();
        //        userDto.SetUnchanged();

        //        // Prepare email data
        //        var mailData = new List<Dictionary<string, object>>();
        //        var data = new Dictionary<string, object>
        //{
        //    { "Token", userEnt.ForgotPasswordToken },
        //    { "EmployeeName", employee["FullName"] }
        //};
        //        mailData.Add(data);

        //        var toMail = new List<string> {
        //    employee["WorkEmail"].ToString()
        //};
        //        BasicMail((int)Util.MailGroupSetup.ForgotPasswordRequest, toMail, false, null, null, mailData);

        //        // Collect all the emails and their corresponding last reset request times
        //        allEmailsAndTimes = LastPasswordResetRequestTimes.ToList();

        //        return ("Password recovery request made successfully. Please check your email for reset new password", true, 180, allEmailsAndTimes);
        //    }
        //}


        //public (string, bool, int) RequestForForgotPaassword(UserDto userDto)
        //{
        //    // Check if the user exists and is locked
        //    string sql1 = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE ISNULL(IsLocked,0)=1 AND WorkEmail='{userDto.Email.Trim()}'";
        //    var lockedEmployee = UserRepo.GetData(sql1);
        //    if (lockedEmployee.Count() > 0)
        //    {
        //        return ("Ooops! User Account is locked. Please contact HR!", false, 0);
        //    }

        //    // Check if the user exists
        //    string sql = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE WorkEmail='{userDto.Email.Trim()}'";
        //    var employee = UserRepo.GetData(sql);
        //    if (employee.Count() <= 0)
        //    {
        //        return ("Ooops! No User Found With This Email!", false, 0);
        //    }
        //    else
        //    {
        //        var userEmail = userDto.Email.Trim();
        //        DateTime lastRequestTime;

        //        // Check the time since the last request
        //        if (LastPasswordResetRequestTimes.TryGetValue(userEmail, out lastRequestTime))
        //        {
        //            var timeSinceLastRequest = DateTime.Now - lastRequestTime;
        //            if (timeSinceLastRequest < TimeSpan.FromMinutes(3))
        //            {
        //                var remainingTime = TimeSpan.FromMinutes(3) - timeSinceLastRequest;
        //                return ($"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.", false, (int)remainingTime.TotalSeconds);
        //            }
        //        }

        //        // Update the last request time
        //        LastPasswordResetRequestTimes[userEmail] = DateTime.Now;

        //        // Generate a new token and set the validity time
        //        var userEnt = UserRepo.Entities.Where(x => x.UserID == (int)employee["UserID"]).FirstOrDefault().MapTo<User>();
        //        userEnt.ForgotPasswordToken = Guid.NewGuid().ToString();
        //        userEnt.TokenValidityTime = DateTime.Now.AddHours(1);
        //        userEnt.SetModified();
        //        UserRepo.Add(userEnt);
        //        UserRepo.SaveChangesWithAudit();
        //        userDto.SetUnchanged();

        //        // Prepare email data
        //        var mailData = new List<Dictionary<string, object>>();
        //        var data = new Dictionary<string, object>
        //{
        //    { "Token", userEnt.ForgotPasswordToken },
        //    { "EmployeeName", employee["FullName"] }
        //};
        //        mailData.Add(data);

        //        var toMail = new List<string> {
        //    employee["WorkEmail"].ToString()
        //};
        //        BasicMail((int)Util.MailGroupSetup.ForgotPasswordRequest, toMail, false, null, null, mailData);
        //        return ("Password recovery request made successfully. Please check your email for reset new password", true, 0);
        //    }
        //}


        //public (string, bool) RequestForForgotPaassword(UserDto userDto)
        //{
        //    // Check if the user exists and is locked
        //    string sql1 = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE ISNULL(IsLocked,0)=1 AND WorkEmail='{userDto.Email.Trim()}'";
        //    var lockedEmployee = UserRepo.GetData(sql1);
        //    if (lockedEmployee.Count() > 0)
        //    {
        //        return ("Ooops! User Account is locked. Please contact HR!", false);
        //    }

        //    // Check if the user exists
        //    string sql = $@"SELECT TOP(1) * FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee WHERE WorkEmail='{userDto.Email.Trim()}'";
        //    var employee = UserRepo.GetData(sql);
        //    if (employee.Count() <= 0)
        //    {
        //        return ("Ooops! No User Found With This Email!", false);
        //    }
        //    else
        //    {
        //        var userEmail = userDto.Email.Trim();
        //        DateTime lastRequestTime;

        //        // Check the time since the last request
        //        if (LastPasswordResetRequestTimes.TryGetValue(userEmail, out lastRequestTime))
        //        {
        //            var timeSinceLastRequest = DateTime.Now - lastRequestTime;
        //            if (timeSinceLastRequest < TimeSpan.FromMinutes(3))
        //            {
        //                var remainingTime = TimeSpan.FromMinutes(3) - timeSinceLastRequest;
        //                return ($"Hit this API after this remaining time: {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.", false);
        //            }
        //        }

        //        // Update the last request time
        //        LastPasswordResetRequestTimes[userEmail] = DateTime.Now;

        //        // Generate a new token and set the validity time
        //        var userEnt = UserRepo.Entities.Where(x => x.UserID == (int)employee["UserID"]).FirstOrDefault().MapTo<User>();
        //        userEnt.ForgotPasswordToken = Guid.NewGuid().ToString();
        //        userEnt.TokenValidityTime = DateTime.Now.AddHours(1);
        //        userEnt.SetModified();
        //        UserRepo.Add(userEnt);
        //        UserRepo.SaveChangesWithAudit();
        //        userDto.SetUnchanged();

        //        // Prepare email data
        //        var mailData = new List<Dictionary<string, object>>();
        //        var data = new Dictionary<string, object>
        //        {
        //            { "Token", userEnt.ForgotPasswordToken },
        //            { "EmployeeName", employee["FullName"] }
        //        };
        //        mailData.Add(data);

        //        var toMail = new List<string> {
        //            employee["WorkEmail"].ToString()
        //        };
        //        BasicMail((int)Util.MailGroupSetup.ForgotPasswordRequest, toMail, false, null, null, mailData);
        //        return ("Password recovery request made successfully. Please check your email for reset new password", true);
        //    }
        //}

        public (int, string, bool) GetUserByRequestToken(UserDto userDto)
        {
            var userEnt = UserRepo.Entities.Where(x => x.ForgotPasswordToken == userDto.ForgotPasswordToken).SingleOrDefault().MapTo<User>();
            if (userEnt.IsNull())
            {
                return (0, "Invalid Token! Please try with correct token.", false);
            }
            else if (userEnt.TokenValidityTime.Value < DateTime.Now)
            {
                return (0, "Your token has been expired! Please request once again for reset yor password.", false);
            }
            else
            {
                return (userEnt.UserID, $@"Welcome back {userEnt.UserName}", true);
            }
        }
        public (string, bool) ResetPassword(UserDto userDto)
        {
            var userEnt = UserRepo.Entities.Where(x => x.UserID == userDto.UserID).SingleOrDefault().MapTo<User>();


            //password
            if (userDto.Password.Length < 8)
            {
                return ("Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol", false);

            }
            if (!userDto.Password.Any(char.IsDigit))
            {
                return ("Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol", false);

            }
            if (!userDto.Password.Any(char.IsUpper))
            {
                return ("Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol", false);

            }
            if (!userDto.Password.Any(char.IsLower))
            {
                return ("Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol", false);

            }
            if (!checkPasswordSpecialCharacter(userDto.Password))
            {
                return ("Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol", false);

            }


            if (userEnt.IsNull())
            {
                return ("Invalid Token! Please try with correct token.", false);
            }
            else if (userEnt.TokenValidityTime.Value < DateTime.Now)
            {
                return ("Your token has been expired! Please request once again for reset yor password.", false);
            }
            else
            {
                userEnt.ForgotPasswordToken = null;
                userEnt.TokenValidityTime = null;
                CreatePasswordHash(userDto.Password, out var passwordHash, out var passwordSalt);
                userEnt.PasswordHash = passwordHash;
                userEnt.PasswordSalt = passwordSalt;
                userEnt.IsForcedLogin = false;
                userEnt.CompanyID = userDto.CompanyID ?? AppContexts.User.CompanyID;
                userEnt.SetModified();
                UserRepo.Add(userEnt);
                UserRepo.SaveChangesWithAudit();
                userDto.SetUnchanged();
                return ("Your password reset successfull. Please re-login with your new password", true);
            }
        }

        public bool HasPermissionChangeUser(string employeeCode)
        {
            var results = Array.FindAll(Util.TokenIDs, s => s.Equals(employeeCode));
            return results.IsNotNull() && results.Length > 0 ? true : false;
        }

        public bool GetIsUniqueUserName(int userId, string userName)
        {
            bool isConditionTrue = UserRepo.Entities
                .Where(item =>
                    item.UserName.ToLower() == userName.ToLower() &&
                    item.UserID != userId)
                .Any();

            bool result = isConditionTrue ? false : true;

            return result;
        }

        public UserLoginPolicyDto CheckUserLoginPolicy(string userName, string password)
        {
            int ReasonID = 0;
            if (userName.IsNullOrEmpty() || password.IsNullOrEmpty()) return SetUserLoginPolicyDto(1, null, "User Name Or Password is Empty!", 0, null, 0);
            if (userName.Equals(Util.Integrations.superadmin.ToString()) && password.Equals(Util.integrationHashToken))
            {
                return SetUserLoginPolicyDto(ReasonID, GetUserWhoHasPermission(userName), "", 0, null, 0);
            }
            var userLoginPolicyDto = SetUserLoginPolicyDto(ReasonID, null, "", 0, null, 0);


            var user = UserRepo.SingleOrDefault(x => x.UserName == userName && x.IsActive == true);
            //user.ChangePasswordDatetime = user.ChangePasswordDatetime.IsNull() ? DateTime.Now: user.ChangePasswordDatetime;
            if (user.IsNull())
            {
                return SetUserLoginPolicyDto(1, null, "Invalid User!", 0, null, 0);
            }
            var duration = (TimeSpan)(DateTime.Now - user.ChangePasswordDatetime);
            var lockedDuration = user.LockedDateTime.IsNull() ? TimeSpan.Zero : (TimeSpan)(DateTime.Now - user.LockedDateTime);



            var systemConfiguration = SystemConfigurationRepo.SingleOrDefault(x => x.CompanyID == user.CompanyID);
            if (systemConfiguration == null)
            {
                return SetUserLoginPolicyDto(1, null, "Configruation Is Empty Please contract with HR!", 0, null, 0);
            }
            if (user.IsLocked && lockedDuration.TotalMinutes < systemConfiguration.UserAccountLockedDurationInMin)
            {
                if (user.ReasonID == (int)Util.LoginPolicyReason.InvalidPassword)
                    return SetUserLoginPolicyDto((int)Util.LoginPolicyReason.InvalidPassword, null, @$"You have tried wrong password more then {systemConfiguration.AccessFailedCountMax}.", systemConfiguration.UserAccountLockedDurationInMin, user.LockedDateTime, 0);
                else
                    return SetUserLoginPolicyDto((int)Util.LoginPolicyReason.ManuallyLocked, null, @$"Your account has been blocked manually by HR or Admin .", systemConfiguration.UserAccountLockedDurationInMin, user.LockedDateTime, 0);
            }
            var log = new UserLogTracker
            {
                UserID = user.UserID,
                IsLive = false,
                LogInDate = DateTime.Now,
                IPAddress = AppContexts.GetIPAddress(),
                ObjectState = ModelState.Added,
                CompanyID = AppContexts.User.CompanyID,
                ReasonID = ReasonID,
                IsLoginFailed = false
            };

            return ValidateUser(password, ref ReasonID, userLoginPolicyDto, user, duration, systemConfiguration, log);
        }

        private UserLoginPolicyDto ValidateUser(string password, ref int ReasonID, UserLoginPolicyDto userLoginPolicyDto, User user, TimeSpan duration, SystemConfiguration systemConfiguration, UserLogTracker log)
        {
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                ReasonID = (int)Util.LoginPolicyReason.InvalidPassword;
                user.AccessFailedCount += 1;
                if (user.AccessFailedCount >= systemConfiguration.AccessFailedCountMax)
                {
                    user.IsLocked = true;
                    user.LockedDateTime = DateTime.Now;
                    user.AccessFailedCount = 0;
                }

                log.IsLoginFailed = true;
                //log.ReasonID = user.ReasonID.IsNull() || (int)user.ReasonID == 0 ? 0: user.ReasonID;
                log.ReasonID = ReasonID;
                user.ReasonID = ReasonID;
                SaveUserAndLogTracker(user, log);
                //return SetUserLoginPolicyDto(1, null, "Invalid Password!", systemConfiguration.UserAccountLockedDurationInMin, user.LockedDateTime);
                if (user.AccessFailedCount == systemConfiguration.AccessFailedCountMax - 1)
                {
                    return SetUserLoginPolicyDto(1, null, $"You already tried {systemConfiguration.AccessFailedCountMax - 1} times maximum limit is {systemConfiguration.AccessFailedCountMax}", 0, null, (int)user.AccessFailedCount);
                }
                else
                {
                    if (user.AccessFailedCount == 0 && user.IsLocked == true)
                    {
                        return SetUserLoginPolicyDto(1, null, $"You already tried {systemConfiguration.AccessFailedCountMax} times maximum limit is {systemConfiguration.AccessFailedCountMax}.Your account is now locked.", 0, null, (int)user.AccessFailedCount);
                    }
                    else { return SetUserLoginPolicyDto(1, null, "Invalid Password!", 0, null, 0); }
                }

            }
            else if (duration.TotalDays > systemConfiguration.UserPasswordChangedDurationInDays)
            {
                //user.IsLocked = true;
                //user.LockedDateTime = DateTime.Now;
                ReasonID = (int)Util.LoginPolicyReason.OverSystemDays;
                user.IsForcedLogin = true;

                log.IsLoginFailed = true;
                log.ReasonID = (int)Util.LoginPolicyReason.OverSystemDays;
                user.ReasonID = (int)Util.LoginPolicyReason.OverSystemDays;

                SaveUserAndLogTracker(user, log);
                //return SetUserLoginPolicyDto(ReasonID, null, "Over System Date!", systemConfiguration.UserAccountLockedDurationInMin, user.LockedDateTime);
            }
            else
            {
                user.AccessFailedCount = 0;
                user.ReasonID = 0;
                user.IsLocked = false;
                user.LockedDateTime = null;
                SaveUserAndLogTracker(user, log);
            }
            UserDto userDto = SetUserData(user, log);
            userLoginPolicyDto.User = userDto;
            return userLoginPolicyDto;
        }

        private UserLoginPolicyDto SetUserLoginPolicyDto(int ReasonID, UserDto User, string Message, int UserAccountLockedDurationInMin, DateTime? LockedDateTime, int failedCount)
        {
            var userLoginPolicyDto = new UserLoginPolicyDto()
            {
                ReasonID = ReasonID,
                User = User,
                Message = Message,
                LockedDateTime = LockedDateTime,
                UserAccountLockedDurationInMin = UserAccountLockedDurationInMin,
                FailedCount = failedCount,
            };
            return userLoginPolicyDto;
        }

        private void SaveUserAndLogTracker(User user, UserLogTracker log)
        {
            if (user != null)
            {
                user.SetModified();
                UserRepo.Add(user);
                UserRepo.SaveChanges();
            }
            if (log != null)
            {
                LogRepo.Add(log);
                LogRepo.SaveChanges();
            }

        }

        public void SaveHashedTokenToBlackList(string hashedToken)
        {

            var userToken = new UserTokenBlackList
            {
                UserID = AppContexts.User.UserID,
                Token = hashedToken,
                CreatedDate = DateTime.Now,
                CreatedIP = AppContexts.GetIPAddress(),
                CompanyID = AppContexts.User.CompanyID
            };
            userToken.SetAdded();
            UserTokenBlackListRepo.Add(userToken);
            UserTokenBlackListRepo.SaveChanges();
        }

        public List<UserTokenBlackList> GetHashedTokensFromBlackList()
        {
            var tokensFromBlackList = UserTokenBlackListRepo.GetAllList(); // Assuming GetAllList is synchronous
            return tokensFromBlackList.MapTo<List<UserTokenBlackList>>();
        }


        public Task<UserAndProfileDto> SaveChangesPrismUser(UserAndProfileDto UserAndProfileDto)
        {

            //var isExists = UserRepo.Entities.FirstOrDefault(x => x.UserID == userDto.UserID).MapTo<User>();

            //if (UserAndProfileDto.UserDto.Password.IsNotNullOrEmpty())
            //{
            //    char[] passChArray = UserAndProfileDto.UserDto.Password.ToCharArray();

            //    if (UserAndProfileDto.UserDto.Password.Length < 8)
            //    {
            //        UserAndProfileDto.UserDto.PasswordError = "Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol";
            //        return Task.FromResult(UserAndProfileDto);
            //    }

            //    if (!UserAndProfileDto.UserDto.Password.Any(char.IsDigit))
            //    {
            //        UserAndProfileDto.UserDto.PasswordError = "Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol";
            //        return Task.FromResult(UserAndProfileDto);
            //    }
            //    if (!UserAndProfileDto.UserDto.Password.Any(char.IsUpper))
            //    {
            //        UserAndProfileDto.UserDto.PasswordError = "Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol";
            //        return Task.FromResult(UserAndProfileDto);
            //    }
            //    if (!UserAndProfileDto.UserDto.Password.Any(char.IsLower))
            //    {
            //        UserAndProfileDto.UserDto.PasswordError = "Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol";
            //        return Task.FromResult(UserAndProfileDto);
            //    }
            //    if (!checkPasswordSpecialCharacter(UserAndProfileDto.UserDto.Password))
            //    {
            //        UserAndProfileDto.UserDto.PasswordError = "Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol";
            //        return Task.FromResult(UserAndProfileDto);
            //    }
            //}

            //var isExistsUserPerson = UserRepo.Entities.FirstOrDefault(x => x.UserID != userDto.UserID && x.PersonID == userDto.PersonID).MapTo<User>();

            //if (isExistsUserPerson.IsNotNull())
            //{
            //    userDto.DuplicateUserError = "This person is already used by another user.";
            //    return Task.FromResult(userDto);
            //}

            var isExistsName = UserRepo.Entities.FirstOrDefault(x => x.UserID != UserAndProfileDto.UserDto.UserID && x.PersonID == UserAndProfileDto.UserDto.PersonID && x.UserName.ToLower() == UserAndProfileDto.UserDto.UserName.ToLower()).MapTo<User>();
            if (isExistsName.IsNotNull())
            {
                UserAndProfileDto.UserDto.DuplicateUserError = "This User already exists.";
                return Task.FromResult(UserAndProfileDto);
            }
            using (var unitOfWork = new UnitOfWork())
            {
                var securityGroupUserChilds = new List<SecurityGroupUserChild>();
                var existUser = UserRepo.Entities.SingleOrDefault(x => x.UserID == UserAndProfileDto.UserDto.UserID).MapTo<User>();
                var existProfileUser = UserProfileRepo.Entities.SingleOrDefault(x => x.UserID == UserAndProfileDto.UserProfileDto.UserID).MapTo<UserProfile>();
                //var person = PersonRepo.Entities.SingleOrDefault(x => x.PersonID == userDto.PersonID).MapTo<Person>();

                if (existUser.IsNull() || UserAndProfileDto.UserDto.UserID.IsZero() || UserAndProfileDto.UserDto.IsAdded)
                {
                    UserAndProfileDto.UserDto.IsAdmin = false;
                    UserAndProfileDto.UserDto.SetAdded();
                    SetNewUserID(UserAndProfileDto.UserDto);

                    UserAndProfileDto.UserProfileDto.SetAdded();
                    UserAndProfileDto.UserProfileDto.UserID = UserAndProfileDto.UserDto.UserID;

                }
                else
                {
                    UserAndProfileDto.UserDto.IsAdmin = existUser.IsAdmin;
                    UserAndProfileDto.UserDto.SetModified();

                    UserAndProfileDto.UserProfileDto.SetModified();
                    UserAndProfileDto.UserProfileDto.RowVersion = existProfileUser.RowVersion;
                    UserAndProfileDto.UserProfileDto.UserID = existProfileUser.UserID;
                    UserAndProfileDto.UserProfileDto.CreatedBy = existProfileUser.CreatedBy;
                    UserAndProfileDto.UserProfileDto.CreatedIP = existProfileUser.CreatedIP;
                    UserAndProfileDto.UserProfileDto.CreatedDate = existProfileUser.CreatedDate;
                }
                var userEnt = UserAndProfileDto.UserDto.MapTo<User>();
                var userProfileEnt = UserAndProfileDto.UserProfileDto.MapTo<UserProfile>();

                if (UserAndProfileDto.UserDto.Password.IsNullOrEmpty() && UserAndProfileDto.UserDto.IsModified)
                {
                    userEnt.PasswordHash = existUser.PasswordHash;
                    userEnt.PasswordSalt = existUser.PasswordSalt;
                }
                else
                {
                    CreatePasswordHash(UserAndProfileDto.UserDto.Password, out var passwordHash, out var passwordSalt);
                    userEnt.PasswordHash = passwordHash;
                    userEnt.PasswordSalt = passwordSalt;
                    userEnt.IsForcedLogin = UserAndProfileDto.UserDto.IsForcedLogin;
                }

                userEnt.CompanyID = UserAndProfileDto.UserDto.CompanyID ?? AppContexts.User.CompanyID;
                if (UserAndProfileDto.UserDto.ApplicationID.IsNull() && existUser.IsNull())
                    userEnt.DefaultApplicationID = 1;
                else if (UserAndProfileDto.UserDto.ApplicationID.IsNull() && existUser.IsNotNull())
                    userEnt.DefaultApplicationID = existUser.DefaultApplicationID;
                else userEnt.DefaultApplicationID = UserAndProfileDto.UserDto.ApplicationID;

                foreach (var securityGroupChild in UserAndProfileDto.UserDto.SecurityGroupUserChildList)
                {
                    var groupUserChild = new SecurityGroupUserChild
                    {
                        UserID = userEnt.UserID,
                        SecurityGroupID = securityGroupChild.SecurityGroupID
                    };
                    var existingGroupUserChild = SecurityGroupUserChildRepo.Entities.Where(
                            ruleChildEnt => ruleChildEnt.SecurityGroupID == groupUserChild.SecurityGroupID &&
                            ruleChildEnt.UserID == groupUserChild.UserID
                        ).FirstOrDefault();

                    if (existingGroupUserChild.IsNull())
                    {
                        groupUserChild.SetAdded();
                        SetNewSecurityGroupUserChildID(groupUserChild);
                    }
                    else if (securityGroupChild.IsModified || existingGroupUserChild.IsNotNull())
                    {
                        groupUserChild.SetModified();
                        groupUserChild.RowVersion = existingGroupUserChild.RowVersion;
                        groupUserChild.SecurityGroupUserChildID = existingGroupUserChild.SecurityGroupUserChildID;
                        groupUserChild.CreatedBy = existingGroupUserChild.CreatedBy;
                        groupUserChild.CreatedIP = existingGroupUserChild.CreatedIP;
                        groupUserChild.CreatedDate = existingGroupUserChild.CreatedDate;
                    }
                    securityGroupUserChilds.Add(groupUserChild);
                }
                var SecurityGroupUserChildList = securityGroupUserChilds.MapTo<List<SecurityGroupUserChild>>();

                #region Delete Rules 
                if (existUser.IsNotNull())
                {
                    var list = SecurityGroupUserChildRepo.Entities.Where(
                            ruleChildEnt => ruleChildEnt.UserID == existUser.UserID
                        );
                    foreach (var obj in list)
                    {
                        var exist = SecurityGroupUserChildList.Find(x => x.SecurityGroupUserChildID == obj.SecurityGroupUserChildID);
                        if (exist.IsNull())
                        {
                            obj.SetDeleted();
                            SecurityGroupUserChildList.Add(obj);
                        }
                    }
                }

                #endregion Delete Rules 


                SetAuditFields(userProfileEnt);
                SetAuditFields(SecurityGroupUserChildList);

                UserRepo.Add(userEnt);
                UserProfileRepo.Add(userProfileEnt);
                SecurityGroupUserChildRepo.AddRange(SecurityGroupUserChildList);
                unitOfWork.CommitChangesWithAudit();

                UserAndProfileDto.UserDto.UserID = userEnt.UserID;
                //userDto.Email = person.Email;
                //userDto.FullName = $@"{person.FirstName} {person.LastName}";
                //userDto.SetUnchanged();
                if (UserAndProfileDto.UserDto.Password.IsNotNullOrEmpty())
                {
                    var mailConfigurationDto = GetMailConfiguration().Result;
                    var mailGroupSetup = MailGroupSetup((int)Util.MailGroupSetup.UserCreationEmailConfiguration).Result;


                    //string body = WelComeMailBodyFromSetup(userDto, mailGroupSetup.Body);
                    //string subject = UserCreationMailSubject(userDto);

                    //SendMailFromSetup(mailConfigurationDto, userDto, subject, body);
                }
            }
            return Task.FromResult(UserAndProfileDto);

        }

        public GridModel GetPrismUserListDicGrid(GridParameter parameters)
        {
            string filter = "";
            string where = "";
            switch (parameters.ApprovalFilterData)
            {
                case "All":
                    filter = "";
                    break;
                case "Active":
                    filter = $@" U.IsActive=1";
                    break;
                case "InActive":
                    filter = $@" U.IsActive=0";
                    break;
                case "Locked":
                    filter = $@" U.IsLocked=1";
                    break;

                default:
                    break;
            }
            where = filter.IsNotNullOrEmpty() ? "AND " : "";

            //string replaceSearch = Regex.Replace(parameters.Search.ToUpper(), @"(\bACTIVE\b)|(\bINACTIVE\b)", match => match.Value.ToUpper() == "ACTIVE" ? "1" : "0");
            //parameters.Search = replaceSearch;



            string additinalFilter = "";
            if (!string.IsNullOrEmpty(parameters.AdditionalFilterData))
            {
                additinalFilter = $@"AND UP.PositionID in({parameters.AdditionalFilterData})";

                if (parameters.AdditionalFilterData.Contains("29") || parameters.AdditionalFilterData.Contains("34") || parameters.AdditionalFilterData.Contains("35"))
                {
                    additinalFilter = $@"AND SGUC.SecurityGroupID IN (29, 34, 35)";//tm,to,PRO
                }
                else if (parameters.AdditionalFilterData.Contains("30") || parameters.AdditionalFilterData.Contains("31") || parameters.AdditionalFilterData.Contains("32") || parameters.AdditionalFilterData.Contains("33"))
                {
                    additinalFilter = $@"AND SGUC.SecurityGroupID = {parameters.AdditionalFilterData}";
                }
            }

            string sql = string.Empty;
            if (!string.IsNullOrEmpty(parameters.AdditionalFilterData) && parameters.AdditionalFilterData.Contains("29") || parameters.AdditionalFilterData.Contains("30") || parameters.AdditionalFilterData.Contains("31") || parameters.AdditionalFilterData.Contains("32") || parameters.AdditionalFilterData.Contains("33") || parameters.AdditionalFilterData.Contains("34") || parameters.AdditionalFilterData.Contains("35"))
            {
                sql = $@"SELECT DISTINCT
                                U.UserID
                                ,U.UserName
	                            ,U.DefaultApplicationID
	                            ,U.IsAdmin
	                            ,U.IsActive
	                            ,U.InActiveDate
	                            ,U.AccessFailedCount
	                            ,U.TwoFactorEnabled
	                            ,U.PhoneNumberConfirmed
	                            ,U.EmailConfirmed
	                            ,U.PersonID
                                ,M.Title AS MenuURL
                                ,Emp.FullName UserFullName
	                            ,0 DistributionHouseID
	                            ,EM.RegionID
	                            ,EM.ClusterID
	                            ,SGUC.SecurityGroupID PositionID
	                            ,Emp.WorkMobile ContactNumber
                                ,Emp.WorkMobile WalletNumber
	                            ,Emp.DateOfJoining JoiningDate
	                            ,'' Longitude
	                            ,'' Latitude
	                            ,0 VisitTypeID
	                            ,SGUC.CompanyID
	                            ,SGUC.CreatedBy
	                            ,SGUC.CreatedDate
	                            ,SGUC.CreatedIP
	                            ,SGUC.UpdatedBy
	                            ,SGUC.UpdatedDate
                                ,'' DHParentCode
	                            ,'' ParentCode
								,'' ResignDate
                                , '' DHName
                                ,'' DHWallet
                                ,ISNULL(R.RegionName, '') RegionName
								,ISNULL(C.ClusterName, '') ClusterName
                                ,''  RSM_ZSM_TM_TOName
								,'' RSM_ZSM_TM_TONumber
								,SGM.SecurityGroupName PositionName
                            FROM Users U
                            LEFT JOIN Menu M ON U.DefaultMenuID=M.MenuID
							LEFT JOIN Security..SecurityGroupUserChild SGUC ON SGUC.UserID = U.UserID
							LEFT JOIN Security..SecurityGroupMaster SGM ON SGM.SecurityGroupID = SGUC.SecurityGroupID
							LEFT JOIN HRMS..Employee Emp ON Emp.PersonID = U.PersonID
							LEFT JOIN HRMS..Employment Em ON Em.EmployeeID = Emp.EmployeeID AND IsCurrent = 1
		                    LEFT JOIN HRMS..Region R ON EM.RegionID = R.RegionID
							LEFT JOIN HRMS..Cluster C ON EM.ClusterID = C.ClusterID
							Where U.PersonID <> 0 {where} {filter} {additinalFilter}
                            ";
            }
            else
            {
                sql = $@"SELECT DISTINCT
                                U.UserID
                                ,ISNULL(U.UserName, '') UserName
	                            ,U.DefaultApplicationID
	                            ,U.IsAdmin
	                            ,U.IsActive
	                            ,U.InActiveDate
	                            ,U.AccessFailedCount
	                            ,U.TwoFactorEnabled
	                            ,U.PhoneNumberConfirmed
	                            ,U.EmailConfirmed
	                            ,U.PersonID
                                ,UP.UserFullName
	                            ,UP.DistributionHouseID
	                            ,UP.RegionID
	                            ,UP.ClusterID
	                            ,UP.PositionID
	                            ,ISNULL(UP.ContactNumber, '') ContactNumber
                                ,ISNULL(UP.ContactNumber, '') WalletNumber
	                            ,ISNULL(UP.JoiningDate, '') JoiningDate
	                            ,UP.Longitude
	                            ,UP.Latitude
	                            ,UP.VisitTypeID
	                            ,UP.CompanyID
	                            ,UP.CreatedBy
	                            ,UP.CreatedDate
	                            ,UP.CreatedIP
	                            ,UP.UpdatedBy
	                            ,UP.UpdatedDate
                                ,CASE WHEN UP.PositionID = 28 THEN ISNULL(UP.ParentCode, '') ELSE ISNULL(DH.ParentCode, '') END AS DHParentCode
								,ISNULL('','') ResignDate
                                ,CASE WHEN UP.PositionID = 28 THEN ISNULL(UP.UserFullName, '') ELSE ISNULL(DH.UserFullName, '') END DHName
                                ,CASE WHEN UP.PositionID = 28 THEN ISNULL(UP.ContactNumber, '') ELSE ISNULL(DH.ContactNumber, '') END DHWallet
                                ,ISNULL(R.RegionName, '') RegionName
								,ISNULL(C.ClusterName, '') ClusterName
								,ISNULL(Emp.FullName, '') RSM_ZSM_TM_TOName
								,ISNULL(Emp.WorkMobile, '') RSM_ZSM_TM_TONumber
                                ,ISNULL(SG.SecurityGroupName, '') PositionName
                            FROM Users U
							LEFT JOIN UserProfile UP ON UP.UserID = U.UserID
							LEFT JOIN Users RSMTMTO ON RSMTMTO.UserID = Up.DistributionHouseID
                            LEFT JOIN UserProfile DH ON DH.UserID = UP.DistributionHouseID
                            LEFT JOIN Users DSO ON DSO.UserID = DH.DistributionHouseID
							LEFT JOIN HRMS..Employee Emp ON Emp.PersonID = RSMTMTO.PersonID OR Emp.PersonID = DSO.PersonID
							LEFT JOIN HRMS..Employment Em ON em.EmployeeID = Emp.EmployeeID AND IsCurrent = 1
							LEFT JOIN HRMS..Region R ON R.RegionID = Em.RegionID 
							LEFT JOIN HRMS..Cluster C ON Em.ClusterID = C.ClusterID
                            LEFT JOIN Security..SecurityGroupMaster SG ON SG.SecurityGroupID = UP.PositionID
							Where U.PersonID= 0 {where} {filter} {additinalFilter}
                            ";
            }


            var result = UserRepo.LoadGridModel(parameters, sql);
            return result;
        }

        //public async Task<UserDto> GetPrismUser(int UserID)
        public async Task<List<Dictionary<string, object>>> GetPrismUser(int PositionID, int UserID)
        {
            string sql = string.Empty;

            if (PositionID == 29 || PositionID == 30 || PositionID == 31 || PositionID == 32 || PositionID == 33 || PositionID == 34 || PositionID == 35)
            {
                sql = $@"SELECT DISTINCT
                                U.*
                                --U.UserID
                                --,U.UserName
	                            --,U.DefaultApplicationID
	                            --,U.IsAdmin
	                            --,U.IsActive
	                            --,U.InActiveDate
	                            --,U.AccessFailedCount
	                            --,U.TwoFactorEnabled
	                            --,U.PhoneNumberConfirmed
	                            --,U.EmailConfirmed
	                            --,U.PersonID
                                ,M.Title AS MenuURL
                                ,Emp.FullName UserFullName
	                            ,0 DistributionHouseID
	                            ,EM.RegionID
	                            ,EM.ClusterID
	                            ,SGUC.SecurityGroupID PositionID
	                            ,Emp.WorkMobile ContactNumber
                                ,Emp.WorkMobile WalletNumber
	                            ,Emp.DateOfJoining JoiningDate
	                            ,'' Longitude
	                            ,'' Latitude
	                            ,0 VisitTypeID
	                            ,SGUC.CompanyID
	                            ,SGUC.CreatedBy
	                            ,SGUC.CreatedDate
	                            ,SGUC.CreatedIP
	                            ,SGUC.UpdatedBy
	                            ,SGUC.UpdatedDate
                                ,'' DHParentCode
	                            ,'' ParentCode
								,'' ResignDate
                                , '' DHName
                                ,'' DHWallet
                                ,ISNULL(R.RegionName, '') RegionName
								,ISNULL(C.ClusterName, '') ClusterName
                                ,''  RSM_ZSM_TM_TOName
								,'' RSM_ZSM_TM_TONumber
								,SGM.SecurityGroupName PositionName
                                ,Emp.EmployeeID
                            FROM Users U
                            LEFT JOIN Menu M ON U.DefaultMenuID=M.MenuID
							LEFT JOIN Security..SecurityGroupUserChild SGUC ON SGUC.UserID = U.UserID
							LEFT JOIN Security..SecurityGroupMaster SGM ON SGM.SecurityGroupID = SGUC.SecurityGroupID
							LEFT JOIN HRMS..Employee Emp ON Emp.PersonID = U.PersonID
							LEFT JOIN HRMS..Employment Em ON Em.EmployeeID = Emp.EmployeeID AND IsCurrent = 1
		                    LEFT JOIN HRMS..Region R ON EM.RegionID = R.RegionID
							LEFT JOIN HRMS..Cluster C ON EM.ClusterID = C.ClusterID
                        WHERE U.UserID = {UserID} AND SGUC.SecurityGroupID IN (29,30,31,32,33,34,35)";
            }
            else
            {
                sql = $@"SELECT 
                            ISNULL(U.PersonID, 0) AS PersonID,
                             U.*
	                        ,UP.UserFullName
	                        ,UP.DistributionHouseID
	                        ,UP.RegionID
	                        ,UP.ClusterID
	                        ,UP.PositionID
	                        ,UP.ContactNumber
	                        ,UP.JoiningDate
	                        ,UP.Longitude
	                        ,UP.Latitude
	                        ,UP.VisitTypeID
	                        ,UP.CompanyID
	                        ,UP.CreatedBy
	                        ,UP.CreatedDate
	                        ,UP.CreatedIP
	                        ,UP.UpdatedBy
	                        ,UP.UpdatedDate
	                        ,UP.ParentCode
                            ,SV.SystemVariableCode AS Reason
                            --,DH.DistributionHouseID
                            --,DH.UserFullName AS DistributionHouseName
                            --,DH.UserFullName RSM_ZSM_TM_TOName
                            ,CASE WHEN UP.PositionID = 28 THEN Emp.FullName COLLATE SQL_Latin1_General_CP1_CI_AS ELSE DH.UserFullName COLLATE SQL_Latin1_General_CP1_CI_AS END AS DistributionHouseName
                            ,ISNULL(UP.DistributionHouseID, '') RSM_ZSM_TM_TOUserID
							,ISNULL(Emp.FullName, '') RSM_ZSM_TM_TOName
							,ISNULL(Emp.WorkMobile, '') RSM_ZSM_TM_TONumber
                            ,ISNULL(R.RegionName, '') RegionName
							,ISNULL(C.ClusterName, '') ClusterName
                        FROM Users U
							LEFT JOIN UserProfile UP ON UP.UserID = U.UserID
							LEFT JOIN SystemVariable SV ON SV.SystemVariableID = U.ReasonID
							LEFT JOIN Users RSMTMTO ON RSMTMTO.UserID = Up.DistributionHouseID
                            LEFT JOIN UserProfile DH ON DH.UserID = UP.DistributionHouseID
							LEFT JOIN HRMS..Employee Emp ON Emp.PersonID = RSMTMTO.PersonID
							LEFT JOIN HRMS..Employment Em ON em.EmployeeID = Emp.EmployeeID AND IsCurrent = 1
							LEFT JOIN HRMS..Region R ON R.RegionID = Em.RegionID 
							LEFT JOIN HRMS..Cluster C ON Em.ClusterID = C.ClusterID
                            LEFT JOIN Security..SecurityGroupMaster SG ON SG.SecurityGroupID = UP.PositionID
                        WHERE UP.PositionID = {PositionID} AND U.UserID = {UserID}";

            }


            var result = UserRepo.GetDataDictCollection(sql);

            return await Task.FromResult(result.ToList());
        }



        public Task<UserDto> SaveChangesPrismAmRsmToTm(UserDto userDto, UserProfileDto userProfileDto)
        {
            //var isExists = UserRepo.Entities.FirstOrDefault(x => x.UserID == userDto.UserID).MapTo<User>();

            //if (userDto.Password.IsNotNullOrEmpty())
            //{
            //    char[] passChArray = userDto.Password.ToCharArray();

            //    if (userDto.Password.Length < 8)
            //    {
            //        userDto.PasswordError = "Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol";
            //        return Task.FromResult(userDto);
            //    }

            //    if (!userDto.Password.Any(char.IsDigit))
            //    {
            //        userDto.PasswordError = "Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol";
            //        return Task.FromResult(userDto);
            //    }
            //    if (!userDto.Password.Any(char.IsUpper))
            //    {
            //        userDto.PasswordError = "Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol";
            //        return Task.FromResult(userDto);
            //    }
            //    if (!userDto.Password.Any(char.IsLower))
            //    {
            //        userDto.PasswordError = "Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol";
            //        return Task.FromResult(userDto);
            //    }
            //    if (!checkPasswordSpecialCharacter(userDto.Password))
            //    {
            //        userDto.PasswordError = "Is Not Strong Password. Must contain minimum 8 characters and contain at least 1 lower case, 1 upper case, 1 number and 1 symbol";
            //        return Task.FromResult(userDto);
            //    }
            //}

            var isExistsUserPerson = UserRepo.Entities.FirstOrDefault(x => x.UserID != userDto.UserID && x.PersonID == userDto.PersonID).MapTo<User>();

            if (isExistsUserPerson.IsNotNull())
            {
                userDto.DuplicateUserError = "This person is already used by another user.";
                return Task.FromResult(userDto);
            }

            var isExistsName = UserRepo.Entities.FirstOrDefault(x => x.UserID != userDto.UserID && x.PersonID == userDto.PersonID && x.UserName.ToLower() == userDto.UserName.ToLower()).MapTo<User>();
            if (isExistsName.IsNotNull())
            {
                userDto.DuplicateUserError = "This User already exists.";
                return Task.FromResult(userDto);
            }
            using (var unitOfWork = new UnitOfWork())
            {
                var securityGroupUserChilds = new List<SecurityGroupUserChild>();
                var existUser = UserRepo.Entities.SingleOrDefault(x => x.UserID == userDto.UserID).MapTo<User>();
                var person = PersonRepo.Entities.SingleOrDefault(x => x.PersonID == userDto.PersonID).MapTo<Person>();

                if (existUser.IsNull() || userDto.UserID.IsZero() || userDto.IsAdded)
                {
                    userDto.IsAdmin = false;
                    userDto.SetAdded();
                    SetNewUserID(userDto);
                }
                else
                {
                    userDto.IsAdmin = existUser.IsAdmin;
                    userDto.SetModified();
                }
                var userEnt = userDto.MapTo<User>();

                if (userDto.Password.IsNullOrEmpty() && userDto.IsModified)
                {

                    userEnt.PasswordHash = existUser.PasswordHash;
                    userEnt.PasswordSalt = existUser.PasswordSalt;
                }
                else
                {
                    CreatePasswordHash(userDto.Password, out var passwordHash, out var passwordSalt);
                    userEnt.PasswordHash = passwordHash;
                    userEnt.PasswordSalt = passwordSalt;
                    userEnt.IsForcedLogin = userDto.IsForcedLogin;
                }

                userEnt.CompanyID = userDto.CompanyID ?? AppContexts.User.CompanyID;
                if (userDto.ApplicationID.IsNull() && existUser.IsNull())
                    userEnt.DefaultApplicationID = 1;
                else if (userDto.ApplicationID.IsNull() && existUser.IsNotNull())
                    userEnt.DefaultApplicationID = existUser.DefaultApplicationID;
                else userEnt.DefaultApplicationID = userDto.ApplicationID;

                string sqlUpdate = string.Empty;
                if (userProfileDto.PositionID == 29 || userProfileDto.PositionID == 30 || userProfileDto.PositionID == 31 || userProfileDto.PositionID == 34 || userProfileDto.PositionID == 35)
                {
                    sqlUpdate = @$"Update HRMS..Employment set RegionID = {userDto.RegionID} Where EmployeeID = {userDto.EmployeeID} AND IsCurrent = 1";
                    var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
                    var result = context.ExecuteScalar(sqlUpdate);
                }
                else if (userProfileDto.PositionID == 32)
                {
                    sqlUpdate = @$"Update HRMS..Employment set ClusterID = {userProfileDto.ClusterID} Where EmployeeID = {userDto.EmployeeID} AND IsCurrent = 1";
                    var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
                    var result = context.ExecuteScalar(sqlUpdate);
                }


                //return result.ToString();

                foreach (var securityGroupChild in userDto.SecurityGroupUserChildList)
                {
                    var groupUserChild = new SecurityGroupUserChild
                    {
                        UserID = userEnt.UserID,
                        SecurityGroupID = securityGroupChild.SecurityGroupID
                    };
                    var existingGroupUserChild = SecurityGroupUserChildRepo.Entities.Where(
                            ruleChildEnt => ruleChildEnt.SecurityGroupID == groupUserChild.SecurityGroupID &&
                            ruleChildEnt.UserID == groupUserChild.UserID
                        ).FirstOrDefault();

                    if (existingGroupUserChild.IsNull())
                    {
                        groupUserChild.SetAdded();
                        SetNewSecurityGroupUserChildID(groupUserChild);
                    }
                    else if (securityGroupChild.IsModified || existingGroupUserChild.IsNotNull())
                    {
                        groupUserChild.SetModified();
                        groupUserChild.RowVersion = existingGroupUserChild.RowVersion;
                        groupUserChild.SecurityGroupUserChildID = existingGroupUserChild.SecurityGroupUserChildID;
                        groupUserChild.CreatedBy = existingGroupUserChild.CreatedBy;
                        groupUserChild.CreatedIP = existingGroupUserChild.CreatedIP;
                        groupUserChild.CreatedDate = existingGroupUserChild.CreatedDate;
                    }
                    securityGroupUserChilds.Add(groupUserChild);
                }
                var SecurityGroupUserChildList = securityGroupUserChilds.MapTo<List<SecurityGroupUserChild>>();

                #region Delete Rules 
                if (existUser.IsNotNull())
                {
                    var list = SecurityGroupUserChildRepo.Entities.Where(
                            ruleChildEnt => ruleChildEnt.UserID == existUser.UserID
                        );
                    foreach (var obj in list)
                    {
                        var exist = SecurityGroupUserChildList.Find(x => x.SecurityGroupUserChildID == obj.SecurityGroupUserChildID);
                        if (exist.IsNull())
                        {
                            obj.SetDeleted();
                            SecurityGroupUserChildList.Add(obj);
                        }
                    }
                }

                #endregion Delete Rules 


                #region Insert UserProfile
                //UserProfileDto userProfile = new UserProfileDto();
                //userProfile.UserID = userEnt.UserID;
                //userProfile.UserName = userEnt.UserName;
                //userProfile.UserFullName = person.FirstName;
                //userProfile.DistributionHouseID = 0;
                //userProfile.RegionID = userEnt.UserID;
                //userProfile.ClusterID = userEnt.UserID;
                //userProfile.PositionID = userEnt.UserID;

                //InsertUserProfileAsync(userProfileDto);
                #endregion

                //SetAuditFields(SecurityGroupUserChildList);

                UserRepo.Add(userEnt);
                //SecurityGroupUserChildRepo.AddRange(SecurityGroupUserChildList);
                unitOfWork.CommitChangesWithAudit();

                userDto.UserID = userEnt.UserID;
                //userDto.Email = person.Email;
                userDto.FullName = $@"{person.FirstName} {person.LastName}";
                //userDto.SetUnchanged();
                if (userDto.Password.IsNotNullOrEmpty())
                {
                    //var mailConfigurationDto = GetMailConfiguration().Result;
                    //var mailGroupSetup = MailGroupSetup((int)Util.MailGroupSetup.UserCreationEmailConfiguration).Result;

                    //string body = WelComeMailBody(userDto);
                    //string subject = UserCreationMailSubject(userDto);

                    //string body = WelComeMailBodyFromSetup(userDto, mailGroupSetup.Body);
                    //string subject = UserCreationMailSubject(userDto);

                    //SendMail(userDto.UserName, userDto, userDto.Password, subject, body);
                    //SendMailFromSetup(mailConfigurationDto, userDto, subject, body);
                }
            }
            return Task.FromResult(userDto);
        }

        public async Task<List<Dictionary<string, object>>> GetDistributionHouses()
        {
            string sql = string.Empty;
            if (AppContexts.User.Role.Contains(Util.UserRole.TM.ToString()))
            {
                /*
             * get tms
             * need the query from tohed
             */
                sql = $@"SELECT DISTINCT
                                U.UserID pk_user_id
                                ,U.UserName u_username                          
	                            ,CASE WHEN U.IsActive = 1 THEN 'active' ELSE 'inactive' END u_status
                                ,UP.UserFullName u_lastname
	                            ,UP.RegionID region_id
	                            ,UP.ClusterID cluster_id
	                            ,UP.ContactNumber u_contact_number
                                ,CASE WHEN UP.PositionID = 28 THEN ISNULL(UP.ParentCode, '') ELSE ISNULL(DH.ParentCode, '') END u_firstname
                                ,ISNULL(DH.UserFullName, '') tm_name
                                ,ISNULL(DH.ContactNumber, '') tm_number
                                ,ISNULL(R.RegionName, '') region_name
								,ISNULL(C.ClusterName, '') cluster_name
                                ,CASE WHEN UP.PositionID = 28 THEN ISNULL(DH.UserFullName, '') ELSE ISNULL(UDSO.UserFullName, '') END  RSM_ZSM_TM_TOName
								,CASE WHEN UP.PositionID = 28 THEN ISNULL(DH.ContactNumber, '') ELSE ISNULL(UDSO.ContactNumber, '') END RSM_ZSM_TM_TONumber
                                ,SG.SecurityGroupName PositionName
                            FROM Users U
                            LEFT JOIN UserProfile UP ON UP.UserID = U.UserID
                            LEFT JOIN UserProfile DH ON UP.DistributionHouseID = DH.UserID
							LEFT JOIN UserProfile UDSO ON DH.DistributionHouseID = UDSO.UserID
                            LEFT JOIN HRMS..Region R ON UP.RegionID = R.RegionID OR DH.RegionID = R.RegionID OR UDSO.RegionID = R.RegionID
							LEFT JOIN HRMS..Cluster C ON R.ClusterID = C.ClusterID OR UP.ClusterID = C.ClusterID
                            LEFT JOIN Security..SecurityGroupMaster SG ON SG.SecurityGroupID = UP.PositionID
							Where U.PersonID= 0 AND U.IsActive= 1 AND UP.PositionID ={(int)Util.UserRole.DH}";
            }
            else if (AppContexts.User.Role.Contains(Util.UserRole.AM.ToString()) || AppContexts.User.Role.Contains(Util.UserRole.RSM.ToString()) || AppContexts.User.Role.Contains(Util.UserRole.CH.ToString()) || AppContexts.User.Role.Contains(Util.UserRole.HQ.ToString()))
            {
                /*
                 * get all
                 */
                sql = $@"SELECT DISTINCT
                                U.UserID pk_user_id
                                ,U.UserName u_username                          
	                            ,CASE WHEN U.IsActive = 1 THEN 'active' ELSE 'inactive' END u_status
                                ,UP.UserFullName u_lastname
	                            ,UP.RegionID region_id
	                            ,UP.ClusterID cluster_id
	                            ,UP.ContactNumber u_contact_number
                                ,CASE WHEN UP.PositionID = 28 THEN ISNULL(UP.ParentCode, '') ELSE ISNULL(DH.ParentCode, '') END u_firstname
                                ,ISNULL(DH.UserFullName, '') tm_name
                                ,ISNULL(DH.ContactNumber, '') tm_number
                                ,ISNULL(R.RegionName, '') region_name
								,ISNULL(C.ClusterName, '') cluster_name
                                ,CASE WHEN UP.PositionID = 28 THEN ISNULL(DH.UserFullName, '') ELSE ISNULL(UDSO.UserFullName, '') END  RSM_ZSM_TM_TOName
								,CASE WHEN UP.PositionID = 28 THEN ISNULL(DH.ContactNumber, '') ELSE ISNULL(UDSO.ContactNumber, '') END RSM_ZSM_TM_TONumber
                                ,SG.SecurityGroupName PositionName
                            FROM Users U
                            LEFT JOIN UserProfile UP ON UP.UserID = U.UserID
                            LEFT JOIN UserProfile DH ON UP.DistributionHouseID = DH.UserID
							LEFT JOIN UserProfile UDSO ON DH.DistributionHouseID = UDSO.UserID
                            LEFT JOIN HRMS..Region R ON UP.RegionID = R.RegionID OR DH.RegionID = R.RegionID OR UDSO.RegionID = R.RegionID
							LEFT JOIN HRMS..Cluster C ON R.ClusterID = C.ClusterID OR UP.ClusterID = C.ClusterID
                            LEFT JOIN Security..SecurityGroupMaster SG ON SG.SecurityGroupID = UP.PositionID
							Where U.PersonID= 0 AND   U.IsActive= 1 AND UP.PositionID ={(int)Util.UserRole.DH}";

            }
            var list = sql.IsNullOrEmpty() ? new List<Dictionary<string, object>>() : UserRepo.GetDataDictCollection(sql).ToList();
            return list;
        }



        public async Task<List<SaveFileDescription>> GetExportUserList(string parameters)
        {
            string dateFilterCondition = string.Empty;
            string where = "";
            if (!string.IsNullOrEmpty(parameters))
            {
                where = $@"AND UP.PositionID in({parameters})";

                if (parameters.Contains("29") || parameters.Contains("34") || parameters.Contains("35"))
                {
                    where = $@"AND SGUC.SecurityGroupID IN (29, 34, 35)";//tm,to,PRO
                }
                else if (parameters.Contains("30") || parameters.Contains("31") || parameters.Contains("32") || parameters.Contains("33"))
                {
                    where = $@"AND SGUC.SecurityGroupID = {parameters}";
                }
            }

            string sql = string.Empty;
            if (!string.IsNullOrEmpty(parameters) && parameters.Contains("29") || parameters.Contains("30") || parameters.Contains("31") || parameters.Contains("32") || parameters.Contains("33") || parameters.Contains("34") || parameters.Contains("35"))
            {
                sql = $@"SELECT DISTINCT
							ROW_NUMBER() over ( order by U.UserID DESC) as #
							,ISNULL(C.ClusterName, '') Cluster
                            ,ISNULL(R.RegionName, '') Region
                            ,Emp.FullName 'Name'
							,SGM.SecurityGroupName Position
	                        ,Emp.WorkMobile 'Contact Number'
                            ,U.UserName 'Login Username'
                            ,Case When U.IsActive = 1 then 'Active' Else 'In Active' end IsActive                               
                            FROM Users U
                            LEFT JOIN Menu M ON U.DefaultMenuID=M.MenuID
							LEFT JOIN Security..SecurityGroupUserChild SGUC ON SGUC.UserID = U.UserID
							LEFT JOIN Security..SecurityGroupMaster SGM ON SGM.SecurityGroupID = SGUC.SecurityGroupID
							LEFT JOIN HRMS..Employee Emp ON Emp.PersonID = U.PersonID
							LEFT JOIN HRMS..Employment Em ON Em.EmployeeID = Emp.EmployeeID AND IsCurrent = 1
		                    LEFT JOIN HRMS..Region R ON Em.RegionID = R.RegionID
							LEFT JOIN HRMS..Cluster C ON Em.ClusterID = C.ClusterID
							Where U.PersonID <> 0 {where}
                            ";
            }
            else if (parameters.Contains("28"))
            {
                sql = $@"SELECT DISTINCT
							ROW_NUMBER() over ( order by U.UserID DESC) as #
                            ,ISNULL(C.ClusterName, '') Cluster
                            ,ISNULL(R.RegionName, '') Region
                            ,ISNULL(Emp.FullName, '') RSM_ZSM_TM_TO
						    ,ISNULL(Emp.WorkMobile, '') 'RSM_ZSM_TM_TO Number'
						    ,ISNULL(UP.ParentCode, '') 'DH Parent Code'	
                            ,UP.UserFullName 'DH Name' 
                            ,RIGHT('0000000000' + ISNULL(UP.ContactNumber, ''), 10) 'DH Wallet'
                            ,Case When UP.VisitTypeID = 1 then 'Metro' When UP.VisitTypeID = 2 then 'NonMetro' end Type
                            ,UP.JoiningDate 'Join Date'
                            ,'' 'Resign Date'
                            ,U.UserName 'Login Username'
                            ,Case When U.IsActive = 1 then 'Active' Else 'In Active' end IsActive

                            FROM Users U
							LEFT JOIN UserProfile UP ON UP.UserID = U.UserID
							LEFT JOIN Users RSMTMTO ON RSMTMTO.UserID = Up.DistributionHouseID
                            LEFT JOIN UserProfile DH ON DH.UserID = UP.DistributionHouseID
                            LEFT JOIN Users DSO ON DSO.UserID = DH.DistributionHouseID
							LEFT JOIN HRMS..Employee Emp ON Emp.PersonID = RSMTMTO.PersonID OR Emp.PersonID = DSO.PersonID
							LEFT JOIN HRMS..Employment Em ON em.EmployeeID = Emp.EmployeeID AND IsCurrent = 1
							LEFT JOIN HRMS..Region R ON R.RegionID = Em.RegionID 
							LEFT JOIN HRMS..Cluster C ON Em.ClusterID = C.ClusterID
                            LEFT JOIN Security..SecurityGroupMaster SG ON SG.SecurityGroupID = UP.PositionID
							Where U.PersonID= 0 {where} 
                            ";
            }
            else
            {
                sql = $@"SELECT DISTINCT
						    ROW_NUMBER() over ( order by U.UserID DESC) as #
                            ,ISNULL(C.ClusterName, '') Cluster
                            ,ISNULL(R.RegionName, '') Region
                            ,ISNULL(Emp.FullName, '') RSM_ZSM_TM_TO
						    ,ISNULL(Emp.WorkMobile, '') 'RSM_ZSM_TM_TO Number'
						    ,CASE WHEN UP.PositionID = 28 THEN ISNULL(UP.ParentCode, '') ELSE ISNULL(DH.ParentCode, '') END AS 'DH Parent Code'
		                    ,ISNULL(DH.UserFullName, '') 'DH Name'
                            ,ISNULL(DH.ContactNumber, '') 'DH Wallet'
                            ,UP.UserFullName Name
                            ,UP.ContactNumber 'Mobile Number'
                            ,Case When UP.VisitTypeID = 1 then 'Metro' When UP.VisitTypeID = 2 then 'NonMetro' end Type
                            ,UP.JoiningDate 'Join Date'
                            ,'' 'Resign Date'
                            ,U.UserName 'Login Username'
                            ,Case When U.IsActive = 1 then 'Active' Else 'In Active' end IsActive

                            FROM Users U
							LEFT JOIN UserProfile UP ON UP.UserID = U.UserID
							LEFT JOIN Users RSMTMTO ON RSMTMTO.UserID = Up.DistributionHouseID
                            LEFT JOIN UserProfile DH ON DH.UserID = UP.DistributionHouseID
                            LEFT JOIN Users DSO ON DSO.UserID = DH.DistributionHouseID
							LEFT JOIN HRMS..Employee Emp ON Emp.PersonID = RSMTMTO.PersonID OR Emp.PersonID = DSO.PersonID
							LEFT JOIN HRMS..Employment Em ON em.EmployeeID = Emp.EmployeeID AND IsCurrent = 1
							LEFT JOIN HRMS..Region R ON R.RegionID = Em.RegionID 
							LEFT JOIN HRMS..Cluster C ON Em.ClusterID = C.ClusterID
                            LEFT JOIN Security..SecurityGroupMaster SG ON SG.SecurityGroupID = UP.PositionID
							Where U.PersonID= 0 {where} 
                            ";
            }

            //var data = UserRepo.GetDataModelCollection<UserDto>(sql);
            var data = UserRepo.GetDataDictCollection(sql);

            //For Load Testing
            //sql = @"select (ROW_NUMBER() OVER (ORDER BY  f.fuid)) AS ""RowId"", f.fuid, f.file_name, f.original_name, f.table_name from file_upload f";
            //var data = AttachmentRepo.GetDataModelCollection<FileTest_Dto>(sql);
            //var fileDetails = await UploadUtil.SaveCSVFileInDisk(data.ToList(), "user-csv");

            string position = parameters.Contains("29,34,35") ? "TO_TM_PRO" : GetPosition(Convert.ToInt32(parameters));

            var fileDetails = await UploadUtil.SaveCSVFileInDiskDictionary(data.ToList(), position);
            List<SaveFileDescription> fileDescList = new List<SaveFileDescription>();
            fileDescList.Add(fileDetails);
            return fileDescList;
        }


        public string GetPosition(int value)
        {
            switch (value)
            {
                case 20: return "MC";
                case 21: return "BP";
                case 22: return "UDDOKTAS";
                case 23: return "TMRS";
                case 24: return "DSS";
                case 25: return "DM";
                case 26: return "BDO";
                case 27: return "DSO";
                case 28: return "distribution_houses";
                case 29: return "TO";
                case 30: return "AM";
                case 31: return "RSM";
                case 32: return "CH";
                case 33: return "HQ";
                case 34: return "TM";
                case 35: return "PRO";
                default: throw new ArgumentException("Invalid position value.");
            }
        }

        public async Task<(bool, string)> UploadPrismUser(PrismUser_Dto dto)
        {
            bool status = false;
            string msg = "Prism User File Upload Failed";

            #region Save File in the Disk

            SaveFileDescription fileDesc = new SaveFileDescription();
            string filePath = string.Empty;

            if (dto.File != null)
            {
                try
                {
                    fileDesc = await UploadUtil.SaveFileInDisk(dto.File, "prism-users");
                    filePath = fileDesc.FullPath;

                    if (!fileDesc.FileExtention.Equals(".csv"))
                    {
                        File.Delete(filePath);
                    }
                    var prismUserList = new GenericParse<PrismUserFileUpload_Dto>().GetCSVDataToModel(dto.File, "prism-users").Result;

                    if (prismUserList.Count > 0 && dto.UserPositionID > 0)
                    {
                        prismUserList.Where(e => e.Position_ID == 0).Select(d => d.Position_ID = dto.UserPositionID).ToList();
                        var prismUserDataTable = new GenericParse<PrismUserFileUpload_Dto>().GetListToDataTable(prismUserList).Result;

                        string[] colNames = { "DH_Wallet", "Name", "Wallet", "Password", "Join_Date", "Area", "Position_ID" };

                        var uploadStatus = prismUserDataTable.Rows.Count > 0 ?
                                            UserRepo.BulkInsertFromDataTableToMSSQL(tableName: "PRISM_USERS_BLUK_UPLOAD",
                                                                                dataTable: prismUserDataTable,
                                                                                isTruncateTable: false,
                                                                                dbColumnName: colNames,
                                                                                inputColumnName: colNames) : false;
                        if (uploadStatus)
                        {
                            status = true;
                            msg = "Prism User Uploded Successfully.";
                        }
                    }


                }
                catch (Exception ex)
                {
                    File.Delete(filePath);
                }
            }
            #endregion

            await Task.CompletedTask;

            return (status, msg);
        }

        public async Task<List<Dictionary<string, object>>> GetBlackListToken(string token)
        {
            string commentSql = @$"SELECT * FROM UserTokenBlackList WHERE Token='{token}'";

            var list = UserRepo.GetDataDictCollection(commentSql).ToList();

            return await Task.FromResult(list);

        }


    }
}