using System;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Numerics.LinearAlgebra.Complex;
using LoadFlowCalculation;

namespace LoadFlowCalculationTest
{
    [TestClass]
    public class LoadFlowCalculatorTest
    {
        [TestMethod]
        public void calculateNodeVoltages_fromOneSideSuppliedConnection_correctVoltages()
        {
            //Complex[,] admittancesArray = { { new Complex(1, 0), new Complex(-1, 0) }, { new Complex(-1, 0), new Complex(1, 0) } };
            //DenseMatrix admittances = DenseMatrix.OfArray(admittancesArray);
        }
    }
}
