using Core;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using Newtonsoft.Json;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Security.Manager.Dto
{
    [AutoMap(typeof(User)), Serializable]
    public class UserDto : EntityBase
    {
        public int UserID { get; set; }
        [JsonIgnore]
        public int LogedID { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string ShortName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string CurrentPassword { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public int? ApplicationID { get; set; }
        public string CompanyID { get; set; }
        public string CompanyName { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }
        public int AccessFailedCount { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; }
        public int? PersonID { get; set; }
        public int? EmployeeID { get; set; }
        public int? DivisionID { get; set; }
        public int? DepartmentID { get; set; }
        public string EmployeeCode { get; set; }
        public string ImagePath { get; set; }
        public bool IsForcedLogin { get; set; }
        public string DivisionName { get; set; }
        public string DepartmentName { get; set; }
        public string DesignationName { get; set; }
        public string ForgotPasswordToken { get; set; }
        public DateTime? TokenValidityTime { get; set; }
        public int? DefaultMenuID { get; set; }

        public string MenuURL { get; set; }
        public string Url { get; set; }
        public string DuplicateUserError { get; set; }
        public string PasswordError { get; set; }
        public string CurrentPasswordError { get; set; }
        public string ConfirmPasswordError { get; set; }
        public int? DesignationID { get; set; }
        public string CompanyShortCode { get; set; }
        public string WorkMobile { get; set; }
        public bool IsLocked { get; set; }
        public int? ReasonID { get; set; }
        public DateTime? LockedDateTime { get; set; }
        public DateTime? ChangePasswordDatetime { get; set; }
        public string Reason { get; set; }
        public string Role { get; set; }
        public int RegionID { get; set; }
        public List<SecurityGroupUserChildDto> SecurityGroupUserChildList { get; set; }
        public List<ComboModel> SecurityGroupuserChildComboList { get; set; }
        public string Longitude { get; set; } = string.Empty;
        public string Latitude { get; set; } = string.Empty;


    }
}
