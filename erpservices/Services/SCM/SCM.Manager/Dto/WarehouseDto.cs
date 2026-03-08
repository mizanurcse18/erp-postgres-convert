
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using SCM.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCM.Manager.Dto
{
    [AutoMap(typeof(Warehouse)), Serializable]
    public class WarehouseDto : Auditable
    {
        public int WarehouseID { get; set; }
        
        public string WarehouseName { get; set; }
        public string WarehouseNameError { get; set; }

        public string ContactPerson { get; set; }
        
        public string WarehouseAddress { get; set; }
        
        public string ContactNo { get; set; }
        
        public string AuthorisePersonName { get; set; }
        
        public string AuthorisePersonDesignation { get; set; }

        public long? GLID { get; set; }

        public long? SalesReturnGLID { get; set; }
        public string GLName { get; set; }

        public string SalesReturnGLName { get; set; }

        public int AbleToID { get; set; }
        
        public int WarehouseTypeID { get; set; }
        public List<Attachments> Attachments { get; set; }

    }
}
