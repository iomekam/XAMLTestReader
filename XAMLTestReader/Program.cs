using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XAMLTestReader
{
    class Program
    {
        static void Main(string[] args)
        {
            AttributeBackingStore store = new AttributeBackingStore(@"..\..\nativeTest.h");

            var allMethods = store.GetAllMethodProperties();
            allMethods.ForEach(p => p.Value.Add(new TestProperty(TestPropertyType.Method, p.Key, "Ignore", "TRUE")));

            store.Print();
        }
    }
}
