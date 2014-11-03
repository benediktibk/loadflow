using System.Collections.Generic;
using System.Numerics;
using Calculation;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Calculation.SinglePhase.SingleVoltageLevel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PowerNetComputable = Calculation.SinglePhase.MultipleVoltageLevels.PowerNetComputable;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class PowerNetComputableTest
    {
        [TestMethod]
        public void CalculateNodeVoltages_CalculationFails_Null()
        {
            var powerNetFactoryMock = new Mock<IPowerNetFactory>();
            var singleVoltagePowerNetMock = new Mock<Calculation.SinglePhase.SingleVoltageLevel.IPowerNetComputable>();
            var nodeGraphMock = new Mock<INodeGraph>();
            var powerNet = new PowerNetComputable(50, powerNetFactoryMock.Object, nodeGraphMock.Object);
            powerNet.AddNode(0, 1, 0, "");
            powerNet.AddNode(1, 1, 0, "");
            powerNet.AddFeedIn(0, new Complex(1.05, 0), 0, 1.1, 1);
            powerNet.AddLoad(1, new Complex(-0.6, -1));
            powerNet.AddGenerator(1, 1.02, -0.4);
            powerNet.AddTransmissionLine(0, 1, 0, 0.00006366197723675813, 0, 0, 1, true);
            powerNetFactoryMock.Setup(
                x =>
                    x.Create(It.IsAny<Calculation.SinglePhase.SingleVoltageLevel.IAdmittanceMatrix>(),
                        It.IsAny<double>())).Returns(singleVoltagePowerNetMock.Object);
            singleVoltagePowerNetMock.Setup(c => c.CalculateNodeResults()).Returns((IList<NodeResult>) null);

            var nodeResults = powerNet.CalculateNodeVoltages();

            Assert.AreEqual(null, nodeResults);
        }
    }
}
