using Core.AppContexts;
using Core.Extensions;
using CsvHelper;
using CsvHelper.Configuration;
using FastMember;
using Manager.Core.CommonDto;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Hosting;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manager.Core
{
    public static class UploadUtil
    {
        public static byte[] Base64ToByteArray(string base64String)
        {
            return Convert.FromBase64String(base64String.Replace("data:image/jpeg;base64,", "").
                Replace("data:image/x-icon;base64,", "").
                Replace("data:image/png;base64,", "").
                Replace("data:application/vnd.ms-excel;base64,", "").
                Replace("data:application/pdf;base64,", "").
                Replace("data:application/vnd.openxmlformats-officedocument.wordprocessingml.document;base64,", "").
                Replace("data:text/plain;base64,", "").
                Replace("data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64,", "").Replace("data:;base64,", "").Replace("data:application/msword;base64,", "")
                );
        }

        public static System.Drawing.Image Base64ToImage(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String.Replace("data:image/jpeg;base64,", ""));
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);

            ms.Write(imageBytes, 0, imageBytes.Length);
            return System.Drawing.Image.FromStream(ms, true);
        }

        public static string SaveImageInDisk(byte[] byteArrayIn, string fileName, string folderName = "temp")
        {
            string imageFolder = "upload\\images";
            if (byteArrayIn.IsNull() || StructuralComparisons.StructuralEqualityComparer.Equals((object)byteArrayIn, (object)new byte[0]))
                return string.Empty;
            IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
            //IOptions<AppContexts> instance2 = (IOptions<AppContexts>)AppContexts.GetInstance(typeof(IOptions<AppContexts>));
            string str = Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot\\" + imageFolder + "\\" + folderName);
            if (!Directory.Exists(str))
                Directory.CreateDirectory(str);
            string path = Path.Combine(str, fileName);
            if (File.Exists(path))
                File.Delete(path);
            File.WriteAllBytes(path, byteArrayIn);
            //UploadUtil.GetHostPath() +
            return "/" + imageFolder + "/" + folderName + "/" + fileName;
        }

        public static string SaveAttachmentInDisk(byte[] byteArrayIn, string fileName, string folderName = "temp", string attachmentFolder = "upload\\attachments")
        {
            if (byteArrayIn.IsNull() || StructuralComparisons.StructuralEqualityComparer.Equals((object)byteArrayIn, (object)new byte[0]))
                return string.Empty;
            IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
            string str = Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName);
            if (!Directory.Exists(str))
                Directory.CreateDirectory(str);
            string path = Path.Combine(str, fileName);
            if (File.Exists(path))
                File.Delete(path);
            File.WriteAllBytes(path, byteArrayIn);
            //UploadUtil.GetHostPath() +
            return "/" + attachmentFolder + "/" + folderName + "/" + fileName;

        }

        public static string SaveAttachmentInDiskWithCustomText(byte[] byteArrayIn, string fileName, string text, string module)
        {
            if (byteArrayIn.IsNull() || StructuralComparisons.StructuralEqualityComparer.Equals((object)byteArrayIn, (object)new byte[0]))
                return string.Empty;
            IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
            string str = Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot\\" + @$"upload\\{module}\\{DateTime.Now.Year}\\{DateTime.Now.Month}\\{DateTime.Now.Day}");
            if (!Directory.Exists(str))
                Directory.CreateDirectory(str);
            string path = Path.Combine(str, fileName);
            if (File.Exists(path))
                File.Delete(path);
            File.WriteAllBytes(path, byteArrayIn);

            using (MemoryStream ms = new MemoryStream(byteArrayIn))
            {
                using (Image image = Image.FromStream(ms))
                {
                    // Create a graphics object from the image
                    using (Graphics graphics = Graphics.FromImage(image))
                    {
                        // Define the font and text to be drawn
                        Font font = new Font("Arial", 10, FontStyle.Bold);
                        //string text = "Sample Text";

                        // Define the position and color of the text
                        PointF position = new PointF(10, 10);
                        Brush brush = Brushes.Gray;

                        // Draw the text on the image
                        graphics.DrawString(text, font, brush, position);
                    }

                    // Save the image
                    image.Save(path, ImageFormat.Jpeg);
                }
            }
            //UploadUtil.GetHostPath() +
            return "/" + $@"upload/{module}/{DateTime.Now.Year}/{DateTime.Now.Month}/{DateTime.Now.Day}" + "/" + fileName;

        }


        public async static Task<SaveFileDescription> SaveFileInDisk(IFormFile file, string folderName = "temp", string attachmentFolder = "upload\\attachments")
        {
            SaveFileDescription description = new SaveFileDescription();
            description.FileOriginalName = file.FileName;
            description.FileExtention = "." + file.FileName.Split(".")[1];
            description.SavedFileName = file.FileName.Split(".")[0] + "-" + DateTime.Now.Ticks.ToString() + "." + file.FileName.Split(".")[1];
            description.FileSize = file.Length;
            IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
            string str = Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName);
            if (!Directory.Exists(str))
                Directory.CreateDirectory(str);
            string filePath = Path.Combine(str, description.SavedFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            description.FullPath = filePath;
            description.FileRelativePath = "/" + attachmentFolder + "/" + folderName + "/" + description.SavedFileName;
            return description;
        }

        public async static Task<SaveFileDescription> SaveCSVFileInDisk<T>(List<T> data, string folderName = "temp", string attachmentFolder = "upload\\attachments")
        {
            SaveFileDescription description = new SaveFileDescription();
            try
            {
                description.FileExtention = ".csv";
                description.SavedFileName = folderName + "_" + DateTime.Now.Ticks.ToString() + ".csv";
                description.FileRelativePath = "/" + attachmentFolder + "/" + folderName + "/" + description.SavedFileName;
                IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();

                string str = Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName);

                if (!Directory.Exists(str))
                    Directory.CreateDirectory(str);
                string filePath = Path.Combine(str, description.SavedFileName);
                using (var writer = new StreamWriter(filePath))
                using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csvWriter.WriteRecords(data);
                }
            }
            catch (Exception ex) { }

            return description;

        }


        public async static Task<SaveFileDescription> SaveCSVFileInDiskDictionary(List<Dictionary<string, object>> data, string folderName = "temp", string attachmentFolder = "upload\\attachments")
        {
            SaveFileDescription description = new SaveFileDescription();
            try
            {
                description.FileExtention = ".csv";
                description.FolderName = folderName;
                description.SavedFileName = folderName + "_" + DateTime.Now.Ticks.ToString() + ".csv";
                description.FileRelativePath = "/" + attachmentFolder + "/" + folderName + "/" + description.SavedFileName;
                IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();

                string str = Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName);

                if (!Directory.Exists(str))
                    Directory.CreateDirectory(str);

                string filePath = Path.Combine(str, description.SavedFileName);
                using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    // Write header
                    if (data.Count > 0)
                    {
                        foreach (var header in data[0].Keys)
                        {
                            csvWriter.WriteField(header);
                        }
                        csvWriter.NextRecord();

                        // Write rows
                        foreach (var row in data)
                        {
                            foreach (var field in row.Values)
                            {
                                csvWriter.WriteField(field.ToString());
                            }
                            csvWriter.NextRecord();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception (e.g., log it)
            }

            return description;
        }

        //public async static Task<SaveFileDescription> SaveCSVFileInDiskDictionary(List<Dictionary<string, object>> data, string folderName = "temp", string attachmentFolder = "upload\\attachments", List<string> fieldsToConcatenate = null)
        //{
        //    fieldsToConcatenate = new List<string> { "CH Mobile" };

        //    SaveFileDescription description = new SaveFileDescription();
        //    try
        //    {
        //        description.FileExtention = ".csv";
        //        description.SavedFileName = folderName + "_" + DateTime.Now.Ticks.ToString() + ".csv";
        //        description.FileRelativePath = "/" + attachmentFolder + "/" + folderName + "/" + description.SavedFileName;
        //        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();

        //        string str = Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName);

        //        if (!Directory.Exists(str))
        //            Directory.CreateDirectory(str);

        //        string filePath = Path.Combine(str, description.SavedFileName);
        //        using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
        //        {
        //            // Write header
        //            if (data.Count > 0)
        //            {
        //                var headers = string.Join(",", data[0].Keys.Select(k => $"\"{k}\""));
        //                writer.WriteLine(headers);

        //                // Write rows
        //                foreach (var row in data)
        //                {
        //                    var values = row.Select(kv =>
        //                    {
        //                        if (fieldsToConcatenate != null && fieldsToConcatenate.Contains(kv.Key) && kv.Value != null)
        //                        {
        //                            // Add a single quote at the start of the value
        //                            return $"'{kv.Value.ToString().Replace("'", "''")}\"";
        //                        }
        //                        else
        //                        {
        //                            // Normal value
        //                            return kv.Value != null ? $"\"{kv.Value.ToString().Replace("\"", "\"\"")}\"" : "\"\"";
        //                        }
        //                    });

        //                    var line = string.Join(",", values);
        //                    writer.WriteLine(line);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle exception (e.g., log it)
        //    }

        //    return description;
        //}






        public static bool DeleteFileFromDisk(string fileName, string folderName = "temp", string attachmentFolder = "upload\\attachments")
        {
            try
            {
                IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                string str = Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName);
                if (Directory.Exists(str))
                    File.Delete(str + "\\" + fileName);
                else
                    return false;
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public static async Task<FileInfo[]> GetFilesFromDirectory(string filePath)
        {
            FileInfo[] files = null;
            try
            {
                DirectoryInfo d = new DirectoryInfo(filePath);

                return d.GetFiles();
            }
            catch (Exception ex)
            {
                return files;
            }
        }


        public static ExcelWorksheet ConvertFileToExcel(string filePath)
        {
            try
            {
                // Set EPPlus license context
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                // Verify file exists
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException("Excel file not found", filePath);
                }

                // Create file info
                var fileInfo = new FileInfo(filePath);
                
                // Verify it's an Excel file
                if (!fileInfo.Extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase) && 
                    !fileInfo.Extension.Equals(".xls", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("File is not an Excel file");
                }

                // Create new package with file
                var package = new ExcelPackage(fileInfo);

                // Verify package has worksheets
                if (package.Workbook.Worksheets.Count == 0)
                {
                    package.Dispose();
                    throw new InvalidOperationException("Excel file contains no worksheets");
                }

                // Get first worksheet
                var worksheet = package.Workbook.Worksheets[0];

                if (worksheet == null)
                {
                    package.Dispose();
                    throw new InvalidOperationException("Could not access first worksheet");
                }

                return worksheet;
            }
            catch (Exception ex)
            {
                // Log the specific error
                System.Diagnostics.Debug.WriteLine($"Error in ConvertFileToExcel: {ex.Message}");
                return null;
            }
        }


        public static T ParseExcelFileToObject<T>(T obj, ExcelWorksheet worksheet, int row, int colCount, string[] ObjOrderValue = null)
        {
            Type t = obj.GetType();
            try
            {
                string[] value = new string[colCount];
                for (int col = 1; col <= colCount; col++)
                {
                    var cellValue = worksheet.Cells[row, col].Value?.ToString();
                    value[col - 1] = cellValue.TrimStart().TrimEnd();

                }

                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                if (ObjOrderValue != null)
                {
                    int col = 1;
                    foreach (var item in ObjOrderValue)
                    {
                        while (col <= colCount)
                        {
                            var cellValue = worksheet.Cells[row, col].Value?.ToString();
                            keyValuePairs.Add(item, cellValue);
                            break;
                        }
                        col++;
                    }
                }

                int valueMinIndex = 0;
                int valueMaxIndex = value.Length;
                foreach (var propInfo in t.GetProperties())
                {
                    if (ObjOrderValue != null)
                    {
                        //foreach ( var item in ObjOrderValue)
                        for (int i = 0; i < ObjOrderValue.Length; i++)
                        {
                            if (propInfo.Name == ObjOrderValue[i] && valueMinIndex < valueMaxIndex)
                            {
                                var propType = propInfo.PropertyType.Name;
                                var typeCode = propType == "Int32" ? TypeCode.Int32 : propType == "String" ? TypeCode.String :
                                               propType == "Decimal" ? TypeCode.Decimal :
                                               propType == "Boolean" ? TypeCode.Boolean :
                                               propType == "DateTime" ? TypeCode.DateTime :
                                               propType == "Double" ? TypeCode.Double :
                                               propType == "Int64" ? TypeCode.Int64 : 0;

                                propInfo.SetValue(obj, Convert.ChangeType((value[valueMinIndex]), typeCode), null);
                                valueMinIndex++;
                                break;
                            }

                        }
                    }

                    else
                    {
                        if (valueMinIndex < valueMaxIndex)
                        {
                            var propType = propInfo.PropertyType.Name;
                            var typeCode = propType == "Int32" ? TypeCode.Int32 : propType == "String" ? TypeCode.String :
                                           propType == "Decimal" ? TypeCode.Decimal :
                                           propType == "Boolean" ? TypeCode.Boolean :
                                           propType == "DateTime" ? TypeCode.DateTime :
                                           propType == "Double" ? TypeCode.Double :
                                           propType == "Int64" ? TypeCode.Int64 : 0;

                            propInfo.SetValue(obj, Convert.ChangeType((value[valueMinIndex]), typeCode), null);
                            valueMinIndex++;
                        }
                        else { break; }
                    }

                }
                return obj;
            }
            catch (Exception ex)
            {
                return obj;
            }
        }


        public static List<T> ParseExcelFileToObjectList<T>(T obj, ExcelWorksheet worksheet, int rowCount, int colCount, string[] ObjOrderValue = null, bool fileWithHeader = true)
        {
            List<T> objList = new List<T>();

            // obj = new T();

            try
            {
                for (int row = fileWithHeader ? 2 : 1; row <= rowCount; row++)
                {
                    string[] value = new string[colCount];
                    for (int col = 1; col <= colCount; col++)
                    {
                        var cellValue = worksheet.Cells[row, col].Value?.ToString();
                        value[col - 1] = cellValue.TrimStart().TrimEnd();

                    }
                    int valueMinIndex = 0;
                    int valueMaxIndex = value.Length - 1;

                    //GenericClass<T> obj = new GenericClass<T>();
                    Type t = obj.GetType();

                    foreach (var propInfo in t.GetProperties())
                    {
                        if (ObjOrderValue != null)
                        {

                        }

                        else
                        {
                            if (valueMinIndex < valueMaxIndex)
                            {
                                var propType = propInfo.PropertyType.Name;
                                var typeCode = propType == "Int32" ? TypeCode.Int32 : propType == "String" ? TypeCode.String :
                                               propType == "Decimal" ? TypeCode.Decimal :
                                               propType == "Boolean" ? TypeCode.Boolean :
                                               propType == "DateTime" ? TypeCode.DateTime :
                                               propType == "Double" ? TypeCode.Double :
                                               propType == "Int64" ? TypeCode.Int64 : 0;

                                propInfo.SetValue(obj, Convert.ChangeType((value[valueMinIndex]), typeCode), null);
                                valueMinIndex++;
                            }
                            else { }
                        }

                    }
                    objList.Add(obj);
                }
                return objList;
            }
            catch (Exception ex)
            {
                return objList;
            }
        }

        public static string GetHostPath()
        {
            IHttpContextAccessor instance = AppContexts.GetInstance<IHttpContextAccessor>();
            string encodedPathAndQuery = UriHelper.GetEncodedPathAndQuery(instance.HttpContext.Request);
            return UriHelper.GetEncodedUrl(instance.HttpContext.Request).Replace(encodedPathAndQuery, string.Empty);
        }


        public static bool IsFileValid(byte[] file, string FileName)
        {
            var stream = new MemoryStream(file);
            FormFile file1 = new FormFile(stream, 0, file.Length, FileName, FileName);
            using (var reader = new BinaryReader(file1.OpenReadStream()))
            {
                var signatures = _fileSignatures.Values.SelectMany(x => x).ToList();  // flatten all signatures to single list
                var headerBytes = reader.ReadBytes(_fileSignatures.Max(m => m.Value.Max(n => n.Length)));
                bool result = signatures.Any(signature => headerBytes.Take(signature.Length).SequenceEqual(signature));
                return result;
            }
        }
        private static readonly Dictionary<string, List<byte[]>> _fileSignatures = new Dictionary<string, List<byte[]>>
        {
            { ".gif", new List<byte[]> { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
            { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            {
                ".jpeg",
                new List<byte[]>
                    {
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xEE },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xDB },
                    }
            },
            { ".jpeg2000", new List<byte[]> { new byte[] { 0x00, 0x00, 0x00, 0x0C, 0x6A, 0x50, 0x20, 0x20, 0x0D, 0x0A, 0x87, 0x0A } } },
            { ".jpg",
                new List<byte[]>
                    {
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xEE },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xDB },
                    }
            },
                    //    {
                    //        ".zip",
                    //        new List<byte[]> //also docx, xlsx, pptx, ...
                    //{
                    //    new byte[] { 0x50, 0x4B, 0x03, 0x04 },
                    //    new byte[] { 0x50, 0x4B, 0x4C, 0x49, 0x54, 0x45 },
                    //    new byte[] { 0x50, 0x4B, 0x53, 0x70, 0x58 },
                    //    new byte[] { 0x50, 0x4B, 0x05, 0x06 },
                    //    new byte[] { 0x50, 0x4B, 0x07, 0x08 },
                    //    new byte[] { 0x57, 0x69, 0x6E, 0x5A, 0x69, 0x70 },
                    //}
                    //    },

                    //    { ".pdf", new List<byte[]> { new byte[] { 0x25, 0x50, 0x44, 0x46 } } },
                    //    {
                    //        ".z",
                    //        new List<byte[]>
                    //{
                    //    new byte[] { 0x1F, 0x9D },
                    //    new byte[] { 0x1F, 0xA0 }
                    //}
                    //    },
                    //    {
                    //        ".tar",
                    //        new List<byte[]>
                    //{
                    //    new byte[] { 0x75, 0x73, 0x74, 0x61, 0x72, 0x00, 0x30 , 0x30 },
                    //    new byte[] { 0x75, 0x73, 0x74, 0x61, 0x72, 0x20, 0x20 , 0x00 },
                    //}
                    //    },
                    //    {
                    //        ".tar.z",
                    //        new List<byte[]>
                    //{
                    //    new byte[] { 0x1F, 0x9D },
                    //    new byte[] { 0x1F, 0xA0 }
                    //}
                    //    },
                    //    {
                    //        ".tif",
                    //        new List<byte[]>
                    //{
                    //    new byte[] { 0x49, 0x49, 0x2A, 0x00 },
                    //    new byte[] { 0x4D, 0x4D, 0x00, 0x2A }
                    //}
                    //    },
                    //    {
                    //        ".tiff",
                    //        new List<byte[]>
                    //{
                    //    new byte[] { 0x49, 0x49, 0x2A, 0x00 },
                    //    new byte[] { 0x4D, 0x4D, 0x00, 0x2A }
                    //}
                    //    },
                    //    {
                    //        ".rar",
                    //        new List<byte[]>
                    //{
                    //    new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07 , 0x00 },
                    //    new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07 , 0x01, 0x00 },
                    //}
                    //    },
                    //    {
                    //        ".7z",
                    //        new List<byte[]>
                    //{
                    //    new byte[] { 0x37, 0x7A, 0xBC, 0xAF, 0x27 , 0x1C },
                    //}
                    //    },
                    //    {
                    //        ".txt",
                    //        new List<byte[]>
                    //{
                    //    new byte[] { 0xEF, 0xBB , 0xBF },
                    //    new byte[] { 0xFF, 0xFE},
                    //    new byte[] { 0xFE, 0xFF },
                    //    new byte[] { 0x00, 0x00, 0xFE, 0xFF },
                    //}
                    //    },
                    //    {
                    //        ".mp3",
                    //        new List<byte[]>
                    //{
                    //    new byte[] { 0xFF, 0xFB },
                    //    new byte[] { 0xFF, 0xF3},
                    //    new byte[] { 0xFF, 0xF2},
                    //    new byte[] { 0x49, 0x44, 0x43},
                    //}
            //},
        };

        public static bool IsFileValidForDocument(byte[] file, string FileName)
        {
            var stream = new MemoryStream(file);
            FormFile file1 = new FormFile(stream, 0, file.Length, FileName, FileName);
            using (var reader = new BinaryReader(file1.OpenReadStream()))
            {
                var signatures = _fileSignaturesForDocument.Values.SelectMany(x => x).ToList();  // flatten all signatures to single list
                var headerBytes = reader.ReadBytes(_fileSignaturesForDocument.Max(m => m.Value.Max(n => n.Length)));
                bool result = signatures.Any(signature => headerBytes.Take(signature.Length).SequenceEqual(signature));
                return result;
            }
        }
        private static readonly Dictionary<string, List<byte[]>> _fileSignaturesForDocument = new Dictionary<string, List<byte[]>>
        {
            { ".gif", new List<byte[]> { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
            { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            {
                ".jpeg",
                new List<byte[]>
                    {
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xEE },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xDB },
                    }
            },
            { ".jpeg2000", new List<byte[]> { new byte[] { 0x00, 0x00, 0x00, 0x0C, 0x6A, 0x50, 0x20, 0x20, 0x0D, 0x0A, 0x87, 0x0A } } },
            { ".jpg",
                new List<byte[]>
                    {
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xEE },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xDB },
                    }
            },
            {
                ".docx",
                new List<byte[]> //also docx, xlsx, pptx, ...
                {
                    new byte[] { 0x50, 0x4B, 0x03, 0x04 },
                    new byte[] { 0x50, 0x4B, 0x4C, 0x49, 0x54, 0x45 },
                    new byte[] { 0x50, 0x4B, 0x53, 0x70, 0x58 },
                    new byte[] { 0x50, 0x4B, 0x05, 0x06 },
                    new byte[] { 0x50, 0x4B, 0x07, 0x08 },
                    new byte[] { 0x57, 0x69, 0x6E, 0x5A, 0x69, 0x70 },
                }
            },
            {
                ".xls",
                new List<byte[]>
                {
                    new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 },
                    new byte[] { 0x09, 0x08, 0x00, 0x00, 0x06, 0x05, 0x00 },

                }
            },
            {
                ".doc",
                new List<byte[]>
                {
                    new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 },
                    new byte[] { 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1, 0x00 },
                }
            },
            {
                ".xlsx",
                new List<byte[]>
                {
                    new byte[] { 0x50, 0x4B, 0x03, 0x04 },
                    new byte[] { 0x50, 0x4B, 0x4C, 0x49, 0x54, 0x45 },
                    new byte[] { 0x50, 0x4B, 0x53, 0x70, 0x58 },
                    new byte[] { 0x50, 0x4B, 0x05, 0x06 },
                    new byte[] { 0x50, 0x4B, 0x07, 0x08 },
                    new byte[] { 0x57, 0x69, 0x6E, 0x5A, 0x69, 0x70 },
                }
            },

            { ".pdf", new List<byte[]> { new byte[] { 0x25, 0x50, 0x44, 0x46 } } },
                   
            //    {
            //        ".txt",
            //        new List<byte[]>
            //{
            //    new byte[] { 0xEF, 0xBB , 0xBF },
            //    new byte[] { 0xFF, 0xFE},
            //    new byte[] { 0xFE, 0xFF },
            //    new byte[] { 0x00, 0x00, 0xFE, 0xFF },
            //}
            //    },
            //},
        };


        public static byte[] GetFileToByteArray(string path)
        {
            return File.ReadAllBytes(path);
        }
    }

    public class GenericParse<T> where T : class, new()
    {
        public T CreateInstance()
        {
            return new T();
        }

        public List<T> ParseExcelFileToObjectList(ExcelWorksheet worksheet, int rowCount, int colCount)
        {
            List<T> objList = new List<T>();
            Dictionary<string, string> headerMapping = new Dictionary<string, string>();
            string[] colName = new string[colCount];
            try
            {
                for (int row = 1; row <= 1; row++)
                {
                    for (int col = 1; col <= colCount; col++)
                    {
                        var cellValue = worksheet.Cells[row, col].Value?.ToString();
                        headerMapping.Add(cellValue.TrimStart().TrimEnd(), "");
                        colName[col - 1] = cellValue.TrimStart().TrimEnd();
                    }
                }
                for (int row = 2; row <= rowCount; row++)
                {
                    string[] value = new string[colCount];
                    for (int col = 1; col <= colCount; col++)
                    {
                        var cellValue = worksheet.Cells[row, col].Value?.ToString();
                        headerMapping[colName[col - 1]] = cellValue.TrimStart().TrimEnd();
                    }
                    var newInstance = CreateInstance();
                    Type t = newInstance.GetType();
                    foreach (var propInfo in t.GetProperties())
                    {
                        var propType = propInfo.PropertyType.Name;
                        var typeCode = propType == "Int32" ? TypeCode.Int32 : propType == "String" ? TypeCode.String :
                                       propType == "Decimal" ? TypeCode.Decimal :
                                       propType == "Boolean" ? TypeCode.Boolean :
                                       propType == "DateTime" ? TypeCode.DateTime :
                                       propType == "Double" ? TypeCode.Double :
                                       propType == "Int64" ? TypeCode.Int64 : TypeCode.String;

                        if (headerMapping.ContainsKey(propInfo.Name))
                        {
                            propInfo.SetValue(newInstance, Convert.ChangeType((headerMapping[propInfo.Name]), typeCode), null);
                        }
                    }
                    objList.Add(newInstance);
                }
                return objList;
            }
            catch (Exception ex)
            {
                return objList;
            }
        }

        public async Task<List<T>> GetExcelDataToModel(IFormFile UPFile, string fileSavePath)
        {
            SaveFileDescription fileDesc = new SaveFileDescription();
            string filePath = string.Empty;
            try
            {
                #region Save file in Disk & convert to ObjectList from Excel and Dispose file

                fileDesc = await UploadUtil.SaveFileInDisk(UPFile, fileSavePath);
                filePath = fileDesc.FullPath;

                if (!fileDesc.FileExtention.Equals(".xlsx"))
                {
                    File.Delete(filePath);
                    return new List<T>();
                }

                var worksheet = UploadUtil.ConvertFileToExcel(filePath);
                int rowCount = worksheet.Dimension.Rows;
                int colCount = worksheet.Dimension.Columns;

                if (rowCount < 1 || colCount < 1)
                {
                    File.Delete(filePath);
                }

                var data = new GenericParse<T>().ParseExcelFileToObjectList(worksheet, rowCount, colCount);
                worksheet.Dispose();
                File.Delete(filePath);
                return await Task.FromResult(data.ToList());
                #endregion
            }
            catch (Exception ex)
            {
                File.Delete(filePath);
                return new List<T>();
            }
        }

        public async Task<List<T>> GetCSVDataToModel(IFormFile UPFile, string fileSavePath)
        {
            SaveFileDescription fileDesc = new SaveFileDescription();
            string filePath = string.Empty;
            try
            {
                #region Save file in Disk & convert to ObjectList from Excel and Dispose file

                fileDesc = await UploadUtil.SaveFileInDisk(UPFile, fileSavePath);
                filePath = fileDesc.FullPath;

                if (!fileDesc.FileExtention.Equals(".csv"))
                {
                    File.Delete(filePath);
                    return new List<T>();
                }

                List<T> dataList = new List<T>();

                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    dataList = csv.GetRecords<T>().ToList();
                }
                File.Delete(filePath);

                return await Task.FromResult(dataList.ToList());
                #endregion
            }
            catch (Exception ex)
            {
                File.Delete(filePath);
                return new List<T>();
            }
        }

        public async Task<DataTable> GetCSVDataToDataTable(IFormFile UPFile, string fileSavePath)
        {
            DataTable dataTable = new DataTable();
            SaveFileDescription fileDesc = new SaveFileDescription();
            string filePath = string.Empty;
            try
            {
                #region Save file in Disk & convert to ObjectList from Excel and Dispose file

                fileDesc = await UploadUtil.SaveFileInDisk(UPFile, fileSavePath);
                filePath = fileDesc.FullPath;

                if (!fileDesc.FileExtention.Equals(".csv"))
                {
                    File.Delete(filePath);
                    return new DataTable();
                }

                List<T> dataList = new List<T>();

                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    dataList = csv.GetRecords<T>().ToList();
                }
                using (var reader = ObjectReader.Create(dataList))
                {
                    dataTable.Load(reader);
                }
                File.Delete(filePath);

                return await Task.FromResult(dataTable);
                #endregion
            }
            catch (Exception ex)
            {
                File.Delete(filePath);
                return new DataTable();
            }
        }

        public async Task<DataTable> GetListToDataTable(List<T> listData)
        {
            DataTable dataTable = new DataTable();
            try
            {
                #region Save file in Disk & convert to ObjectList from Excel and Dispose file

                using (var reader = ObjectReader.Create(listData))
                {
                    dataTable.Load(reader);
                }

                return await Task.FromResult(dataTable);
                #endregion
            }
            catch (Exception ex)
            {
                return new DataTable();
            }
        }

        public async Task<ExcelWorksheet> ConvertCsvToXlsx(IFormFile csvFile)
        {
            try
            {
                string fileName = csvFile.FileName.Split('.')[0];
                DataTable dataTable = new DataTable();
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using var package = new ExcelPackage();

                var worksheet = package.Workbook.Worksheets.Add("Sheet1");
                using (var memoryStream = new MemoryStream())
                {
                    using (var streamReader = new StreamReader(csvFile.OpenReadStream()))
                    using (var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
                    {
                        var d = csvReader.GetRecords<T>().ToList();
                        //var records = new List<T>();
                        //while (await csvReader.ReadAsync())
                        //{
                        //    var record = csvReader.GetRecord<T>();
                        //    records.Add(record);
                        //}
                        // Load records into the worksheet
                       // worksheet.Cells["A1"].LoadFromCollection(records, true);

                        // Save to memory stream
                        await package.SaveAsAsync(memoryStream);

                    }
                   
                    return worksheet;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        //public List<T> ConvertCsvToList(IFormFile csvFile,ref List<string> headers, string[] requiredHeaders)
        //{
        //    List<T> dataList = new List<T>();
        //    List<string[]> csvData;
        //    try
        //    {
        //        headers = new List<string>(requiredHeaders);
        //        using (var memoryStream = new MemoryStream())
        //        {
        //            using (var streamReader = new StreamReader(csvFile.OpenReadStream()))
        //            using (var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
        //            {
        //                dataList = csvReader.GetRecords<T>().ToList();
        //            }
        //        }
        //        using (var stream = new MemoryStream())
        //        {
        //            csvFile.CopyToAsync(stream);
        //            stream.Position = 0;

        //            using (var reader = new StreamReader(stream))
        //            {
        //                // Read the first line to get headers
        //                var line = reader.ReadLine();
        //                if (line != null)
        //                {
        //                    headers.AddRange(line.Split(',')); // Adjust the delimiter if needed
        //                }
        //            }
        //        }

        //        return dataList;

        //    }
        //    catch (Exception ex)
        //    {
        //        return dataList;
        //    }
        //}
        public List<T> ConvertCsvToList(IFormFile csvFile, ref List<string> headers, string[] requiredHeaders)
        {
            List<T> dataList = new List<T>();

            try
            {
                using (var streamReader = new StreamReader(csvFile.OpenReadStream()))
                using (var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
                {
                    // Read headers from the CSV file
                    csvReader.Read();
                    csvReader.ReadHeader();
                    headers = csvReader.HeaderRecord?.ToList() ?? new List<string>();

                    // Check if all headers in requiredHeaders are present in the CSV
                    var missingHeaders = requiredHeaders.Except(headers).ToList();
                    if (missingHeaders.Any())
                    {
                        throw new Exception($"The CSV is missing the following required headers: {string.Join(", ", missingHeaders)}");
                    }

                    // Read records and convert to list
                    dataList = csvReader.GetRecords<T>().ToList();
                }

                return dataList;
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                return dataList;
            }
        }

    }

    public class PayrollUpload
    {

    }
}
