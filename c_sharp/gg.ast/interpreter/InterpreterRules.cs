/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using gg.ast.core;
using gg.ast.core.rules;
using gg.ast.common;

using static gg.ast.common.ShortHandRules;

namespace gg.ast.interpreter
{
    public static class InterpreterRules
    {
        public static IRule CreateInterpreterRule(InterpreterConfig config)
        {
            var useBlock = new RepeatRule()
            {
                Tag = config.Tags.UseList,
                Visibility = RuleVisiblity.Visible,
                Min = 0,
                Max = -1,
                Subrule = UseStatement(config)
            };

            return new SequenceRule()
            {
                Tag = config.Tags.Interpreter,
                Subrules = new IRule[]
                {
                    useBlock,
                    CreateRuleListRule(config)
                }
            };
        }

        public static IRule UseStatement(InterpreterConfig config, RuleVisiblity visibility = RuleVisiblity.Transitive)
        {
            var ruleTerminator = new CriticalRule().Bind(new LiteralRule()
            {
                Tag = config.Tags.RuleTerminator,
                Characters = config.Tokens.RuleTerminator,
                Visibility = RuleVisiblity.Hidden
            });

            var fileName = new CriticalRule().Bind(TypeRules.CreateStringRule(tag: config.Tags.UseFile));

            var useKeyword = new LiteralRule()
            {
                Visibility = RuleVisiblity.Hidden,
                Characters = config.Tokens.Use,
            };

            return new SequenceRule()
            {
                Tag = config.Tags.UseList,
                WhiteSpaceRule = config.WhiteSpace,
                Visibility = visibility,
                Subrules = new IRule[]
                {
                    useKeyword,
                    fileName,        
                    ruleTerminator
                }
            };
        }
            
        public static IRule CreateRuleListRule(InterpreterConfig config, RuleVisiblity visibility = RuleVisiblity.Visible)
        {
            return ZeroOrMore(InterpreterRule(config), config.WhiteSpace, visibility, config.Tags.RuleList);
        }

        /// <summary>
        /// Create a rule which creates a rule to parse a rule :)
        /// A rule has the form of "identifier: ruleValue;"
        /// </summary>
        /// <param name="config"></param>
        /// <param name="visibility"></param>
        /// <returns></returns>
        public static IRule InterpreterRule(InterpreterConfig config, RuleVisiblity visibility = RuleVisiblity.Visible)
        {
            var ruleSeparator = new CriticalRule().Bind(new LiteralRule()
            {
                Tag = config.Tags.RuleSeparator,
                Characters = config.Tokens.RuleSeparator,
                Visibility = RuleVisiblity.Hidden
            });

            var ruleValue = new CriticalRule().Bind(RuleValue(config, visibility: RuleVisiblity.Visible));

            var ruleTerminator = new CriticalRule().Bind(new LiteralRule()
            {
                Tag = config.Tags.RuleTerminator,
                Characters = config.Tokens.RuleTerminator,
                Visibility = RuleVisiblity.Hidden
            });

            var visibilityRule = new CharRule()
            {
                Tag = config.Tags.RuleVisibility,
                Characters = config.Tokens.RuleVisibility,
                Min = 0, 
                Max = 2,
                MatchCharacters = CharRule.MatchType.InEnumeration
            };

            return new SequenceRule()
            {
                Tag = config.Tags.Rule,
                Visibility = visibility,
                WhiteSpaceRule = config.WhiteSpace,
                Subrules = new IRule[] {
                    visibilityRule,
                    CreateIdentifierRule(config, tag: config.Tags.RuleTag),
                    ruleSeparator,
                    ruleValue,
                    ruleTerminator
                }
            };
        }

        /// <summary>
        /// Match any character
        /// </summary>
        /// <param name="config"></param>
        /// <param name="visibility"></param>
        /// <returns></returns>
        public static IRule MatchAnyCharacter(InterpreterConfig config, RuleVisiblity visibility = RuleVisiblity.Visible)
        {
            return new LiteralRule()
            {
                Tag = config.Tags.MatchAnyCharacter,
                Characters = config.Tokens.MatchAnyCharacter,
                Visibility = visibility
            };
        }

        /// <summary>
        /// Match characters in a certain range eg. [a..z]
        /// </summary>
        /// <param name="config"></param>
        /// <param name="visibility"></param>
        /// <returns></returns>
        public static IRule MatchCharactersInRange(InterpreterConfig config, 
            RuleVisiblity visibility = RuleVisiblity.Visible)
        {
            return ExtendedStringRules.CreateStringRule(tag: config.Tags.MatchCharactersInRange,
                                            delimiters:config.Tokens.MatchCharactersInRangeDelimiter,
                                            visibility: visibility);
        }

