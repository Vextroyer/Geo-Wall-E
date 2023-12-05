//This is a G# program with errors.

print let
        a = 5;
        b = let
                a = 4; // Error, redefinición de la constante a.
            in a + 2;
        in a + b;