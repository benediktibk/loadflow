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
            Assert.AreEqual(64, calculatorCasted.BitPrecision);
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
            Assert.AreEqual(200, calculatorCasted.BitPrecision);
        }

        [TestMethod]
        public void CreateNodeVoltageCalculator_HelmWithCurrentIteration_HelmWithCurrentIterationWithCorrectValues()
        {
            _factory.MaximumIterations = 1234;
            _factory.TargetPrecision = 0.4567;
            _factory.BitPrecision = 200;
            _factory.CoefficientCount = 123;

            var calculator = _factory.CreateNodeVoltageCalculator(Selection.HolomorphicEmbeddedLoadFlowWithCurrentIteration);

            var calculatorCasted = calculator as TwoStepMethod;
            Assert.IsNotNull(calculatorCasted);
            var firstMethod = calculatorCasted.FirstMethod as HolomorphicEmbeddedLoadFlowMethod;
            var secondMethod = calculatorCasted.SecondMethod as CurrentIteration;
            Assert.IsNotNull(firstMethod);
            Assert.IsNotNull(secondMethod);
            Assert.AreEqual(0.4567, firstMethod.TargetPrecision, 1e-10);
            Assert.AreEqual(123, firstMethod.NumberOfCoefficients);
            Assert.AreEqual(200, firstMethod.BitPrecision);
            Assert.AreEqual(1234, secondMethod.MaximumIterations);
            Assert.AreEqual(0.4567, secondMethod.TargetPrecision, 1e-10);
        }

        [TestMethod]
        public void CreateNodeVoltageCalculator_HelmWithNewtonRaphson_HelmWithNewtonRaphsonWithCorrectValues()
        {
            _factory.MaximumIterations = 1234;
            _factory.TargetPrecision = 0.4567;
            _factory.BitPrecision = 200;
            _factory.CoefficientCount = 123;

            var calculator = _factory.CreateNodeVoltageCalculator(Selection.HolomorphicEmbeddedLoadFlowWithNewtonRaphson);

            var calculatorCasted = calculator as TwoStepMethod;
            Assert.IsNotNull(calculatorCasted);
            var firstMethod = calculatorCasted.FirstMethod as HolomorphicEmbeddedLoadFlowMethod;
            var secondMethod = calculatorCasted.SecondMethod as NewtonRaphsonMethod;
            Assert.IsNotNull(firstMethod);
            Assert.IsNotNull(secondMethod);
            Assert.AreEqual(0.4567, firstMethod.TargetPrecision, 1e-10);
            Assert.AreEqual(123, firstMethod.NumberOfCoefficients);
            Assert.AreEqual(200, firstMethod.BitPrecision);
            Assert.AreEqual(1234, secondMethod.MaximumIterations);
            Assert.AreEqual(0.4567, secondMethod.TargetPrecision, 1e-10);
        }
    }
}
