/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

/*
 * Demonstrate various forms of repeating rules
 */

helloWorld				= "hello world";

// short hand forms, short hand does not parse whitespace
zeroOrOne               = helloWorld?;
zeroOrMore              = helloWorld*;
oneOrMore               = helloWorld+;

// explicit form repeats allowing for whitespace in between
explicitWs.zeroOrOne    = helloWorld[ .. 1];
explicitWs.zeroOrMore   = helloWorld[];
explicitWs.oneOrMore    = helloWorld[ 1.. ];
explicitWs.oneOrTwo     = helloWorld[1..2];
explicitWs.two          = helloWorld[2];

// explicit form repeats with no whitespace in between
explicitNoWs.zeroOrOne  = helloWorld<..1>;
explicitNoWs.zeroOrMore = helloWorld<>;
explicitNoWs.oneOrMore  = helloWorld<1..>;
explicitNoWs.oneOrTwo   = helloWorld<1..2>;
explicitNoWs.two        = helloWorld < 2 >;