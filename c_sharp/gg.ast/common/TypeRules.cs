/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System;
using System.Collections.Generic;
using System.Linq;

using gg.ast.core;
using gg.ast.core.rules;

namespace gg.ast.common
{
    /// <summary>
    /// Series of rule which can be used to parse common basic types: numbers, booleans and strings.
    /// </summary>
    public static class TypeRules
    {
        public static class Tags
        {
            public static readonly string SignSymbol = "sign symbol";
            public static readonly string Sign = "sign";
            public static readonly string NumberString = "number string";
            public static readonly string Number = "number";
            public static readonly string Integer = "int";
            public static readonly string Decimal = "decimal";
            public static readonly string Exponent = "exponent";
            public static readonly string Boolean = "boolean";
            public static readonly string String = "string";

            public static readonly string EscapeSequence = "Escape sequence";
            public static readonly string EscapeCharacter = "escape character";
            public static readonly string EscapeSpecification = "escape specification";

            public static readonly string HexString = "hex string";

            public static readonly string StartString = "starting string delimiter";
            public static readonly string StringCharacters = "string characters";
            public static readonly string CloseString = "closing string delimiter";
        }

        /// <summary>
        /// Creates a mapping from strings to values. 
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static ValueMap CreateValueMap(ValueMap map = null)
        {
            var result = map ?? new ValueMap();

            result[Tags.Integer] = (str, node) => int.Parse(str.AsSpan(node.StartIndex, node.Length));
            result[Tags.Decimal] = (str, node) => double.Parse(str.AsSpan(node.StartIndex, node.Length));
            result[Tags.Exponent] = (str, node) => double.Parse(str.AsSpan(node.StartIndex, node.Length));
            result[Tags.String] = (str, node) => str.AsSpan(node.StartIndex+1, node.Length-2).ToString();
            result[Tags.Boolean] = (str, node) => bool.Parse(str.AsSpan(node.StartIndex, node.Length));
            result[Tags.Number] = (str, node) => double.Parse(str.AsSpan(node.StartIndex, node.Length));

            return result;
        }

        public static IRule CreateNumberStringRule(NodeVisiblity isHidden = NodeVisiblity.Hidden, int min = 1, int max = -1) =>
            new CharRule()
            {
                Tag = Tags.NumberString,
                Visibility = isHidden,
                MatchCharacters = CharRule.MatchType.InRange,
                Min = min,
                Max = max,
                Characters = "09"
            };

        public static IRule CreateHexCharactersRule(NodeVisiblity isHidden = NodeVisiblity.Hidden, int min = 1, int max = -1, bool abortOnFailure = false)
        {
            var rule = new CharRule()
            {
                Tag = Tags.HexString,
                Visibility = isHidden,
                MatchCharacters = CharRule.MatchType.InMultiRange,
                Characters = "afAF09",
                Min = min,
                Max = max,
            };

            return abortOnFailure 
                ? (IRule) new CriticalRule() 
                { 
                    Subrule = rule,
                    Tag = rule.Tag
                } 
                : rule;
        }

        public static IRule CreateSignRule(NodeVisiblity isHidden = NodeVisiblity.Hidden) =>      
           new CharRule()
           {
               Tag = Tags.Sign,
               Visibility = isHidden,
               MatchCharacters = CharRule.MatchType.InEnumeration,
               Min = 0,
               Max = 1,
               Characters = "+-"
           };


        public static IRule CreateIntegerRule(NodeVisiblity visibility = NodeVisiblity.Visible, string tag = null) =>        
            new SequenceRule()
            {
                Tag = tag ?? Tags.Integer,
                Visibility = visibility,
                Subrules = new IRule[]
                {
                    CreateSignRule(NodeVisiblity.Hidden),
                    CreateNumberStringRule(NodeVisiblity.Hidden)
                }
            };

