// rule substitution
a = b;		
b = `09`*;

// meta rule substitution
c = !b;
d = !a;

// group rule substitution 
e = a, b;	 

// inline group rule substitution
f = `09`+, b;

// somewhat complex 
g = f, a, b, e;