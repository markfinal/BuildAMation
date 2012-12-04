// <copyright file="Program.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Option Interface Property Generator</summary>
// <author>Mark Final</author>
namespace OpusOptionInterfacePropertyGenerator
{
    class Exception : System.Exception
    {
        public Exception(string message)
            : base(message)
        {
        }

        public Exception(string message, params object[] args)
            : base(System.String.Format (message, args))
        {
        }
    }

    class PropertySignature
    {
        public string Type
        {
            get;
            set;
        }

        public bool IsValueType
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string Interface
        {
            get;
            set;
        }

        public bool HasGet
        {
            get;
            set;
        }

        public bool HasSet
        {
            get;
            set;
        }
    }

    class DelegateSignature
    {
        public string InNamespace
        {
            get;
            set;
        }

        public string ReturnType
        {
            get;
            set;
        }

        public string ArgumentString
        {
            get;
            set;
        }

        public System.Collections.Specialized.StringCollection Body
        {
            get;
            set;
        }
    }

    class Parameters
    {
        [System.Flags]
        public enum Mode
        {
            GenerateProperties = (1<<0),
            GenerateDelegates  = (1<<1)
        }

        public string[] args;
        public string outputPropertiesPathName;
        public string outputDelegatesPathName;
        public string[] inputPathNames;
        public string[] inputDelegates;
        public string outputNamespace;
        public string outputClassName;
        public string privateDataClassName;
        public Mode mode;
        public bool toStdOut;
        public bool forceWrite;
        public bool extendedDelegates;
        public bool isBaseClass;
    }

    class Program
    {
        static Parameters ProcessArgs(string[] args)
        {
            if (0 == args.Length)
            {
                throw new Exception("Arguments required");
            }

            Parameters parameters = new Parameters();
            parameters.args = args;
            parameters.mode = 0;

            foreach (string arg in args)
            {
                if (arg.StartsWith("-o="))
                {
                    parameters.mode |= Parameters.Mode.GenerateProperties;
                    // redundant, ignore
                }
                else if (arg.StartsWith("-i="))
                {
                    string[] split = arg.Split(new char[] { '=' });
                    string inputFilesString = split[1];
                    parameters.inputPathNames = inputFilesString.Split(new char[] { System.IO.Path.PathSeparator });
                }
                else if (arg.StartsWith("-n="))
                {
                    string[] split = arg.Split(new char[] { '=' });
                    parameters.outputNamespace = split[1];
                }
                else if (arg.StartsWith("-c="))
                {
                    string[] split = arg.Split(new char[] { '=' });
                    parameters.outputClassName = split[1];
                }
                else if (arg.StartsWith("-pv="))
                {
                    string[] split = arg.Split(new char[] { '=' });
                    parameters.privateDataClassName = split[1];
                }
                else if (arg.StartsWith("-p"))
                {
                    parameters.mode |= Parameters.Mode.GenerateProperties;
                }
                else if (arg.StartsWith("-dd="))
                {
                    string[] split = arg.Split(new char[] { '=' });
                    string inputFilesString = split[1];
                    parameters.inputDelegates = inputFilesString.Split(new char[] { System.IO.Path.PathSeparator });
                }
                else if (arg.StartsWith("-d"))
                {
                    parameters.mode |= Parameters.Mode.GenerateDelegates;
                }
                else if (arg.StartsWith("-s"))
                {
                    parameters.toStdOut = true;
                }
                else if (arg.StartsWith("-f"))
                {
                    parameters.forceWrite = true;
                }
                else if (arg.StartsWith("-e"))
                {
                    parameters.extendedDelegates = true;
                }
                else if (arg.StartsWith("-b"))
                {
                    parameters.isBaseClass = true;
                }
                else
                {
                    throw new Exception(System.String.Format("Unrecognized argument '{0}'", arg));
                }
            }

            return parameters;
        }

        static void Validate(Parameters parameters)
        {
            if (null == parameters.inputPathNames)
            {
                throw new Exception("No input files provided");
            }

            if (null == parameters.outputNamespace)
            {
                throw new Exception("No output namespace provided");
            }

            if (null == parameters.outputClassName)
            {
                throw new Exception("No output class name provided");
            }

            parameters.outputPropertiesPathName = parameters.outputClassName + "Properties.cs";
            parameters.outputDelegatesPathName = parameters.outputClassName + "Delegates.cs";
        }

