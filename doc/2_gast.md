# Gast Tool

The `gast tool` ([gg.ast.tool](../c_sharp/gg.ast.tool/Program.cs)) is a command line tool to (eventually) run various AST related tasks. Currently it can only be used to export rules in spec files to a mermaid diagram.

The command line options for the gast tool are:

`gast.exe specfile -m|--mermaid mermaidOutputfile -o|--options rdm`

* `r`: show reference nodes. Having this option will add nodes produced by reference rules to the diagram.
* `d`: allow duplicates. Normally the graph will only show unique nodes and will create loops if rules  are referred to by different rules. However this may create somewhat confusing diagrams. With duplication these references are replicated.
* `m`: export the chart with `"```mermaid"` tags so it can be easily added to a .md file (like this one)

Take the spec following as an example:

```csharp
rule0 = rule1 | rule2;

rule1 = a | b;
a = "foo";
b = "bar";
c = b;

rule2 = "baz" | a | c;

```

With no options (`gast example.spec --mermaid example.md`) this results in (possibly best seen with a mermaid plugin):

```mermaid
flowchart TD
fe29af48["rule0(or)"]
  fe29af48 --> 64e47996
  64e47996["rule1(or)"]
    64e47996 --> 49c63f1b
    49c63f1b["&quot;foo&quot;"]:::literalRule
    64e47996 --> 2aa16b5a
    2aa16b5a["&quot;bar&quot;"]:::literalRule
  fe29af48 --> 79062999
  79062999["rule2(or)"]
    79062999 --> 420af9ff
    420af9ff["&quot;baz&quot;"]:::literalRule
      79062999 -.-> 49c63f1b
    79062999 --> 6123901a
    6123901a["&quot;bar&quot;"]:::literalRule
```

With the show references, 'r', option (`gast example.spec --mermaid example.md -o r`) the flowchart will look this: 

```mermaid
flowchart TD
7910d8f6["rule0(or)"]
  7910d8f6 --> e9bdf35f
  e9bdf35f[/"rule1(ref)"/]:::referenceRule
    e9bdf35f --> f796274f
    f796274f["rule1(or)"]
      f796274f --> 464e7d4
      464e7d4[/"a(ref)"/]:::referenceRule
        464e7d4 --> 8e09bf9b
        8e09bf9b["&quot;foo&quot;"]:::literalRule
      f796274f --> 3e3b913c
      3e3b913c[/"b(ref)"/]:::referenceRule
        3e3b913c --> a25350e
        a25350e["&quot;bar&quot;"]:::literalRule
  7910d8f6 --> a07b6571
  a07b6571[/"rule2(ref)"/]:::referenceRule
    a07b6571 --> 59ece3f4
    59ece3f4["rule2(or)"]
      59ece3f4 --> 4e8b9ebc
      4e8b9ebc["&quot;baz&quot;"]:::literalRule
      59ece3f4 --> 1f7bc37
      1f7bc37[/"a(ref)"/]:::referenceRule
          1f7bc37 -.-> 8e09bf9b
      59ece3f4 --> 6bbd6663
      6bbd6663[/"c(ref)"/]:::referenceRule
        6bbd6663 --> 585b7de1
        585b7de1[/"b(ref)"/]:::referenceRule
            585b7de1 -.-> a25350e
```

With the 'd' option, allow duplicates (`gast example.spec --mermaid example.md -o d`) the flowchart will look this: 

```mermaid
flowchart TD
8c48e12f["rule0(or)"]
  8c48e12f --> 94c423ea
  94c423ea["rule1(or)"]
    94c423ea --> 283d6e20
    283d6e20["&quot;foo&quot;"]:::literalRule
    94c423ea --> 5f6db12a
    5f6db12a["&quot;bar&quot;"]:::literalRule
  8c48e12f --> 7f4b6fbf
  7f4b6fbf["rule2(or)"]
    7f4b6fbf --> 87a26bde
    87a26bde["&quot;baz&quot;"]:::literalRule
    7f4b6fbf --> e5e787bd
    e5e787bd["&quot;foo&quot;"]:::literalRule
    7f4b6fbf --> 599d7e93
    599d7e93["&quot;bar&quot;"]:::literalRule
```

With the 'rd' option, allow both references and duplicates (`gast example.spec --mermaid example.md -o rd`) the flowchart will look this: 

```mermaid
flowchart TD
1952b1da["rule0(or)"]
  1952b1da --> 2cd6dba0
  2cd6dba0[/"rule1(ref)"/]:::referenceRule
    2cd6dba0 --> d379051e
    d379051e["rule1(or)"]
      d379051e --> 60d37709
      60d37709[/"a(ref)"/]:::referenceRule
        60d37709 --> 9a1de7e8
        9a1de7e8["&quot;foo&quot;"]:::literalRule
      d379051e --> 53f64ea
      53f64ea[/"b(ref)"/]:::referenceRule
        53f64ea --> 7c3409de
        7c3409de["&quot;bar&quot;"]:::literalRule
  1952b1da --> ed900812
  ed900812[/"rule2(ref)"/]:::referenceRule
    ed900812 --> 8d4acd8e
    8d4acd8e["rule2(or)"]
      8d4acd8e --> 434d5214
      434d5214["&quot;baz&quot;"]:::literalRule
      8d4acd8e --> fbd99318
      fbd99318[/"a(ref)"/]:::referenceRule
        fbd99318 --> 16cd33f8
        16cd33f8["&quot;foo&quot;"]:::literalRule
      8d4acd8e --> a46ab8db
      a46ab8db[/"c(ref)"/]:::referenceRule
        a46ab8db --> 905ea207
        905ea207[/"b(ref)"/]:::referenceRule
          905ea207 --> 5d473734
          5d473734["&quot;bar&quot;"]:::literalRule
```


