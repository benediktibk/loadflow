﻿using System;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public class Factory
    {
        private double _targetPrecision;
        private int _maximumIterations;
        private int _coefficientCount;
        private int _bitPrecision;

        public Factory()
        {
            TargetPrecision = 1e-5;
            MaximumIterations = 1000;
            CoefficientCount = 50;
            BitPrecision = 64;
            IterativeSolver = true;
        }

        public double TargetPrecision
        {
            get { return _targetPrecision; }
            set
            {
                if (value <= 0 || value >= 1)
                    throw new ArgumentOutOfRangeException("value", "must be between 0 and 1");

                _targetPrecision = value;
            }
        }

        public int MaximumIterations
        {
            get { return _maximumIterations; }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("value", "must be greater or equal 1");

                _maximumIterations = value;
            }
        }

        public int CoefficientCount
        {
            get { return _coefficientCount; }
            set
            {
                if (value < 3)
                    throw new ArgumentOutOfRangeException("value", "must be greater or equal 3");

                _coefficientCount = value;
            }
        }

        public int BitPrecision
        {
            get { return _bitPrecision; }
            set
            {
                if (value < 64)
                    throw new ArgumentOutOfRangeException("value", "must be greater or equal 64");

                _bitPrecision = value;
            }
        }

        public bool IterativeSolver { get; set; }

        public INodeVoltageCalculator CreateNodeVoltageCalculator(Selection selection)
        {
            switch (selection)
            {
                case Selection.NodePotential:
                    return new NodePotentialMethod(IterativeSolver);
                case Selection.CurrentIteration:
                    return new CurrentIteration(TargetPrecision, MaximumIterations, IterativeSolver);
                case Selection.NewtonRaphson:
                    return new NewtonRaphsonMethod(TargetPrecision, MaximumIterations, IterativeSolver);
                case Selection.FastDecoupledLoadFlow:
                    return new FastDecoupledLoadFlowMethod(TargetPrecision, MaximumIterations, IterativeSolver);
                case Selection.HolomorphicEmbeddedLoadFlow:
                    return new HolomorphicEmbeddedLoadFlowMethod(TargetPrecision, CoefficientCount, BitPrecision, IterativeSolver);
                case Selection.HolomorphicEmbeddedLoadFlowWithCurrentIteration:
                    return new TwoStepMethod(new HolomorphicEmbeddedLoadFlowMethod(TargetPrecision, CoefficientCount, BitPrecision, IterativeSolver), new CurrentIteration(TargetPrecision, MaximumIterations, IterativeSolver));
                case Selection.HolomorphicEmbeddedLoadFlowWithNewtonRaphson:
                    return new TwoStepMethod(new HolomorphicEmbeddedLoadFlowMethod(TargetPrecision, CoefficientCount, BitPrecision, IterativeSolver), new NewtonRaphsonMethod(TargetPrecision, MaximumIterations, IterativeSolver));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
