using System;


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Core;
using API.Core;
using Core.Extensions;
using Core.AppContexts;

using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using External.API.Models;
using System.IO;
using System.Threading.Tasks;
using static Core.Util;
using System.Net;

namespace External.API.Controllers
{
    [AllowAnonymous, ApiController, Route("[controller]")]
    public class SMSController : BaseController
    {
        public SMSController()
        {

        }

        [HttpPost("SendSMSRecom")]
        public IActionResult SendSMSRecom(SMSModel model)
        {
            var contentReturn = "";
            var decryptedString = Util.DecryptString(Util.OTPKey, model.OTPText);

            if (model.OTPText.IsNullOrEmpty())
            {
                model.OTPMismatchOrInvalidError = "Invalid OTP.";
                return OkResult(model);
            }
            if (AppContexts.User.WorkMobile.IsNullOrEmpty())
            {
                model.WorkMobileEmptyError = "Work mobile is empty.Please contact with HR.";
                return OkResult(model);
            }
            if (model.MessageBody.IsNullOrEmpty())
            {
                if (Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.PaySlip && !model.MessageBody.Contains("{{OTP}}"))
                {
                    model.MessageBodyError = "Message Body not found For Pay Slip.";
                    return OkResult(model);
                }
                else if (Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.TaxCard && !model.MessageBody.Contains("{{OTPTax}}"))
                {
                    model.MessageBodyError = "Message Body not found For Tax Card.";
                    return OkResult(model);
                }
            }

            string msgBody = "";
            if (Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.PaySlip)
            {
                msgBody = model.MessageBody.Replace("{{OTP}}", decryptedString);
            }
            else if (Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.TaxCard)
            {
                msgBody = model.MessageBody.Replace("{{OTPTax}}", decryptedString);
            }

            string proxyUrl = ExternalUtils.ExternalUtils.Proxy;
            if (proxyUrl.IsNotNullOrEmpty())
            {
                var proxy = new WebProxy
                {
                    Address = new Uri(proxyUrl),
                    BypassProxyOnLocal = false,
                    UseDefaultCredentials = false
                };

                var httpClientHandler = new HttpClientHandler
                {
                    Proxy = proxy,
                    UseProxy = true
                };

                using (HttpClient client = new HttpClient(httpClientHandler))
                {
                    var body = new
                    {
                        to = AppContexts.User.WorkMobile,
                        text = msgBody,
                        accessChanel = "APP",
                        referenceId = "ahdjdkknc",
                        mno = "GP"
                    };
                    var smsUrl = ExternalUtils.ExternalUtils.SMS;
                    if (smsUrl.IsNullOrEmpty())
                    {
                        model.APINotFoundError = "SMS Api not found!";
                        return OkResult(model);
                    }
                    StringContent content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

                    using (var Response = client.PostAsync(smsUrl, content))
                    {
                        var res = Response.Result;
                        if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            model.APIUnreachableError = "SMS Api is unreachable.";
                            return OkResult(model);
                        }

                        contentReturn = Response.Result.Content.ReadAsStringAsync().Result;
                    }
                }
            }

            return OkResult(contentReturn);
        }


