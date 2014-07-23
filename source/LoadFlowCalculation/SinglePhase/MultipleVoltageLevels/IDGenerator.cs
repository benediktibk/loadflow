using System;
using System.Collections.Generic;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public class IdGenerator
    {
        private readonly HashSet<long> _inUse;
        private long _creationCounter;

        public IdGenerator()
        {
            _inUse = new HashSet<long>();
            _creationCounter = 0;
        }

        public bool IsAlreadyUsed(long id)
        {
            return _inUse.Contains(id);
        }

        public void Add(long id)
        {
            if (id < 0)
                throw new ArgumentOutOfRangeException("id", "must not be negative");

            if (IsAlreadyUsed(id))
                throw new ArgumentException("id is already in use");

            _inUse.Add(id);
        }

        public long Generate()
        {
            if (_creationCounter == long.MinValue)
                throw new InvalidOperationException("no free ids left");

            --_creationCounter;
            _inUse.Add(_creationCounter);
            return _creationCounter;
        }

        public long Count
        {
            get { return _inUse.Count; }
        }
    }
}
