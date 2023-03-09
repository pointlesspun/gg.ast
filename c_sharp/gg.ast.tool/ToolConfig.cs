/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

namespace gg.ast.tool
{
    /// <summary>
    /// Configuration filled with data from the command line args
    /// </summary>
    public class ToolConfig
    {
        public CommandLineArgument SpecFilename { get; private set; } = 
            new CommandLineArgument(0, "specFile", description: "\n\t\tName of the specfile to load.");

        public CommandLineArgument MermaidOutputFilename { get; private set; } 
            = new CommandLineArgument('m', "mermaid", isOptional: false, description: "\n\t\tName of the Mermaid chart output file.");

        public CommandLineArgument Options { get; private set; } = 
            new CommandLineArgument('o', "options", isOptional: true, 
                    description: "\n\t\tMermaid chart options eg -r, -rd, 'r': show reference files, 'd' allow for duplicates.");
    }
}


