function [okay] = testHelmOnePqNode()
    admittances = [1 -1; -1 1];
    slackIndices = 1;
    slackVoltages = 1;
    voltages = helm(admittances, slackIndices, slackVoltages, @rightHandSide, 50);
    okay = norm(voltages - 0.641421356) < 1e-8;
end

function [result] = rightHandSide(constantCurrents, rowAdmittanceSums, coefficients, inverseCoefficients)
    result = rightHandSidePq(constantCurrents(1), rowAdmittanceSums(1), coefficients(:, 1), inverseCoefficients(:, 1), -0.23);
end

