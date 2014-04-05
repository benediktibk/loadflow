using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using LoadFlowCalculation;
using LoadFlowCalculation.NodeVoltageCalculators;
using LoadFlowCalculationComparison.AlgorithmSettings;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;
using MathNet.Numerics.Statistics;

namespace LoadFlowCalculationComparison
{
    public class Calculator
    {
        #region variables
        private readonly GeneralSettings _generalSettings; 
        private readonly IterativeMethodSettings _currentIteration;
        private readonly IterativeMethodSettings _fastDecoupledLoadFlow;
        private readonly IterativeMethodSettings _newtonRaphson;
        private readonly HolomorphicEmbeddedLoadFlowMethodSettings _holomorphicEmbeddedLoadFlow;
        private readonly HolomorphicEmbeddedLoadFlowMethodSettingsHighAccuracy _holomorphicEmbeddedLoadFlowHighAccuracy;
        private readonly CombinedCalculationResults _combinedCalculationResults;
        private readonly CalculationResults _calculationResults;
        private readonly NodeVoltages _nodeVoltages;
        private readonly Dispatcher _mainDispatcher;
        #endregion

        #region delegates
        private delegate void ResultCalculated(CalculationResult result);
        private delegate void CombinedResultCalculated(CombinedCalculationResult result);
        private delegate void SetVoltages(Vector<Complex> voltages);
        #endregion

        #region constructor
        public Calculator(GeneralSettings generalSettings, IterativeMethodSettings currentIteration,
            IterativeMethodSettings fastDecoupledLoadFlow, IterativeMethodSettings newtonRaphson,
            HolomorphicEmbeddedLoadFlowMethodSettings holomorphicEmbeddedLoadFlow,
            HolomorphicEmbeddedLoadFlowMethodSettingsHighAccuracy holomorphicEmbeddedLoadFlowHighAccuracy,
            CombinedCalculationResults combinedCalculationResults, CalculationResults calculationResults,
            NodeVoltages nodeVoltages, Dispatcher mainDispatcher)
        {
            _generalSettings = generalSettings;
            _currentIteration = currentIteration;
            _fastDecoupledLoadFlow = fastDecoupledLoadFlow;
            _newtonRaphson = newtonRaphson;
            _holomorphicEmbeddedLoadFlow = holomorphicEmbeddedLoadFlow;
            _holomorphicEmbeddedLoadFlowHighAccuracy = holomorphicEmbeddedLoadFlowHighAccuracy;
            _combinedCalculationResults = combinedCalculationResults;
            _calculationResults = calculationResults;
            _nodeVoltages = nodeVoltages;
            _mainDispatcher = mainDispatcher;
        }
        #endregion

