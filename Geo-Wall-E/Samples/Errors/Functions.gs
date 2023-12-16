//This is a G# program with errors.

a() = 2 + 3;//Define a as a function.

a = 3;//Redefinition of a on the same scope. Error.

eval let a = 3; in a; //Redefinition of a as a constant, but on a different scope, ok.

a(3) = let print "A is 3 here" in 3;//The argument of a must be an identifier, but is a number. Error.
