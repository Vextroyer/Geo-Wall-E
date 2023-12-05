//This is a G# program with errors.

//Variable a is used but not declared.
c = let
        a = let
                d = 3;
            in a + d;
    in a - 2;