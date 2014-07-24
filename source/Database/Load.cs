using System.Numerics;

namespace Database
{
    public class Load
    {
        public Load()
        {
            Value = new Complex();
        }

        public Node Node { get; set; }
        public Complex Value { get; set; }
    }
}
