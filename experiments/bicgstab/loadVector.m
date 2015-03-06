function [x] = loadVector(fileName)
    content = load(fileName);
    x = content(:,1) + i*content(:,2);
end