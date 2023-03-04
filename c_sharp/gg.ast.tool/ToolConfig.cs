namespace gg.ast.tool
{

    public class ToolConfig
    {
        public CommandLineArgument SpecFilename { get; private set; } = new CommandLineArgument(0, isOptional: false);

        public CommandLineArgument MermaidOutputFilename { get; private set; } = new CommandLineArgument('m', "mermaid", isOptional: false);

        public CommandLineArgument Options { get; private set; } = new CommandLineArgument('o', "options", isOptional: true);
    }
}


