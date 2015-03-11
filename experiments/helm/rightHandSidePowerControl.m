function [result] = rightHandSidePowerControl(constantCurrent, rowAdmittanceSum, inverseCoefficientsCurrent, squaredCoefficientsOther, coefficientsOther, a, b, c)
    n = size(inverseCoefficientsCurrent, 1);
    
    if n == 0
        result = rowAdmittanceSum;
        return;
    end
    
    ax = convoluteNextCoefficient(squaredCoefficientsOther, inverseCoefficientsCurrent);
    bx = convoluteNextCoefficient(coefficientsOther, inverseCoefficientsCurrent);
    cx = inverseCoefficientsCurrent(n);
    
    result = conj(a*ax + b*bx + c*cx);
    
    if n == 1
        result = result + constantCurrent - rowAdmittanceSum;
    end
end

