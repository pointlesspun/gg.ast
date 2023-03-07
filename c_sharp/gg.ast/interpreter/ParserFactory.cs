/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

using gg.ast.core;
using gg.ast.core.rules;

using static gg.ast.interpreter.InterpreterRules;

namespace gg.ast.interpreter
{
    /// <summary>
    /// Interprets a spec file builds a RuleSet Lexer/Parser.
    /// </summary>
    public class ParserFactory
    {
        private readonly Dictionary<string, IRule> _ruleSet = new();
        private readonly HashSet<string> _loadedScripts = new();
        private readonly ValueMap _valueMap = null;
        private readonly List<ReferenceRule> _referenceList = new();

        private InterpreterConfig _config;

        public InterpreterConfig Config
        {
            get => _config;
            set => _config = value;
        }

        public ParserFactory(InterpreterConfig config = null, ValueMap valueMap = null)
        {
            _config = config ?? new InterpreterConfig();
            _valueMap = valueMap ?? InterpreterValueMap.CreateValueMap(_config, _referenceList);
        }

        public IRule ParseFile(string filename)
        {
            return Parse(File.ReadAllText(filename));
        }

        /// <summary>
        /// Parse a spec file and return a dictionary of all rules in this spec file. 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public Dictionary<string, IRule> ParseFileRules(string filename)
        {
            return ParseRules(File.ReadAllText(filename));
        }

        /// <summary>
        /// Parse all rules in the given script and return either the rule with name
        /// "main" or the first rule encountered.
        /// </summary>
        /// <param name="interpreterScript"></param>
        /// <param name="parseConfig"></param>
        /// <returns></returns>
        public IRule Parse(string interpreterScript)
        {
            return ParseRules(interpreterScript)?[_config.Tags.Main];
        }

        /// <summary>
        /// Parse all rules in the given script
        /// </summary>
        /// <param name="interpreterScript"></param>
        /// <param name="findMainRule">Optionally tries to assign a main rule if no explicit main
        /// rule has been defined.</param>
        /// <returns>A dictionary of the rules (tag/irule) </returns>
        public Dictionary<string, IRule> ParseRules(string interpreterScript, bool findMainRule = true)
        {
            return ParseRules(CreateInterpreterRule(_config), interpreterScript, findMainRule);
        }

        public Dictionary<string, IRule> ParseRules(IRule interpreterRule, string interpreterScript, bool findMainRule = true)
        {
            var result = interpreterRule.Parse(interpreterScript);

            if (result.IsSuccess
                && result.Nodes[0].Children != null
                && result.Nodes[0].Children.Count > 0)
            {
                return CreateInterpreterRules(result.Nodes, interpreterScript, findMainRule);
            }

            return null;
        }

        private Dictionary<string, IRule> CreateInterpreterRules(
            List<AstNode> nodes, 
            string interpreterScript, 
            bool findMainRule = true
        ) { 
            // register the whitespace rule
            _ruleSet[_config.Tags.Whitespace] = _config.WhiteSpace;
            _ruleSet[_config.Tags.WhitespaceShortHand] = _config.WhiteSpace;

            // read the using and rule parts
            var (useList, ruleList) = GetUseBlockAndRuleList(nodes[0]);

            if (useList != null)
            {
                // import all the spec files referred to in the using block
                ParseUseList(useList, interpreterScript);
            }

            if (ruleList != null)
            {
                // first pass, parse as many rules as we can and collect all references (to other rules) found
                var firstRuleTag = ParseRules(ruleList, interpreterScript);

                // fill in all references
                _referenceList.ForEach(referenceRule => InlineReference(referenceRule, _ruleSet));
                _referenceList.Clear();

                // substitude all subrules (that can be substitued
                Substitude(_ruleSet);

                // if there is no main rule create a main rule referencing the first rule encountered
                if (findMainRule && !_ruleSet.ContainsKey(_config.Tags.Main))
                {
                    _ruleSet[_config.Tags.Main] = _ruleSet[firstRuleTag];
                }
            }

            return _ruleSet;
        }

        /// <summary>
        /// Go through all use (using/import) statements and load the rules
        /// if they haven't already been loaded.
        /// </summary>
        /// <param name="useList"></param>
        /// <param name="interpreterScript"></param>
        private void ParseUseList(AstNode useList, string interpreterScript)
        {
            foreach (var useNode in useList.Children)
            {
                var filename = interpreterScript.AsSpan(useNode.StartIndex + 1, useNode.Length - 2).ToString();

                if (!_loadedScripts.Contains(filename))
                {
                    _loadedScripts.Add(filename);
                    ParseRules(File.ReadAllText(filename), false);
                }
            }
        }

