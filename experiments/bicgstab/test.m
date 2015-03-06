A = [   1+2i,   3+4i,   5+6i;
        0       7+8i,   9+10i;
        11+12i, 0,      13+14i];
x = [15+16i; 17+18i; 19+20i];
b = A*x;
P = diag(diag(A));
x0 = zeros(3, 1);
maximumIterations = 100;
epsilon = 1e-15;

xMatlab = bicgstab(A, b, epsilon, maximumIterations, P);
matlabError = norm(xMatlab - x);
xOriginal = bicgstabOriginal(A, x0, b, P, maximumIterations, epsilon);
originalError = norm(xOriginal - x);
xModified = bicgstabModified(A, x0, b, P, maximumIterations, epsilon);
modifiedError = norm(xModified - x);

display(strcat('matlab error: ',num2str(matlabError)));
display(strcat('original error: ',num2str(originalError)));
display(strcat('modified error: ',num2str(modifiedError)));
    