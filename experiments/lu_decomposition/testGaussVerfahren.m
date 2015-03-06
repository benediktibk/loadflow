function testGaussVerfahren()
    rng(0);
    
    A = [2, 1, 1; 4, 3, 3; 8, 7, 9];
    xShouldBe = [3, 2, 5]';
    executeTest(A, xShouldBe);
    
    A = [0, 3, 3; -1, 3, 4; -2, 1, 5];
    xShouldBe = [3, 2, 5]';
    executeTest(A, xShouldBe);
    
    A = [1, 1, 1, 1; 2, 3, -1, -3; 4, -1, 1, 1; -1, -2, -3, 1];
    xShouldBe = [3, 4, 1, -2]';
    executeTest(A, xShouldBe);
    
    A = rand(4);
    xShouldBe = [3, 4, 1, -2]';
    executeTest(A, xShouldBe);
    
    A = rand(10);
    xShouldBe = [3, 4, 1, -2, 3, -5, -2, -3, 5, 10]';
    executeTest(A, xShouldBe);
    
    A = rand(100);
    xShouldBe = rand(100, 1);
    executeTest(A, xShouldBe);
    
    A = rand(100);
    xShouldBe = rand(100, 1);
    executeTest(A, xShouldBe);
end

function executeTest(A, xShouldBe)
    b = A*xShouldBe;
    
    tic;
    xGaussVerfahren = gaussVerfahren(A, b);
    timeGaussVerfahren = toc;
    
    tic;
    [L, U, P] = lu(A);
    y = L\(P*b);
    xMatlab = U\y;
    timeMatlab = toc;

    errorGaussVerfahren = norm(xGaussVerfahren - xShouldBe);
    if (errorGaussVerfahren < 0.00001)
        display('solution of custom algorithm is correct');
    else
        display('solution of custom algorithm is not correct');
        display(xShouldBe);
        display(xGaussVerfahren);
    end

    errorMatlab = norm(xMatlab - xShouldBe);
    if (errorMatlab < 0.00001)
        display('solution of matlab is correct');
    else
        display('solution of matlab is not correct');
        display(xShouldBe);
        display(xMatlab);
    end
    
    display(strcat('custom algorithm:',num2str(timeGaussVerfahren),' s'));
    display(strcat('matlab:',num2str(timeMatlab),' s'));
    display(strcat('matlab is times faster:', num2str(timeGaussVerfahren/timeMatlab)));
end
