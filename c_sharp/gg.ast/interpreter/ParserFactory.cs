/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System;
using System.Collections.Generic;
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
        /// Replace the references with their actual refered value. 
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="ruleSet"></param>
        private static void InlineReference(
            ReferenceRule rule, 
            Dictionary<string, IRule> ruleSet)
        {
            rule.Subrule = ruleSet[rule.Reference];
         
            if (rule.Parent == null)
            {
                // top level rule, replace this rule in the ruleset
                ruleSet[rule.Tag] = rule.Subrule;
            }
            
            else if (rule.Parent is IRuleGroup ruleGroup)
            {
                // replace the reference in the group with the actual rule
                var ruleIndex = Array.IndexOf(ruleGroup.Subrules, rule);
                ruleGroup.Subrules[ruleIndex] = rule.Subrule;
            }
            else if (rule.Parent is IMetaRule metaRule)
            {
                // if the meta rule is a repeat rule and the current reference
                // is to a character rule, copy the repeat parameters (min, max)
                // and replace the repeat with the char rule.
                if (metaRule is RepeatRule repeatRule && rule.Subrule is CharRule charRule)
                {
                    var inlineCharRule = (CharRule) charRule.Clone();

                    inlineCharRule.Min = repeatRule.Min;
                    inlineCharRule.Max = repeatRule.Max;
                    
                    if (repeatRule.Parent != null)
                    {
                        if (repeatRule.Parent is IRuleGroup groupParent)
                        {
                            var ruleIndex = Array.IndexOf(groupParent.Subrules, repeatRule);
                            groupParent.Subrules[ruleIndex] = inlineCharRule;
                        }
                        else if (repeatRule.Parent is IMetaRule metaParent)
                        {
                            metaParent.Subrule = inlineCharRule;
                        }
                    }
                    else
                    {
                        // repeat doesn't have a parent, so it's a top level rule.
                        // replace the entire repeat with the reference rule
                        ruleSet[repeatRule.Tag] = inlineCharRule;
                    }
                }
                else
                {
                    metaRule.Subrule = rule.Subrule;
                }
            }
        }
    }
}
