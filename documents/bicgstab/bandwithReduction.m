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

%% Plot of decomposition structures
close all;
[L, U, P] = lu(A);
[row, col] = find(L);
scatter(row, col);
title('L');
figure;
[row, col] = find(U);
scatter(row, col);
title('U');

[L, U, P] = lu(AReduced);
[row, col] = find(L);
figure;
scatter(row, col);
title('L, reduced');
figure;
[row, col] = find(U);
scatter(row, col);
title('U, reduced');

%% Comparison of memory usage
mCandidates = 100:50:500;
count = size(mCandidates, 2);
bytesDirect = zeros(count, 1);
bytesReduced = zeros(count, 1);
i = 1;

for m = mCandidates
    disp(m);
    A = loadMatrix('matrix.csv', n);
    A = A(1:m, 1:m);
    order = symrcm(A);
    AReduced = A(order, order);
    
    [L, U,] = luDecomposition(A);
    [LReduced, UReduced,] = luDecomposition(AReduced);
    
    sL = whos('L');
    sU = whos('U');
    bytesDirect(i) = sL.bytes + sU.bytes;
    sL = whos('LReduced');
    sU = whos('UReduced');
    bytesReduced(i) = sL.bytes + sU.bytes;
    i = i + 1;
end

%% Plot memory usage
close all;
figure;
plot(bytesReduced, 'g');
hold on;
plot(bytesDirect, 'r');
hold off;