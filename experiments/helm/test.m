okay = testWynnEpsilon();

if okay
    display('Wynns Epsilon Algorithm is okay');
else
    display('ERROR: Wynns Epsilon Algorithm is not okay!');
end

okay = testHelm();

if okay
    display('HELM is okay');
else
    display('ERROR: HELM is not okay!');
end