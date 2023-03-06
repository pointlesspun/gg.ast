/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

/*
 * Example of a minimal json specification.
 */

document = array | object;

# jsonValue = typeValue | array | object;

property = key, ":", jsonValue;

// using an identifier as key is not really according to standards... but it's practical
# key = string | identifier;

identifier = (`azAZ` | "_") (`azAZ09` | "_")*;

array = "[", (jsonValue, (",", jsonValue)*)?, "]";
object = "{", (property, (",", property)*)?, "}";

# typeValue = null | boolean | string | number;

null = "null";
boolean = "true" | "false" | "True" | "False";

// --- strings ------------------------

string = '"' stringCharacters '"';

# stringCharacters = ( escape | notEscape )*;
# escape = "\\" $;
# notEscape = !'"\\'+;

number = hex | exponent | decimal | integer;
exponent = (integer 'eE' integer) | (decimal 'eE' integer);
decimal         = integer "." numberString;
integer = sign ? numberString;
hex = "0x" hexString;
# numberString	= `09`+;
# hexString		= (`09` | `af` | `AF`)+;
# sign			= '+-';