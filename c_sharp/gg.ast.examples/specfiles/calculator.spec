/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using "./specfiles/numbers.spec";

# calculation = evaluation[1..];

# evaluation = multiply | divide | add | subtract | value;

// evaluations

multiply	= value, "*", evaluation;
add			= value, "+", evaluation;
subtract	= value, "-", evaluation;
divide		= value, "/", evaluation;

group		= "(", evaluation, ")";

# value		= number | group;