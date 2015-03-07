function [inverseCoefficient] = calculateInverseCoefficient(coefficients, inverseCoefficients)
    n = size(coefficients, 1);
    
    if n == 1
        inverseCoefficient = 1/coefficients(1);
        return;
    end
    
    inverseCoefficient = 0;
    
    for i = 1:(n - 1)
            inverseCoefficient = inverseCoefficient - inverseCoefficients(i)*coefficients(n - i + 1);
    end
    
    inverseCoefficient = inverseCoefficient/conj(coefficients(1));
end

