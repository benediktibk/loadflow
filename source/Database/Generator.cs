namespace Database
{
    public class Generator
    {
        public Generator()
        {
            VoltageMagnitude = 1;
            RealPower = 0;
        }

        public Node Node { get; set; }
        public double VoltageMagnitude { get; set; }
        public double RealPower { get; set; }
    }
}
