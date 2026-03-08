using System;
using System.Collections.Generic;
using System.Text;

namespace API.Core.Logging
{
    public interface ILogger
    {
        void LogInformation(string log);
        void LogWarning(string log);
        void LogError(ErrorLog log);
    }
}
