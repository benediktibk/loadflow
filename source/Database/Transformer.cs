namespace Database
{
    public class Transformer
    {
        public Transformer()
        {
            NominalPower = 10;
            RelativeShortCircuitVoltage = 0.01;
            CopperLosses = 0.01;
            IronLosses = 0.01;
            RelativeNoLoadCurrent = 0.01;
            Ratio = 1;
        }

        public Node UpperSideNode { get; set; }
        public Node LowerSideNode { get; set; }
        public double NominalPower { get; set; }
        public double RelativeShortCircuitVoltage { get; set; }
        public double CopperLosses { get; set; }
        public double IronLosses { get; set; }
        public double RelativeNoLoadCurrent { get; set; }
        public double Ratio { get; set; }
    }
}
