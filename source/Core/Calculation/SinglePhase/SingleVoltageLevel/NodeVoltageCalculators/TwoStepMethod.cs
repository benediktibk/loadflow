using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using MathNet.Numerics.LinearAlgebra;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public class TwoStepMethod : INodeVoltageCalculator
    {
        private bool _secondMethodRunning;
        private readonly Mutex _secondMethodRunningMutex;

        public TwoStepMethod(INodeVoltageCalculator firstMethod, INodeVoltageCalculator secondMethod)
        {
            if (firstMethod == null)
                throw new ArgumentNullException("firstMethod");

            if (secondMethod == null)
                throw new ArgumentNullException("secondMethod");

            FirstMethod = firstMethod;
            SecondMethod = secondMethod;
            _secondMethodRunning = false;
            _secondMethodRunningMutex = new Mutex();
        }

        public INodeVoltageCalculator FirstMethod { get; private set; }
        public INodeVoltageCalculator SecondMethod { get; private set; }

        public Vector<Complex> CalculateUnknownVoltages(IReadOnlyAdmittanceMatrix admittances, IList<Complex> totalAdmittanceRowSums, double nominalVoltage, Vector<Complex> initialVoltages, Vector<Complex> constantCurrents, IList<PqNodeWithIndex> pqBuses, IList<PvNodeWithIndex> pvBuses)
        {
            _secondMethodRunningMutex.WaitOne();
            _secondMethodRunning = false;
            _secondMethodRunningMutex.ReleaseMutex();
            var improvedInitialVoltages = FirstMethod.CalculateUnknownVoltages(admittances, totalAdmittanceRowSums,
                nominalVoltage, initialVoltages, constantCurrents, pqBuses, pvBuses);
            _secondMethodRunningMutex.WaitOne();
            _secondMethodRunning = true;
            _secondMethodRunningMutex.ReleaseMutex();
            return SecondMethod.CalculateUnknownVoltages(admittances, totalAdmittanceRowSums,
                nominalVoltage, improvedInitialVoltages, constantCurrents, pqBuses, pvBuses);
        }

        public double MaximumRelativePowerError
        {
            get { return SecondMethod.MaximumRelativePowerError; }
        }

        public double Progress
        {
            get { return (FirstMethod.Progress + SecondMethod.Progress)/2; }
        }

        public double RelativePowerError
        {
            get
            {
                _secondMethodRunningMutex.WaitOne();
                var result = _secondMethodRunning ? SecondMethod.RelativePowerError : FirstMethod.RelativePowerError;
                _secondMethodRunningMutex.ReleaseMutex();
                return result;
            }
        }
    }
}
