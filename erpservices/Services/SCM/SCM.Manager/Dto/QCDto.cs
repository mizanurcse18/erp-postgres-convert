using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;

namespace SCM.Manager.Dto
{
    public class QCDto
    {
        public long QCMID { get; set; }

        public string ReferenceNo { get; set; }

        public string ReferenceKeyword { get; set; }

        public DateTime ReceiptDate { get; set; }

        public long WarehouseID { set; get; }

        public long SupplierID { get; set; }

        public long PRMasterID { get; set; }

        public long POMasterID { get; set; }

        public decimal TotalSuppliedQty { get; set; }

        public decimal TotalAcceptedQty { get; set; }

        public decimal TotalRejectedQty { get; set; }

        public int ApprovalStatusID { get; set; }

        public string BudgetPlanRemarks { get; set; }

        public string ChalanNo { get; set; }

        public DateTime ChalanDate { get; set; }
        public long RTVMID { get; set; }
        public bool IsDraft { get; set; }
        public List<QCItemDetails> QCItemDetails { get; set; }
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();
        public List<ManualApprovalPanelEmployeeDto> QCApprovalPanelList { get; set; }
        public int ApprovalProcessID { get; set; } = 0;

    }

    public class QCItemDetails
    {
        public long QCCID { get; set; }


        public long QCMID { get; set; }


        public long ItemID { get; set; }


        public int POCID { get; set; }


        public Decimal SuppliedQty { get; set; }


        public Decimal AcceptedQty { get; set; }


        public Decimal RejectedQty { get; set; }

        public string QCCNote { get; set; }

    }
}
