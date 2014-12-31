using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculationTest.SinglePhase.SingleVoltageLevel
{
    [TestClass]
    public class PowerNetFactoryTest
    {
        private PowerNetFactory _factory;
        private IAdmittanceMatrix _admittanceMatrix;
        private INodeVoltageCalculator _nodeVoltageCalculator;

        [TestInitialize]
        public void SetUp()
        {
            _admittanceMatrix = new AdmittanceMatrix(3);
            _nodeVoltageCalculator = new NodePotentialMethod(false);
            _factory = new PowerNetFactory(_nodeVoltageCalculator);
        }

        [TestMethod]
        public void Create_ValidParameter_NodeVoltageCalculatorOfResultIsCorrect()
        {
            var powerNet = _factory.Create(_admittanceMatrix, 5);

            Assert.AreEqual(_nodeVoltageCalculator, powerNet.NodeVoltageCalculator);
        }

        [TestMethod]
        public void Create_ValidParameter_AdmittanceMatrixOfResultIsCorrect()
        {
            var powerNet = _factory.Create(_admittanceMatrix, 5);

            Assert.AreEqual(_admittanceMatrix, powerNet.Admittances);
        }

        [TestMethod]
        public void Create_ValidParameter_NominalVoltageOfResultIsCorrect()
        {
            var powerNet = _factory.Create(_admittanceMatrix, 5);

            Assert.AreEqual(5, powerNet.NominalVoltage, 0.00001);
        }
    }
}
