function [L, U, P] = luDecomposition(A)
    n = size(A, 1);
    L = sparse(n, n);
    P = speye(n);

    for i = 1:n-1
        pivotIndex = i;
        pivot = A(pivotIndex, i);
        Psub = speye(n);
        Psub(:, [i, pivotIndex]) = Psub(:, [pivotIndex, i]);
        A([i, pivotIndex], :) = A([pivotIndex, i], :);
        L([i, pivotIndex], :) = L([pivotIndex, i], :);
        P = Psub*P;
        pivotRow = A(i, i+1:n);
        for j = i+1:n
            factor = A(j, i)/pivot;
            L(j, i) = factor;        
            currentRow = A(j, i+1:n);
            A(j, i+1:n) = currentRow - factor*pivotRow;
            A(j, i) = 0;
        end
    end

    U = A;
    L = L + speye(n);
end