        public static IRule CreateDecimalRule(
            NodeVisiblity visiblity = NodeVisiblity.Visible,
            IRule integerRule = null,
            IRule numberStringRule = null,
            string tag = null) =>
            new SequenceRule()
            {
                Tag = tag ?? Tags.Decimal,
                Visibility = visiblity,
                Subrules = new IRule[]
                {
                    integerRule ?? CreateIntegerRule(NodeVisiblity.Hidden),
                    new LiteralRule()
                    {
                        Tag = "dot",
                        Characters = "."
                    },
                    numberStringRule ?? CreateNumberStringRule(NodeVisiblity.Hidden)
                }
            };

        public static IRule CreateExponentRule(
            NodeVisiblity visibility = NodeVisiblity.Visible,
            IRule integerRule = null,
            IRule decimalRule = null,
            string tag = null

        ) {
            var exponentIntegerRule = integerRule ?? CreateIntegerRule(NodeVisiblity.Hidden);
            var exponentToken = new LiteralRule() {
                Tag = "exponent literal",
                Characters = "e",
                IsCaseSensitive = false
            };

            var integerExponent = new SequenceRule()
            {
                Tag = Tags.Integer,
                Subrules = new IRule[] {
                    exponentIntegerRule,
                    exponentToken,
                    exponentIntegerRule
                }
            };

            var decimalExponent = new SequenceRule()
            {
                Tag = Tags.Decimal,
                Visibility = NodeVisiblity.Hidden,
                Subrules = new IRule[] {
                    decimalRule ?? CreateDecimalRule(NodeVisiblity.Hidden),
                    exponentToken,
                    exponentIntegerRule
                }
            };

            return new OrRule()
            {
                Tag = tag ?? Tags.Exponent,
                Visibility = visibility,
                Subrules = new IRule[] { decimalExponent, integerExponent }
            };
        }

        public static IRule CreateNumberRule(
            NodeVisiblity visibility = NodeVisiblity.Hidden,
            IRule exponentRule = null,
            IRule decimalRule = null,
            IRule integerRule = null,
            string decimalTag = null,
            string integerTag = null,
            string exponentTag = null) =>
            new OrRule()
            {
                Tag = Tags.Number,
                Visibility = visibility,
                Subrules = new IRule[]
                {
                    exponentRule ?? CreateExponentRule(tag: exponentTag),
                    decimalRule ?? CreateDecimalRule(tag: decimalTag),
                    integerRule ?? CreateIntegerRule(tag: integerTag)
                }
            };

        public static IRule CreateEscapeRule(
            IRule argumentRule = null, 
            string escapeCharacters = "\\", 
            string letterEnumeration = "Uu", 
            NodeVisiblity visibility = NodeVisiblity.Hidden,
            bool abortOnCriticalFailue = true
        ) {
            var rules = new List<IRule>();

            if (escapeCharacters != null)
            {
                var rule = new CharRule()
                {
                    Visibility = NodeVisiblity.Hidden,
                    MatchCharacters = CharRule.MatchType.InEnumeration,
                    Characters = escapeCharacters,
                    Max = 1,
                    Tag = Tags.EscapeCharacter
                };
                rules.Add(rule);
            }

            var escapeCharacterRule = new CharRule()
            {
                Tag = Tags.EscapeSpecification,
                Visibility = NodeVisiblity.Hidden,
                MatchCharacters = CharRule.MatchType.InEnumeration,
                Characters = letterEnumeration,
                Max = 1,
            };

            rules.Add(abortOnCriticalFailue 
                ? (IRule) new CriticalRule() 
                { 
                    Tag = escapeCharacterRule.Tag,
                    Subrule = escapeCharacterRule 
                } 
                : escapeCharacterRule
            );

            if (argumentRule != null)
            {
                rules.Add(argumentRule);
            }

            return new SequenceRule()
            {
                Tag = Tags.EscapeSequence,
                Visibility = visibility,
                Subrules = rules.ToArray()
            };
        }

