using HRMS.DAL.Entities;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using Manager.Core.CommonDto;

namespace HRMS.Manager.Dto
{
    public class RequestSupportDto
    {
        public int RSMID { get; set; }
        public int SupportTypeID { get; set; }
        public string UOM { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeDetails { get; set; }
        public string LocationOrFloor { get; set; }
        public string LocationName { get; set; }
        public DateTime? NeededByDate { get; set; }
        public string RemarksOrCommentsOrPurpose { get; set; }
        public string AdminRemarks { get; set; }
        public string SettlementRemarks { get; set; }
        public string ReferenceNo { get; set; }
        public string ReferenceKeyword { get; set; }
        public int ApprovalStatusID { get; set; }
        public bool IsCurrentAPEmployee { get; set; }
        public bool IsReassessment { get; set; }
        public bool IsReturned { get; set; }
        public bool IsDraft { get; set; }
        public bool IsEditable { get; set; }
        public List<Attachments> Attachments { get; set; }
        public string EmployeeNameError { get; set; }
        public int ApprovalProcessID { get; set; } = 0;
        public int ADApprovalProcessID { get; set; } = 0;
        public DateTime CreatedDate { get; set; }

        public List<ItemDetailsDto> ItemDetails { get; set; }
        public List<VehicleDetailsDto> VehicleDetails { get; set; }
        public List<FacilitiesDetailsDto> FacilitiesDetails { get; set; }
        public List<RenovationORMaintenanceDetailsDto> RenovationORMaintenanceDetails { get; set; }

        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
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

    [AutoMap(typeof(RequestSupportItemDetails)), Serializable]
    public class ItemDetailsDto : Auditable
    {
        public int RSIDID { get; set; }
        public string UOM { get; set; }
        public string UOMLabel { get; set; }
        public int RSMID { get; set; }
        public int ItemID { get; set; }
        public decimal Quantity { get; set; }
        public string Remarks { get; set; }
        public string ItemName { get; set; }
        public int InventoryTypeID { get; set; }
    }
    [AutoMap(typeof(RequestSupportVehicleDetails)), Serializable]
    public class VehicleDetailsDto : Auditable
    {
        public int RSVDID { get; set; }
        public int RSMID { get; set; }
        public DateTime TransportNeededFrom { get; set; }
        public DateTime TransportNeededTo { get; set; }
        public int TransportTypeID { get; set; }
        public int PersonQuantity { get; set; }
        public int TransportQuantity { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Duration { get; set; }
        public string Remarks { get; set; }
        public string TransportType { get; set; }
        public string ST_TIME { get; set; }
        public string END_TIME { get; set; }
        public string Total_HOUR { get; set; }

        public int FromDivisionID { get; set; }
        public int FromDistrictID { get; set; }
        public int FromThanaID { get; set; }
        public string FromArea { get; set; }
        public int ToDivisionID { get; set; }
        public int ToDistrictID { get; set; }
        public int ToThanaID { get; set; }
        public string ToArea { get; set; }
        public bool IsOthers { get; set; }
        public string Vehicle { get; set; }
        public string Driver { get; set; }
        public string ContactNumber { get; set; }

        public string FromDivisionName { get; set; }
        public string FromDistrictName { get; set; }
        public string FromThanaName { get; set; }
        public string ToDivisionName { get; set; }
        public string ToDistrictName { get; set; }
        public string ToThanaName { get; set; }
        public string VehicleDetl { get; set; }
        public string DriverDetl { get; set; }
    }
    [AutoMap(typeof(RequestSupportFacilitiesDetails)), Serializable]
    public class FacilitiesDetailsDto : Auditable
    {
        public int RSFDID { get; set; }
        public int RSMID { get; set; }
        public int SupportCategoryID { get; set; }
        public DateTime NeededByDate { get; set; }
        public string Remarks { get; set; }
        public string SupportCategoryName { get; set; }
    }
    [AutoMap(typeof(RequestSupportRenovationORMaintenanceDetails)), Serializable]
    public class RenovationORMaintenanceDetailsDto : Auditable
    {
        public int RSRMDID { get; set; }
        public int RSMID { get; set; }
        public int RenoOrMainCategoryID { get; set; }
        public DateTime NeededByDate { get; set; }
        public string Remarks { get; set; }
        public string RenoOrMainCategoryName { get; set; }
        public List<Attachments> Attachments { get; set; }
    }

}
