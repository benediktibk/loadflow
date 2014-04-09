using System.Numerics;
using LoadFlowCalculation.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestHelper;

namespace LoadFlowCalculationTest.MultipleVoltageLevels
{
    [TestClass]
    public class LoadTest
    {
        [TestMethod]
        public void Constructor_hanzAsName_NameIshanz()
        {
            var node = new Node("heinz", 4);
            var load = new Load("hanz", new Complex(4, 1), node);

            Assert.AreEqual("hanz", load.Name);
        }

        [TestMethod]
        public void Constructor_4AsNominalVoltage_NominalVoltageIs4()
        {
            var node = new Node("heinz", 4);
            var load = new Load("hanz", new Complex(4, 1), node);

            Assert.AreEqual(4, load.NominalVoltage, 0.00001);
        }

        [TestMethod]
        public void Constructor_ValidLoad_LoadIsCorrect()
        {
            var node = new Node("heinz", 4);
            var load = new Load("hanz", new Complex(4, 1), node);

            ComplexAssert.AreEqual(4, 1, load.Value, 0.00001);
        }

        [TestMethod]
        public void Constructor_ValidNode_NodeHasNoConnectedElements()
        {
            var node = new Node("heinz", 4);
            var load = new Load("hanz", new Complex(4, 1), node);

            Assert.AreEqual(0, node.ConnectedElements.Count);
        }

        [TestMethod]
        public void NominalVoltage_NodeHasNominalVoltageOf3_3()
        {
            var node = new Node("heinz", 3);
            var load = new Load("hanz", new Complex(4, 1), node);

            Assert.AreEqual(3, load.NominalVoltage, 0.00001);
        }
    }
}
