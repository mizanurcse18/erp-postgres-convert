using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using SCM.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCM.Manager.Dto
{
    public class PRNFAMapDto:Auditable
    {

        public long PRNFAMapID { get; set; }
        public long PRMID { get; set; }
        public int? NFAID { get; set; }
        public string NFAReferenceNo { get; set; }
        public decimal NFAAmount { get; set; }
        public bool IsFromSystem { get; set; }

    }
}
