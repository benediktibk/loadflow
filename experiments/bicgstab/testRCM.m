%% example 1
n = 8;
A = sparse(n, n);
A(1, 5) = 1;
A(2, 3) = 1;
A(2, 6) = 1;
A(2, 8) = 1;
A(3, 5) = 1;
A(4, 7) = 1;
A(6, 8) = 1;
A = A + A' + speye(n);
order = symrcm(A);
disp(strcat('matlab order  :',num2str(order)));
disp(strcat('tutorial order:',num2str([1 5 3 2 6 8 4 7])));

%% example 2
close all;
rng(1);
n = 20;
A = sprandsym(n, 0.2);
order = symrcm(A);
Areduced = A(order, order);
[row, col] = find(A);
scatter(row, col);
[row, col] = find(Areduced);
figure;
scatter(row, col);

%% example 3
close all;
rng(1);
n = 15025;
m = 50;
A = loadMatrix('matrix.csv', n);
A = A(1:m, 1:m);
order = symrcm(A);
Areduced = A(order, order);
[row, col] = find(A);
scatter(row, col);
[row, col] = find(Areduced);
figure;
scatter(row, col);