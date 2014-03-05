
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
        private int _numberOfExecutions = 5;

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

        public int NumberOfExecutions
        {
            get { return _numberOfExecutions; }
            set
            {
                if (value == _numberOfExecutions)
                    return;

                _numberOfExecutions = value;
                OnPropertyChanged();
            }
        }
    }
}
