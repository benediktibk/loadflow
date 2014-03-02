using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation
{
    public class NewtonRaphsonMethod : JacobiMatrixBasedMethod
    {
        public NewtonRaphsonMethod(double targetPrecision, int maximumIterations) : base(targetPrecision, maximumIterations, 1, 0)
        { }

        protected override Vector<Complex> CalculateVoltageChanges(Matrix<Complex> admittances, Vector<Complex> voltages,
            Vector<Complex> constantCurrents, Vector<double> powersRealError, Vector<double> powersImaginaryError)
        {
            var voltagesReal = ExtractRealParts(voltages);
            var voltagesImaginary = ExtractImaginaryParts(voltages);
            var constantCurrentsReal = ExtractRealParts(constantCurrents);
            var constantCurrentsImaginary = ExtractImaginaryParts(constantCurrents);
            var changeMatrix = CalculateChangeMatrix(admittances, voltagesReal, voltagesImaginary,
                constantCurrentsReal, constantCurrentsImaginary);
            var rightSide = CombineParts(powersRealError, powersImaginaryError);
            var factorization = changeMatrix.QR();
            var voltageChanges = factorization.Solve(rightSide);
            Vector<double> voltageChangesReal;
            Vector<double> voltageChangesImaginary;
            DivideParts(voltageChanges, out voltageChangesReal, out voltageChangesImaginary);
            return CombineRealAndImaginaryParts(voltageChangesReal, voltageChangesImaginary);
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
        
        public static void CalculateLeftUpperChangeMatrix(Matrix<Complex> admittances, IList<double> voltagesReal,
            IList<double> voltagesImaginary, Matrix<double> changeMatrix, IList<double> currentsReal, int startRow, int startColumn)
        {
            var nodeCount = admittances.RowCount;

            for (var i = 0; i < nodeCount; ++i)
            {
                var voltageReal = voltagesReal[i];
                var voltageImaginary = voltagesImaginary[i];

                for (var j = 0; j < nodeCount; ++j)
                {
                    var admittanceReal = admittances[i, j].Real;
                    var admittanceImaginary = admittances[i, j].Imaginary;

                    changeMatrix[i + startRow, j + startColumn] = voltageReal * admittanceReal + voltageImaginary * admittanceImaginary;

                    if (i == j)
                        changeMatrix[i + startRow, i + startColumn] += currentsReal[i];
                }
            }
        }

        public static void CalculateRightUpperChangeMatrix(Matrix<Complex> admittances, IList<double> voltagesReal,
            IList<double> voltagesImaginary, Matrix<double> changeMatrix, IList<double> currentsImaginary, int startRow, int startColumn)
        {
            var nodeCount = admittances.RowCount;

            for (var i = 0; i < nodeCount; ++i)
            {
                var voltageReal = voltagesReal[i];
                var voltageImaginary = voltagesImaginary[i];

                for (var j = 0; j < nodeCount; ++j)
                {
                    var admittanceReal = admittances[i, j].Real;
                    var admittanceImaginary = admittances[i, j].Imaginary;

                    changeMatrix[i + startRow, j + startColumn] = voltageImaginary * admittanceReal - voltageReal * admittanceImaginary;

                    if (i == j)
                        changeMatrix[i + startRow, i + startColumn] += currentsImaginary[i];
                }
            }
        }

        public static void CalculateLeftLowerChangeMatrix(Matrix<Complex> admittances, IList<double> voltagesReal,
            IList<double> voltagesImaginary, Matrix<double> changeMatrix, IList<double> currentsImaginary, int startRow, int startColumn)
        {
            var nodeCount = admittances.RowCount;

            for (var i = 0; i < nodeCount; ++i)
            {
                var voltageReal = voltagesReal[i];
                var voltageImaginary = voltagesImaginary[i];

                for (var j = 0; j < nodeCount; ++j)
                {
                    var admittanceReal = admittances[i, j].Real;
                    var admittanceImaginary = admittances[i, j].Imaginary;

                    changeMatrix[i + startRow, j + startColumn] = voltageImaginary * admittanceReal - voltageReal * admittanceImaginary;

                    if (i == j)
                        changeMatrix[i + startRow, i + startColumn] -= currentsImaginary[i];
                }
            }
        }

        public static void CalculateRightLowerChangeMatrix(Matrix<Complex> admittances, IList<double> voltagesReal,
            IList<double> voltagesImaginary, Matrix<double> changeMatrix, IList<double> currentsReal, int startRow, int startColumn)
        {
            var nodeCount = admittances.RowCount;

            for (var i = 0; i < nodeCount; ++i)
            {
                var voltageReal = voltagesReal[i];
                var voltageImaginary = voltagesImaginary[i];

                for (var j = 0; j < nodeCount; ++j)
                {
                    var admittanceReal = admittances[i, j].Real;
                    var admittanceImaginary = admittances[i, j].Imaginary;

                    changeMatrix[i + startRow, j + startColumn] = (-1) *
                                                                 (voltageReal * admittanceReal +
                                                                  voltageImaginary * admittanceImaginary);

                    if (i == j)
                        changeMatrix[i + startRow, i + startColumn] += currentsReal[i];
                }
            }
        }
    }
}
