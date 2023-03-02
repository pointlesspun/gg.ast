/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

// This by itself will always succeed as long as not "foo" on the input. 
// Since the not rule will not move the cursor, this will be stuck in an infinite loop
// on any input except "foo" (except the repeat rule will throw an exception for finding
// an infinite loop)
infiniteLoop = !"foo"*;

// alternatively using ~ instead of ! will read one character when the not rule succeeds,
// avoiding an infinite loop. 
avoidInfiniteLoop   = ~"foo"*;

// this next example shows how to avoid this infinite loop by reading a character after not
commentStart        = "/*";
commentEnd          = "*/";

// adding a $ at the end will read at least one character thus avoiding the infinite loop   
notEndOfComment     = !commentEnd $;

// alternatively using ~ instead of ! will read one character when the not rule succeeds,
// providing a performance benefit. 
skipUntilCommentEnd = ~commentEnd*;

comment             = commentStart notEndOfComment* commentEnd;

altComment          = commentStart skipUntilCommentEnd commentEnd;