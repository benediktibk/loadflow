using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;
using Moq;
using SincalConnector;

namespace SincalConnectorIntegrationTest
{
    [TestClass]
    public class PowerNetDatabaseAdapterTest
    {
        private INodeVoltageCalculator _calculator;
        private INodeVoltageCalculator _calculatorHelm;
        private Mock<INodeVoltageCalculator> _calculatorMock;
        private double _relativePowerError;

        [TestInitialize]
        public void SetUp()
        {
            _calculator = new CurrentIteration(0.0000001, 10000, true);
            _calculatorHelm = new HolomorphicEmbeddedLoadFlowMethod(0.0000001, 50, 64, true);
            _calculatorMock = new Mock<INodeVoltageCalculator>();
            _calculatorMock.Setup(x => x.CalculateUnknownVoltages(
                It.IsAny<IReadOnlyAdmittanceMatrix>(), It.IsAny<IList<Complex>>(), It.IsAny<double>(),
                It.IsAny<Vector<Complex>>(), It.IsAny<Vector<Complex>>(), It.IsAny<IList<PqNodeWithIndex>>(),
                It.IsAny<IList<PvNodeWithIndex>>()))
                .Returns((IReadOnlyAdmittanceMatrix admittances, IList<Complex> admittanceRowSums, double nominalVoltage, Vector<Complex> initialVoltages,
                        Vector<Complex> constantCurrents, IList<PqNodeWithIndex> pqBuses, IList<PvNodeWithIndex>  pvBuses) => initialVoltages);
        }

        [TestMethod]
        public void Constructor_NetWithTwoNodes_NodeValuesAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/node_files/database.mdb");

            var nodes = powerNet.Data.Nodes;
            Assert.AreEqual(1, nodes[0].Id);
            Assert.AreEqual("N1", nodes[0].Name);
            Assert.AreEqual(1000, nodes[0].NominalVoltage, 0.0001);
            Assert.AreEqual(2, nodes[1].Id);
            Assert.AreEqual("N2", nodes[1].Name);
            Assert.AreEqual(1000, nodes[1].NominalVoltage, 0.0001);
        }

        [TestMethod]
        public void Constructor_NetWithTwoNodesAt10kV_NodeValuesAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/node_10kV_files/database.mdb");

