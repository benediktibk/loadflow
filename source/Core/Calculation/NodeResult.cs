using System.Numerics;

namespace Calculation
{
    public class NodeResult
    {
        public NodeResult()
        {
            Voltage = new Complex();
            Power = new Complex();
            Id = 0;
        }

        public int Id { get; set; }
        public Complex Voltage { get; set; }
        public Complex Power { get; set; }
    }
}
