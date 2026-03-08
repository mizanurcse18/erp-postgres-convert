using Accounts.DAL.Entities;
using Core;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Accounts.Manager
{
    [AutoMap(typeof(TaxationVettingMaster)), Serializable]
    public class TaxationVettingMasterDto : Auditable
    {
        public long TVMID { get; set; }

        public DateTime TVMDate { get; set; }
        public long InvoiceMasterID { get; set; }
        
        
        public long PRMasterID { get; set; }
        
        
        public long POMasterID { get; set; }


        public bool IsDraft { get; set; }
        public int VATRebatableID { get; set; }
        
        
        public decimal VATRebatablePercent { get; set; }
        
        
        public decimal VATRebatableAmount { get; set; }
        
        
        public long VDSRateID { get; set; }
        
        
        public decimal VDSRatePercent { get; set; }
        
        
        public decimal VDSAmount { get; set; }
        
        
        public int TDSMethodID { get; set; }
        public long TDSRateID { get; set; }


        public decimal TDSRate { get; set; }
        
        
        public decimal TDSAmount { get; set; }
        
        
        public String ReferenceNo { get; set; }
        
        public String ReferenceKeyword { get; set; }
        
        
        public int ApprovalStatusID { get; set; }
        
        
        public decimal GrandTotal { get; set; }
        public int ApprovalProcessID { get; set; } = 0;

        public string Remarks { get; set; }
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();

        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public string ApprovalStatus { get; set; }
        public string AmountInWords { get; set; }
        public string ImagePath { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string POEmployeeCode { get; set; }
        public string POEmployeeName { get; set; }
        public bool IsCurrentAPEmployee { get; set; }
        public int APEmployeeFeedbackID { get; set; }
        public int APForwardInfoID { get; set; }
        public bool IsEditable { get; set; }
        public bool IsReassessment { get; set; }
        public bool IsReturned { get; set; }
        public int DivisionID { get; set; }
        public string DivisionName { get; set; }
    }

    public class Attachment
    {
        public string AID { get; set; }
        public int FUID { get; set; }
        public int ID
        {
            get
            {
                int fuid;
                if (int.TryParse(AID, out fuid))
                {
                    return fuid;
                }
                else
                {
                    return 0;
                }
            }
        }
        public string AttachedFile { get; set; }
        public string Type { get; set; }
        public string OriginalName { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public int ReferenceId { get; set; }
        public decimal Size { get; set; }
        public string Description { get; set; }

    }

    public class MaterialReceiveDto
    {
        public long MRID { get; set; }


        public long QCMID { get; set; }


        public string ReferenceNo { get; set; }
        public string GRNNo { get; set; }

        public string ReferenceKeyword { get; set; }


        public DateTime MRDate { get; set; }


        public long WarehouseID { set; get; }


        public long SupplierID { get; set; }

        public long PRMasterID { get; set; }

        public long POMasterID { get; set; }

        public decimal TotalReceivedQty { get; set; }

        public decimal TotalReceivedAmount { get; set; }

        public decimal TotalReceivedAvgRate { get; set; }

        public int ApprovalStatusID { get; set; }

        public string BudgetPlanRemarks { get; set; }

        public string ChalanNo { get; set; }


        public DateTime ChalanDate { get; set; }

        public bool IsDraft { get; set; }
        public decimal TotalVatAmount { get; set; }

        public decimal TotalWithoutVatAmount { get; set; }

        public int InventoryTypeID { get; set; }
        public List<MRItemDetails> MRItemDetails { get; set; }
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();

        public int ApprovalProcessID { get; set; } = 0;

    }
    public class MRItemDetails
    {
        public long MRCID { get; set; }


        public long MRID { get; set; }
        public int POCID { get; set; }

        public long ItemID { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public string UnitName { get; set; }


        public Decimal ReceiveQty { get; set; }


        public Decimal ItemRate { get; set; }


        public Decimal TotalAmount { get; set; }
        public long QCCID { get; set; }
        public decimal TotalAmountIncludingVat { get; set; }

        public decimal VatAmount { get; set; }
        public decimal POVatPercent { get; set; }

    }



    #region SccModel
    public class SCCMasterDto
    {
        public long SCCMID { get; set; }
        public long SCCCID { get; set; }

        public string ReferenceNo { get; set; }

        public string ReferenceKeyword { get; set; }


        public long SupplierID { get; set; }

        public long PRMasterID { get; set; }

        public long POMasterID { get; set; }

        public string InvoiceNoFromVendor { get; set; }

        public DateTime InvoiceDateFromVendor { get; set; }


        public decimal InvoiceAmountFromVendor { get; set; }

        public DateTime? ServicePeriodFrom { get; set; }

        public DateTime? ServicePeriodTo { get; set; }

        public int PaymentType { get; set; }

        public string PaymentFixedOrPercent { get; set; }


        public decimal PaymentFixedOrPercentAmount { get; set; }


        public decimal PaymentFixedOrPercentTotalAmount { get; set; }


        public decimal SCCAmount { get; set; }

        public bool IsDraft { get; set; }


        public bool PerformanceAssessment1 { get; set; }

        public bool PerformanceAssessment2 { get; set; }

        public bool PerformanceAssessment3 { get; set; }

        public bool PerformanceAssessment4 { get; set; }

        public bool PerformanceAssessment5 { get; set; }

        public bool PerformanceAssessment6 { get; set; }

        public string PerformanceAssessmentComment { get; set; }

        public int Lifecycle { get; set; }

        public string LifecycleComment { get; set; }
        public decimal TotalReceivedQty { get; set; }

        public int ApprovalStatusID { get; set; }
        public List<SCCItemDetails> SCCItemDetails { get; set; }
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();
        //public List<ManualApprovalPanelEmployeeDto> SCCApprovalPanelList { get; set; }
        public int ApprovalProcessID { get; set; } = 0;
        public decimal TotalVatAmount { get; set; } = 0;
        public decimal TotalReceivedAmount { get; set; } = 0;
        public decimal TotalWithoutVatAmount { get; set; } = 0;
        public DateTime? SccDate { get; set; }

    }

    public class SCCItemDetails
    {
        public long SCCCID { get; set; }
        public long SCCMID { get; set; }
        public long ItemID { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public string UnitName { get; set; }
        public int POCID { get; set; }
        public Decimal InvoiceAmount { get; set; }
        public Decimal ItemRate { get; set; }
        public Decimal POVatPercent { get; set; }
        public Decimal VatAmount { get; set; }
        public Decimal TotalAmount { get; set; }
        public Decimal TotalAmountIncludingVat { get; set; }
        public Decimal ReceivedQty { get; set; }
        public DateTime? DeliveryOrJobCompletionDate { get; set; }

        public string SCCCNote { get; set; }
        public Decimal Rate { get; set; }

    }


    #endregion






}
