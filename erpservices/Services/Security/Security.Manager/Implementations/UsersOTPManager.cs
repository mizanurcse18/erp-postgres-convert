using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using Manager.Core;
using Manager.Core.Mapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Security.DAL.Entities;
using Security.Manager.Dto;
using Security.Manager.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Core.Util;

namespace Security.Manager.Implementations
{
    class UsersOTPManager : ManagerBase, IUsersOTPManager
    {

        readonly IRepository<UsersOTP> UsersOTPRepo;
        readonly IEmailManager EmailManager;
        public UsersOTPManager(IRepository<UsersOTP> usersOTPRepo,
            IEmailManager emailManager
            )
        {
            UsersOTPRepo = usersOTPRepo;
            EmailManager = emailManager;
        }


        public GridModel GetOTPHistoryList(GridParameter parameters)
        {
            string monthid = Convert.ToInt32(parameters.ApprovalFilterData) == (int)HRSupportCategoryType.PaySlip ? "OTP.Month" : "1";
            string year = Convert.ToInt32(parameters.ApprovalFilterData) == (int)HRSupportCategoryType.PaySlip ? "ISNULL(F.YearDescription, '')" : "F.YearDescription";
            string sql = $@"SELECT OTP.UOTPID,convert(varchar, OTP.ExpiredDate, 0) ExpiredDate, CASE WHEN OTP.IsVerified=1 THEN 'Verified' ELSE 'Not Verified' END Verified, OTP.SMPPResponse, OTP.Year,
            DATENAME(month, DATEADD(month, OTP.Month-1, CAST('2008-01-01' AS datetime))) Month,
            ISNULL(F.YearDescription, '') YearDescription 
            FROM UsersOTP OTP 
            LEFT JOIN FinancialYear F ON F.FinancialYearID = OTP.Year AND OTP.CategoryType={(int)HRSupportCategoryType.TaxCard}
            WHERE UserID={AppContexts.User.UserID} AND CategoryType={parameters.ApprovalFilterData}";

            var result = UsersOTPRepo.LoadGridModel(parameters, sql);
            return result;
        }
        private void SetNewUserOTPID(UsersOTP obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("UsersOTP", AppContexts.User.CompanyID);
            obj.UOTPID = code.MaxNumber;
        }
        public async Task<(bool, object)> GenerateOTP(UsersOTPDto dto)
        {

            List<UsersOTP> otpList = new List<UsersOTP>();

            string generatedOTP = GenerateRandomPassword();
            string encryptedString = CreatePasswordHash(generatedOTP);

            var data = new UsersOTP
            {
                OTPHash = encryptedString,
                IsExpired = false,
                ExpiredDate = DateTime.Now.AddMinutes(3),
                UserID = AppContexts.User.UserID,
                Year = dto.SelectedYear,
                Month = dto.SelectedMonth,
                CategoryType = dto.CategoryType
            };
            //int groupid = Convert.ToInt32(dto.CategoryType) == (int)HRSupportCategoryType.PaySlip ? (int)Util.MailGroupSetup.OTPMessageBody : (int)Util.MailGroupSetup.OTPMessageBodyTax;
            int groupid = Convert.ToInt32(dto.CategoryType) == (int)HRSupportCategoryType.TaxCard ? (int)Util.MailGroupSetup.OTPMessageBodyTax : (int)Util.MailGroupSetup.OTPMessageBody;

            string sql = $@"SELECT * FROM MailConfiguration..MailGroupSetup WHERE GroupId = {groupid}";
            var messageData = UsersOTPRepo.GetData(sql);

            var existsOTPs = UsersOTPRepo.GetAllList().Where(x => x.UserID == AppContexts.User.UserID && x.IsExpired == false).MapTo<List<UsersOTP>>();
            using (var unitOfWork = new UnitOfWork())
            {

                data.SetAdded();
                SetNewUserOTPID(data);
                otpList.Add(data);

                foreach (var obj in existsOTPs)
                {
                    obj.SetModified();
                    obj.IsExpired = true;
                    otpList.Add(obj);
                }

                SetAuditFields(otpList);

                UsersOTPRepo.AddRange(otpList);

                unitOfWork.CommitChangesWithAudit();
            }


            var mailData = new List<Dictionary<string, object>>();
            var mdata = new Dictionary<string, object>
                {
                    { "OTP", generatedOTP }
                };
            mailData.Add(mdata);

            var toMail = new List<string> {
                    new string(AppContexts.User.Email)
                };
            //BasicMail(Convert.ToInt32(dto.CategoryType) == (int)HRSupportCategoryType.PaySlip ? (int)Util.MailGroupSetup.OTPMessageBody : (int)Util.MailGroupSetup.OTPMessageBodyTax, toMail, false, null, null, mailData);
            BasicMail(Convert.ToInt32(dto.CategoryType) == (int)HRSupportCategoryType.TaxCard ? (int)Util.MailGroupSetup.OTPMessageBodyTax : (int)Util.MailGroupSetup.OTPMessageBody, toMail, false, null, null, mailData);
            //await SendMail(AppContexts.User.EmployeeCode, messageData["Subject"].ToString(), messageData["Body"].ToString().Replace("{{OTP}}", generatedOTP));

            await Task.CompletedTask;

            return (true, new { messageBody = messageData["Body"], otpText = encryptedString, CategoryType = dto.CategoryType });
        }

