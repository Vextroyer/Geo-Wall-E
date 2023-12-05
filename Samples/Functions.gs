//This is a G# program with some functions declarations and usage.

//Compute the maxium betweem two numbers.
max(x,y) = if(x <= y) then y else x;

print max(2,3);
print max(5,6);

increment(x) = x + 1;
decrement(x) = x - 1;

x = 3;

print max(increment(x),decrement(x));

min(x,y) = if(max(x,y) == x) then y else x;

print min(2,3);
print min(5,6);

print min(increment(x),decrement(x));