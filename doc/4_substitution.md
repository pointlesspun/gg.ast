# Substitution and Inlining

When compiling a IRule graph there are various optimizations happening. The optimizations can be grouped in two types of operations. 

* `Inlining`: this process replaces `'reference rules'` ie, rules referring to other rules with their actual non-reference rule.
* `Substitution`: this process replaces groups of rules for single rules where possible. 

## Inlining 

let's say we have the following spec file:

```csharp
rule = a | b;
a = "foo";
b = c
c = a, "bar";
```

in `rule = a | b`, 'a' and 'b' are reference rules, referring to well ... a and b. The corresponding graph will look like this:


```mermaid
flowchart TD
4c923616["rule(or)"]
  4c923616 --> 88903b44
  88903b44[/"a(ref)"/]:::referenceRule
    88903b44 --> 665644aa
    665644aa["&quot;foo&quot;"]:::literalRule
  4c923616 --> a0e3ddb9
  a0e3ddb9[/"b(ref)"/]:::referenceRule
    a0e3ddb9 --> 12ec3177
    12ec3177[/"c(ref)"/]:::referenceRule
      12ec3177 --> e1e0a8e5
      e1e0a8e5["c(sequence)"]
        e1e0a8e5 --> dbb5f9a5
        dbb5f9a5[/"a(ref)"/]:::referenceRule
          dbb5f9a5 --> 7dcb06a
          7dcb06a["&quot;foo&quot;"]:::literalRule
        e1e0a8e5 --> 4116a682
        4116a682["&quot;bar&quot;"]:::literalRule
```

When parsing a text using this graph, the rule graph will have to go through the reference rules to find the eventual rules that actually test something. In order to avoid having to do all this redundant work, we can replace the references actual values. This will reduce the graph to this: 

```mermaid
flowchart TD
330401e6["rule(or)"]
  330401e6 --> 94d82830
  94d82830["&quot;foo&quot;"]:::literalRule
  330401e6 --> 50272b6c
  50272b6c["b(sequence)"]
    50272b6c --> d915d2ad
    d915d2ad["&quot;foo&quot;"]:::literalRule
    50272b6c --> 49eb2c1f
    49eb2c1f["&quot;bar&quot;"]:::literalRule
```

Note that while the references are replaced, it is only their "content" which changes but the Tag stays the same. Eg when `b` in the example above is replaced with `c`, b's contents change from a `reference` to a `sequence` (ie the contents of `c`) but it keeps the tag 'b'. 

By keeping the tags, we can make aliases to other rules thus defining the "intended use" as expressed by its tag in context of the rest of the rule graph, but re-using the content of already defined rules. Eg in json a "string" can have the "intended use" of a "key" of a property but also the "value" of the same property.  

## Substitution

When applying _substitution_ to a rule graph we try to replace multiple nodes with a single node. In the current version there is only of case where this happens: the substitution of a "charRule" followed by a "repeatRule". The charRule has a "build-in" repeat, so instead of having an extra object specifying the min and max number of repeats, we can just fold that into the charRule itself.