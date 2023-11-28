//This is a G# program with errors.


//Checking that the a on the let -in expression goes out of scope
print let a = 3; in a;
a = 5;
print a;

//Cant redeclare constant, even on child scope.
print let a = -1; in a;

//Empty expression after `in` keyword
print let
            b = 4;
      in ;

//Usage of undeclared variable a
b = let 
        a = 5;
    in a + b;

c = let
        a = let
                d = 3;
            in a + d;
    in a - 2;


print let
        a = 5;
        b = let
                a = 4; // Error, redefinición de la constante a.
            in a + 2;
        in a + b;


print let
        a = let
                b = 4;
                in b + a; // Error, la constante ‘a’ no está definida
      in a + 5;

print let
        a = let
                b = 4;
            in b + 2;
      in a + b; // Error, la constante ‘b’ no está definida