/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

// Example of an or rule.
helloOrWorld = "hello" | "world";

// Order matters: wrongResult will try a value before an operation and stop. If the input
// is an operation it will always be tagged as a value. correctResult will try to parse
// an operation before a value and will correctly tag input as either an operation or a value.
# wrongResult        = value | operation;
# correctResult      = operation | value;

operation          = value, operator, value;
operator           = "+" | "-" | "/" | "*";
value              = `09`;