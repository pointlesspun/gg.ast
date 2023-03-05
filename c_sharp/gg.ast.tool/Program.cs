/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

/// Tool to read spec files and turn them into mermaid diagrams

using gg.ast.interpreter;
using gg.ast.tool;
using gg.ast.util;

Console.WriteLine($"gg.ast.tool v0.2");

var toolCommandLineArgs = new ToolConfig();

try
{
    CommandLineArgument.ReadCommandLine(toolCommandLineArgs, args);
    var mainRule = new ParserFactory().ParseFile(toolCommandLineArgs.SpecFilename.Value);
    var mermaid = new MermaidOutput();

    Console.WriteLine($"writing {toolCommandLineArgs.SpecFilename.Value} to {toolCommandLineArgs.MermaidOutputFilename.Value}...");

    if (!string.IsNullOrEmpty(toolCommandLineArgs.Options.Value))
    {
        var options = toolCommandLineArgs.Options.Value;
        mermaid.ShowReferenceRules = options.Contains('r');
        mermaid.AllowDuplicateBranches = options.Contains('d');
    }
    mermaid.ToChartFile(mainRule, toolCommandLineArgs.MermaidOutputFilename.Value);
}
catch (ArgumentException ae)
{
    Console.WriteLine("Something went not right: " + ae.Message);
    Console.WriteLine("\nTool usage: gast.exe specfile -m filename (-o 'rd')*");
    Console.WriteLine(CommandLineArgument.ListArguments(toolCommandLineArgs));
}
catch (FileNotFoundException fnf)
{
    Console.WriteLine($"Cannot load file, {fnf.Message}");
}

