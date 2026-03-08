using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Accounts.DAL.Entities
{
    [Table("VoucherMaster"), Serializable]
    public class VoucherMaster : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long VoucherMasterID { get; set; }

        [Loggable]
        [Required]
        public String ReferenceNo { get; set; }
        
        [Loggable]
        [Required]
        public bool IsExcelUpload { get; set; }

        [Loggable]
        [Required]
        public long VoucherTypeID { get; set; }

        [Loggable]
        [Required]
        public string Remarks { get; set; }

        [Loggable]
        [Required]
        public DateTime VoucherDate { get; set; }

        [Loggable]
        public String ReferenceKeyword { get; set; }
        [Loggable]
        [Required]
        public int ApprovalStatusID { get; set; }

        [Loggable]
        public bool IsActive { get; set; }  

    }
}
