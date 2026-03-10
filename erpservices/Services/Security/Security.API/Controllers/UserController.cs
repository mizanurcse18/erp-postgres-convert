using System;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

using Core;
using API.Core;
using Core.Extensions;
using Core.AppContexts;

using Security.Manager.Dto;
using Security.Manager.Interfaces;
using Security.API.Models;
using System.Linq;
using API.Core.Mvc;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Security.API.Controllers
{
    //[Authorize, ApiController, Route("[controller]/[action]")]
    [Authorize, ApiController, Route("[controller]")]
    public class UserController : BaseController
    {
        private readonly AppSettings appSettings;
        private readonly IUserManager Manager;


        public UserController(IUserManager manager, IOptions<AppSettings> appSettings)
        {
            Manager = manager;
            this.appSettings = appSettings.Value;
        }

        // POST: /User/SignIn
        [AllowAnonymous, HttpPost("SignIn")]
        public IActionResult SignIn([FromBody] LoginUser user)
        {
            var logInDate = DateTime.Now;
            var userLoginPolicyDto = Manager.CheckUserLoginPolicy(user.UserName, user.Password);
            if (userLoginPolicyDto.ReasonID > 0)
            {
                return OkResult(new
                {
                    userLoginPolicyDto.ReasonID,
                    userLoginPolicyDto.Message,
                    userLoginPolicyDto.LockedDateTime,
                    userLoginPolicyDto.UserAccountLockedDurationInMin
                });
            }
            var logUser = userLoginPolicyDto.User;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                   new Claim(ClaimTypes.Name, logUser.UserName),
                    new Claim("LogedID", logUser.LogedID.ToString()),
                    new Claim("UserID", logUser.UserID.ToString()),
                    new Claim("PersonID", logUser.PersonID.ToString()),
                    new Claim("IsAdmin", logUser.IsAdmin.ToString()),
                    new Claim("ApplicationID", logUser.ApplicationID.ToString()),
                    new Claim("CompanyID", logUser.CompanyID.ToString()),
                    new Claim("CompanyName", logUser.CompanyName.IsNull() ?"Nagad":logUser.CompanyName),
                    new Claim("LogInDateTime", logInDate.ToString()),
                    new Claim("IPAddress", AppContexts.GetIPAddress()),
                    new Claim("UserName",logUser.UserName ?? "Not yet set"),
                    new Claim("FullName",logUser.FullName ?? "Not yet set"),
                    new Claim("ShortName",logUser.ShortName?? "Not yet set"),
                    new Claim("ImagePath",logUser.ImagePath?? ""),
                    new Claim("IsForcedLogin",logUser.IsForcedLogin.ToString()),
                    new Claim("EmployeeID",logUser.EmployeeID.ToString() ?? "0"),
                    new Claim("EmployeeCode",logUser.EmployeeCode ?? "Not yet Set"),
                    new Claim("DivisionID",logUser.DivisionID.ToString() ?? "0"),
                    new Claim("DepartmentID",logUser.DepartmentID.ToString() ?? "0"),
                    new Claim("DivisionName",logUser.DivisionName.ToString() ?? ""),
                    new Claim("DepartmentName",logUser.DepartmentName.ToString() ?? ""),
                    new Claim("DesignationName",logUser.DesignationName.ToString() ?? ""),
                    new Claim("DesignationID",logUser.DesignationID.ToString() ?? ""),
                    new Claim("CompanyShortCode",logUser.CompanyShortCode.ToString() ?? ""),
                    new Claim("WorkMobile",logUser.WorkMobile.ToString() ?? "")

                }),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return OkResult(new
            {
                logUser.LogedID,
                logUser.UserID,
                logUser.PersonID,
                logUser.UserName,
                logUser.CompanyName,
                DateFormat = Util.SysDateFormat,
                Token = tokenString,
                logUser.FullName,
                logUser.ShortName,
                logUser.ImagePath,
                logUser.IsForcedLogin,
                logUser.EmployeeID,
                logUser.EmployeeCode,
                logUser.CompanyShortCode
            });
        }

        [AllowAnonymous, HttpPost("api/login")]
        public ActionResult Login([FromBody] dynamic requestFromBody)
        {

            if (requestFromBody != null)
            {
                var user = JsonConvert.DeserializeObject<LoginUser>(requestFromBody.ToString());
                var logInDate = DateTime.Now;
                var userLoginPolicyDto = Manager.CheckUserLoginPolicy(user.userName, user.userPassword);
                if (userLoginPolicyDto.ReasonID > 0)
                {
                    return Ok(new
                    {
                        status = Util.Status.error.ToString(),
                        msg = userLoginPolicyDto.Message,
                        reasonID = userLoginPolicyDto.ReasonID,
                        lockedDateTime = userLoginPolicyDto.LockedDateTime,
                        userAccountLockedDurationInMin = userLoginPolicyDto.UserAccountLockedDurationInMin,
                        failedCount = userLoginPolicyDto.FailedCount,
                        longitude = string.Empty,
                        latitude = string.Empty,
                        fullName = string.Empty
                    });
                }
                var logUser = userLoginPolicyDto.User;
                string AllRoles = logUser.Role;
                logUser.Role = AllRoles.Replace(",PRISM ADMIN", "");
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(appSettings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, logUser.UserName),
                    new Claim("LogedID", logUser.LogedID.ToString()),
                    new Claim("UserID", logUser.UserID.ToString()),
                    new Claim("PersonID", logUser.PersonID.ToString()),
                    new Claim("IsAdmin", logUser.IsAdmin.ToString()),
                    new Claim("ApplicationID", "0"),
                    new Claim("CompanyID", "nagad"),
                    new Claim("CompanyName", logUser.CompanyName),
                    new Claim("LogInDateTime", logInDate.ToString()),
                    new Claim("IPAddress", AppContexts.GetIPAddress()),
                    new Claim("UserName",logUser.UserName),
                    new Claim("FullName",logUser.FullName),
                    new Claim("ShortName",logUser.ShortName),
                    new Claim("ImagePath",logUser.ImagePath?? "Not yet set"),
                    new Claim("IsForcedLogin",logUser.IsForcedLogin.ToString()),
                    new Claim("EmployeeID",logUser.EmployeeID.ToString() ?? "0"),
                    new Claim("EmployeeCode",logUser.EmployeeCode ?? "Not yet Set"),
                    new Claim("DivisionID",logUser.DivisionID.ToString() ?? "0"),
                    new Claim("DepartmentID",logUser.DepartmentID.ToString() ?? "0"),
                    new Claim("DivisionName",logUser.DivisionName.ToString() ?? "Not yet set"),
                    new Claim("DepartmentName",logUser.DepartmentName.ToString() ?? "Not yet set"),
                    new Claim("DesignationName",logUser.DesignationName.ToString() ?? "Not yet set"),
                    new Claim("DesignationID",logUser.DesignationID.ToString() ?? "Not yet set"),
                    new Claim("CompanyShortCode",logUser.CompanyShortCode.ToString() ?? "Not yet set"),
                    new Claim("WorkMobile",logUser.WorkMobile.ToString() ?? "Not yet set"),
                    new Claim("Email",logUser.Email.ToString() ?? "Not yet set"),
                    new Claim("Role",logUser.Role.ToString() ?? "Not yet set")
                }),
                    Expires = DateTime.Now.AddDays(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                return Ok(new
                {
                    status = Util.Status.success.ToString(),
                    msg = Util.GetMessage(Util.Status.success),
                    userID = logUser.UserID.ToString(),
                    userToken = tokenString,
                    jwt = tokenString,
                    userName = logUser.UserName,
                    changePassword = logUser.IsForcedLogin ? "1" : "0",
                    role = logUser.Role,
                    home_location_text = "",
                    contactNumber = logUser.WorkMobile,
                    longitude = logUser.Longitude,
                    latitude = logUser.Latitude,
                    fullName = logUser.FullName
                });
            }
            return ApiCoreUtility.ResponseAPPVersion();
        }

        [AllowAnonymous, HttpPost("SignInWithMenus")]
        public async Task<IActionResult> SignInWithMenus([FromBody] LoginUser user)
        {
            var logInDate = DateTime.Now;
            var userLoginPolicyDto = Manager.CheckUserLoginPolicy(user.UserName, user.Password);
            if (userLoginPolicyDto.ReasonID > 0 && userLoginPolicyDto.ReasonID != (int)Util.LoginPolicyReason.OverSystemDays)
            {
                return OkResult(new
                {
                    userLoginPolicyDto.ReasonID,
                    userLoginPolicyDto.Message,
                    userLoginPolicyDto.LockedDateTime,
                    userLoginPolicyDto.UserAccountLockedDurationInMin,
                    userLoginPolicyDto.FailedCount

                });
            }
            var logUser = userLoginPolicyDto.User;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);


            var settings = Manager.GetSettings(logUser.UserID);


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, logUser.UserName),
                    new Claim("LogedID", logUser.LogedID.ToString()),
                    new Claim("UserID", logUser.UserID.ToString()),
                    new Claim("PersonID", logUser.PersonID.ToString()),
                    new Claim("IsAdmin", logUser.IsAdmin.ToString()),
                    new Claim("ApplicationID", logUser.ApplicationID.ToString()),
                    new Claim("CompanyID", "nagad"),
                    new Claim("CompanyName", logUser.CompanyName.IsNull() ?"Nagad":logUser.CompanyName),
                    new Claim("LogInDateTime", logInDate.ToString()),
                    new Claim("IPAddress", AppContexts.GetIPAddress()),
                    new Claim("UserName",logUser.UserName ?? "Not yet set"),
                    new Claim("FullName",logUser.FullName ?? "Not yet set"),
                    new Claim("ShortName",logUser.ShortName?? "Not yet set"),
                    new Claim("ImagePath",logUser.ImagePath?? "Not yet set"),
                    new Claim("IsForcedLogin",logUser.IsForcedLogin.ToString()),
                    new Claim("EmployeeID",logUser.EmployeeID.ToString() ?? "0"),
                    new Claim("EmployeeCode",logUser.EmployeeCode ?? "Not yet Set"),
                    new Claim("DivisionID",logUser.DivisionID.ToString() ?? "0"),
                    new Claim("DepartmentID",logUser.DepartmentID.ToString() ?? "0"),
                    new Claim("DivisionName",logUser.DivisionName.ToString() ?? "Not yet set"),
                    new Claim("DepartmentName",logUser.DepartmentName.ToString() ?? "Not yet set"),
                    new Claim("DesignationName",logUser.DesignationName.ToString() ?? "Not yet set"),
                    new Claim("DesignationID",logUser.DesignationID.ToString() ?? "Not yet set"),
                    new Claim("CompanyShortCode",logUser.CompanyShortCode.ToString() ?? "Not yet set"),
                    new Claim("WorkMobile",logUser.WorkMobile.ToString() ?? "Not yet set"),
                    new Claim("Email",logUser.Email.ToString() ?? "Not yet set"),
                    new Claim("Role",logUser.Role.ToString() ?? "Not yet set")
                }),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            var menus = await Manager.GetMenus(logUser.UserID, 2, logUser.IsAdmin);



            var hasPermissionForChangeUser = Manager.HasPermissionChangeUser(logUser.EmployeeCode);

            return OkResult(new
            {
                logUser.LogedID,
                logUser.UserID,
                logUser.PersonID,
                logUser.UserName,
                logUser.CompanyName,
                DateFormat = Util.SysDateFormat,
                Token = tokenString,
                logUser.FullName,
                logUser.ShortName,
                logUser.ImagePath,
                logUser.IsForcedLogin,
                logUser.EmployeeID,
                logUser.EmployeeCode,
                logUser.DepartmentName,
                logUser.DivisionName,
                logUser.DesignationName,
                logUser.DivisionID,
                logUser.DepartmentID,
                logUser.DesignationID,
                logUser.CompanyShortCode,
                logUser.WorkMobile,
                logUser.Email,
                //logUser.ReasonID,

                logUser.ChangePasswordDatetime,
                logUser.LockedDateTime,
                logUser.IsLocked,
                menus,
                ReasonID = userLoginPolicyDto.ReasonID,
                message = ((logUser.IsForcedLogin == true && logUser.ReasonID == (int)Util.LoginPolicyReason.OverSystemDays) ? "Reminder: Your password was last updated more than 90 days ago. For security, please consider updating it now." : ""),
                settings = settings.IsNull() ? "" : settings.Settings,
                shortcuts = settings.IsNull() ? "" : settings.ShortCuts.IsNullOrEmpty() ? "" : settings.ShortCuts,
                redirectUrl = logUser.Url.IsNotNullOrEmpty() ? logUser.Url : "",
                hasPermissionForChangeUser
            });
        }


        [AllowAnonymous, HttpPost("api/verifyToken")]
        public IActionResult verifyToken([FromBody] dynamic requestFromBody)
        {
            try
            {
                var key = Encoding.ASCII.GetBytes(appSettings.Secret);
                var handler = new JwtSecurityTokenHandler();
                var securityToken = handler.ReadJwtToken(Convert.ToString(requestFromBody?.token?.Value)) as JwtSecurityToken;
                string Longitude = "", Latitude = "";
                VerifyToken verifyToken = new VerifyToken();
                var token = verifyToken.VerifyTokenHash(Convert.ToString(requestFromBody?.token?.Value));
                var list = Manager.GetBlackListToken(token);
                if (list.Result.Count > 0)
                {
                    throw new Exception("Token is blacklisted.");
                }
                //var menus = Manager.GetMenus(AppContexts.User.UserID, AppContexts.User.ApplicationID, AppContexts.User.IsAdmin);
                //var settings = Manager.GetSettings(AppContexts.User.UserID);
                //var hasPermissionForChangeUser = false;


                if (securityToken.Payload.Count > 0)
                {
                    var latestUser = Manager.GetAPPUserData(Convert.ToInt32(Convert.ToString(securityToken.Payload["UserID"])), Convert.ToInt32(Convert.ToString(securityToken.Payload["PersonID"])));
                    Longitude = latestUser.Longitude;
                    Latitude = latestUser.Latitude;
                    string AllRoles = latestUser.Role;
                    latestUser.Role = AllRoles.Replace(",PRISM ADMIN", "");
                    //hasPermissionForChangeUser = Manager.HasPermissionChangeUser(latestUser.Result.EmployeeCode);
                    if (latestUser.IsNotNull())
                    {
                        securityToken.Payload["LogedID"] = latestUser.LogedID;
                        securityToken.Payload["UserID"] = latestUser.UserID;
                        securityToken.Payload["CompanyID"] = "nagad";
                        securityToken.Payload["PersonID"] = latestUser.PersonID;
                        securityToken.Payload["UserName"] = latestUser.UserName;
                        securityToken.Payload["FullName"] = latestUser.FullName;
                        securityToken.Payload["ShortName"] = latestUser.ShortName;
                        securityToken.Payload["ImagePath"] = latestUser.ImagePath;
                        securityToken.Payload["IsAdmin"] = latestUser.IsAdmin;
                        securityToken.Payload["IsForcedLogin"] = latestUser.IsForcedLogin;
                        securityToken.Payload["EmployeeID"] = latestUser.EmployeeID;
                        securityToken.Payload["EmployeeCode"] = latestUser.EmployeeCode;
                        securityToken.Payload["DepartmentID"] = latestUser.DepartmentID;
                        securityToken.Payload["DivisionID"] = latestUser.DivisionID;
                        securityToken.Payload["DivisionName"] = latestUser.DivisionName;
                        securityToken.Payload["DepartmentName"] = latestUser.DepartmentName;
                        securityToken.Payload["DesignationName"] = latestUser.DesignationName;
                        securityToken.Payload["DesignationID"] = latestUser.DesignationID;
                        securityToken.Payload["CompanyShortCode"] = latestUser.CompanyShortCode;
                        securityToken.Payload["WorkMobile"] = latestUser.WorkMobile;
                        securityToken.Payload["Email"] = latestUser.Email;
                        securityToken.Payload["Role"] = latestUser.Role;
                        //securityToken.Payload["settings"] = settings.IsNull() ? "" : settings.Settings;
                        //securityToken.Payload["shortcuts"] = settings.IsNull() ? "" : settings.ShortCuts.IsNullOrEmpty() ? "" : settings.ShortCuts;
                        securityToken.Payload["redirectUrl"] = latestUser.Url.IsNotNullOrEmpty() ? latestUser.Url : "";
                        //securityToken.Payload["hasPermissionForChangeUser"] = hasPermissionForChangeUser;
                    }
                    //AppContexts.User.MenuApiPaths = jsonData;
                }

                return Ok(new
                {
                    status = Util.Status.success.ToString(),
                    msg = Util.GetMessage(Util.MessageType.success),
                    userID = Convert.ToString(securityToken.Payload["UserID"]),
                    userToken = Convert.ToString(requestFromBody?.token?.Value),
                    jwt = Convert.ToString(requestFromBody?.token?.Value),
                    userName = Convert.ToString(securityToken.Payload["UserName"]),
                    changePassword = Convert.ToString(securityToken.Payload["IsForcedLogin"]) == "True" ? "1" : "0",
                    role = Convert.ToString(securityToken.Payload["Role"]),
                    home_location_text = string.Empty,
                    contactNumber = Convert.ToString(securityToken.Payload["WorkMobile"]),
                    longitude = Longitude,
                    latitude = Latitude,
                    fullName = Convert.ToString(securityToken.Payload["FullName"])
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    status = Util.Status.error.ToString(),
                    msg = Util.GetMessage(Util.MessageType.loginPasswordFailed),
                    userID = string.Empty,
                    userToken = string.Empty,
                    jwt = string.Empty,
                    userName = string.Empty,
                    changePassword = string.Empty,
                    role = string.Empty,
                    home_location_text = string.Empty,
                    contactNumber = string.Empty,
                    longitude = "",
                    latitude = "",
                });
            }

        }

        [AllowAnonymous, HttpPost("ValidateUserWithToken")]
        public IActionResult ValidateUserWithToken([FromBody] LoginUser user)
        {
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.ReadJwtToken(user.UserName) as JwtSecurityToken;
            var menus = Manager.GetMenus(AppContexts.User.UserID, AppContexts.User.ApplicationID, AppContexts.User.IsAdmin);
            var settings = Manager.GetSettings(AppContexts.User.UserID);
            var hasPermissionForChangeUser = false;


            if (securityToken.Payload.Count > 0)
            {
                var latestUser = Manager.GetUserLatest(AppContexts.User.UserID, AppContexts.User.PersonID);
                if ((latestUser.Result.IsActive.IsFalse() || latestUser.Result.IsLocked.IsTrue()) && user.TokenHash.IsNullOrEmpty())
                    return BadRequest();
                hasPermissionForChangeUser = Manager.HasPermissionChangeUser(latestUser.Result.EmployeeCode);
                if (latestUser.IsNotNull())
                {
                    securityToken.Payload["LogedID"] = latestUser.Result.LogedID;
                    securityToken.Payload["UserID"] = latestUser.Result.UserID;
                    securityToken.Payload["CompanyID"] = "nagad";
                    securityToken.Payload["PersonID"] = latestUser.Result.PersonID;
                    securityToken.Payload["UserName"] = latestUser.Result.UserName;
                    securityToken.Payload["FullName"] = latestUser.Result.FullName;
                    securityToken.Payload["ShortName"] = latestUser.Result.ShortName;
                    securityToken.Payload["ImagePath"] = latestUser.Result.ImagePath;
                    securityToken.Payload["IsAdmin"] = latestUser.Result.IsAdmin;
                    securityToken.Payload["IsForcedLogin"] = user.TokenHash.IsNullOrEmpty() ? latestUser.Result.IsForcedLogin : false;
                    securityToken.Payload["EmployeeID"] = latestUser.Result.EmployeeID;
                    securityToken.Payload["EmployeeCode"] = latestUser.Result.EmployeeCode;
                    securityToken.Payload["DepartmentID"] = latestUser.Result.DepartmentID;
                    securityToken.Payload["DivisionID"] = latestUser.Result.DivisionID;
                    securityToken.Payload["DivisionName"] = latestUser.Result.DivisionName;
                    securityToken.Payload["DepartmentName"] = latestUser.Result.DepartmentName;
                    securityToken.Payload["DesignationName"] = latestUser.Result.DesignationName;
                    securityToken.Payload["DesignationID"] = latestUser.Result.DesignationID;
                    securityToken.Payload["CompanyShortCode"] = latestUser.Result.CompanyShortCode;
                    securityToken.Payload["WorkMobile"] = latestUser.Result.WorkMobile;
                    securityToken.Payload["Email"] = latestUser.Result.Email;
                    securityToken.Payload["Role"] = latestUser.Result.Role;
                    securityToken.Payload["settings"] = settings.IsNull() ? "" : settings.Settings;
                    securityToken.Payload["shortcuts"] = settings.IsNull() ? "" : settings.ShortCuts.IsNullOrEmpty() ? "" : settings.ShortCuts;
                    securityToken.Payload["redirectUrl"] = latestUser.Result.Url.IsNotNullOrEmpty() ? latestUser.Result.Url : "";
                    securityToken.Payload["hasPermissionForChangeUser"] = hasPermissionForChangeUser;
                }
                //AppContexts.User.MenuApiPaths = jsonData;
            }
            return OkResult(new
            {
                securityToken.Payload,
                menus = menus.Result
            });
        }

        [AllowAnonymous, HttpPost("ChangeUserWithToken")]
        public IActionResult ChangeUserWithToken([FromBody] LoginUser user)
        {
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            if (!Util.ValidateWithSecretPassword(user.Password))
            {
                return BadRequestResult("Username and password are invalid.");
            }

            var latestUser = Manager.GetUserLatest(user.UserName).Result;
            var securityToken = GenerateToken(latestUser);
            return OkResult(new
            {
                SecurityToken = securityToken,
                RedirectUrl = latestUser.Url.IsNotNullOrEmpty() ? latestUser.Url : "/apps/nfa/nfaBoards",
                Util.TokenHash
            }); ;
        }
        // GET: /User/GetUsers
        //[HttpGet("GetUsers")]
        //public async Task<IActionResult> GetUsers()
        //{
        //    var users = await Manager.GetUserListDic();
        //    return OkResult(users);
        //}
        [HttpPost("GetUserListDicGrid")]
        public async Task<IActionResult> GetUserListDicGrid([FromBody] GridParameter parameters)
        {
            var model = Manager.GetUserListDicGrid(parameters);
            return OkResult(new { parentDataSource = model });
        }

        // GET: /User/GetUser/{UserID}
        [HttpGet("GetUser/{UserID:int}")]
        public async Task<IActionResult> GetUser(int UserID)
        {
            var user = await Manager.GetUser(UserID);
            user.PasswordSalt = null;
            user.PasswordHash = null;
            user.SecurityGroupuserChildComboList = await Manager.GetSecurityGroupUserChildList(UserID);
            return OkResult(user);
        }

        [HttpPost("Register")]
        public IActionResult Register([FromBody] UserDto user)
        {
            Manager.SaveChanges(user);
            return Ok(user);
        }

        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] UserDto user)
        {
            var newData = await Manager.SaveChanges(user);
            if (!string.IsNullOrEmpty(newData.DuplicateUserError)) return OkResult(newData);
            if (!string.IsNullOrEmpty(newData.PasswordError)) return OkResult(newData);

            //Manager.SaveChanges(user);
            return OkResult(user);
        }
        // POST: /User/Delete
        [HttpPost("Delete")]
        public IActionResult Delete([FromBody] UserDto user)
        {
            Manager.Delete(user);
            return Ok(new { Success = true });
        }

        //// get: /user/signout
        //[httpget("signout")]
        //public async task<actionresult> signout()
        //{
        //    await manager.signoutasync(appcontexts.user.logedid, datetime.now);
        //    return okresult();
        //}

        //// GET: /User/SignOut 2
        //[HttpGet("SignOut")]
        //public async Task<ActionResult> SignOut()
        //{
        //    await Manager.SignOutAsync(AppContexts.User.LogedID, DateTime.Now);
        //    return OkResult(); 
        //}

        // GET: /User/SignOut 3 [Add Date: 24-Aug-2023]

        [HttpGet("SignOut")]
        public async Task<ActionResult> SignOut()
        {
            //var token = ExtractTokenFromHeaders();          

            //using (var sha256 = new SHA256Managed())
            //{
            //    var tokenBytes = Encoding.UTF8.GetBytes(token);
            //    var hashBytes = sha256.ComputeHash(tokenBytes);
            //    var hashedToken = BitConverter.ToString(hashBytes);

            //    Manager.SaveHashedTokenToBlackList(hashedToken);
            //}

            VerifyToken verifyToken = new VerifyToken();
            Manager.SaveHashedTokenToBlackList(verifyToken.VerifyTokenHash(HttpContext));
            await Manager.SignOutAsync(AppContexts.User.LogedID, DateTime.Now);
            return OkResult();
        }


        [HttpPost("GetUserGroups")]
        public async Task<IActionResult> GetUserGroups([FromBody] GridParameter parameters)
        {
            //if (!HasViewPrivilege())
            //    return ValidationResult("You have no privilege to view any information.");
            var model = Manager.GetUserGroups(parameters);
            return OkResult(new { parentDataSource = model, parentKey = "SecurityGroupUserChildID" });
        }

        // POST: /User/GetUserCompanies
        [HttpPost("GetUserCompanies")]
        public async Task<IActionResult> GetUserCompanies([FromBody] GridParameter parameters)
        {
            //if (!HasViewPrivilege())
            //    return ValidationResult("You have no privilege to view any information.");
            var model = Manager.GetUserCompanies(parameters);
            return OkResult(new { parentDataSource = model, parentKey = "UserCompanyID" });  // ??
        }

        // POST: /User/Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] UserDto user)
        {
            //await Manager.CreateAsync(user);
            return CreatedResult();
        }

        //GET: /User/GetMenus
        [HttpGet("GetMenus")]
        public async Task<ActionResult> GetMenus()
        {
            var menus = await Manager.GetMenus(AppContexts.User.UserID, AppContexts.User.ApplicationID, AppContexts.User.IsAdmin);
            return OkResult(new { menus });
        }

        [HttpGet("GetMenusJson")]
        public async Task<ActionResult> GetMenusJson()
        {
            var menus = await Manager.GetMenusJson(AppContexts.User.UserID, AppContexts.User.ApplicationID);
            return OkResult(menus);
        }

        // PUT: /User/ResetPassword
        [AllowAnonymous, HttpPut("ResetPassword")]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPassword resetPassword)
        {
            await Manager.ResetPasswordAsync(resetPassword.Email, resetPassword.UserName);
            return OkResult();
        }

        [HttpPost("ChangePassword")]
        public IActionResult ChangePassword([FromBody] UserDto changePassword)
        {
            if (changePassword.Password != changePassword.ConfirmPassword) return OkResult(new { msg = "New password and Confirm password mismatch.", IsSuccess = false });
            changePassword.UserID = AppContexts.User.UserID;
            bool isMatchedCurrentPassword = Manager.CheckCurrentPasswordMatch(changePassword.CurrentPassword, changePassword);
            if (!isMatchedCurrentPassword) return OkResult(new { msg = "Current Password Mismatch.", IsSuccess = false });
            if (changePassword.CurrentPassword == changePassword.Password) return OkResult(new { msg = "You cannot reuse old password.", IsSuccess = false });
            var newData = Manager.ChangePassword(changePassword);
            //return OkResult(new { msg = "Password Changed Successfully.", IsSuccess = true });

            if (!string.IsNullOrEmpty(newData.Result.PasswordError)) return OkResult(newData.Result);
            if (!string.IsNullOrEmpty(newData.Result.ConfirmPasswordError)) return OkResult(newData.Result);
            //if (!string.IsNullOrEmpty(newData.Result.CurrentPasswordError)) return OkResult(newData.Result);

            //Manager.SaveChanges(user);
            return OkResult(changePassword);
        }

        [HttpPost("api/changePassword")]
        public IActionResult AppChangePassword([FromBody] dynamic requestFromBody)
        {
            if (requestFromBody != null)
            {
                var user = JsonConvert.DeserializeObject<UserDto>(requestFromBody.ToString());

                if (user.Password != user.ConfirmPassword) return Error(Util.Status.error.ToString(), Util.GetMessage(Util.MessageType.missMatchPassword));
                user.UserID = AppContexts.User.UserID;
                bool isMatchedCurrentPassword = Manager.CheckCurrentPasswordMatch(user.CurrentPassword, user);
                if (!isMatchedCurrentPassword) return Error(Util.Status.error.ToString(), Util.GetMessage(Util.MessageType.missMatchOldPassword));
                if (user.CurrentPassword == user.Password) return Error(Util.Status.error.ToString(), Util.GetMessage(Util.MessageType.reuseOldPassword));
                var newData = Manager.ChangePassword(user);


                if (!string.IsNullOrEmpty(newData.Result.PasswordError)) return Error(Util.Status.error.ToString(), newData.Result.PasswordError);
                if (!string.IsNullOrEmpty(newData.Result.ConfirmPasswordError)) return Error(Util.Status.error.ToString(), newData.Result.ConfirmPasswordError);

                return Success(Util.Status.success.ToString(), Util.GetMessage(Util.MessageType.successChangedPassword));
            }
            return ApiCoreUtility.ResponseAPPVersion();
        }
        [HttpGet("SaveUserSettings")]

        public async Task<IActionResult> SaveUserSettings(string settings, string shortcuts)
        {
            await Manager.SaveUserSettings(settings, shortcuts);
            return Ok(new { Success = true });
        }
        //[AllowAnonymous, HttpPost("ResetPasswordChange")]
        //public async Task<IActionResult> ResetPasswordChange([FromBody] UserDto user)
        //{
        //    var oneEmailandTime = Manager.ResetPasswordChange(user);
        //    return Ok(oneEmailandTime);
        //}

        [AllowAnonymous, HttpPost("ForgotPasswordRequest")]
        public async Task<IActionResult> ForgotPasswordRequest([FromBody] UserDto user)
        {
            // Call the Manager to handle the ForgotPasswordRequest logic
            var singleEmailAndTimes = Manager.RequestForForgotPassword(user);

            // Construct the response to include the message
            var response = new
            {
                singleEmailAndTimes,
                Message = "Success",
                ResponseCode = "OK",
                StatusCode = 200
            };

            return Ok(response);
        }



        //// User/ForgotPasswordRequest
        //[AllowAnonymous, HttpPost("ForgotPasswordRequest")]
        //public async Task<IActionResult> ForgotPasswordRequest([FromBody] UserDto user)
        //{
        //    // Call the Manager to handle the ForgotPasswordRequest logic
        //    var res = Manager.RequestForForgotPaassword(user);

        //    // Construct the response to include the message, success status, and the list of emails with last request times
        //    var response = new
        //    {
        //        msg = res.Item1, // The message returned from the Manager
        //        IsSuccess = res.Item2, // Success status
        //        remainingTime = res.Item3, // Remaining time (in seconds)
        //        allEmailsAndTimes = res.Item4 // List of all emails and their corresponding last reset request times
        //    };

        //    // Return the response wrapped in OkResult
        //    return Ok(response);
        //}


        [AllowAnonymous, HttpPost("GetUserByToken")]
        public IActionResult GetUserByToken([FromBody] UserDto user)
        {

            var res = Manager.GetUserByRequestToken(user);
            return OkResult(new { UserID = res.Item1, msg = res.Item2, IsSuccess = res.Item3 });
        }
        [AllowAnonymous, HttpPost("ResetPassword")]
        public IActionResult ResetPassword([FromBody] UserDto user)
        {

            var res = Manager.ResetPassword(user);
            return OkResult(new { msg = res.Item1, IsSuccess = res.Item2 });
        }
        [HttpGet]
        public string GenerateToken(UserDto logUser)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, logUser.UserName),
                    new Claim("LogedID", logUser.LogedID.ToString()),
                    new Claim("UserID", logUser.UserID.ToString()),
                    new Claim("PersonID", logUser.PersonID.ToString()),
                    new Claim("IsAdmin", logUser.IsAdmin.ToString()),
                    new Claim("ApplicationID", logUser.ApplicationID.ToString()),
                    new Claim("CompanyID", "nagad"),
                    new Claim("CompanyName", logUser.CompanyName.IsNull() ?"Nagad":logUser.CompanyName),
                    new Claim("LogInDateTime", DateTime.Now.ToString()),
                    new Claim("IPAddress", AppContexts.GetIPAddress()),
                    new Claim("UserName",logUser.UserName ?? "Not yet set"),
                    new Claim("FullName",logUser.FullName ?? "Not yet set"),
                    new Claim("ShortName",logUser.ShortName?? "Not yet set"),
                    new Claim("ImagePath",logUser.ImagePath?? "Not yet set"),
                    new Claim("IsForcedLogin",logUser.IsForcedLogin.ToString()),
                    new Claim("EmployeeID",logUser.EmployeeID.ToString() ?? "0"),
                    new Claim("EmployeeCode",logUser.EmployeeCode ?? "Not yet Set"),
                    new Claim("DivisionID",logUser.DivisionID.ToString() ?? "0"),
                    new Claim("DepartmentID",logUser.DepartmentID.ToString() ?? "0"),
                    new Claim("DivisionName",logUser.DivisionName.ToString() ?? "Not yet set"),
                    new Claim("DepartmentName",logUser.DepartmentName.ToString() ?? "Not yet set"),
                    new Claim("DesignationName",logUser.DesignationName.ToString() ?? "Not yet set"),
                    new Claim("CompanyShortCode",logUser.CompanyShortCode.ToString() ?? "Not yet set"),
                    new Claim("WorkMobile",logUser.WorkMobile.ToString() ?? "Not yet set"),
                    new Claim("Email",logUser.Email.ToString() ?? "Not yet set"),
                    new Claim("Role",logUser.Role.ToString() ?? "Not yet set"),
                }),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        }
        [HttpGet("GetIsUniqueUserName/{UserID:int}/{UserName}")]
        public async Task<IActionResult> GetIsUniqueUserName(int UserID, string UserName)
        {
            var isUniqueUser = Manager.GetIsUniqueUserName(UserID, UserName);

            return OkResult(new { IsUniqueUser = isUniqueUser });
        }


        //Dynamic Sql Data Showing
        //[AllowAnonymous, HttpPost("ExecuteSQL/{sqlQuery}")]
        [HttpPost("ExecuteSQL/{sqlQuery}")]
        public async Task<IActionResult> ExecuteSQL(string sqlQuery)
        {
            try
            {
                // Implement SQL execution logic here
                // Execute the SQL query and return the result
                string result = "SQL query executed successfully";
                return OkResult(result);
                //return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #region Prism User

        [HttpPost("GetPrismUserListDicGrid")]
        public async Task<IActionResult> GetPrismUserListDicGrid([FromBody] GridParameter parameters)
        {
            var model = Manager.GetPrismUserListDicGrid(parameters);
            return OkResult(new { parentDataSource = model });
        }

        [HttpGet("GetPrismUser/{PositionID:int}/{UserID:int}")]
        public async Task<IActionResult> GetPrismUser(int PositionID, int UserID)
        {
            var users = await Manager.GetPrismUser(PositionID, UserID);
            var securityGrpList = await Manager.GetSecurityGroupUserChildList(UserID);
            foreach (var user in users)
            {
                user.Remove("PasswordSalt");
                user.Remove("PasswordHash");
                user["SecurityGroupuserChildComboList"] = securityGrpList;
            }
            return OkResult(users);
        }

        [HttpPost("CreatePrismUser")]
        public async Task<IActionResult> CreatePrismUser([FromBody] UserAndProfileDto userAndProfile)
        {
            if (userAndProfile.UserProfileDto.PositionID == 29 || userAndProfile.UserProfileDto.PositionID == 30 || userAndProfile.UserProfileDto.PositionID == 31 || userAndProfile.UserProfileDto.PositionID == 32 || userAndProfile.UserProfileDto.PositionID == 33 || userAndProfile.UserProfileDto.PositionID == 34 || userAndProfile.UserProfileDto.PositionID == 35)
            {
                //var newData = await Manager.SaveChanges(userAndProfile.UserDto);
                var newData = await Manager.SaveChangesPrismAmRsmToTm(userAndProfile.UserDto, userAndProfile.UserProfileDto);
                if (!string.IsNullOrEmpty(newData.DuplicateUserError)) return OkResult(newData);
                if (!string.IsNullOrEmpty(newData.PasswordError)) return OkResult(newData);
            }
            else
            {
                var newData = await Manager.SaveChangesPrismUser(userAndProfile);
                if (!string.IsNullOrEmpty(newData.UserDto.DuplicateUserError)) return OkResult(newData);
                if (!string.IsNullOrEmpty(newData.UserDto.PasswordError)) return OkResult(newData);
            }


            //Manager.SaveChanges(user);
            return OkResult(userAndProfile);
        }

        [AllowAnonymous]
        [HttpPost("api/isConnectible")]
        public async Task<IActionResult> IsConnectible([FromBody] dynamic requestFromBody)
        {
            return Success(Util.Status.success.ToString(), "success");
        }

        #endregion

        [HttpPost("api/getMyDistributionHouses")]
        public async Task<ActionResult> GetMyDistributionHouses([FromBody] dynamic requestFromBody)
        {
            var result = new object[] { };
            if (requestFromBody != null)
            {
                var listOfDhs = Manager.GetDistributionHouses().Result;
                return Success("success", "", listOfDhs);
            }

            return Ok(result);

        }

        [HttpGet("GetExportUserList")]
        public ActionResult GetExportUserList(string WhereCondition, string FromDate, string ToDate)
        {
            var reqList = Manager.GetExportUserList(WhereCondition);
            return OkResult(reqList.Result);
        }


        [HttpPost("UploadPrismUser")]
        public IActionResult UploadPrismUser([FromForm] PrismUser_Dto dto)
        {
            var response = Manager.UploadPrismUser(dto).Result;
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }
    }


}
