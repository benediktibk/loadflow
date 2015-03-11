function [voltages] = helm(admittances, slackIndices, slackVoltages, rightHandSideFunction, coefficientCount)
    [admittancesReduced, constantCurrents] = reduceAdmittanceMatrix(admittances, slackIndices, slackVoltages);
    n = size(admittancesReduced, 1);
    coefficients = zeros(coefficientCount, n);
    inverseCoefficients = zeros(coefficientCount, n);
    squaredCoefficients = zeros(coefficientCount, n);
    admittanceWeightedCoefficients = zeros(coefficientCount, n);
    rowAdmittanceSums = sum(admittancesReduced, 2);
    
    for i = 1:coefficientCount
        rightHandSide = rightHandSideFunction(constantCurrents, rowAdmittanceSums, coefficients(1:(i - 1), :), inverseCoefficients(1:(i - 1), :), squaredCoefficients(1:(i - 1), :), admittanceWeightedCoefficients(1:(i - 1), :));
        newCoefficients = admittancesReduced\rightHandSide;
        coefficients(i, :) = newCoefficients';        
    
        for j = 1:n
            inverseCoefficients(i, j) = calculateInverseCoefficient(coefficients(1:i, j), inverseCoefficients(1:(i - 1), j));
            squaredCoefficients(i, j) = calculateSquaredCoefficient(coefficients(1:i, j));
            admittanceWeightedCoefficients(i, j) = calculateAdmittanceWeightedCoefficient(admittancesReduced, coefficients(1:i, :), squaredCoefficients(1:i, :), j);
        end
    end
    
    voltages = zeros(n, 1);
    
    for i = 1:n
        voltages(i) = wynnEpsilon(cumsum(coefficients(:, i)));
    end
end

