namespace gg.ast.tool
{
    public class CommandLineArgument
    {
        public static T ReadCommandLine<T>(string[] args)
        {
            var obj = Activator.CreateInstance<T>();

            foreach (var property in obj.GetType().GetProperties())
            {
                if (property.PropertyType == typeof(CommandLineArgument))
                {
                    ((CommandLineArgument)property.GetValue(obj)).Initialize(args);
                }
            }

            return obj;
        }

        public char Letter { get; set; }

        public string? Name{ get; set; }

        public string? Value { get; set; }

        public int Index { get; set; } = -1;

        public bool IsOptional { get; set; } = true;

        public CommandLineArgument()
        {
        }

        public CommandLineArgument(char letter, string name, int index = -1, bool isOptional = true)
        {
            Letter = letter;
            Name = name;
            Index = index;
            IsOptional = isOptional;
        }

        public CommandLineArgument(int index, bool isOptional = true)
        {
            Index = index;
            IsOptional = isOptional;
        }

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
                        if (args[i].Length > 2 && args[i].Substring(2) == Name)
                        {
                            Value = TryToGetValue(args, i);
                        }
                    }
                    else if (args[i].IndexOf("-") == 0 && args[i].Length > 1 && args[i][1] == Letter)
                    {
                        Value = TryToGetValue(args, i);
                    }
                }
            }

            if (!IsOptional && string.IsNullOrWhiteSpace(Value))
            {
                throw new ArgumentException($"Argyment {Name} is required but wasn't found on commandline.");
            }
        }

        private string TryToGetValue(string[] args, int index)
        {
            if (args.Length > index)
            {
                return args[index + 1];
            }
            
            throw new ArgumentException($"Expected a value for {Name} to follow after ${args[index]}");
            
        }
    }
}