        static string ReadLine(System.IO.TextReader reader, bool skipComments, out int prefixSpaces)
        {
            int numPrefixSpaces = 0;
            string line = null;
            do
            {
                line = reader.ReadLine();
                if (null == line)
                {
                    numPrefixSpaces = 0;
                    break;
                }
                int originalLineLength = line.Length;
                line = line.TrimStart();
                numPrefixSpaces = originalLineLength - line.Length;
                line = line.TrimEnd();

                if (skipComments && line.StartsWith("//"))
                {
                    line = ""; // just to get through the while test
                    continue;
                }
            }
            while (0 == line.Length);
            prefixSpaces = numPrefixSpaces;
            return line;
        }

        static string ReadLine(System.IO.TextReader reader)
        {
            int prefixSpaces = 0;
            return ReadLine(reader, false, out prefixSpaces);
        }

        static string ReadLine(System.IO.TextReader reader, bool skipComments)
        {
            int prefixSpaces = 0;
            return ReadLine(reader, skipComments, out prefixSpaces);
        }

        static string ReadLine(System.IO.TextReader reader, out int prefixSpaces)
        {
            return ReadLine(reader, false, out prefixSpaces);
        }

        static void Write(System.IO.TextWriter writer, int tabCount, string format, params string[] args)
        {
            for (int i = 0; i < tabCount; ++i)
            {
                writer.Write("    ");
            }
            if (0 == args.Length)
            {
                writer.Write(format);
            }
            else
            {
                writer.Write(format, args);
            }
        }

        static void Write(System.Text.StringBuilder builder, int tabCount, string format, params string[] args)
        {
            builder.Append(new string('\t', tabCount));
            builder.AppendFormat(format, args);
        }

        static void WriteLine(System.IO.TextWriter writer, int tabCount, string format, params string[] args)
        {
            Write(writer, tabCount, format, args);
            writer.Write(writer.NewLine);
        }

        static void WriteLine(System.Text.StringBuilder builder, int tabCount, string format, params string[] args)
        {
            Write(builder, tabCount, format, args);
            builder.Append(System.Environment.NewLine);
        }

        static DelegateSignature ReadDelegate(string filename)
        {
            if (!System.IO.File.Exists(filename))
            {
                throw new Exception(System.String.Format("File '{0}' does not exist", filename));
            }
            System.Console.WriteLine("\nDelegate to read: '{0}'", filename);

            DelegateSignature signature = new DelegateSignature();

            using (System.IO.TextReader reader = new System.IO.StreamReader(filename))
            {
                string line;

                line = ReadLine(reader, true);
                if (null == line)
                {
                    throw new Exception("Interface file is empty");
                }
                // namespace
                string[] namespaceStrings = line.Split(new char[] { ' ' });
                if (!namespaceStrings[0].Equals("namespace"))
                {
                    throw new Exception(System.String.Format("Interface file does not start with namespace or comments; instead starts with '{0}'", namespaceStrings[0]));
                }
                string namespaceName = namespaceStrings[1];
                System.Console.WriteLine("Namespace found is '{0}'", namespaceName);

                // opening namespace scope
                line = ReadLine(reader);
                if (!line.StartsWith("{"))
                {
                    throw new Exception("No scope opened after namespace");
                }

                // delegate
                line = ReadLine(reader);
                string[] delegateStrings = line.Split(new char[] { ' ' });
                if ("public" != delegateStrings[0] || "delegate" != delegateStrings[1])
                {
                    throw new Exception("No public delegate found");
                }
                System.Console.WriteLine("Delegate found is '{0}'", line);
                string returnType = delegateStrings[2];
                if ("void" != returnType)
                {
                    returnType = namespaceName + "." + returnType;
                }
                int firstParenthesis = line.IndexOf('(', 0);
                string argumentsWithoutParentheses = line.Substring(firstParenthesis + 1, line.Length - firstParenthesis - 3);
                string[] argumentList = argumentsWithoutParentheses.Split(',');
                for (int i = 0; i < argumentList.Length; ++i)
                {
                    argumentList[i] = argumentList[i].Trim();
                    string[] argumentSplit = argumentList[i].Split(' ');
                    if (argumentSplit[0] == "object")
                    {
                        // nothing needed doing
                    }
                    else if (argumentSplit[0].Contains("."))
                    {
                        // fully specified namespace
                    }
                    else
                    {
                        argumentList[i] = namespaceName + "." + argumentSplit[0] + " " + argumentSplit[1];
                    }
                }
                string argumentsWithParentheses = "(" + string.Join(", ", argumentList) + ")";

                signature.InNamespace = namespaceName;
                signature.ReturnType = returnType;
                signature.ArgumentString = argumentsWithParentheses;
                if (signature.ReturnType == "void")
                {
                    signature.Body = null;
                }
                else
                {
                    signature.Body = new System.Collections.Specialized.StringCollection();
                    signature.Body.Add(System.String.Format("{0} returnVal = new {0}();", returnType));
                    signature.Body.Add("return returnVal;");
                }
            }

            return signature;
        }

