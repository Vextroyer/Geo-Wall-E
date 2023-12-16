//A sample G# program to demostrate let-in usage.


//Constant redeclaration is not allowed even in children scopes. But argument redeclaration is allowed.
point p1 "Iam p1";
print p1;
a = let 
        point p2 "Iam p2";
        print p2;
        point p1 "Iam p1 inside let in";
        print p1;
    in 2;
print p1;

//Nested let-in usage
print let
        z = let 
                b = if 2 + 3 < 5 then 1 else (2-3)^5;
                c = let
                        k = 3 + b * b;
                    in
                        k * k;
            in
                - b - c;
      in
        987 * z;

//Some arithmetics
print let
        z = 2;
        b = 3;
        c = 7;
      in
        z * c - b;

//Empty statements
print let ;;;;;;;;; in -27;

//Let-in used inside an if-else-then expression
q = if (let x = 2; in x * x) < 4 then "Oh no, its bugged" else "Woooo, it works";
print q;

print 
    let
        z = let
            b = 4;
        in b;
        b = let // Es válido porque está declarada luego del hijo que la usa
            c = 5;
        in c;
    in z + b;

//Accesing variables in a parent scope.
print let z = 1; in let b = 2; in let c = 3; in let d = 4; in let e = 5; in z + b + c + d + e;