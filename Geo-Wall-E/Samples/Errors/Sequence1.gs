print {1,2,3};
point p1;
point p2;
point p3;
m = measure(p1,p2);
print p1;
print p2;
print p3;
print m;
print {point(2,3),arc(p1,p2,p3,m)};//Not allowed to have point and arc on a sequence.