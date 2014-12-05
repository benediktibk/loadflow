using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public abstract class NodeVoltageCalculator : INodeVoltageCalculator
    {
        private readonly object _progressLock = new object();
        private double _progress;
        private double _relativePowerError;

        public abstract Vector<Complex> CalculateUnknownVoltages(IReadOnlyAdmittanceMatrix admittances, IList<Complex> totalAdmittanceRowSums,
            double nominalVoltage, Vector<Complex> initialVoltages, Vector<Complex> constantCurrents, IList<PqNodeWithIndex> pqBuses, IList<PvNodeWithIndex> pvBuses);

        public abstract double MaximumRelativePowerError { get; }

        public double Progress
        {
            get
            {
                lock (_progressLock)
                {
                    return _progress;
                }
            }

            protected set
            {
                lock (_progressLock)
                {
                    _progress = value;
                }
            }
        }

        public double RelativePowerError
        {
            get
            {
                lock (_progressLock)
                {
                    return _relativePowerError;
                }
            }

            protected set
            {
                lock (_progressLock)
                {
                    _relativePowerError = value;
                }
            }
        }
    }
}
