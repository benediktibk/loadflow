
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation
{
    public class FastDecoupledLoadFlowMethod : NewtonRaphsonMethod
    {
        public FastDecoupledLoadFlowMethod(double targetPrecision, int maximumIterations) : base(targetPrecision, maximumIterations)
        { }

        protected override Matrix<double> CalculateChangeMatrix(Matrix<Complex> admittances, IList<double> voltagesReal,
            IList<double> voltagesImaginary, Vector<double> constantCurrentsReal, Vector<double> constantCurrentsImaginary)
        {
            Vector<double> currentsReal;
            Vector<double> currentsImaginary;
            DenseMatrix changeMatrix;
            InitializeChangeMatrix(admittances, voltagesReal, voltagesImaginary, constantCurrentsReal, constantCurrentsImaginary, out currentsReal, out currentsImaginary, out changeMatrix);

            var nodeCount = admittances.RowCount;
            CalculateLeftUpperChangeMatrix(admittances, voltagesReal, voltagesImaginary, changeMatrix, currentsReal, 0, 0);
            CalculateRightLowerChangeMatrix(admittances, voltagesReal, voltagesImaginary, changeMatrix,
                currentsReal, nodeCount, nodeCount);

            return changeMatrix;
        }
    }
}