        #region power net creation
        private PowerNetSingleVoltageLevel CreatePowerNet(out Vector<Complex> correctVoltages, out bool voltageCollapse)
        {
            switch (_generalSettings.ProblemSelection)
            {
                case ProblemSelectionEnum.CollapsingTwoNodeSystem:
                    return CreatePowerNetForCollapsingTwoNodeSystem(out correctVoltages, out voltageCollapse);
                case ProblemSelectionEnum.StableTwoNodeSystem:
                    return CreatePowerNetForStableTwoNodeSystem(out correctVoltages, out voltageCollapse);
                case ProblemSelectionEnum.FiveNodeSystemWithFourPQBuses:
                    return CreatePowerNetForFiveNodeSystemWithFourPQBuses(out correctVoltages, out voltageCollapse);
                case ProblemSelectionEnum.FiveNodeSystemWithOneGroundNode:
                    return CreatePowerNetForFiveNodeSystemWithOneGroundNode(out correctVoltages, out voltageCollapse);
                case ProblemSelectionEnum.FiveNodeSystemWithThreePQBusesAndOnePVBus:
                    return CreatePowerNetForFiveNodeSystemWithThreePQBusesAndOnePVBus(out correctVoltages,
                        out voltageCollapse);
                case ProblemSelectionEnum.TwoNodeSystemWithOnePVBus:
                    return CreatePowerNetOnePVBus(out correctVoltages, out voltageCollapse);
                case ProblemSelectionEnum.ThreeNodeSystemWithTwoPVBusses:
                    return CreatePowerNetThreeNodeSystemWithTwoPVBuses(out correctVoltages, out voltageCollapse);
                case ProblemSelectionEnum.NearlyCollapsingSystemWithPQBus:
                    return CreatePowerNetWithPQBusNearlyCollapsing(out correctVoltages, out voltageCollapse);
                case ProblemSelectionEnum.NearlyCollapsingSystemWithPVBus:
                    return CreatePowerNetWithPVBusNearlyCollapsing(out correctVoltages, out voltageCollapse);
                case ProblemSelectionEnum.ThreeNodeSystemWithOnePQAndOnePVBus:
                    return CreatePowerNetWithOnePQAndOnePVBus(out correctVoltages, out voltageCollapse);
                case ProblemSelectionEnum.ThreeNodeSystemWithUnsymmetricMatrixAndOnePQAndOnePVBus:
                    return CreatePowerNetWithUnsymmetricMatrixAndOnePQAndOnePVBus(out correctVoltages, out voltageCollapse);
                case ProblemSelectionEnum.ThreeNodeSystemWithUnsymmetricMatrixAndPVBusses:
                    return CreatePowerNetWithUnsymmetricMatrixAndPVBusses(out correctVoltages, out voltageCollapse);
                case ProblemSelectionEnum.ThreeNodeSystemWithUnsymmetricMatrixAndPQBusses:
                    return CreatePowerNetWithUnsymmetricMatrixAndPQBusses(out correctVoltages, out voltageCollapse);
            }

            throw new ArgumentOutOfRangeException();
        }

        private static PowerNetSingleVoltageLevel CreatePowerNetWithUnsymmetricMatrixAndPQBusses(out Vector<Complex> correctVoltages, out bool voltageCollapse)
        {
            var powerNet = new PowerNetSingleVoltageLevel(3, 1);
            powerNet.AddSymmetricAdmittance(0, 1, new Complex(100, 200));
            powerNet.AddSymmetricAdmittance(0, 2, new Complex(50, -100));
            powerNet.AddSymmetricAdmittance(1, 2, new Complex(200, 600));
            powerNet.AddUnsymmetricAdmittance(1, 2, new Complex(4, 1));
            powerNet.AddUnsymmetricAdmittance(2, 1, new Complex(-4, -1));
            correctVoltages = new DenseVector(new[] { new Complex(1.1, 0.12), new Complex(0.9, 0.1), new Complex(0.95, 0.05) });
            var powers = LoadFlowCalculator.CalculateAllPowers(powerNet.Admittances, correctVoltages);
            var supplyNode = new Node { Voltage = correctVoltages[0] };
            var loadNodeOne = new Node() { Power = powers[1] };
            var loadNodeTwo = new Node() { Power = powers[2] };
            powerNet.SetNode(0, supplyNode);
            powerNet.SetNode(1, loadNodeOne);
            powerNet.SetNode(2, loadNodeTwo);

            voltageCollapse = false;
            return powerNet;
        }

        private static PowerNetSingleVoltageLevel CreatePowerNetWithUnsymmetricMatrixAndPVBusses(out Vector<Complex> correctVoltages, out bool voltageCollapse)
        {
            var powerNet = new PowerNetSingleVoltageLevel(3, 1);
            powerNet.AddSymmetricAdmittance(0, 1, new Complex(100, 200));
            powerNet.AddSymmetricAdmittance(0, 2, new Complex(50, -100));
            powerNet.AddSymmetricAdmittance(1, 2, new Complex(200, 600));
            powerNet.AddUnsymmetricAdmittance(1, 2, new Complex(4, 1));
            powerNet.AddUnsymmetricAdmittance(2, 1, new Complex(-4, -1));
            correctVoltages = new DenseVector(new[] { new Complex(1.1, 0.12), new Complex(0.9, 0.1), new Complex(0.95, 0.05) });
            var powers = LoadFlowCalculator.CalculateAllPowers(powerNet.Admittances, correctVoltages);
            var supplyNode = new Node { Voltage = correctVoltages[0] };
            var loadNodeOne = new Node() { RealPower = powers[1].Real, VoltageMagnitude = correctVoltages[1].Magnitude };
            var loadNodeTwo = new Node() { RealPower = powers[2].Real, VoltageMagnitude = correctVoltages[2].Magnitude };
            powerNet.SetNode(0, supplyNode);
            powerNet.SetNode(1, loadNodeOne);
            powerNet.SetNode(2, loadNodeTwo);

            voltageCollapse = false;
            return powerNet;
        }

