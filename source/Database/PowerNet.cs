using System.Collections.Generic;

namespace Database
{
    public class PowerNet
    {
        #region variables

        private readonly List<Node> _nodes; 
        private readonly List<Line> _lines;
        private readonly List<Load> _loads;
        private readonly List<FeedIn> _feedIns;
        private readonly List<Generator> _generators;
        private readonly List<Transformer> _transformers;

        #endregion

        #region constructor

        public PowerNet()
        {
            Frequency = 50;
            _nodes = new List<Node>();
            _lines = new List<Line>();
            _loads = new List<Load>();
            _feedIns = new List<FeedIn>();
            _generators = new List<Generator>();
            _transformers = new List<Transformer>();
        }

        #endregion

        #region properties

        public double Frequency { get; set; }

        public IList<Node> Nodes
        {
            get { return _nodes; }
        } 

        public IList<Line> Lines
        {
            get { return _lines; }
        }

        public IList<Load> Loads
        {
            get { return _loads; }
        }

        public IList<FeedIn> FeedIns
        {
            get { return _feedIns; }
        }

        public IList<Generator> Generators
        {
            get { return _generators; }
        }

        public IList<Transformer> Transformers
        {
            get { return _transformers; }
        }

        #endregion
    }
}
