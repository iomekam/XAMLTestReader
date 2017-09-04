using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XAMLTestReader2
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
            if (line.Contains("TestClass")) return new Tuple<AttributeType, AttributePropertyType>(AttributeType.Class, AttributePropertyType.Begin);
            if (line.Contains("ClassInitialize")) return new Tuple<AttributeType, AttributePropertyType>(AttributeType.Class, AttributePropertyType.Setup);
            if (line.Contains("TestSetup")) return new Tuple<AttributeType, AttributePropertyType>(AttributeType.Method, AttributePropertyType.Setup);
            if (line.Contains("TestCleanup")) return new Tuple<AttributeType, AttributePropertyType>(AttributeType.Method, AttributePropertyType.Cleanup);
            if (line.Contains("TestProperty")) return new Tuple<AttributeType, AttributePropertyType>(AttributeType.Method, AttributePropertyType.Prop);

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
                    break;
                case AttributePropertyType.Prop:

                    break;
            }
        }
    }
}
