using API.Core;
using Core;
using External.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace External.API.Controllers
{
    [AllowAnonymous, ApiController, Route("[controller]")]
    public class DocumentUploadController : BaseController
    {

        public DocumentUploadController()
        {

        }


        [HttpPost("UploadDocumentAsync")]
        public IActionResult UploadDocumentAsync([FromBody] EncryptedTaxReturnUploadModel encryptedTaxReturnUploadModel)
        {
            try
            {
                string token = "";
                if (HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authHeader))
                {
                    if (authHeader.ToString().StartsWith("Bearer "))
                    {
                        token = authHeader.ToString().Substring("Bearer ".Length).Trim();
                    }
                }



                var decryptedString = Util.DecryptString(Util.OTPKey, encryptedTaxReturnUploadModel.EncryptedData);
                string Client_Token = ExternalUtils.ExternalUtils.Client_Token_TaxUpload;

                TaxReturnUploadModel taxReturnUploadModel = JsonConvert.DeserializeObject<TaxReturnUploadModel>(decryptedString);

                var baseUrl = ExternalUtils.ExternalUtils.TaxUpload;

                HttpClient client = new HttpClient();
                MultipartFormDataContent form = new MultipartFormDataContent();

                string SubmissionDate = taxReturnUploadModel.SubmissionDate.ToString("dd-MMM-yyyy");

                form.Add(new StringContent(taxReturnUploadModel.EmployeeCode), "emp_code");
                form.Add(new StringContent(taxReturnUploadModel.IncomeYearTitle), "income_year");
                form.Add(new StringContent(taxReturnUploadModel.AssessmentYearTitle), "assessment_year");
                form.Add(new StringContent("TRA"), "file_type");
                form.Add(new StringContent(taxReturnUploadModel.RegSlNo), "reg_sl_no");
                form.Add(new StringContent(taxReturnUploadModel.TaxZone), "tax_zone");
                form.Add(new StringContent(taxReturnUploadModel.TaxCircle), "tax_circle");
                form.Add(new StringContent(taxReturnUploadModel.TaxUnit), "tax_unit");
                form.Add(new StringContent(taxReturnUploadModel.PayableAmount.ToString()), "tax_payable");
                form.Add(new StringContent(taxReturnUploadModel.PaidAmount.ToString()), "paid_amount");
                form.Add(new StringContent(SubmissionDate), "submission_date");

                string FileName = taxReturnUploadModel.EmployeeCode + "_TRA_" + taxReturnUploadModel.IncomeYearTitle;

                int fileCount = 1;
                if (taxReturnUploadModel.filePaths.Count > 0)
                {
                    foreach (FilePath path in taxReturnUploadModel.filePaths)
                    {
                        var extension = Path.GetExtension(path.link);
                        HttpContent content = new StringContent("");
                        Stream stream = new MemoryStream();
                        using (var client1 = new WebClient())
                        {
                            client1.Headers.Add(HttpRequestHeader.Authorization, "Bearer "+ token);

                            var content1 = client1.DownloadData(path.link);
                            stream = new MemoryStream(content1);
                        }
                        content = new StreamContent(stream);
                        FileName = FileName + "_" + fileCount;
                        content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                        {
                            Name = "file" + fileCount,
                            FileName = FileName + extension
                        };
                        fileCount++;
                        form.Add(content);
                    }
                }


                client.DefaultRequestHeaders.Add("Client_Token", Client_Token);

                HttpResponseMessage response = null;

                //try
                //{
                    response = (client.PostAsync(baseUrl, form)).Result;
                //}
                //catch (Exception ex)
                //{
                //    //Console.WriteLine(ex.Message);
                //}

                var res = response.Content.ReadAsStringAsync().Result;


                dynamic obj = JsonConvert.DeserializeObject<dynamic>(res);
                var StatusCode = response.StatusCode;
                string message = obj.Message;

                return OkResult(new { ApiStatus = StatusCode, ApiResponse = message });
            }

            catch (Exception ex)
            {
                string errr = ex.Message.Replace(ExternalUtils.ExternalUtils.TaxUpload, " ");
                return OkResult(new { ApiStatus = 404, ApiResponse = errr });
                //return OkResult(new { ApiStatus = 404, ApiResponse = "Could not upload document!. Please contact with IT Support" });
            }

        }


    }
}
