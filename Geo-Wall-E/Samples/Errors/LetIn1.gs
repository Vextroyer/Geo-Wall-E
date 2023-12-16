//This is a G# program with errors.


//Checking that the a on the let -in expression goes out of scope
print let a = 3; in a;
a = 5;
print a;

//Cant redeclare constant, even on child scope.
print let a = -1; in a;
