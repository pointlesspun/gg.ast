/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

// $ matches any character (as long as there are characters)
anyCharacter = $;

// matches characters between 'a' and 'z'
aToZSet = `az`;

// matches characters between 'a' and 'z', 'A' and 'Z' and '0' to '9'
wideSet = `azAZ09`;

// matches any character as long as it's a, b, c
abcEnumeration = 'abc';

// matches any character as long as it is NOT a, b, c
notABCEnumeration = !'abc';
