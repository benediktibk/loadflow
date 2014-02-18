using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Numerics.LinearAlgebra.Complex;
using LoadFlowCalculation;

namespace LoadFlowCalculationTest
{
    [TestClass]
    abstract public class LoadFlowCalculatorSmallAccuracyTest :
        LoadFlowCalculatorTest
    {
        [TestMethod]
        public void calculateNodeVoltages_fromOneSideSuppliedConnectionWithBigResistance_correctVoltages()
        {
            Matrix admittances;
            Node[] nodes;
            double nominalVoltage;
            double expectedOutputVoltage;
            double expectedInputPower;
            CreateOneSideSuppliedConnection(0.1, out admittances, out nodes, out nominalVoltage, out expectedOutputVoltage, out expectedInputPower);
            var caclulator = CreateLoadFlowCalculator();

            nodes = caclulator.CalculateNodeVoltages(admittances, nominalVoltage, nodes);

            ComplexAssert.AreEqual(1, 0, nodes[0].Voltage, 0.0001);
            ComplexAssert.AreEqual(expectedOutputVoltage, 0, nodes[1].Voltage, 0.1);
            ComplexAssert.AreEqual(expectedInputPower, 0, nodes[0].Power, 0.2);
            ComplexAssert.AreEqual(-1, 0, nodes[1].Power, 0.2);
        }

        [TestMethod]
        public void calculateNodeVoltages_fromOneSideSuppliedConnectionWithSmallResistance_correctVoltages()
        {
            Matrix admittances;
            Node[] nodes;
            double nominalVoltage;
            double expectedOutputVoltage;
            double expectedInputPower;
            CreateOneSideSuppliedConnection(0.001, out admittances, out nodes, out nominalVoltage, out expectedOutputVoltage, out expectedInputPower);
            var caclulator = CreateLoadFlowCalculator();

            nodes = caclulator.CalculateNodeVoltages(admittances, nominalVoltage, nodes);
            
            ComplexAssert.AreEqual(1, 0, nodes[0].Voltage, 0.0001);
            ComplexAssert.AreEqual(expectedOutputVoltage, 0, nodes[1].Voltage, 0.0001);
            ComplexAssert.AreEqual(expectedInputPower, 0, nodes[0].Power, 0.01);
            ComplexAssert.AreEqual(-1, 0, nodes[1].Power, 0.01);
        }
    }
}