        /// <summary>
        /// Match characters in a certain set eg. {abc}
        /// </summary>
        /// <param name="config"></param>
        /// <param name="visibility"></param>
        /// <returns></returns>
        public static IRule MatchCharactersInSet(InterpreterConfig config,
            RuleVisiblity visibility = RuleVisiblity.Visible)
        {
            return ExtendedStringRules.CreateStringRule(tag: config.Tags.MatchCharactersInEnumeration,
                                            delimiters: config.Tokens.MatchCharactersInEnumerationDelimiter,
                                            visibility: visibility);
        }

        /// <summary>
        /// Selects one of the character rules
        /// </summary>
        /// <param name="config"></param>
        /// <param name="visibility"></param>
        /// <returns></returns>
        public static IRule CharRuleSelection(InterpreterConfig config,
            RuleVisiblity visibility = RuleVisiblity.Transitive)
        {
            return new OrRule()
            {
                Tag = "select charrule",
                Visibility = visibility,
                Subrules = new IRule[]
                {
                    MatchCharactersInRange(config),
                    MatchAnyCharacter(config),
                    MatchCharactersInSet(config)
                }
            };
        }


        /// <summary>
        /// Create a "loose" rule to match identifiers. Identifiers can be anything
        /// made out of alphanumeric and _ or @ characters.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="visibility"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static IRule CreateIdentifierRule(
            InterpreterConfig config, 
            RuleVisiblity visibility = RuleVisiblity.Visible, 
            string tag = null)
        {
            var nonAlphanumericCharacter = new CharRule()
            {
                Tag = "non alphanumeric characters",
                Characters = "_@.",
                MatchCharacters = CharRule.MatchType.InEnumeration,
                Visibility = RuleVisiblity.Hidden
            };

            var alphanumericCharacter = new CharRule()
            {
                Tag = "alphanumeric",
                Characters = "azAZ09",
                MatchCharacters = CharRule.MatchType.InMultiRange,
                Visibility = RuleVisiblity.Hidden
            };

            var identifierCharacters = new OrRule()
            {
                Tag = "Identifier character selection",
                Visibility = RuleVisiblity.Hidden,
                Subrules = new IRule[] {
                    nonAlphanumericCharacter ,
                    alphanumericCharacter
                }
            };

            return OneOrMore(identifierCharacters, visibility: visibility, tag: tag ?? config.Tags.Identifier);              
        }

        /// <summary>
        /// Create a rule grouping: "(" and a rule value closing with ")". This allows
        /// for sequences in sequences, eg an or inside a sequence
        /// </summary>
        /// <param name="config"></param>
        /// <param name="visibility"></param>
        public static IRule GroupingRule(
            InterpreterConfig config,
            RuleVisiblity visibility = RuleVisiblity.Transitive,
            IRule valueRule = null)
        {
            var value = valueRule ?? RuleValue(config, RuleVisiblity.Visible);

            var beginGroup = new LiteralRule()
            {
                Tag = config.Tags.BeginGroup,
                Characters = config.Tokens.BeginGroup,
                Visibility = RuleVisiblity.Hidden
            };

            var endGroup = new CriticalRule()
                .Bind(new LiteralRule()
                {
                    Tag = config.Tags.EndGroup,
                    Characters = config.Tokens.EndGroup,
                    Visibility = RuleVisiblity.Hidden
                });

            return new SequenceRule()
            {
                Tag = config.Tags.Group,
                Visibility = visibility,
                WhiteSpaceRule = config.WhiteSpace,
                Subrules = new IRule[]
                {
                    beginGroup,
                    value,
                    endGroup
                }
            };
        }

        /// <summary>
        /// Creates a (or) rule to parse one of the unary values: string, ref or group. This
        /// does not include sequences or 'or' rules.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="visibility"></param>
        /// <returns></returns>
        public static IRule UnaryValueRule(
            InterpreterConfig config, 
            RuleVisiblity visibility = RuleVisiblity.Visible,
            IRule ruleValueRule = null)
        {
            var unaryValue = new SequenceRule()
            {
                Tag = config.Tags.UnaryValue,
                Visibility = visibility,
                WhiteSpaceRule = config.WhiteSpace
            };

            var stringRule = ExtendedStringRules.CreateStringRule(tag: config.Tags.Literal);
            var identifierRule = CreateIdentifierRule(config, tag: config.Tags.RuleReference);
            var valueRule = ruleValueRule ?? RuleValue(config, unaryValueSelection: unaryValue);

            var selectValue = SelectFrom(stringRule,
                identifierRule,
                CharRuleSelection(config),
                GroupingRule(config, valueRule: valueRule));

            unaryValue.Subrules = new IRule[]
            {
                CreateNotRule(config),
                selectValue,
                Optional(CreateRepeatRule(config), config.WhiteSpace)
            };

            return unaryValue;
        }