        private static PowerNetSingleVoltageLevel CreatePowerNetWithUnsymmetricMatrixAndOnePQAndOnePVBus(out Vector<Complex> correctVoltages, out bool voltageCollapse)
        {
            var powerNet = new PowerNetSingleVoltageLevel(3, 1);
            powerNet.AddSymmetricAdmittance(0, 1, new Complex(100, 200));
            powerNet.AddSymmetricAdmittance(0, 2, new Complex(50, -100));
            powerNet.AddSymmetricAdmittance(1, 2, new Complex(200, 600));
            powerNet.AddUnsymmetricAdmittance(1, 2, new Complex(4, 1));
            powerNet.AddUnsymmetricAdmittance(2, 1, new Complex(-4, -1));
            correctVoltages = new DenseVector(new[] { new Complex(1.1, 0.12), new Complex(0.9, 0.1), new Complex(0.95, 0.05) });
            var powers = LoadFlowCalculator.CalculateAllPowers(powerNet.Admittances, correctVoltages);
            var supplyNode = new Node { Voltage = correctVoltages[0] };
            var loadNodeOne = new Node() { RealPower = powers[1].Real, VoltageMagnitude = correctVoltages[1].Magnitude };
            var loadNodeTwo = new Node() { Power = powers[2] };
            powerNet.SetNode(0, supplyNode);
            powerNet.SetNode(1, loadNodeOne);
            powerNet.SetNode(2, loadNodeTwo);

            voltageCollapse = false;
            return powerNet;
        }

        private static PowerNetSingleVoltageLevel CreatePowerNetWithOnePQAndOnePVBus(
            out Vector<Complex> correctVoltages,
            out bool voltageCollapse)
        {
            var powerNet = new PowerNetSingleVoltageLevel(3, 1);
            powerNet.AddSymmetricAdmittance(0, 1, new Complex(1000, 500));
            powerNet.AddSymmetricAdmittance(0, 2, new Complex(200, -200));
            powerNet.AddSymmetricAdmittance(1, 2, new Complex(100, 300));
            correctVoltages = new DenseVector(3);
            correctVoltages[0] = new Complex(1, 0.2);
            correctVoltages[1] = new Complex(1.1, -0.1);
            correctVoltages[2] = new Complex(0.8, 0);
            var powers = LoadFlowCalculator.CalculateAllPowers(powerNet.Admittances, correctVoltages);
            var supplyNode = new Node { Voltage = correctVoltages[0] };
            var pvNode = new Node() { RealPower = powers[1].Real, VoltageMagnitude = correctVoltages[1].Magnitude };
            var pqNode = new Node() { Power = powers[2] };
            powerNet.SetNode(0, supplyNode);
            powerNet.SetNode(1, pvNode);
            powerNet.SetNode(2, pqNode);

            voltageCollapse = false;
            return powerNet;
        }

        private static PowerNetSingleVoltageLevel CreatePowerNetWithPQBusNearlyCollapsing(out Vector<Complex> correctVoltages,
            out bool voltageCollapse)
        {
            const double load = 0.2499999; // 0.25 is the collapse-border
            var loadVoltage = new Complex((1 + Math.Sqrt(1 - 4 * load)) / 2, 0);
            var supplyVoltage = new Complex(1, 0);
            correctVoltages = new DenseVector(new[] { supplyVoltage, loadVoltage });
            voltageCollapse = false;
            var powerNet = new PowerNetSingleVoltageLevel(2, 1);
            powerNet.AddSymmetricAdmittance(0, 1, new Complex(1, 0));
            var supplyNode = new Node() { Voltage = supplyVoltage };
            var loadNode = new Node() { Power = new Complex((-1) * load, 0) };
            powerNet.SetNode(0, supplyNode);
            powerNet.SetNode(1, loadNode);
            return powerNet;
        }

