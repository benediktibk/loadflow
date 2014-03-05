
namespace LoadFlowCalculationComparison.AlgorithmSettings
{
    enum ProblemSelectionEnum
    {
        CollapsingTwoNodeSystem,
        StableTwoNodeSystem,
        FiveNodeSystemWithFourPQBuses,
        FiveNodeSystemWithThreePQBusesAndOnePVBus
    }

    class ProblemSelection : NotifyPropertyChanged
    {
        private ProblemSelectionEnum _value = ProblemSelectionEnum.CollapsingTwoNodeSystem;

        public ProblemSelectionEnum Value
        {
            get { return _value; }
            set
            {
                if (value == _value)
                    return;

                _value = value;
                OnPropertyChanged();
            }
        }
    }
}