        private async Task SendMail(string userName, string mailSubject, string mailBody)
        {
            EmailDto emailBody = new EmailDto
            {
                FromEmailAddress = "nagad.erp.test@gmail.com",
                FromEmailAddressDisplayName = "No Reply",
                ToEmailAddress = new List<string>() { AppContexts.User.Email },
                CCEmailAddress = new List<string>(),
                BCCEmailAddress = new List<string>(),
                EmailDate = DateTime.Now,
                Subject = mailSubject,
                EmailBody = mailBody

            };

            await EmailManager.SendEmail(emailBody);
        }

        private static string GenerateRandomPassword()
        {
            string generatedOTP = "";

            Random random = new Random();
            string applicableCharacters = string.Concat(Enumerable.Range(0, 20));

            int otpLength = 4;

            var cryptoRandom = new RNGCryptoServiceProvider();
            var otpBuilder = new StringBuilder();

            var passBuffer = new byte[otpLength];

            cryptoRandom.GetBytes(passBuffer);

            foreach (var passPostitionByte in passBuffer)
            {
                otpBuilder.Append(applicableCharacters[passPostitionByte % applicableCharacters.Length]);
            }

            generatedOTP = otpBuilder.ToString();

            return generatedOTP;
        }
        private static string CreatePasswordHash(string otp)
        {
            if (otp.IsNullOrEmpty()) throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(otp));

            var encryptedString = Util.EncryptString(Util.OTPKey, otp);
            return encryptedString;
        }
        public bool SaveSMSResponse(string resp)
        {
            var user = UsersOTPRepo.SingleOrDefault(x => x.UserID == AppContexts.User.UserID && x.IsExpired == false);

            using (var unitOfWork = new UnitOfWork())
            {

                user.SetModified();
                user.SMPPResponse = resp;

                SetAuditFields(user);

                UsersOTPRepo.Add(user);

                unitOfWork.CommitChangesWithAudit();
            }
            return true;

        }
        public (bool,bool) VerifyOTP(string otp)
        {
            try
            {
                var user = UsersOTPRepo.SingleOrDefault(x => x.UserID == AppContexts.User.UserID && (x.IsExpired == false && x.ExpiredDate > DateTime.Now));
                bool otpExpired = false;
                if(user == null)
                {
                    return (false, true);
                }

                if (!VerifyOTPHash(otp.ToString(), user.OTPHash)) return (false,otpExpired);
                using (var unitOfWork = new UnitOfWork())
                {

                    user.SetModified();
                    user.IsVerified = true;

                    SetAuditFields(user);

                    UsersOTPRepo.Add(user);

                    unitOfWork.CommitChangesWithAudit();
                }

            }
            catch (Exception ex)
            {

                throw new Exception();
            }

            return (true,true);

        }

