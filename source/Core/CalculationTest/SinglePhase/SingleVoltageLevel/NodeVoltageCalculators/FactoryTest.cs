using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculationTest.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    [TestClass]
    public class FactoryTest
    {
        private Factory _factory;

        [TestInitialize]
        public void SetUp()
        {
            _factory = new Factory();
        }

        [TestMethod]
        public void Constructor_Empty_InitialValuesAreReasonable()
        {
            Assert.AreEqual(64, _factory.BitPrecision);
            Assert.IsTrue(_factory.TargetPrecision < 1);
            Assert.IsTrue(_factory.CoefficientCount > 10);
            Assert.IsTrue(_factory.MaximumIterations > 0);
        }

        [TestMethod]
        public void CreateNodeVoltageCalculator_NodePotential_NodePotentialMethod()
        {
            var calculator = _factory.CreateNodeVoltageCalculator(Selection.NodePotential);

            var calculatorCasted = calculator as NodePotentialMethod;
            Assert.IsNotNull(calculatorCasted);
        }

        [TestMethod]
        public void CreateNodeVoltageCalculator_CurrentIteration_CurrentIterationWithCorrectValues()
        {
            _factory.MaximumIterations = 1234;
            _factory.TargetPrecision = 0.4567;

            var calculator = _factory.CreateNodeVoltageCalculator(Selection.CurrentIteration);

            var calculatorCasted = calculator as CurrentIteration;
            Assert.IsNotNull(calculatorCasted);
            Assert.AreEqual(1234, calculatorCasted.MaximumIterations);
            Assert.AreEqual(0.4567, calculatorCasted.TargetPrecision, 1e-10);
        }

        [TestMethod]
        public void CreateNodeVoltageCalculator_NewtonRaphson_NewtonRaphsonWithCorrectValues()
        {
            _factory.MaximumIterations = 1234;
            _factory.TargetPrecision = 0.4567;

            var calculator = _factory.CreateNodeVoltageCalculator(Selection.NewtonRaphson);

            var calculatorCasted = calculator as NewtonRaphsonMethod;
            Assert.IsNotNull(calculatorCasted);
            Assert.AreEqual(1234, calculatorCasted.MaximumIterations);
            Assert.AreEqual(0.4567, calculatorCasted.TargetPrecision, 1e-10);
        }

        [TestMethod]
        public void CreateNodeVoltageCalculator_FastDecoupledLoadFlow_FastDecoupledLoadFlowWithCorrectValues()
        {
            _factory.MaximumIterations = 1234;
            _factory.TargetPrecision = 0.4567;

            var calculator = _factory.CreateNodeVoltageCalculator(Selection.NewtonRaphson);

            var calculatorCasted = calculator as NewtonRaphsonMethod;
            Assert.IsNotNull(calculatorCasted);
            Assert.AreEqual(1234, calculatorCasted.MaximumIterations);
            Assert.AreEqual(0.4567, calculatorCasted.TargetPrecision, 1e-10);
        }

        [TestMethod]
        public void CreateNodeVoltageCalculator_HelmWith64Bit_HelmWithCorrectValues()
        {
            _factory.TargetPrecision = 0.4567;
            _factory.BitPrecision = 64;
            _factory.CoefficientCount = 123;

            var calculator = _factory.CreateNodeVoltageCalculator(Selection.HolomorphicEmbeddedLoadFlow);

            var calculatorCasted = calculator as HolomorphicEmbeddedLoadFlowMethod;
            Assert.IsNotNull(calculatorCasted);
            Assert.AreEqual(0.4567, calculatorCasted.TargetPrecision, 1e-10);
            Assert.AreEqual(64, calculatorCasted.Precision.Bits);
        }

        [TestMethod]
        public void CreateNodeVoltageCalculator_HelmWith200Bit_HelmWithCorrectValues()
        {
            _factory.TargetPrecision = 0.4567;
            _factory.BitPrecision = 200;
            _factory.CoefficientCount = 123;

            var calculator = _factory.CreateNodeVoltageCalculator(Selection.HolomorphicEmbeddedLoadFlow);

            var calculatorCasted = calculator as HolomorphicEmbeddedLoadFlowMethod;
            Assert.IsNotNull(calculatorCasted);
            Assert.AreEqual(0.4567, calculatorCasted.TargetPrecision, 1e-10);
            Assert.AreEqual(200, calculatorCasted.Precision.Bits);
        }
    }
}
