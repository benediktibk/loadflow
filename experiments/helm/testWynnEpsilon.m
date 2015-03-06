function [okay] = testWynnEpsilon()
    sampleEven = [0 0.5 0.5625 0.57910156 0.58383965 0.58521719]';
    sampleOdd = sampleEven(1:(end - 1), 1);
    
    resultEven = wynnEpsilon(sampleEven);
    resultOdd = wynnEpsilon(sampleOdd);
    
    resultEvenShouldBe = 0.58578573;
    resultOddShouldBe = 0.58574349;
    
    okay = abs(resultEven - resultEvenShouldBe) < 1e-7;
    okay = okay & abs(resultOdd - resultOddShouldBe) < 1e-7;
end