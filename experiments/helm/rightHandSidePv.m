function [result] = rightHandSidePv(constantCurrent, rowAdmittanceSum, coefficients, squaredCoefficients, admittanceWeightedCoefficients, realPower, voltageMagnitude)
    n = size(coefficients, 1);
    
    if n == 0
        result = rowAdmittanceSum + constantCurrent;
        return;
    end
    
    voltageMagnitudeSquared = voltageMagnitude*voltageMagnitude;    
    result = (2*realPower*coefficients(n) - admittanceWeightedCoefficients(n) + conj(constantCurrent)*squaredCoefficients(n))/voltageMagnitudeSquared - coefficients(n);
    
    if n == 1
        result = result - rowAdmittanceSum;
    end
end

