using System.Collections.Generic;
using System.Threading.Tasks;

namespace Manager.Core
{
    public interface IManager
    {
        UniqueCode GenerateSystemCode(string tableName, string companyId, short addNumber = 1, string prefix = "", string suffix = "");
        Task<List<Dictionary<string, object>>> GetApiPath(string apipath, int UserID);
        Task<List<Dictionary<string, object>>> GetBlackListToken(string token, int UserID);
        bool GetValidatePath(int employeeID, string filePath);
        bool CheckChangeValidUser( int UserID);
    }
}
