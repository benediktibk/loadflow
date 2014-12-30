function x = gaussVerfahren(A, b)
    [L, U, P] = LUZerlegung(A);
    [y] = vorwaertsSubstitution(L, P, b);
    [x] = rueckwaertsSubstitution(U, y);
end

function [L, U, P] = LUZerlegung(A)
    n = size(A, 1);
    L = zeros(n, n);
    P = eye(n);

    for i = 1:n-1
        [pivot, pivotIndex] = max(abs(A(i:n, i)));
        pivotIndex = pivotIndex + (i - 1);
        pivot = A(pivotIndex, i);
        A([i, pivotIndex], :) = A([pivotIndex, i], :);
        L([i, pivotIndex], :) = L([pivotIndex, i], :);
        P([i, pivotIndex], :) = P([pivotIndex, i], :);
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
    L = L + eye(n);
end

function [y] = vorwaertsSubstitution(L, P, b)
    n = size(L, 1);
    y = zeros(n, 1);
    b = P*b;
    y(1) = b(1)/L(1, 1);

    for i = 2:n
        rowSum = L(i, 1:i-1)*y(1:i-1);
        y(i) = (b(i) - rowSum)/L(i, i);
    end
end

function [x] = rueckwaertsSubstitution(U, y)
    n = size(U, 1);
    x = zeros(n, 1);
    x(n) = y(n)/U(n, n);

    for i = n-1:-1:1
        rowSum = U(i, i+1:n)*x(i+1:n);
        x(i) = (y(i) - rowSum)/U(i, i);
    end
end
