using System;
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation
{
    public class PowerNetSingleVoltageLevel
    {
        #region variables
        private readonly int _nodeCount;
        private IList<Node> _nodes;
        private readonly Matrix<Complex> _admittances;
        private readonly double _nominalVoltage;
        #endregion

        #region public functions
        public PowerNetSingleVoltageLevel(int nodeCount, double nominalVoltage)
        {
            if (nodeCount <= 0)
                throw new ArgumentOutOfRangeException("nodeCount", "there must be at least one node");

            if (nominalVoltage <= 0)
                throw new ArgumentOutOfRangeException("nominalVoltage", "the nominal voltage must be positive");

            _nominalVoltage = nominalVoltage;
            _nodeCount = nodeCount;
            _admittances = new SparseMatrix(_nodeCount, _nodeCount);
            _nodes = new List<Node>(nodeCount);

            for (var i = 0; i < _nodeCount; ++i)
                _nodes.Add(new Node());
        }

        public bool CalculateMissingInformation(LoadFlowCalculator calculator)
        {
            bool voltageCollapse;
            _nodes = calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, _nodes,
                out voltageCollapse);
            return voltageCollapse;
        }

        public void SetNode(int i, Node node)
        {
            _nodes[i] = node;
        }

        public void SetAdmittance(int i, int j, Complex admittance)
        {
            if (i == j)
                throw new ArgumentOutOfRangeException();

            _admittances[i, i] += _admittances[i, j];
            _admittances[j, j] += _admittances[i, j];
            _admittances[i, i] += admittance;
            _admittances[j, j] += admittance;
            _admittances[i, j] = (-1)*admittance;
            _admittances[j, i] = (-1)*admittance;
        }

        public Complex GetAdmittance(int i, int j)
        {
            return _admittances[i, j];
        }
        #endregion

        #region properties
        public double RelativePowerError
        {
            get
            {
                var powerSum = NodePowers.Sum();
                var powerLoss = LoadFlowCalculator.CalculatePowerLoss(_admittances, NodeVoltages);
                var absolutePowerError = powerSum - powerLoss;
                return absolutePowerError.Magnitude/powerSum.Magnitude;
            }
        }

        public Vector<Complex> NodeVoltages
        {
            get
            {
                var voltages = new DenseVector(_nodeCount);

                for (var i = 0; i < _nodeCount; ++i)
                    voltages[i] = _nodes[i].Voltage;

                return voltages;
            }
        }

        public Vector<Complex> NodePowers
        {
            get
            {
                var powers = new DenseVector(_nodeCount);

                for (var i = 0; i < _nodeCount; ++i)
                    powers[i] = _nodes[i].Power;

                return powers;
            }
        }

        public int NodeCount
        {
            get { return _nodeCount; }
        }

        public double NominalVoltage
        {
            get { return _nominalVoltage; }
        }
        #endregion
    }
}
