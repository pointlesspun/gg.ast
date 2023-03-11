/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using Microsoft.VisualStudio.TestTools.UnitTesting;

using gg.ast.core;
using gg.ast.core.rules;

using gg.ast.common;

namespace gg.ast.tests.core.rules
{
    [TestClass]
    public class RuleTests
    {
        private static readonly IRule DefaultWhitespace = ShortHandRules.CreateWhitespaceRule();

        // --- Any ----------------------------------------------------------------------------------------------------

        [TestMethod]
        public void AnyTryParse()
        {
            var text = "abz123";
            var matchCharRule = new CharRule()
            {
                Characters = "az",
                MatchCharacters = CharRule.MatchType.Any
            };

            Assert.IsFalse(matchCharRule.Parse(text, -1).IsSuccess);

            for (var i = 0; i < 6; i++)
            {
                Assert.IsTrue(matchCharRule.Parse(text, i).IsSuccess);
            }

            Assert.IsFalse(matchCharRule.Parse(text, 6).IsSuccess);

            matchCharRule.MatchCharacters = CharRule.MatchType.InRange;

            for (var i = 0; i < 3; i++)
            {
                Assert.IsTrue(matchCharRule.Parse(text, i).IsSuccess);
            }

            for (var i = 3; i < 6; i++)
            {
                Assert.IsFalse(matchCharRule.Parse(text, i).IsSuccess);
            }

            matchCharRule.MatchCharacters = CharRule.MatchType.NotInRange;

            for (var i = 0; i < 3; i++)
            {
                Assert.IsFalse(matchCharRule.Parse(text, i).IsSuccess);
            }

            for (var i = 3; i < 6; i++)
            {
                Assert.IsTrue(matchCharRule.Parse(text, i).IsSuccess);
            }


            matchCharRule.MatchCharacters = CharRule.MatchType.InEnumeration;

            Assert.IsTrue(matchCharRule.Parse(text, 0).IsSuccess);
            Assert.IsFalse(matchCharRule.Parse(text, 1).IsSuccess);
            Assert.IsTrue(matchCharRule.Parse(text, 2).IsSuccess);
            Assert.IsFalse(matchCharRule.Parse(text, 3).IsSuccess);
        }

        // --- Literal ------------------------------------------------------------------------------------------------

        [TestMethod]
        public void LiteralTryParseTest()
        {
            var text1 = "foo";
            var text2 = "   foo";
            var text2StartIndex = 3;

            var literal = new LiteralRule()
            {
                Tag = "tag",
                Characters = text1,
                Visibility = NodeVisiblity.Hidden
            };

            var literal2 = new LiteralRule()
            {
                Tag = "tag",
                Characters = text1,
                Visibility = NodeVisiblity.Visible
            };

            var resultNoMap = literal.Parse(text1);

            Assert.IsTrue(resultNoMap.IsSuccess);
            Assert.IsTrue(resultNoMap.Nodes == null);
            Assert.IsTrue(resultNoMap.CharactersRead == text1.Length);

            var resultWithMap = literal2.Parse(text1);

            Assert.IsTrue(resultWithMap.IsSuccess);
            Assert.IsTrue(resultWithMap.CharactersRead == text1.Length);

            Assert.IsTrue(resultWithMap.Nodes.Count == 1);

            Assert.IsTrue(resultWithMap.Nodes[0].Children == null);
            Assert.IsTrue(resultWithMap.Nodes[0].Rule == literal2);
            Assert.IsTrue(resultWithMap.Nodes[0].StartIndex == 0);
            Assert.IsTrue(resultWithMap.Nodes[0].Length == text1.Length);
            Assert.IsTrue(resultWithMap.Nodes[0].Parent == null);

            resultNoMap = literal.Parse(text2, text2StartIndex);

            Assert.IsTrue(resultNoMap.IsSuccess);
            Assert.IsTrue(resultNoMap.Nodes == null);
            Assert.IsTrue(resultNoMap.CharactersRead == text2.Length - text2StartIndex);

            resultNoMap = literal.Parse("bar");

            Assert.IsFalse(resultNoMap.IsSuccess);

            resultNoMap = literal.Parse("foo", 1);

            Assert.IsFalse(resultNoMap.IsSuccess);
        }

        // --- Or -----------------------------------------------------------------------------------------------------

