using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XAMLTestReader
{
    public class Test
    {
        private AttributeBackingStore _backingStore;

        public Test(string filePath)
        {
            _backingStore = new AttributeBackingStore(filePath);
        }

        public ObservableCollection<TestProperty> GetMethodProperties(string name)
        {
            return _backingStore.GetMethodProperties(name);
        }

        public ObservableCollection<TestProperty> GetAllProperties()
        {
            return _backingStore.GetAllProperties();
        }

        public void UpdateTestPropery(TestProperty info)
        {

        }

        public void Print()
        {
            _backingStore.Print();
        }
    }
}
