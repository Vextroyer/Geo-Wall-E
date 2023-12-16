//This is a G# program with errors.

//Compute the maxium betweem two numbers.
max(x,y) = if(x <= y) then y else x;


increment(x) = x + 1;
decrement(x) = x - 1;

x = 3;

min(x,y) = if(max(x,y) == x) then y else x;

//Error , incremented a string.
//print increment("1");
//Error , decrementing a string
//print decrement("1");
