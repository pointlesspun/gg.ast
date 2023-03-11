# Releases

This is an overview of functionality of previous releases

## v 0.2

* Implement interpreter in spec file
* Create Mermaid files from RuleSets and Asts
* Add example program start with `gg.ast "specfile" << input` or `gg.ast "specfile" -f "inputfile"`.
* Add not_in_range to characters
* More clean up and documentation
	* Split up read me into a short read me and some different documentation
	* Add substitution documentation
	* Add tool documentation
	* Add interpreter documentation

### rejected 

* add long form for some: sequence(a,b,c), or(a,b), repeat(a, 3, 4), critical(), not(a), scan(x), move(-3), anchor("bla"), characters(any, "axsv"),
	Not going to do this, needs to be done in the spec file.
