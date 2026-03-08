using System;
using System.Collections.Generic;
using System.Text;

namespace Manager.Core.CommonDto
{
    public class ItemDetailsSR
    {
        public int RSIDID { get; set; }
        public int ItemID { get; set; }
        public Decimal Quantity { get; set; }
        public int RSMID { get; set; }
        public string Remarks { get; set; }
       

    }
}
