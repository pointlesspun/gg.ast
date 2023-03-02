/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

/**
 * Rules to parse strings, booleans, hexadecimal numbers, integers, decimals, exponents and their superset: numbers.
 */

using "./types/numbers.spec";

# typeValue = null | boolean | string | number;

null = "null";
boolean = "true" | "false" | "True" | "False";

// --- strings ------------------------

string = '"' stringCharacters '"';

# stringCharacters = ( escape | notEscape )*;
# escape = "\\" $;
# notEscape = !'"\\'+;
