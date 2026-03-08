using System;

namespace DAL.Core.Helper
{
    internal class OracleGen : ISqlGen
    {
        public string IsNullFunction()
        {
            return "NVL";
        }

        public string QuoteIdentifier(string name)
        {
            return "\"" + name.Replace("\"", "\"\"") + "\"";
        }

        public string QuoteIdentifierStoreFunctionName(string name)
        {
            if (!name.Contains(".")) return "\"" + name + "\"";

            int num = 0;
            for (int index = name.IndexOf(".", StringComparison.Ordinal); index != -1; index = name.IndexOf(".", index + 1, StringComparison.Ordinal))
                ++num;

            if (num == 1)
                return "\"" + name.Replace(".", "\".\"") + "\"";

            int length = name.LastIndexOf(".", StringComparison.Ordinal);

            return "\"" + name.Substring(0, length) + "\".\"" + name.Substring(length + 1) + "\"";
        }
    }
}