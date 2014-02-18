using System.Linq;
using LoadFlowCalculation;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LoadFlowCalculationTest
{
    public static class NodeAssert
    {
        public static void AreEqual(Node[] nodes, Vector voltages, Vector powers, double voltageDelta, double powerDelta)
        {
            Assert.AreEqual(nodes.Count(), voltages.Count);
            Assert.AreEqual(nodes.Count(), powers.Count);

            for (var i = 0; i < nodes.Count(); ++i)
            {
                var node = nodes[i];
                var voltage = voltages.At(i);
                var power = powers.At(i);
                ComplexAssert.AreEqual(voltage, node.Voltage, voltageDelta);
                ComplexAssert.AreEqual(power, node.Power, powerDelta);
            }
        }
    }
}
