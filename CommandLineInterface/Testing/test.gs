//Courtesy of HulkI AutomatedTesting class library. You can see it at https://github.com/vextroyer/hulki
true = 1;
false = 0;
print "Hello world";
print 2;
print 3;
print 2 +3;
print if 2 > 3 then "llego aqui" else "llego alla";
print 2.141527;
print 12;
print "Pedrito fue a la escuela.";
print -15;
print -12.25;
print --1;
print ---15.37;
print 2 + 3 + 4 + 5 + 6 + 7 + 8 + 9 + 10;
print 2 - 3 - 4 - 5 - 6;
print 2 * 3 * 4 * 5;
print 4 / 2;
print 4^3^2;
print 15 % 4 % 2;
print 16%4;
print 2.14 - 3.14 + 0.86;
print 3 + 10 / 5;
print 10 / 5 + 3;
print 2 * 2.28 - 4.56;
print -4.56 + 2.28 * 2;
print 2^4*3+1.15;
print 3*2^4+1.15;
print 1.15+3*2^4;
print 1.15+2^4*3;
print 4^0.5;
print 2+3*4;
print 2*3+4;
print 2-3*4;
print 3+4/2;
print 4/2+3;
print 3-4/2;
print 4/2-3;
print 2^2 +3;
print 12 * -5;
print -5 * 12;
print -5 * -12;
print -12 * -5;
print 2 ^ 3 + 4 ^ 2 * 3 - 15 + 12 * -5 / 60;
print 28*(1+1);
print 28*(1-1);
print -32 * (-1) + 2^(2+3);
print -32 + (-1) * 2^(2+3);
print -32 - (-1) * 2^(2+3);
print -32 * (-1) - 2^(2+3);
print -(2 - 3);
print 10 / (5 / 5);
print (10 / 5) / 4;
print 2 - 3 - 4;
print 2 - (3 - 4);
print 1 + (3 - 4 * (12 - 574) + 3.14 - (-5 / 2.5));
print 2 > 3;
print 3 > 2;
print 2 + 3 < 3 + 2;
print 2 + 3 <= 3 + 2;
print 2 * -1 <= 0 + 1;
print -3 ^ 4 < 0;
print 2 + 2 < 2 * 2;
print 14 * 21 ^ 3 - 5.16 > (-5.16 + (-(13 ^ 4))) * 1.75;
print 12 * 10 / 2 / 4 >= 15;
print !(3 > 2);
print !(2 > 3);
print 2 ^ 15 > -2 ^ 15;
print 2 > 3 == false;
print 3 == 4;
print 2 + 3 == 5 * 12;
print !(2 * 2 ^(2 + 1) != 32 / 2 + 0);
print (2 == 3) != (2 > 3);
print true != true;
print true == true;
print false != false;
print false == false;
print true == false;
print false == true;
print true != false;
print false != true;
print true & true;
print true & false;
print false & true;
print false & false;
print true | true;
print true | false;
print false | true;
print false | false;
print "negro"=="blanco" and 2 + 2 == 4;
print (false & false) | true;
print false & (false | true);
print false & false | true;
print !(true | false);
print 3 >= 2 & 3 <= 2;
print 3 >= 2 & 2 >= 3;
print if (true) then 5 else 6;
print if (false) then 5 else 6;
print 2 + (if (2 <= 3) then 5 ^ 3 else 24 / 14.3);
print (if("Pedrito" == "Calvo") then 54 else 27 ^ (2 + 1) - 1) % 27;
print let in 2;

print let
        a = 2;
        b = 3;
        c = 7;
      in
        a * c - b;

print let ;;;;;;;;; in -27;

print let
        a = let 
                b = if 2 + 3 < 5 then 1 else (2-3)^5;
                c = let
                        k = 3 + b * b;
                    in
                        k * k;
            in
                - b - c;
      in
        987 * a;


print let
        point p1;
        a = let
                point p2;
                point p1;
            in 5;
      in a;  

