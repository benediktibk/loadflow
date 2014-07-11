using System;
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation.SinglePhase.SingleVoltageLevel
{
    public class PowerNet
    {
        #region variables
        private readonly int _nodeCount;
        private IList<Node> _nodes;
        private readonly Matrix<Complex> _admittances;
        private readonly double _nominalVoltage;
        #endregion

        #region public functions
        public PowerNet(int nodeCount, double nominalVoltage)
        {
            if (nodeCount <= 0)
                throw new ArgumentOutOfRangeException("nodeCount", "there must be at least one node");

            if (nominalVoltage <= 0)
                throw new ArgumentOutOfRangeException("nominalVoltage", "the nominal voltage must be positive");

            _nominalVoltage = nominalVoltage;
            _nodeCount = nodeCount;
            _admittances = new SparseMatrix(_nodeCount, _nodeCount);
            InitializeNodes();
        }

        public PowerNet(Matrix<Complex> admittances, double nominalVoltage)
        {
            if (admittances.RowCount != admittances.ColumnCount)
                throw new ArgumentOutOfRangeException("admittances", "must be symmetric");

            if (nominalVoltage <= 0)
                throw new ArgumentOutOfRangeException("nominalVoltage", "the nominal voltage must be positive");

            _nominalVoltage = nominalVoltage;
            _nodeCount = admittances.RowCount;
            _admittances = admittances;
            InitializeNodes();
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

        public bool IsVoltageOfNodeKnown(int node)
        {
            return _nodes[node].VoltageIsKnown;
        }

        public void AddSymmetricAdmittance(int i, int j, Complex admittance)
        {
            if (i == j)
                throw new ArgumentOutOfRangeException();

            _admittances[i, i] += admittance;
            _admittances[j, j] += admittance;
            _admittances[i, j] -= admittance;
            _admittances[j, i] -= admittance;
        }

        public void AddUnsymmetricAdmittance(int i, int j, Complex admittance)
        {
            _admittances[i, j] += admittance;
        }

        public Complex GetAdmittance(int i, int j)
        {
            return _admittances[i, j];
        }

        public IReadOnlyList<Node> GetNodes()
        {
            return (IReadOnlyList<Node>) _nodes;
        }
        #endregion

        #region private functions

        private void InitializeNodes()
        {
            _nodes = new List<Node>(_nodeCount);

            for (var i = 0; i < _nodeCount; ++i)
                _nodes.Add(new Node());
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

        public Matrix<Complex> Admittances
        {
           get { return _admittances.Clone(); }
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