        static void Execute(Parameters parameters)
        {
            System.Collections.Generic.List<PropertySignature> propertyList = new System.Collections.Generic.List<PropertySignature>();

            // TODO:
            // handle anything before an interface, e.g. enum
            foreach (string inputPath in parameters.inputPathNames)
            {
                if (!System.IO.File.Exists(inputPath))
                {
                    throw new Exception(System.String.Format("Input file '{0}' does not exist", inputPath));
                }
                System.Console.WriteLine("\nInterface to read: '{0}'", inputPath);

                using (System.IO.TextReader reader = new System.IO.StreamReader(inputPath))
                {
                    string line;

                    line = ReadLine(reader, true);
                    if (null == line)
                    {
                        throw new Exception("Interface file is empty");
                    }

                    // namespace
                    string[] namespaceStrings = line.Split(new char[] { ' ' });
                    if (!namespaceStrings[0].Equals("namespace"))
                    {
                        throw new Exception(System.String.Format("Interface file does not start with namespace or comments; instead starts with '{0}'", namespaceStrings[0]));
                    }
                    string namespaceName = namespaceStrings[1];
                    System.Console.WriteLine("Namespace found is '{0}'", namespaceName);

                    // opening namespace scope
                    line = ReadLine(reader);
                    if (!line.StartsWith("{"))
                    {
                        throw new Exception("No scope opened after namespace");
                    }

                    // interface
                    line = ReadLine(reader, true);
                    string[] interfaceStrings = line.Split(new char[] { ' ' });
                    if ("public" != interfaceStrings[0] || "interface" != interfaceStrings[1])
                    {
                        throw new Exception("No public interface found: '{0}'", line);
                    }
                    string interfaceName = interfaceStrings[2];
                    System.Console.WriteLine("Interface found is '{0}'", interfaceName);

                    // opening interface scope
                    line = ReadLine(reader);
                    if (!line.StartsWith("{"))
                    {
                        throw new Exception("No scope opened after interface");
                    }

                    do
                    {
                        line = ReadLine(reader, true);
                        if (line.StartsWith("}"))
                        {
                            break;
                        }

                        string[] propertyStrings = line.Split(new char[] { ' ' });
                        System.Console.WriteLine("PropertySignature found: type '{0}', name '{1}'", propertyStrings[0], propertyStrings[1]);

                        PropertySignature property = new PropertySignature();
                        property.Name = propertyStrings[1];
                        property.Type = propertyStrings[0];
                        if (parameters.outputNamespace != namespaceName)
                        {
                            property.Interface = namespaceName + "." + interfaceName;
                        }
                        else
                        {
                            property.Interface = interfaceName;
                        }

                        // determine if the type is value or reference
                        string[] typeSplit = property.Type.Split(new char[] { '.' });
                        string trueType = typeSplit[typeSplit.Length - 1];
                        if (property.Type == "bool" || property.Type == "int" || (trueType.StartsWith("E") && System.Char.IsUpper(trueType[1])))
                        {
                            property.IsValueType = true;
                            System.Console.WriteLine("\tIs ValueType");
                        }
                        else
                        {
                            property.IsValueType = false;
                            System.Console.WriteLine("\tIs ReferenceType");
                        }

                        // opening property scope
                        line = ReadLine(reader);
                        if (!line.StartsWith("{"))
                        {
                            throw new Exception("No scope opened after property");
                        }

                        line = ReadLine(reader);
                        if (line.Equals("get;"))
                        {
                            System.Console.WriteLine("\twith get");
                            property.HasGet = true;
                        }
                        else if (line.Equals("set;"))
                        {
                            System.Console.WriteLine("\twith set");
                            property.HasSet = true;
                        }
                        else
                        {
                            throw new Exception(System.String.Format("Unexpected string '{0}'", line));
                        }

                        line = ReadLine(reader);
                        if (line.Equals("get;"))
                        {
                            System.Console.WriteLine("\twith get");
                            property.HasGet = true;
                        }
                        else if (line.Equals("set;"))
                        {
                            System.Console.WriteLine("\twith set");
                            property.HasSet = true;
                        }
                        else
                        {
                            throw new Exception(System.String.Format("Unexpected string '{0}'", line));
                        }

                        // closing property scope
                        line = ReadLine(reader);
                        if (!line.StartsWith("}"))
                        {
                            throw new Exception("No scope closed after property");
                        }

                        propertyList.Add(property);
                    }
                    while (true);

                    if (0 == propertyList.Count)
                    {
                        throw new Exception("No properties were found in the interface");
                    }
                }
            }

            System.Collections.Generic.List<DelegateSignature> delegateSignatures = new System.Collections.Generic.List<DelegateSignature>();
            foreach (string path in parameters.inputDelegates)
            {
                delegateSignatures.Add(ReadDelegate(path));
            }

            if (Parameters.Mode.GenerateProperties == (parameters.mode & Parameters.Mode.GenerateProperties))
            {
                System.Console.WriteLine("Generating properties...");
                WritePropertiesFile(parameters, propertyList);
            }
            if (Parameters.Mode.GenerateDelegates == (parameters.mode & Parameters.Mode.GenerateDelegates))
            {
                System.Console.WriteLine ("Generating delegates...");
                WriteDelegatesFile(parameters, propertyList, delegateSignatures);
            }
        }

