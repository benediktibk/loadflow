using System;
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public class PqNode : INode
    {
        public PqNode(Complex power)
        {
            Power = power;
        }

        public Complex Power { get; private set; }

        public void AddTo(IList<NodeWithIndex> slackNodes, IList<NodeWithIndex> pqNodes, IList<NodeWithIndex> pvNodes, int index)
        {
            pqNodes.Add(new NodeWithIndex(this, index));
        }

        public void AddTo(IList<PvNodeWithIndex> pvBuses, int index)
        {
            throw new InvalidOperationException();
        }

        public void AddTo(IList<PqNodeWithIndex> pqBuses, int index)
        {
            pqBuses.Add(new PqNodeWithIndex(index, Power));
        }

        public void SetVoltageIn(Vector<Complex> voltages, int index)
        {
            throw new InvalidOperationException();
        }

        public void SetVoltageMagnitudeIn(Vector<Complex> voltages, int index)
        {
            throw new InvalidOperationException();
        }

        public void SetPowerIn(Vector<Complex> powers, int index)
        {
            powers[index] = Power;
        }

        public void SetRealPowerIn(Vector<Complex> powers, int index)
        {
            throw new InvalidOperationException();
        }

        public INode Merge(INode node)
        {
            if (node is SlackNode)
                return node.Merge(this);
            if (node is PvNode)
                return node.Merge(this);

            var pqNode = node as PqNode;

            if (pqNode == null)
                throw new NotSupportedException("node type is not supported");

            return new PqNode(Power + pqNode.Power);
        }
    }
}
