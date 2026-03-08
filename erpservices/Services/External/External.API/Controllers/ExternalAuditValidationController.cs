using API.Core;
using Core;
using External.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Net;
using System;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Core.AppContexts;
using Newtonsoft.Json.Linq;
using static Core.Util;

namespace External.API.Controllers
{
    [AllowAnonymous, ApiController, Route("[controller]")]
    public class ExternalAuditValidationController : BaseController
    {
        [HttpPost("GetExternalAuditMerchantDetails")]
        public async Task<IActionResult> GetExternalAuditMerchantDetails([FromBody] dynamic wallet)
        {
            string walletNo = Convert.ToString(wallet?.walletNo?.Value);

            if(string.IsNullOrWhiteSpace(walletNo))
            {
                return OkResult(new { ApiStatus = false, ApiMessage = "not a valid wallet", WalletNo = walletNo, WalletName = string.Empty });
            }

            try
            {
                bool status = false;
                string message = string.Empty; string walletName = string.Empty;
                var checkSum = generateCheckSum(walletNo);
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, ExternalUtils.ExternalUtils.NGDMerchant);
                request.Headers.Add("checkSum", checkSum);
                var newReqBody = new
                {
                    msisdn = walletNo,
                };

                string jsonData = JsonConvert.SerializeObject(newReqBody);

                var content = new StringContent(jsonData, null, "application/json");
                request.Content = content;
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var resp = await response.Content.ReadAsStringAsync();

                if (resp != null)
                {
                    JObject jsonObjectMerchant = JObject.Parse(resp);
                    if (jsonObjectMerchant["reason"].ToString().Equals("00_0000"))
                    {
                        JObject merchantInfo = JObject.Parse(jsonObjectMerchant["merchantInfo"].ToString());
                        if (merchantInfo["status"].ToString().Equals("ACTIVE"))
                        {
                            walletName = merchantInfo["accountName"].ToString();
                            message = "success";
                            status = true;
                        }

                        else
                        {
                            message = "failed";
                        }
                    }

                    else
                    {
                        message = "invalid wallet";
                    }
                }

                else
                {
                    message = "error";
                }

                return OkResult(new { ApiStatus = status, ApiMessage = message, WalletNo = walletNo, WalletName = walletName });
            }
            catch (Exception ex)
            {
                return OkResult(new { ApiStatus = false, ApiMessage = "failed with exception", WalletNo = walletNo, WalletName = string.Empty });
            }

        }
        private static string generateCheckSum(string walletNo)
        {
            string secretKey = ExternalUtils.ExternalUtils.NGDMerchantSecretKey;
            string payload = walletNo + secretKey;
            string hashedData = ComputeSha256Hash(payload);
            return hashedData;

        }
        private static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