        public static IRule CreateStringRule(
            IRule escapeRule,
            string escapeCharacterEnumeration,
            string delimiters = "\"'`", 
            string tag = null,
            NodeVisiblity visibility = NodeVisiblity.Visible
        ) {
            return CreateStringRule((str) => {

                var notEscapeOrDelimiterCharacter = new CharRule()
                {
                    MatchCharacters = CharRule.MatchType.NotInEnumeration,
                    Characters = escapeCharacterEnumeration + str,
                    Visibility = NodeVisiblity.Hidden,
                    Min = 1,
                    Max = -1,                    
                };

                return new RepeatRule()
                {
                    Tag = "optional string characters",
                    Visibility = NodeVisiblity.Hidden,
                    Min = 0,
                    Max = -1,
                    Subrule = new OrRule()
                    {
                        Tag = "(not) escape characters",
                        Visibility = NodeVisiblity.Hidden,
                        Subrules = new IRule[]
                        {
                            escapeRule, 
                            notEscapeOrDelimiterCharacter
                        }
                    }
                };
            }, delimiters: delimiters, tag: tag, visibility : visibility);
        }

        public static IRule CreateStringRule(
            string delimiters = "\"'`",
            string tag = null,
            NodeVisiblity visibility = NodeVisiblity.Visible
        )
        {
            var characterRuleFunction = new Func<string, IRule>(str =>
            {
                var stringCharactersRule = new CharRule()
                {
                    MatchCharacters = CharRule.MatchType.NotInEnumeration,
                    Tag = Tags.StringCharacters,
                    Characters = str,
                    Visibility = NodeVisiblity.Hidden,
                    Min = 0,
                    Max = -1,
                };

                return new CriticalRule() 
                { 
                    Tag = stringCharactersRule.Tag,
                    Subrule = stringCharactersRule 
                };
            });

            return CreateStringRule(characterRuleFunction, delimiters, tag, visibility);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="characterRuleFunction">A function which takes a string as input and returns a rule which accepts
        /// all characters until a character is encountered which is in the input string.</param>
        /// <param name="delimiters"></param>
        /// <param name="tag"></param>
        /// <param name="visibility"></param>
        /// <returns></returns>
        public static IRule CreateStringRule(
            Func<string, IRule> characterRuleFunction,
            string delimiters = "\"'`",
            string tag = null,
            NodeVisiblity visibility = NodeVisiblity.Visible
        )
        {
            var subrules = new IRule[delimiters.Length];

            for (var i = 0; i < delimiters.Length; i++)
            {
                var delimiter = delimiters[i];

                subrules[i] = new SequenceRule()
                {
                    Tag = tag ?? Tags.String,
                    Visibility = visibility,
                    Subrules = new IRule[]
                    {
                        new LiteralRule() {
                            Tag = Tags.StartString,
                            Characters = delimiter.ToString(),
                            Visibility = NodeVisiblity.Hidden
                        },
                        characterRuleFunction(delimiters[i].ToString()),
                        new CriticalRule() 
                        {
                            Tag = Tags.CloseString,
                            Visibility = NodeVisiblity.Hidden,
                            Subrule = new LiteralRule()
                            {
                                Tag = Tags.CloseString,
                                Characters = delimiter.ToString(),
                                Visibility = NodeVisiblity.Hidden
                            },
                        }
                    },
                };
            }

            return new OrRule()
            {
                Tag = Tags.String,
                Visibility = NodeVisiblity.Hidden,
                Subrules = subrules
            };
        }

        public static IRule CreateBooleanRule(
            string[] booleanLiterals = null, 
            NodeVisiblity visibility = NodeVisiblity.Visible, 
            string tag = null)
        {
            var literals = booleanLiterals ?? new string[] { "true", "false" };
            var subrules = literals.Select(value => new LiteralRule()
            {
                Tag = "boolean literal",
                Characters = value,
                IsCaseSensitive = false
            }).ToArray();

            return new OrRule()
            {
                Tag = tag ?? Tags.Boolean,
                Visibility = visibility,
                Subrules = subrules
            };
        }
    }
}
 