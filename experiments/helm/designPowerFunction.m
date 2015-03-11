input = [0.85; 0.8; 1];
output = [0.0475; 0.1; 0];
A = [input.^2 input ones(3, 1)];
coefficients = A\output;
f = @(x) coefficients(1)*x^2 + coefficients(2)*x + coefficients(3);
fplot(f, [0.7 1.1]);
display(coefficients);