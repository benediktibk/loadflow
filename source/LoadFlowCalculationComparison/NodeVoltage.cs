
using System.Numerics;

namespace LoadFlowCalculationComparison
{
    public class NodeVoltage
    {
        public string NodeName { get; set; }
        public Complex Correct { get; set; }
        public Complex NodePotentialMethod { get; set; }
        public Complex CurrentIteraion { get; set; }
        public Complex NewtonRaphson { get; set; }
        public Complex FastDecoupledLoadFlow { get; set; }
        public Complex HolomorphicEmbeddingLoadFlowLongDouble { get; set; }
        public Complex HolomorphicEmbeddingLoadFlowMulti { get; set; }
    }
}
