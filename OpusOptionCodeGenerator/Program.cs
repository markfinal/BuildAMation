// <copyright file="Program.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Option Code Generator</summary>
// <author>Mark Final</author>
namespace OpusOptionCodeGenerator
{
    class Program
    {
        static void Log(string format, params string[] args)
        {
            System.Console.WriteLine(format, args);
        }

        static void PrintHelp()
        {
            Log("OpusOptionCodeGenerator [options]");
            Log("Options:");
            Log("-i=<interface filenames, using path separators>");
            Log("-n=<namespace>");
            Log("-c=<generated class name>");
            Log("-pv=<delegate private data class name>");
            Log("-p [generate properties file]");
            Log("-dd=<delegate type definition files, using path separators>");
            Log("-d [generate delegates file]");
            Log("-s [only write to stdout]");
            Log("-f [force write to file]");
            Log("-e [delegates extend an existing implementation]");
            Log("-b [generated delegates class is a base class]");
            Log("-uh [updated header of generated files if needed]");
            Log("-ih [ignore any updates to updates of generated files]");
            Log("--- Obsolete options ---");
            Log("-o=<output properties filename> [use -p]");
        }

        static Parameters ProcessArgs(string[] args)
        {
            if (0 == args.Length)
            {
                PrintHelp();
                throw new Exception("No arguments were passed");
            }

            var parameters = new Parameters();
            parameters.args = args;
            parameters.mode = 0;

            foreach (var arg in args)
            {
                if (arg.StartsWith("-o="))
                {
                    parameters.mode |= Parameters.Mode.GenerateProperties;
                    // redundant, ignore
                }
                else if (arg.StartsWith("-i="))
                {
                    var split = arg.Split(new char[] { '=' });
                    var inputFilesString = split[1];
                    parameters.inputPathNames = inputFilesString.Split(new char[] { System.IO.Path.PathSeparator });
                }
                else if (arg.StartsWith("-n="))
                {
                    var split = arg.Split(new char[] { '=' });
                    parameters.outputNamespace = split[1];
                }
                else if (arg.StartsWith("-c="))
                {
                    var split = arg.Split(new char[] { '=' });
                    parameters.outputClassName = split[1];
                }
                else if (arg.StartsWith("-pv="))
                {
                    var split = arg.Split(new char[] { '=' });
                    parameters.privateDataClassName = split[1];
                }
                else if (arg.StartsWith("-p"))
                {
                    parameters.mode |= Parameters.Mode.GenerateProperties;
                }
                else if (arg.StartsWith("-dd="))
                {
                    var split = arg.Split(new char[] { '=' });
                    var inputFilesString = split[1];
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
                else if (arg.StartsWith("-uh"))
                {
                    parameters.updateHeader = true;
                }
                else if (arg.StartsWith("-ih"))
                {
                    parameters.ignoreHeaderUpdates = true;
                }
                else
                {
                    throw new Exception("Unrecognized argument '{0}'", arg);
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

        static string ReadLine(System.IO.TextReader reader, bool skipComments, out int prefixSpaces, out bool stateOnly)
        {
            int numPrefixSpaces = 0;
            bool stateOnlyProxy = false;
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

                if (skipComments)
                {
                    // doxygen style comment
                    if (line.StartsWith("///"))
                    {
                        line = ""; // just to get through the while test
                        continue;
                    }
                    else if (line.StartsWith("// "))
                    {
                        // search for specialize comments which add metadata to the properties
                        if (line.EndsWith("StateOnly"))
                        {
                            stateOnlyProxy = true;
                        }
                        line = "";
                        continue;
                    }
                }
            }
            while (0 == line.Length);
            prefixSpaces = numPrefixSpaces;
            stateOnly = stateOnlyProxy;
            return line;
        }

        static string ReadLine(System.IO.TextReader reader)
        {
            int prefixSpaces = 0;
            bool stateOnly = false;
            return ReadLine(reader, false, out prefixSpaces, out stateOnly);
        }

        static string ReadLine(System.IO.TextReader reader, bool skipComments)
        {
            int prefixSpaces = 0;
            bool stateOnly = false;
            return ReadLine(reader, skipComments, out prefixSpaces, out stateOnly);
        }

        static string ReadLine(System.IO.TextReader reader, out int prefixSpaces)
        {
            bool stateOnly = false;
            return ReadLine(reader, false, out prefixSpaces, out stateOnly);
        }

        static string ReadLine(System.IO.TextReader reader, bool skipComments, out bool stateOnly)
        {
            int prefixSpaces = 0;
            return ReadLine(reader, skipComments, out prefixSpaces, out stateOnly);
        }

        /* NOT USED
        static string ReadLine(System.IO.TextReader reader, out int prefixSpaces, out bool stateOnly)
        {
            return ReadLine(reader, false, out prefixSpaces, out stateOnly);
        }
        */

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
            builder.Append("\n");
        }

        static DelegateSignature ReadDelegate(string filename)
        {
            if (!System.IO.File.Exists(filename))
            {
                throw new Exception("File '{0}' does not exist", filename);
            }
            Log("\nDelegate to read: '{0}'", filename);

            var signature = new DelegateSignature();

            using (var reader = new System.IO.StreamReader(filename))
            {
                string line;

                line = ReadLine(reader, true);
                if (null == line)
                {
                    throw new Exception("Interface file is empty");
                }
                // namespace
                var namespaceStrings = line.Split(new char[] { ' ' });
                if (!namespaceStrings[0].Equals("namespace"))
                {
                    throw new Exception("Interface file does not start with namespace or comments; instead starts with '{0}'", namespaceStrings[0]);
                }
                var namespaceName = namespaceStrings[1];
                Log("Namespace found is '{0}'", namespaceName);

                // opening namespace scope
                line = ReadLine(reader);
                if (!line.StartsWith("{"))
                {
                    throw new Exception("No scope opened after namespace");
                }

                // delegate
                line = ReadLine(reader, true);
                while (!line.EndsWith(");"))
                {
                    var newLine = ReadLine(reader);
                    line += " "; // add a space as buffers around joining lines up
                    line += newLine;
                }
                Log("Delegate potential: '{0}'", line);
                var delegateStrings = line.Split(new char[] { ' ' });
                if ("public" != delegateStrings[0] || "delegate" != delegateStrings[1])
                {
                    throw new Exception("No public delegate found in '{0}'", filename);
                }
                Log("Delegate found is '{0}'", line);
                var returnType = delegateStrings[2];
                if ("void" != returnType)
                {
                    returnType = namespaceName + "." + returnType;
                }
                int firstParenthesis = line.IndexOf('(', 0);
                var argumentsWithoutParentheses = line.Substring(firstParenthesis + 1, line.Length - firstParenthesis - 3);
                var argumentList = argumentsWithoutParentheses.Split(',');
                for (int i = 0; i < argumentList.Length; ++i)
                {
                    argumentList[i] = argumentList[i].Trim();
                    var argumentSplit = argumentList[i].Split(' ');
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
                var argumentsWithParentheses = "(" + string.Join(", ", argumentList) + ")";

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
            var propertyMap = new PropertySignatureMap();

            // TODO:
            // * handle anything before an interface, e.g. enum
            foreach (var inputPath in parameters.inputPathNames)
            {
                if (!System.IO.File.Exists(inputPath))
                {
                    throw new Exception("Input file '{0}' does not exist", inputPath);
                }
                Log("\nInterface to read: '{0}'", inputPath);

                using (var reader = new System.IO.StreamReader(inputPath))
                {
                    string line;

                    line = ReadLine(reader, true);
                    if (null == line)
                    {
                        throw new Exception("Interface file is empty");
                    }

                    // namespace
                    var namespaceStrings = line.Split(new char[] { ' ' });
                    if (!namespaceStrings[0].Equals("namespace"))
                    {
                        throw new Exception("Interface file does not start with namespace or comments; instead starts with '{0}'", namespaceStrings[0]);
                    }
                    var namespaceName = namespaceStrings[1];
                    Log("Namespace found is '{0}'", namespaceName);

                    // opening namespace scope
                    line = ReadLine(reader);
                    if (!line.StartsWith("{"))
                    {
                        throw new Exception("No scope opened after namespace");
                    }

                    // interface
                    line = ReadLine(reader, true);
                    var interfaceStrings = line.Split(new char[] { ' ' });
                    if ("public" != interfaceStrings[0] || "interface" != interfaceStrings[1])
                    {
                        throw new Exception("No public interface found: '{0}'", line);
                    }
                    var interfaceName = interfaceStrings[2];
                    Log("Interface found is '{0}'", interfaceName);

                    // opening interface scope
                    line = ReadLine(reader);
                    if (!line.StartsWith("{"))
                    {
                        throw new Exception("No scope opened after interface");
                    }

                    var propertyList = propertyMap[inputPath] = new PropertySignatureList();
                    if (parameters.outputNamespace != namespaceName)
                    {
                        propertyList.InterfaceName = namespaceName + "." + interfaceName;
                    }
                    else
                    {
                        propertyList.InterfaceName = interfaceName;
                    }
                    do
                    {
                        bool stateOnly = false;
                        line = ReadLine(reader, true, out stateOnly);
                        if (line.StartsWith("}"))
                        {
                            break;
                        }

                        var propertyStrings = line.Split(new char[] { ' ' });
                        Log("PropertySignature found: type '{0}', name '{1}'", propertyStrings[0], propertyStrings[1]);

                        var property = new PropertySignature();
                        property.Name = propertyStrings[1];
                        property.Type = propertyStrings[0];

                        // determine if the type is value or reference
                        var typeSplit = property.Type.Split(new char[] { '.' });
                        var trueType = typeSplit[typeSplit.Length - 1];
                        if (property.Type == "bool" || property.Type == "int" || (trueType.StartsWith("E") && System.Char.IsUpper(trueType[1])))
                        {
                            property.IsValueType = true;
                            Log("\tIs ValueType");
                        }
                        else
                        {
                            property.IsValueType = false;
                            Log("\tIs ReferenceType");
                        }

                        if (stateOnly)
                        {
                            Log("\tState only");
                            property.StateOnly = true;
                        }
                        else
                        {
                            Log("\tHas delegate functions");
                            property.StateOnly = false;
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
                            Log("\twith get");
                            property.HasGet = true;
                        }
                        else if (line.Equals("set;"))
                        {
                            Log("\twith set");
                            property.HasSet = true;
                        }
                        else
                        {
                            throw new Exception("Unexpected string '{0}'", line);
                        }

                        line = ReadLine(reader);
                        if (line.Equals("get;"))
                        {
                            Log("\twith get");
                            property.HasGet = true;
                        }
                        else if (line.Equals("set;"))
                        {
                            Log("\twith set");
                            property.HasSet = true;
                        }
                        else
                        {
                            throw new Exception("Unexpected string '{0}'", line);
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
                }
            }

            var delegateSignatures = new System.Collections.Generic.List<DelegateSignature>();
            if (null != parameters.inputDelegates)
            {
                foreach (var path in parameters.inputDelegates)
                {
                    delegateSignatures.Add(ReadDelegate(path));
                }
            }

            if (Parameters.Mode.GenerateProperties == (parameters.mode & Parameters.Mode.GenerateProperties))
            {
                Log("Generating properties...");
                WritePropertiesFile(parameters, propertyMap);
            }
            if (Parameters.Mode.GenerateDelegates == (parameters.mode & Parameters.Mode.GenerateDelegates))
            {
                Log("Generating delegates...");
                WriteDelegatesFile(parameters, propertyMap, delegateSignatures);
            }
        }

        private static void WritePropertiesFile(Parameters parameters, PropertySignatureMap propertyMap)
        {
            // write out C# file containing the properties
            if (parameters.toStdOut)
            {
                Log("Would have written file '{0}'", parameters.outputPropertiesPathName);
            }
            using (var writer = !parameters.toStdOut ? new System.IO.StreamWriter(parameters.outputPropertiesPathName) : System.Console.Out)
            {
                writer.NewLine = "\n";

                WriteLine(writer, 0, "// Automatically generated file from OpusOptionCodeGenerator. DO NOT EDIT.");
                WriteLine(writer, 0, "// Command line:");
                Write(writer, 0, "//");
                foreach (var arg in parameters.args)
                {
                    if (Parameters.excludedFlagsFromHeaders.Contains(arg))
                    {
                        continue;
                    }
                    Write(writer, 0, " {0}", arg);
                }
                Write(writer, 0, writer.NewLine);
                WriteLine(writer, 0, "namespace {0}", parameters.outputNamespace);
                WriteLine(writer, 0, "{");
                WriteLine(writer, 1, "public partial class {0}", parameters.outputClassName);
                WriteLine(writer, 1, "{");

                foreach (var interfacePath in propertyMap.Keys)
                {
                    var propertyList = propertyMap[interfacePath];
                    WriteLine(writer, 2, "#region {0} Option properties", propertyList.InterfaceName);
                    foreach (var property in propertyList)
                    {
                        var typeString = property.IsValueType ? "ValueType" : "ReferenceType";
                        WriteLine(writer, 2, "{0} {1}.{2}", property.Type, propertyList.InterfaceName, property.Name);
                        WriteLine(writer, 2, "{");
                        if (property.HasGet)
                        {
                            WriteLine(writer, 3, "get");
                            WriteLine(writer, 3, "{");
                            WriteLine(writer, 4, "return this.Get{0}Option<{1}>(\"{2}\", this.SuperSetOptionCollection);", typeString, property.Type, property.Name);
                            WriteLine(writer, 3, "}");
                        }
                        if (property.HasSet)
                        {
                            WriteLine(writer, 3, "set");
                            WriteLine(writer, 3, "{");
                            WriteLine(writer, 4, "this.Set{0}Option<{1}>(\"{2}\", value);", typeString, property.Type, property.Name);
                            WriteLine(writer, 4, "this.ProcessNamedSetHandler(\"{0}SetHandler\", this[\"{0}\"]);", property.Name);
                            WriteLine(writer, 3, "}");
                        }
                        WriteLine(writer, 2, "}");
                    }
                    WriteLine(writer, 2, "#endregion");
                }

                WriteLine(writer, 1, "}");
                WriteLine(writer, 0, "}");
            }
            // flush to disk
            if (!parameters.toStdOut)
            {
                if (null != parameters.outputPropertiesPathName)
                {
                    Log("Wrote file '{0}'", parameters.outputPropertiesPathName);
                }
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

            var layout = new DelegateFileLayout();
            using (var reader = new System.IO.StreamReader(parameters.outputDelegatesPathName))
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
                        line = line + "\n";
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
                    Log("\nDelegate source line: '{0}'", line);
                    // done reading - it's the end of the class
                    if (line.StartsWith("}"))
                    {
                        break;
                    }

                    // look for function signatures
                    if (line.EndsWith(")"))
                    {
                        // use a key that will not change if the signature changes
                        var functionName = line.Substring(0, line.IndexOf('('));
                        functionName = functionName.Substring(functionName.LastIndexOf(' ') + 1);
                        Log("\tFound function '{0}'", functionName);
                        var body = layout.functions[functionName] = new System.Collections.Generic.List<IndentedString>();
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
                            //Log("Found {0} open braces and {1} close braces", openBraces, closeBraces);

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
                    else if (line.StartsWith("#region") || line.StartsWith("#endregion"))
                    {
                        Log("\tIgnored preprocessor line");
                        line = ReadLine(reader);
                    }
                }
            }

            return layout;
        }

        private static void WriteDelegatesFile(Parameters parameters, PropertySignatureMap propertyMap, System.Collections.Generic.List<DelegateSignature> delegateSignatures)
        {
            // write out C# file containing the delegates
            if (parameters.toStdOut)
            {
                Log("Would have written file '{0}'", parameters.outputDelegatesPathName);
            }

            var layout = ReadAndParseDelegatesFile(parameters);

            var memStream = new System.IO.MemoryStream();
            using (var writer = !parameters.toStdOut ? new System.IO.StreamWriter(memStream) : System.Console.Out)
            {
                writer.NewLine = "\n";

                var builder = new System.Text.StringBuilder();
                builder.Length = 0;
                bool writeToDisk = (null == layout);

                // write header
                WriteLine(builder, 0, "// Automatically generated file from OpusOptionCodeGenerator.");
                WriteLine(builder, 0, "// Command line:");
                Write(builder, 0, "//");
                foreach (var arg in parameters.args)
                {
                    if (Parameters.excludedFlagsFromHeaders.Contains(arg))
                    {
                        continue;
                    }
                    Write(builder, 0, " {0}", arg);
                }
                Write(builder, 0, "\n");
                if (null != layout && layout.header.Length != 0 && !builder.ToString().Equals(layout.header.ToString()))
                {
                    var message = new System.Text.StringBuilder();

                    var layoutHeaderBytes = System.Text.Encoding.ASCII.GetBytes(layout.header.ToString());
                    var builderHeaderBytes = System.Text.Encoding.ASCII.GetBytes(builder.ToString());
                    var differenceIndices = new System.Collections.Generic.List<int>();
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

                    if (parameters.ignoreHeaderUpdates)
                    {
                        builder = layout.header;
                    }
                    else
                    {
                        if (parameters.forceWrite || parameters.updateHeader)
                        {
                            Log(message.ToString());
                            Log("**** FORCE WRITE ALLOWING THIS TO CONTINUE\n");
                            writeToDisk = true;
                        }
                        else
                        {
                            throw new Exception(message.ToString());
                        }
                    }
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

                var delegatesToRegister = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>();

                // write delegates for each property of the interface
                foreach (var interfacePath in propertyMap.Keys)
                {
                    WriteLine(writer, 2, "#region {0} Option delegates", propertyMap[interfacePath].InterfaceName);
                    foreach (var property in propertyMap[interfacePath])
                    {
                        if (property.StateOnly)
                        {
                            delegatesToRegister[property.Name] = null;
                            continue;
                        }
                        else
                        {
                            delegatesToRegister[property.Name] = new System.Collections.Generic.List<string>();
                        }

                        foreach (var delegateSig in delegateSignatures)
                        {
                            var delegateName = property.Name + delegateSig.InNamespace;
                            delegatesToRegister[property.Name].Add(delegateName);

                            var propertyDelegate = new System.Text.StringBuilder();
                            propertyDelegate.AppendFormat("{0} static {1} {2}{3}", parameters.isBaseClass ? "public" : "private", delegateSig.ReturnType, delegateName, delegateSig.ArgumentString);
                            WriteLine(writer, 2, propertyDelegate.ToString());
                            if (null != layout && layout.functions.ContainsKey(delegateName))
                            {
                                Log("\tFunction '{0}' reusing from file", delegateName);
                                foreach (var line in layout.functions[delegateName])
                                {
                                    // TODO: magic number
                                    WriteLine(writer, 2 + line.NumSpaces / 4, line.Line);
                                }
                            }
                            else
                            {
                                Log("\tFunction '{0}' generating empty code for", propertyDelegate.ToString());
                                WriteLine(writer, 2, "{");
                                if (null != delegateSig.Body)
                                {
                                    foreach (var line in delegateSig.Body)
                                    {
                                        WriteLine(writer, 3, line);
                                    }
                                }
                                WriteLine(writer, 2, "}");
                                writeToDisk = true;
                            }
                        }
                    }
                    WriteLine(writer, 2, "#endregion");
                }

                if (null != layout && (propertyMap.Count * delegateSignatures.Count + 1) != layout.functions.Count)
                {
                    writeToDisk = true;
                }

                // write the SetDelegates function that assigns the accumulated delegates above to their named properties
                string setDelegatesFunctionSignature = "protected override void SetDelegates(Opus.Core.DependencyNode node)";
                WriteLine(writer, 2, setDelegatesFunctionSignature);
                if (!writeToDisk && null != layout && layout.functions.ContainsKey(setDelegatesFunctionSignature))
                {
                    Log("Reusing existing code for function: '{0}'", setDelegatesFunctionSignature);
                    foreach (var line in layout.functions[setDelegatesFunctionSignature])
                    {
                        // TOOD: magic number
                        WriteLine(writer, 2 + line.NumSpaces/4, line.Line);
                    }
                }
                else
                {
                    if (null == parameters.privateDataClassName)
                    {
                        throw new Exception("Private data class name was not provided");
                    }

                    Log("Generating new code for the function: '{0}'", setDelegatesFunctionSignature);
                    WriteLine(writer, 2, "{");
                    if (parameters.extendedDelegates)
                    {
                        WriteLine(writer, 3, "base.SetDelegates(node);");
                    }
                    foreach (var propertyName in delegatesToRegister.Keys)
                    {
                        var registration = new System.Text.StringBuilder();
                        if (null != delegatesToRegister[propertyName])
                        {
                            registration.AppendFormat("this[\"{0}\"].PrivateData = new {1}(", propertyName, parameters.privateDataClassName);
                            if (delegatesToRegister[propertyName].Count > 0)
                            {
                                foreach (var delegateToRegister in delegatesToRegister[propertyName])
                                {
                                    registration.AppendFormat("{0},", delegateToRegister);
                                }
                                registration.Remove(registration.Length - 1, 1); // last comma
                            }
                            registration.Append(");");
                        }
                        else
                        {
                            registration.AppendFormat("// Property '{0}' is state only", propertyName);
                        }
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
                        Log("File '{0}' already up-to-date", parameters.outputDelegatesPathName);
                    }
                    else
                    {
                        writer.Flush();

                        using (var finalWriter = new System.IO.StreamWriter(parameters.outputDelegatesPathName))
                        {
                            memStream.WriteTo(finalWriter.BaseStream);
                        }

                        Log("Wrote file '{0}'", parameters.outputDelegatesPathName);
                    }
                }
            }
        }

        static int Main(string[] args)
        {
            try
            {
                var parameters = ProcessArgs(args);
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