        private static void WritePropertiesFile(Parameters parameters, System.Collections.Generic.List<PropertySignature> propertyList)
        {
            // write out C# file containing the properties
            if (parameters.toStdOut)
            {
                System.Console.WriteLine ("Would have written file '{0}'", parameters.outputPropertiesPathName);
            }
            using (System.IO.TextWriter writer = !parameters.toStdOut ? new System.IO.StreamWriter(parameters.outputPropertiesPathName) : System.Console.Out)
            {
                WriteLine(writer, 0, "// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.");
                WriteLine(writer, 0, "// Command line:");
                Write(writer, 0, "//");
                foreach (string arg in parameters.args)
                {
                    Write(writer, 0, " {0}", arg);
                }
                Write(writer, 0, writer.NewLine);
                WriteLine(writer, 0, "namespace {0}", parameters.outputNamespace);
                WriteLine(writer, 0, "{");
                WriteLine(writer, 1, "public partial class {0}", parameters.outputClassName);
                WriteLine(writer, 1, "{");

                foreach (PropertySignature property in propertyList)
                {
                    WriteLine(writer, 2, "{0} {1}.{2}", property.Type, property.Interface, property.Name);
                    WriteLine(writer, 2, "{");
                    if (property.HasGet)
                    {
                        WriteLine(writer, 3, "get");
                        WriteLine(writer, 3, "{");
                        WriteLine(writer, 4, "return this.Get{0}Option<{1}>(\"{2}\");", property.IsValueType ? "ValueType" : "ReferenceType", property.Type, property.Name);
                        WriteLine(writer, 3, "}");
                    }
                    if (property.HasSet)
                    {
                        WriteLine(writer, 3, "set");
                        WriteLine(writer, 3, "{");
                        WriteLine(writer, 4, "this.Set{0}Option<{1}>(\"{2}\", value);", property.IsValueType ? "ValueType" : "ReferenceType", property.Type, property.Name);
                        WriteLine(writer, 4, "this.ProcessNamedSetHandler(\"{0}SetHandler\", this[\"{0}\"]);", property.Name);
                        WriteLine(writer, 3, "}");
                    }
                    WriteLine(writer, 2, "}");
                }

                WriteLine(writer, 1, "}");
                WriteLine(writer, 0, "}");
            }
            if (null != parameters.outputPropertiesPathName)
            {
                System.Console.WriteLine("Wrote file '{0}'", parameters.outputPropertiesPathName);
            }
        }

