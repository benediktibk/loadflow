%% Initialization
n = 15025;
A = loadMatrix('matrix.csv', n);
b = loadVector('vector_helm.csv');

%% Bandwidth reduction
order = symrcm(A);
AReduced = A(order, order);

%% Plot of matrix structures
close all;
[row, col] = find(A);
scatter(row, col);
lowerBandwidth = bandwidth(A, 'lower');
upperBandwidth = bandwidth(A, 'upper');
disp(strcat('bandwith of A:',num2str(max([lowerBandwidth, upperBandwidth]))));

[row, col] = find(AReduced);
figure;
scatter(row, col);
lowerBandwidth = bandwidth(AReduced, 'lower');
upperBandwidth = bandwidth(AReduced, 'upper');
disp(strcat('bandwith of reduced A:',num2str(max([lowerBandwidth, upperBandwidth]))));

%% Decomposition
[L, U, P] = luDecomposition(AReduced);