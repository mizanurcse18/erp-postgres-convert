
using DAL.Core.EntityBase;
using HRMS.DAL.Entities;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    [AutoMap(typeof(EmployeeLeaveAccount)), Serializable]
    public class EmployeeLeaveAccountDto : Auditable
    {
        public int ELAID { get; set; }
        public int FinancialYearID { get; set; }
        public int Year { get; set; }
        public int EmployeeID { get; set; }
        public string Employee { get; set; }
        public string EmployeeName { get; set; }
        public int LeaveCategoryID { get; set; }
        public string LeaveCategoryName { get; set; }
        public decimal LeaveDays { get; set; }
        public string Remarks { get; set; }
        public bool IsExists { get; set; }
        public bool IsExistsPolicy { get; set; }
        public string ErrorMessage { get; set; }
        public string EmployeeStatusName { get; set; }
        public DateTime? DateOfJoining { get; set; }
        public DateTime? ConfirmDate { get; set; }
        public List<EmployeeLeaveAccountDto> EmployeeLeaveAccountList { get; set; }
        public string EmployeeCode { get; set; }
        public string ProfilePicture { get; set; }
        public int row_num { get; set; }
        public decimal PreviousLeaveDays { get; set; }

    }
}
