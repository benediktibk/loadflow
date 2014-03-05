
using System.Collections.Generic;
using System.Data;

namespace LoadFlowCalculationComparison.AlgorithmSettings
{
    enum ProblemSelectionEnum
    {
        CollapsingTwoNodeSystem,
        StableTwoNodeSystem,
        FiveNodeSystemWithFourPQBuses,
        FiveNodeSystemWithOneGroundNode,
        FiveNodeSystemWithThreePQBusesAndOnePVBus
    }

    class GeneralSettings : NotifyPropertyChanged
    {
        private ProblemSelectionEnum _problemSelection = ProblemSelectionEnum.CollapsingTwoNodeSystem;
        private int _numberOfExecutions = 10;
        private bool _calculationRunning = false;

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

        public bool CalculationRunning
        {
            get { return _calculationRunning; }
            set
            {
                if (value == _calculationRunning)
                    return;

                _calculationRunning = value;
                OnPropertyChanged();
                OnPropertyChanged("ChangesAllowed");
            }
        }

        public bool ChangesAllowed
        {
            get { return !_calculationRunning; }
        }
    }
}
