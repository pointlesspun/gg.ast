/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

// This sequence separates the rules with a space, this implies tokens need to appear in the input text without spaces.
helloWorld = helloToken worldToken;

// This sequence separates the rules with a comma, allowing for whitespace to appear in between the tokens.
helloSpaciousWorld = helloToken, worldToken;

helloToken = "hello";
worldToken = "world";