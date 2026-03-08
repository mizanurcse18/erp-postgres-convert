using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Security.Manager
{
    [AutoMap(typeof(NFAMaster)), Serializable]
    public class NFAMasterDto:Auditable
    {
        public int NFAID { get; set; }
       
        public DateTime? NFADate { get; set; }
       
        public string ReferenceNo { get; set; }
       
        public string Subject { get; set; }
       
        public string Preamble { get; set; }
        public string Description { get; set; }
       
        public string PriceAndCommercial { get; set; }
        public string BudgetPlanCategoryName { get; set; }

        public string Solicitation { get; set; }
       
        public string BudgetPlanRemarks { get; set; }
        public int BudgetPlanCategoryID { get; set; }

        public decimal GrandTotal { get; set; }
       
        public int ApprovalStatusID { get; set; }
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public string ApprovalStatus { get; set; }
        public string ImagePath { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public int ApprovalProcessID { get; set; }
        public bool IsCurrentAPEmployee { get; set; }
        public int APEmployeeFeedbackID { get; set; }
        public int APForwardInfoID { get; set; }
        public bool IsEditable { get; set; }
        public bool IsReassessment { get; set; }
        public bool IsReturned { get; set; }
        public int DivisionID { get; set; }
        public string DivisionName { get; set; }
        public int TemplateID { get; set; }
        public string DescriptionImageURL { get; set; }
        public string ReferenceKeyword { get; set; }
        public string WorkMobile { get; set; }
        public DateTime? LastActionDate { set; get; }
        public bool IsDraft { get; set; }
    }
}
