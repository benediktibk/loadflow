using System;
using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using MathNet.Numerics;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class TransmissionLine : IPowerNetElement
    {
        private readonly IExternalReadOnlyNode _sourceNode;
        private readonly IExternalReadOnlyNode _targetNode;
        private readonly TransmissionLineData _data;

        public TransmissionLine(IExternalReadOnlyNode sourceNode, IExternalReadOnlyNode targetNode, double seriesResistancePerUnitLength, double seriesInductancePerUnitLength, double shuntCapacityPerUnitLength, double shuntConductancePerUnitLength, double length, double frequency, bool transmissionEquationModel)
        {
            _sourceNode = sourceNode;
            _targetNode = targetNode;
            _data = new TransmissionLineData(seriesResistancePerUnitLength, seriesInductancePerUnitLength,
                shuntCapacityPerUnitLength, shuntConductancePerUnitLength, length, frequency, transmissionEquationModel);
        }

        public Complex LengthImpedance
        {
            get { return _data.LengthImpedance; }
        }

        public Complex ShuntAdmittance
        {
            get { return _data.ShuntAdmittance; }
        }

        public double SourceNominalVoltage
        {
            get { return _sourceNode.NominalVoltage; }
        }

        public double TargetNominalVoltage
        {
            get { return _targetNode.NominalVoltage; }
        }

        public bool NominalVoltagesMatch
        {
            get { return Math.Abs((TargetNominalVoltage - SourceNominalVoltage)/TargetNominalVoltage) < 0.0000001; }
        }

        public bool NeedsGroundNode
        {
            get { return _data.NeedsGroundNode; }
        }

        public double MaximumPower
        {
            get { return 0; }
        }

        public bool IsDirectConnection
        {
            get { return _data.IsDirectConnection; }
        }

        public void AddDirectConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            if (visitedNodes.Contains(_sourceNode) || IsDirectConnection)
                _sourceNode.AddDirectConnectedNodes(visitedNodes);
            if (visitedNodes.Contains(_targetNode) || IsDirectConnection)
                _targetNode.AddDirectConnectedNodes(visitedNodes);
        }

        public INode CreateSingleVoltageNode(double scaleBasePower, ISet<IExternalReadOnlyNode> visited, bool includeDirectConnections)
        {
            INode result = new PqNode(new Complex());

            if (!IsDirectConnection || !includeDirectConnections) 
                return result;

            if (!visited.Contains(_sourceNode))
                result = result.Merge(_sourceNode.CreateSingleVoltageNode(scaleBasePower, visited, includeDirectConnections));

            if (!visited.Contains(_targetNode))
                result = result.Merge(_targetNode.CreateSingleVoltageNode(scaleBasePower, visited, includeDirectConnections));

            return result;
        }

        public void AddConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            _sourceNode.AddConnectedNodes(visitedNodes);
            _targetNode.AddConnectedNodes(visitedNodes);
        }

        public void AddConnectedNodesOnSameVoltageLevel(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            _sourceNode.AddConnectedNodesOnSameVoltageLevel(visitedNodes);
            _targetNode.AddConnectedNodesOnSameVoltageLevel(visitedNodes);
        }

        public void FillInAdmittances(IAdmittanceMatrix admittances, double scaleBasePower, IReadOnlyNode groundNode, double expectedLoadFlow)
        {
            if (NeedsGroundNode && groundNode == null)
                throw new ArgumentNullException("groundNode");

            var scaler = new DimensionScaler(TargetNominalVoltage, scaleBasePower);

            if (!IsDirectConnection)
            {
                var lengthAdmittanceScaled = scaler.ScaleAdmittance(1.0/LengthImpedance);
                admittances.AddConnection(_sourceNode, _targetNode, lengthAdmittanceScaled);
            }

            if (!NeedsGroundNode)
                return;

            var shuntAdmittanceScaled = scaler.ScaleAdmittance(ShuntAdmittance);
            admittances.AddConnection(_sourceNode, groundNode, shuntAdmittanceScaled);
            admittances.AddConnection(_targetNode, groundNode, shuntAdmittanceScaled);
        }

        public void FillInConstantCurrents(IList<Complex> constantCurrents, IReadOnlyDictionary<IReadOnlyNode, int> nodeIndices, double scaleBasePower)
        { }

        public bool IsConnectedTo(ISet<IExternalReadOnlyNode> nodes)
        {
            return nodes.Contains(_sourceNode) || nodes.Contains(_targetNode);
        }

        public IList<IReadOnlyNode> GetInternalNodes()
        {
            return new List<IReadOnlyNode>();
        }
    }
}
