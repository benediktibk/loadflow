%% Initalization
n = 15025;
A = loadMatrix('matrix.csv', n);
bCurrentIteration = loadVector('vector_currentiteration.csv');
bHELM = loadVector('vector_helm.csv');

%% Backslash
name = 'backslash operator';
x = A\bCurrentIteration;
error = calculateResidualError(A, x, bCurrentIteration);
disp(strcat(name,', current iteration:',num2str(error)));

x = A\bHELM;
error = calculateResidualError(A, x, bHELM);
disp(strcat(name,', HELM:',num2str(error)));
errorsHELM = error;
names = name;

%% BiCGSTAB
name = 'BiCGSTAB          ';
x = bicgstab(A, bCurrentIteration, 1e-5, 1000);
error = calculateResidualError(A, x, bCurrentIteration);
disp(strcat(name,', current iteration:',num2str(error)));

x = bicgstab(A, bHELM, 1e-5, 1000);
error = calculateResidualError(A, x, bHELM);
disp(strcat(name,', HELM:',num2str(error)));
errorsHELM = [errorsHELM; error];
names = [names; name];

%% PCG
name = 'PCG               ';
x = pcg(A, bCurrentIteration, 1e-5, 1000);
error = calculateResidualError(A, x, bCurrentIteration);
disp(strcat(name,', current iteration:',num2str(error)));

x = pcg(A, bHELM, 1e-5, 1000);
error = calculateResidualError(A, x, bHELM);
disp(strcat(name,', HELM:',num2str(error)));
errorsHELM = [errorsHELM; error];
names = [names; name];

%% BiCG
name = 'BiCG              ';
x = bicg(A, bCurrentIteration, 1e-5, 1000);
error = calculateResidualError(A, x, bCurrentIteration);
disp(strcat(name,', current iteration:',num2str(error)));

x = bicg(A, bHELM, 1e-5, 1000);
error = calculateResidualError(A, x, bHELM);
disp(strcat(name,', HELM:',num2str(error)));
errorsHELM = [errorsHELM; error];
names = [names; name];

%% BiCGSTAB(I)
name = 'BiCGSTAB(I)       ';
x = bicgstabl(A, bCurrentIteration, 1e-5, 1000);
error = calculateResidualError(A, x, bCurrentIteration);
disp(strcat(name,', current iteration:',num2str(error)));

x = bicgstabl(A, bHELM, 1e-5, 1000);
error = calculateResidualError(A, x, bHELM);
disp(strcat(name,', HELM:',num2str(error)));
errorsHELM = [errorsHELM; error];
names = [names; name];

%% CGS
name = 'CGS               ';
x = cgs(A, bCurrentIteration, 1e-5, 1000);
error = calculateResidualError(A, x, bCurrentIteration);
disp(strcat(name,', current iteration:',num2str(error)));

x = cgs(A, bHELM, 1e-5, 1000);
error = calculateResidualError(A, x, bHELM);
disp(strcat(name,', HELM:',num2str(error)));
errorsHELM = [errorsHELM; error];
names = [names; name];

%% LSQR
name = 'LSQR              ';
x = lsqr(A, bCurrentIteration, 1e-5, 1000);
error = calculateResidualError(A, x, bCurrentIteration);
disp(strcat(name,', current iteration:',num2str(error)));

x = lsqr(A, bHELM, 1e-5, 1000);
error = calculateResidualError(A, x, bHELM);
disp(strcat(name,', HELM:',num2str(error)));
errorsHELM = [errorsHELM; error];
names = [names; name];

%% MINRES
name = 'MINRES            ';
x = minres(A, bCurrentIteration, 1e-5, 1000);
error = calculateResidualError(A, x, bCurrentIteration);
disp(strcat(name,', current iteration:',num2str(error)));

x = minres(A, bHELM, 1e-5, 1000);
error = calculateResidualError(A, x, bHELM);
disp(strcat(name,', HELM:',num2str(error)));
errorsHELM = [errorsHELM; error];
names = [names; name];

%% QMR
name = 'QMR               ';
x = qmr(A, bCurrentIteration, 1e-5, 1000);
error = calculateResidualError(A, x, bCurrentIteration);
disp(strcat(name,', current iteration:',num2str(error)));

x = qmr(A, bHELM, 1e-5, 1000);
error = calculateResidualError(A, x, bHELM);
disp(strcat(name,', HELM:',num2str(error)));
errorsHELM = [errorsHELM; error];
names = [names; name];

%% SYMMLQ
name = 'SYMMLQ            ';
x = symmlq(A, bCurrentIteration, 1e-5, 1000);
error = calculateResidualError(A, x, bCurrentIteration);
disp(strcat(name,', current iteration:',num2str(error)));

x = symmlq(A, bHELM, 1e-5, 1000);
error = calculateResidualError(A, x, bHELM);
disp(strcat(name,', HELM:',num2str(error)));
errorsHELM = [errorsHELM; error];
names = [names; name];

%% TFMQR
name = 'TFMQR             ';
x = tfqmr(A, bCurrentIteration, 1e-5, 1000);
error = calculateResidualError(A, x, bCurrentIteration);
disp(strcat(name,', current iteration:',num2str(error)));

x = tfqmr(A, bHELM, 1e-5, 1000);
error = calculateResidualError(A, x, bHELM);
disp(strcat(name,', HELM:',num2str(error)));
errorsHELM = [errorsHELM; error];
names = [names; name];

%% LU
name = 'LU                ';
[L, U] = lu(A);
x = U\(L\bCurrentIteration);
error = calculateResidualError(A, x, bCurrentIteration);
disp(strcat(name,', current iteration:',num2str(error)));

x = U\(L\bHELM);
error = calculateResidualError(A, x, bHELM);
disp(strcat(name,', HELM:',num2str(error)));
errorsHELM = [errorsHELM; error];
names = [names; name];

%% QR
name = 'QR                ';
[Q, R] = qr(A);
x = R\(Q'\bCurrentIteration);
error = calculateResidualError(A, x, bCurrentIteration);
disp(strcat(name,', current iteration:',num2str(error)));

x = R\(Q'\bHELM);
error = calculateResidualError(A, x, bHELM);
disp(strcat(name,', HELM:',num2str(error)));
errorsHELM = [errorsHELM; error];
names = [names; name];

%% Output
[errorsSorted, order] = sort(errorsHELM(:));
namesSorted = names(order, :);
algorithmCount = size(namesSorted, 1);

disp('------------------------');
disp('results, sorted');
for i = 1:algorithmCount
    disp(strcat(namesSorted(i, :),': ',num2str(errorsSorted(i))));
end