        private static IRule CreateNotRule(InterpreterConfig config)
        {
            return Optional(SelectFrom(
                new LiteralRule()
                {
                    Tag = config.Tags.Not,
                    Characters = config.Tokens.Not
                },
                new LiteralRule()
                {
                    Tag = config.Tags.NotAndSkip,
                    Characters = config.Tokens.NotAndSkip
                }
            ));
        }     


        /// <summary>
        /// A rulevalue consists of a main node with one or two children:
        /// 
        /// - RuleValue (Sequence)
        ///    - The value
        ///    - The repeat component (optional)
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="visibility"></param>
        /// <returns></returns>
        public static IRule RuleValue(
            InterpreterConfig config, 
            RuleVisiblity visibility = RuleVisiblity.Visible,
            IRule unaryValueSelection = null)
        {
            var ruleValue = new OrRule()
            {
                Tag = config.Tags.RuleValue,
                Visibility = visibility
            };
                
            var unaryValue = unaryValueSelection ?? UnaryValueRule(config, ruleValueRule: ruleValue);

            var groupTypeSelection = SelectFrom(
                    SequenceRule(config, valueRule: unaryValue),
                    CreateOrRule(config, valueRule: unaryValue),
                    NoSeparatorSequenceRule(config, valueRule: unaryValue));

            var notOperator = CreateNotRule(config);

            var repeatOperator = Optional(CreateRepeatRule(config), config.WhiteSpace);

            ruleValue.Subrules = new IRule[]
            {
                groupTypeSelection,
                WrapUnaryOperators(config, notOperator, ExtendedStringRules.CreateStringRule(tag: config.Tags.Literal), repeatOperator),
                WrapUnaryOperators(config, notOperator, CreateIdentifierRule(config,tag: config.Tags.RuleReference), repeatOperator ),
                WrapUnaryOperators(config, notOperator, GroupingRule(config, valueRule: ruleValue), repeatOperator),
                WrapUnaryOperators(config, notOperator, CharRuleSelection(config), repeatOperator)
            };

            return ruleValue;
        }

        /// <summary>
        /// Wrap the given rule in optional unary operators ie not and repeat (! and [])
        /// </summary>
        /// <param name="config"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        public static IRule WrapUnaryOperators(InterpreterConfig config, IRule notOperator, IRule rule, IRule repeatOperator)
        {
            return new SequenceRule()
            {
                Tag = "rule and annotations",
                Visibility = RuleVisiblity.Transitive,
                Subrules = new IRule[]
                {
                    notOperator,
                    rule,
                    repeatOperator
                },
                WhiteSpaceRule = config.WhiteSpace
            };
        }


        public static IRule SequenceRule(InterpreterConfig config, RuleVisiblity visibility = RuleVisiblity.Visible, IRule valueRule = null)
        {
            return CompositeRule(
                config,
                config.Tags.Sequence,
                RuleGroupSeparatorRule(config.Tags.SequenceSeparator, config.Tokens.SequenceValueListSepatator),
                config.WhiteSpace,
                visibility: visibility,
                valueRule: valueRule);
        }

        /// <summary>
        /// Create a serquence rule which does not use whitespace but uses whitespace 
        /// as a separator. This should allow for a rule definition like:
        ///
        ///     rule = "foo" "bar";
        ///     
        /// where no whitespace is expected in between the input. So "foobar" is valid
        /// input for the generater intepreter; "foo bar" or "foo \n bar" is not.
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="visibility"></param>
        /// <param name="valueRule"></param>
        /// <returns></returns>
        public static IRule NoSeparatorSequenceRule(
            InterpreterConfig config, 
            RuleVisiblity visibility = RuleVisiblity.Visible, 
            IRule valueRule = null)
        {
            var groupValueRule = valueRule ?? UnaryValueRule(config, RuleVisiblity.Visible);
            var groupSeparatorRule = RuleGroupSeparatorRule(config.Tags.SequenceSeparator, config.Tokens.Whitespace, max: -1);
            
            var endGroup = new LiteralRule()
            {
                Tag = config.Tags.EndGroup,
                Characters = config.Tokens.EndGroup,
                Visibility = RuleVisiblity.Hidden
            };

            var ruleTerminator = new LiteralRule()
            {
                Tag = "rule terminator",
                Characters = ";",
                Visibility = RuleVisiblity.Hidden
            };

            var chainEndTokens = SelectFrom(ruleTerminator, endGroup);

            var testEndGroupToken = new NotRule()
            {
                Subrule = new SequenceRule()
                {
                    Visibility = RuleVisiblity.Hidden,
                    WhiteSpaceRule = null,
                    Subrules = new IRule[]
                    {
                        config.WhiteSpace,
                        chainEndTokens
                    }
                }
            };

            var groupChain = new SequenceRule()
            {
                Tag = $"{groupSeparatorRule} value!",
                Visibility = RuleVisiblity.Transitive,
                Subrules = new IRule[]
                {
                    testEndGroupToken,
                    groupSeparatorRule,
                    groupValueRule
                }
            };

            return new SequenceRule()
            {
                Tag = config.Tags.SequenceWithoutSeparator,
                Visibility = visibility,
                WhiteSpaceRule = null,
                Subrules = new IRule[]
                {
                    config.WhiteSpace,
                    groupValueRule,
                    groupSeparatorRule,
                    groupValueRule,
                    ZeroOrMore(groupChain),
                }
            };
        }