        private static PowerNetSingleVoltageLevel CreatePowerNetWithPVBusNearlyCollapsing(out Vector<Complex> correctVoltages,
            out bool voltageCollapse)
        {
            const double load = 0.2499999; // 0.25 is the collapse-border
            var loadVoltage = new Complex((1 + Math.Sqrt(1 - 4 * load)) / 2, 0);
            var supplyVoltage = new Complex(1, 0);
            correctVoltages = new DenseVector(new[] { supplyVoltage, loadVoltage });
            voltageCollapse = false;
            var powerNet = new PowerNetSingleVoltageLevel(2, 1);
            powerNet.AddSymmetricAdmittance(0, 1, new Complex(1, 0));
            var supplyNode = new Node() { Voltage = supplyVoltage };
            var loadNode = new Node() { RealPower = (-1) * load, VoltageMagnitude = loadVoltage.Magnitude };
            powerNet.SetNode(0, supplyNode);
            powerNet.SetNode(1, loadNode);
            return powerNet;
        }

        private static PowerNetSingleVoltageLevel CreatePowerNetThreeNodeSystemWithTwoPVBuses(out Vector<Complex> correctVoltages, out bool voltageCollapse)
        {
            var powerNet = new PowerNetSingleVoltageLevel(3, 1);
            powerNet.AddSymmetricAdmittance(0, 1, new Complex(1000, 500));
            powerNet.AddSymmetricAdmittance(0, 2, new Complex(0, 0));
            powerNet.AddSymmetricAdmittance(1, 2, new Complex(100, 300));
            correctVoltages = new DenseVector(3);
            correctVoltages[0] = new Complex(1, 0.2);
            correctVoltages[1] = new Complex(1.1, -0.1);
            correctVoltages[2] = new Complex(0.8, 0);
            var powers = LoadFlowCalculator.CalculateAllPowers(powerNet.Admittances, correctVoltages);

            for (var i = 1; i < 3; ++i)
            {
                var node = new Node() { RealPower = powers[i].Real, VoltageMagnitude = correctVoltages[i].Magnitude };
                powerNet.SetNode(i, node);
            }

            var supplyNode = new Node { Voltage = correctVoltages[0] };
            powerNet.SetNode(0, supplyNode);

            voltageCollapse = false;
            return powerNet;
        }

        private static PowerNetSingleVoltageLevel CreatePowerNetForFiveNodeSystemWithFourPQBuses(out Vector<Complex> correctVoltages, out bool voltageCollapse)
        {
            var powerNet = new PowerNetSingleVoltageLevel(5, 1);
            powerNet.AddSymmetricAdmittance(0, 1, new Complex(1000, 500));
            powerNet.AddSymmetricAdmittance(0, 2, new Complex(0, 0));
            powerNet.AddSymmetricAdmittance(0, 3, new Complex(200, -100));
            powerNet.AddSymmetricAdmittance(0, 4, new Complex(0, -200));
            powerNet.AddSymmetricAdmittance(1, 2, new Complex(100, 300));
            powerNet.AddSymmetricAdmittance(1, 3, new Complex(0, 0));
            powerNet.AddSymmetricAdmittance(1, 4, new Complex(0, 0));
            powerNet.AddSymmetricAdmittance(2, 3, new Complex(200, -500));
            powerNet.AddSymmetricAdmittance(2, 4, new Complex(0, 0));
            powerNet.AddSymmetricAdmittance(3, 4, new Complex(700, 500));
            correctVoltages = new DenseVector(5);
            correctVoltages[0] = new Complex(1, -0.1);
            correctVoltages[1] = new Complex(1.05, 0.1);
            correctVoltages[2] = new Complex(0.95, 0.2);
            correctVoltages[3] = new Complex(0.97, -0.15);
            correctVoltages[4] = new Complex(0.95, 0.03);
            var powers = LoadFlowCalculator.CalculateAllPowers(powerNet.Admittances, correctVoltages);

            for (var i = 1; i < 5; ++i)
            {
                var node = new Node { Power = powers[i] };
                powerNet.SetNode(i, node);
            }

            var supplyNode = new Node { Voltage = correctVoltages[0] };
            powerNet.SetNode(0, supplyNode);

            voltageCollapse = false;
            return powerNet;
        }

