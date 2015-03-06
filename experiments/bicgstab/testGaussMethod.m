rng(0);
correctTotal = 0;
wrongTotal = 0;

A = [2, 1, 1; 4, 3, 3; 8, 7, 9];
xShouldBe = [3, 2, 5]';
[correct,] = testLUDecomposition(A, xShouldBe);

if (correct)
    correctTotal = correctTotal + 1;
else
    wrongTotal = wrongTotal + 1;
end

A = [1, 1, 1, 1; 2, 3, -1, -3; 4, -1, 1, 1; -1, -2, -3, 1];
xShouldBe = [3, 4, 1, -2]';
[correct,] = testLUDecomposition(A, xShouldBe);

if (correct)
    correctTotal = correctTotal + 1;
else
    wrongTotal = wrongTotal + 1;
end

A = rand(4) + eye(4);
xShouldBe = [3, 4, 1, -2]';
[correct,] = testLUDecomposition(A, xShouldBe);

if (correct)
    correctTotal = correctTotal + 1;
else
    wrongTotal = wrongTotal + 1;
end

A = rand(10) + eye(10);
xShouldBe = [3, 4, 1, -2, 3, -5, -2, -3, 5, 10]';
[correct,] = testLUDecomposition(A, xShouldBe);

if (correct)
    correctTotal = correctTotal + 1;
else
    wrongTotal = wrongTotal + 1;
end

A = rand(100) + eye(100);
xShouldBe = rand(100, 1);
[correct,] = testLUDecomposition(A, xShouldBe);

if (correct)
    correctTotal = correctTotal + 1;
else
    wrongTotal = wrongTotal + 1;
end

A = rand(100) + eye(100);
xShouldBe = rand(100, 1);
[correct,] = testLUDecomposition(A, xShouldBe);

if (correct)
    correctTotal = correctTotal + 1;
else
    wrongTotal = wrongTotal + 1;
end

n = 100;
A = sprandsym(n, 2e-4) + eye(n);
xShouldBe = rand(n, 1);
[correct, timesFaster] = testLUDecomposition(A, xShouldBe);

display(strcat('matlab is times faster:',num2str(timesFaster)));

if (correct)
    correctTotal = correctTotal + 1;
else
    wrongTotal = wrongTotal + 1;
end

disp(strcat('correct:',num2str(correctTotal)));
disp(strcat('wrong:',num2str(wrongTotal)));
