namespace DR.Sleipner.Config.Parsers
{
    public interface IParameterParser
    {
        bool IsMatch(object value);
    }
}