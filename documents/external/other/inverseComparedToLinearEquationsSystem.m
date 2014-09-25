function [ timeForLinearEquationsSystemSolving, timeForInversion] = inverseComparedToLinearEquationsSystem( dimension )
Y = rand(dimension, dimension);
b = rand(dimension, 1);

tic;
x = Y\b;
timeForLinearEquationsSystemSolving = toc;
display(['time to solve the linear equation system is ', num2str(timeForLinearEquationsSystemSolving)]);

tic;
Yinverted = inv(Y);
timeForInversion = toc;
display(['time to calculate the inverse is ', num2str(timeForInversion)]);

display(['solving the linear equation system is ', num2str(timeForInversion/timeForLinearEquationsSystemSolving), ' times faster']);