        private static PowerNetSingleVoltageLevel CreatePowerNetForFiveNodeSystemWithOneGroundNode(out Vector<Complex> correctVoltages, out bool voltageCollapse)
        {
            var powerNet = new PowerNetSingleVoltageLevel(5, 1);
            powerNet.AddSymmetricAdmittance(0, 1, new Complex(1000, 500));
            powerNet.AddSymmetricAdmittance(0, 2, new Complex(0, 0));
            powerNet.AddSymmetricAdmittance(0, 3, new Complex(200, -100));
            powerNet.AddSymmetricAdmittance(0, 4, new Complex(0, -200));
            powerNet.AddSymmetricAdmittance(1, 2, new Complex(100, 300));
            powerNet.AddSymmetricAdmittance(1, 3, new Complex(0, 0));
            powerNet.AddSymmetricAdmittance(1, 4, new Complex(0, 0));
            powerNet.AddSymmetricAdmittance(2, 3, new Complex(200, -500));
            powerNet.AddSymmetricAdmittance(2, 4, new Complex(0, 0));
            powerNet.AddSymmetricAdmittance(3, 4, new Complex(700, 500));
            correctVoltages = new DenseVector(new[] { new Complex(1, -0.1), new Complex(1.05, 0.1), new Complex(0.95, 0.2), new Complex(0.97, -0.15), new Complex(0, 0) });
            var powers = LoadFlowCalculator.CalculateAllPowers(powerNet.Admittances, correctVoltages);
            IList<Node> nodes = new[] { new Node(), new Node(), new Node(), new Node(), new Node() };
            nodes[0].Power = powers.At(0);
            nodes[1].Voltage = correctVoltages.At(1);
            nodes[2].Voltage = correctVoltages.At(2);
            nodes[3].Power = powers.At(3);
            nodes[4].Voltage = correctVoltages.At(4);

            for (var i = 0; i < 5; ++i)
                powerNet.SetNode(i, nodes[i]);

            voltageCollapse = false;
            return powerNet;
        }

        private static PowerNetSingleVoltageLevel CreatePowerNetForFiveNodeSystemWithThreePQBusesAndOnePVBus(out Vector<Complex> correctVoltages, out bool voltageCollapse)
        {
            var powerNet = new PowerNetSingleVoltageLevel(5, 1);
            powerNet.AddSymmetricAdmittance(0, 1, new Complex(1000, 500));
            powerNet.AddSymmetricAdmittance(0, 2, new Complex(0, 0));
            powerNet.AddSymmetricAdmittance(0, 3, new Complex(200, -100));
            powerNet.AddSymmetricAdmittance(0, 4, new Complex(0, -200));
            powerNet.AddSymmetricAdmittance(1, 2, new Complex(100, 300));
            powerNet.AddSymmetricAdmittance(1, 3, new Complex(0, 0));
            powerNet.AddSymmetricAdmittance(1, 4, new Complex(0, 0));
            powerNet.AddSymmetricAdmittance(2, 3, new Complex(200, -500));
            powerNet.AddSymmetricAdmittance(2, 4, new Complex(0, 0));
            powerNet.AddSymmetricAdmittance(3, 4, new Complex(700, 500));
            correctVoltages = new DenseVector(new[] { new Complex(1, -0.1), new Complex(1.05, 0.1), new Complex(0.95, 0.02), new Complex(0.97, -0.15), new Complex(0.8, 0.05) });
            var powers = LoadFlowCalculator.CalculateAllPowers(powerNet.Admittances, correctVoltages);
            IList<Node> nodes = new[] { new Node(), new Node(), new Node(), new Node(), new Node() };
            nodes[0].Voltage = correctVoltages.At(0);
            nodes[1].Power = powers.At(1);
            nodes[2].VoltageMagnitude = correctVoltages.At(2).Magnitude;
            nodes[2].RealPower = powers.At(2).Real;
            nodes[3].Power = powers.At(3);
            nodes[4].Power = powers.At(4);

            for (var i = 0; i < 5; ++i)
                powerNet.SetNode(i, nodes[i]);

            voltageCollapse = false;
            return powerNet;
        }

