namespace DAL.Core.Helper
{
    internal interface ISqlGen
    {
        string QuoteIdentifier(string name);
        string QuoteIdentifierStoreFunctionName(string name);
        string IsNullFunction();
    }
}