        [HttpPost("SendSMS")]
        public IActionResult SendSMS(SMSModel model)
        {
            var contentReturn = "";
            var decryptedString = Util.DecryptString(Util.OTPKey, model.OTPText);

            if (model.OTPText.IsNullOrEmpty())
            {
                model.OTPMismatchOrInvalidError = "Invalid OTP.";
                return OkResult(model);
            }
            if (AppContexts.User.WorkMobile.IsNullOrEmpty())
            {
                model.WorkMobileEmptyError = "Work mobile is empty.Please contact with HR.";
                return OkResult(model);
            }
            //if (model.MessageBody.IsNullOrEmpty() || !model.MessageBody.Contains("{{OTP}}"))
            //{
            //    model.MessageBodyError = "Message Body not found.";
            //    return OkResult(model);
            //}
            if (model.MessageBody.IsNullOrEmpty())
            {
                switch (Convert.ToInt32(model.CategoryType))
                {
                    case (int)HRSupportCategoryType.PaySlip when !model.MessageBody.Contains("{{OTP}}"):
                        model.MessageBodyError = "Message Body not found For Pay Slip.";
                        return OkResult(model);
                    case (int)HRSupportCategoryType.TaxCard when !model.MessageBody.Contains("{{OTPTax}}"):
                        model.MessageBodyError = "Message Body not found For Tax Card.";
                        return OkResult(model);
                    case (int)HRSupportCategoryType.IncentivePayslip when !model.MessageBody.Contains("{{OTP}}"):
                        model.MessageBodyError = "Message Body not found For Incentive.";
                        return OkResult(model);
                    case (int)HRSupportCategoryType.FestivalBonusPayslip when !model.MessageBody.Contains("{{OTP}}"):
                        model.MessageBodyError = "Message Body not found For Festival Bonus.";
                        return OkResult(model);
                }
            }

            string msgBody = "";
            if (Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.PaySlip)
            {
                msgBody = model.MessageBody.Replace("{{OTP}}", decryptedString);
            }
            else if (Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.TaxCard)
            {
                msgBody = model.MessageBody.Replace("{{OTP}}", decryptedString);
            }
            else if (Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.IncentivePayslip)
            {
                msgBody = model.MessageBody.Replace("{{OTP}}", decryptedString);
            }
            else if (Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.FestivalBonusPayslip)
            {
                msgBody = model.MessageBody.Replace("{{OTP}}", decryptedString);
            }

            string proxyUrl = ExternalUtils.ExternalUtils.Proxy;
            string enableProxy = ExternalUtils.ExternalUtils.EnableProxy;

            bool ValUseProxy = enableProxy.IsNotNullOrEmpty() && enableProxy.Contains("True") ? true: false;
         
            if (proxyUrl.IsNotNullOrEmpty())
            {
                var proxy = new WebProxy
                {
                    Address = new Uri(proxyUrl),
                    BypassProxyOnLocal = false,
                    UseDefaultCredentials = false
                };

                var httpClientHandler = new HttpClientHandler
                {
                    Proxy = proxy,
                    UseProxy = ValUseProxy
                };

                using (HttpClient client = new HttpClient(httpClientHandler))

                {

                    var smsUrl = ExternalUtils.ExternalUtils.SMS;
                    if (smsUrl.IsNullOrEmpty())
                    {
                        model.APINotFoundError = "SMS Api not found!";
                        return OkResult(model);
                    }

                    var fullUrl = smsUrl + "?masking=" + ExternalUtils.ExternalUtils.Masking + "&userName=" + ExternalUtils.ExternalUtils.UserName
                                + "&password=" + ExternalUtils.ExternalUtils.Password + "&MsgType=" + ExternalUtils.ExternalUtils.MsgType + "&receiver="
                                + AppContexts.User.WorkMobile + "&message=" + msgBody;//model.MessageBody.Replace("{{OTP}}", decryptedString);


                    using (var Response = client.GetAsync(fullUrl))
                    {
                        var res = Response.Result;
                        if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            model.APIUnreachableError = "SMS Api is unreachable.";
                            return OkResult(model);
                        }

                        contentReturn = Response.Result.Content.ReadAsStringAsync().Result;
                    }
                }
            }

