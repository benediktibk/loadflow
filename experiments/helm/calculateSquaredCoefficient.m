function [squaredCoefficient] = calculateSquaredCoefficient(coefficients)
    n = size(coefficients, 1);    
    squaredCoefficient = 0;
    
    for i = 1:n
        squaredCoefficient = squaredCoefficient + coefficients(i)*coefficients(n - i + 1);
    end
end

