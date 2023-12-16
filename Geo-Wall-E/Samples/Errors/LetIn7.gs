//This is a G# program with errors.

print let
        a = let
                b = 4;
            in b + 2;
      in a + b; // Error, constant ‘b’ is not defined.