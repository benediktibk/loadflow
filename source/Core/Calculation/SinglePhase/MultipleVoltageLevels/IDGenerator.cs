﻿using System;
using System.Collections.Generic;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class IdGenerator
    {
        private readonly HashSet<int> _inUse;
        private int _creationCounter;

        public IdGenerator()
        {
            _inUse = new HashSet<int>();
            _creationCounter = 0;
        }

        public bool IsAlreadyUsed(int id)
        {
            return _inUse.Contains(id);
        }

        public void Add(int id)
        {
            if (id < 0)
                throw new ArgumentOutOfRangeException("id", "must not be negative");

            if (IsAlreadyUsed(id))
                throw new ArgumentException("id is already in use");

            _inUse.Add(id);
        }

        public int Generate()
        {
            if (_creationCounter == int.MinValue)
                throw new InvalidOperationException("no free ids left");

            --_creationCounter;
            _inUse.Add(_creationCounter);
            return _creationCounter;
        }

        public int Count
        {
            get { return _inUse.Count; }
        }
    }
}
