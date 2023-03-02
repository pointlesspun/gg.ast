/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System;
using System.Collections.Generic;
using System.IO;

using gg.ast.core;
using gg.ast.core.rules;

using gg.ast.common;

namespace gg.ast.examples.json
{
    public static class JsonRules
    {
        private static readonly IRule DefaultWhitespace = ShortHandRules.CreateWhitespaceRule();

        public static class Tags
        {
            public static readonly string Document = "document";

            public static readonly string Value = "value";
            public static readonly string Object = "object";
            public static readonly string Property = "property";
            public static readonly string PropertySeparator = "propery separator (:)";
            public static readonly string PropertyName = "name";
            public static readonly string PropertyList = "property list";
            public static readonly string PropertyListSeparator = "property list separator (,)";
            public static readonly string PropertyListItems = "property list items";
            public static readonly string OptionalProperties = "optional properties";

            public static readonly string Array = "array";
            public static readonly string ValueList = "value list";
            public static readonly string ValueListSeparator = "value list separator (,)";
            public static readonly string ValueListItems = "value list items";
            public static readonly string OptionalArrayListValues = "optional array list values";

            public static readonly string String = "string";
            public static readonly string EscapeCharacter = "escape character";
            public static readonly string EscapeSequence = "escape sequence";

            public static readonly string Boolean = "boolean";
            public static readonly string Number = "number";
            public static readonly string Null = "null";
        }

        public static (string text, List<AstNode> ast) ReadJsonFile(string filename)
        {
            var text = File.ReadAllText(filename);
            return (text, Parse(text));
        }

        public static List<AstNode> Parse(string jsonText)
        {
            return CreateJsonDocument().Parse(jsonText).Nodes;
        }

        public static ValueMap CreateValueMap(ValueMap map = null)
        {
            var jsonValueMap = map ?? TypeRules.CreateValueMap();

            jsonValueMap[Tags.Array] = (str, node) =>
            {
                var length = node.Children.Count;
                var result = new object[length];

                for (var i = 0; i < length; ++i)
                {
                    result[i] = jsonValueMap.Map(node.Children[i].GetTag(), str, node.Children[i]);
                }

                return result;
            };

            jsonValueMap[Tags.Object] = (str, node) =>
            {
                var length = node.Children.Count;
                var result = new Dictionary<string, object>();

                for (var i = 0; i < length; ++i)
                {
                    var property = node.Children[i];
                    var name = str.AsSpan(property[0].StartIndex + 1, property[0].Length - 2).ToString();
                    var value = jsonValueMap.Map(property[1].GetTag(), str, property[1]);

                    result[name] = value;
                }

                return result;
            };

            jsonValueMap[Tags.Document] = jsonValueMap[Tags.Object];
            jsonValueMap[Tags.Null] = (str, node) => null;

            return jsonValueMap;
        }



        public static IRule CreateValueRule(RuleVisiblity visibility = RuleVisiblity.Hidden)
        {
            var valueRule = new OrRule()
            {
                Tag = Tags.Value,
                Visibility = visibility,
                Subrules = new IRule[]
                {
                    TypeRules.CreateNumberRule(decimalTag: Tags.Number, integerTag: Tags.Number, exponentTag: Tags.Number),
                    ExtendedStringRules.CreateStringRule(),
                    TypeRules.CreateBooleanRule(tag: Tags.Boolean),
                    new LiteralRule()
                    {
                        Tag = Tags.Null,
                        Characters = "null",
                        Visibility = RuleVisiblity.Visible
                    }
                }
            };

            return valueRule;
        }

        public static IRule CreateValueRule(
            IRule objParser,
            IRule arrayParser,
            RuleVisiblity visibility = RuleVisiblity.Hidden)
        {
            var result = (OrRule)CreateValueRule(visibility);

            var subrules = new IRule[result.Subrules.Length + 2];
            Array.Copy(result.Subrules, 0, subrules, 2, result.Subrules.Length);

            subrules[0] = objParser;
            subrules[1] = arrayParser;

            result.Subrules = subrules;

            return result;
        }

