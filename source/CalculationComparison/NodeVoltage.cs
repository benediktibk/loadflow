using System.Numerics;

namespace CalculationComparison
{
    public class NodeVoltage
    {
        public string NodeName { get; set; }
        public bool Known { get; set; }
        public Complex Correct { get; set; }
        public Complex NodePotentialMethod { get; set; }
        public Complex CurrentIteraion { get; set; }
        public Complex NewtonRaphson { get; set; }
        public Complex FastDecoupledLoadFlow { get; set; }
        public Complex HolomorphicEmbeddingLoadFlowLongDouble { get; set; }
        public Complex HolomorphicEmbeddingLoadFlowMulti { get; set; }

        public NodeVoltage DeepClone()
        {
            return new NodeVoltage
            {
                NodeName = new string(NodeName.ToCharArray()),
                Known = Known,
                Correct = DeepCloneComplex(Correct),
                NodePotentialMethod = DeepCloneComplex(NodePotentialMethod),
                CurrentIteraion = DeepCloneComplex(CurrentIteraion),
                NewtonRaphson = DeepCloneComplex(NewtonRaphson),
                FastDecoupledLoadFlow = DeepCloneComplex(FastDecoupledLoadFlow),
                HolomorphicEmbeddingLoadFlowLongDouble = DeepCloneComplex(HolomorphicEmbeddingLoadFlowLongDouble),
                HolomorphicEmbeddingLoadFlowMulti = DeepCloneComplex(HolomorphicEmbeddingLoadFlowMulti)
            };
        }

        private static Complex DeepCloneComplex(Complex value)
        {
            return new Complex(value.Real, value.Imaginary);
        }
    }
}
