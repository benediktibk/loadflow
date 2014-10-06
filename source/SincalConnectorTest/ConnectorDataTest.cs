using Microsoft.VisualStudio.TestTools.UnitTesting;
using SincalConnector;

namespace SincalConnectorTest
{
    [TestClass]
    public class ConnectorDataTest
    {
        #region variables

        private ConnectorData _connectorData;

        #endregion

        #region set up

        [TestInitialize]
        public void SetUp()
        {
            _connectorData = new ConnectorData();
        }

        #endregion

        #region tests

        [TestMethod]
        public void Constructor_Empty_IsValid()
        {
            Assert.IsTrue(_connectorData.IsValid);
        }

        #endregion
    }
}
