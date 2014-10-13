using System;
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
            var voltage = Complex.FromPolarCoordinates(990, 10*Math.PI/180);
            ComplexAssert.AreEqual(voltage, feedIn.Voltage, 0.000001);
            Assert.AreEqual(1e9, feedIn.ShortCircuitPower, 0.000001);
            Assert.AreEqual(0.1, feedIn.RealToImaginaryRatio, 0.000001);
            Assert.AreEqual(1.2, feedIn.C, 0.000001);
        }

        #endregion
    }
}
