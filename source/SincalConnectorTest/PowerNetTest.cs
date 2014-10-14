using System;
using System.Linq;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SincalConnector;
using UnitTestHelper;

namespace SincalConnectorTest
{
    [TestClass]
    public class PowerNetTest
    {
        #region variables

        

        #endregion

        #region setup

        [TestInitialize]
        public void SetUp()
        {
           
        }

        #endregion

        #region tests

        [TestMethod]
        public void Constructor_NetWithTwoNodes_NodeValuesAreCorrect()
        {
            var powerNet = new PowerNet("testdata/node_files/database.mdb");

            var nodes = powerNet.Nodes;
            Assert.AreEqual(1, nodes[0].Id);
            Assert.AreEqual("N1", nodes[0].Name);
            Assert.AreEqual(1000, nodes[0].NominalVoltage, 0.0001);
            Assert.AreEqual(2, nodes[1].Id);
            Assert.AreEqual("N2", nodes[1].Name);
            Assert.AreEqual(1000, nodes[1].NominalVoltage, 0.0001);
        }

        [TestMethod]
        public void Constructor_NetWithTwoNodesAt10kV_NodeValuesAreCorrect()
        {
            var powerNet = new PowerNet("testdata/node_10kV_files/database.mdb");

            var nodes = powerNet.Nodes;
            Assert.AreEqual(1, nodes[0].Id);
            Assert.AreEqual("N1", nodes[0].Name);
            Assert.AreEqual(10000, nodes[0].NominalVoltage, 0.0001);
            Assert.AreEqual(2, nodes[1].Id);
            Assert.AreEqual("N2", nodes[1].Name);
            Assert.AreEqual(10000, nodes[1].NominalVoltage, 0.0001);
        }

        [TestMethod]
        public void Constructor_VillageNet_116Nodes()
        {
            var powerNet = new PowerNet("testdata/dorfnetz_files/database.mdb");

            Assert.AreEqual(116, powerNet.Nodes.Count);
        }

        [TestMethod]
        public void Constructor_NetWithTwoNodes_FeedInValuesAreCorrect()
        {
            var powerNet = new PowerNet("testdata/node_files/database.mdb");

            var feedIns = powerNet.FeedIns;
            Assert.AreEqual(1, feedIns.Count);
            var feedIn = feedIns[0];
            var voltage = Complex.FromPolarCoordinates(990, 10 * Math.PI / 180);
            ComplexAssert.AreEqual(voltage, feedIn.Voltage, 0.000001);
            Assert.AreEqual(1e9, feedIn.ShortCircuitPower, 0.000001);
            Assert.AreEqual(0.1, feedIn.RealToImaginaryRatio, 0.000001);
            Assert.AreEqual(1.2, feedIn.C, 0.000001);
        }

        [TestMethod]
        public void Constructor_NetWithTwoNodesAt10kV_FeedInVoltageIsCorrect()
        {
            var powerNet = new PowerNet("testdata/node_10kv_files/database.mdb");

            var feedIns = powerNet.FeedIns;
            Assert.AreEqual(1, feedIns.Count);
            var feedIn = feedIns[0];
            var voltage = Complex.FromPolarCoordinates(10100, 0);
            ComplexAssert.AreEqual(voltage, feedIn.Voltage, 0.000001);
        }

        [TestMethod]
        public void Constructor_NetWithSpecialFeedIn_FeedInVoltageIsCorrect()
        {
            var powerNet = new PowerNet("testdata/feedin_files/database.mdb");

            var feedIns = powerNet.FeedIns;
            Assert.AreEqual(1, feedIns.Count);
            var feedIn = feedIns[0];
            var voltage = Complex.FromPolarCoordinates(1050, 10*Math.PI/180);
            ComplexAssert.AreEqual(voltage, feedIn.Voltage, 0.000001);
        }

        [TestMethod]
        public void Constructor_NetWithTwoNodes_LoadIsCorrect()
        {
            var powerNet = new PowerNet("testdata/node_files/database.mdb");

            var loads = powerNet.Loads;
            Assert.AreEqual(1, loads.Count);
            var load = loads[0];
            ComplexAssert.AreEqual(1e5, 1e4, load.LoadValue, 0.000001);
        }

        [TestMethod]
        public void Constructor_NetWithTwoNodes_FrequencyIs50()
        {
            var powerNet = new PowerNet("testdata/node_files/database.mdb");

            Assert.AreEqual(50, powerNet.Frequency, 0.000001);
        }

        [TestMethod]
        public void Constructor_NetWithTwoNodes_TransmissionLineIsCorrect()
        {
            var powerNet = new PowerNet("testdata/node_files/database.mdb");

            var transmissionLines = powerNet.TransmissionLines;
            Assert.AreEqual(1, transmissionLines.Count);
            var transmissionLine = transmissionLines.First();
            Assert.AreEqual(1000, transmissionLine.Length, 0.000001);
            Assert.AreEqual(1e-4, transmissionLine.SeriesResistancePerUnitLength, 0.000001);
            Assert.AreEqual(1.273239545e-6, transmissionLine.SeriesInductancePerUnitLength, 0.000001);
            Assert.AreEqual(2e7, transmissionLine.ShuntConductancePerUnitLength, 0.000001);
            Assert.AreEqual(0.2e-12, transmissionLine.ShuntCapacityPerUnitLength, 0.000001);
            Assert.IsTrue(transmissionLine.TransmissionEquationModel);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void Constructor_NetWithUnsupportedElement_ThrowsException()
        {
            var powerNet = new PowerNet("testdata/unsupported_files/database.mdb");
        }

        #endregion
    }
}
