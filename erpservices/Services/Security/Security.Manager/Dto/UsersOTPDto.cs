using Core;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using Newtonsoft.Json;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Security.Manager.Dto
{
    [AutoMap(typeof(UsersOTP)), Serializable]
    public class UsersOTPDto : EntityBase
    {
        public int UOTPID { get; set; }
        public string OTPHash { get; set; }
        public bool IsExpired { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string EmployeeName { get; set; }
        public int SelectedYear { get; set; }
        public int SelectedMonth { get; set; }
        public string Verified { get; set; }
        public string SelectedMonthName { get; set; }
        public string SMPPResponse { get; set; }
        public string ExpiredDateStr { get; set; }
        public string CategoryType { get; set; }

    }
}
