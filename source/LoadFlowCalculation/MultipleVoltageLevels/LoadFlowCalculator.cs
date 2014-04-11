using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using LoadFlowCalculation.SingleVoltageLevel;
using LoadFlowCalculation.SingleVoltageLevel.NodeVoltageCalculators;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public class LoadFlowCalculator
    {
        #region variables

        private readonly double _scaleBasisVoltage;
        private readonly double _scaleBasisPower;
        private readonly double _scaleBasisCurrent;
        private readonly double _scaleBasisImpedance;
        private readonly INodeVoltageCalculator _nodeVoltageCalculator;

        #endregion

        #region public functions

        public LoadFlowCalculator(double scaleBasisVoltage, double scaleBasisPower, INodeVoltageCalculator nodeVoltageCalculator)
        {
            _scaleBasisVoltage = scaleBasisVoltage;
            _scaleBasisPower = scaleBasisPower;
            _scaleBasisCurrent = scaleBasisPower/scaleBasisVoltage;
            _scaleBasisImpedance = _scaleBasisVoltage/_scaleBasisCurrent;
            _nodeVoltageCalculator = nodeVoltageCalculator;
        }

        public IDictionary<string, Complex> CalculateNodeVoltages(IReadOnlyPowerNet powerNet)
        {
            if (powerNet.CheckIfFloatingNodesExists())
                throw new ArgumentOutOfRangeException("powerNet", "there must not be a floating node");
            if (powerNet.CheckIfNominalVoltagesDoNotMatch())
                throw new ArgumentOutOfRangeException("powerNet", "the nominal voltages must match on connected nodes");
            if (powerNet.CheckIfNodeIsOverdetermined())
                throw new ArgumentOutOfRangeException("powerNet", "one node is overdetermined");

            var nodes = powerNet.GetNodes();
            var lines = powerNet.GetLines();

            var nodeIndexes = new Dictionary<IReadOnlyNode, int>();
            for (var i = 0; i < nodes.Count; ++i)
                nodeIndexes.Add(nodes[i], i);

            var admittanes = new SparseMatrix(nodes.Count, nodes.Count);
            foreach (var line in lines)
                line.FillInAdmittances(admittanes, nodeIndexes, ScaleBasisImpedance);

            var singleVoltageNodes = new SingleVoltageLevel.Node[nodes.Count];
            foreach (var node in nodes)
            {
                var singleVoltageNode = new SingleVoltageLevel.Node();

                if (node.MustBeSlackBus)
                    singleVoltageNode.Voltage = node.GetSlackVoltage(ScaleBasisVoltage);
                else if (node.MustBePVBus)
                {
                    var data = node.GetVoltageMagnitudeAndRealPowerForPVBus(ScaleBasisVoltage, ScaleBasisPower);
                    singleVoltageNode.VoltageMagnitude = data.Item1;
                    singleVoltageNode.RealPower = data.Item2;
                }
                else
                    singleVoltageNode.Power = node.GetTotalPowerForPQBus(ScaleBasisPower);

                var nodeIndex = nodeIndexes[node];
                singleVoltageNodes[nodeIndex] = singleVoltageNode;
            }

            var calculator = new SingleVoltageLevel.LoadFlowCalculator(_nodeVoltageCalculator);
            bool voltageCollapse;
            var singleVoltageNodesWithResults = calculator.CalculateNodeVoltagesAndPowers(admittanes, ScaleBasisVoltage,
                singleVoltageNodes, out voltageCollapse);

            var nodeVoltages = new Dictionary<string, Complex>();

            foreach (var node in nodes)
            {
                var index = nodeIndexes[node];
                var name = node.Name;
                var voltage = singleVoltageNodesWithResults[index].Voltage*node.NominalVoltage;
                nodeVoltages.Add(name, voltage);
            }

            return nodeVoltages;
        }

        #endregion

        #region public properties

        public double ScaleBasisVoltage
        {
            get { return _scaleBasisVoltage; }
        }

        public double ScaleBasisPower
        {
            get { return _scaleBasisPower; }
        }

        public double ScaleBasisCurrent
        {
            get { return _scaleBasisCurrent; }
        }

        public double ScaleBasisImpedance
        {
            get { return _scaleBasisImpedance; }
        }

        #endregion

        #region private functions
        #endregion
    }
}
