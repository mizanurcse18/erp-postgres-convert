
using Approval.DAL.Entities;
using Core;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Approval.Manager.Dto
{
    
    public class ReplaceOrProxyForPendingListDto
    {
        public int ReplacingEmployeeID { get; set; }
        public string ReplacingEmployeeName { get; set; }
        public int ReplacedEmployeeID { get; set; }
        public string ReplacedEmployeeName { get; set; }
        public bool IsReplaced { get; set; }
        public string ReplaceProxyRemarks { get; set; }
        public int APEmployeeFeedbackRemarksID { get; set; }
        public List<ComboModel> ProxyEmployeeForPendingList { get; set; }

    }
}
