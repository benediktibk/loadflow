function [okay] = testHelmPowerControl()
    admittances = [1 -1 0; -1 2 -1; 0 -1 1];
    slackIndices = 1;
    slackVoltages = 1;
    voltages = helm(admittances, slackIndices, slackVoltages, @rightHandSide, 50);
    okay = norm(voltages - [0.95; 0.85]) < 1e-8;
end

function [result] = rightHandSide(constantCurrents, rowAdmittanceSums, coefficients, inverseCoefficients, squaredCoefficients, admittanceWeightedCoefficients)
    a = 3.6666666666667;
    b = -7.1;
    c = 3.4333333333333;
    resultOne = rightHandSidePowerControl(constantCurrents(1), rowAdmittanceSums(1), inverseCoefficients(:, 1), squaredCoefficients(:, 2), coefficients(:, 2), a, b, c);
    resultTwo = rightHandSidePq(constantCurrents(2), rowAdmittanceSums(2), coefficients(:, 2), inverseCoefficients(:, 2), -0.085);
    result = [resultOne; resultTwo];
end