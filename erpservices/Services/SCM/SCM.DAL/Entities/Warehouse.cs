using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("Warehouse"), Serializable]
    public class Warehouse : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int WarehouseID { get; set; }
        [Required]
        [Loggable]
        public string WarehouseName { get; set; }
        [Loggable]
        public string ContactPerson { get; set; }
        [Loggable]
        public string WarehouseAddress { get; set; }
        [Loggable]
        public string ContactNo { get; set; }
        [Loggable]
        public string AuthorisePersonName { get; set; }
        [Loggable]
        public string AuthorisePersonDesignation { get; set; }
        [Loggable]
        public long? GLID { get; set; }
        [Loggable]
        public long? SalesReturnGLID { get; set; }
        [Loggable]
        public int AbleToID { get; set; }
        [Loggable]
        public int WarehouseTypeID { get; set; }
    }
}
