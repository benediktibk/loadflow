function [A] = loadMatrix(fileName, dimension)
    content = load(fileName);
    rows = content(:,1) + 1;
    cols = content(:,2) + 1;
    values = content(:,3) + i*content(:,4);
    A = sparse(rows, cols, values, dimension, dimension);
end

