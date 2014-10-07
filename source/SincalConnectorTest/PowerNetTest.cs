using Microsoft.VisualStudio.TestTools.UnitTesting;
using SincalConnector;

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
        [DeploymentItem("testdata/node_files/database.mdb")]
        public void Constructor_NetWithTwoNodes_NodeValuesAreCorrect()
        {
            var powerNet = new PowerNet("database.mdb");

            var nodes = powerNet.Nodes;
            Assert.AreEqual(1, nodes[0].Id);
            Assert.AreEqual("N1", nodes[0].Name);
            Assert.AreEqual(1000, nodes[0].NominalVoltage, 0.0001);
            Assert.AreEqual(2, nodes[1].Id);
            Assert.AreEqual("N2", nodes[1].Name);
            Assert.AreEqual(1000, nodes[1].NominalVoltage, 0.0001);
        }

        #endregion
    }
}
