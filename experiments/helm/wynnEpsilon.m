function [y] = wynnEpsilon(x)
    current = x;
    n = size(current, 1);
    previous = zeros(n + 1, 1);
    
    for i = 1:(n - 1)
        next = zeros(n - i, 1);
        
        for j = 1:(n - i)
            next(j) = previous(j + 1) + 1/(current(j + 1) - current(j));
        end
        
        previous = current;
        current = next;
    end
    
    if mod(n, 2) == 0
        y = previous(2);
    else
        y = current(1);
    end
end

