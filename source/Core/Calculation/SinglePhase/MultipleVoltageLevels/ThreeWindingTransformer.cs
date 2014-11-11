using System;
using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using Misc;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class ThreeWindingTransformer : IPowerNetElement
    {
        private readonly IExternalReadOnlyNode _nodeOne;
        private readonly IExternalReadOnlyNode _nodeTwo;
        private readonly IExternalReadOnlyNode _nodeThree;
        private readonly Complex _shuntAdmittance;
        private readonly Complex _lengthAdmittanceOne;
        private readonly Complex _lengthAdmittanceTwo;
        private readonly Complex _lengthAdmittanceThree;
        private readonly Angle _nominalPhaseShiftOneToTwo;
        private readonly Angle _nominalPhaseShiftTwoToThree;
        private readonly Angle _nominalPhaseShiftThreeToOne;
        private readonly DerivedInternalPQNode _internalNode;

        public ThreeWindingTransformer(
            IExternalReadOnlyNode nodeOne, IExternalReadOnlyNode nodeTwo, IExternalReadOnlyNode nodeThree,
            double nominalPowerOneToTwo, double nominalPowerTwoToThree, double nominalPowerThreeToOne,
            double relativeShortCircuitVoltageOneToTwo, double relativeShortCircuitVoltageTwoToThree,
            double relativeShortCircuitVoltageThreeToOne,
            double copperLossesOneToTwo, double copperLossesTwoToThree, double copperLossesThreeToOne,
            double ironLosses, double relativeNoLoadCurrent,
            Angle nominalPhaseShiftOneToTwo, Angle nominalPhaseShiftTwoToThree, Angle nominalPhaseShiftThreeToOne,
            string name, IdGenerator idGenerator)
        {
            _nodeOne = nodeOne;
            _nodeTwo = nodeTwo;
            _nodeThree = nodeThree;
            _nominalPhaseShiftOneToTwo = nominalPhaseShiftOneToTwo;
            _nominalPhaseShiftTwoToThree = nominalPhaseShiftTwoToThree;
            _nominalPhaseShiftThreeToOne = nominalPhaseShiftThreeToOne;
            _internalNode = new DerivedInternalPQNode(nodeTwo, idGenerator.Generate(), new Complex(), name + "#internal");
            CalculateAdmittances(nominalPowerOneToTwo, nominalPowerTwoToThree, nominalPowerThreeToOne,
                relativeShortCircuitVoltageOneToTwo, relativeShortCircuitVoltageTwoToThree,
                relativeShortCircuitVoltageThreeToOne, copperLossesOneToTwo, copperLossesTwoToThree,
                copperLossesThreeToOne, ironLosses, relativeNoLoadCurrent, out _shuntAdmittance,
                out _lengthAdmittanceOne, out _lengthAdmittanceTwo, out _lengthAdmittanceThree);
        }

        public IExternalReadOnlyNode NodeOne
        {
            get { return _nodeOne; }
        }

        public IExternalReadOnlyNode NodeTwo
        {
            get { return _nodeTwo; }
        }

        public IExternalReadOnlyNode NodeThree
        {
            get { return _nodeThree; }
        }

        public Angle NominalPhaseShiftOneToTwo
        {
            get { return _nominalPhaseShiftOneToTwo; }
        }

        public Angle NominalPhaseShiftTwoToThree
        {
            get { return _nominalPhaseShiftTwoToThree; }
        }

        public Angle NominalPhaseShiftThreeToOne
        {
            get { return _nominalPhaseShiftThreeToOne; }
        }

        public void AddConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            _nodeOne.AddConnectedNodes(visitedNodes);
            _nodeTwo.AddConnectedNodes(visitedNodes);
            _nodeThree.AddConnectedNodes(visitedNodes);
        }

        public void AddConnectedNodesOnSameVoltageLevel(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            if (visitedNodes.Contains(_nodeOne))
                _nodeOne.AddConnectedNodesOnSameVoltageLevel(visitedNodes);
            else if (visitedNodes.Contains(_nodeTwo))
                _nodeTwo.AddConnectedNodesOnSameVoltageLevel(visitedNodes);
            else if (visitedNodes.Contains(_nodeThree))
                _nodeThree.AddConnectedNodesOnSameVoltageLevel(visitedNodes);
            else
                throw new ArgumentException("none of the nodes have been visited yet");
        }

        public INode CreateSingleVoltageNode(double scaleBasePower)
        {
            return new PqNode(new Complex());
        }

        public IList<IReadOnlyNode> GetInternalNodes()
        {
            return new IReadOnlyNode[] { _internalNode };
        }

        public void FillInAdmittances(IAdmittanceMatrix admittances, double scaleBasisPower, IReadOnlyNode groundNode,
            double expectedLoadFlow)
        {
            var scaler = new DimensionScaler(_nodeOne.NominalVoltage, scaleBasisPower);
            var shuntAdmittanceScaled = scaler.ScaleAdmittance(_shuntAdmittance);
            var lengthAdmittanceOneScaled = scaler.ScaleAdmittance(_lengthAdmittanceOne);
            var lengthAdmittanceTwoScaled = scaler.ScaleAdmittance(_lengthAdmittanceTwo);
            var lengthAdmittanceThreeScaled = scaler.ScaleAdmittance(_lengthAdmittanceThree);
            admittances.AddConnection(_nodeOne, groundNode, shuntAdmittanceScaled);
            admittances.AddConnection(_internalNode, groundNode, shuntAdmittanceScaled);
            admittances.AddConnection(_nodeOne, _internalNode, lengthAdmittanceOneScaled);
            admittances.AddConnection(_nodeTwo, _internalNode, lengthAdmittanceTwoScaled);
            admittances.AddConnection(_nodeThree, _internalNode, lengthAdmittanceThreeScaled);
        }

        public bool NominalVoltagesMatch
        {
            get { return true; }
        }

        public bool NeedsGroundNode
        {
            get { return true; }
        }

        private void CalculateAdmittances(double nominalPowerOneToTwo, double nominalPowerTwoToThree, double nominalPowerThreeToOne,
            double relativeShortCircuitVoltageOneToTwo, double relativeShortCircuitVoltageTwoToThree,
            double relativeShortCircuitVoltageThreeToOne,
            double copperLossesOneToTwo, double copperLossesTwoToThree, double copperLossesThreeToOne,
            double ironLosses, double relativeNoLoadCurrent,
            out Complex shuntAdmittance, out Complex lengthAdmittanceOne, out Complex lengthAdmittanceTwo,
            out Complex lengthAdmittanceThree)
        {
            var nominalVoltage = _nodeOne.NominalVoltage;
            var idleLosses = relativeNoLoadCurrent * (nominalPowerOneToTwo + nominalPowerTwoToThree + nominalPowerThreeToOne);

            if (ironLosses > idleLosses)
                throw new ArgumentException("the iron losses are too high compared to the relative no load current");

            var lengthImpedanceOneToTwo = CalculateLengthImpedance(nominalPowerOneToTwo, relativeShortCircuitVoltageOneToTwo, copperLossesOneToTwo, nominalVoltage);
            var lengthImpedanceTwoToThree = CalculateLengthImpedance(nominalPowerTwoToThree, relativeShortCircuitVoltageTwoToThree, copperLossesTwoToThree, nominalVoltage);
            var lengthImpedanceThreeToOne = CalculateLengthImpedance(nominalPowerThreeToOne, relativeShortCircuitVoltageThreeToOne, copperLossesThreeToOne, nominalVoltage);
            lengthAdmittanceOne = 2 / (lengthImpedanceOneToTwo + lengthImpedanceThreeToOne - lengthImpedanceTwoToThree);
            lengthAdmittanceTwo = 2 / (lengthImpedanceOneToTwo - lengthImpedanceThreeToOne + lengthImpedanceTwoToThree);
            lengthAdmittanceThree = 2 / (lengthImpedanceThreeToOne - lengthImpedanceOneToTwo + lengthImpedanceTwoToThree);
            shuntAdmittance = new Complex(ironLosses, (-1)*Math.Sqrt(idleLosses*idleLosses - ironLosses*ironLosses))/
                              (2*nominalVoltage*nominalVoltage);
        }

        private static Complex CalculateLengthImpedance(double nominalPower, double relativeShortCircuitVoltage,
            double copperLosses, double nominalVoltage)
        {
            var relativeShortCircuitVoltageComplex = CalculateRelativeShortCircuitVoltage(nominalPower,
                relativeShortCircuitVoltage, copperLosses);
            return (nominalVoltage*nominalVoltage/nominalPower)*
                                   relativeShortCircuitVoltageComplex;
        }

        private static Complex CalculateRelativeShortCircuitVoltage(double nominalPower, double relativeShortCircuitVoltage,
            double copperLosses)
        {
            var relativeShortCircuitVoltageReal = copperLosses/nominalPower;

            if (relativeShortCircuitVoltageReal > relativeShortCircuitVoltage)
                throw new ArgumentException("the copper losses are too high compared to the nominal power");

            var relativeShortCircuitVoltageImaginary =
                Math.Sqrt(relativeShortCircuitVoltage*relativeShortCircuitVoltage -
                          relativeShortCircuitVoltageReal*relativeShortCircuitVoltageReal);
            return new Complex(relativeShortCircuitVoltageReal,
                relativeShortCircuitVoltageImaginary);
        }
    }
}
