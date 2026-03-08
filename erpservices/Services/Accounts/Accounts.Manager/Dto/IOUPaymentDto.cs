using System;
using System.Collections.Generic;
using System.Text;

namespace Accounts.Manager.Dto
{
    public class IOUPaymentDto
    {
        public long PaymentMasterID { get; set; }
        public string ReferenceKeyword { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal GrandTotal { get; set; }
        public int ApprovalProcessID { get; set; }
        public List<IOUClaimFilterdData> Details { get; set; }

    }

}
