
using System.Collections.Generic;
using System.Data;

namespace LoadFlowCalculationComparison.AlgorithmSettings
{
    enum ProblemSelectionEnum
    {
        CollapsingTwoNodeSystem,
        StableTwoNodeSystem,
        FiveNodeSystemWithFourPQBuses,
        FiveNodeSystemWithThreePQBusesAndOnePVBus
    }

    class GeneralSettings : NotifyPropertyChanged
    {
        private ProblemSelectionEnum _problemSelection = ProblemSelectionEnum.CollapsingTwoNodeSystem;

        public ProblemSelectionEnum ProblemSelection
        {
            get { return _problemSelection; }
            set
            {
                if (value == _problemSelection)
                    return;

                _problemSelection = value;
                OnPropertyChanged();
            }
        }
    }
}
