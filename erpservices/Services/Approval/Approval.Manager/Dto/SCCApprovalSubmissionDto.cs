using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Approval.Manager.Dto
{
    public class SCCApprovalSubmissionDto
    {
        public int ApprovalProcessID { get; set; }
        public int APEmployeeFeedbackID { get; set; }
        public int APFeedbackID { get; set; }
        public string Remarks { get; set; }
        public int APTypeID { get; set; }
        public int ReferenceID { get; set; }
        public int ToAPMemberFeedbackID { get; set; }
        public int APForwardInfoID { get; set; }
        public bool IsEditable { get; set; }
        public List<SCCItemDetails> SCCItemDetails { get; set; } = new List<SCCItemDetails>();


        public DateTime? ServicePeriodFrom { get; set; }

        public DateTime? ServicePeriodTo { get; set; }
        public int PaymentType { get; set; }

        public string PaymentFixedOrPercent { get; set; }


        public decimal PaymentFixedOrPercentAmount { get; set; }
        public decimal TotalReceivedQty { get; set; }


        public decimal PaymentFixedOrPercentTotalAmount { get; set; }
        public decimal SCCAmount { get; set; }
        public int Lifecycle { get; set; }

        public string LifecycleComment { get; set; }
        public bool PerformanceAssessment1 { get; set; }
        public bool PerformanceAssessment2 { get; set; }
        public bool PerformanceAssessment3 { get; set; }
        public bool PerformanceAssessment4 { get; set; }
        public bool PerformanceAssessment5 { get; set; }
        public bool PerformanceAssessment6 { get; set; }
        public string PerformanceAssessmentComment { get; set; }
        public decimal InvoiceAmountFromVendor { get; set; }
        public List<Attachment> ProposedAttachments { get; set; }
    }


    public class SCCItemDetails
    {
        public long SCCCID { get; set; }
        public long SCCMID { get; set; }
        public long ItemID { get; set; }
        public int POCID { get; set; }
        public Decimal InvoiceAmount { get; set; }
        public Decimal ReceivedQty { get; set; }

        public string SCCCNote { get; set; }
        public DateTime? DeliveryOrJobCompletionDate { get; set; }
        public Decimal VatAmount { get; set; }
        public Decimal TotalAmount { get; set; }
        public Decimal TotalAmountIncludingVat { get; set; }
        public Decimal Rate { get; set; }
        public string Remarks { get; set; }

    }
}
