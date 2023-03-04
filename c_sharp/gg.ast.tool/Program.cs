

using gg.ast.interpreter;
using gg.ast.tool;
using gg.ast.util;

var toolCommandLineArgs = CommandLineArgument.ReadCommandLine<ToolConfig>(args);
var mainRule            = new ParserFactory().ParseFile(toolCommandLineArgs.SpecFilename.Value);
var mermaid             = new MermaidOutput();

Console.WriteLine($"gg.ast.tool v0.2");
Console.WriteLine($"writing {toolCommandLineArgs.SpecFilename.Value} to {toolCommandLineArgs.MermaidOutputFilename.Value}...");

if (!string.IsNullOrEmpty(toolCommandLineArgs.Options.Value))
{
    var options = toolCommandLineArgs.Options.Value;
    mermaid.ShowReferenceRules = options.Contains('r');
    mermaid.AllowDuplicateBranches = options.Contains('d');
}

mermaid.ToChartFile(mainRule, toolCommandLineArgs.MermaidOutputFilename.Value);

