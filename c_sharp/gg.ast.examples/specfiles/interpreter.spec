/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using "./specfiles/types.spec";

interpreter					= usingStatements, ruleList;

usingStatements				= ("using", string, ";")*;

ruleList					= rule[];
rule						= visibility?, identifier, "=", ruleValue, ";";

visibility					= "#";

ruleValue					= groupType | unaryValue;

# groupType					= sequence | or | sequenceNoSeparator;

unaryValue					= notType?, (literal | ruleReference | charRule | grouping), repeat?;

# notType					= (not | notAndSkip);
not							= "!"; 
notAndSkip					= "~";

literal						= string; 
ruleReference				= identifier;

# charRule					= charRule.any | charRule.enumeration | charRule.range;
charRule.any				= "$" | "any";
charRule.enumeration		= "'" enumeration.chars "'";
charRule.range				= "`" range.chars "`";

# enumeration.chars			= ( escape | endOfEnumerationChars )+;
# endOfEnumerationChars		= !'\'\\'+;

# range.chars				= ( escape | endOfRangeChars )+;
# endOfRangeChars			= !'`\\'+;

grouping					= "(", (ruleValue, (",", ruleValue)*)?, ")";

# repeat					= repeat.ws | repeat.noWs;

repeat.ws					= "[", repeat.specification, "]";

repeat.noWs					= ("<", repeat.specification, ">") | repeat.unary;

# repeat.specification		= repeat.betweenNandM | repeat.nOrMore | repeat.noMoreThanM | repeat.exact | repeat.zeroOrMore;

repeat.zeroOrMore			= whitespace;
repeat.nOrMore				= integer, "..";
repeat.noMoreThanM			= "..", integer;
repeat.betweenNandM			= integer, "..", integer;
repeat.exact				= integer;

# repeat.unary				= repeat.unary.zeroOrMore | repeat.unary.oneOrMore | repeat.unary.zeroOrOne;

repeat.unary.zeroOrMore		= "*";
repeat.unary.oneOrMore		= "+";
repeat.unary.zeroOrOne		= "?";

sequence					= unaryValue, ",", unaryValue, (",", unaryValue)*;
or							= unaryValue, "|", unaryValue, ("|", unaryValue)*;
sequenceNoSeparator			= unaryValue whitespace unaryValue (whitespace unaryValue)*;

identifier					= (`azAZ` | "_") (`azAZ09` | '_.' )*;