        /// <summary>
        /// Go through all declared rules and generate rule objects for each.
        /// </summary>
        /// <param name="ruleList"></param>
        /// <param name="interpreterScript"></param>
        /// <returns></returns>
        private string ParseRules(AstNode ruleList, string interpreterScript)
        {
            var firstRuleTag = (String)null;

            // create the rules found
            foreach (var node in ruleList.Children)
            {
                var rule = _valueMap.Map<IRule>(interpreterScript, node);

                _ruleSet[rule.Tag] = rule;

                // replace the config whitespace
                if (rule.Tag == _config.Tags.Whitespace || rule.Tag == _config.Tags.WhitespaceShortHand)
                {
                    if (rule is LiteralRule whitespaceLiteral 
                        && string.IsNullOrEmpty(whitespaceLiteral.Characters))
                    {
                        // if whitespace is empty just set it to null
                        _config.ValueMapWhiteSpace = null;
                    }
                    else if (rule is CharRule whitespaceCharRule
                        && string.IsNullOrEmpty(whitespaceCharRule.Characters))
                    {
                        // if whitespace is empty just set it to null
                        _config.ValueMapWhiteSpace = null;
                    }
                    else
                    {
                        _config.ValueMapWhiteSpace = rule;
                    }
                }
                else
                {
                    // whitespace rule will not be returned as 'first tag'
                    firstRuleTag ??= rule.Tag;
                }
            }

            return firstRuleTag;
        }

        /// <summary>
        /// Find the useblock and rulelist in the document (if any)./
        /// </summary>
        /// <param name="interpreterDocument"></param>
        /// <returns></returns>
        private (AstNode useBlock, AstNode ruleList) GetUseBlockAndRuleList(AstNode interpreterDocument)
        {
            AstNode useBlock = interpreterDocument[0].Tag == _config.Tags.UseList ? interpreterDocument[0] : null;
            AstNode ruleList = null;

            if (useBlock == null && interpreterDocument[0].Tag == _config.Tags.RuleList)
            {
                ruleList = interpreterDocument[0];
            }
            else if (interpreterDocument.Children.Count > 1 && interpreterDocument[1].Tag == _config.Tags.RuleList)
            {
                ruleList = interpreterDocument[1];
            }

            return (useBlock, ruleList);
         }

        /// <summary>
        /// Replace the references with their actual referred value. 
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="ruleSet"></param>
        private static void InlineReference(ReferenceRule rule, Dictionary<string, IRule> ruleSet)
        {
            // is this a toplevel rule (eg a = b) ? 
            if (rule.Parent == null)
            {
                // has this reference rule already been resolved ?
                if (ruleSet.TryGetValue(rule.Tag, out var registeredRule) && registeredRule == rule)
                {
                    ruleSet[rule.Tag] = Dereference(ruleSet[rule.Reference], rule.Tag, ruleSet);
                }
            }
            else
            {
                // reference is part of a IRuleGroup or IMetaRule
                var alias = ruleSet[rule.Reference];

                // if alias is a ReferenceRule it implies the reference hasn't been resolved,
                // so dereference it first
                if (alias is ReferenceRule) 
                {
                    alias = Dereference(alias, rule.Reference, ruleSet);
                    ruleSet[rule.Reference] = alias;
                }

                // replace the reference in the parent with the alias
                if (rule.Parent is IRuleGroup ruleGroup)
                {
                    var ruleIndex = Array.IndexOf(ruleGroup.Subrules, rule);
                    ruleGroup.Subrules[ruleIndex] = alias;
                }
                else if (rule.Parent is IMetaRule metaRule)
                {
                    metaRule.Subrule = alias;
                }
            }
        }

        private static IRule Dereference(IRule alias, string tag, Dictionary<string, IRule> ruleSet)
        {
            while (alias is ReferenceRule deref)
            {
                alias = ruleSet[deref.Reference];
            }

            alias = (IRule)alias.Clone();
            alias.Tag = tag;            

            return alias;
        }

        /// <summary>
        /// Replace rules in the ruleset with optimized versions eg
        /// Repeat(Char, min, max) => Char(min, max)
        /// (Not, $) => Not(Skip = 1) // <- to do
        /// </summary>
        /// <param name="ruleSet"></param>
        private static void Substitude(Dictionary<string, IRule> ruleSet)
        {
            foreach (var rule in ruleSet)
            {
                if (rule.Value is IRuleGroup ruleGroup)
                {
                    TrySubstitudeSubrules(ruleGroup);
                }
                else if (rule.Value is IMetaRule metaRule)
                {
                    TrySubstitudeSubrule(metaRule);
                }
                else
                {
                    TrySubstitudeRule(rule.Key, rule.Value, ruleSet);
                }
            }
        }

        private static void TrySubstitudeSubrules(IRuleGroup group)
        {
            for (var i = 0; i < group.Subrules.Length; i++)
            {
                var subRule = group.Subrules[i];

                if (subRule is RepeatRule repeatRule && repeatRule.Subrule is CharRule charRule)
                {
                    group.Subrules[i] = SubstitudeRepeatRule(repeatRule, charRule);
                }
            }
        }

        private static void TrySubstitudeSubrule(IMetaRule metaRule)
        {
            if (metaRule.Subrule is RepeatRule repeatRule && repeatRule.Subrule is CharRule charRule)
            {
                metaRule.Subrule = SubstitudeRepeatRule(repeatRule, charRule);
            }
        }

        private static void TrySubstitudeRule(string key, IRule rule, Dictionary<string, IRule> ruleSet)
        {
            if (rule is RepeatRule repeatRule && repeatRule.Subrule is CharRule charRule)
            {
                ruleSet[key] = SubstitudeRepeatRule(repeatRule, charRule);
            }
        }

        private static CharRule SubstitudeRepeatRule(RepeatRule repeatRule, CharRule charRule)
        {
            var inlineCharRule = (CharRule)charRule.Clone();

            inlineCharRule.Min = repeatRule.Min;
            inlineCharRule.Max = repeatRule.Max;

            return inlineCharRule;
        }       
    }
}
