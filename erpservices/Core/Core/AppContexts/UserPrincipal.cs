using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Core.AppContexts
{
    public class UserPrincipal : ClaimsPrincipal
    {
        public int LogedID { get; set; }
        public int UserID { get; set; }
        public int PersonID { get; set; }
        public int? EmployeeID { get; set; }
        public int? DivisionID { get; set; }
        public int? DepartmentID { get; set; }
        public string EmployeeCode { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public bool IsAdmin { get; set; }
        public int ApplicationID { get; set; }
        public string CompanyID { get; set; }
        public string CompanyName { get; set; }
        public string IPAddress { get; set; }
        public DateTime LogInDateTime { get; set; }
        public string DivisionName { get; set; }
        public string DepartmentName { get; set; }
        public string DesignationName { get; set; }
        public bool CanCreate
        {
            get;
            set;
        }

        public bool CanDelete
        {
            get;
            set;
        }

        public bool CanRead
        {
            get;
            set;
        }

        public bool CanUpdate
        {
            get;
            set;
        }
        public string CompanyShortCode
        {
            get;
            set;
        }
        [MaxLength(int.MaxValue)]
        public string WorkMobile
        {
            get;
            set;
        }
        public string Email
        {
            get;
            set;
        }
        public string Role { get; set; }
    }
}
