function [voltages] = helm(admittances, slackIndices, slackVoltages, rightHandSideFunction, coefficientCount)
    [admittancesReduced, constantCurrents] = reduceAdmittanceMatrix(admittances, slackIndices, slackVoltages);
    n = size(admittancesReduced, 1);
    coefficients = zeros(coefficientCount, n);
    inverseCoefficients = zeros(coefficientCount, n);
    rowAdmittanceSums = sum(admittancesReduced, 2);
    
    for i = 1:coefficientCount
        rightHandSide = rightHandSideFunction(constantCurrents, rowAdmittanceSums, coefficients(1:(i - 1), :), inverseCoefficients(1:(i - 1), :));
        newCoefficients = admittancesReduced\rightHandSide;
        coefficients(i, :) = newCoefficients';        
    
        for j = 1:n
            inverseCoefficients(i, j) = calculateInverseCoefficient(coefficients(1:i, j), inverseCoefficients(1:(i - 1), j));
        end
    end
    
    voltages = zeros(1:n, 1);
    
    for i = 1:n
        voltages(i) = wynnEpsilon(cumsum(coefficients(:, i)));
    end
end