        public static IRule CreatePropertyRule(
            IRule propertyValueParser = null,
            IRule propertyNameParser = null,
            RuleVisiblity visibility = RuleVisiblity.Visible,
            string separator = ":")
        {
            var valueRule = propertyValueParser ?? CreateValueRule(visibility: RuleVisiblity.Transitive);

            var propertySeparatorRule = new LiteralRule()
            {
                Tag = Tags.PropertySeparator,
                Characters = separator,
                Visibility = RuleVisiblity.Hidden
            };

            return new SequenceRule()
            {
                Tag = Tags.Property,
                Visibility = visibility,
                WhiteSpaceRule = DefaultWhitespace,
                Subrules = new IRule[]
                {
                    propertyNameParser ?? TypeRules.CreateStringRule(delimiters: "\"", tag: Tags.PropertyName, visibility: RuleVisiblity.Visible),
                    new CriticalRule()
                    {
                        Tag = propertySeparatorRule.Tag,
                        Subrule = propertySeparatorRule,
                        Visibility = RuleVisiblity.Transitive

                    },
                    new CriticalRule()
                    {
                        Tag = valueRule.Tag,
                        Subrule = valueRule,
                    }
                }
            };
        }

        public static IRule CreatePropertyListRule(
            IRule propertyRule = null,
            RuleVisiblity visibility = RuleVisiblity.Visible,
            string separator = ",")
        {
            var listPropertyRule = propertyRule ?? CreatePropertyRule();

            var propertyChain = new SequenceRule()
            {
                Tag = "property chain",
                Visibility = RuleVisiblity.Transitive,
                WhiteSpaceRule = DefaultWhitespace,
                Subrules = new IRule[]
                {
                    new LiteralRule() {
                    Tag = Tags.PropertyListSeparator,
                    Characters = separator,
                    Visibility = RuleVisiblity.Hidden
                },
                    new CriticalRule()
                    {
                        Tag = listPropertyRule.Tag,
                        Subrule = listPropertyRule
                    }
                }
            };

            var propertyChainRepeat = new RepeatRule()
            {
                Tag = "property chain repeat",
                Min = 0,
                Max = -1,
                Visibility = RuleVisiblity.Transitive,
                Subrule = propertyChain,
                WhiteSpaceRule = DefaultWhitespace
            };

            return new SequenceRule()
            {
                Tag = Tags.PropertyList,
                Visibility = visibility,
                WhiteSpaceRule = DefaultWhitespace,
                Subrules = new IRule[]
                {
                    listPropertyRule,
                    propertyChainRepeat
                }
            };
        }

        public static IRule CreateValueListRule(RuleVisiblity visibility = RuleVisiblity.Visible, IRule valueRule = null)
        {
            var listValueRule = valueRule ?? CreateValueRule();

            var valueChain = new SequenceRule()
            {
                Tag = "comma value!",
                Visibility = RuleVisiblity.Transitive,
                WhiteSpaceRule = DefaultWhitespace,
                Subrules = new IRule[]
                {
                    new LiteralRule()
                    {
                        Tag = Tags.ValueListSeparator,
                        Characters = ",",
                        Visibility = RuleVisiblity.Hidden,
                    },
                    new CriticalRule()
                    {
                        Tag = listValueRule.Tag,
                        Subrule = listValueRule,
                    }
                }
            };

            return new SequenceRule()
            {
                Tag = Tags.ValueList,
                Visibility = visibility,
                WhiteSpaceRule = DefaultWhitespace,
                Subrules = new IRule[]
                {
                    listValueRule,
                    new RepeatRule()
                    {
                        Tag = "value chain",
                        Min = 0,
                        Max = -1,
                        Visibility = RuleVisiblity.Transitive,
                        WhiteSpaceRule = DefaultWhitespace,
                        Subrule = valueChain
                    }
                }
            };
        }

        public static (IRule objectRule, IRule valueRule, IRule arrayRule)
                CreateCoreRules(IRule valueRule = null, SequenceRule arrayRule = null)
        {
            var objectRule = new SequenceRule()
            {
                Tag = Tags.Object,
                Visibility = RuleVisiblity.Visible,
                WhiteSpaceRule = DefaultWhitespace
            };

            return InitializeObjectRuleList(objectRule, valueRule, arrayRule);
        }

