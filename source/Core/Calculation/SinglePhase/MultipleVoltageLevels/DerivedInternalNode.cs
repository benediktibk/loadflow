﻿using System.Collections.Generic;
using Calculation.SinglePhase.SingleVoltageLevel;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public abstract class DerivedInternalNode : IReadOnlyNode
    {
        private readonly IExternalReadOnlyNode _sourceNode;

        protected DerivedInternalNode(IExternalReadOnlyNode sourceNode, int id, string name)
        {
            _sourceNode = sourceNode;
            Id = id;
            Name = name;
        }

        public double NominalVoltage 
        {
            get { return _sourceNode.NominalVoltage; }
        }

        public int Id { get; private set; }

        public string Name { get; private set; }

        public abstract INode CreateSingleVoltageNode(double scaleBasePower, ISet<IExternalReadOnlyNode> visited, bool includeDirectConnections);
    }
}
