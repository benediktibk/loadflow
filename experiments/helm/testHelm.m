function [okay] = testHelm()
    okay = testHelmOnePqNode();
    okay = okay & testHelmOnePvNode();
    okay = okay & testHelmPowerControl();
end

