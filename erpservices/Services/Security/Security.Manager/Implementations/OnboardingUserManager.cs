using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.EntityBase;
using DAL.Core.Repository;
using Manager.Core;
using Manager.Core.CommonDto;
using Manager.Core.Mapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Security.DAL.Entities;
using Security.Manager.Dto;
using Security.Manager.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Core.Util;

namespace Security.Manager.Implementations
{
    public class OnboardingUserManager : ManagerBase, IOnboardingUserManager
    {
        private readonly IRepository<OnboardingUser> UserRepo;
        private readonly IRepository<User> MainUserRepo;
        private readonly IRepository<UserLogTracker> LogRepo;

        private readonly IModelAdapter Adapter;
        private readonly IRepository<Person> PersonRepo;
        private readonly IRepository<PersonImage> PersonImageRepo;

        readonly IRepository<PersonAddressInfo> PersonAddressInfoRepo;
        readonly IRepository<PersonWorkExperience> PersonWorkExperienceRepo;
        readonly IRepository<PersonAcademicInfo> PersonAcademicInfoRepo;
        readonly IRepository<PersonTrainingInfo> PersonTrainingInfoRepo;
        readonly IRepository<PersonAwardInfo> PersonAwardInfoRepo;
        readonly IRepository<PersonFamilyInfo> PersonFamilyInfoRepo;
        readonly IRepository<PersonReferenceInfo> PersonReferenceInfoRepo;
        readonly IRepository<NomineeInformation> NomineeInformationRepo;
        readonly IRepository<PersonEmergencyContactInfo> PersonEmergencyContactInfoRepo;
        private readonly IRepository<SystemVariable> SystemVariableRepo;
        private readonly IRepository<District> DistrictRepo;
        private readonly IRepository<Thana> ThanaRepo;
        readonly IRepository<EmployeeProfileApproval> PersonProfileApprovalRepo;

        private readonly IRepository<UserThemeSetting> UserThemeSettingRepo;
        readonly IEmailManager EmailManager;
        private SmtpClient client = new SmtpClient();
        //private readonly IRepository<MenuPrivilege> MenuPrivilegeRepo;

        public OnboardingUserManager(IRepository<OnboardingUser> userRepo,
            IRepository<UserLogTracker> logRepo,
            IModelAdapter adapter,
            IRepository<Person> PersonRepo,
            IRepository<PersonImage> PersonImageRepo,

            IRepository<PersonAddressInfo> personAddressInfoRepo,
            IRepository<PersonWorkExperience> personWorkExperienceRepo, 
            IRepository<PersonAcademicInfo> personAcademicInfoRepo, 
            IRepository<PersonTrainingInfo> personTrainingInfoRepo, 
            IRepository<PersonAwardInfo> personAwardInfoRepo, 
            IRepository<PersonFamilyInfo> personFamilyInfoRepo,
            IRepository<PersonReferenceInfo> personReferenceInfoRepo, 
            IRepository<PersonEmergencyContactInfo> personEmergencyContactInfoRepo,
            IRepository<NomineeInformation> nomineeInformationRepo,
            IRepository<SystemVariable> SystemVariableRepo,
            IRepository<District> districtRepo,
            IRepository<Thana> thanaRepo,
            IRepository<EmployeeProfileApproval> personProfileApprovalRepo,

            IRepository<SecurityGroupUserChild> securityGroupUserChildRepo,
            IRepository<UserThemeSetting> userThemeSetting,
            IEmailManager emailManager,
            IRepository<User> mainUserRepo
            //IRepository<MenuPrivilege> menuPrivilegeRepo
            )
        {
            UserRepo = userRepo;
            LogRepo = logRepo;
            Adapter = adapter;
            this.PersonRepo = PersonRepo;
            this.PersonImageRepo = PersonImageRepo;
            PersonAddressInfoRepo = personAddressInfoRepo;
            PersonWorkExperienceRepo = personWorkExperienceRepo;
            PersonAcademicInfoRepo = personAcademicInfoRepo;
            PersonTrainingInfoRepo = personTrainingInfoRepo;
            PersonAwardInfoRepo = personAwardInfoRepo;
            PersonFamilyInfoRepo = personFamilyInfoRepo;
            PersonReferenceInfoRepo = personReferenceInfoRepo;
            PersonEmergencyContactInfoRepo = personEmergencyContactInfoRepo;
            NomineeInformationRepo = nomineeInformationRepo;
            this.SystemVariableRepo = SystemVariableRepo;
            DistrictRepo = districtRepo;
            ThanaRepo = thanaRepo;
            PersonProfileApprovalRepo = personProfileApprovalRepo;
            EmailManager = emailManager;
            UserThemeSettingRepo = userThemeSetting;
            MainUserRepo = mainUserRepo;
            //MenuPrivilegeRepo = menuPrivilegeRepo;
        }

