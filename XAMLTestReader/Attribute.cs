using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XAMLTestReader
{
    public enum AttributeType
    {
        None,
        Class,
        Method
    }

    public enum AttributePropertyType
    {
        None,
        Begin,
        Prop,
        End,
        Setup,
        Cleanup
    }

    public class Attribute : IEquatable<Attribute>
    {
        private string _data;

        public AttributePropertyType PropertyType { get; set; }
        public AttributeType Type { get; set; }
        public string[] Values { get; private set; }

        public Attribute(string data)
        {
            _data = data;

            var types = GetAttributeTypes(data);
            this.Type = types.Item1;
            this.PropertyType = types.Item2;

            ExtractDataAndReformat();
        }

        public Attribute(Attribute attribute, params string[] values)
        {
            _data = attribute._data;
            Type = attribute.Type;
            PropertyType = attribute.PropertyType;
            Values = values;
        }

        public string GetLine()
        {
            if (Type != AttributeType.None) return string.Format(_data, Values);
            else return _data;
        }

        public bool Equals(Attribute other)
        {
            return Type == other.Type && PropertyType == other.PropertyType && _data == other._data && 
                Values.SequenceEqual(other.Values);
        }

        private static Tuple<AttributeType, AttributePropertyType> GetAttributeTypes(string line)
        {
            if (line.Contains("BEGIN_TEST_CLASS")) return new Tuple<AttributeType, AttributePropertyType>(AttributeType.Class, AttributePropertyType.Begin);
            if (line.Contains("BEGIN_TEST_METHOD")) return new Tuple<AttributeType, AttributePropertyType>(AttributeType.Method, AttributePropertyType.Begin);

            if (line.Contains("TEST_CLASS_SETUP")) return new Tuple<AttributeType, AttributePropertyType>(AttributeType.Class, AttributePropertyType.Setup);
            if (line.Contains("TEST_METHOD_SETUP")) return new Tuple<AttributeType, AttributePropertyType>(AttributeType.Method, AttributePropertyType.Setup);

            if (line.Contains("TEST_CLASS_CLEANUP")) return new Tuple<AttributeType, AttributePropertyType>(AttributeType.Class, AttributePropertyType.Cleanup);
            if (line.Contains("TEST_METHOD_CLEANUP")) return new Tuple<AttributeType, AttributePropertyType>(AttributeType.Method, AttributePropertyType.Cleanup);

            if (line.Contains("TEST_CLASS_PROPERTY")) return new Tuple<AttributeType, AttributePropertyType>(AttributeType.Class, AttributePropertyType.Prop);
            if (line.Contains("TEST_METHOD_PROPERTY")) return new Tuple<AttributeType, AttributePropertyType>(AttributeType.Method, AttributePropertyType.Prop);

            if (line.Contains("END_TEST_CLASS")) return new Tuple<AttributeType, AttributePropertyType>(AttributeType.Class, AttributePropertyType.End);
            if (line.Contains("END_TEST_METHOD")) return new Tuple<AttributeType, AttributePropertyType>(AttributeType.Method, AttributePropertyType.End);

            if (line.Contains("TEST_METHOD") && !line.Contains("BEGIN_TEST_METHOD") && !line.Contains("END_TEST_METHOD"))
                return new Tuple<AttributeType, AttributePropertyType>(AttributeType.Method, AttributePropertyType.Begin);

            return new Tuple<AttributeType, AttributePropertyType>(AttributeType.None, AttributePropertyType.None);
        }

        private void ExtractDataAndReformat()
        {
            // Regardless of type, all strings have in common the fact that entries start after the '(',
            // so we will search for the first '(' and go from there.

            int leftParenIndex = _data.IndexOf('(');
            int rightParenIndex = 0;
            int length = 0;

            switch (PropertyType)
            {
                case AttributePropertyType.End:
                    // No data
                    Values = new string[0];
                    break;
                case AttributePropertyType.Begin:
                case AttributePropertyType.Cleanup:
                case AttributePropertyType.Setup:
                    // One value expected, which is the name
                    rightParenIndex = _data.IndexOf(')');
                    length = rightParenIndex - leftParenIndex - 1;
                    Values = new string[1] { _data.Substring(leftParenIndex + 1, length)};

                    // format string to look like this: Blah({0})
                    _data = _data.Remove(leftParenIndex + 1, length).Insert(leftParenIndex+1, "{0}");
                    break;
                case AttributePropertyType.Prop:
                    // two values expected
                    int keyStart = leftParenIndex + 3; // incorporate the L" portion as well
                    int keyEnd = _data.Substring(keyStart).IndexOf('\"') + keyStart; // index of second quote of key

                    length = keyEnd - keyStart;
                    string key = _data.Substring(keyStart, length);

                    // Value is a little harder since it doesn't have to be a string. To identift, we will look
                    // for the first letter after the comma
                    int comma = _data.Substring(keyEnd).IndexOf(',') + keyEnd; // index of comma
                    int quote = _data.Substring(comma).IndexOf("L\""); // find the first quote. If there is a quote
                    int valueSearchStartIndex = quote == -1 ? comma : comma + quote + 2; // If a quote was not found, then this value is not a string

                    char letter = _data.Substring(valueSearchStartIndex).First(c => char.IsLetter(c));
                    int valueStart = _data.Substring(valueSearchStartIndex).IndexOf(letter) + valueSearchStartIndex; // index of first letter after comma
                    int endParen = _data.Substring(valueSearchStartIndex).IndexOf(')') + valueSearchStartIndex;
                    letter = _data.Substring(valueSearchStartIndex, endParen - valueSearchStartIndex).Last(c => char.IsLetterOrDigit(c)); // get the last letter between the start of the value and end of parenthesis
                    int valueEnd = _data.Substring(valueSearchStartIndex, endParen - valueSearchStartIndex).LastIndexOf(letter) + valueSearchStartIndex + 1; // index of last letter after comma

                    int valuelength = valueEnd - valueStart;
                    string value = _data.Substring(valueStart, valuelength);

                    // format string to look like this: Blah({0}, {1})
                    _data = _data.Remove(keyStart, length).Insert(keyStart, "{0}");

                    valueStart -= length - "{0}".Length;
                    _data = _data.Remove(valueStart, valuelength).Insert(valueStart, "{1}");

                    Values = new string[2] { key, value };

                    break;
            }
        }
    }
}
