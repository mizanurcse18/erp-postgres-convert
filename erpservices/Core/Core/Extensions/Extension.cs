using CoreHtmlToImage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Core.Extensions
{
    public static class Extension
    {
        public static bool Compare<T>(T x, T y)
        {
            return EqualityComparer<T>.Default.Equals(x, y);
        }
        public static bool NotEquals<T>(this T val, T compVal)
        {
            return !Compare(val, compVal);
        }
        public static bool IsZero(this long val)
        {
            return val.Equals(0);
        }
        public static bool IsNotZero(this long val)
        {
            return !val.Equals(0);
        }
        public static bool IsZero(this decimal val)
        {
            return val.Equals(decimal.Zero);
        }
        public static bool IsNotZero(this decimal val)
        {
            return !val.Equals(decimal.Zero);
        }
        public static bool IsZero(this double val)
        {
            return val.Equals(0);
        }
        public static bool IsNotZero(this double val)
        {
            return !val.Equals(0);
        }
        public static bool IsZero(this int val)
        {
            return val.Equals(0);
        }
        public static bool IsNotZero(this int val)
        {
            return !val.Equals(0);
        }
        public static bool IsZero(this short val)
        {
            return val.Equals(0);
        }
        public static bool IsNotZero(this short val)
        {
            return !val.Equals(0);
        }
        public static bool IsTrue(this bool val)
        {
            return val;
        }
        public static bool IsFalse(this bool val)
        {
            return !val;
        }
        public static bool IsNull(this object obj)
        {
            return obj == null;
        }
        public static bool IsNotNull(this object obj)
        {
            return obj != null;
        }
        public static bool IsNullOrDbNull(this object obj)
        {
            return obj == null || obj == DBNull.Value;
        }
        public static bool IsMinValue(this DateTime obj)
        {
            return obj == DateTime.MinValue;
        }
        public static bool IsNotMinValue(this DateTime obj)
        {
            return obj != DateTime.MinValue;
        }
        public static bool IsNullOrMinValue(this DateTime? obj)
        {
            return obj == null || obj == DateTime.MinValue;
        }
        public static object Value(this object value)
        {
            return value ?? DBNull.Value;
        }
        public static object Value(this DateTime dateTime, string format = Util.DateFormat)
        {
            if (dateTime.Equals(DateTime.MinValue))
                return DBNull.Value;
            return dateTime.ToString(format);
        }
        public static object Value(this DateTime? dateTime, string format = Util.DateFormat)
        {
            if (dateTime.IsNull() || dateTime.Equals(DateTime.MinValue))
                return DBNull.Value;
            return Convert.ToDateTime(dateTime).ToString(format);
        }
        public static string StringValue(this DateTime dateTime)
        {
            return dateTime.Equals(DateTime.MinValue) ? string.Empty : dateTime.ToString(Util.DateFormat);
        }
        public static string StringValue(this DateTime? dateTime)
        {
            return dateTime.IsNull() ? string.Empty : Convert.ToDateTime(dateTime).ToString(Util.DateFormat);
        }
        public static int GetAge(this DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var a = (today.Year * 100 + today.Month) * 100 + today.Day;
            var b = (dateOfBirth.Year * 100 + dateOfBirth.Month) * 100 + dateOfBirth.Day;
            return (a - b) / 10000;
        }
        public static Exception GetOriginalException(this Exception ex)
        {
            if (ex.InnerException == null) return ex;

            return ex.InnerException.GetOriginalException();
        }
        public static void Sort<T, TU>(this List<T> list, Func<T, TU> expression, IComparer<TU> comparer) where TU : IComparable<TU>
        {
            list.Sort((x, y) => comparer.Compare(expression.Invoke(x), expression.Invoke(y)));
        }
        public static object MapField(this object value, Type type)
        {
            if (value.GetType() == type)
            {
                return value;
            }
            if (value == DBNull.Value)
            {
                return null;
            }
            Type underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType.IsNull())
            {
                underlyingType = type;
            }
            return Convert.ChangeType(value, underlyingType);
        }
        public static void CloseReader(this IDataReader reader)
        {
            if (reader.IsNotNull() && !reader.IsClosed)
            {
                reader.Close();
            }
        }
        public static byte[] HtmlToImage(string htmlText)
        {
            var converter = new HtmlConverter();
            var bytes = converter.FromHtmlString(htmlText,500,ImageFormat.Png);
            //File.WriteAllBytes(@"E:/Projects/Back End/nagaderpservices/Services/Security/Security.API/wwwroot/upload/attachments/image.jpeg", bytes);
            return bytes;
        }

        // In my case this is https://localhost:44366/
        private static readonly string apiBasicUri = "http://localhost:5000/mail";//ConfigurationManager.AppSettings["apiBasicUri"];
        //static IConfiguration conf = (new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build());
        //public static int MonitoringTime = Convert.ToInt32(conf["ApplicationSetting:MonitoringTime"]);

        public static async Task Post<T>(string url, T contentValue)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    //var content = new FormUrlEncodedContent(new[]{ new KeyValuePair<string, string>("", "login")});
                    client.BaseAddress = new Uri(apiBasicUri);
                     var content = new StringContent(JsonConvert.SerializeObject(contentValue), Encoding.UTF8, "application/json");
                    var result = await client.PostAsync(url, content);
                    result.EnsureSuccessStatusCode();
                    //string resultContent = await result.Content.ReadAsStringAsync();
                    //Console.WriteLine(resultContent);
                }
                catch (Exception ex)
                {
                    //
                }
            }
        }

        public static async Task Put<T>(string url, T stringValue)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiBasicUri);
                var content = new StringContent(JsonConvert.SerializeObject(stringValue), Encoding.UTF8, "application/json");
                var result = await client.PutAsync(url, content);
                result.EnsureSuccessStatusCode();
            }
        }

        public static async Task<T> Get<T>(string url)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiBasicUri);
                var result = await client.GetAsync(url);
                result.EnsureSuccessStatusCode();
                string resultContentString = await result.Content.ReadAsStringAsync();
                T resultContent = JsonConvert.DeserializeObject<T>(resultContentString);
                return resultContent;
            }
        }

        public static async Task Delete(string url)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiBasicUri);
                var result = await client.DeleteAsync(url);
                result.EnsureSuccessStatusCode();
            }
        }


        public static decimal stringToDecimal(this string value, decimal defaultValue = 0)
        {
            if (decimal.TryParse(value, out decimal result))
            {
                return Math.Round(result, 2, MidpointRounding.AwayFromZero);
            }
            else
            {
                Console.WriteLine("Invalid input");
                return Math.Round(defaultValue, 2, MidpointRounding.AwayFromZero);
            }
        }

        public static bool IsNumeric(this string value)
        {
            // Return false if the value is null or empty
            if (string.IsNullOrWhiteSpace(value))
                return false;

            // Attempt to parse the string as a decimal
            return decimal.TryParse(value, out _);
        }

    }
}
