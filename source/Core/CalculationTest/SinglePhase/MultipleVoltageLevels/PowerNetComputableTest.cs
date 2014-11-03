using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using AdmittanceMatrix = Calculation.SinglePhase.SingleVoltageLevel.AdmittanceMatrix;
using PowerNetComputable = Calculation.SinglePhase.MultipleVoltageLevels.PowerNetComputable;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class PowerNetComputableTest
    {
        [TestMethod]
        public void CalculateNodeVoltages_CalculationFails_Null()
        {
            var nodeVoltageCalculatorMock = new Mock<INodeVoltageCalculator>();
            var nodeGraphMock = new Mock<INodeGraph>();
            var powerNet = new PowerNetComputable(50, nodeVoltageCalculatorMock.Object, nodeGraphMock.Object);
            powerNet.AddNode(0, 1, 0, "");
            powerNet.AddNode(1, 1, 0, "");
            powerNet.AddFeedIn(0, new Complex(1.05, 0), 0, 1.1, 1);
            powerNet.AddLoad(1, new Complex(-0.6, -1));
            powerNet.AddGenerator(1, 1.02, -0.4);
            powerNet.AddTransmissionLine(0, 1, 0, 0.00006366197723675813, 0, 0, 1, true);
            nodeVoltageCalculatorMock.Setup(
                c =>
                    c.CalculateUnknownVoltages(
                        It.IsAny<AdmittanceMatrix>(),
                        It.IsAny<IList<Complex>>(), It.IsAny<double>(), It.IsAny<Vector<Complex>>(),
                        It.IsAny<Vector<Complex>>(), It.IsAny<IList<PqBus>>(), It.IsAny<IList<PvBus>>()))
                .Returns(DenseVector.Create(2, i => new Complex(0, 0)));

            var nodeResults = powerNet.CalculateNodeVoltages();

            Assert.AreEqual(null, nodeResults);
        }
    }
}
