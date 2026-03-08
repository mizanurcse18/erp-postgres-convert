using Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace API.Core.Logging
{
    public class FileLogger : ILogger
    {
        readonly string logFolder;

        public FileLogger()
        {
            var location = Assembly.GetEntryAssembly()?.Location;
            logFolder = $"{Path.GetDirectoryName(location)}\\Logs";
        }

        public void LogError(ErrorLog log)
        {
            Log("ErrorLog", JsonConvert.SerializeObject(log));
        }

        public void LogInformation(string log)
        {
            Log("InfoLog", log);
        }

        public void LogWarning(string log)
        {
            Log("WarningLog", log);
        }

        private void Log(string fileName, string error)
        {
            try
            {
                var logfilePath = $@"{logFolder}\{fileName}-{DateTime.Today.ToString(Util.DateFormat)}.log";
                EnsureDirectoryExists(logfilePath);

                using (var writer = new StreamWriter(logfilePath, true))
                {
                    writer.WriteLine(error);
                }
            }
            catch
            {
                // ignored
            }
        }

        private static void EnsureDirectoryExists(string filePath)
        {
            try
            {
                FileInfo fi = new FileInfo(filePath);
                if (fi.Directory != null && !fi.Directory.Exists)
                {
                    Directory.CreateDirectory(fi.DirectoryName);
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}
