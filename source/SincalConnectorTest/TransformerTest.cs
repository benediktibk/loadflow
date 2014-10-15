using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SincalConnector;

namespace SincalConnectorTest
{
    [TestClass]
    public class TransformerTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void MapConnectionSymbolToPhaseShiftFactor_0_ThrowsException()
        {
            TwoWindingTransformer.MapConnectionSymbolToPhaseShiftFactor(0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void MapConnectionSymbolToPhaseShiftFactor_82_ThrowsException()
        {
            TwoWindingTransformer.MapConnectionSymbolToPhaseShiftFactor(0);
        }

        [TestMethod]
        public void MapConnectionSymbolToPhaseShiftFactor_1To81_ValidValue()
        {
            var validValues = new List<int>() {0, 1, 5, 6, 7, 11};
            for (var i = 1; i < 81; ++i)
            {
                var result = TwoWindingTransformer.MapConnectionSymbolToPhaseShiftFactor(i);
                Assert.IsTrue(validValues.Contains(result));
            }
        }
    }
}