            return OkResult(contentReturn);
        }



        [HttpGet("DownloadPayslipOld/{year:int}/{month:int}")]
        public async Task<ActionResult> DownloadPayslipOld(int year, int month)
        {
            var contentReturn = "";
            PaySlipModel model = new PaySlipModel();

            if (year.ToString() == "" || year <= 0)
            {
                model.YearMonthError = "Invalid year or month.";
                return OkResult(model);
            }
            if (month.ToString() == "" || month <= 0)
            {
                model.YearMonthError = "Invalid year or month.";
                return OkResult(model);
            }
            if (AppContexts.User.EmployeeCode.IsNullOrEmpty())
            {
                model.YearMonthError = "Invalid year or month or user.";
                return OkResult(model);
            }

            using (HttpClient client = new HttpClient())
            {
                var Client_Token = ExternalUtils.ExternalUtils.Client_Token;
                client.DefaultRequestHeaders.Add("Client_Token", Client_Token);

                string param = "?emp_code=" + AppContexts.User.EmployeeCode.Replace("TW", "").Replace("TWD", "").Replace("TWS", "").Replace("TWC", "") + "&year=" + year + "&month=" + month + "";
                var payslipUrl = ExternalUtils.ExternalUtils.PaySlip + param;

                if (payslipUrl.IsNullOrEmpty())
                {
                    model.APINotFoundError = "Payslip Api not found!";
                    return OkResult(model);
                }

                using (var Response = client.GetAsync(payslipUrl))
                {
                    var res = Response.Result;
                    if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        DateTime dt = new DateTime(year, month, 1);
                        model.APIUnreachableError = @$"No data found for month {dt.ToString("MMMM")}";
                        return OkResult(model);
                    }
                    var fileName = res.Content.Headers.ContentDisposition.FileName;
                    var stream = res.Content.ReadAsStreamAsync().Result;
                    var memoryStream = new MemoryStream();
                    stream.CopyTo(memoryStream);

                    // Convert Stream To Array
                    byte[] byteArray = memoryStream.ToArray();
                    return OkResult(byteArray);
                    //return File(stream.Result.ReadByte(), "application/pdf", fileName);

                    //contentReturn = file;

                }
            }


            return OkResult(contentReturn);
        }

        //[HttpPost("DownloadPayslip")]
        //public async Task<ActionResult> DownloadPayslip(PaySlipModel model)
        //{
        //    var contentReturn = "";
        //    //PaySlipModel model = new PaySlipModel();

        //    if (Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.PaySlip)
        //    {
        //        if (model.year.ToString() == "" || model.year <= 0)
        //        {
        //            model.YearMonthError = "Invalid year or month.";
        //            return OkResult(model);
        //        }
        //        if (model.month.ToString() == "" || model.month <= 0)
        //        {
        //            model.YearMonthError = "Invalid year or month.";
        //            return OkResult(model);
        //        }
        //        if (AppContexts.User.EmployeeCode.IsNullOrEmpty())
        //        {
        //            model.YearMonthError = "Invalid year or month or user.";
        //            return OkResult(model);
        //        }

        //    }
        //    if (Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.TaxCard)
        //    {
        //        if (model.fiscalYear.ToString() == "" || model.fiscalYear.Length <= 0)
        //        {
        //            model.YearMonthError = "Invalid Fiscal Year.";
        //            return OkResult(model);
        //        }
        //        if (AppContexts.User.EmployeeCode.IsNullOrEmpty())
        //        {
        //            model.YearMonthError = "Invalid Fiscal Year or User.";
        //            return OkResult(model);
        //        }

        //    }

        //    using (HttpClient client = new HttpClient())
        //    {
        //        string Client_Token = "";
        //        //var Client_Token = ExternalUtils.ExternalUtils.Client_Token;
        //        if (Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.PaySlip)
        //        {
        //            Client_Token = ExternalUtils.ExternalUtils.Client_Token;
        //        }
        //        else if (Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.TaxCard)
        //        {
        //            Client_Token = ExternalUtils.ExternalUtils.Client_Token_Tax;
        //        }

        //        client.DefaultRequestHeaders.Add("Client_Token", Client_Token);

        //        string paramPayslip = "?emp_code=" + AppContexts.User.EmployeeCode.Replace("TW", "").Replace("TWD", "").Replace("TWS", "").Replace("TWC", "") + "&year=" + model.year + "&month=" + model.month + "";
        //        string paramTax = "?emp_code=" + AppContexts.User.EmployeeCode.Replace("TW", "").Replace("TWD", "").Replace("TWS", "").Replace("TWC", "") + "&fiscal_year=" + model.fiscalYear+"";
        //        var url = Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.PaySlip ? ExternalUtils.ExternalUtils.PaySlip + paramPayslip : ExternalUtils.ExternalUtils.TaxCard + paramTax;

        //        if (url.IsNullOrEmpty())
        //        {
        //            model.APINotFoundError = Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.PaySlip ?"Payslip Api not found!" : "TaxCard Api not found!";
        //            return OkResult(model);
        //        }
        //        Uri myUri = new Uri(url, UriKind.Absolute);
        //        using (var Response = await client.GetAsync(myUri))
        //        {
        //            var res = Response;
        //            if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
        //            {
        //                DateTime dt = Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.PaySlip ? new DateTime(model.year, model.month, 1) : new DateTime();
        //                model.APIUnreachableError = Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.PaySlip? @$"No data found for month {dt.ToString("MMMM")}" : @$"No data found for fiscal year {model.fiscalYear}";
        //                return OkResult(model);
        //            }
        //            var fileName = res.Content.Headers.ContentDisposition.FileName;
        //            var stream = res.Content.ReadAsStreamAsync().Result;
        //            var memoryStream = new MemoryStream();
        //            stream.CopyTo(memoryStream);

        //            // Convert Stream To Array
        //            byte[] byteArray = memoryStream.ToArray();
        //            return OkResult(byteArray);
        //            //return File(stream.Result.ReadByte(), "application/pdf", fileName);

        //            //contentReturn = file;

        //        }
        //    }


        //    return OkResult(contentReturn);
        //}


        [HttpPost("DownloadPayslip")]
        public async Task<ActionResult> DownloadPayslip(PaySlipModel model)
        {
            var contentReturn = "";
            //PaySlipModel model = new PaySlipModel();

            if (Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.PaySlip)
            {
                if (model.year.ToString() == "" || model.year <= 0)
                {
                    model.YearMonthError = "Invalid year or month.";
                    return OkResult(model);
                }
                if (model.month.ToString() == "" || model.month <= 0)
                {
                    model.YearMonthError = "Invalid year or month.";
                    return OkResult(model);
                }
                if (AppContexts.User.EmployeeCode.IsNullOrEmpty())
                {
                    model.YearMonthError = "Invalid year or month or user.";
                    return OkResult(model);
                }

            }
            if (Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.TaxCard)
            {
                if (model.fiscalYear.ToString() == "" || model.fiscalYear.Length <= 0)
                {
                    model.YearMonthError = "Invalid Fiscal Year.";
                    return OkResult(model);
                }
                if (AppContexts.User.EmployeeCode.IsNullOrEmpty())
                {
                    model.YearMonthError = "Invalid Fiscal Year or User.";
                    return OkResult(model);
                }

            }


            if (Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.IncentivePayslip)
            {
                if (model.PayslipType == "Half Yearly")
                {

                    if (AppContexts.User.EmployeeCode.IsNullOrEmpty())
                    {
                        model.YearMonthError = "Invalid Fiscal Year or User.";
                        return OkResult(model);
                    }
                    if (model.fiscalYear.ToString() == "" || model.fiscalYear.Length <= 0)
                    {
                        model.YearMonthError = "Invalid Fiscal Year.";
                        return OkResult(model);
                    }
                    if (model.Quarter.IsNullOrEmpty())
                    {
                        model.YearMonthError = "Invalid Quarter.";
                        return OkResult(model);
                    }
                }
                else
                {
                    if (AppContexts.User.EmployeeCode.IsNullOrEmpty())
                    {
                        model.YearMonthError = "Invalid Fiscal Year or User.";
                        return OkResult(model);
                    }
                    if (model.fiscalYear.ToString() == "" || model.fiscalYear.Length <= 0)
                    {
                        model.YearMonthError = "Invalid Fiscal Year.";
                        return OkResult(model);
                    }
                    if (model.month.ToString() == "" || model.month <= 0)
                    {
                        model.YearMonthError = "Invalid year or month.";
                        return OkResult(model);
                    }
                }


            }

            if (Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.FestivalBonusPayslip)
            {
                if (model.fiscalYear.ToString() == "" || model.fiscalYear.Length <= 0)
                {
                    model.YearMonthError = "Invalid Fiscal Year.";
                    return OkResult(model);
                }
                if (model.FestivalBonus.ToString() == "" || model.FestivalBonus.Length <= 0)
                {
                    model.YearMonthError = "Invalid Festival Bonus.";
                    return OkResult(model);
                }
                if (AppContexts.User.EmployeeCode.IsNullOrEmpty())
                {
                    model.YearMonthError = "Invalid Fiscal Year or User.";
                    return OkResult(model);
                }

            }

            using (HttpClient client = new HttpClient())
            {
                string Client_Token = "";
                //var Client_Token = ExternalUtils.ExternalUtils.Client_Token;
                if (Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.PaySlip)
                {
                    Client_Token = ExternalUtils.ExternalUtils.Client_Token;
                }
                else if (Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.TaxCard)
                {
                    Client_Token = ExternalUtils.ExternalUtils.Client_Token_Tax;
                }
                else if (Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.IncentivePayslip)
                {
                    Client_Token = ExternalUtils.ExternalUtils.Client_Token_IncentivePayslip;
                }
                else if (Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.FestivalBonusPayslip)
                {
                    Client_Token = ExternalUtils.ExternalUtils.Client_Token_FestivalBonusPayslip;
                }

                client.DefaultRequestHeaders.Add("Client_Token", Client_Token);

                string paramPayslip = "?emp_code=" + AppContexts.User.EmployeeCode.Replace("TW", "").Replace("TWD", "").Replace("TWS", "").Replace("TWC", "") + "&year=" + model.year + "&month=" + model.month + "";
                string paramTax = "?emp_code=" + AppContexts.User.EmployeeCode.Replace("TW", "").Replace("TWD", "").Replace("TWS", "").Replace("TWC", "") + "&fiscal_year=" + model.fiscalYear + "";
                string paramQuarterIncentivePayslip = "PerformanceBonusSlip?emp_code=" + AppContexts.User.EmployeeCode.Replace("TW", "").Replace("TWD", "").Replace("TWS", "").Replace("TWC", "") + "&year=" + model.year + "&period=" + model.Quarter + "";
                string paramFestivalBonusPayslip = "BonusPaySlip?emp_code=" + AppContexts.User.EmployeeCode.Replace("TW", "").Replace("TWD", "").Replace("TWS", "").Replace("TWC", "") + "&year=" + model.year + "&bonus_name=" + model.FestivalBonus + "";

                //var url = Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.PaySlip ? ExternalUtils.ExternalUtils.PaySlip + paramPayslip : ExternalUtils.ExternalUtils.TaxCard + paramTax;

                string url = "";

                if (Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.PaySlip)
                {
                    url = ExternalUtils.ExternalUtils.PaySlip + paramPayslip;
                }
                else if (Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.TaxCard)
                {
                    url = ExternalUtils.ExternalUtils.TaxCard + paramTax;
                }
                else if (Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.IncentivePayslip)
                {
                    url = model.PayslipType == "Monthly" ? ExternalUtils.ExternalUtils.IncentivePayslip + "/MonthlyIncentiveSlip" + paramPayslip : ExternalUtils.ExternalUtils.IncentivePayslip + paramQuarterIncentivePayslip;
                }
                else if (Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.FestivalBonusPayslip)
                {
                    url = ExternalUtils.ExternalUtils.FestivalBonusPayslip + paramFestivalBonusPayslip;
                }



                if (url.IsNullOrEmpty())
                {
                    model.APINotFoundError = Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.PaySlip ? "Payslip Api not found!" : "TaxCard Api not found!";
                    return OkResult(model);
                }
                Uri myUri = new Uri(url, UriKind.Absolute);
                using (var Response = await client.GetAsync(myUri))
                {
                    var res = Response;
                    if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        DateTime dt = Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.PaySlip ? new DateTime(model.year, model.month, 1) : new DateTime();
                        model.APIUnreachableError = Convert.ToInt32(model.CategoryType) == (int)HRSupportCategoryType.PaySlip ? @$"No data found for month {dt.ToString("MMMM")}" : @$"No data found for fiscal year {model.fiscalYear}";
                        return OkResult(model);
                    }
                    //var fileName = res.Content.Headers.ContentDisposition.FileName;
                    var stream = res.Content.ReadAsStreamAsync().Result;
                    var memoryStream = new MemoryStream();
                    stream.CopyTo(memoryStream);

                    // Convert Stream To Array
                    byte[] byteArray = memoryStream.ToArray();
                    return OkResult(byteArray);
                    //return File(stream.Result.ReadByte(), "application/pdf", fileName);

                    //contentReturn = file;

                }
            }


            return OkResult(contentReturn);
        }

    }
}