        private static PowerNetSingleVoltageLevel CreatePowerNetForStableTwoNodeSystem(out Vector<Complex> correctVoltages, out bool voltageCollapse)
        {
            var powerNet = new PowerNetSingleVoltageLevel(2, 1);
            powerNet.AddSymmetricAdmittance(0, 1, new Complex(1000, 2000));
            correctVoltages = new DenseVector(2);
            correctVoltages[0] = new Complex(1, 0);
            correctVoltages[1] = new Complex(0.95, 0.03);
            var powers = LoadFlowCalculator.CalculateAllPowers(powerNet.Admittances, correctVoltages);
            var supplyNode = new Node { Voltage = correctVoltages[0] };
            var loadNode = new Node { Power = powers[1] };
            powerNet.SetNode(0, supplyNode);
            powerNet.SetNode(1, loadNode);
            voltageCollapse = false;
            return powerNet;
        }

        public static PowerNetSingleVoltageLevel CreatePowerNetForCollapsingTwoNodeSystem(out Vector<Complex> correctVoltages, out bool voltageCollapse)
        {
            var powerNet = new PowerNetSingleVoltageLevel(2, 1);
            var supplyNode = new Node { Voltage = new Complex(1, 0) };
            var loadNode = new Node { Power = new Complex(-100, -50) };
            powerNet.SetNode(0, supplyNode);
            powerNet.SetNode(1, loadNode);
            powerNet.AddSymmetricAdmittance(0, 1, new Complex(1, 2));

            correctVoltages = new DenseVector(2);
            voltageCollapse = true;
            return powerNet;
        }

        public static PowerNetSingleVoltageLevel CreatePowerNetOnePVBus(out Vector<Complex> correctVoltages,
            out bool voltageCollapse)
        {
            var powerNet = new PowerNetSingleVoltageLevel(2, 1);
            powerNet.AddSymmetricAdmittance(0, 1, new Complex(1000, 2000));
            correctVoltages = new DenseVector(2);
            correctVoltages[0] = new Complex(1, 0);
            correctVoltages[1] = new Complex(0.95, 0.03);
            var powers = LoadFlowCalculator.CalculateAllPowers(powerNet.Admittances, correctVoltages);
            var supplyNode = new Node { Voltage = correctVoltages[0] };
            var loadNode = new Node { VoltageMagnitude = correctVoltages[1].Magnitude, RealPower = powers[1].Real };
            powerNet.SetNode(0, supplyNode);
            powerNet.SetNode(1, loadNode);
            voltageCollapse = false;
            return powerNet;
        }
        #endregion

        #region result adding
        private void AddCalculationResult(CalculationResult result)
        {
            _calculationResults.Add(result);
        }

        private void AddCombinedCalculationResult(CombinedCalculationResult result)
        {
            _combinedCalculationResults.Add(result);
        }

        private void SetVoltagesNodePotentialMethod(Vector<Complex> voltages)
        {
            for (var i = 0; i < _nodeVoltages.Count; ++i)
            {
                var copy = _nodeVoltages[i].DeepClone();
                copy.NodePotentialMethod = voltages[i];
                _nodeVoltages[i] = copy;
            }
        }

        private void SetVoltagesCurrentIteration(Vector<Complex> voltages)
        {
            for (var i = 0; i < _nodeVoltages.Count; ++i)
            {
                var copy = _nodeVoltages[i].DeepClone();
                copy.CurrentIteraion = voltages[i];
                _nodeVoltages[i] = copy;
            }
        }

