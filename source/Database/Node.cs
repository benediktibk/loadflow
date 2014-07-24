using System.Numerics;

namespace Database
{
    public class Node
    {
        public Node()
        {
            NominalVoltage = 1;
            Voltage = new Complex();
        }

        public long Id { get; set; }
        public double NominalVoltage { get; set; }
        public string Name { get; set; }
        public Complex Voltage { get; set; }
    }
}