        [TestMethod]
        public void OrParseTest()
        {
            var textA = "foo";
            var literalA = new LiteralRule()
            {
                Tag = "tag",
                Characters = textA,
                Visibility = NodeVisiblity.Visible
            };

            var textB = "barr";
            var literalB = new LiteralRule()
            {
                Tag = "tag",
                Characters = textB,
                Visibility = NodeVisiblity.Visible
            };

            var or = new OrRule()
            {
                Tag = "or",
                Subrules = new IRule[] { literalA, literalB }
            };

            var result = or.Parse(" " + textA, 1).Nodes[0];

            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Parent == null);
            Assert.IsTrue(result.Length == textA.Length);
            Assert.IsTrue(result.StartIndex == 1);
            Assert.IsTrue(result.Rule == or);
            Assert.IsTrue(result.Children[0].Parent == result);
            Assert.IsTrue(result.Children[0].Length == textA.Length);
            Assert.IsTrue(result.Children[0].StartIndex == 1);
            Assert.IsTrue(result.Children[0].Rule == literalA);

            result = or.Parse("  " + textB, 2).Nodes[0];

            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Parent == null);
            Assert.IsTrue(result.Length == textB.Length);
            Assert.IsTrue(result.StartIndex == 2);
            Assert.IsTrue(result.Rule == or);
            Assert.IsTrue(result.Children[0].Parent == result);
            Assert.IsTrue(result.Children[0].Length == textB.Length);
            Assert.IsTrue(result.Children[0].StartIndex == 2);
            Assert.IsTrue(result.Children[0].Rule == literalB);


