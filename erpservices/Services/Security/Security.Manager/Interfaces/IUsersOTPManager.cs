using Core;
using Security.DAL.Entities;
using Security.Manager.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Security.Manager.Interfaces
{
    public interface IUsersOTPManager
    {
        public Task<(bool, object)> GenerateOTP(UsersOTPDto dto);
        public Task<(bool, object)> UploadPayslipGenerateOTP(UsersOTPDto dto);
        (bool,bool) VerifyOTP(string otp);
        bool SaveSMSResponse(string resp);
        GridModel GetOTPHistoryList(GridParameter parameters); 
    }
}
