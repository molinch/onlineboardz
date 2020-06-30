using System.Text;

namespace Api.Persistence
{
    public class StringMatchBuilder
    {
        private StringBuilder _buffer = new StringBuilder();

        public StringMatchBuilder Add(string match)
        {
            _buffer.Append(match);
            return this;
        }

        public StringMatchBuilder Equal<T>(string fieldPath, T matchValue)
        {
            var value = matchValue == null ? "null" : matchValue.ToString();
            return EqualValueNonQuoted(fieldPath, $"'{value}'");
        }

        public StringMatchBuilder Equal(string fieldPath, int matchValue)
        {
            return EqualValueNonQuoted(fieldPath, matchValue.ToString());
        }

        public StringMatchBuilder IsNull(string fieldPath)
        {
            return EqualValueNonQuoted(fieldPath, "{ $type: 10 }");  // $type 10 is null
        }

        private StringMatchBuilder EqualValueNonQuoted(string fieldPath, string matchValue)
        {
            if (_buffer.Length > 0)
            {
                _buffer.Append(",");
            }

            _buffer.Append($"'{fieldPath}': {matchValue}");
            return this;
        }

        public string Build()
        {
            return "{ " + _buffer.ToString() + " }";
        }
    }
}
