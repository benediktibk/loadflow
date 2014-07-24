using System.Collections.Generic;

namespace Database
{
    public class Database
    {
        private readonly List<PowerNet> _powerNets;

        public Database()
        {
            _powerNets = new List<PowerNet>();
        }

        public IList<PowerNet> PowerNets
        {
            get { return _powerNets; }
        }
    }
}