            var nodes = powerNet.Data.Nodes;
            Assert.AreEqual(1, nodes[0].Id);
            Assert.AreEqual("N1", nodes[0].Name);
            Assert.AreEqual(10000, nodes[0].NominalVoltage, 0.0001);
            Assert.AreEqual(2, nodes[1].Id);
            Assert.AreEqual("N2", nodes[1].Name);
            Assert.AreEqual(10000, nodes[1].NominalVoltage, 0.0001);
        }

        [TestMethod]
        public void Constructor_VillageNet_116Nodes()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/dorfnetz_files/database.mdb");

            Assert.AreEqual(116, powerNet.Data.Nodes.Count);
        }

        [TestMethod]
        public void Constructor_NetWithTwoNodes_FeedInValuesAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/node_files/database.mdb");

            var feedIns = powerNet.Data.FeedIns;
            Assert.AreEqual(1, feedIns.Count);
            var feedIn = feedIns[0];
            var voltage = Complex.FromPolarCoordinates(990, 10 * Math.PI / 180);
            ComplexAssert.AreEqual(voltage, feedIn.Voltage, 0.000001);
        }

        [TestMethod]
        public void Constructor_NetWithTwoNodesAt10kV_FeedInVoltageIsCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/node_10kv_files/database.mdb");

            var feedIns = powerNet.Data.FeedIns;
            Assert.AreEqual(1, feedIns.Count);
            var feedIn = feedIns[0];
            var voltage = Complex.FromPolarCoordinates(10100, 0);
            ComplexAssert.AreEqual(voltage, feedIn.Voltage, 0.000001);
        }

        [TestMethod]
        public void Constructor_NetWithSpecialFeedIn_FeedInVoltageIsCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/feedin_files/database.mdb");

            var feedIns = powerNet.Data.FeedIns;
            Assert.AreEqual(1, feedIns.Count);
            var feedIn = feedIns[0];
            var voltage = Complex.FromPolarCoordinates(1050, 10 * Math.PI / 180);
            ComplexAssert.AreEqual(voltage, feedIn.Voltage, 0.000001);
        }

        [TestMethod]
        public void Constructor_NetWithTwoNodes_LoadIsCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/node_files/database.mdb");

            var loads = powerNet.Data.Loads;
            Assert.AreEqual(1, loads.Count);
            var load = loads[0];
            ComplexAssert.AreEqual(-1e5, -1e4, load.LoadValue, 0.000001);
        }

        [TestMethod]
        public void Constructor_NetWithTwoNodes_FrequencyIs50()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/node_files/database.mdb");

            Assert.AreEqual(50, powerNet.Data.Frequency, 0.000001);
        }

        [TestMethod]
        public void Constructor_NetWithTwoNodes_TransmissionLineIsCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/node_files/database.mdb");

            var transmissionLines = powerNet.Data.TransmissionLines;
            Assert.AreEqual(1, transmissionLines.Count);
            var transmissionLine = transmissionLines.First();
            Assert.AreEqual(1000, transmissionLine.Length, 0.000001);
            Assert.AreEqual(1e-4, transmissionLine.SeriesResistancePerUnitLength, 0.000001);
            Assert.AreEqual(1.273239545e-6, transmissionLine.SeriesInductancePerUnitLength, 0.000001);
            Assert.AreEqual(1.0 / 2e7, transmissionLine.ShuntConductancePerUnitLength, 0.000001);
            Assert.AreEqual(0.2e-12, transmissionLine.ShuntCapacityPerUnitLength, 0.000001);
            Assert.IsTrue(transmissionLine.TransmissionEquationModel);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void Constructor_NetWithUnsupportedElement_ThrowsException()
        {
            new PowerNetDatabaseAdapter("testdata/unsupported_files/database.mdb");
        }

        [TestMethod]
        public void Constructor_NetWithTransformer_TransformerValuesAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/transformer_files/database.mdb");

            var transformers = powerNet.Data.TwoWindingTransformers;
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
        public void Constructor_NetWithGeneratorVersionOne_GeneratorValuesAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/generator1_files/database.mdb");

            var generators = powerNet.Data.Generators;
            Assert.AreEqual(1, generators.Count);
            var generator = generators.First();
            Assert.AreEqual(9e5, generator.RealPower, 0.00001);
            Assert.AreEqual(1050, generator.VoltageMagnitude, 0.00001);
        }

        [TestMethod]
        public void Constructor_NetWithGeneratorVersionTwo_GeneratorValuesAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/generator2_files/database.mdb");

            var generators = powerNet.Data.Generators;
            Assert.AreEqual(1, generators.Count);
            var generator = generators.First();
            Assert.AreEqual(9e5, generator.RealPower, 0.00001);
            Assert.AreEqual(1030, generator.VoltageMagnitude, 0.00001);
        }

        [TestMethod]
        public void Constructor_NetWithGeneratorVersionThree_GeneratorValuesAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/generator3_files/database.mdb");

            var generators = powerNet.Data.Generators;
            Assert.AreEqual(1, generators.Count);
            var generator = generators.First();
            Assert.AreEqual(9e5, generator.RealPower, 0.00001);
            Assert.AreEqual(1050, generator.VoltageMagnitude, 0.00001);
        }

        [TestMethod]
        public void Constructor_NetWithGeneratorVersionFour_GeneratorValuesAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/generator4_files/database.mdb");

            var generators = powerNet.Data.Generators;
            Assert.AreEqual(1, generators.Count);
            var generator = generators.First();
            Assert.AreEqual(9e5, generator.RealPower, 0.00001);
            Assert.AreEqual(1030, generator.VoltageMagnitude, 0.00001);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void Constructor_NetWithTransmissionLine1_ExceptionThrown()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/transmissionline1_files/database.mdb");

            var generators = powerNet.Data.Generators;
            Assert.AreEqual(1, generators.Count);
            var generator = generators.First();
            Assert.AreEqual(9e5, generator.RealPower, 0.00001);
            Assert.AreEqual(1030, generator.VoltageMagnitude, 0.00001);
        }

        [TestMethod]
        public void Constructor_NetWithTransmissionLine2_ShuntConductanceIsCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/transmissionline2_files/database.mdb");

            var transmissionLines = powerNet.Data.TransmissionLines;
            Assert.AreEqual(1, transmissionLines.Count);
            var transmissionLine = transmissionLines.First();
            Assert.AreEqual(5/1e6, transmissionLine.ShuntConductancePerUnitLength, 0.00001);
        }

        [TestMethod]
        public void Constructor_GermanNet_NoErrors()
        {
            new PowerNetDatabaseAdapter("testdata/uebertragungsnetz_deutschland_files/database.mdb");
        }

        [TestMethod]
        public void Constructor_NetWithImpedanceLoadVersionOne_ImpedanceLoadValueseAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/impedanceload1_files/database.mdb");

            var impedanceLoads = powerNet.Data.ImpedanceLoads;
            Assert.AreEqual(1, impedanceLoads.Count);
            var impedanceLoad = impedanceLoads.First();
            var impedance = impedanceLoad.Impedance;
            var impedanceShouldBe = (1050 * 1050) / new Complex(1e5, -1e4);
            ComplexAssert.AreEqual(impedanceShouldBe, impedance, 0.00001);
        }

        [TestMethod]
        public void Constructor_NetWithImpedanceLoadVersionTwo_ImpedanceLoadValueseAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/impedanceload2_files/database.mdb");

            var impedanceLoads = powerNet.Data.ImpedanceLoads;
            Assert.AreEqual(1, impedanceLoads.Count);
            var impedanceLoad = impedanceLoads.First();
            var impedance = impedanceLoad.Impedance;
            var impedanceShouldBe = (980 * 980) / new Complex(1e5, -1e4);
            ComplexAssert.AreEqual(impedanceShouldBe, impedance, 0.00001);
        }

        [TestMethod]
        public void Constructor_NetWithSlackGeneratorVersionOne_GeneratorValuesAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/slackgenerator1_files/database.mdb");

            var slackGenerators = powerNet.Data.SlackGenerators;
            Assert.AreEqual(1, slackGenerators.Count);
            var slackGenerator = slackGenerators.First();
            var voltage = slackGenerator.Voltage;
            var voltageShouldBe = Complex.FromPolarCoordinates(1050, 10 * Math.PI / 180);
            ComplexAssert.AreEqual(voltageShouldBe, voltage, 0.00001);
        }

        [TestMethod]
        public void Constructor_NetWithSlackGeneratorVersionTwo_GeneratorValuesAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/slackgenerator2_files/database.mdb");

            var slackGenerators = powerNet.Data.SlackGenerators;
            Assert.AreEqual(1, slackGenerators.Count);
            var slackGenerator = slackGenerators.First();
            var voltage = slackGenerator.Voltage;
            var voltageShouldBe = Complex.FromPolarCoordinates(1030, 10 * Math.PI / 180);
            ComplexAssert.AreEqual(voltageShouldBe, voltage, 0.00001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithOneTransmissionLineVersionOne_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_transmissionline1_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithOneTransmissionLineVersionTwo_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_transmissionline2_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.00001);
            ArePowersEqual(sincalResults, ownResults, 0.00001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithOneTransmissionLineVersionThree_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_transmissionline3_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.00001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithOneTransmissionLineVersionFour_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_transmissionline4_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.00001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithOneTransmissionLineVersionFive_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_transmissionline5_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.00001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithSlackGeneratorVersionOne_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_slackgenerator1_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.00001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithDirectConnection_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_transmissionline7_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.00001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithOpenSwitch_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_transmissionline8_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.00001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithYearlyDefinedLoad_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_load1_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.00001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithImpedanceLoadVersionOne_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_impedanceload1_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.00001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithImpedanceLoadVersionTwo_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_impedanceload2_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.00001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithGeneratorVersionOne_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_generator1_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithTransformerVersionOne_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_transformer1_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.00001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithTransformerVersionTwo_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_transformer2_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.00001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithTransformerVersionThree_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_transformer3_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithTransformerVersionFour_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_transformer4_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithTransformerVersionFive_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_transformer5_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_CountryNetWithOverheadLine1_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/landnetz_freileitung1_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_CountryNetWithOverheadLine2_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/landnetz_freileitung2_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_CountryNetWithCable1_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/landnetz_kabel1_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_CountryNetWithCable2_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/landnetz_kabel2_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_SuburbanNetWithCable1_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/vorstadtnetz_kabel1_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_SuburbanNetWithCable2_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/vorstadtnetz_kabel2_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithTransformerVersionSix_AllTableEntriesAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_transformer6_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultTableEntriesFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultTableEntriesFromDatabase();
            AreEqual(sincalResults, ownResults, 1e-6, 1e-5, 1e-3, 1e-2);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithTwoFeedIns_AllTableEntriesAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_feedin2_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultTableEntriesFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultTableEntriesFromDatabase();
            AreEqual(sincalResults, ownResults, 1e-6, 1e-5, 1e-3, 1e-2);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithTransmissionLineVersionSix_AllTableEntriesAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_transmissionline6_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultTableEntriesFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultTableEntriesFromDatabase();
            AreEqual(sincalResults, ownResults, 1e-6, 1e-5, 1e-3, 1e-2);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithNotConnectedTransmissionLine_AllTableEntriesAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_transmissionline9_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultTableEntriesFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultTableEntriesFromDatabase();
            AreEqual(sincalResults, ownResults, 1e-6, 1e-5, 1e-3, 1e-2);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithParallelSystemsVersionOne_AllTableEntriesAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_transmissionline10_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultTableEntriesFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultTableEntriesFromDatabase();
            AreEqual(sincalResults, ownResults, 1e-6, 1e-5, 1e-3, 1e-2);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithThreeWindingTransformerVersionOne_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_threewindingtransformer1_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithThreeWindingTransformerVersionTwo_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_threewindingtransformer2_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithThreeWindingTransformerVersionThree_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_threewindingtransformer3_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithThreeWindingTransformerVersionFour_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_threewindingtransformer4_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithThreeWindingTransformerVersionFive_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_threewindingtransformer5_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithThreeWindingTransformerVersionSix_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_threewindingtransformer6_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithThreeWindingTransformerVersionSeven_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_threewindingtransformer7_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithShuntReactanceVersionOne_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_shuntreactance1_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithShuntReactanceVersionTwo_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_shuntreactance2_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithShuntReactanceVersionThree_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_shuntreactance3_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithShuntReactanceVersionFour_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_shuntreactance4_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_NetWithShuntReactanceVersionFive_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/calculation_shuntreactance5_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_RuralNetWithCableVersionOne_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/landnetz_kabel1_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.0001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_GermanNetAndMockCalculator_NoExceptionThrown()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/uebertragungsnetz_deutschland_files/database.mdb");

            powerNet.CalculateNodeVoltages(_calculatorMock.Object, out _relativePowerError);
        }

        [TestMethod]
        public void CalculateNodeVoltages_CountryNetVersionOne_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/landnetz_kabel1_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.00001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_CountryNetVersionTwo_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/landnetz_kabel2_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 0.00001);
            ArePowersEqual(sincalResults, ownResults, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_CountryNetVersionOneWithHelm_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/landnetz_kabel1_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();

            var success = powerNet.CalculateNodeVoltages(_calculatorHelm, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 1);
            ArePowersEqual(sincalResults, ownResults, 100);
        }

        [TestMethod]
        public void CalculateNodeVoltages_CountryNetVersionOneWithHelmAndMorePreciseDatatype_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/landnetz_kabel1_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();
            var calculator = new HolomorphicEmbeddedLoadFlowMethod(1e-10, 70, 100, false);

            var success = powerNet.CalculateNodeVoltages(calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 1);
            ArePowersEqual(sincalResults, ownResults, 100);
        }

        [TestMethod]
        public void CalculateNodeVoltages_SuburbanNetAtConvergenceBorder_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/vorstadt_konvergenzgrenze_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();
            var calculator = new CurrentIteration(0.0000001, 100, true);

            var success = powerNet.CalculateNodeVoltages(calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 1);
            ArePowersEqual(sincalResults, ownResults, 1e5);
        }

        [TestMethod]
        public void CalculateNodeVoltages_SuburbanNetAtConvergenceBorderVersionTwo_ResultsAreCorrect()
        {
            var powerNet = new PowerNetDatabaseAdapter("testdata/vorstadt_konvergenzgrenze_version2_files/database.mdb");
            var sincalResults = powerNet.GetNodeResultsFromDatabase();
            var calculator = new CurrentIteration(0.0000001, 100, true);

            var success = powerNet.CalculateNodeVoltages(calculator, out _relativePowerError);

            Assert.IsTrue(success);
            var ownResults = powerNet.GetNodeResultsFromDatabase();
            AreVoltagesEqual(sincalResults, ownResults, 1);
            ArePowersEqual(sincalResults, ownResults, 1e2);
        }

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
    }
}