        private void SetVoltagesNewtonRaphson(Vector<Complex> voltages)
        {
            for (var i = 0; i < _nodeVoltages.Count; ++i)
            {
                var copy = _nodeVoltages[i].DeepClone();
                copy.NewtonRaphson = voltages[i];
                _nodeVoltages[i] = copy;
            }
        }

        private void SetVoltagesFastDecoupledLoadFlow(Vector<Complex> voltages)
        {
            for (var i = 0; i < _nodeVoltages.Count; ++i)
            {
                var copy = _nodeVoltages[i].DeepClone();
                copy.FastDecoupledLoadFlow = voltages[i];
                _nodeVoltages[i] = copy;
            }
        }

        private void SetVoltagesHolomorphicEmbeddingLoadFlowLongDouble(Vector<Complex> voltages)
        {
            for (var i = 0; i < _nodeVoltages.Count; ++i)
            {
                var copy = _nodeVoltages[i].DeepClone();
                copy.HolomorphicEmbeddingLoadFlowLongDouble = voltages[i];
                _nodeVoltages[i] = copy;
            }
        }

        private void SetVoltagesHolomorphicEmbeddingLoadFlowMulti(Vector<Complex> voltages)
        {
            for (var i = 0; i < _nodeVoltages.Count; ++i)
            {
                var copy = _nodeVoltages[i].DeepClone();
                copy.HolomorphicEmbeddingLoadFlowMulti = voltages[i];
                _nodeVoltages[i] = copy;
            }
        }
        #endregion

