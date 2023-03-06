/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using "./specfiles/types.spec";

interpreter				= usingStatements, ruleList;

usingStatements			= ("using", string, ";")*;

ruleList				= rule*;
rule					= visibility?, identifier, "=", ruleValue, ";";

visibility				= "#";

ruleValue				= groupType | unaryValue;

groupType				= sequence | or | sequenceNoSeparator;

unaryValue				= not? (literal | identifier | charRule | grouping) repeat?;

not						= "!"; 

// xxx should be able to state literal = string
literal					= '"' ( ("\\" $) | !'"\\'+ )* '"';

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

sequence				= unaryValue, ",", unaryValue, (",", unaryValue)*;
or						= unaryValue, "|", unaryValue, ("|", unaryValue)*;
sequenceNoSeparator		= unaryValue whitespace unaryValue (whitespace unaryValue)*;

identifier				= (`azAZ` | "_") (`azAZ09` | "_")*;

