using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SincalConnector;
using UnitTestHelper;

namespace SincalConnectorTest
{
    [TestClass]
    public class PowerNetTest
    {
        #region variables

        private INodeVoltageCalculator _calculator;

        #endregion

        #region setup

        [TestInitialize]
        public void SetUp()
        {
            _calculator = new CurrentIteration(0.0000001, 10000);
        }

        #endregion

        #region read tests

        [TestMethod]
        [TestCategory("Integration")]
        public void Constructor_NetWithTwoNodes_NodeValuesAreCorrect()
        {
            var powerNet = new PowerNet("testdata/node_files/database.mdb");

            var nodes = powerNet.Nodes;
            Assert.AreEqual(1, nodes[0].Id);
            Assert.AreEqual("N1", nodes[0].Name);
            Assert.AreEqual(1000, nodes[0].NominalVoltage, 0.0001);
            Assert.AreEqual(2, nodes[1].Id);
            Assert.AreEqual("N2", nodes[1].Name);
            Assert.AreEqual(1000, nodes[1].NominalVoltage, 0.0001);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Constructor_NetWithTwoNodesAt10kV_NodeValuesAreCorrect()
        {
            var powerNet = new PowerNet("testdata/node_10kV_files/database.mdb");

            var nodes = powerNet.Nodes;
            Assert.AreEqual(1, nodes[0].Id);
            Assert.AreEqual("N1", nodes[0].Name);
            Assert.AreEqual(10000, nodes[0].NominalVoltage, 0.0001);
            Assert.AreEqual(2, nodes[1].Id);
            Assert.AreEqual("N2", nodes[1].Name);
            Assert.AreEqual(10000, nodes[1].NominalVoltage, 0.0001);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Constructor_VillageNet_116Nodes()
        {
            var powerNet = new PowerNet("testdata/dorfnetz_files/database.mdb");

            Assert.AreEqual(116, powerNet.Nodes.Count);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Constructor_NetWithTwoNodes_FeedInValuesAreCorrect()
        {
            var powerNet = new PowerNet("testdata/node_files/database.mdb");

            var feedIns = powerNet.FeedIns;
            Assert.AreEqual(1, feedIns.Count);
            var feedIn = feedIns[0];
            var voltage = Complex.FromPolarCoordinates(990, 10 * Math.PI / 180);
            ComplexAssert.AreEqual(voltage, feedIn.Voltage, 0.000001);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Constructor_NetWithTwoNodesAt10kV_FeedInVoltageIsCorrect()
        {
            var powerNet = new PowerNet("testdata/node_10kv_files/database.mdb");

            var feedIns = powerNet.FeedIns;
            Assert.AreEqual(1, feedIns.Count);
            var feedIn = feedIns[0];
            var voltage = Complex.FromPolarCoordinates(10100, 0);
            ComplexAssert.AreEqual(voltage, feedIn.Voltage, 0.000001);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Constructor_NetWithSpecialFeedIn_FeedInVoltageIsCorrect()
        {
            var powerNet = new PowerNet("testdata/feedin_files/database.mdb");

            var feedIns = powerNet.FeedIns;
            Assert.AreEqual(1, feedIns.Count);
            var feedIn = feedIns[0];
            var voltage = Complex.FromPolarCoordinates(1050, 10*Math.PI/180);
            ComplexAssert.AreEqual(voltage, feedIn.Voltage, 0.000001);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Constructor_NetWithTwoNodes_LoadIsCorrect()
        {
            var powerNet = new PowerNet("testdata/node_files/database.mdb");

            var loads = powerNet.Loads;
            Assert.AreEqual(1, loads.Count);
            var load = loads[0];
            ComplexAssert.AreEqual(-1e5, -1e4, load.LoadValue, 0.000001);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Constructor_NetWithTwoNodes_FrequencyIs50()
        {
            var powerNet = new PowerNet("testdata/node_files/database.mdb");

            Assert.AreEqual(50, powerNet.Frequency, 0.000001);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Constructor_NetWithTwoNodes_TransmissionLineIsCorrect()
        {
            var powerNet = new PowerNet("testdata/node_files/database.mdb");

            var transmissionLines = powerNet.TransmissionLines;
            Assert.AreEqual(1, transmissionLines.Count);
            var transmissionLine = transmissionLines.First();
            Assert.AreEqual(1000, transmissionLine.Length, 0.000001);
            Assert.AreEqual(1e-4, transmissionLine.SeriesResistancePerUnitLength, 0.000001);
            Assert.AreEqual(1.273239545e-6, transmissionLine.SeriesInductancePerUnitLength, 0.000001);
            Assert.AreEqual(1.0/2e7, transmissionLine.ShuntConductancePerUnitLength, 0.000001);
            Assert.AreEqual(0.2e-12, transmissionLine.ShuntCapacityPerUnitLength, 0.000001);
            Assert.IsTrue(transmissionLine.TransmissionEquationModel);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [ExpectedException(typeof(NotSupportedException))]
        public void Constructor_NetWithUnsupportedElement_ThrowsException()
        {
            var powerNet = new PowerNet("testdata/unsupported_files/database.mdb");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Constructor_NetWithTransformer_TransformerValuesAreCorrect()
        {
            var powerNet = new PowerNet("testdata/transformer_files/database.mdb");

            var transformers = powerNet.TwoWindingTransformers;
            Assert.AreEqual(1, transformers.Count);
            var transformer = transformers.First();
            Assert.AreEqual(5e5, transformer.NominalPower, 0.00001);
            Assert.AreEqual(0.08, transformer.RelativeShortCircuitVoltage, 0.000001);
            Assert.AreEqual(0.02, transformer.RelativeNoLoadCurrent, 0.000001);
            Assert.AreEqual(1e3, transformer.IronLosses, 0.000001);
            Assert.AreEqual(Math.PI, transformer.PhaseShift.Radiant, 0.000001);
            Assert.AreEqual(5e2, transformer.CopperLosses, 0.000001);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Constructor_NetWithGeneratorVersionOne_GeneratorValuesAreCorrect()
        {
            var powerNet = new PowerNet("testdata/generator1_files/database.mdb");

            var generators = powerNet.Generators;
            Assert.AreEqual(1, generators.Count);
            var generator = generators.First();
            Assert.AreEqual(9e5, generator.RealPower, 0.00001);
            Assert.AreEqual(1050, generator.VoltageMagnitude, 0.00001);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Constructor_NetWithGeneratorVersionTwo_GeneratorValuesAreCorrect()
        {
            var powerNet = new PowerNet("testdata/generator2_files/database.mdb");

            var generators = powerNet.Generators;
            Assert.AreEqual(1, generators.Count);
            var generator = generators.First();
            Assert.AreEqual(9e5, generator.RealPower, 0.00001);
            Assert.AreEqual(1030, generator.VoltageMagnitude, 0.00001);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Constructor_NetWithGeneratorVersionThree_GeneratorValuesAreCorrect()
        {
            var powerNet = new PowerNet("testdata/generator3_files/database.mdb");

            var generators = powerNet.Generators;
            Assert.AreEqual(1, generators.Count);
            var generator = generators.First();
            Assert.AreEqual(9e5, generator.RealPower, 0.00001);
            Assert.AreEqual(1050, generator.VoltageMagnitude, 0.00001);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Constructor_NetWithGeneratorVersionFour_GeneratorValuesAreCorrect()
        {
            var powerNet = new PowerNet("testdata/generator4_files/database.mdb");

            var generators = powerNet.Generators;
            Assert.AreEqual(1, generators.Count);
            var generator = generators.First();
            Assert.AreEqual(9e5, generator.RealPower, 0.00001);
            Assert.AreEqual(1030, generator.VoltageMagnitude, 0.00001);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Constructor_GermanNet_NoErrors()
        {
            var powerNet = new PowerNet("testdata/uebertragungsnetz_deutschland_files/database.mdb");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Constructor_NetWithImpedanceLoadVersionOne_ImpedanceLoadValueseAreCorrect()
        {
            var powerNet = new PowerNet("testdata/impedanceload1_files/database.mdb");

            var impedanceLoads = powerNet.ImpedanceLoads;
            Assert.AreEqual(1, impedanceLoads.Count);
            var impedanceLoad = impedanceLoads.First();
            var impedance = impedanceLoad.Impedance;
            var impedanceShouldBe = (1050*1050)/new Complex(1e5, -1e4);
            ComplexAssert.AreEqual(impedanceShouldBe, impedance, 0.00001);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Constructor_NetWithImpedanceLoadVersionTwo_ImpedanceLoadValueseAreCorrect()
        {
            var powerNet = new PowerNet("testdata/impedanceload2_files/database.mdb");

            var impedanceLoads = powerNet.ImpedanceLoads;
            Assert.AreEqual(1, impedanceLoads.Count);
            var impedanceLoad = impedanceLoads.First();
            var impedance = impedanceLoad.Impedance;
            var impedanceShouldBe = (980*980) / new Complex(1e5, -1e4);
            ComplexAssert.AreEqual(impedanceShouldBe, impedance, 0.00001);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Constructor_NetWithSlackGeneratorVersionOne_GeneratorValuesAreCorrect()
        {
            var powerNet = new PowerNet("testdata/slackgenerator1_files/database.mdb");

            var slackGenerators = powerNet.SlackGenerators;
            Assert.AreEqual(1, slackGenerators.Count);
            var slackGenerator = slackGenerators.First();
            var voltage = slackGenerator.Voltage;
            var voltageShouldBe = Complex.FromPolarCoordinates(1050, 10*Math.PI/180);
            ComplexAssert.AreEqual(voltageShouldBe, voltage, 0.00001);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Constructor_NetWithSlackGeneratorVersionTwo_GeneratorValuesAreCorrect()
        {
            var powerNet = new PowerNet("testdata/slackgenerator2_files/database.mdb");

            var slackGenerators = powerNet.SlackGenerators;
            Assert.AreEqual(1, slackGenerators.Count);
            var slackGenerator = slackGenerators.First();
            var voltage = slackGenerator.Voltage;
            var voltageShouldBe = Complex.FromPolarCoordinates(1030, 10 * Math.PI / 180);
            ComplexAssert.AreEqual(voltageShouldBe, voltage, 0.00001);
        }

        #endregion

        #region caculation tests

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_NetWithOneTransmissionLineVersionOne_ResultsAreCorrect()
        {
            var powerNet = new PowerNet("testdata/calculation_transmissionline1_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_NetWithOneTransmissionLineVersionTwo_ResultsAreCorrect()
        {
            var powerNet = new PowerNet("testdata/calculation_transmissionline2_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.00001);
            ArePowersEqual(sincalResults, ownResults, 0.00001);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_NetWithOneTransmissionLineVersionThree_ResultsAreCorrect()
        {
            var powerNet = new PowerNet("testdata/calculation_transmissionline3_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.00001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_NetWithOneTransmissionLineVersionFour_ResultsAreCorrect()
        {
            var powerNet = new PowerNet("testdata/calculation_transmissionline4_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.00001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_NetWithOneTransmissionLineVersionFive_ResultsAreCorrect()
        {
            var powerNet = new PowerNet("testdata/calculation_transmissionline5_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.00001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_NetWithSlackGeneratorVersionOne_ResultsAreCorrect()
        {
            var powerNet = new PowerNet("testdata/calculation_slackgenerator1_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.00001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_NetWithImpedanceLoadVersionOne_ResultsAreCorrect()
        {
            var powerNet = new PowerNet("testdata/calculation_impedanceload1_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.00001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_NetWithImpedanceLoadVersionTwo_ResultsAreCorrect()
        {
            var powerNet = new PowerNet("testdata/calculation_impedanceload2_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.00001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_NetWithGeneratorVersionOne_ResultsAreCorrect()
        {
            var powerNet = new PowerNet("testdata/calculation_generator1_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_NetWithTransformerVersionOne_ResultsAreCorrect()
        {
            var powerNet = new PowerNet("testdata/calculation_transformer1_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.00001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_NetWithTransformerVersionTwo_ResultsAreCorrect()
        {
            var powerNet = new PowerNet("testdata/calculation_transformer2_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.00001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_NetWithTransformerVersionThree_ResultsAreCorrect()
        {
            var powerNet = new PowerNet("testdata/calculation_transformer3_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_NetWithTransformerVersionFour_ResultsAreCorrect()
        {
            var powerNet = new PowerNet("testdata/calculation_transformer4_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_NetWithTransformerVersionFive_ResultsAreCorrect()
        {
            var powerNet = new PowerNet("testdata/calculation_transformer5_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_CountryNetWithOverheadLine1_ResultsAreCorrect()
        {
            var powerNet = new PowerNet("testdata/landnetz_freileitung1_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_CountryNetWithOverheadLine2_ResultsAreCorrect()
        {
            var powerNet = new PowerNet("testdata/landnetz_freileitung2_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_CountryNetWithCable1_ResultsAreCorrect()
        {
            var powerNet = new PowerNet("testdata/landnetz_kabel1_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_CountryNetWithCable2_ResultsAreCorrect()
        {
            var powerNet = new PowerNet("testdata/landnetz_kabel2_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_SuburbanNetWithCable1_ResultsAreCorrect()
        {
            var powerNet = new PowerNet("testdata/vorstadtnetz_kabel1_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_SuburbanNetWithCable2_ResultsAreCorrect()
        {
            var powerNet = new PowerNet("testdata/vorstadtnetz_kabel2_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_NetWithTransformerVersionSix_AllTableEntriesAreCorrect()
        {
            var powerNet = new PowerNet("testdata/calculation_transformer6_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultTableEntriesFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultTableEntriesFromDatabase();
            AreEqual(sincalResults, ownResults, 1e-6, 1e-5, 1e-3, 1e-2);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_NetWithTransmissionLineVersionSix_AllTableEntriesAreCorrect()
        {
            var powerNet = new PowerNet("testdata/calculation_transmissionline6_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultTableEntriesFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultTableEntriesFromDatabase();
            AreEqual(sincalResults, ownResults, 1e-6, 1e-5, 1e-3, 1e-2);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_NetWithThreeWindingTransformer_ResultsAreCorrect()
        {
            var powerNet = new PowerNet("testdata/calculation_threewindingtransformer1_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        #endregion

        #region static functions 

        public static void AreEqual(NodeResultTableEntry one, NodeResultTableEntry two, double deltaPower, double deltaVoltageMagnitude, double deltaVoltagePhase, double deltaVoltagePercentage)
        {
            Assert.AreEqual(one.VoltageMagnitude, two.VoltageMagnitude, deltaVoltageMagnitude);
            Assert.AreEqual(one.VoltageMagnitudeToNominalVoltage, two.VoltageMagnitudeToNominalVoltage, deltaVoltagePercentage);
            Assert.AreEqual(one.VoltagePhase, two.VoltagePhase, deltaVoltagePhase);
            Assert.AreEqual(one.RealPower, two.RealPower, deltaPower);
            Assert.AreEqual(one.ImaginaryPower, two.ImaginaryPower, deltaPower);
            Assert.AreEqual(one.PowerMagnitude, two.PowerMagnitude, deltaPower);
            Assert.AreEqual(one.StringVoltageMagnitude, two.StringVoltageMagnitude, deltaVoltageMagnitude);
            Assert.AreEqual(one.StringVoltageMagnitudeToNominalStringVoltage, two.StringVoltageMagnitudeToNominalStringVoltage, deltaVoltagePercentage);
            Assert.AreEqual(one.StringVoltagePhase, two.StringVoltagePhase, deltaVoltagePhase);
            Assert.AreEqual(one.VoltagePhaseWithRotation, two.VoltagePhaseWithRotation, deltaVoltagePhase);
            Assert.AreEqual(one.StringVoltagePhaseWithRotation, two.StringVoltagePhaseWithRotation, deltaVoltagePhase);
            Assert.AreEqual(one.ResultType, two.ResultType);
            Assert.AreEqual(one.ResultState, two.ResultState);
        }

        public static void AreEqual(IList<NodeResultTableEntry> one, IList<NodeResultTableEntry> two, double deltaPower,
            double deltaVoltageMagnitude, double deltaVoltagePhase, double deltaVoltagePercentage)
        {
            Assert.AreEqual(one.Count, two.Count);

            for (var i = 0; i < one.Count; ++i)
                AreEqual(one[i], two[i], deltaPower, deltaVoltageMagnitude, deltaVoltagePhase, deltaVoltagePercentage);
        }

        public static void AreVoltagesEqual(IList<NodeResult> first, IList<NodeResult> second, double delta)
        {
            Assert.AreEqual(first.Count, second.Count);

            var firstById = first.ToDictionary(node => node.NodeId);

            foreach (var secondNode in second)
            {
                var firstNode = firstById[secondNode.NodeId];
                ComplexAssert.AreEqual(firstNode.Voltage, secondNode.Voltage, delta);
            }
        }

        public static void ArePowersEqual(IList<NodeResult> first, IList<NodeResult> second, double delta)
        {
            Assert.AreEqual(first.Count, second.Count);

            var firstById = first.ToDictionary(node => node.NodeId);

            foreach (var secondNode in second)
            {
                var firstNode = firstById[secondNode.NodeId];
                ComplexAssert.AreEqual(firstNode.Power, secondNode.Power, delta);
            }
        }

        #endregion
    }
}
