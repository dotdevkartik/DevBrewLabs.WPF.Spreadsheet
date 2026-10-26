using DevBrewLabs.Parserly;
using System.Text.RegularExpressions;

namespace DevBrewLabs.Spreadsheet.CalcEngine.Parsers.TokenParsers
{
    internal class CellRefTokenParser : Parser<StringResult>
    {
        private static readonly Regex _refRegex = new Regex(@"^(('([^']+)'|([A-Za-z0-9_ ]+))!)?\$?[a-zA-Z]+\$?[0-9]+", RegexOptions.Compiled);

        protected override IParserState ParseInput(IParserState inputState)
        {
            var match = _refRegex.Match(inputState.ActualInput.Substring(inputState.Index));

            if (match.Success)
            {
                return ParserStates.Result(inputState, new StringResult(match.Value), inputState.Index + match.Length);
            }
            else
            {
                return ParserStates.Error(inputState, new ParserError(inputState.Index, "Invalid cell ref value"));
            }
        }
    }
}
