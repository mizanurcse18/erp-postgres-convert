using System;
using System.Collections.Generic;
using System.Text;

namespace SCM.Manager.Dto
{

    public class InvoiceFilteredData
    {
        public long PaymentChildID { get; set; }
        public long IPaymentMasterID { get; set; }
        public long TVPID { get; set; }
        public long InvoiceMasterID { get; set; }
        public long SupplierID { get; set; }
        public int PaymentModeID { get; set; }
        public long POMasterID { get; set; }
        public long PRMasterID { get; set; }
        public long WarehouseID { get; set; }
        public string InvoiceNo { get; set; }
        public string PONo { get; set; }
        public string PRNo { get; set; }        
        public decimal TotalPayableAmount{ get; set; }
        public DateTime InvoiceDate { get; set; }
        public string InvoiceDateString { get { return InvoiceDate.ToShortDateString(); } }
        public DateTime InvoiceReceiveDate { get; set; }
        public string InvoiceReceiveDateString { get { return InvoiceReceiveDate.ToShortDateString(); } }
        public DateTime? AccountingDate { get; set; }
        public string AccountingDateString { get { return InvoiceReceiveDate.ToShortDateString(); } }
        public string SupplierName { get; set; }
        public string InvoiceTypeName { get; set; }
        public string ReferenceNo { get; set; }
        public bool IsAdvanceInvoice { get; set; }

        public decimal InvoiceAmount { get; set; }
        public decimal NetPayableAmount { get; set; }
        public decimal CustomDeduction { get; set; }
        public long TVMID { get; set; }

        public string BankName { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankBranch { get; set; }
        public string BINNumber { get; set; }
        public string RoutingNumber { get; set; }
        public string SwiftCode { get; set; }
        public int TDSMethodID { get; set; }
        public decimal VDSRatePercent { get; set; }
        public int VDSRateID { get; set; }
        public decimal TDSRatePercent { get; set; }
        public decimal VDSAmount { get; set; }
        public decimal TDSAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal GrossAmount { get; set; }

        //Supplier Bank Info
        public string SupplierBank { get; set; }
        public string SupplierBankAccName { get; set; }
        public string SupplierAccNumber { get; set; }
        public string SuplierBranch { get; set; }
        public string SupplierBINNo { get; set; }
        public string SupplierRoutingNo { get; set; }
        public string SupplierSwiftCode { get; set; }
        public int ApprovalProcessID { get; set; }
        public int PRApprovalProcessID { get; set; }


    }
}
