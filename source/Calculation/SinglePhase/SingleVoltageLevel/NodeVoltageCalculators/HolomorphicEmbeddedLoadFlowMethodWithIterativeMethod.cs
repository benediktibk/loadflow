using System;
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public class HolomorphicEmbeddedLoadFlowMethodWithIterativeMethod : INodeVoltageCalculator
    {
        #region variables

        private readonly double _targetPrecision;
        private readonly INodeVoltageCalculator _iterativeMethod;

        #endregion

        #region constructor

        public HolomorphicEmbeddedLoadFlowMethodWithIterativeMethod(double targetPrecision,
            INodeVoltageCalculator iterativeMethod)
        {
            if (iterativeMethod == null)
                throw new ArgumentNullException("iterativeMethod");

            _targetPrecision = targetPrecision;
            _iterativeMethod = iterativeMethod;
        }

        #endregion

        #region INodeVoltageCalculator

        public Vector<Complex> CalculateUnknownVoltages(AdmittanceMatrix admittances, IList<Complex> totalAdmittanceRowSums, double nominalVoltage,
            Vector<Complex> initialVoltages, Vector<Complex> constantCurrents, IList<PQBus> pqBuses, IList<PVBus> pvBuses)
        {
            var helm = new HolomorphicEmbeddedLoadFlowMethod(_targetPrecision, 50, new PrecisionLongDouble(), true);
            var improvedInitialVoltages = helm.CalculateUnknownVoltages(admittances, totalAdmittanceRowSums,
                nominalVoltage, initialVoltages, constantCurrents, pqBuses, pvBuses);
            return _iterativeMethod.CalculateUnknownVoltages(admittances, totalAdmittanceRowSums,
                nominalVoltage, improvedInitialVoltages, constantCurrents, pqBuses, pvBuses);
        }

        public double GetMaximumPowerError()
        {
            return _iterativeMethod.GetMaximumPowerError();
        }

        #endregion
    }
}
