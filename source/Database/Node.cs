namespace Database
{
    public class Node
    {
        public Node()
        {
            NominalVoltage = 1;
        }

        public long Id { get; set; }
        public double NominalVoltage { get; set; }
    }
}
