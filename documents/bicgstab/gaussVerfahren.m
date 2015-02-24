function x = gaussVerfahren(A, b)
    [L, U, P] = luDecomposition(A);
    [y] = vorwaertsSubstitution(L, P, b);
    [x] = rueckwaertsSubstitution(U, y);
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
