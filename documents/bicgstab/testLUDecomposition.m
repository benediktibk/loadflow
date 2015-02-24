function [correct, matlabTimesFaster] = testLUDecomposition(A, xShouldBe)
    b = A*xShouldBe;
    
    tic;
    xGaussMethod = gaussMethod(A, b);
    timeGaussVerfahren = toc;
    
    tic;
    [L, U, P] = lu(A);
    y = L\(P*b);
    xMatlab = U\y;
    timeMatlab = toc;

    errorGaussVerfahren = norm(xGaussMethod - xShouldBe);
    correct = errorGaussVerfahren < 0.00001;
    matlabTimesFaster = timeGaussVerfahren/timeMatlab;
end

