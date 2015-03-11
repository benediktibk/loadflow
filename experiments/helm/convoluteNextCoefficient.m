function [result] = convoluteNextCoefficient(a, b)
    n = size(a, 1);    
    result = 0;
    
    for i = 1:n
        result = result + a(i)*b(n - i + 1);
    end
end