        public static IRule CreateObjectRule(IRule valueParser = null, SequenceRule arrayParser = null)
        {
            var objectRule = new SequenceRule()
            {
                Tag = Tags.Object,
                Visibility = RuleVisiblity.Visible,
                WhiteSpaceRule = DefaultWhitespace
            };

            return InitializeObjectRuleList(objectRule, valueParser, arrayParser).objectRule;
        }

        public static (IRule objectRule, IRule valueRule, IRule arrayRule) InitializeObjectRuleList(
            SequenceRule objectParser,
            IRule valueRule = null,
            SequenceRule arrayParser = null)
        {
            var objectArrayParser = arrayParser ?? new SequenceRule()
            {
                Tag = Tags.Array,
                Visibility = RuleVisiblity.Visible,
                WhiteSpaceRule = DefaultWhitespace
            };

            valueRule ??= CreateValueRule(objectParser, objectArrayParser);

            if (arrayParser == null)
            {
                InitializeArrayRule(objectArrayParser, valueRule, objectParser);
            }

            objectParser.Subrules = new IRule[] {
                new LiteralRule()
                {
                    Tag = "object start",
                    Characters = "{",
                    Visibility = RuleVisiblity.Hidden,
                },
                new RepeatRule()
                {
                    Tag = Tags.OptionalProperties,
                    Subrule = CreatePropertyListRule(CreatePropertyRule(valueRule), visibility: RuleVisiblity.Transitive),
                    Min = 0,
                    Max = 1,
                    Visibility = RuleVisiblity.Transitive,
                    WhiteSpaceRule = DefaultWhitespace
                },
                new LiteralRule()
                {
                    Tag = "object end",
                    Characters = "}",
                    Visibility = RuleVisiblity.Hidden,
                }
            };

            return (objectParser, valueRule, objectArrayParser);
        }

        public static IRule CreateArrayRule(IRule valueParser = null, SequenceRule objectParser = null)
        {
            var arrayRule = new SequenceRule()
            {
                Tag = Tags.Array,
                Visibility = RuleVisiblity.Visible,
                WhiteSpaceRule = DefaultWhitespace
            };

            return InitializeArrayRule(arrayRule, valueParser, objectParser).arrayRule;
        }

        public static (IRule arrayRule, IRule valueRule, IRule objectRule) InitializeArrayRule(SequenceRule arrayParser, IRule valueRule = null, SequenceRule objectParser = null)
        {
            var arrayObjectParser = objectParser ?? new SequenceRule()
            {
                Tag = Tags.Object,
                WhiteSpaceRule = DefaultWhitespace
            };

            valueRule ??= CreateValueRule(arrayObjectParser, arrayParser);

            if (objectParser == null)
            {
                InitializeObjectRuleList(arrayObjectParser, valueRule, arrayParser);
            }

            arrayParser.Subrules = new IRule[]
            {
                new LiteralRule()
                {
                    Tag = "array start",
                    Characters = "[",
                    Visibility = RuleVisiblity.Hidden,
                },
                new RepeatRule()
                {
                    Tag = Tags.OptionalProperties,
                    Subrule = CreateValueListRule(valueRule: valueRule, visibility: RuleVisiblity.Transitive),
                    Min = 0,
                    Max = 1,
                    Visibility = RuleVisiblity.Transitive,
                    WhiteSpaceRule = DefaultWhitespace
                },
                new LiteralRule()
                {
                    Tag = "array end",
                    Characters = "]",
                    Visibility = RuleVisiblity.Hidden,
                },
            };

            return (arrayParser, valueRule, arrayObjectParser);
        }

        public static IRule CreateJsonDocument()
        {
            var (objectRule, _, arrayRule) = InitializeObjectRuleList(new SequenceRule()
            {
                Tag = Tags.Object,
                WhiteSpaceRule = DefaultWhitespace
            }, null, null);

            return new OrRule()
            {
                Tag = Tags.Document,
                Visibility = RuleVisiblity.Transitive,
                Subrules = new IRule[]
                {
                    objectRule, arrayRule
                }
            };
        }
    }
}
