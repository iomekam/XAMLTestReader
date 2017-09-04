using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XAMLTestReader
{
    public class AttributeBackingStore
    {
        private List<Attribute> _attributeList;

        private string _filePath;

        private Dictionary<string, List<Attribute>> _methodProperties;
        private List<Attribute> _classProperties;

        public string ClassName { get; set; }

        public AttributeBackingStore(string filePath)
        {
            _filePath = filePath;
            _attributeList = new List<Attribute>();
            _methodProperties = new Dictionary<string, List<Attribute>>();
            _classProperties = new List<Attribute>();

            string[] lines = System.IO.File.ReadAllLines(filePath);
            string currentMethod = null;

            foreach(string line in lines)
            {
                Attribute attribute = new Attribute(line);
                _attributeList.Add(attribute);

                if(attribute.Type == AttributeType.Method && attribute.PropertyType == AttributePropertyType.Begin)
                {
                    currentMethod = attribute.Values[0];
                    _methodProperties.Add(currentMethod, new List<Attribute>());
                }
                else if(currentMethod != null && attribute.PropertyType == AttributePropertyType.Prop) // We check for null instead of Type for cases a test has a class property under a method
                {
                    _methodProperties[currentMethod].Add(attribute);
                }
                else if (attribute.Type == AttributeType.Class && attribute.PropertyType == AttributePropertyType.Prop)
                {
                    _classProperties.Add(attribute);
                }
                else if(attribute.Type == AttributeType.Class && attribute.PropertyType == AttributePropertyType.Begin)
                {
                    ClassName = attribute.Values[0];
                }
            }
        }

        private void UpdateBackingStore(string name, Attribute attribute)
        {
            if (attribute.Type == AttributeType.Method)
            {
                Attribute methodAttribute = _attributeList.Where(a =>
                    a.Type == AttributeType.Method &&
                    a.PropertyType == AttributePropertyType.Begin &&
                    a.Values[0].ToLower().Equals(name.ToLower())).First();

                bool isCollapsedMethodGroup = !methodAttribute.GetLine().Contains("BEGIN_TEST_METHOD");
                int index = _attributeList.IndexOf(methodAttribute);

                // If we have a collapsed group, expand it so we can add a property
                if (isCollapsedMethodGroup)
                {
                    string methodData = methodAttribute.GetLine();
                    methodData = methodData.Replace("TEST_METHOD", "BEGIN_TEST_METHOD");

                    Attribute newAttribute = new Attribute(methodData);
                    _attributeList.Remove(methodAttribute);
                    _attributeList.Insert(index, newAttribute);

                    // Insert new property
                    _attributeList.Insert(index + 1, attribute);

                    // Add END_TEST_METHOD portion
                    const string endTestMethod = "END_TEST_METHOD()";
                    int spaces = methodData.IndexOf(methodData.First(c => char.IsLetter(c)));

                    newAttribute = new Attribute(methodData.Substring(0, spaces) + endTestMethod);
                    _attributeList.Insert(index + 2, newAttribute);
                }
                else
                {
                    // Search through the list starting from the method attribute until we've found the last property
                    for (int count = index; count < _attributeList.Count; count++)
                    {
                        if (_attributeList[count].PropertyType == AttributePropertyType.End && _attributeList[count].Type == AttributeType.Method)
                        {
                            _attributeList.Insert(count, attribute);
                            return;
                        }
                    }
                }
            }

            if (attribute.Type == AttributeType.Class)
            {
                Attribute classAttribute = _attributeList.Where(a =>
                    a.Type == AttributeType.Class &&
                    a.PropertyType == AttributePropertyType.Begin).First();

                int index = _attributeList.IndexOf(classAttribute);

                // Search through the list starting from the method attribute until we've found the last property
                for (int count = index; count < _attributeList.Count; count++)
                {
                    if (_attributeList[count].PropertyType == AttributePropertyType.End && _attributeList[count].Type == AttributeType.Class)
                    {
                        _attributeList.Insert(count, attribute);
                        return;
                    }
                }
            }
        }

        private void RemoveFromBackingStore(Attribute attribute)
        {
            _attributeList.Remove(attribute);
        }

        public ObservableCollection<TestProperty> GetMethodProperties(string name)
        {
            List<Attribute> attributeList = null;
            _methodProperties.TryGetValue(name, out attributeList);

            if (attributeList == null) return null;

            ObservableCollection<TestProperty> properties = new ObservableCollection<TestProperty>();

            foreach(Attribute attribute in attributeList)
            {
                properties.Add(new TestProperty(attribute, name));
            }

            properties.CollectionChanged += CollectionChanged;
            return properties;
        }

        public ObservableCollection<TestProperty> GetClassProperties()
        {
            ObservableCollection<TestProperty> properties = new ObservableCollection<TestProperty>();

            foreach (Attribute attribute in _classProperties)
            {
                string key = attribute.Values[0];
                string value = attribute.Values[1];
                properties.Add(new TestProperty(attribute, ClassName));
            }

            properties.CollectionChanged += CollectionChanged;
            return properties;
        }

        public List<KeyValuePair<string, ObservableCollection<TestProperty>>> GetAllMethodProperties()
        {
            List<KeyValuePair<string, ObservableCollection<TestProperty>>> collection = 
                new List<KeyValuePair<string, ObservableCollection<TestProperty>>>();

            foreach (var pair in _methodProperties)
            {
                ObservableCollection<TestProperty> properties = new ObservableCollection<TestProperty>();
                foreach (var attribute in pair.Value)
                {
                    TestProperty prop = new TestProperty(attribute, pair.Key);
                    properties.Add(prop);
                }
                collection.Add(new KeyValuePair<string, ObservableCollection<TestProperty>>(pair.Key, properties));
                properties.CollectionChanged += CollectionChanged;
            }

            
            return collection;
        }

        public ObservableCollection<TestProperty> GetAllProperties()
        {
            ObservableCollection<TestProperty> properties = new ObservableCollection<TestProperty>();

            foreach(var attribute in _classProperties)
            {
                TestProperty prop = new TestProperty(attribute, ClassName);
                properties.Add(prop);
            }

            foreach (var pair in _methodProperties)
            {
                foreach (var attribute in pair.Value)
                {
                    TestProperty prop = new TestProperty(attribute, pair.Key);
                    properties.Add(prop);
                }
            }

            properties.CollectionChanged += CollectionChanged;
            return properties;
        }

        private void CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add) CollectionChanged_Add(e.NewItems);
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                CollectionChanged_Remove(e.OldItems);
            }
        }

        private void CollectionChanged_Add(IList newItems)
        {
            foreach (var item in newItems)
            {
                var prop = item as TestProperty;
                Attribute attribute = null;

                if (prop.Type == TestPropertyType.Method)
                {
                    // If this method group already has a method property, reuse that for this new property
                    if (_methodProperties[prop.GroupName].Count > 0)
                    {
                        attribute = new Attribute(_methodProperties[prop.GroupName].First(), prop.Key, prop.Value);
                    }
                    else
                    {
                        Attribute methodAttribute = _attributeList.Where(a =>
                           a.Type == AttributeType.Method &&
                           a.PropertyType == AttributePropertyType.Begin &&
                           a.Values[0].ToLower().Equals(prop.GroupName.ToLower())).First();

                        const string testProp = "TEST_METHOD_PROPERTY(L\"KEY\", L\"VALUE\")";

                        string methodData = methodAttribute.GetLine();
                        int spaces = methodData.IndexOf(methodData.First(c => char.IsLetter(c)));
                        attribute = new Attribute(methodData.Substring(0, spaces) + "    " + testProp);
                        attribute.Values[0] = prop.Key;
                        attribute.Values[1] = prop.Value;
                    }

                    _methodProperties[prop.GroupName].Add(attribute);
                    UpdateBackingStore(prop.GroupName, attribute);
                }
                else if(prop.Type == TestPropertyType.Class)
                {
                    _classProperties.Add(attribute);
                    UpdateBackingStore(prop.GroupName, attribute);
                }
            }
        }

        private void CollectionChanged_Remove(IList newItems)
        {
            foreach (var item in newItems)
            {
                var prop = item as TestProperty;
                Attribute attribute = null;

                if (prop.Type == TestPropertyType.Class)
                {
                    attribute = _classProperties.First(a => a.Values[0] == prop.Key);

                    _classProperties.Remove(attribute);
                    RemoveFromBackingStore(attribute);
                }
                else if(prop.Type == TestPropertyType.Method)
                {
                    attribute = _methodProperties[prop.GroupName].First(a => a.Values[0] == prop.Key);

                    _methodProperties[prop.GroupName].Remove(attribute);
                    RemoveFromBackingStore(attribute);
                }
            }
        }

        public void Print()
        {
            foreach(Attribute attribute in _attributeList)
            {
                Console.WriteLine(attribute.GetLine());
            }
        }
    }
}
