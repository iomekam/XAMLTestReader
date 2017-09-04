using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XAMLTestReader
{
    public enum TestPropertyType
    {
        Class,
        Method
    }
    public class TestProperty
    {
        private Attribute _attribute;
        private string _key;
        private string _value;

        public string GroupName { get; private set; }
        public string Key
        {
            get { return _key; }
            set { _key = value; _attribute.Values[0] = value; }
        }

        public string Value
        {
            get { return _value; }
            set { _value = value; _attribute.Values[1] = value; }
        }

        public TestPropertyType Type { get; private set; }

        public TestProperty(Attribute attribute, string groupName)
        {
            _attribute = attribute;

            GroupName = groupName;
            _key = _attribute.Values[0];
            _value = _attribute.Values[1];
            Type = _attribute.Type == AttributeType.Class ? TestPropertyType.Class : TestPropertyType.Method;
        }

        public TestProperty(TestPropertyType type, string groupName, string key, string value)
        {
            _key = key;
            _value = value;
            GroupName = groupName;
            Type = type;
        }
    }
}
