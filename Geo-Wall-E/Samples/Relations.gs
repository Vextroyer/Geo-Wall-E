//Sample program to test relational operators and conditional constructs.

print if 2 > 3 then "llego aqui" else "llego alla";
a = 2;
b = 3;
c = 12;
d = -1;
e = if (a * c + d) ^ 2 > 3 and d + c > a - b then c ^ 3 else c ^ (1 / 3);
print e;
f = if a then if b then c else d else if e then 4 else 5; 
print f; 
print 2 > 3;
print 2 < 3;
print 2 >= 3;
print 2 <= 3;
print 2 == 3;
print 2 != 3;
print 2 == 2;
print 2 != 2;
// print 2 > "strong"; //error
print if 0 then "pinto 0" else "pinto 1";
print if 1 then "pinto 0" else "pinto 1";