        public static IRule CreateOrRule(InterpreterConfig config, RuleVisiblity visibility = RuleVisiblity.Visible, IRule valueRule = null)
        {
            return CompositeRule(
                config,
                config.Tags.Or,
                RuleGroupSeparatorRule(config.Tags.OrSeparator, config.Tokens.OrValueListSepatator),                
                config.WhiteSpace,
                visibility: visibility,
                valueRule: valueRule);
        }

        /// <summary>
        /// Create a rule to parse a "repeat" variations 
        /// </summary>
        /// <param name="hidden"></param>
        /// <returns></returns>
        public static IRule CreateRepeatRule(InterpreterConfig config, RuleVisiblity visibility = RuleVisiblity.Visible)
        {
            // create all the repeat variations with whitespace ([], [N..], [..M], [N..M])
            var repeat = CreateRepeatRule(config, 
                config.Tags.Repeat, 
                config.Tokens.BeginRepeat, 
                config.Tokens.EndRepeat,
                visibility);

            // create all the repeat variations without whitespace (<>, <N..>, <..M>, <N..M>)
            var repeatNoWhitespace = CreateRepeatRule(config,
                config.Tags.RepeatNoWhitespace,
                config.Tokens.BeginRepeatNoWhitespace,
                config.Tokens.EndRepeatNoWhitespace,
                visibility);

            // create a repeat for +
            var repeatOneOrMore = new LiteralRule()
            {
                Tag = config.Tags.RepeatOneOrMore,
                Characters = config.Tokens.RepeatOneOrMore,
                Visibility = visibility
            };

            // create a repeat for *
            var repeatZeroOrMore = new LiteralRule()
            {
                Tag = config.Tags.RepeatZeroOrMore,
                Characters = config.Tokens.RepeatZeroOrMore,
                Visibility = visibility
            };

            // create a repeat for ?
            var repeatZeroOrOne = new LiteralRule()
            {
                Tag = config.Tags.RepeatZeroOrOne,
                Characters = config.Tokens.RepeatZeroOrOne,
                Visibility = visibility
            };

            // collect all unary repeat rules under the 'no whitespace' tag
            var repeatUnaryValues = new OrRule()
            {
                Tag = config.Tags.RepeatNoWhitespace,
                Visibility = RuleVisiblity.Visible,
                Subrules = new IRule[]
                {
                    repeatOneOrMore,
                    repeatZeroOrMore,
                    repeatZeroOrOne
                }
            };

            // (transitive) select from all repeats
            return SelectFrom(repeat, repeatNoWhitespace, repeatUnaryValues);
        }

