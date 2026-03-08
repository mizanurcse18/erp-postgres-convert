using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HRMS.DAL.Entities
{
    [Table("LFADeclaration")]
    public class LFADeclaration : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LFADID { get; set; }
        [Loggable]
        public string Year { get; set; }
        [Loggable]
        public int TravelType { get; set; }
        [Loggable]
        public string TravelDestination { get; set; }
        [Loggable]
        public DateTime FromDate { get; set; }
        [Loggable]
        public DateTime ToDate { get; set; }
        [Loggable]
        public int EmployeeLeaveAID { get; set; }
        [Loggable]
        public int ApprovalStatusID { get; set; }
    }
}