        public UserThemeSetting GetSettings(int userid)
        {

            var setting = UserThemeSettingRepo.SingleOrDefault(x => x.UserID == userid);
            if (setting.IsNull()) return null;
            return setting;
        }

        public async Task<List<UserListDto>> GetUsers()
        {
            var users = await UserRepo.GetAllListAsync();
            return users.MapTo<List<UserListDto>>();
        }

        public async Task<OnboardingUserDto> GetUser(int userId)
        {
            var user = await UserRepo.GetAsync(userId);
            return user.MapTo<OnboardingUserDto>();
        }

        public async Task<OnboardingUserDto> ChangePassword(OnboardingUserDto userDto)
        {
            //var userEnt = UserRepo.Entities.SingleOrDefault(x => x.UserID == userDto.UserID).MapTo<OnboardingUser>();
            //userEnt.SetModified();
            ////var userEnt = userDto.MapTo<User>();
            //CreatePasswordHash(userDto.Password, out var passwordHash, out var passwordSalt);
            //userEnt.IsForcedLogin = false;
            //userEnt.PasswordHash = passwordHash;
            //userEnt.PasswordSalt = passwordSalt;
            //userEnt.CompanyID = userDto.CompanyID ?? AppContexts.User.CompanyID;
            //UserRepo.Add(userEnt);
            //UserRepo.SaveChangesWithAudit();
            //userDto.SetUnchanged();
            return await Task.FromResult(userDto);
        }

        public Task<OnboardingUserDto> SaveChanges(OnboardingUserDto userDto)
        {

            string query = $@"SELECT * FROM  Users WHERE LOWER(UserName) IN(SELECT LOWER(UserName) FROM OnboardingUser WHERE  LOWER(UserName) = '{userDto.UserName}')";

            var existMainUser = UserRepo.GetDataDictCollection(query);
            if (existMainUser.Count() > 0)
            {
                userDto.DuplicateError = "Regular User is created by this username.";
                return Task.FromResult(userDto);
            }

            using (var unitOfWork = new UnitOfWork())
            {
                var existUser = UserRepo.Entities.SingleOrDefault(x => x.UserID == userDto.UserID).MapTo<OnboardingUser>();

                var person = new Person { FirstName = userDto.FullName, Email = userDto.Email, PersonTypeID = (int)Util.PersonType.Onboarding };


                if (existUser.IsNull() || userDto.UserID.IsZero() || userDto.IsAdded)
                {
                    userDto.Password = GenerateRandomPassword();
                    CreatePasswordHash(userDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

                    // set generated password to user
                    userDto.PasswordHash = passwordHash;
                    userDto.PasswordSalt = passwordSalt;

                    userDto.IsActive = true;

                    userDto.SetAdded();
                    SetNewUserID(userDto);

                    person.SetAdded();
                    SetNewPersonID(person);
                    userDto.PersonID = person.PersonID;
                }
                else
                {
                    userDto.SetModified();

                    userDto.RowVersion = existUser.RowVersion;
                    userDto.CreatedBy = existUser.CreatedBy;
                    userDto.CreatedDate = existUser.CreatedDate;
                    userDto.CreatedIP = existUser.CreatedIP;

                    var existPerson = PersonRepo.Entities.SingleOrDefault(x => x.PersonID == userDto.PersonID).MapTo<Person>();
                    if (existPerson.IsNotNull())
                    {
                        person = existPerson;
                        person.FirstName = userDto.FullName;
                        person.Email = userDto.Email;
                        person.SetModified();
                    }

                }

                if (existUser.IsNull() || userDto.UserID.IsZero() || userDto.IsSentInvitationClicked)
                {
                    userDto.Password = GenerateRandomPassword();
                    CreatePasswordHash(userDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

                    // set generated password to user
                    userDto.PasswordHash = passwordHash;
                    userDto.PasswordSalt = passwordSalt;
                }
                else
                {
                    userDto.PasswordHash = existUser.PasswordHash;
                    userDto.PasswordSalt = existUser.PasswordSalt;

                }

                var userEnt = userDto.MapTo<OnboardingUser>();

                var personEnt = person.MapTo<Person>();
                PersonRepo.Add(personEnt);


                UserRepo.Add(userEnt);

                unitOfWork.CommitChangesWithAudit();

                //if (userDto.Password.IsNotNullOrEmpty())
                if (existUser.IsNull() || userDto.IsSentInvitationClicked)
                {
                    var mailConfigurationDto = GetMailConfiguration().Result;
                    var mailGroupSetup = MailGroupSetup((int)Util.MailGroupSetup.OnboardingMail).Result;

                    string body = WelComeMailBodyFromSetup(userDto, mailGroupSetup.Body);
                    string subject = mailGroupSetup.Subject; //UserCreationMailSubject(userDto);

                    SendMailFromSetup(mailConfigurationDto, userDto, subject, body);
                }
            }
            return Task.FromResult(userDto);
        }

        private async Task SendMailFromSetup(MailConfigurationDtoCore mailConfigurationDtoCore, OnboardingUserDto user, string mailSubject, string mailBody)
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
                //GeneratedPassword = user.Password,
                Subject = mailSubject,
                EmailBody = mailBody,
                IsBodyHtml = true
            };
            client = SetMailServerConfiguration(mailConfigurationDtoCore).Result;
            await SendEmail(resetPasswordEmailData, client);
        }
        public string WelComeMailBodyFromSetup(OnboardingUserDto userDto, string body)
        {
            body = body.Replace("{{FullName}}", userDto.FullName);
            body = body.Replace("{{UserName}}", userDto.UserName);
            body = body.Replace("{{PersonID}}", userDto.PersonID.ToString());
            body = body.Replace("{{Password}}", userDto.Password);
            return body;
        }


