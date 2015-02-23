%% Initalization
n = 15025;
A = loadMatrix('matrix.csv', n);
bCurrentIteration = loadVector('vector_currentiteration.csv');
bHELM = loadVector('vector_helm.csv');

%% Backslash
x = A\bCurrentIteration;
error = calculateResidualError(A, x, bCurrentIteration);
disp(strcat('backslash operator, current iteration:',num2str(error)));

x = A\bHELM;
error = calculateResidualError(A, x, bHELM);
disp(strcat('backslash operator, HELM:',num2str(error)));

%% BiCGSTAB
x = bicgstab(A, bCurrentIteration, 1e-5, 1000);
error = calculateResidualError(A, x, bCurrentIteration);
disp(strcat('BiCGSTAB, current iteration:',num2str(error)));

x = bicgstab(A, bHELM, 1e-5, 1000);
error = calculateResidualError(A, x, bHELM);
disp(strcat('BiCGSTAB, HELM:',num2str(error)));

%% PCG
x = pcg(A, bCurrentIteration, 1e-5, 1000);
error = calculateResidualError(A, x, bCurrentIteration);
disp(strcat('PCG, current iteration:',num2str(error)));

x = pcg(A, bHELM, 1e-5, 1000);
error = calculateResidualError(A, x, bHELM);
disp(strcat('PCG, HELM:',num2str(error)));

%% BiCG
x = bicg(A, bCurrentIteration, 1e-5, 1000);
error = calculateResidualError(A, x, bCurrentIteration);
disp(strcat('BiCG, current iteration:',num2str(error)));

x = bicg(A, bHELM, 1e-5, 1000);
error = calculateResidualError(A, x, bHELM);
disp(strcat('BiCG, HELM:',num2str(error)));

%% BiCGSTAB(I)
x = bicgstabl(A, bCurrentIteration, 1e-5, 1000);
error = calculateResidualError(A, x, bCurrentIteration);
disp(strcat('BiCGSTAB(I), current iteration:',num2str(error)));

x = bicgstabl(A, bHELM, 1e-5, 1000);
error = calculateResidualError(A, x, bHELM);
disp(strcat('BiCGSTAB(I), HELM:',num2str(error)));

%% CGS
x = cgs(A, bCurrentIteration, 1e-5, 1000);
error = calculateResidualError(A, x, bCurrentIteration);
disp(strcat('CGS, current iteration:',num2str(error)));

x = cgs(A, bHELM, 1e-5, 1000);
error = calculateResidualError(A, x, bHELM);
disp(strcat('CGS, HELM:',num2str(error)));

%% LSQR
x = lsqr(A, bCurrentIteration, 1e-5, 1000);
error = calculateResidualError(A, x, bCurrentIteration);
disp(strcat('LSQR, current iteration:',num2str(error)));

x = lsqr(A, bHELM, 1e-5, 1000);
error = calculateResidualError(A, x, bHELM);
disp(strcat('LSQR, HELM:',num2str(error)));

%% MINRES
x = minres(A, bCurrentIteration, 1e-5, 1000);
error = calculateResidualError(A, x, bCurrentIteration);
disp(strcat('MINRES, current iteration:',num2str(error)));

x = minres(A, bHELM, 1e-5, 1000);
error = calculateResidualError(A, x, bHELM);
disp(strcat('MINRES, HELM:',num2str(error)));

%% QMR
x = qmr(A, bCurrentIteration, 1e-5, 1000);
error = calculateResidualError(A, x, bCurrentIteration);
disp(strcat('QMR, current iteration:',num2str(error)));

x = qmr(A, bHELM, 1e-5, 1000);
error = calculateResidualError(A, x, bHELM);
disp(strcat('QMR, HELM:',num2str(error)));

%% SYMMLQ
x = symmlq(A, bCurrentIteration, 1e-5, 1000);
error = calculateResidualError(A, x, bCurrentIteration);
disp(strcat('SYMMLQ, current iteration:',num2str(error)));

x = symmlq(A, bHELM, 1e-5, 1000);
error = calculateResidualError(A, x, bHELM);
disp(strcat('SYMMLQ, HELM:',num2str(error)));