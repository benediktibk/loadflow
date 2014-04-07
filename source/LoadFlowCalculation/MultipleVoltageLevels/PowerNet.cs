using System;
using System.Collections.Generic;
using System.Numerics;
using LoadFlowCalculation.SingleVoltageLevel.NodeVoltageCalculators;

namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public class PowerNet
    {
        #region variables
        private readonly double _frequency;
        #endregion

        #region
        public PowerNet(double frequency)
        {
            _frequency = frequency;
        }
        #endregion

        #region public functions
        public void AddNode(string name, double nominalVoltage)
        {
            throw new NotImplementedException();
        }

        public void AddLine(string name, string firstNode, string secondNode, double lengthResistance, double lengthInductance,
            double shuntConductance, double capacity)
        {
            throw new NotImplementedException();
        }

        public void AddGenerator(string node, string name, double synchronLengthInductance, double synchronousGeneratedVoltage)
        {
            throw new NotImplementedException();
        }

        public void AddFeedIn(string node, string name, double shortCircuitPower)
        {
            throw new NotImplementedException();
        }

        public void AddTransformator(string upperSideNode, string lowerSideNode, string name, double nominalPower,
            double shortCircuitVoltageInPercentage, double copperLosses, double ironLosses, double alpha)
        {
            throw new NotImplementedException();
        }

        public void AddLoad(string node, string name, Complex power)
        {
            throw new NotImplementedException();
        }

        public void Calculate(INodeVoltageCalculator nodeVoltageCalculator)
        {
            throw new NotImplementedException();
        }
        #endregion

    }
}