        private static string UserCreationMailSubject(OnboardingUserDto obj)
        {
            return obj.IsAdded ? @$"It's official. You're now a Nagad ERP User!" : "Trouble Logging in? I am your reset link!";
        }

        private void SetNewUserID(OnboardingUserDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("Users", AppContexts.User.CompanyID);
            obj.UserID = code.MaxNumber;
        }
        private void SetNewPersonID(Person obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("Person", AppContexts.User.CompanyID);
            obj.PersonID = code.MaxNumber;
        }

        private void SetNewSecurityGroupUserChildID(SecurityGroupUserChild obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("SecurityGroupUserChild", AppContexts.User.CompanyID);
            obj.SecurityGroupUserChildID = code.MaxNumber;
        }
        public void Delete(OnboardingUserDto userDto)
        {
            var userEnt = userDto.MapTo<OnboardingUser>();
            UserRepo.Add(userEnt);
            UserRepo.SaveChanges();
        }

        public async Task AddAsUser(int UserID)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var onboardUser = UserRepo.Entities.SingleOrDefault(x => x.UserID == UserID);
                var existMainUser = MainUserRepo.Entities.SingleOrDefault(x => x.UserID == UserID).MapTo<User>();

                if (existMainUser.IsNull())
                {
                    var user = new User
                    {
                        UserID = onboardUser.UserID,
                        UserName = onboardUser.UserName,
                        PasswordHash = onboardUser.PasswordHash,
                        PasswordSalt = onboardUser.PasswordSalt,
                        DefaultApplicationID = 1,
                        CompanyID = onboardUser.CompanyID,
                        IsAdmin = false,
                        IsActive = true,
                        InActiveDate = null,
                        AccessFailedCount = 0,
                        TwoFactorEnabled = false,
                        PhoneNumberConfirmed = false,
                        EmailConfirmed = false,
                        PersonID = onboardUser.PersonID,
                        IsForcedLogin = false
                    };
                    if (existMainUser.IsNull())
                    {
                        user.SetAdded();
                    }

                    var userEnt = user.MapTo<User>();
                    SetAuditFields(userEnt);
                    MainUserRepo.Add(userEnt);
                }

                unitOfWork.CommitChangesWithAudit();
            }
            await Task.CompletedTask;
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
	                        Cast(1 as Bit) CanCreate,
	                        Cast(1 as Bit) CanRead,
	                        Cast(1 as Bit) CanUpdate,
	                        Cast(1 as Bit) CanDelete
                        FROM Menu order by SequenceNo asc"
                    :
                    $@"SELECT 
                            *
                        FROM 
                            ViewGetAllMenuPermission
                        WHERE 
                            UserID = '{UserId}' --AND Menu.ApplicationID = '{ApplicationId}' 
                            
