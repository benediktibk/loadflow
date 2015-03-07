function [result] = calculateAdmittanceWeightedCoefficient(admittances, coefficients, squaredCoefficients, k)
    n = size(admittances, 1);
    m = size(coefficients, 1);
    result = 0;
    
    for i = 1:n
        partialResult = 0;
        
        if i ~= k
            for l = 1:m
                partialResult = partialResult + squaredCoefficients(l, i)*coefficients(m - l + 1, i);
            end
        end
        
        result = result + conj(admittances(k, i))*partialResult;
    end
end

