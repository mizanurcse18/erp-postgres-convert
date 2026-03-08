using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    public class PayrollFileUploadDto
    {
        public int patID { get; set; }
        public int activityTypeID { get; set; }
        public string periodID { get; set; }
        public string monthId { get; set; }
        public int yearID { get; set; }
        public string IncentiveType { get; set; }
        public int BonusType { get; set; }
        public IFormFile file { get; set; }
    }
}
