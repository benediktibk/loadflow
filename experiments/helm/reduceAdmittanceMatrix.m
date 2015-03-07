function [admittancesReduced, constantCurrents] = reduceAdmittanceMatrix(admittances, slackIndices, slackVoltages)
    rowCount = size(admittances, 1);
    columnCount = size(admittances, 2);
    
    if rowCount ~= columnCount
        error('admittance matrix must be symmetric');
    end
    
    n = rowCount;
    leftOverIndices = setdiff(1:n, slackIndices);
    admittancesReduced = admittances(leftOverIndices, leftOverIndices);
    slackAdmittances = admittances(leftOverIndices, slackIndices);
    constantCurrents = (-1)*slackAdmittances*slackVoltages;    
end

