using DAL.Core.EntityBase;
using HRMS.DAL.Entities;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    [AutoMap(typeof(Designation)), Serializable]
    public class DesignationDto:Auditable
    {

        public int DesignationID { get; set; }
        public string DesignationName { get; set; }
        public string DesignationCode { get; set; }
        public bool IsRemovable { get; set; }
        public string DesignationNameError { get; set; }
        public string DesignationCodeError { get; set; }
    }
}
