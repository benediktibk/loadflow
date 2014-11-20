using System;
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public class TwoStepMethod : INodeVoltageCalculator
    {
        public TwoStepMethod(INodeVoltageCalculator firstMethod, INodeVoltageCalculator secondMethod)
        {
            if (firstMethod == null)
                throw new ArgumentNullException("firstMethod");

            if (secondMethod == null)
                throw new ArgumentNullException("secondMethod");

            FirstMethod = firstMethod;
            SecondMethod = secondMethod;
        }

        public INodeVoltageCalculator FirstMethod { get; private set; }
        public INodeVoltageCalculator SecondMethod { get; private set; }

        public Vector<Complex> CalculateUnknownVoltages(IReadOnlyAdmittanceMatrix admittances, IList<Complex> totalAdmittanceRowSums, double nominalVoltage, Vector<Complex> initialVoltages, Vector<Complex> constantCurrents, IList<PqNodeWithIndex> pqBuses, IList<PvNodeWithIndex> pvBuses)
        {
            var improvedInitialVoltages = FirstMethod.CalculateUnknownVoltages(admittances, totalAdmittanceRowSums,
                nominalVoltage, initialVoltages, constantCurrents, pqBuses, pvBuses);
            return SecondMethod.CalculateUnknownVoltages(admittances, totalAdmittanceRowSums,
                nominalVoltage, improvedInitialVoltages, constantCurrents, pqBuses, pvBuses);
        }

        public double MaximumRelativePowerError
        {
            get { return SecondMethod.MaximumRelativePowerError; }
        }
    }
}
