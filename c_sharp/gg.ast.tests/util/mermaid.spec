/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

/**
 * Short version to parse to mermaid diagrams.
 */

mermaidChart        = flowChart | "sequenceDiagram";

flowChart           = flowChart.header, flowChart.element*;

flowChart.header    = flowChart.direction, "flowChart", eoln;
flowChart.element   = nodeName, edge, nodeName, eoln;

flowChart.direction = "TB" | "BT" | "RL" | "LR" | "TD";

nodeName            = (`azAZ09` | '_')*;

edge                =  arrow | line | dottedLine | labeledLine | labeledArrow | labeledDottedLine | labeledDottedArrow;

arrow               = "-->";
line                = "---";
dottedLine          = "-.->";
labeledLine         = "-- " label " ---";
labeledArrow        = "-- " label " -->";
labeledDottedLine   = "-- " label " ---";
labeledDottedArrow  = "-- " label " -->";
label               = (`azAZ09` | ' _')*;

eoln				= '\n\r';