function [squaredCoefficient] = calculateSquaredCoefficient(coefficients)
    squaredCoefficient = convoluteNextCoefficient(coefficients, coefficients);
end

