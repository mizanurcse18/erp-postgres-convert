using API.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace External.API.ExternalUtils
{
    public static class ExternalUtils
    {

        static IConfiguration conf = (new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build());

        public static string SMS = conf["AppSettings:ExternalUrl:SMS"];
        public static string PaySlip = conf["AppSettings:ExternalUrl:PaySlip"];
        public static string TaxCard = conf["AppSettings:ExternalUrl:TaxCard"];
        public static string TaxUpload = conf["AppSettings:ExternalUrl:TaxUpload"];
        public static string IncentivePayslip = conf["AppSettings:ExternalUrl:IncentivePayslip"];
        public static string FestivalBonusPayslip = conf["AppSettings:ExternalUrl:FestivalBonusPayslip"];
        public static string Client_Token_Tax = "59d78e5c-946f-11kc-9359-009056r00001";
        public static string Client_Token_TaxUploadTest = "59d78e5c-946f-11kc-9359-009056r00001";
        public static string Client_Token_TaxUpload = "9673tr10-a487-4s65-u87b-15m57it8a2pi";//Live
        public static string Client_Token = "59d78e5c-946f-11kc-9359-009056r00001";//conf["AppSettings:ExternalUrl:Client_Token"]; 
        public static string Client_Token_IncentivePayslipTest = "20Mn23-i05n-0cE5-n11t-34i57V-Es8l2iP";
        public static string Client_Token_IncentivePayslip = "E8fC-02nA-498C-802e-Aa3C-962F";//Live
        public static string Client_Token_FestivalBonusPayslip = "F3s9T6-Bn8Us-2p6A2-8y98s-5L9i7p";//Live
        public static string Masking = conf["AppSettings:ExternalUrl:Masking"];
        public static string UserName = conf["AppSettings:ExternalUrl:UserName"]; 
        public static string Password = conf["AppSettings:ExternalUrl:Password"]; 
        public static string MsgType = conf["AppSettings:ExternalUrl:MsgType"];
        public static string NGDMerchant = conf["AppSettings:ExternalUrl:NGDMerchant"];
        public static string NGDMerchantSecretKey = conf["AppSettings:ExternalUrl:NGDMerchantSecretKey"];
        public static string Proxy = conf["AppSettings:ExternalUrl:Proxy"];
        public static string EnableProxy = conf["AppSettings:ExternalUrl:EnableProxy"];


    }
}
