using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;

namespace SCM.Manager.Dto
{
    public class MaterialReceiveDto
    {
        public long MRID { get; set; }


        public long QCMID { get; set; }


        public string ReferenceNo { get; set; }
        public string GRNNo { get; set; }
        public string QCMasterRefNo { get; set; }

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
        public List<ManualApprovalPanelEmployeeDto> GRNApprovalPanelList { get; set; }
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();

        public int ApprovalProcessID { get; set; }=0;

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
        public string QCMasterRefNo { get; set; }

    }
    
}
