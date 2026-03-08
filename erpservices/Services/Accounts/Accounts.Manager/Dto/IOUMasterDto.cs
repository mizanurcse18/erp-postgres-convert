using Accounts.DAL.Entities;
using Accounts.Manager.Dto;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Accounts.Manager
{
    [AutoMap(typeof(IOUMaster)), Serializable]
    public class IOUMasterDto:Auditable
    {
        public long IOUMasterID { get; set; }
        
        
        public int EmployeeID { get; set; }

        public DateTime RequestDate { get; set; }
        
        
        public DateTime SettlementDate { get; set; }
        
        
        public string ReferenceNo { get; set; }
        
        public string ReferenceKeyword { get; set; }
        
        
        public int ApprovalStatusID { get; set; }

        public string PendingAt { get; set; }
        public string ClaimToPaymentTime { get; set; }
        public decimal GrandTotal { get; set; }
        public string DivisionName { get; set; }
        public string DepartmentName { get; set; }
        public string ApprovalStatus { get; set; }

        public int DepartmentID { get; set; }
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
        public int TemplateID { get; set; }
        public string DescriptionImageURL { get; set; }
        public List<Attachments> Attachments { get; set; }
        public List<IOUChildDto> ItemDetails { get; set; }
    }
}