print if (let x = 2; in x * x) < 4 then "Oh no, its bugged" else "Woooo, it works";

print let a = 1; b = 2; in a + b;

print let var = 5; in 6;

print let var = 5; in var;

print 2 + (let var1 = 5; var2 = 7; in (var1 * var2) ^ 2 );

print let a = "number"; b="two"; in a;

print let a = 2; in a + (let b = 3; in b + (let c = 4; in c));

print let moves = "e4xd5"; in if(moves == "e4xd5") then "Scandinavian defense" else "Cant figure it out";

print let moves = "e3xd5"; in if(moves == "e4xd5") then "Scandinavian defense" else "Cant figure it out";

print true | false | true;

print let x = 12; in x == 9 | x == 10 | x == 11 | x == 12;

print true & false & true;

print let x = 12; in x == 9 & x == 10 & x == 11 & x == 12;

print let a = 2; b = 4; c = "pedrito"; in (a == b) != c;

print let a = 2; b = 4; c = "pedrito"; in a == (b != c);

print   let
            a = let
                c = 4;
            in c;
            b = let
                c = 5; // válido
            in c;
        in a + b;

print 
    let
        a = let
            b = 4;
        in b;
        b = let // Es válido porque está declarada luego del hijo que la usa
            c = 5;
        in c;
    in a + b;

print let a = 1; in let b = 2; in let c = 3; in let d = 4; in let e = 5; in a + b + c + d + e;

print 4 ^ 3 ^ 2;

print if 2 > 3 then "llego aqui" else "llego alla";

print let
        a = 2;
        b = 3;
        c = 12;
        d = -1;
        e = if (a * c + d) ^ 2 > 3 and d + c > a - b then c ^ 3 else c ^ (1 / 3);
      in e;

print let
        a = 2;
        b = 3;
        c = 12;
        d = -1;
        e = if (a * c + d) ^ 2 > 3 and d + c > a - b then c ^ 3 else c ^ (1 / 3);
        f = if a then if b then c else d else if e then 4 else 5; 
      in f;
print 2 > 3;
print 2 < 3;
print 2 >= 3;
print 2 <= 3;
print 2 == 3;
print 2 != 3;
print 2 == 2;
print 2 != 2;
print if 0 then "pinto 0" else "pinto 1";
print if 1 then "pinto 0" else "pinto 1";

print let
        a() = "helloworld";
      in a();

print let
        a() = "helloworld";
      in let
            a = 3;
            b(a) = a;
      in b(a);

print let
        fib(n) = if n <= 1 then 1 else fib(n - 1) + fib(n - 2);
      in fib(10);

print let
        fact(n) = if(n <= 0) then 1 else n * fact(n - 1);
      in fact(5);

print measure(point(0,0),point(0,1));
print measure(point(0,0),point(1,0));
print measure(point(1,0),point(0,0));
print measure(point(0,1),point(0,0));

print let
        point(3,7) p1;
        point(6,3) p2;
        m = measure(p1,p2);
      in m;

print {1,2,3};
print {3,2,1};

print let
        f(x) = 2 ^ x;
        seq = {f(0),f(1),f(2),f(3)};
      in seq;

print let
point(3,-456.3) p1;
point(-265,2) p2;
point(12,5) p3;
in {p1,p2,p3};

print {1 ... 3};

print let
f(x) = if x <= 0 then 1 else f(x-1) + f(x-2);
in {f(2) ... f(6)};

eval let
a,_= {1,2};
print a;
in 1;

eval let
b,c,_ = {1,2};
print b;
print c;
in 1;

eval let
e,rest = {1,2};
print rest;
in 1;

eval let
f,g,resto = {15 ... };
h,_ = resto;
print h;
in 1;

eval let
a,b,c,d,resto = {1,2};
print a;
print b;
print c;
print d;
print resto;
in 1;

eval let
_,t = {1,2,3};
print t;
in 1;