        class IndentedString
        {
            public IndentedString(int numSpaces, string line)
            {
                this.NumSpaces = numSpaces;
                this.Line = line;
            }

            public int NumSpaces
            {
                get;
                private set;
            }

            public string Line
            {
                get;
                private set;
            }
        }

        class DelegateFileLayout
        {
            public System.Text.StringBuilder header = new System.Text.StringBuilder();
            public string namespaceName = null;
            public string className = null;
            public System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<IndentedString>> functions = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<IndentedString>>();
        }

        private static DelegateFileLayout ReadAndParseDelegatesFile(Parameters parameters)
        {
            if (!System.IO.File.Exists(parameters.outputDelegatesPathName))
            {
                return null;
            }

            DelegateFileLayout layout = new DelegateFileLayout();
            using (System.IO.TextReader reader = new System.IO.StreamReader(parameters.outputDelegatesPathName))
            {
                string line = null;

                // read the header
                for (;;)
                {
                    line = ReadLine(reader);
                    if (null == line)
                    {
                        return layout;
                    }
                    if (line.StartsWith("//"))
                    {
                        line.Trim('\n', '\r');
                        line = line + System.Environment.NewLine;
                        layout.header.Append(line);
                    }
                    else
                    {
                        break;
                    }
                }
                while (line == string.Empty)
                {
                    line = ReadLine(reader);
                }

                // read the namespace
                if (!line.StartsWith("namespace"))
                {
                    throw new Exception("Expected 'namespace'; found '{0}'", line);
                }
                layout.namespaceName = line.Split(' ')[1];
                line = ReadLine(reader);
                if (!line.StartsWith("{"))
                {
                    throw new Exception("Expected namespace opening scope '{'; found '{0}'", line);
                }
                line = ReadLine(reader);
                while (line == string.Empty)
                {
                    line = ReadLine(reader);
                }

                // read the class
                if (!line.Contains("class"))
                {
                    throw new Exception("Expected a class; found '{0}'", line);
                }
                layout.className = line.Split(' ')[3];
                line = ReadLine(reader);
                if (!line.StartsWith("{"))
                {
                    throw new Exception("Expected namespace opening scope '{'; found '{0}'", line);
                }
                line = ReadLine(reader);
                while (line == string.Empty)
                {
                    line = ReadLine(reader);
                }

                // find functions
                for (;;)
                {
                    System.Console.WriteLine("C: '{0}'", line);
                    // done reading - it's the end of the class
                    if (line.StartsWith("}"))
                    {
                        break;
                    }

                    // look for function signatures
                    if (line.EndsWith(")"))
                    {
                        System.Console.WriteLine("Found function '{0}'", line);
                        layout.functions[line] = new System.Collections.Generic.List<IndentedString>();
                        System.Collections.Generic.List<IndentedString> body = layout.functions[line];
                        int braceCount = 0;
                        int baselineIndentation = -1;
                        for (;;)
                        {
                            int indentation;
                            line = ReadLine(reader, out indentation);
                            if (baselineIndentation == -1)
                            {
                                baselineIndentation = indentation;
                            }
                            int openBraces = line.Split('{').Length - 1;
                            int closeBraces = line.Split('}').Length - 1;
                            System.Console.WriteLine("Found {0} open braces and {1} close braces", openBraces, closeBraces);

                            braceCount += openBraces;
                            if (braceCount > 0)
                            {
                                body.Add(new IndentedString(indentation - baselineIndentation, line));
                            }

                            braceCount -= closeBraces;
                            if (0 == braceCount)
                            {
                                break;
                            }
                        }

                        line = ReadLine(reader);
                    }
                }
            }

            return layout;
        }

