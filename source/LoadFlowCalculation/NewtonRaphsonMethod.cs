using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation
{
    public class NewtonRaphsonMethod : JacobiMatrixBasedMethod
    {
        public NewtonRaphsonMethod(double targetPrecision, int maximumIterations) : base(targetPrecision, maximumIterations)
        { }

        protected override Vector<double> CalculateVoltageChanges(Matrix<Complex> admittances, Vector<double> voltagesReal, Vector<double> voltagesImaginary,
            Vector<double> constantCurrentsReal, Vector<double> constantCurrentsImaginary, Vector<double> powersReal, Vector<double> lastPowersReal,
            Vector<double> powersImaginary, Vector<double> lastPowersImaginary)
        {
            var changeMatrix = CalculateChangeMatrix(admittances, voltagesReal, voltagesImaginary,
                constantCurrentsReal, constantCurrentsImaginary);
            var rightSide = CombineParts(powersReal - lastPowersReal, powersImaginary - lastPowersImaginary);
            var factorization = changeMatrix.QR();
            var voltageChanges = factorization.Solve(rightSide);
            return voltageChanges;
        }

        private Matrix<double> CalculateChangeMatrix(Matrix<Complex> admittances, IList<double> voltagesReal,
            IList<double> voltagesImaginary, Vector<double> constantCurrentsReal, Vector<double> constantCurrentsImaginary)
        {
            Vector<double> currentsReal;
            Vector<double> currentsImaginary;
            DenseMatrix changeMatrix;
            InitializeChangeMatrix(admittances, voltagesReal, voltagesImaginary, constantCurrentsReal, constantCurrentsImaginary, out currentsReal, out currentsImaginary, out changeMatrix);

            var nodeCount = admittances.RowCount;
            CalculateLeftUpperChangeMatrix(admittances, voltagesReal, voltagesImaginary, changeMatrix, currentsReal, 0, 0);
            CalculateRightUpperChangeMatrix(admittances, voltagesReal, voltagesImaginary, changeMatrix, currentsImaginary, 0, nodeCount);
            CalculateLeftLowerChangeMatrix(admittances, voltagesReal, voltagesImaginary, changeMatrix,
                currentsImaginary, nodeCount, 0);
            CalculateRightLowerChangeMatrix(admittances, voltagesReal, voltagesImaginary, changeMatrix,
                currentsReal, nodeCount, nodeCount);

            return changeMatrix;
        }

        public static void InitializeChangeMatrix(Matrix<Complex> admittances, IList<double> voltagesReal, IList<double> voltagesImaginary,
            Vector<double> constantCurrentsReal, Vector<double> constantCurrentsImaginary, out Vector<double> currentsReal, out Vector<double> currentsImaginary,
            out DenseMatrix changeMatrix)
        {
            var nodeCount = voltagesReal.Count;
            var loadCurrentsReal = CalculateLoadCurrentRealParts(admittances, voltagesReal, voltagesImaginary);
            var loadCurrentsImaginary = CalculateLoadCurrentImaginaryParts(admittances, voltagesReal, voltagesImaginary);
            currentsReal = loadCurrentsReal - constantCurrentsReal;
            currentsImaginary = loadCurrentsImaginary - constantCurrentsImaginary;
            changeMatrix = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(nodeCount * 2, nodeCount * 2);
        }
    }
}
