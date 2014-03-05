using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LoadFlowCalculation;
using MathNet.Numerics.LinearAlgebra.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestHelper;

namespace LoadFlowCalculationTest
{
    public static class NodeAssert
    {
        public static void AreEqual(IList<Node> nodes, Vector<Complex> voltages, Vector<Complex> powers, double voltageDelta, double powerDelta)
        {
            Assert.AreEqual(nodes.Count(), voltages.Count);
            Assert.AreEqual(nodes.Count(), powers.Count);

            for (var i = 0; i < nodes.Count(); ++i)
            {
                var node = nodes[i];
                var voltage = voltages.At(i);
                ComplexAssert.AreEqual(voltage, node.Voltage, voltageDelta);
            }

            for (var i = 0; i < nodes.Count(); ++i)
            {
                var node = nodes[i];
                var power = powers.At(i);
                ComplexAssert.AreEqual(power, node.Power, powerDelta);
            }
        }
    }
}
