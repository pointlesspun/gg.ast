/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

/*
 * Example of a minimal json specification.
 */
using "./specfiles/types.spec";

document = array | object;

# jsonValue = typeValue | array | object;

property = key, ":", jsonValue;

// using an identifier as key is not really according to standards... but it's practical
# key = string | identifier;

identifier = (`azAZ` | "_") (`azAZ09` | "_")*;

array = "[", (jsonValue, (",", jsonValue)*)?, "]";
object = "{", (property, (",", property)*)?, "}";
