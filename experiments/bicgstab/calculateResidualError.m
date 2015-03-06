function [error] = calculateResidualError(A, xEstimate, b)
    residual = A*xEstimate - b;
    error = abs(norm(residual))/abs(norm(b));
end