        private static bool VerifyOTPHash(string otp, string storedHash)
        {
            if (otp.IsNullOrEmpty()) throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(otp));

            var decryptedString = Util.DecryptString(Util.OTPKey, storedHash);
            if (decryptedString == otp)
                return true;
            else return false;
        }


        public async Task<(bool, object)> UploadPayslipGenerateOTP(UsersOTPDto dto)
        {

            List<UsersOTP> otpList = new List<UsersOTP>();

            string generatedOTP = GenerateRandomPassword();
            string encryptedString = CreatePasswordHash(generatedOTP);

            var data = new UsersOTP
            {
                OTPHash = encryptedString,
                IsExpired = false,
                ExpiredDate = DateTime.Now.AddMinutes(3),
                UserID = AppContexts.User.UserID,
                Year = dto.SelectedYear,
                Month = dto.SelectedMonth,
                CategoryType = dto.CategoryType
            };
            //int groupid = Convert.ToInt32(dto.CategoryType) == (int)HRSupportCategoryType.PaySlip ? (int)Util.MailGroupSetup.OTPMessageBody : (int)Util.MailGroupSetup.OTPMessageBodyTax;
            int groupid = Convert.ToInt32(dto.CategoryType) == (int)HRSupportCategoryType.TaxCard ? (int)Util.MailGroupSetup.OTPMessageBodyTax : (int)Util.MailGroupSetup.OTPMessageBody;

            string sql = $@"SELECT * FROM MailConfiguration..MailGroupSetup WHERE GroupId = {groupid}";
            var messageData = UsersOTPRepo.GetData(sql);

            var userIdString = Util.UploadPayslipUserCredential().Item1;
            var userIds = userIdString.Split(',')
                .Select(id => int.Parse(id.Trim()))
                .ToList();

            //var existsOTPs = UsersOTPRepo.GetAllList().Where(x => x.UserID == Util.UploadPayslipUserCredential().Item1 && x.IsExpired == false).MapTo<List<UsersOTP>>();
            var existsOTPs = UsersOTPRepo.GetAllList().Where(x => userIds.Contains(x.UserID) && x.IsExpired == false).MapTo<List<UsersOTP>>();
            using (var unitOfWork = new UnitOfWork())
            {

                data.SetAdded();
                SetNewUserOTPID(data);
                otpList.Add(data);

                foreach (var obj in existsOTPs)
                {
                    obj.SetModified();
                    obj.IsExpired = true;
                    otpList.Add(obj);
                }

                SetAuditFields(otpList);

                UsersOTPRepo.AddRange(otpList);

                unitOfWork.CommitChangesWithAudit();
            }


            var mailData = new List<Dictionary<string, object>>();
            var mdata = new Dictionary<string, object>
                {
                    { "OTP", generatedOTP }
                };
            mailData.Add(mdata);

            var emailString = Util.UploadPayslipUserCredential().Item2;
            var toMail = emailString.Split(',')
                .Select(email => email.Trim())
                .ToList();
            //var toMail = new List<string> {
            //        Util.UploadPayslipUserCredential().Item2
            //    };
            //BasicMail(Convert.ToInt32(dto.CategoryType) == (int)HRSupportCategoryType.PaySlip ? (int)Util.MailGroupSetup.OTPMessageBody : (int)Util.MailGroupSetup.OTPMessageBodyTax, toMail, false, null, null, mailData);
            BasicMail(Convert.ToInt32(dto.CategoryType) == (int)HRSupportCategoryType.TaxCard ? (int)Util.MailGroupSetup.OTPMessageBodyTax : (int)Util.MailGroupSetup.OTPMessageBody, toMail, false, null, null, mailData);
            //await SendMail(AppContexts.User.EmployeeCode, messageData["Subject"].ToString(), messageData["Body"].ToString().Replace("{{OTP}}", generatedOTP));

            await Task.CompletedTask;

            return (true, new { messageBody = messageData["Body"], otpText = encryptedString, CategoryType = dto.CategoryType });
        }
    }
}
