﻿using System;

namespace CalculationComparison.AlgorithmSettings
{
    public class IterativeMethodSettings : ChangeAllowable
    {
        private double _targetPrecision = 0.00001;
        private int _maximumIterations = 1000;

        public IterativeMethodSettings(GeneralSettings generalSettings) : base(generalSettings)
        { }

        public double TargetPrecision
        {
            get { return _targetPrecision; }
            set
            {
                if (value == _targetPrecision)
                    return;

                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "mustn't be negative");

                _targetPrecision = value;
                OnPropertyChanged();
            }
        }

        public int MaximumIterations
        {
            get { return _maximumIterations; }
            set
            {
                if (value == _maximumIterations)
                    return;

                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "mustn't be negative");

                _maximumIterations = value;
                OnPropertyChanged();
            }
        }
    }
}