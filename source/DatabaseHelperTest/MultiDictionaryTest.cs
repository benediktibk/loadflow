using DatabaseHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseHelperTest
{
    [TestClass]
    public class MultiDictionaryTest
    {
        [TestMethod]
        public void Add_TwoDifferentKey_ValuesAreCorrect()
        {
            var dictionary = new MultiDictionary<int,char>();

            dictionary.Add(5, 'c');
            dictionary.Add(1, 'a');

            var fiveValues = dictionary.Get(5);
            var oneValues = dictionary.Get(1);
            Assert.AreEqual(1, fiveValues.Count);
            Assert.AreEqual(1, oneValues.Count);
            Assert.AreEqual('c', fiveValues[0]);
            Assert.AreEqual('a', oneValues[0]);
        }

        [TestMethod]
        public void Add_TwoDifferentKeysAndOneTwice_ValuesAreCorrect()
        {
            var dictionary = new MultiDictionary<int, char>();

            dictionary.Add(5, 'c');
            dictionary.Add(1, 'a');
            dictionary.Add(5, 'z');

            var fiveValues = dictionary.Get(5);
            var oneValues = dictionary.Get(1);
            Assert.AreEqual(2, fiveValues.Count);
            Assert.AreEqual(1, oneValues.Count);
            Assert.AreEqual('c', fiveValues[0]);
            Assert.AreEqual('z', fiveValues[1]);
            Assert.AreEqual('a', oneValues[0]);
        }
    }
}
