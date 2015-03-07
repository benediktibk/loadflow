function [result] = rightHandSidePq(constantCurrent, rowAdmittanceSum, coefficients, inverseCoefficients, power)
    n = size(coefficients, 1);
    
    if n == 0
        result = rowAdmittanceSum;
    elseif n == 1
        result = constantCurrent + conj(power)*inverseCoefficients(1) - rowAdmittanceSum;
    else
        result = conj(power)*inverseCoefficients(n);
    end
end

