using System.Text;

namespace gg.ast.tool
{
    /// <summary>
    /// Utility to parse commandline arguments
    /// </summary>
    public class CommandLineArgument
    {
        /// <summary>
        /// Given command line arguments and a target class which contains CommandLineArgument properties, 
        /// constructs an object of the target class and initializes all its CommandLineArgument  properties
        /// with the command line arguments
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns>An object of type T</returns>
        public static T ReadCommandLine<T>(string[] args)
        {
            return ReadCommandLine(Activator.CreateInstance<T>(), args);
        }

        /// <summary>
        /// Given command line arguments and a target object which contains CommandLineArgument properties, 
        /// initializes all its CommandLineArgument  properties
        /// with the command line arguments
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns>The provided obj</returns>
        public static T ReadCommandLine<T>(T obj, string[] args)
        {
            var commandLineArgs = obj.GetType().GetProperties()
                            .Where(property => property.PropertyType == typeof(CommandLineArgument))
                            .Select(property => (CommandLineArgument)property.GetValue(obj));

            var minRequiredArgs = commandLineArgs.Count(arg => !arg.IsOptional);

            if (args == null || args.Length < minRequiredArgs)
            {
                throw new ArgumentException($"Expecting {minRequiredArgs} command line arguments, but found {args.Length}.");
            }

            foreach (var commandLineArg in commandLineArgs)
            {
                commandLineArg.Initialize(args);              
            }

            return obj;
        }

        /// <summary>
        /// Lists all commandline arguments, their name and description to a string
        /// </summary>
        /// <param name="argumentObject"></param>
        /// <returns></returns>
        public static string ListArguments(object argumentObject)
        {
            var builder = new StringBuilder();

            var argumentGroups = argumentObject.GetType().GetProperties()
                            .Where(property => property.PropertyType == typeof(CommandLineArgument))
                            .Select(property => (CommandLineArgument)property.GetValue(argumentObject))
                            .GroupBy(arg => arg.Index >= 0)
                            .OrderBy(group => !group.Key);

            foreach (var group in argumentGroups) 
            {
                var sorted = group.OrderBy(arg => arg.Name);

                foreach (var argument in sorted)
                {
                    var description = argument.Description ?? "";

                    if (group.Key)
                    {
                        builder.AppendLine($"   [{argument.Index}] {argument.Name}.{description}");
                    }
                    else
                    {
                        var isRequiredText = argument.IsOptional ? "" : ", required";
                        builder.AppendLine($"   {argument.Name} -{argument.Letter}, --{argument.Name}{isRequiredText}.{description}");
                    }
                }
            }
            
            return builder.ToString();
        }

        /// <summary>
        /// Letter used to denote an argument, next string in the command line argument(s)
        /// will hold the value. eg -f filename.text
        /// </summary>
        public char Letter { get; set; }

        /// <summary>
        /// Name of the argument which can be used identify this argument in an argument list. 
        /// The next string in the command line argument(s)
        /// will hold the value. eg --filename filename.text
        /// </summary>
        public string? Name{ get; set; }

        /// <summary>
        /// String value of this argument as found on the command line
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// If true the value of this argument is expected to be the next argument on the commandline.
        /// If false the value is what this command line value currently is matching with
        /// </summary>
        public bool HasSeparateValue { get; set; } = true;

        /// <summary>
        /// Index in the command line arguments where this value is expected to appear
        /// </summary>
        public int Index { get; set; } = -1;

        /// <summary>
        /// If true this argument will not cause an error if not found on the commandline
        /// </summary>
        public bool IsOptional { get; set; } = true;

        /// <summary>
        /// Human understandable description of the use and intention of this argument
        /// </summary>
        public string? Description { get; set; }


        public CommandLineArgument()
        {
        }

        public CommandLineArgument(char letter, string name, int index = -1, bool isOptional = true, string? description = null)
        {
            Letter = letter;
            Name = name;
            Index = index;
            IsOptional = isOptional;
            Description = description;
        }

        public CommandLineArgument(int index, string name, string? description = null)
        {
            Index = index;
            Name = name;
            IsOptional = false;
            Description = description;
        }
        
        /// <summary>
        /// Initializes this argument with a matching value in the given args
        /// </summary>
        /// <param name="args"></param>
        /// <exception cref="ArgumentException"></exception>
        public void Initialize(string[] args)
        {
            if (Index >= 0)
            {
                Value = args[Index]; 
            }
            else
            {
                for (var i = 0; i < args.Length; i++)
                {
                    if (args[i].IndexOf("--") == 0)
                    {
                        if (HasSeparateValue)
                        {
                            if (args[i].Length > 2 && args[i].Substring(2) == Name)
                            {
                                Value = TryToGetValue(args, i);
                            }
                        }
                        else
                        {
                            Value = args[i];
                        }
                    }
                    else if (args[i].IndexOf("-") == 0 && args[i].Length > 1 && args[i][1] == Letter)
                    {
                        if (HasSeparateValue)
                        {
                            Value = TryToGetValue(args, i);
                        }
                        else
                        {
                            Value = args[i];
                        }
                    }
                }
            }

            if (!IsOptional && string.IsNullOrWhiteSpace(Value))
            {
                throw new ArgumentException($"Argument '{Name}' is required but wasn't found on commandline.");
            }
        }

        private string TryToGetValue(string[] args, int index)
        {
            if (args.Length > index + 1)
            {
                return args[index + 1];
            }
            
            throw new ArgumentException($"Expected a value for '{Name}' to follow after '{args[index]}'");            
        }
    }
}


