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
    [AutoMap(typeof(OnboardingUser)),Serializable]
    public class OnboardingUserDto : Auditable
    {
        public int UserID { get; set; }
        [JsonIgnore]
        public int LogedID { get; set; }


        public string UserName { get; set; }
        
        
        public string FullName { get; set; }
        
        
        public string Email { get; set; }
        
        
        public byte[] PasswordHash { get; set; }
        
        
        public byte[] PasswordSalt { get; set; }
        
        public bool IsActive { get; set; }
        public bool IsSubmit { get; set; }
        public int? PersonID { get; set; }
        public bool IsForcedLogin { get; set; }

        public string Password { get; set; }

        public string Division { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public string MobileNo { get; set; }
        public string Location { get; set; }
        public bool IsSentInvitationClicked { get; set; }
        public string DuplicateError { get; set; }
        public string DepartmentName { get; set; }
        public string DivisionName { get; set; }
        public string DesignationName { get; set; }
        public int DivisionID { get; set; }
        public int DepartmentID { get; set; }
        public int DesignationID { get; set; }
        public string CompanyShortCode { get; set; }
        public string WorkMobile { get; set; }
        public bool IsAdmin { get; set; }
        public int ApplicationID { get; set; }
        public string CompanyName { get; set; }
        public string ShortName { get; set; }
        public string ImagePath { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeCode { get; set; }


    }
}