        #region result calculation
        public void Calculate()
        {
            _generalSettings.CalculationRunning = true;
            _combinedCalculationResults.Clear();
            _calculationResults.Clear();
            Vector<Complex> correctVoltages;
            bool voltageCollapse;
            var powerNet = CreatePowerNet(out correctVoltages, out voltageCollapse);
            var nodeCount = powerNet.NodeCount;
            _nodeVoltages.Clear();

            for (var i = 0; i < nodeCount; ++i)
            {
                var line = new NodeVoltage { NodeName = "node " + i, Correct = correctVoltages[i], Known = powerNet.IsVoltageOfNodeKnown(i) };
                _nodeVoltages.Add(line);
            }

            var calculationTask = Task.Factory.StartNew(() =>
            {
                CalculateNodePotentialResult();
                CalculateCurrentIterationResult();
                CalculateNewtonRaphsonResult();
                CalculateFastDecoupledLoadFlowResult();
                CalculateHolomorphicEmbeddingLoadFlowResult();
                CalculateHolomorphicEmbeddingLoadFlowHighAccuracyResult();
            });

            calculationTask.ContinueWith(t => CalculationFinished(), CancellationToken.None,
                TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void CalculationFinished()
        {
            _generalSettings.CalculationRunning = false;
        }

        private void CalculateNodePotentialResult()
        {
            var calculator = new NodePotentialMethod();
            var voltages = CalculateResult(calculator, "Node Potential");
            _mainDispatcher.Invoke(new SetVoltages(SetVoltagesNodePotentialMethod), voltages);
        }

        private void CalculateCurrentIterationResult()
        {
            var calculator = new CurrentIteration(_currentIteration.TargetPrecision,
                _currentIteration.MaximumIterations);
            var voltages = CalculateResult(calculator, "Current Iteration");
            _mainDispatcher.Invoke(new SetVoltages(SetVoltagesCurrentIteration), voltages);
        }

        private void CalculateNewtonRaphsonResult()
        {
            var calculator = new NewtonRaphsonMethod(_newtonRaphson.TargetPrecision,
                _newtonRaphson.MaximumIterations);
            var voltages = CalculateResult(calculator, "Newton Raphson");
            _mainDispatcher.Invoke(new SetVoltages(SetVoltagesNewtonRaphson), voltages);
        }

        private void CalculateFastDecoupledLoadFlowResult()
        {
            var calculator = new FastDecoupledLoadFlowMethod(_fastDecoupledLoadFlow.TargetPrecision,
                _fastDecoupledLoadFlow.MaximumIterations);
            var voltages = CalculateResult(calculator, "FDLF");
            _mainDispatcher.Invoke(new SetVoltages(SetVoltagesFastDecoupledLoadFlow), voltages);
        }

        private void CalculateHolomorphicEmbeddingLoadFlowResult()
        {
            var calculator = new HolomorphicEmbeddedLoadFlowMethod(_holomorphicEmbeddedLoadFlow.TargetPrecision,
                _holomorphicEmbeddedLoadFlow.MaximumNumberOfCoefficients, new PrecisionLongDouble(), true);
            var voltages = CalculateResult(calculator, "HELM - double");
            _mainDispatcher.Invoke(new SetVoltages(SetVoltagesHolomorphicEmbeddingLoadFlowLongDouble), voltages);
        }

        private void CalculateHolomorphicEmbeddingLoadFlowHighAccuracyResult()
        {
            var calculator = new HolomorphicEmbeddedLoadFlowMethod(_holomorphicEmbeddedLoadFlowHighAccuracy.TargetPrecision,
                _holomorphicEmbeddedLoadFlowHighAccuracy.MaximumNumberOfCoefficients, new PrecisionMulti(_holomorphicEmbeddedLoadFlowHighAccuracy.BitPrecision), true);
            var voltages = CalculateResult(calculator, "HELM - multi");
            _mainDispatcher.Invoke(new SetVoltages(SetVoltagesHolomorphicEmbeddingLoadFlowMulti), voltages);
        }

        private Vector<Complex> CalculateResult(INodeVoltageCalculator nodeVoltageCalculator, string algorithmName)
        {
            var numberOfExecutions = _generalSettings.NumberOfExecutions;
            var executionTimes = new List<double>(numberOfExecutions);
            var stopWatch = new Stopwatch();
            PowerNetSingleVoltageLevel powerNet;
            bool voltageCollapseDetected;
            Vector<Complex> correctVoltages;
            bool voltageCollapseReal;
            var i = 0;
            Vector<Complex> voltageError;
            var loadFlowCalculator = new LoadFlowCalculator(nodeVoltageCalculator);

            do
            {
                powerNet = CreatePowerNet(out correctVoltages, out voltageCollapseReal);

                stopWatch.Restart();
                voltageCollapseDetected = powerNet.CalculateMissingInformation(loadFlowCalculator);
                stopWatch.Stop();
                executionTimes.Add(stopWatch.Elapsed.TotalSeconds);

                var result = new CalculationResult();
                voltageError = correctVoltages - powerNet.NodeVoltages;
                result.Algorithm = algorithmName;
                result.MaximumRelativeVoltageError = voltageError.AbsoluteMaximum().Magnitude / powerNet.NominalVoltage;
                result.VoltageCollapseDetected = voltageCollapseDetected;
                result.VoltageCollapse = voltageCollapseReal;
                result.ExecutionTime = stopWatch.Elapsed.TotalSeconds;
                result.RelativePowerError = powerNet.RelativePowerError;

                ++i;
                _mainDispatcher.Invoke(new ResultCalculated(AddCalculationResult), result);
            } while (i < numberOfExecutions);

            var combinedResult = new CombinedCalculationResult();
            var statistics = new DescriptiveStatistics(executionTimes);
            voltageError = correctVoltages - powerNet.NodeVoltages;
            combinedResult.VoltageCollapseDetected = voltageCollapseDetected;
            combinedResult.VoltageCollapse = voltageCollapseReal;
            combinedResult.AverageExecutionTime = statistics.Mean;
            combinedResult.StandardDeviationExecutionTime = statistics.StandardDeviation;
            combinedResult.RelativePowerError = powerNet.RelativePowerError;
            combinedResult.MaximumRelativeVoltageError = voltageError.AbsoluteMaximum().Magnitude / powerNet.NominalVoltage;
            combinedResult.Algorithm = algorithmName;

            _mainDispatcher.Invoke(new CombinedResultCalculated(AddCombinedCalculationResult), combinedResult);
            return powerNet.NodeVoltages;
        }
        #endregion
    }
}
