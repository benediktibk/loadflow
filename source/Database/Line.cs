namespace Database
{
    public class Line
    {
        public Line()
        {
            SeriesResistancePerUnitLength = 1;
            SeriesInductancePerUnitLength = 1;
            Length = 1;
            ShuntCapacityPerUnitLength = 0;
            ShuntConductancePerUnitLength = 0;
        }

        public Node NodeOne { get; set; }
        public Node NodeTwo { get; set; }
        public double SeriesResistancePerUnitLength { get; set; }
        public double SeriesInductancePerUnitLength { get; set; }
        public double ShuntConductancePerUnitLength { get; set; }
        public double ShuntCapacityPerUnitLength { get; set; }
        public double Length { get; set; }
    }
}