            Assert.IsTrue(or.Parse("qaz", 2).Nodes == null);
        }

        // --- Repeat -------------------------------------------------------------------------------------------------


        [TestMethod]
        public void ZeroOrMoreTest()
        {
            var textA = "foo";
            var literalA = new LiteralRule()
            {
                Tag = "tag",
                Characters = textA,
                Visibility = NodeVisiblity.Visible
            };

            var repeat = new RepeatRule()
            {
                Subrule = literalA
            };

            var testText = textA + textA + "xxx";
            var result = repeat.Parse(testText, 0);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Nodes.Count == 1);
            Assert.IsTrue(result.CharactersRead == textA.Length * 2);
            Assert.IsTrue(result.Nodes[0].Children.Count == 2);

            result = repeat.Parse(testText, textA.Length);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Nodes.Count == 1);
            Assert.IsTrue(result.CharactersRead == textA.Length);
            Assert.IsTrue(result.Nodes[0].Children.Count == 1);

            result = repeat.Parse(testText, 1);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.CharactersRead == 0);
            Assert.IsTrue(result.Nodes == null);
        }

        [TestMethod]
        public void NTimesOrMoreTest()
        {
            var textA = "foo";
            var literalA = new LiteralRule()
            {
                Tag = "tag",
                Characters = textA,
                Visibility = NodeVisiblity.Visible
            };

            var repeat = new RepeatRule()
            {
                Subrule = literalA,
                Min = 1
            };

            var testText = textA + textA + "xxx";
            var result = repeat.Parse(testText);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Nodes.Count == 1);
            Assert.IsTrue(result.CharactersRead == textA.Length * 2);
            Assert.IsTrue(result.Nodes[0].Children.Count == 2);

            result = repeat.Parse(testText, textA.Length);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.CharactersRead == textA.Length);
            Assert.IsTrue(result.Nodes.Count == 1);
            Assert.IsTrue(result.Nodes[0].Children.Count == 1);

            result = repeat.Parse(testText, 1);

            Assert.IsFalse(result.IsSuccess);

            repeat.Min = 2;

            result = repeat.Parse(testText, 0);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Nodes.Count == 1);
            Assert.IsTrue(result.CharactersRead == textA.Length * 2);
            Assert.IsTrue(result.Nodes[0].Children.Count == 2);

            result = repeat.Parse(testText, textA.Length);

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void RepeatNTimesAndNoMoreThanYTest()
        {
            var textA = "foo";
            var literalA = new LiteralRule()
            {
                Tag = "tag",
                Characters = textA,
                Visibility = NodeVisiblity.Visible
            };

            var repeat = new RepeatRule()
            {
                Subrule = literalA,
                Min = 1,
                Max = 1
            };

            var testText = textA + textA + textA + "xxx";
            var result = repeat.Parse(testText);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.CharactersRead == textA.Length);
            Assert.IsTrue(result.Nodes.Count == 1);
            Assert.IsTrue(result.Nodes[0].Children.Count == 1);

            result = repeat.Parse(testText, textA.Length);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.CharactersRead == textA.Length);
            Assert.IsTrue(result.Nodes[0].Children.Count == 1);

            result = repeat.Parse(testText, 1);

            Assert.IsFalse(result.IsSuccess);

            repeat.Max = 2;

            result = repeat.Parse(testText);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.CharactersRead == textA.Length * 2);
            Assert.IsTrue(result.Nodes[0].Children.Count == 2);

            result = repeat.Parse(testText, textA.Length);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.CharactersRead == textA.Length * 2);
            Assert.IsTrue(result.Nodes[0].Children.Count == 2);

            result = repeat.Parse(testText, textA.Length * 2);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.CharactersRead == textA.Length);
            Assert.IsTrue(result.Nodes[0].Children.Count == 1);

            repeat.Min = 2;

            result = repeat.Parse(testText);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.CharactersRead == textA.Length * 2);
            Assert.IsTrue(result.Nodes[0].Children.Count == 2);

            result = repeat.Parse(testText, textA.Length);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.CharactersRead == textA.Length * 2);
            Assert.IsTrue(result.Nodes[0].Children.Count == 2);

            result = repeat.Parse(testText, textA.Length * 2);

            Assert.IsFalse(result.IsSuccess);
        }

        // --- Sequence -----------------------------------------------------------------------------------------------


        [TestMethod]
        public void SequenceTryParseTest()
        {
            var textA = "foo";
            var literalA = new LiteralRule()
            {
                Tag = "tag",
                Characters = textA,
                Visibility = NodeVisiblity.Visible
            };
            var comma = new LiteralRule()
            {
                Tag = "tag",
                Characters = ",",
                Visibility = NodeVisiblity.Hidden
            };
            var textB = "bar";
            var literalB = new LiteralRule()
            {
                Tag = "tag",
                Characters = textB,
                Visibility = NodeVisiblity.Visible
            };

            var sequence = new SequenceRule()
            {
                Tag = "test sequence",
                WhiteSpaceRule = DefaultWhitespace,
                Subrules = new IRule[] {
                    literalA,
                    comma,
                    literalB
                }
            };

            var testText = "  " + textA + ", " + textB;
            var token = sequence.Parse(testText).Nodes[0];

            Assert.IsNotNull(token);

            Assert.IsTrue(token.Parent == null);
            Assert.IsTrue(token.Length == testText.Length);
            Assert.IsTrue(token.StartIndex == 0);
            Assert.IsTrue(token.Rule == sequence);

            Assert.IsTrue(token.Children[0].Parent == token);
            Assert.IsTrue(token.Children[0].StartIndex == 2);
            Assert.IsTrue(token.Children[0].Length == textA.Length);
            Assert.IsTrue(token.Children[0].Rule == literalA);

            Assert.IsTrue(token.Children[1].Parent == token);
            Assert.IsTrue(token.Children[1].StartIndex == 2 + textA.Length + 2);
            Assert.IsTrue(token.Children[1].Length == textB.Length);
            Assert.IsTrue(token.Children[1].Rule == literalB);

            testText = "  " + textB + ", " + textA;

            Assert.IsNull(sequence.Parse(testText).Nodes);
        }

        // --- Clone --------------------------------------------------------------------------------------------------

        [TestMethod]
        public void CloneTest()
        {
            var literal = new LiteralRule()
            {
                Tag = "lit",
                Characters = "foo",
                IsCaseSensitive = false,
            };

            var repeat = new RepeatRule()
            {
                Tag = "rep",
                Min = 0,
                Max = 42,
                Subrule = literal
            };

            var sequence = new SequenceRule()
            {
                Tag = "seq",
                Subrules = new IRule[]
                {
                    literal,
                    repeat
                }
            };

            var clonedLiteral = (LiteralRule)literal.Clone();
            var clonedRepeat = (RepeatRule)repeat.Clone();
            var clonedSequence = (SequenceRule)sequence.Clone();

            Assert.IsTrue(clonedLiteral != literal);
            Assert.IsTrue(clonedLiteral.Tag == literal.Tag);
            Assert.IsTrue(clonedLiteral.Characters == literal.Characters);
            Assert.IsTrue(clonedLiteral.IsCaseSensitive == literal.IsCaseSensitive);

            Assert.IsTrue(clonedRepeat != repeat);
            Assert.IsTrue(clonedRepeat.Tag == repeat.Tag);
            Assert.IsTrue(clonedRepeat.Subrule != repeat.Subrule);
            Assert.IsTrue(clonedRepeat.Subrule != clonedLiteral);

            Assert.IsTrue(clonedSequence != sequence);
            Assert.IsTrue(clonedSequence.Tag == sequence.Tag);
            Assert.IsTrue(clonedSequence.Subrules.Length == sequence.Subrules.Length);

            Assert.IsTrue(clonedSequence.Subrules[0] != literal);
            Assert.IsTrue(clonedSequence.Subrules[0].Tag == literal.Tag);
            Assert.IsTrue(((LiteralRule)clonedSequence.Subrules[0]).Characters == literal.Characters);
            Assert.IsTrue(((LiteralRule)clonedSequence.Subrules[0]).IsCaseSensitive == literal.IsCaseSensitive);

            Assert.IsTrue(clonedSequence.Subrules[1] != repeat);
            Assert.IsTrue(clonedSequence.Subrules[1].Tag == repeat.Tag);
            Assert.IsTrue(((RepeatRule)clonedSequence.Subrules[1]).Subrule.Tag == literal.Tag);
        }
    }
}