        /// <summary>
        /// Create a parameterized repeat rule (eg [], <>, [N..M], <N..M>)
        /// </summary>
        /// <param name="config"></param>
        /// <param name="tag"></param>
        /// <param name="startDelimiter"></param>
        /// <param name="endDelimiter"></param>
        /// <param name="visibility"></param>
        /// <returns></returns>
        private static IRule CreateRepeatRule(
            InterpreterConfig config,
            string tag,
            string startDelimiter,
            string endDelimiter,
            RuleVisiblity visibility = RuleVisiblity.Visible)
        {
            var intRule = TypeRules.CreateIntegerRule(tag: TypeRules.Tags.Integer);
            var ellipsisRule = new LiteralRule()
            {
                Tag = "ellipsis",
                Characters = "..",
                Visibility = RuleVisiblity.Hidden
            };
            var beginRepeat = new LiteralRule()
            {
                Tag = "begin repeat",
                Characters = startDelimiter,
                Visibility = RuleVisiblity.Hidden
            };

            var closeRepeat = new CriticalRule().Bind(
                new LiteralRule()
                {
                    Tag = "close repeat",
                    Characters = endDelimiter,
                    Visibility = RuleVisiblity.Hidden
                });

            var repeatBetweenNAndM = new SequenceRule()
            {
                Tag = config.Tags.RepeatBetweenNandM,
                Visibility = visibility,
                WhiteSpaceRule = config.WhiteSpace,
                Subrules = new IRule[]
                {
                    beginRepeat, intRule, ellipsisRule, intRule, closeRepeat
                }
            };

            var repeatNOrMore = new SequenceRule()
            {
                Tag = config.Tags.RepeatNOrMore,
                Visibility = visibility,
                WhiteSpaceRule = config.WhiteSpace,
                Subrules = new IRule[] { beginRepeat, intRule, ellipsisRule, closeRepeat }
            };

            var repeatNoMoreThanN = new SequenceRule()
            {
                Tag = config.Tags.RepeatNoMoreThanN,
                Visibility = visibility,
                WhiteSpaceRule = config.WhiteSpace,
                Subrules = new IRule[] { beginRepeat, ellipsisRule, intRule, closeRepeat }
            };

            var repeatExact = new SequenceRule()
            {
                Tag = config.Tags.RepeatExact,
                Visibility = visibility,
                WhiteSpaceRule = config.WhiteSpace,
                Subrules = new IRule[] { beginRepeat, intRule, closeRepeat }
            };

            var repeatZeroOrMore = new SequenceRule()
            {
                Tag = config.Tags.RepeatZeroOrMore,
                Visibility = visibility,
                WhiteSpaceRule = config.WhiteSpace,
                Subrules = new IRule[] { beginRepeat, closeRepeat }
            };

            return new OrRule()
            {
                Tag = tag,
                Visibility = RuleVisiblity.Visible,
                Subrules = new IRule[]
                {
                    repeatBetweenNAndM,
                    repeatNOrMore,
                    repeatNoMoreThanN,
                    repeatExact,
                    repeatZeroOrMore,
                }
            };
        }

        #region --- Private methods -----------------------------------------------------------------------------------

        private static IRule RuleGroupSeparatorRule(string separatorTag, string separatorCharacters, int min = 1, int max = 1)
        {
            return new CharRule()
            {
                Tag = separatorTag,
                Characters = separatorCharacters,
                MatchCharacters = CharRule.MatchType.InEnumeration,
                Visibility = RuleVisiblity.Hidden,
                Min = min,
                Max = max
            };
        }

        /// <summary>
        /// Parse rules which contain their own composition of rules such as the or or sequence rules. 
        /// These rules consist of "(" value, value (, value)* ")". The delimiters "(" and ")" and
        /// and separator "," are parameterized.
        /// </summary>
        /// <param name="compositeTag"></param>
        /// <param name="separatorCharacters"></param>
        /// <param name="separatorTag"></param>
        /// <param name="beginCompositeLiteral"></param>
        /// <param name="beginCompositeTag"></param>
        /// <param name="closeCompositeLiteral"></param>
        /// <param name="closeCompositeTag"></param>
        /// <param name="hidden"></param>
        /// <param name="valueRule"></param>
        /// <returns></returns>
        private static IRuleGroup CompositeRule(
            InterpreterConfig config,
            string compositeTag,
            IRule groupSeparatorRule,
            IRule whiteSpaceRule,
            RuleVisiblity visibility = RuleVisiblity.Visible,
            IRule valueRule = null)
        {
            var groupValueRule = valueRule ?? UnaryValueRule(config, RuleVisiblity.Visible);
            var criticalValueRule = new CriticalRule().Bind(groupValueRule);

            var groupChain = new SequenceRule()
            {
                Tag = $"{groupSeparatorRule} value!",
                Visibility = RuleVisiblity.Transitive,
                WhiteSpaceRule = whiteSpaceRule,
                Subrules = new IRule[]
                {
                    groupSeparatorRule,
                    criticalValueRule
                }
            };              

            return new SequenceRule()
            {
                Tag = compositeTag,
                Visibility = visibility,
                WhiteSpaceRule = whiteSpaceRule,
                Subrules = new IRule[]
                {
                    // composite is, at the minimum, defined as ( value separator value )
                    groupValueRule,
                    // should not be critical
                    groupSeparatorRule,
                    criticalValueRule,
                    // optional other arguments
                    ZeroOrMore(groupChain, whiteSpaceRule),
                }
            };
        }

        #endregion
    }
}
