using System;
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public class HolomorphicEmbeddedLoadFlowMethodWithIterativeMethod : INodeVoltageCalculator
    {
        public HolomorphicEmbeddedLoadFlowMethodWithIterativeMethod(double targetPrecision, int coefficientCount, int bitPrecision, INodeVoltageCalculator iterativeMethod)
        {
            if (iterativeMethod == null)
                throw new ArgumentNullException("iterativeMethod");

            IterativeMethod = iterativeMethod;
            HolomorphicEmbeddedLoadFlowMethod = new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, coefficientCount, bitPrecision);
        }

        public INodeVoltageCalculator IterativeMethod { get; private set; }
        public HolomorphicEmbeddedLoadFlowMethod HolomorphicEmbeddedLoadFlowMethod { get; private set; }

        public Vector<Complex> CalculateUnknownVoltages(IReadOnlyAdmittanceMatrix admittances, IList<Complex> totalAdmittanceRowSums, double nominalVoltage, Vector<Complex> initialVoltages, Vector<Complex> constantCurrents, IList<PqNodeWithIndex> pqBuses, IList<PvNodeWithIndex> pvBuses)
        {
            var improvedInitialVoltages = HolomorphicEmbeddedLoadFlowMethod.CalculateUnknownVoltages(admittances, totalAdmittanceRowSums,
                nominalVoltage, initialVoltages, constantCurrents, pqBuses, pvBuses);
            return IterativeMethod.CalculateUnknownVoltages(admittances, totalAdmittanceRowSums,
                nominalVoltage, improvedInitialVoltages, constantCurrents, pqBuses, pvBuses);
        }

        public double MaximumRelativePowerError
        {
            get { return IterativeMethod.MaximumRelativePowerError; }
        }
    }
}
