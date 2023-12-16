//This is a G# program with errors.

print let
        a = let
                b = 4;
                in b + a; // Error, constant `a` is not defined.
      in a + 5;