        private static void WriteDelegatesFile(Parameters parameters, System.Collections.Generic.List<PropertySignature> propertyList, System.Collections.Generic.List<DelegateSignature> delegateSignatures)
        {
            // write out C# file containing the delegates
            if (parameters.toStdOut)
            {
                System.Console.WriteLine ("Would have written file '{0}'", parameters.outputDelegatesPathName);
            }

            DelegateFileLayout layout = ReadAndParseDelegatesFile(parameters);

            System.IO.MemoryStream memStream = new System.IO.MemoryStream();
            using (System.IO.TextWriter writer = !parameters.toStdOut ? new System.IO.StreamWriter(memStream) : System.Console.Out)
            {
                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                builder.Length = 0;
                bool writeToDisk = (null == layout);

                // write header
                WriteLine(builder, 0, "// Automatically generated file from OpusOptionInterfacePropertyGenerator.");
                WriteLine(builder, 0, "// Command line:");
                Write(builder, 0, "//");
                foreach (string arg in parameters.args)
                {
                    Write(builder, 0, " {0}", arg);
                }
                Write(builder, 0, System.Environment.NewLine);
                if (null != layout && layout.header.Length != 0 && !builder.ToString().Equals(layout.header.ToString()))
                {
                    System.Text.StringBuilder message = new System.Text.StringBuilder();

                    byte[] layoutHeaderBytes = System.Text.Encoding.ASCII.GetBytes(layout.header.ToString());
                    byte[] builderHeaderBytes = System.Text.Encoding.ASCII.GetBytes(builder.ToString());
                    System.Collections.Generic.List<int> differenceIndices = new System.Collections.Generic.List<int>();
                    for (int i = 0; i < System.Math.Min(layoutHeaderBytes.Length, builderHeaderBytes.Length); ++i)
                    {
                        if (layoutHeaderBytes[i] != builderHeaderBytes[i])
                        {
                            differenceIndices.Add(i);
                        }
                    }

                    message.AppendFormat("Headers are different:\nFile:\n'{0}'\nNew:\n'{1}'\nDifferences at:\n", layout.header.ToString(), builder.ToString());
                    foreach (int diff in differenceIndices)
                    {
                        message.AppendFormat("\t{0}: {1} vs {2}\n", diff, layoutHeaderBytes[diff], builderHeaderBytes[diff]);
                    }

                    throw new Exception(message.ToString());
                }
                WriteLine(writer, 0, builder.ToString());

                // open namespace
                if (null != layout && layout.namespaceName != parameters.outputNamespace)
                {
                    throw new Exception("Namespaces are different:\nFile:\n'{0}'\nNew:\n'{1}'", layout.namespaceName, parameters.outputNamespace);
                }
                WriteLine(writer, 0, "namespace {0}", parameters.outputNamespace);
                WriteLine(writer, 0, "{");

                // open OptionCollection partial class
                if (null != layout && layout.className != parameters.outputClassName)
                {
                    throw new Exception("Classes are different:\nFile:\n'{0}'\nNew:\n'{1}'", layout.className, parameters.outputClassName);
                }
                WriteLine(writer, 1, "public partial class {0}", parameters.outputClassName);
                WriteLine(writer, 1, "{");

                System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>> delegatesToRegister = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>();

                // write delegates for each property of the interface
                foreach (PropertySignature property in propertyList)
                {
                    delegatesToRegister[property.Name] = new System.Collections.Generic.List<string>();

                    foreach (DelegateSignature delegateSig in delegateSignatures)
                    {
                        string delegateName = property.Name + delegateSig.InNamespace;
                        delegatesToRegister[property.Name].Add(delegateName);

                        System.Text.StringBuilder propertyDelegate = new System.Text.StringBuilder();
                        propertyDelegate.AppendFormat("{0} static {1} {2}{3}", parameters.isBaseClass ? "protected" : "private", delegateSig.ReturnType, delegateName, delegateSig.ArgumentString);
                        WriteLine(writer, 2, propertyDelegate.ToString());
                        if (null != layout && layout.functions.ContainsKey(propertyDelegate.ToString()))
                        {
                            System.Console.WriteLine("Function '{0}' reusing from file", propertyDelegate.ToString());
                            foreach (IndentedString line in layout.functions[propertyDelegate.ToString()])
                            {
                                // TODO: magic number
                                WriteLine(writer, 2 + line.NumSpaces / 4, line.Line);
                            }
                        }
                        else
                        {
                            System.Console.WriteLine("Function '{0}' generating code for", propertyDelegate.ToString());
                            WriteLine(writer, 2, "{");
                            if (null != delegateSig.Body)
                            {
                                foreach (string line in delegateSig.Body)
                                {
                                    WriteLine(writer, 3, line);
                                }
                            }
                            WriteLine(writer, 2, "}");
                            writeToDisk = true;
                        }
                    }
                }

                if (null != layout && (propertyList.Count * delegateSignatures.Count + 1) != layout.functions.Count)
                {
                    writeToDisk = true;
                }

                // write the SetDelegates function that assigns the accumulated delegates above to their named properties
                string setDelegatesFunctionSignature = "protected override void SetDelegates(Opus.Core.DependencyNode node)";
                WriteLine(writer, 2, setDelegatesFunctionSignature);
                if (!writeToDisk && null != layout && layout.functions.ContainsKey(setDelegatesFunctionSignature))
                {
                    System.Console.WriteLine("Function '{0}' reusing from file", setDelegatesFunctionSignature);
                    foreach (IndentedString line in layout.functions[setDelegatesFunctionSignature])
                    {
                        // TOOD: magic number
                        WriteLine(writer, 2 + line.NumSpaces/4, line.Line);
                    }
                }
                else
                {
                    System.Console.WriteLine("Function '{0}' generating code for", setDelegatesFunctionSignature);
                    WriteLine(writer, 2, "{");
                    if (parameters.extendedDelegates)
                    {
                        WriteLine(writer, 3, "base.SetDelegates(node);");
                    }
                    foreach (string propertyName in delegatesToRegister.Keys)
                    {
                        System.Text.StringBuilder registration = new System.Text.StringBuilder();
                        registration.AppendFormat("this[\"{0}\"].PrivateData = new {1}(", propertyName, parameters.privateDataClassName);
                        foreach (string delegateToRegister in delegatesToRegister[propertyName])
                        {
                            registration.AppendFormat("{0},", delegateToRegister);
                        }
                        registration.Remove(registration.Length - 1, 1); // last comma
                        registration.Append(");");
                        WriteLine(writer, 3, registration.ToString());
                    }
                    WriteLine(writer, 2, "}");
                    writeToDisk = true;
                }

                // close the class
                WriteLine(writer, 1, "}");

                // close the namespace
                WriteLine(writer, 0, "}");

                // flush to disk
                if (!parameters.toStdOut)
                {
                    if (!writeToDisk && !parameters.forceWrite)
                    {
                        System.Console.WriteLine("File '{0}' already up-to-date", parameters.outputDelegatesPathName);
                    }
                    else
                    {
                        writer.Flush();

                        using (System.IO.StreamWriter finalWriter = new System.IO.StreamWriter(parameters.outputDelegatesPathName))
                        {
                            memStream.WriteTo(finalWriter.BaseStream);
                        }

                        System.Console.WriteLine("Wrote file '{0}'", parameters.outputDelegatesPathName);
                    }
                }
            }
        }

        static int Main(string[] args)
        {
            try
            {
                Parameters parameters = ProcessArgs(args);
                Validate(parameters);
                Execute(parameters);
            }
            catch (Exception ex)
            {
                System.Console.Error.WriteLine("Local exception");
                System.Console.Error.WriteLine("{0}\n{1}", ex.Message, ex.StackTrace);
                return -1;
            }
            catch (System.Exception ex)
            {
                System.Console.Error.WriteLine("System exception");
                System.Console.Error.WriteLine("{0}\n{1}", ex.Message, ex.StackTrace);
                return -2;
            }

            return 0;
        }
    }
}
