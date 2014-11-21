using System;
using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Calculation.ThreePhase;
using MathNet.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;
using PowerNetComputable = Calculation.SinglePhase.MultipleVoltageLevels.PowerNetComputable;

namespace CalculationIntegrationTest.ThreePhase
{
    [TestClass]
    public class SymmetricPowerNetTest
    {
        private INodeVoltageCalculator _newtonRaphsonCalculator;
        private INodeVoltageCalculator _helmCalculator;
        private INodeVoltageCalculator _currentIterationCalculator;
        private SymmetricPowerNet _powerNetWithNewtonRaphson;
        private SymmetricPowerNet _powerNetWithHelm;
        private SymmetricPowerNet _powerNetWithCurrentIteration;

        [TestInitialize]
        public void SetUp()
        {
            _newtonRaphsonCalculator = new NewtonRaphsonMethod(0.0000001, 1000);
            _helmCalculator = new HolomorphicEmbeddedLoadFlowMethod(0.00000001, 80, 200);
            _currentIterationCalculator = new CurrentIteration(0.0000001, 1000);
            _powerNetWithNewtonRaphson = new SymmetricPowerNet(new PowerNetComputable(50, new PowerNetFactory(_newtonRaphsonCalculator), new NodeGraph()));
            _powerNetWithHelm = new SymmetricPowerNet(new PowerNetComputable(50, new PowerNetFactory(_helmCalculator), new NodeGraph()));
            _powerNetWithCurrentIteration = new SymmetricPowerNet(new PowerNetComputable(50, new PowerNetFactory(_currentIterationCalculator), new NodeGraph()));
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneLineBetweenFeedInAndLoad_CorrectResults()
        {
            const double omega = 2 * Math.PI * 50;
            _powerNetWithNewtonRaphson.AddNode(1, 400, "");
            _powerNetWithNewtonRaphson.AddNode(2, 400, "");
            _powerNetWithNewtonRaphson.AddFeedIn(1, new Complex(400, 0), new Complex());
            _powerNetWithNewtonRaphson.AddLoad(2, new Complex(-20000, -2000));
            _powerNetWithNewtonRaphson.AddTransmissionLine(1, 2, 0.1, 0.4 / omega, 100.0 / (400 * 400), 1e-10, 1, true);

            var nodeResults = _powerNetWithNewtonRaphson.CalculateNodeVoltages();

            var sourceVoltage = nodeResults[1].Voltage;
            var loadVoltage = nodeResults[2].Voltage;
            var sourceVoltageShouldBe = Complex.FromPolarCoordinates(400, 0);
            var loadVoltageShouldBe = Complex.FromPolarCoordinates(392.363, -2.852 * Math.PI / 180);
            ComplexAssert.AreEqual(sourceVoltageShouldBe, sourceVoltage, 0.01);
            ComplexAssert.AreEqual(loadVoltageShouldBe, loadVoltage, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneLineBetweenFeedInAndLoadAndTransmissionEquationModel_CorrectResults()
        {
            const double omega = 2 * Math.PI * 50;
            _powerNetWithNewtonRaphson.AddNode(1, 400, "");
            _powerNetWithNewtonRaphson.AddNode(2, 400, "");
            _powerNetWithNewtonRaphson.AddFeedIn(1, new Complex(400, 0), new Complex());
            _powerNetWithNewtonRaphson.AddLoad(2, new Complex(-20000, -2000));
            _powerNetWithNewtonRaphson.AddTransmissionLine(1, 2, 0.1, 0.4 / omega, 10000.0 / (400 * 400), 1e-7, 1, true);

            var nodeResults = _powerNetWithNewtonRaphson.CalculateNodeVoltages();

            var sourceVoltage = nodeResults[1].Voltage;
            var loadVoltage = nodeResults[2].Voltage;
            var sourceVoltageShouldBe = Complex.FromPolarCoordinates(400, 0);
            var loadVoltageShouldBe = Complex.FromPolarCoordinates(391.693, -3.212 * Math.PI / 180);
            ComplexAssert.AreEqual(sourceVoltageShouldBe, sourceVoltage, 0.01);
            ComplexAssert.AreEqual(loadVoltageShouldBe, loadVoltage, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneLineBetweenFeedInAndLoadAndOnlyResistance_CorrectResults()
        {
            _powerNetWithNewtonRaphson.AddNode(1, 400, "");
            _powerNetWithNewtonRaphson.AddNode(2, 400, "");
            _powerNetWithNewtonRaphson.AddFeedIn(1, new Complex(400, 0), new Complex());
            _powerNetWithNewtonRaphson.AddLoad(2, new Complex(-20000, 0));
            _powerNetWithNewtonRaphson.AddTransmissionLine(1, 2, 0.1, 0, 0, 0, 1, false);

            var nodeResults = _powerNetWithNewtonRaphson.CalculateNodeVoltages();

            var sourceVoltage = nodeResults[1].Voltage;
            var loadVoltage = nodeResults[2].Voltage;
            var sourceVoltageShouldBe = Complex.FromPolarCoordinates(400, 0);
            var loadVoltageShouldBe = Complex.FromPolarCoordinates(394.936, 0 * Math.PI / 180);
            ComplexAssert.AreEqual(sourceVoltageShouldBe, sourceVoltage, 0.01);
            ComplexAssert.AreEqual(loadVoltageShouldBe, loadVoltage, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneLineBetweenFeedInAndLoadAndOnlyResistanceAndGroundLosses_CorrectResults()
        {
            _powerNetWithNewtonRaphson.AddNode(1, 400, "");
            _powerNetWithNewtonRaphson.AddNode(2, 400, "");
            _powerNetWithNewtonRaphson.AddFeedIn(1, new Complex(400, 0), new Complex());
            _powerNetWithNewtonRaphson.AddLoad(2, new Complex(-20000, 0));
            _powerNetWithNewtonRaphson.AddTransmissionLine(1, 2, 0.05, 0, 50000.0 / (400 * 400), 0, 1, false);

            var nodeResults = _powerNetWithNewtonRaphson.CalculateNodeVoltages();

            var sourceVoltage = nodeResults[1].Voltage;
            var loadVoltage = nodeResults[2].Voltage;
            var sourceVoltageShouldBe = Complex.FromPolarCoordinates(400, 0);
            var loadVoltageShouldBe = Complex.FromPolarCoordinates(394.383, 0 * Math.PI / 180);
            ComplexAssert.AreEqual(sourceVoltageShouldBe, sourceVoltage, 0.01);
            ComplexAssert.AreEqual(loadVoltageShouldBe, loadVoltage, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneLineBetweenFeedInAndLoadAndOnlyResistanceAndShuntCapacity_CorrectResults()
        {
            _powerNetWithNewtonRaphson.AddNode(1, 400, "");
            _powerNetWithNewtonRaphson.AddNode(2, 400, "");
            _powerNetWithNewtonRaphson.AddFeedIn(1, new Complex(400, 0), new Complex());
            _powerNetWithNewtonRaphson.AddLoad(2, new Complex(-20000, 20000));
            _powerNetWithNewtonRaphson.AddTransmissionLine(1, 2, 0.05, 0, 0, 1e-5, 1, false);

            var nodeResults = _powerNetWithNewtonRaphson.CalculateNodeVoltages();

            var sourceVoltage = nodeResults[1].Voltage;
            var loadVoltage = nodeResults[2].Voltage;
            var sourceVoltageShouldBe = Complex.FromPolarCoordinates(400, 0);
            var loadVoltageShouldBe = Complex.FromPolarCoordinates(397.476, -0.365 * Math.PI / 180);
            ComplexAssert.AreEqual(sourceVoltageShouldBe, sourceVoltage, 0.01);
            ComplexAssert.AreEqual(loadVoltageShouldBe, loadVoltage, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneLineBetweenFeedInAndLoadAndSimplifiedModel_CorrectResults()
        {
            const double omega = 2 * Math.PI * 50;
            _powerNetWithNewtonRaphson.AddNode(1, 400, "");
            _powerNetWithNewtonRaphson.AddNode(2, 400, "");
            _powerNetWithNewtonRaphson.AddFeedIn(1, new Complex(400, 0), new Complex());
            _powerNetWithNewtonRaphson.AddLoad(2, new Complex(-20000, -2000));
            _powerNetWithNewtonRaphson.AddTransmissionLine(1, 2, 0.1, 0.4 / omega, 10000.0 / (400 * 400), 1e-7, 1, false);

            var nodeResults = _powerNetWithNewtonRaphson.CalculateNodeVoltages();

            var sourceVoltage = nodeResults[1].Voltage;
            var loadVoltage = nodeResults[2].Voltage;
            var sourceVoltageShouldBe = Complex.FromPolarCoordinates(400, 0);
            var loadVoltageShouldBe = Complex.FromPolarCoordinates(390.845, -3.561 * Math.PI / 180);
            ComplexAssert.AreEqual(sourceVoltageShouldBe, sourceVoltage, 0.01);
            ComplexAssert.AreEqual(loadVoltageShouldBe, loadVoltage, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_Transformer_CorrectResults()
        {
            _powerNetWithNewtonRaphson.AddNode(1, 1000, "");
            _powerNetWithNewtonRaphson.AddNode(2, 400, "");
            _powerNetWithNewtonRaphson.AddFeedIn(1, Complex.FromPolarCoordinates(1000, 2 * Math.PI / 180), new Complex());
            _powerNetWithNewtonRaphson.AddLoad(2, new Complex(-2000, 0));
            _powerNetWithNewtonRaphson.AddTwoWindingTransformer(1, 2, 3000, 0.08, 0.001 * 3000, 10, 0.01, 2.5, new Angle(), "");

            var nodeResults = _powerNetWithNewtonRaphson.CalculateNodeVoltages();

            var sourceVoltage = nodeResults[1].Voltage;
            var loadVoltage = nodeResults[2].Voltage;
            var sourceVoltageShouldBe = Complex.FromPolarCoordinates(1000, 2 * Math.PI / 180);
            var loadVoltageShouldBe = Complex.FromPolarCoordinates(399.007, -1.072 * Math.PI / 180);
            ComplexAssert.AreEqual(sourceVoltageShouldBe, sourceVoltage, 0.01);
            ComplexAssert.AreEqual(loadVoltageShouldBe, loadVoltage, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_TransformerWithNearlyNoIronLosses_CorrectResults()
        {
            _powerNetWithNewtonRaphson.AddNode(1, 1000, "");
            _powerNetWithNewtonRaphson.AddNode(2, 400, "");
            _powerNetWithNewtonRaphson.AddFeedIn(1, Complex.FromPolarCoordinates(1000, 2 * Math.PI / 180), new Complex());
            _powerNetWithNewtonRaphson.AddLoad(2, new Complex(-2000, 0));
            _powerNetWithNewtonRaphson.AddTwoWindingTransformer(1, 2, 3000, 0.08, 0.001 * 3000, 0.00000001, 0.000000001, 2.5, new Angle(), "");

            var nodeResults = _powerNetWithNewtonRaphson.CalculateNodeVoltages();

            var sourceVoltage = nodeResults[1].Voltage;
            var loadVoltage = nodeResults[2].Voltage;
            var sourceVoltageShouldBe = Complex.FromPolarCoordinates(1000, 2 * Math.PI / 180);
            var loadVoltageShouldBe = Complex.FromPolarCoordinates(399.161, -1.063 * Math.PI / 180);
            ComplexAssert.AreEqual(sourceVoltageShouldBe, sourceVoltage, 0.01);
            ComplexAssert.AreEqual(loadVoltageShouldBe, loadVoltage, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_TransformerWithNearlyNoIronLossesAndNoInductance_CorrectResults()
        {
            _powerNetWithNewtonRaphson.AddNode(1, 1000, "");
            _powerNetWithNewtonRaphson.AddNode(2, 400, "");
            _powerNetWithNewtonRaphson.AddFeedIn(1, Complex.FromPolarCoordinates(1000, 2 * Math.PI / 180), new Complex());
            _powerNetWithNewtonRaphson.AddLoad(2, new Complex(-2000, 0));
            _powerNetWithNewtonRaphson.AddTwoWindingTransformer(1, 2, 3000, 0.08, 0.0799999 * 3000, 0.0000000001, 0.00000000001, 2.5, new Angle(), "");

            var nodeResults = _powerNetWithNewtonRaphson.CalculateNodeVoltages();

            var sourceVoltage = nodeResults[1].Voltage;
            var loadVoltage = nodeResults[2].Voltage;
            var sourceVoltageShouldBe = Complex.FromPolarCoordinates(1000, 2 * Math.PI / 180);
            ComplexAssert.AreEqual(sourceVoltageShouldBe, sourceVoltage, 0.01);
            Assert.AreEqual(377.615, loadVoltage.Magnitude, 0.5);
        }

        [TestMethod]
        public void CalculateNodeVoltages_Generator_CorrectResults()
        {
            const double omega = 2 * Math.PI * 50;
            _powerNetWithNewtonRaphson.AddNode(1, 400, "");
            _powerNetWithNewtonRaphson.AddNode(2, 400, "");
            _powerNetWithNewtonRaphson.AddNode(3, 400, "");
            _powerNetWithNewtonRaphson.AddFeedIn(1, Complex.FromPolarCoordinates(400, 0), new Complex());
            _powerNetWithNewtonRaphson.AddGenerator(3, 400, 15000);
            _powerNetWithNewtonRaphson.AddLoad(2, new Complex(-20000, -2000));
            _powerNetWithNewtonRaphson.AddTransmissionLine(1, 2, 0.1, 0.4 / omega, 100.0 / (400 * 400), 1e-10, 1, true);
            _powerNetWithNewtonRaphson.AddTransmissionLine(2, 3, 0.1, 0.4 / omega, 100.0 / (400 * 400), 1e-10, 1, true);

            var nodeResults = _powerNetWithNewtonRaphson.CalculateNodeVoltages();

            var feedInVoltage = nodeResults[1].Voltage;
            var loadVoltage = nodeResults[2].Voltage;
            var generatorVoltage = nodeResults[3].Voltage;
            var feedInVoltageShouldBe = new Complex(400, 0);
            var generatorVoltageShouldBe = Complex.FromPolarCoordinates(400, 1.487 * Math.PI / 180);
            var loadVoltageShouldBe = Complex.FromPolarCoordinates(396.306, -0.670 * Math.PI / 180);
            ComplexAssert.AreEqual(feedInVoltageShouldBe, feedInVoltage, 0.01);
            ComplexAssert.AreEqual(generatorVoltageShouldBe, generatorVoltage, 0.1);
            ComplexAssert.AreEqual(loadVoltageShouldBe, loadVoltage, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_MinimalExampleWithGeneratorAndLoad_CorrectResults()
        {
            const double omega = 2 * Math.PI * 50;
            var threePhaseFactor = Math.Sqrt(3);
            var Z = new Complex(1, 1);
            var U1 = new Complex(1, 0);
            var U2 = new Complex(0.95, 0.1);
            var U3 = new Complex(0.93, 0.08);
            var I1 = (U1 - U2) / Z;
            var I2 = (U2 - U3) / Z;
            var load = (-1) * U3 * I2.Conjugate();
            var generatorLoad = U2 * (I2 - I1).Conjugate();
            _powerNetWithCurrentIteration.AddNode(1, threePhaseFactor, "feed in");
            _powerNetWithCurrentIteration.AddNode(2, threePhaseFactor, "generator");
            _powerNetWithCurrentIteration.AddNode(3, threePhaseFactor, "load");
            _powerNetWithCurrentIteration.AddFeedIn(1, U1 * threePhaseFactor, new Complex());
            _powerNetWithCurrentIteration.AddGenerator(2, U2.Magnitude * threePhaseFactor, generatorLoad.Real * 3);
            _powerNetWithCurrentIteration.AddLoad(3, load * 3);
            _powerNetWithCurrentIteration.AddTransmissionLine(1, 2, 1, 1 / omega, 0, 0, 1, true);
            _powerNetWithCurrentIteration.AddTransmissionLine(2, 3, 1, 1 / omega, 0, 0, 1, true);

            var nodeResults = _powerNetWithCurrentIteration.CalculateNodeVoltages();

            ComplexAssert.AreEqual(U1, nodeResults[1].Voltage / threePhaseFactor, 0.000001);
            ComplexAssert.AreEqual(U2, nodeResults[2].Voltage / threePhaseFactor, 0.000001);
            ComplexAssert.AreEqual(U3, nodeResults[3].Voltage / threePhaseFactor, 0.000001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_MinimalExampleWithLoad_CorrectResults()
        {
            var threePhaseFactor = Math.Sqrt(3);
            _powerNetWithCurrentIteration.AddNode(1, threePhaseFactor, "feed in");
            _powerNetWithCurrentIteration.AddNode(2, threePhaseFactor, "load");
            _powerNetWithCurrentIteration.AddFeedIn(1, new Complex(threePhaseFactor, 0), new Complex());
            _powerNetWithCurrentIteration.AddLoad(2, new Complex(-0.1425, 0));
            _powerNetWithCurrentIteration.AddTransmissionLine(1, 2, 1, 0, 0, 0, 1, true);

            var nodeResults = _powerNetWithCurrentIteration.CalculateNodeVoltages();

            ComplexAssert.AreEqual(1, 0, nodeResults[1].Voltage / threePhaseFactor, 0.000001);
            ComplexAssert.AreEqual(0.95, 0, nodeResults[2].Voltage / threePhaseFactor, 0.000001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_MinimalExampleWithGenerator_CorrectResults()
        {
            var threePhaseFactor = Math.Sqrt(3);
            const double omega = 2 * Math.PI * 50;
            _powerNetWithCurrentIteration.AddNode(1, threePhaseFactor, "feed in");
            _powerNetWithCurrentIteration.AddNode(2, threePhaseFactor, "generator");
            _powerNetWithCurrentIteration.AddFeedIn(1, new Complex(threePhaseFactor, 0), new Complex());
            _powerNetWithCurrentIteration.AddGenerator(2, Math.Sqrt(0.95 * 0.95 + 0.1 * 0.1) * threePhaseFactor, 0.09375);
            _powerNetWithCurrentIteration.AddTransmissionLine(1, 2, 1, 1 / omega, 0, 0, 1, true);

            var nodeResults = _powerNetWithCurrentIteration.CalculateNodeVoltages();

            ComplexAssert.AreEqual(1, 0, nodeResults[1].Voltage / threePhaseFactor, 0.00001);
            ComplexAssert.AreEqual(0.95, 0.1, nodeResults[2].Voltage / threePhaseFactor, 0.00001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_MinimalExampleWithGeneratorAndNewtonRaphson_CorrectResults()
        {
            var threePhaseFactor = Math.Sqrt(3);
            const double omega = 2 * Math.PI * 50;
            _powerNetWithNewtonRaphson.AddNode(1, threePhaseFactor, "feed in");
            _powerNetWithNewtonRaphson.AddNode(2, threePhaseFactor, "generator");
            _powerNetWithNewtonRaphson.AddFeedIn(1, new Complex(threePhaseFactor, 0), new Complex());
            _powerNetWithNewtonRaphson.AddGenerator(2, Math.Sqrt(0.95 * 0.95 + 0.1 * 0.1) * threePhaseFactor, 0.09375);
            _powerNetWithNewtonRaphson.AddTransmissionLine(1, 2, 1, 1 / omega, 0, 0, 1, true);

            var nodeResults = _powerNetWithNewtonRaphson.CalculateNodeVoltages();

            ComplexAssert.AreEqual(1, 0, nodeResults[1].Voltage / threePhaseFactor, 0.001);
            ComplexAssert.AreEqual(0.95, 0.1, nodeResults[2].Voltage / threePhaseFactor, 0.001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_MinimalExampleWithGeneratorAndHELM_CorrectResults()
        {
            var threePhaseFactor = Math.Sqrt(3);
            const double omega = 2 * Math.PI * 50;
            _powerNetWithHelm.AddNode(1, threePhaseFactor, "feed in");
            _powerNetWithHelm.AddNode(2, threePhaseFactor, "generator");
            _powerNetWithHelm.AddFeedIn(1, new Complex(threePhaseFactor, 0), new Complex());
            _powerNetWithHelm.AddGenerator(2, Math.Sqrt(0.95 * 0.95 + 0.1 * 0.1) * threePhaseFactor, 0.09375);
            _powerNetWithHelm.AddTransmissionLine(1, 2, 1, 1 / omega, 0, 0, 1, true);

            var nodeResults = _powerNetWithHelm.CalculateNodeVoltages();

            ComplexAssert.AreEqual(1, 0, nodeResults[1].Voltage / threePhaseFactor, 0.00001);
            ComplexAssert.AreEqual(0.95, 0.1, nodeResults[2].Voltage / threePhaseFactor, 0.00001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithOneTransmissionLine_VoltagesAndPowersAreCorrect()
        {
            _powerNetWithNewtonRaphson.AddNode(0, 1000 * Math.Sqrt(3), "");
            _powerNetWithNewtonRaphson.AddNode(1, 1000 * Math.Sqrt(3), "");
            _powerNetWithNewtonRaphson.AddFeedIn(0, new Complex(1000, 0) * Math.Sqrt(3), new Complex());
            _powerNetWithNewtonRaphson.AddTransmissionLine(0, 1, 1, 0, 0, 0, 1, false);
            _powerNetWithNewtonRaphson.AddLoad(1, new Complex(-3e5, 0));

            var nodeResults = _powerNetWithNewtonRaphson.CalculateNodeVoltages();

            Assert.IsNotNull(nodeResults);
            var targetVoltageShouldBe = (0.5 + Math.Sqrt(0.15)) * 1e3 * Math.Sqrt(3);
            var voltageDrop = 1e3 - targetVoltageShouldBe / Math.Sqrt(3);
            var powerLoss = voltageDrop * voltageDrop * 3;
            var loadPowerShouldBe = 3e5 + powerLoss;
            ComplexAssert.AreEqual(1000 * Math.Sqrt(3), 0, nodeResults[0].Voltage, 0.01);
            ComplexAssert.AreEqual(targetVoltageShouldBe, 0, nodeResults[1].Voltage, 0.01);
            ComplexAssert.AreEqual(loadPowerShouldBe, 0, nodeResults[0].Power, 0.01);
            ComplexAssert.AreEqual(-3e5, 0, nodeResults[1].Power, 0.01);
        }
    }
}
