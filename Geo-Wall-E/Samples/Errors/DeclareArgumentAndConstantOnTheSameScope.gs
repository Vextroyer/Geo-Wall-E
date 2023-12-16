//This is a G# program with errors. A variable and an argument are declared on the same scope.

p = let
        point punto;
        punto = 3;
    in
        2 + punto;
print p;

q = let
        punto = "Hola punto";
        point punto "puntiagudo";
    in
        2 * punto;