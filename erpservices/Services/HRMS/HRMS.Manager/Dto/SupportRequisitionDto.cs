using HRMS.DAL.Entities;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using Manager.Core.CommonDto;

namespace HRMS.Manager.Dto
{
    public class SupportRequisitionDto
    {
        public int SRMID { get; set; }
        public int SupportCategoryID { get; set; }
        //public string UOM { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeDetails { get; set; }
        public DateTime? RequestDate { get; set; }
        public string BusinessJustification { get; set; }
        public string ITRemomandation { get; set; }
        public string ITRemarks { get; set; }
        public string SettlementRemarks { get; set; }
        public string ReferenceNo { get; set; }
        public string ReferenceKeyword { get; set; }
        public int ApprovalStatusID { get; set; }
        public bool IsCurrentAPEmployee { get; set; }
        public bool IsReassessment { get; set; }
        public bool IsReturned { get; set; }
        public bool IsDraft { get; set; }
        public bool IsOnBehalf { get; set; }
        public bool IsNewRequirements { get; set; }
        public bool IsReplacement { get; set; }
        public bool IsEditable { get; set; }
        public List<Attachments> Attachments { get; set; }
        public string EmployeeNameError { get; set; }
        public int ApprovalProcessID { get; set; } = 0;
        public int ADApprovalProcessID { get; set; } = 0;
        public DateTime CreatedDate { get; set; }

        //public List<AccessoriesItemDetailsDto> AccessoriesItemDetails { get; set; }
        public List<AssetItemDetailsDto> AssetItemDetails { get; set; }
        public List<AccessRequestDetailsDto> AccessRequestDetails { get; set; }

        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public string DesignationName { get; set; }
        public string WorkEmail { get; set; }
        public string DivisionName { get; set; }
        public string DepartmentName { get; set; }
        public string WorkMobile { get; set; }
        public string ImagePath { get; set; }
        public string EmpImagePath { get; set; }
        public string SupervisorFullName { get; set; }
        public string SupportType { get; set; }
        public string Total_HOUR { get; set; }
        public bool CanSettle { get; set; }
        public bool IsSettle { get; set; }
        public DateTime SettlementDate { get; set; }

    }

    //[AutoMap(typeof(AccessoriesRequisitionCategoryChild)), Serializable]
    //public class AccessoriesItemDetailsDto : Auditable
    //{
    //    public int AccessoriesRCCID { get; set; }
    //    public string UOM { get; set; }
    //    public string UOMLabel { get; set; }
    //    public int SRMID { get; set; }
    //    public int ItemID { get; set; }
    //    public decimal Quantity { get; set; }
    //    public string Remarks { get; set; }
    //    public string ItemName { get; set; }
    //    public int InventoryTypeID { get; set; }
    //    public int CategoryID { get; set; }
    //    //public List<Attachments> Attachments { get; set; }
    //}
    [AutoMap(typeof(AssetRequisitionCategoryChild)), Serializable]
    public class AssetItemDetailsDto : Auditable
    {
        public int AssetRCID { get; set; }
        //public string UOM { get; set; }
        //public string UOMLabel { get; set; }
        public int SRMID { get; set; }
        public int ItemID { get; set; }
        public decimal Quantity { get; set; }
        public string Remarks { get; set; }
        public string ItemName { get; set; }
        public int InventoryTypeID { get; set; }
        public int CategoryID { get; set; }
        //public List<Attachments> Attachments { get; set; }
    }


    [AutoMap(typeof(AccessRequestCategoryChild)), Serializable]
    public class AccessRequestDetailsDto : Auditable
    {
        public int AccessRCCID { get; set; }
        public int SRMID { get; set; }
        public string AccessTypesIds { get; set; }
        public string AccessTypesNeme { get; set; }
    }
  

}
