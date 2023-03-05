/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using "./specfiles/types.spec";

interpreter				= usingStatement*, rule*;

usingStatement			= "using", string, ";";

rule					= visibility?, identifier, "=", ruleValue, ";";

visibility				= "#";

ruleValue				= groupType | unaryType;

groupType				= sequence | or | sequenceNoSeparator;

unaryType				= not? (literal | identifier | charRule | grouping) repeat?;

not						= "!";

literal					= string;

charRule				= charRule.any | charRule.enumeration | charRule.range;
charRule.any			= "$" | "any";
charRule.enumeration	= "'" $+ "'";
charRule.range			= "`" $+ "'";

grouping				= "(", (ruleValue, (",", ruleValue)*)?, ")";

repeat					= repeat.qualified | repeat.unary;
repeat.qualified		= repeat.betweenNandM | repeat.nOrMore | repeat.noMoreThanM | repeat.exact | repeat.zeroOrMore;

repeat.zeroOrMore		= "[", "]";
repeat.nOrMore			= "[", integer, "..", "]";
repeat.noMoreThanM		= "[", "..", integer, "]";
repeat.betweenNandM		= "[", integer, "..", integer, "]";
repeat.exact			= "[", integer, "]";

repeat.unary			= "+" | "*" | "?";

sequence				= unaryType, ",", unaryType, (",", unaryType)*;
or						= unaryType, "|", unaryType, ("|", unaryType)*;
sequenceNoSeparator		= unaryType whitespace unaryType (whitespace unaryType)*;

identifier				= (`azAZ` | "_") (`azAZ09` | "_")*;