                        ORDER BY SequenceNo";

            var menus = UserRepo.GetDataDictCollection(sql);

            return await Task.FromResult(menus);
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
	                             U.*,
                                CASE WHEN Us.UserID IS NOT NULL
			                        THEN CAST(1 AS BIT)
		                        ELSE CAST(0 AS BIT)
		                        END IsUser,
								Case WHEN U.IsActive =1 then 'Active' ELSE 'InActive' END Active,
								Case WHEN U.IsSubmit =1 then 'Submitted' ELSE 'Not Submitted' END Submitted
                            FROM OnboardingUser U
                            LEFT JOIN Users Us ON Us.UserID=U.UserID";
            var listDict = UserRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }

        public async Task<Dictionary<string, object>> GetUserDetails(int UserID)
        {
            string sql = $@"SELECT 
	                             U.UserID
	                            ,U.UserName	                            
	                            ,U.PersonID
	                            ,P.FirstName FullName
	                            ,P.Email
                            FROM OnboardingUser U
                            INNER JOIN Person P ON P.PersonID = U.PersonID
							WHERE U.UserID  = {UserID}";
            return await Task.FromResult(UserRepo.GetData(sql));
        }

        public async Task<OnboardingUserDto> GetUserLatest(int UserID)
        {
            string sql = $@"SELECT                                  
                                 ISNULL(U.PersonID,0) PersonID
	                             ,U.*
	                            ,Img.ImagePath
	                            ,P.FirstName FullName
	                            ,P.Email
	                            ,P.Mobile  
                                ,Emp.EmployeeID
                                ,Emp.EmployeeCode
								,DepartmentID
								,DivisionID                              
                                ,ISNULL(CAST(U.UpdatedBy as int),0) UpdatedBy
                            FROM OnboardingUser U
                            LEFT JOIN Person P ON P.PersonID = U.PersonID
                            left JOIN (SELECT ImagePath,PersonID FROM PersonImage WHERE IsFavorite=1) Img
								                            ON Img.PersonID = P.PersonID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..Employee Emp ON Emp.PersonID = P.PersonID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..Employment Em ON Em.EmployeeID = Emp.EmployeeID AND Em.IsCurrent = 1
							WHERE U.UserID  = {UserID}";
            return await Task.FromResult(UserRepo.GetModelData<OnboardingUserDto>(sql));
        }

        #endregion
        public async Task<OnboardingUserDto> GetUser(string email, string userName)
        {
            string sql = $@"SELECT 
	                            Email,
	                            U.*
                            FROM 
	                            Security..Person P
	                            INNER JOIN Security..Users U ON P.PersonID = U.PersonID
	                            --LEFT JOIN HRMS..Employee Emp ON Emp.PersonID = P.PersonID
                                WHERE UserName = '{userName}' AND WorkEmail = '{email}'";

            return await Task.FromResult(UserRepo.GetData(sql).MapTo<OnboardingUserDto>());
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
                var userAdd = user.MapTo<OnboardingUser>();
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

        private async Task SendMail(string userName, OnboardingUserDto user, string generatedPassword, string mailSubject, string mailBody)
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

        public OnboardingUserDto SignIn(string userName, string password, DateTime logInDate)
        {
            if (userName.IsNullOrEmpty() || password.IsNullOrEmpty()) return null;

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

            var userDto = user.MapTo<OnboardingUserDto>();
            userDto.LogedID = log.LogedID;
            if (userDto.PersonID.IsNotNull())
            {
                var userDetail = GetUserDetails((int)userDto.UserID).Result.MapTo<OnboardingUserDto>();
                userDto.FullName = userDetail.FullName;
                userDto.PersonID = userDetail.PersonID;
                userDto.Email = userDetail.Email;
                userDto.DepartmentName = "";
                userDto.DivisionName = "";
                userDto.DesignationName = "";
                userDto.DivisionID = 0;
                userDto.DepartmentID = 0;
                userDto.DesignationID = 0;
                userDto.CompanyShortCode = "";
                userDto.WorkMobile = "";


            }

            return userDto;
        } 
    }
}
