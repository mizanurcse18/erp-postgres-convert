using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;

namespace SCM.Manager.Dto
{
    public class SCCDto
    {

        public long SCCMID { get; set; }


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
        public List<Attachment> ProposedAttachments { get; set; } = new List<Attachment>();

        public List<ManualApprovalPanelEmployeeDto> SCCApprovalPanelList { get; set; }
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
        public decimal VatAmount { get; set; }
        public Decimal TotalAmount { get; set; }
        public decimal TotalAmountIncludingVat { get; set; }
        public Decimal ReceivedQty { get; set; }
        public DateTime? DeliveryOrJobCompletionDate { get; set; }

        public string SCCCNote { get; set; }
        public Decimal Rate { get; set; }

    }
}
