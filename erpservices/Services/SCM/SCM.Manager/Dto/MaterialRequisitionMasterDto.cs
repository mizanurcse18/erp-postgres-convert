using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using SCM.DAL.Entities;
using System;

namespace SCM.Manager
{
    [AutoMap(typeof(MaterialRequisitionMaster)), Serializable]
    public class MaterialRequisitionMasterDto:Auditable
    {
        public int MRMasterID { get; set; }
       
        public DateTime? MRDate { get; set; }
       
        public string ReferenceNo { get; set; }
       
        public string Subject { get; set; }
       
        public string Preamble { get; set; }
        public string Description { get; set; }

        public bool IsDraft { get; set; }
        public string PriceAndCommercial { get; set; }
       
        public string Solicitation { get; set; }
       
        public string BudgetPlanRemarks { get; set; }
       
        public decimal GrandTotal { get; set; }
       
        public int ApprovalStatusID { get; set; }
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public string ApprovalStatus { get; set; }
        public string AmountInWords { get; set; }
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
        public int DeliveryLocation { get; set; }
        public string SCMRemarks { get; set; }
        public string DeliveryLocationName { get; set; }
        public bool IsSingleQuotation { get; set; }
        public bool IsSCM { get; set; }
        public int CountPO { get; set; }
        public string WorkMobile { set; get; }
        public DateTime? RequiredByDate { get; set; }
        public DateTime? LastActionDate { set; get; }
        public string PODetails { set; get; }
        public decimal TotalQuotedAmount { get; set; }
    }
}
