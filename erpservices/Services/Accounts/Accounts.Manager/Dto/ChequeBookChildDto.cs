using System;
using System.Collections.Generic;
using System.Text;

namespace Accounts.Manager.Dto
{
    public class ChequeBookChildDto
    {
        public int CBCID { get; set; }
        public int CBID { get; set; }
        public int LeafNo { get; set; }
        public bool IsActiveLeaf { get; set; }
        public bool IsUsed { get; set; }
    }
}
