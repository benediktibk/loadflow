using System.Numerics;

namespace Database
{
    public class FeedIn
    {
        public FeedIn()
        {
            Voltage = new Complex(1, 0);
        }

        public Node Node { get; set; }
        public Complex Voltage { get; set; }
        public double ShortCircuitPower { get; set; }
    }
}
