namespace DAL.Core.Helper
{
    internal class SqlGen : ISqlGen
    {
        public string IsNullFunction()
        {
            return "ISNULL";
        }

        public string QuoteIdentifier(string name)
        {
            return "[" + name.Replace("]", "]]") + "]";
        }

        public string QuoteIdentifierStoreFunctionName(string name)
        {
            return "[" + name.Replace("]", "]]") + "]";
        }
    }
}
