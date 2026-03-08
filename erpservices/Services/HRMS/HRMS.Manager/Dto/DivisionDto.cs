
using DAL.Core.EntityBase;
using HRMS.DAL.Entities;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    [AutoMap(typeof(Division)), Serializable]
    public class DivisionDto : Auditable
    {
        public int DivisionID { get; set; }
        public string DivisionName { get; set; }
        public string DivisionCode { get; set; }


        public string DivisionNameError { get; set; }  
        public string DivisionCodeError { get; set; }

    }
}
