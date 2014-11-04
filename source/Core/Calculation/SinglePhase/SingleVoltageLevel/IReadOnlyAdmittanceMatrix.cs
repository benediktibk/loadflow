using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public interface IReadOnlyAdmittanceMatrix
    {
        int NodeCount { get; }
        Vector<Complex> CalculateCurrents(Vector<Complex> voltages);
        Complex this[int row, int column] { get; }
        ISolver<Complex> CalculateFactorization();
        Vector<Complex> GetRow(int row);
        AdmittanceMatrix CreateReducedAdmittanceMatrix(IReadOnlyList<int> indexOfNodesWithUnknownVoltage,
            IReadOnlyList<int> indexOfNodesWithKnownVoltage, Vector<Complex> knownVoltages,
            out Vector<Complex> constantCurrentRightHandSide);
        Vector<Complex> CalculateRowSums();
    }
}