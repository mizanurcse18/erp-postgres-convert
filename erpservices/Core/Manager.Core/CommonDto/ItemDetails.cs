using System;
using System.Collections.Generic;
using System.Text;

namespace Manager.Core.CommonDto
{
    public class ItemDetails
    {
        public int PRCID { get; set; }
        public int ItemID { get; set; }
        public string Description { get; set; }
        public int ForID { get; set; }
        public int UOM { get; set; }
        public Decimal Qty { get; set; }
        public Decimal? Price { get; set; }
        public Decimal Amount { get; set; }
        public int PRMasterID { get; set; }
        public string Remarks { get; set; }
        public int MRCID { get; set; }
        public int MRMasterID { get; set; }

    }
}
