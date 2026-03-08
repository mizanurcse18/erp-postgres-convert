using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Security.Manager.Dto
{
    public class NotificationDto
    {
        public int APTypeID { get; set; }
        public string APName { get; set; }

        public int ReferenceID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int ApprovalProcessID { get; set; }
        public string APFeedbackName { get; set; }
        public int EmployeeID { get; set; }
        public int ProxyEmployeeID { get; set; }
        public string RequestedEmployee { get; set; }
        public int APEmployeeFeedbackID { get; set; }
        public string RequestedEmployeeImagePath { get; set; }
        public DateTime? FeedbackRequestDate { get; set; }
        public int APFeedbackID { get; set; }
        public int APForwardInfoID { get; set; }
        public bool IsAPEditable { get; set; }
        public bool IsEditable { get; set; }
        public bool IsSCM { get; set; }
        public int TotalCount { get; set; }
        public int rn { get; set; }
        public List<NotificationDto> NotificationDetails { get; set; }
    }

}
