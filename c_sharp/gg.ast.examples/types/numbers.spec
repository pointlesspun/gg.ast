/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

/**
 * Rules to parse hexadecimal numbers, integers, decimals, exponents and their superset: numbers.
 */

number = hex | exponent | decimal | integer;
exponent		= (integer 'eE' integer) | (decimal 'eE' integer);
decimal			= integer "." numberString; 
integer			= sign? numberString;
hex				= "0x" hexString;
# numberString	= `09`+;
# hexString		= (`09` | `af` | `AF`)+;
# sign			= '+-';